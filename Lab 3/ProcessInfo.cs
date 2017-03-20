namespace Lab_3
{
    internal class ProcessInfo
    {
        public uint TotalMemoryReferences;
        public uint TLBMisses;
        public uint PageFaults;
        public uint CleanEvictions;
        public uint DirtyEvictions;
        public decimal TotalEvictions
        {
            get { return CleanEvictions + DirtyEvictions; }
        }
        public decimal PercentageDirtyEvictions
        {
            get
            {
                if (TotalEvictions == 0) return 0;
                return DirtyEvictions / TotalEvictions * 100;
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
