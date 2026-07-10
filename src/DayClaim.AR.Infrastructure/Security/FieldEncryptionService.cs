using System.Security.Cryptography;
using System.Text;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using Microsoft.Extensions.Options;

namespace DayClaim.AR.Infrastructure.Security;

public class EncryptionSettings
{
    public const string SectionName = "Encryption";

    /// <summary>Base64-encoded 256-bit AES-GCM key. In production this is a KMS/Secrets-Manager-managed CMK (deck slide 30).</summary>
    public string DataKey { get; set; } = string.Empty;

    /// <summary>Separate key for the HMAC blind index — never the same key as DataKey, so a blind-index leak can't help decrypt ciphertext.</summary>
    public string BlindIndexKey { get; set; } = string.Empty;
}

/// <summary>
/// AES-256-GCM authenticated encryption with a keyed-HMAC blind index, per
/// deck slide 30: "Per-field design... allows queryability by blind index...
/// supports field-level duplicate detection, not just whole-claim". The
/// blind index is a one-way HMAC — it supports equality lookups only, never
/// decryption or partial/range search, so it doesn't reduce ciphertext
/// strength for pattern-analysis attacks.
/// </summary>
public class FieldEncryptionService : IFieldEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _dataKey;
    private readonly byte[] _blindIndexKey;

    public FieldEncryptionService(IOptions<EncryptionSettings> options)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.DataKey) || string.IsNullOrWhiteSpace(settings.BlindIndexKey))
        {
            throw new InvalidOperationException("Encryption:DataKey and Encryption:BlindIndexKey must be configured (32-byte base64 values).");
        }

        _dataKey = Convert.FromBase64String(settings.DataKey);
        _blindIndexKey = Convert.FromBase64String(settings.BlindIndexKey);

        if (_dataKey.Length != 32)
        {
            throw new InvalidOperationException("Encryption:DataKey must decode to exactly 32 bytes (AES-256).");
        }
    }

    public EncryptedValue Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            return EncryptedValue.Empty;
        }

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertextBytes = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aesGcm = new AesGcm(_dataKey, TagSize);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertextBytes, tag);

        var combined = new byte[NonceSize + ciphertextBytes.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
        Buffer.BlockCopy(ciphertextBytes, 0, combined, NonceSize, ciphertextBytes.Length);
        Buffer.BlockCopy(tag, 0, combined, NonceSize + ciphertextBytes.Length, TagSize);

        return new EncryptedValue
        {
            Ciphertext = Convert.ToBase64String(combined),
            BlindIndex = ComputeBlindIndex(plaintext),
        };
    }

    public string Decrypt(EncryptedValue value)
    {
        if (string.IsNullOrEmpty(value.Ciphertext))
        {
            return string.Empty;
        }

        var combined = Convert.FromBase64String(value.Ciphertext);
        var nonce = combined[..NonceSize];
        var tag = combined[^TagSize..];
        var ciphertextBytes = combined[NonceSize..^TagSize];
        var plaintextBytes = new byte[ciphertextBytes.Length];

        using var aesGcm = new AesGcm(_dataKey, TagSize);
        aesGcm.Decrypt(nonce, ciphertextBytes, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }

    public string ComputeBlindIndex(string plaintext)
    {
        var normalized = plaintext.Trim().ToUpperInvariant();
        var hash = HMACSHA256.HashData(_blindIndexKey, Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(hash);
    }
}
