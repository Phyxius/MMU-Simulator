using System;

namespace Lab_3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: mmu config_file trace_file");
                Environment.Exit(1);
            }

            var settings = SettingsFileLoader.LoadFromFile(args[0]);
            var tlb = new LRUTranslationLookasideBuffer(settings.TLBSize);
            var pageTable = createPageTable(settings);
            var debugLogging = settings.LoggingOutput;
            PrintPreSimulationOutput(settings, pageTable);
        }


        private static void PrintPreSimulationOutput(Settings s, IPageTable p)
        {
            Console.WriteLine($"Page bits: {p.GetPageBits()}");
            Console.WriteLine($"Offset bits: {p.GetOffsetBits()}");
            Console.WriteLine($"TLB size: {s.TLBSize}");
            Console.WriteLine("TLB latency (milliseconds): " + s.TLBLatencyMS.ToString("0.000000"));
            Console.WriteLine($"Physical memory (bytes): {s.PhysicalMemorySize}");
            Console.WriteLine($"Physical frame size (bytes) {s.FrameSize}");
            Console.WriteLine($"Number of physical frames: {p.GetMaxFrames()}");
            Console.WriteLine("Memory latency (milliseconds): " + s.MemoryLatencyMS.ToString("0.000000"));
            Console.WriteLine($"Number of page table entries: {p.GetMaxPages()}");
            Console.WriteLine($"Page replacement strategy: {s.PageReplacementPolicy.GetConfigName()}");
            Console.WriteLine("Disk latency (milliseconds): " + s.DiskLatency.ToString("0.00"));
            Console.WriteLine("Logging: " + (s.LoggingOutput ? "on" : "off"));
        }

        private static IPageTable createPageTable(Settings s)
        {
            switch(s.PageReplacementPolicy)
            {
                case Settings.PageReplacementPolicies.FIFO:
                    return new PageTable<uint>(new PageReplacementPolicies.FIFO(), s.PhysicalMemorySize, s.FrameSize);
                case Settings.PageReplacementPolicies.LRU:
                    return new PageTable<uint>(new PageReplacementPolicies.LeastRecentlyUsed(), s.PhysicalMemorySize, s.FrameSize);
                default:
                    throw new NotImplementedException(s.PageReplacementPolicy.ToString());
            }
        }
    }
}