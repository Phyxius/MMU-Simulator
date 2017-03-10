using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Lab_3
{
    /// <summary>
    /// Reperesents a memory reference; usually corresponds to a single line a trace file.
    /// </summary>
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

    /// <summary>
    /// The types of memory accesses
    /// </summary>
    internal enum MemoryAccessType
    {
        Read, Write, InstructionFetch
    }

    /// <summary>
    /// The classes of memory accesses.
    /// Loads represent read operations, i.e. which do not set the 'dirty' flag
    /// on the corresponding page.
    /// Stores are write operations, i.e. which set the 'dirty' flag on the page.
    /// </summary>
    internal enum MemoryAccessClass
    {
        Load, Store
    }

    internal static class MemoryAccessTypeExtensions
    {
        /// <summary>
        /// Converts a specific memory access type to its corresponding class.
        /// </summary>
        /// <param name="type">This</param>
        /// <returns>The class of the operation</returns>
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
