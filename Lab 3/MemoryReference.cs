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
        private readonly static Regex regex = new Regex(@"(?<pid>\d+) (?<type>[IWR]) (?<address>0x[0-9A-Fa-f]+)");
        private readonly static Dictionary<string, MemoryAccessType> AccessTypeLookup = new Dictionary<string, MemoryAccessType>()
        {
            {"I", MemoryAccessType.InstructionFetch},
            {"W", MemoryAccessType.Write},
            {"R", MemoryAccessType.Read}
        };
        public readonly uint PID;
        public readonly uint Address;
        public MemoryAccessType AccessType;

        public MemoryReference(uint pid, uint address, MemoryAccessType accessType)
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
                Convert.ToUInt32(match.Groups["address"].Value, 16), 
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

        /// <summary>
        /// Converts the memory access type to a friendly verb form for logging
        /// </summary>
        /// <param name="type">this</param>
        /// <returns>The friendly verb form</returns>
        internal static string GetFriendlyVerb(this MemoryAccessType type)
        {
            switch(type)
            {
                case MemoryAccessType.InstructionFetch:
                    return "Instruction fetch from";
                case MemoryAccessType.Read:
                    return "Read from";
                case MemoryAccessType.Write:
                    return "Store to";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
