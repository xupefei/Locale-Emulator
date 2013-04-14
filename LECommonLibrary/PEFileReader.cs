using System;
using System.IO;

namespace LECommonLibrary
{
    public class PEFileReader
    {
        public static PEType GetPEType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return PEType.Unknown;

            var br =
                new BinaryReader(new FileStream(path,
                                                FileMode.Open,
                                                FileAccess.Read,
                                                FileShare.ReadWrite
                                     ));

            byte[] buffer = br.BaseStream.Length > 512
                                ? br.ReadBytes(512)
                                : br.ReadBytes((int)br.BaseStream.Length);

            br.Close();

            // http://superuser.com/questions/103071/quick-way-to-tell-if-an-installed-application-is-64-bit-or-32-bit
            // "PE..L" (hex code: 50 45 00 00 4C) = 32 bit
            // "PE..d†" (hex code: 50 45 00 00 64 86) = 64 bit
            int loc = Array.IndexOf(buffer, (byte)0x50, 0);
            while (loc != -1 && loc <= buffer.Length - 6)
            {
                if (buffer[loc + 1] == 0x45 && buffer[loc + 2] == 0x00 && buffer[loc + 3] == 0x00)
                {
                    if (buffer[loc + 4] == 0x4C)
                    {
                        return PEType.X32;
                    }

                    if (buffer[loc + 4] == 0x64 && buffer[loc + 5] == 0x86)
                    {
                        return PEType.X64;
                    }
                }

                loc = Array.IndexOf(buffer, (byte)0x50, loc + 1);
            }

            return PEType.Unknown;
        }
    }
}