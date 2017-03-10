using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Lab_3
{
    /// <summary>
    /// Loads a settings file based on the lab's specified format.
    /// </summary>
    internal static class SettingsFileLoader
    {
        private static readonly Regex SettingsValidationRegex = new Regex(@"(?<label>[a-z-]+): (?<value>[a-z0-9]+)");
        private static readonly Dictionary<string, PageReplacementPolicies> PageReplacementPoliciesMapping;

        static SettingsFileLoader()
        {
            PageReplacementPoliciesMapping =
                new List<PageReplacementPolicies>(
                    (PageReplacementPolicies[])Enum.GetValues(typeof(PageReplacementPolicies)))
                    .ToDictionary(p => p.GetConfigName());
        }

        /// <summary>
        /// Loads the settings from the file at the specified path.
        /// Throws FormatException for malformed settings files.
        /// </summary>
        /// <param name="path">The path of the file to load</param>
        /// <returns>The Settings object containing the loaded settings</returns>
        public static Settings LoadFromFile(string path)
        {
            var lines = File.ReadLines(path);
            var ret = new Settings();
            foreach (var line in lines)
            {
                var match = SettingsValidationRegex.Match(line);
                if (match.Groups.Count < 2) throw new FormatException(line);
                string group = match.Groups["label"].Value;
                string value = match.Groups["value"].Value;
                switch (group)
                {
                    case "physical-memory-size":
                        ret.PhysicalMemorySize = uint.Parse(value);
                        break;
                    case "frame-size":
                        ret.FrameSize = uint.Parse(value);
                        break;
                    case "memory-latency":
                        ret.MemoryLatency = uint.Parse(value);
                        break;
                    case "page-replacement":
                        ret.PageReplacementPolicy = PageReplacementPoliciesMapping[value];
                        break;
                    case "tlb-size":
                        ret.TLBSize = uint.Parse(value);
                        break;
                    case "tlb-latency":
                        ret.TLBLatency = uint.Parse(value);
                        break;
                    case "disk-latency":
                        ret.DiskLatency = uint.Parse(value);
                        break;
                    case "logging-output":
                        ret.LoggingOutput = 
                            value == "on" ? true :
                            value == "off" ? false :
                            throw new FormatException();
                        break;
                }
            }
            return new Settings();
        }
    }

    internal enum PageReplacementPolicies
    {
        Random,
        LRU,
        MRU,
        LFU,
        FIFO,
        MFU
    }

    internal static class PageReplacementPoliciesMethods
    {
        /// <summary>
        /// Gets the name of the policy as used in the settings file
        /// </summary>
        /// <param name="policy">this</param>
        /// <returns>The settings file name of the policy</returns>
        public static string GetConfigName(this PageReplacementPolicies policy)
        {
            return policy.ToString().ToUpper();
        }

        
    }

    /// <summary>
    /// Represents the configurable options specifiable in the settings files.
    /// </summary>
    internal struct Settings
    {
        public uint PhysicalMemorySize;
        public uint FrameSize;
        public uint MemoryLatency;
        public uint TLBSize;
        public uint TLBLatency;
        public uint DiskLatency;
        public bool LoggingOutput;
        public PageReplacementPolicies PageReplacementPolicy;
    }
}
