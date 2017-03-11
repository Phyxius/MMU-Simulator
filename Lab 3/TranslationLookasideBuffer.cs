using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab_3
{
    internal interface ITranslationLookasideBuffer
    {
        /// <summary>
        /// Looks up an entry in the TLB
        /// </summary>
        /// <param name="virtualAddress">the virtual address to lookup</param>
        /// <returns>the physical address corresponding to the given virtual address, or null if not present</returns>
        uint? LookupEntry(uint virtualAddress);
        /// <summary>
        /// Adds an entry to the TLB, possibly evicting and returning another entry
        /// </summary>
        /// <param name="virtualAddress">The virtual address to add</param>
        /// <param name="physicalAddress">The corresponding physical address</param>
        /// <returns>The evicted address, or null if no entry was evicted</returns>
        uint? AddEntry(uint virtualAddress, uint physicalAddress);
        void Flush();
    }

    /// <summary>
    /// Implements a translation lookaside buffer using a Least-Recently-Used
    /// page replacement policy
    /// </summary>
    internal class LRUTranslationLookasideBuffer : ITranslationLookasideBuffer
    {
        private class TLBEntry
        {
            public uint Timestep;
            public readonly uint PhysicalAddress;

            public TLBEntry(uint timestep, uint physicalAddress)
            {
                Timestep = timestep;
                PhysicalAddress = physicalAddress;
            }
        }

        private uint _timestep;
        public readonly uint Size;

        private readonly Dictionary<uint, TLBEntry> _tlb = new Dictionary<uint, TLBEntry>();

        public LRUTranslationLookasideBuffer(uint size)
        {
            Size = size;
        }

        public void Flush()
        {
            _tlb.Clear();
            _timestep = 0;
        }

        public uint? LookupEntry(uint virtualAddress)
        {
            if (Size == 0) return null;
            if (!_tlb.ContainsKey(virtualAddress))
            {
                return null;
            }
            _timestep++;
            _tlb[virtualAddress].Timestep = _timestep;
            return _tlb[virtualAddress].PhysicalAddress;
        }

        public uint? AddEntry(uint virtualAddress, uint physicalAddress)
        {
            if (Size == 0) return null;
            _tlb[virtualAddress] = new TLBEntry(physicalAddress, _timestep);
            _timestep++;

            if (_tlb.Count <= Size) return null;
            var evictedEntry = _tlb.Min(pair => pair.Value.Timestep);
            _tlb.Remove(evictedEntry);
            return evictedEntry;
        }
    }
}
