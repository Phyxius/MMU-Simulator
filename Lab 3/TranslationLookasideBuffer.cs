using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab_3
{
    internal interface ITranslationLookasideBuffer
    {
        uint? LookupEntry(uint virtualAddress);
        void AddEntry(uint virtualAddress, uint physicalAddress);
        void Flush();
    }
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

        public void AddEntry(uint virtualAddress, uint physicalAddress)
        {
            if (Size == 0) return;
            _tlb[virtualAddress] = new TLBEntry(physicalAddress, _timestep);
            _timestep++;

            if (_tlb.Count > Size)
            {
                _tlb.Remove(_tlb.Min())
            }
        }
    }
}
