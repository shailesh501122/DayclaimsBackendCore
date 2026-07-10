namespace DayClaim.AR.Domain.Common;

/// <summary>
/// PHI/PII field stored as authenticated ciphertext plus a keyed blind index,
/// per the deck's HIPAA design (slide 30): "Per-field design instead of
/// whole-JSON encryption... allows queryability by blind index... supports
/// field-level duplicate detection". <see cref="BlindIndex"/> is an HMAC of
/// the plaintext (never reversible) used for equality lookups/dedup; it is
/// NOT used for wildcard/range search, which is intentionally not supported
/// for encrypted PHI columns.
/// </summary>
public class EncryptedValue
{
    /// <summary>AES-256-GCM ciphertext, base64: nonce || ciphertext || tag.</summary>
    public string Ciphertext { get; set; } = string.Empty;

    /// <summary>HMAC-SHA256(plaintext) under a separate blind-index key, hex-encoded.</summary>
    public string BlindIndex { get; set; } = string.Empty;

    public static EncryptedValue Empty => new();
}
