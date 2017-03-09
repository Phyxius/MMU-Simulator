using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Lab_3
{
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
        public static string GetConfigName(this PageReplacementPolicies policy)
        {
            return policy.ToString().ToUpper();
        }

        
    }


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
