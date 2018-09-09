using System;
using System.IO;

namespace LECommonLibrary
{
    public enum PEType
    {
        X32,
        X64,
        Unknown
    }

    public static class PEFileReader
    {
        public static PEType GetPEType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return PEType.Unknown;
            var br = new BinaryReader(new FileStream(path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                ));

            try
            {


                //The following if clauses are meant to fix non-win32 executables with .exe extension
                //other possible fixes:
                //1. move BinaryReader br outside the try block and call br.close() in catch
                //   (proposed by original author, but I was afraid of creating br throwing execptions)
                //2. add a line for verifying the first two bytes of the .exe to be "MZ" or 0x4D5A
                //   (This may be a better solution, but I'm not sure if things will go fine if
                //    the file is actually a dll or so with the same bytes)
                //PS: I don't know why the file handle doen't close after both the stream and
                //    the binary reader moves out of scope...
                //PPS: For some reasons, the following code leaves the file handle open
                //     if i+4<0 when compiled here, but closes the handle when run seperatedly
                //     Code is changed to verifying the first two bytes...
                /*
                                //fix file handle doesn't close if file is empty
                                if (br.BaseStream.Length < 0x3c) {
                                    br.Close();
                                    return PEType.Unknown;
                                }
                                br.BaseStream.Seek(0x3C, SeekOrigin.Begin);
                                //fix file handle doen't close if file is not truly an WIN32 executable
                                //the "br.ReadInt32()" here may lead to "br.ReadUInt16()" reading after the whole file
                                //or even read before the file...
                                Int32 i = br.ReadInt32();
                                if (br.BaseStream.Length < i + 4 + sizeof(UInt16) || i + 4 < 0)
                                {
                                    br.Close();
                                    return PEType.Unknown;
                                }
                */
/*
                //for some reason, following code still does not work here
                //fix file handle doesn't close if file is empty
                if (br.BaseStream.Length < 0x3c)
                {
                    br.Close();
                    return PEType.Unknown;
                }
                //see if it is a WIN32 app
                if (br.ReadInt16() != 0x4D5A)
                {
                    br.Close();
                    return PEType.Unknown;
                }
*/
                br.BaseStream.Seek(0x3C, SeekOrigin.Begin);
                br.BaseStream.Seek(br.ReadInt32() + 4, SeekOrigin.Begin);
                var machine = br.ReadUInt16();

                br.Close();

                if (machine == 0x014C)
                    return PEType.X32;

                if (machine == 0x8664)
                    return PEType.X64;

                return PEType.Unknown;
            }
            catch
            {
                br.Close();
                return PEType.Unknown;
            }
        }
    }
}