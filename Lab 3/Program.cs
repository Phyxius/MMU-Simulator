using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            var pageTable = CreatePageTable(settings);
            var logger = new Logger { LoggingEnabled = settings.LoggingOutput };
            PrintPreSimulationOutput(settings, pageTable);

            decimal totalLatencyMS = 0;
            uint totalMemoryAccesses = 0;
            uint previousPID = 0;
            var processes = new Dictionary<uint, ProcessInfo>();
            var trace = File.ReadLines(args[1])
                .Select(MemoryReference.FromTraceLine);
            foreach (var m in trace)
            {
                totalMemoryAccesses++;
                if (!processes.ContainsKey(m.PID)) processes[m.PID] = new ProcessInfo();
                processes[m.PID].TotalMemoryReferences++;
                if (m.PID != previousPID) tlb.Flush();
                previousPID = m.PID;
                uint pageNumber = pageTable.GetPageNumber(m.Address);
                logger.LogReference(m.PID, m.AccessType, m.Address,
                    pageNumber, pageTable.GetOffset(m.Address));
                totalLatencyMS += settings.TLBLatencyMS;
                uint? tlbResult = tlb.LookupEntry(pageNumber);
                logger.LogTLBHit(tlbResult != null);
                if (tlbResult != null)
                {
                    logger.LogPageFrame(pageNumber, tlbResult.Value);
                    continue;
                }
                processes[m.PID].TLBMisses++;
                var lookupResult = pageTable.LookupAddress(m.Address, m.PID);
                totalLatencyMS += settings.MemoryLatencyMS;
                logger.LogPageFault(lookupResult != null);
                uint? tlbInsertionResult;
                if (lookupResult != null)
                {
                    if (m.AccessType.GetAccessClass() == MemoryAccessClass.Store) pageTable.SetPageDirty(m.Address, m.PID);
                    totalLatencyMS += settings.TLBLatencyMS;
                    tlbInsertionResult = tlb.AddEntry(pageNumber, lookupResult.Value.FrameNumber);
                    logger.LogTLBEviction(tlbInsertionResult);
                    logger.LogPageFrame(lookupResult.Value.PageNumber, lookupResult.Value.FrameNumber);
                    continue;
                }
                processes[m.PID].PageFaults++;
                totalLatencyMS += settings.MemoryLatency + settings.DiskLatency;
                var pageInsertionResult = pageTable.InsertPage(m.Address, m.PID);
                logger.LogMemoryEviction(pageInsertionResult);
                if (pageInsertionResult != null)
                {
                    if (pageInsertionResult.Value.EvictedPageDirty) totalLatencyMS += settings.DiskLatency;
                    if (pageInsertionResult.Value.EvictedPageDirty) processes[m.PID].DirtyEvictions++;
                    else processes[m.PID].CleanEvictions++;
                }
                lookupResult = pageTable.LookupAddress(m.Address, m.PID);
                if (m.AccessType.GetAccessClass() == MemoryAccessClass.Store) pageTable.SetPageDirty(m.Address, m.PID);
                totalLatencyMS += settings.TLBLatencyMS;
                tlbInsertionResult = tlb.AddEntry(pageNumber, lookupResult.Value.FrameNumber);
                logger.LogTLBEviction(tlbInsertionResult);
                logger.LogPageFrame(lookupResult.Value.PageNumber, lookupResult.Value.FrameNumber);
            }

            PrintPostSimulationOutput(totalLatencyMS, totalMemoryAccesses, settings.MemoryLatencyMS, processes);
        }


        private static void PrintPreSimulationOutput(Settings s, IPageTable p)
        {
            Console.WriteLine($"Page bits: {p.GetPageBits()}");
            Console.WriteLine($"Offset bits: {p.GetOffsetBits()}");
            Console.WriteLine($"TLB size: {s.TLBSize}");
            Console.WriteLine($"TLB latency (milliseconds): {s.TLBLatencyMS.ToString("0.000000")}");
            Console.WriteLine($"Physical memory (bytes): {s.PhysicalMemorySize}");
            Console.WriteLine($"Physical frame size (bytes) {s.FrameSize}");
            Console.WriteLine($"Number of physical frames: {p.GetMaxFrames()}");
            Console.WriteLine($"Memory latency (milliseconds): {s.MemoryLatencyMS.ToString("0.000000")}");
            Console.WriteLine($"Number of page table entries: {p.GetMaxPages()}");
            Console.WriteLine($"Page replacement strategy: {s.PageReplacementPolicy.GetConfigName()}");
            Console.WriteLine($"Disk latency (milliseconds): {s.DiskLatency.ToString("0.00")}");
            Console.WriteLine($"Logging: {(s.LoggingOutput ? "on" : "off")}");
        }

        private static void PrintPostSimulationOutput(decimal overallLatencyMS, uint totalAccesses, decimal memoryLatencyMS, Dictionary<uint, ProcessInfo> processes)
        {
            Console.WriteLine($"Overall latency (milliseconds): {overallLatencyMS.ToString("0.000000")}.");
            decimal averageLatency = overallLatencyMS / totalAccesses;
            Console.WriteLine($"Average memory access latency (milliseconds/reference): {averageLatency.ToString("0.000000")}.");
            Console.WriteLine($"Slowdown: {averageLatency / memoryLatencyMS}");
            PrintProcessInfo(null, processes.Values.Aggregate((l,r) => l + r));
            foreach (var pid in processes.Keys.OrderBy(pid => pid)) PrintProcessInfo(pid, processes[pid]);
        }

        private static void PrintProcessInfo(uint? pid, ProcessInfo info)
        {
            Console.WriteLine(pid == null ? "Overall" : $"Process {pid.Value}");
            Console.WriteLine($"\tMemory references: {info.TotalMemoryReferences}");
            Console.WriteLine($"\tTLB misses: {info.TLBMisses}");
            Console.WriteLine($"\tPage faults: {info.CleanEvictions}");
            Console.WriteLine($"\tClean evictions: {info.CleanEvictions}");
            Console.WriteLine($"\tDirty evictions: {info.DirtyEvictions}");
            Console.WriteLine($"Percentage dirty evictions: {info.PercentageDirtyEvictions.ToString("0.00")}%");
        }

        private static IPageTable CreatePageTable(Settings s)
        {
            uint memorySize = s.PhysicalMemorySize;
            uint frameSize = s.FrameSize;
            switch (s.PageReplacementPolicy)
            {
                case Settings.PageReplacementPolicies.FIFO:
                    return new PageTable<uint>(new PageReplacementPolicies.FIFO(), memorySize, frameSize);
                case Settings.PageReplacementPolicies.LRU:
                    return new PageTable<uint>(new PageReplacementPolicies.LeastRecentlyUsed(), memorySize, frameSize);
                case Settings.PageReplacementPolicies.LFU:
                    return new PageTable<uint>(new PageReplacementPolicies.LeastFrequentlyUsed(), memorySize, frameSize);
                case Settings.PageReplacementPolicies.MFU:
                    return new PageTable<uint>(new PageReplacementPolicies.MostFrequentlyUsed(), memorySize, frameSize);
                case Settings.PageReplacementPolicies.MRU:
                    return new PageTable<uint>(new PageReplacementPolicies.MostRecentlyUsed(), memorySize, frameSize);
                case Settings.PageReplacementPolicies.Random:
                    return new PageTable<object>(new PageReplacementPolicies.Random(), memorySize, frameSize);
                default:
                    throw new NotImplementedException(s.PageReplacementPolicy.ToString());
            }
        }
    }
}