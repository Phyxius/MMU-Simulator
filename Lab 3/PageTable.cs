using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab_3
{
    internal interface IPageTable
    {
        uint GetPageBits();
        uint GetOffsetBits();
        uint GetMaxFrames();
        uint GetMemorySize();
        uint GetMaxPages();
        uint GetPageNumber(uint address);
        uint GetOffset(uint address);
        /// <summary>
        /// Looks up the given address in the page table,
        /// and (if found) returns the result of that lookup via a 
        /// PageTableLookupResult.
        /// If not found (i.e. page not resident), returns null.
        /// </summary>
        /// <param name="address">The virtual address to look up</param>
        /// <param name="pid">The PID of the relevant process</param>
        /// <returns>The reseult of the lookup, or null if not found</returns>
        PageTableLookupResult? LookupAddress(uint address, uint pid);
        /// <summary>
        /// Sets the dirty flag on the page at the specified
        /// address/PID
        /// </summary>
        /// <param name="address">the address of the page</param>
        /// <param name="pid">the PID of the page</param>
        void SetPageDirty(uint address, uint pid);
        /// <summary>
        /// Inserts a page into the page table, including finding an open
        /// frame and evicting another page if necessary.
        /// </summary>
        /// <param name="address">The virtual address of the page to insert</param>
        /// <param name="pid">The PID of the owning process</param>
        /// <returns></returns>
        PageTableInsertionResult? InsertPage(uint address, uint pid);
    }

    internal class PageTable<T> : IPageTable
    {
        private readonly IPageReplacementPolicy<T> _replacementPolicy;
        private readonly Dictionary<PageTableKey, PageTableEntry<T>> _pageTable;
        private readonly bool[] _usedFrames;
        public readonly uint PageBits;
        public readonly uint OffsetBits;
        public readonly uint MaxFrames;
        public readonly uint MemorySize;
        public readonly uint MaxPages;

        public PageTable(IPageReplacementPolicy<T> replacementPolicy, uint memorySize, uint frameSize)
        {
            _pageTable = new Dictionary<PageTableKey, PageTableEntry<T>>();
            _replacementPolicy = replacementPolicy;
            MaxFrames = memorySize / frameSize;
            OffsetBits = (uint)Math.Log(frameSize, 2);
            PageBits = (8 * sizeof(uint)) - OffsetBits;
            MemorySize = memorySize;
            _usedFrames = new bool[MaxFrames];
            MaxPages = (uint)1 << (int)PageBits;
        }

        public uint GetPageNumber(uint address)
        {
            return address >> (int)OffsetBits;
        }

        public uint GetOffset(uint address)
        {
            //(~0u) is a string of all 1's the size of a uint
            return ((~0u) >> (int)PageBits) & address;
        }

        public PageTableLookupResult? LookupAddress(uint address, uint pid)
        {
            var key = new PageTableKey
            {
                PageNumber = GetPageNumber(address),
                PID = pid
            };
            if (!_pageTable.ContainsKey(key)) return null;
            TouchPage(key);
            return new PageTableLookupResult(
                GetPageNumber(address),
                _pageTable[key].FrameNumber,
                GetOffset(address));
        }

        public void SetPageDirty(uint address, uint pid)
        {
            var key = new PageTableKey
            {
                PageNumber = GetPageNumber(address),
                PID = pid
            };
            var page = _pageTable[key];
            page.Dirty = true;
            _pageTable[key] = page;
        }

        public PageTableInsertionResult? InsertPage(uint address, uint pid)
        {
            var key = new PageTableKey
            {
                PageNumber = GetPageNumber(address),
                PID = pid
            };
            var freeFrame = _usedFrames.Select((val, i) => Tuple.Create(val, i))
                .Where(t => !t.Item1)
                .FirstOrDefault()?.Item2;
            var entry = new PageTableEntry<T>();
            if (freeFrame != null)
            {
                entry.FrameNumber = (uint)freeFrame.Value;
                entry.ComparisonKey = _replacementPolicy.GetInitialKeyValue();
                _pageTable[key] = entry;
                _usedFrames[freeFrame.Value] = true;
                return null;
            }

            var evictedPage  = _replacementPolicy.EvictPage(
                _pageTable.ToDictionary(
                    pair => pair.Key, 
                    pair => pair.Value.ComparisonKey));
            PageTableEntry<T> evictedEntry = _pageTable[evictedPage];
            freeFrame = (int)evictedEntry.FrameNumber;
            entry.FrameNumber = (uint)freeFrame.Value;
            entry.ComparisonKey = _replacementPolicy.GetInitialKeyValue();
            _pageTable.Remove(evictedPage);
            _pageTable[key] = entry;
            return new PageTableInsertionResult(evictedPage.PageNumber, evictedPage.PID, evictedEntry.Dirty);
        }

        private void TouchPage(PageTableKey key)
        {
            var oldEntry = _pageTable[key];
            oldEntry.ComparisonKey = _replacementPolicy.TouchPage(oldEntry.ComparisonKey);
            _pageTable[key] = oldEntry;
        }

        public uint GetPageBits()
        {
            return PageBits;
        }

        public uint GetOffsetBits()
        {
            return OffsetBits;
        }

        public uint GetMaxFrames()
        {
            return MaxFrames;
        }

        public uint GetMemorySize()
        {
            return MemorySize;
        }

        public uint GetMaxPages()
        {
            return MaxPages;
        }
    }

    /// <summary>
    /// The result of a page table lookup.
    /// Includes the page number looked up, the offset of the page,
    /// and the corresponding frame number
    /// </summary>
    internal struct PageTableLookupResult
    {
        public PageTableLookupResult(uint pageNumber, uint frameNumber, uint offset)
        {
            PageNumber = pageNumber;
            FrameNumber = frameNumber;
            Offset = offset;
        }
        public readonly uint PageNumber;
        public readonly uint FrameNumber;
        public readonly uint Offset;
    }

    /// <summary>
    /// The result of a page table insertion. 
    /// Contains the evicted page's number, PID, and whether or not it was dirty.
    /// </summary>
    internal struct PageTableInsertionResult
    {
        public PageTableInsertionResult(uint evictedPageNumber, uint evictedPagePID, bool evictedPageDirty)
        {
            EvictedPageNumber = evictedPageNumber;
            EvictedPagePID = evictedPagePID;
            EvictedPageDirty = evictedPageDirty;
        }
        public readonly uint EvictedPageNumber;
        public readonly uint EvictedPagePID;
        public readonly bool EvictedPageDirty;
    }

    /// <summary>
    /// An entry in the page table. Contains both the PID of the process,
    /// and the address of the page.
    /// </summary>
    internal struct PageTableKey
    {
        /// <summary>
        /// The PID of the owning process
        /// </summary>
        public uint PID;
        /// <summary>
        /// The number of the page
        /// </summary>
        public uint PageNumber;
    }

    /// <summary>
    /// An entry in the page table. Contains a member indicating if the page is
    /// clean, and a type-variadic key used by the Page Replacement Policy
    /// to determine which pages to evict.
    /// </summary>
    /// <typeparam name="T">The type of the page replacement policy's key</typeparam>
    internal struct PageTableEntry<T>
    {
        /// <summary>
        /// True if the page is "dirty", i.e. has been written to
        /// </summary>
        public bool Dirty;
        /// <summary>
        /// The number of the corresponding phyiscal frame
        /// </summary>
        public uint FrameNumber;
        /// <summary>
        /// The key value used by the page replacement policy to decide
        /// which page to evict
        /// </summary>
        public T ComparisonKey;
    }

    /// <summary>
    /// Interface representing a page replacement policy for a Page Table.
    /// Each page has a key of type T that is created, updated, and used by
    /// the methods in the interface implementation to keep track of pages and 
    /// determine which ones to evict when needed.
    /// </summary>
    /// <typeparam name="T">The type of the replacement policy's key</typeparam>
    internal interface IPageReplacementPolicy<T>
    {
        /// <summary>
        /// Update the key of a page, given the old one.
        /// </summary>
        /// <param name="prevKey">The previous key</param>
        /// <returns>The new key</returns>
        T TouchPage(T prevKey);
        /// <summary>
        /// Gets the initial value of a newly-created page's key
        /// </summary>
        /// <returns>The intial value of the key</returns>
        T GetInitialKeyValue();
        /// <summary>
        /// Determines which page should be evicted based on the page-key mappings
        /// </summary>
        /// <param name="pages">The page-key mapping</param>
        /// <returns>Which page to evict</returns>
        PageTableKey EvictPage(Dictionary<PageTableKey, T> pages);
    }
}
