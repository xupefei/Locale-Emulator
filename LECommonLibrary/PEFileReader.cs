using System.IO;

namespace LECommonLibrary
{
    public enum PEType
    {
        X32,
        X64,
        Unknown,
    }

    public static class PEFileReader
    {
        public static PEType GetPEType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return PEType.Unknown;

            try
            {
                var br =
                    new BinaryReader(new FileStream(path,
                                                    FileMode.Open,
                                                    FileAccess.Read,
                                                    FileShare.ReadWrite
                                         ));

                br.BaseStream.Seek(0x3C, SeekOrigin.Begin);
                br.BaseStream.Seek(br.ReadInt32() + 4, SeekOrigin.Begin);
                ushort machine = br.ReadUInt16();

                br.Close();

                if (machine == 0x014C)
                    return PEType.X32;

                if (machine == 0x8664)
                    return PEType.X64;

                return PEType.Unknown;
            }
            catch
            {
                return PEType.Unknown;
            }
        }
    }
}