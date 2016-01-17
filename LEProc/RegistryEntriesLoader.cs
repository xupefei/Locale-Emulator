using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LECommonLibrary;

namespace LEProc
{
    public class RegistryEntriesLoader
    {
        private readonly List<IRegistryEntry> entries = new List<IRegistryEntry>();

        public IRegistryEntry[] GetRegistryEntries(bool isAdvanced)
        {
            try
            {
                Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .ToList()
                        .Where(t => t.Namespace.StartsWith("LEProc.RegistryEntries"))
                        .ToList()
                        .ForEach(i => entries.Add((IRegistryEntry)Activator.CreateInstance(i)));

                if (isAdvanced == false)
                    entries.RemoveAll(entry => entry.IsAdvanced);

                return entries.ToArray();
            }
            catch
            {
                return null;
            }
        }
    }
}