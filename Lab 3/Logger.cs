using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lab_3
{
    internal class Logger
    {
        public bool LoggingEnabled;
        public TextWriter Output = Console.Error;

        private void LogLine(string line)
        {
            if (!LoggingEnabled || Output == null) return;
            Output.WriteLine(line);
        }

        public void LogReference(uint pid, MemoryAccessType type, uint address, 
            uint page, uint offset)
        {
            LogLine($"Process[{pid}]: {type.GetFriendlyVerb()} 0x{address.ToString("X")} (page: {page}, offset: {offset})");
        }

        private void LogYesNo(string description, bool yesno, string prefix = "\t")  => LogLine(prefix + description + "? " + (yesno ? "yes" : "no"));

        public void LogTLBHit(bool hit) => LogYesNo("TLB hit", hit);

        public void LogPageFault(bool fault) => LogYesNo("Page fault", fault);

        public void LogTLBEviction(uint? evictedPage)
        {
            LogYesNo("TLB eviction", evictedPage != null);
            if (evictedPage != null) LogLine($"\tpage {evictedPage} evicted from TLB");
        }
        public void LogMemoryEviction(uint pid, uint page, bool dirty) => LogLine(
            $"\tProcess {pid} page {page} ({(dirty ? "dirty" : "clean")}) evicted from memory");

        public void LogPageFrame(uint page, uint frame) => LogLine($"\tpage {page} in frame {frame}");
    }
}
