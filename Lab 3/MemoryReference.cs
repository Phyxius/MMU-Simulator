using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Lab_3
{
    internal class MemoryReference
    {
        private readonly static Regex regex = new Regex(@"(<?pid>\d+) (<?type>[IWR]) (?<address>0x\d+)");
        private readonly static Dictionary<string, MemoryAccessType> AccessTypeLookup = new Dictionary<string, MemoryAccessType>()
        {
            {"I", MemoryAccessType.InstructionFetch},
            {"W", MemoryAccessType.Write},
            {"R", MemoryAccessType.Read}
        };
        public readonly uint PID;
        public readonly ulong Address;
        public MemoryAccessType AccessType;

        public MemoryReference(uint pid, ulong address, MemoryAccessType accessType)
        {
            PID = pid;
            Address = address;
            AccessType = accessType;
        }

        public static MemoryReference FromTraceLine(string traceLine)
        {
            var match = regex.Match(traceLine);
            if (!match.Success) throw new FormatException(traceLine);
            return new MemoryReference(
                uint.Parse(match.Groups["pid"].Value), 
                ulong.Parse(match.Groups["address"].Value), 
                AccessTypeLookup[match.Groups["type"].Value]);
        }
    }

    internal enum MemoryAccessType
    {
        Read, Write, InstructionFetch
    }

    internal enum MemoryAccessClass
    {
        Load, Store
    }

    internal static class MemoryAccessTypeExtensions
    {
        internal static MemoryAccessClass GetAccessClass(this MemoryAccessType type)
        {
            switch(type)
            {
                case MemoryAccessType.Write:
                    return MemoryAccessClass.Store;
                case MemoryAccessType.Read:
                case MemoryAccessType.InstructionFetch:
                    return MemoryAccessClass.Load;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
