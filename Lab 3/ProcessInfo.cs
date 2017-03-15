using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_3
{
    internal class ProcessInfo
    {
        public uint TotalMemoryReferences;
        public uint TLBMisses;
        public uint PageFaults;
        public uint CleanEvictions;
        public uint DirtyEvictions;
        public decimal PercentageDirtyEvictions
        {
            get
            {
                if (CleanEvictions + DirtyEvictions == 0) return 0;
                return DirtyEvictions / (DirtyEvictions + CleanEvictions);
            }
        }

        public static ProcessInfo operator +(ProcessInfo left, ProcessInfo right)
        {
            return new ProcessInfo
            {
                TotalMemoryReferences = left.TotalMemoryReferences + right.TotalMemoryReferences,
                TLBMisses = left.TLBMisses + right.TLBMisses,
                PageFaults = left.PageFaults + right.PageFaults,
                CleanEvictions = left.CleanEvictions + right.CleanEvictions,
                DirtyEvictions = left.DirtyEvictions + right.DirtyEvictions
            };
        }
    }
}
