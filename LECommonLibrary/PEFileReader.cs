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

            //The following if clauses are meant to fix non-win32 executables with .exe extension
            //Possible fixes:
            //1. move BinaryReader br outside the try block and call br.close() in catch
            //   (proposed by original author, but I was afraid of creating br throwing execptions)
            //2. Veryfy the range every time the binary reader reads anything
            //   (The most direct fix)
            //3. add a line for verifying the first two bytes of the .exe to be "MZ" or 0x4D5A
            //   (This may be a better solution, but I'm not sure if things will go fine if
            //    the file is actually a dll or so with the same bytes)
            //PS: I don't know why the file handle doen't close after both the stream and
            //    the binary reader moves out of scope...
            //PPS:Following code verifies the first two bytes, moved the reader to a outside try block
            //    and made sure that the range is correct (all three methods)
            try
            {

                var br = new BinaryReader(new FileStream(path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                ));

                try
                {
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

                    br.BaseStream.Seek(0x3C, SeekOrigin.Begin);
                    Int32 i = br.ReadInt32();

                    //make sure that the read range is not outside the stream
                    //useful if the file starts with 4D5A accidentally
                    //(e.g. ansi text files starting with "MZ" without BOM heading with ".exe" extension)
                    if (i + 4 + sizeof(UInt16) >= br.BaseStream.Length || i + 4 < 0)
                    {
                        br.Close();
                        return PEType.Unknown;
                    }

                    br.BaseStream.Seek(i + 4, SeekOrigin.Begin);
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
                finally
                {
                    br.Dispose();
                }
            }
            catch
            {
                //method to deal with error in starting to read
                return PEType.Unknown;
            }
        }
    }
}