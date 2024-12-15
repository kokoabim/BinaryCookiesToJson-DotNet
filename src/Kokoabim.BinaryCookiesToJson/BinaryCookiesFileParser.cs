namespace BinaryCookiesToJson;

internal static class BinaryCookiesFileParser
{
    // "cook"
    private const uint FileHeader = 1802465123;

    private const int LastEpochOf2000 = 978307200;

    #region methods

    public static IEnumerable<Cookie> ParseFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        var fileHeader = ReadInt32(reader);
        if (fileHeader != FileHeader) throw new InvalidDataException("Invalid binary cookies file");

        var pageCount = ReadInt32(reader, true);
        var pageSizes = Enumerable.Range(0, pageCount).Select(_ => ReadInt32(reader, true)).ToArray();
        var pages = pageSizes.Select(reader.ReadBytes).ToArray();

        List<Cookie> cookies = [];

        foreach (var page in pages)
        {
            using var pageReader = new BinaryReader(new MemoryStream(page));

            _ = ReadInt32(pageReader);
            var cookieCount = ReadInt32(pageReader);
            var cookieOffsets = Enumerable.Range(0, cookieCount).Select(_ => ReadInt32(pageReader)).ToArray();
            _ = ReadInt32(pageReader);

            foreach (var cookieOffset in cookieOffsets)
            {
                pageReader.BaseStream.Seek(cookieOffset, SeekOrigin.Begin);

                var cookieSize = ReadInt32(pageReader);
                var cookieReader = new BinaryReader(new MemoryStream(pageReader.ReadBytes(cookieSize)));

                _ = ReadInt32(cookieReader);

                var flags = ReadInt32(cookieReader);

                _ = ReadInt32(cookieReader);

                var domainOffset = ReadInt32(cookieReader);
                var nameOffset = ReadInt32(cookieReader);
                var pathOffset = ReadInt32(cookieReader);
                var valueOffset = ReadInt32(cookieReader);

                _ = ReadDouble(cookieReader);

                var expiresEpoch = ReadDouble(cookieReader) + LastEpochOf2000;
                var creationEpoch = ReadDouble(cookieReader) + LastEpochOf2000;

                var domain = ReadString(cookieReader, domainOffset - 4);
                var name = ReadString(cookieReader, nameOffset - 4);
                var path = ReadString(cookieReader, pathOffset - 4);
                var value = ReadString(cookieReader, valueOffset - 4);

                cookies.Add(new Cookie
                {
                    Domain = domain,
                    Expires = DateTimeOffset.FromUnixTimeSeconds((long)expiresEpoch).LocalDateTime,
                    Created = DateTimeOffset.FromUnixTimeSeconds((long)creationEpoch).LocalDateTime,
                    HttpOnly = (flags & 0x4) == 0x4,
                    Name = name,
                    Path = path,
                    Secure = (flags & 0x1) == 0x1,
                    Value = value,
                });
            }
        }

        return cookies;
    }

    private static double ReadDouble(BinaryReader reader, bool reverse = false)
    {
        var bytes = reader.ReadBytes(8);
        if (reverse) Array.Reverse(bytes);
        return BitConverter.ToDouble(bytes);
    }

    private static int ReadInt32(BinaryReader reader, bool reverse = false)
    {
        var bytes = reader.ReadBytes(4);
        if (reverse) Array.Reverse(bytes);
        return BitConverter.ToInt32(bytes);
    }

    private static string ReadString(BinaryReader reader, int offset)
    {
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);

        string s = "";
        while (true)
        {
            var c = reader.ReadChar();
            if (c == 0) break;
            s += c;
        }
        return s;
    }

    #endregion 
}