/*
 * This is a *dirty* implementation due to the lack of support of StructureToPtr(struct-has-non-fix-lengthed-array).
 * Maybe I will try another methods in the future.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Amemiya.Extensions;

namespace LEProc
{
    internal class LERegistryRedirector
    {
        /// <summary>
        ///     Registry Root flags
        /// </summary>
        private static readonly Dictionary<string, ulong> _regKeyFlags =
            new Dictionary<string, ulong>
            {
                {"HKEY_CLASSES_ROOT", 0x80000000},
                {"HKEY_CURRENT_USER", 0x80000001},
                {"HKEY_LOCAL_MACHINE", 0x80000002},
                {"HKEY_USERS", 0x80000003},
                {"HKEY_CURRENT_CONFIG", 0x80000005}
            };

        /// <summary>
        ///     Registry data types
        /// </summary>
        private static readonly Dictionary<string, uint> _regTypeFlags =
            new Dictionary<string, uint>
            {
                {"REG_SZ", 1},
                {"REG_EXPAND_SZ", 2},
                {"REG_BINARY", 3},
                {"REG_DWORD", 4},
                {"REG_MULTI_SZ", 7},
                {"REG_QWORD", 11}
            };

        private readonly List<byte> _objectData = new List<byte>();
        private readonly List<REGISTRY_REDIRECTION_ENTRY64> _registryReplacement;

        /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="count">Sum of registry entries</param>
        internal LERegistryRedirector(int count)
        {
            NumberOfRegistryRedirectionEntries = count;

            _registryReplacement = new List<REGISTRY_REDIRECTION_ENTRY64>(count);
        }

        /// <summary>
        ///     Number of registry redirection entries.
        /// </summary>
        internal int NumberOfRegistryRedirectionEntries { get; set; }

        /// <summary>
        ///     Get data in binary array format
        /// </summary>
        internal byte[] GetBinaryData()
        {
            // Write amount of REGISTRY_REDIRECTION_ENTRY64s
            var result = BitConverter.GetBytes((ulong)NumberOfRegistryRedirectionEntries);

            // Write REGISTRY_REDIRECTION_ENTRY64s
            var entrys =
                new byte[NumberOfRegistryRedirectionEntries * Marshal.SizeOf(new REGISTRY_REDIRECTION_ENTRY64())];
            entrys.FillWith((byte)0x00);

            for (var i = 0; i < _registryReplacement.Count; i++)
            {
                entrys.SetRange(ArrayExtensions.StructToBytes(_registryReplacement[i]),
                                i * Marshal.SizeOf(_registryReplacement[i]));
            }
            result = result.CombineWith(entrys);

            // Write data objects
            result = result.CombineWith(_objectData.ToArray());

            return result;
        }

        /// <summary>
        ///     Add a fake registry item to runtime.
        /// </summary>
        internal bool AddRegistryEntry(
            string root,
            string subkey,
            string valueName,
            string dataType,
            string data)
        {
            try
            {
                var original = new REGISTRY_ENTRY64
                               {
                                   Root = _regKeyFlags[root],
                                   SubKey = new UNICODE_STRING64(),
                                   ValueName = new UNICODE_STRING64(),
                                   DataType = _regTypeFlags[dataType],
                                   Data = 0,
                                   DataSize = 0
                               };

                AddStringData(subkey,
                              out original.SubKey.Buffer,
                              out original.SubKey.Length,
                              out original.SubKey.MaximumLength);

                AddStringData(valueName,
                              out original.ValueName.Buffer,
                              out original.ValueName.Length,
                              out original.ValueName.MaximumLength);

                AddObjectData(dataType, data, out original.Data, out original.DataSize);

                var entry = new REGISTRY_REDIRECTION_ENTRY64 {Original = original, Redirected = original};

                _registryReplacement.Add(entry);

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);

                return false;
            }
        }

        /// <summary>
        ///     Append a string to the end of leb block
        /// </summary>
        private void AddStringData(string value, out long offset, out ushort length, out ushort maxLength)
        {
            ulong temp;
            AddObjectData("REG_SZ", value, out offset, out temp);

            length = (ushort)temp;
            maxLength = (ushort)temp;
        }

        /// <summary>
        ///     Append a object to the end of leb block
        /// </summary>
        private void AddObjectData(string type, string value, out long offset, out ulong length)
        {
            offset = Marshal.SizeOf(new LoaderWrapper.LEB())
                     + NumberOfRegistryRedirectionEntries * Marshal.SizeOf(new REGISTRY_REDIRECTION_ENTRY64())
                     + _objectData.Count 
                     + sizeof (ulong) /*Size of NumberOfRegistryRedirectionEntries*/;

            switch (type)
            {
                case "REG_SZ":
                case "REG_EXPAND_SZ":
                case "REG_MULTI_SZ":
                    length = (ulong)Encoding.Unicode.GetBytes(value).Length;
                    _objectData.AddRange(Encoding.Unicode.GetBytes(value));
                    break;

                case "REG_DWORD":
                    length = sizeof (uint);
                    _objectData.AddRange(BitConverter.GetBytes(uint.Parse(value)));
                    break;

                case "REG_QWORD":
                    length = sizeof (ulong);
                    _objectData.AddRange(BitConverter.GetBytes(ulong.Parse(value)));
                    break;

                case "REG_BINARY":
                default:
                    throw new Exception("Data type " + type + " not supported yet.");
            }

            _objectData.AddRange(new byte[] {0x00, 0x00});
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct REGISTRY_ENTRY64
        {
            internal ulong Root;
            internal UNICODE_STRING64 SubKey;
            internal UNICODE_STRING64 ValueName;
            internal uint DataType;
            internal long Data;
            internal ulong DataSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct REGISTRY_REDIRECTION_ENTRY64
        {
            internal REGISTRY_ENTRY64 Original;
            internal REGISTRY_ENTRY64 Redirected;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct UNICODE_STRING64
        {
            internal ushort Length;
            internal ushort MaximumLength;
            internal long Buffer;
        }
    }
}