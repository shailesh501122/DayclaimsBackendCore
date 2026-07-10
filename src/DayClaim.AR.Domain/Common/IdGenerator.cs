using System.Buffers.Binary;
using System.Security.Cryptography;

namespace DayClaim.AR.Domain.Common;

/// <summary>
/// UUIDv7 (time-ordered GUID) generation, so primary keys sort roughly by
/// creation time without leaking a sequential integer, and insert locality on
/// the Postgres primary-key index stays good — matches the "UUIDv7 PKs,
/// microservice generated" decision in the modernization deck.
/// .NET 8 doesn't ship Guid.CreateVersion7 (added in .NET 9), so it's
/// implemented directly per RFC 9562 §5.7. The Guid(int,short,short,byte[8])
/// constructor is used (not the byte-span constructor) because its
/// big-endian overload only exists starting in .NET 9.
/// </summary>
public static class IdGenerator
{
    public static Guid NewId()
    {
        Span<byte> random = stackalloc byte[10];
        RandomNumberGenerator.Fill(random);

        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Field "a" (32-bit): top 32 bits of the 48-bit timestamp.
        uint timeHi = (uint)((unixMs >> 16) & 0xFFFFFFFF);

        // Field "b" (16-bit): low 16 bits of the timestamp.
        ushort timeLo = (ushort)(unixMs & 0xFFFF);

        // Field "c" (16-bit): version nibble (0111) + 12 random bits.
        ushort randA = BinaryPrimitives.ReadUInt16BigEndian(random[..2]);
        ushort verAndRand = (ushort)((0x7 << 12) | (randA & 0x0FFF));

        // Field "d..k" (8 bytes): variant bits (10) in the top 2 bits + 62 random bits.
        byte[] tail = random[2..10].ToArray();
        tail[0] = (byte)((tail[0] & 0x3F) | 0x80);

        return new Guid((int)timeHi, (short)timeLo, (short)verAndRand, tail);
    }
}
