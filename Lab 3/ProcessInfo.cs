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
    }
}
