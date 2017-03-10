using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_3
{
    internal class PageTable<T>
    {
        private readonly IPageReplacementPolicy<T> _replacementPolicy;
        private readonly Dictionary<PageTableKey, PageTableEntry<T>> _pageTable;
        public readonly uint FrameBits;

        public PageTable(IPageReplacementPolicy<T> replacementPolicy, uint memorySize, uint frameSize)
        {
            _pageTable = new Dictionary<PageTableKey, PageTableEntry<T>>();
            _replacementPolicy = replacementPolicy;
            FrameBits = memorySize / FrameBits;
        }


    }

    /// <summary>
    /// An entry in the page table. Contains both the PID of the process,
    /// and the address of the page.
    /// </summary>
    internal struct PageTableKey
    {
        public uint PID;
        public ulong Address;
    }

    /// <summary>
    /// An entry in the page table. Contains a member indicating if the page is
    /// clean, and a type-variadic key used by the Page Replacement Policy
    /// to determine which pages to evict.
    /// </summary>
    /// <typeparam name="T">The type of the page replacement policy's key</typeparam>
    internal struct PageTableEntry<T>
    {
        public bool Dirty;
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
