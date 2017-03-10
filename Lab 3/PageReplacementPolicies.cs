using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab_3.PageReplacementPolicies
{
    /// <summary>
    /// Helper class for "Most 'x' Page" based replacement policies
    /// Automatically finds the most 'x' and returns it in EvictPage using 
    /// the natural ordering
    /// </summary>
    /// <typeparam name="T">The type of the comparison key to use</typeparam>
    internal abstract class MostPageReplacementPolicy<T> : IPageReplacementPolicy<T> where T : IComparable<T>
    {
        public PageTableKey EvictPage(Dictionary<PageTableKey, T > pages)
        {
            return (from pair in pages
                   orderby pair.Value descending
                   select pair.Key).First();
        }
        public abstract T GetInitialKeyValue();
        public abstract T TouchPage(T prevKey);
    }

    /// <summary>
    /// Helper class for "Least 'x' Page" based replacement policies
    /// Automatically finds the least 'x' and returns it in EvictPage using 
    /// the natural ordering
    /// </summary>
    /// <typeparam name="T">The type of the comparison key to use</typeparam>
    internal abstract class LeastPageReplacementPolicy<T> : IPageReplacementPolicy<T> where T : IComparable<T>
    {
        public PageTableKey EvictPage(Dictionary<PageTableKey, T> pages)
        {
            return (from pair in pages
                   orderby pair.Value ascending
                   select pair.Key).First();
        }
        public abstract T GetInitialKeyValue();
        public abstract T TouchPage(T prevKey);
    }

    /// <summary>
    /// Random page eviction policy.
    /// Key always returns null
    /// </summary>
    internal class Random : IPageReplacementPolicy<Object>
    {
        private readonly System.Random _r = new System.Random();
        public PageTableKey EvictPage(Dictionary<PageTableKey, object> pages)
        {
            return pages.Keys.ToList()[_r.Next(pages.Keys.Count)];
        }

        public object GetInitialKeyValue()
        {
            return null;
        }

        public object TouchPage(object prevKey)
        {
            return prevKey;
        }
    }

    /// <summary>
    /// Evicts the most frequently used pages
    /// </summary>
    internal class MostFrequentlyUsed : MostPageReplacementPolicy<uint>
    {
        public override uint GetInitialKeyValue()
        {
            return 0;
        }

        public override uint TouchPage(uint prevKey)
        {
            return prevKey + 1;
        }
    }

    /// <summary>
    /// Evicts the least frequently used pages
    /// </summary>
    internal class LeastFrequentlyUsed : IPageReplacementPolicy<uint>
    {
        public PageTableKey EvictPage(Dictionary<PageTableKey, uint> pages)
        {
            return (from pair in pages
                   orderby pair.Value ascending
                   select pair.Key).First();
        }

        public uint GetInitialKeyValue()
        {
            return 0;
        }

        public uint TouchPage(uint prevKey)
        {
            return prevKey + 1;
        }
    }

    /// <summary>
    /// Evicts the oldest pages
    /// </summary>
    internal class FIFO : IPageReplacementPolicy<uint>
    {
        private uint _pageCounter = 0;
        public PageTableKey EvictPage(Dictionary<PageTableKey, uint> pages)
        {
            return (from pair in pages
                    orderby pair.Value
                    select pair.Key).First();
        }

        public uint GetInitialKeyValue()
        {
            return _pageCounter++;
        }

        public uint TouchPage(uint prevKey)
        {
            return prevKey;
        }
    }

    /// <summary>
    /// Evicts the least recently used pages
    /// </summary>
    internal class LeastRecentlyUsed : LeastPageReplacementPolicy<uint>
    {
        private uint _counter = 0;

        public override uint GetInitialKeyValue()
        {
            return _counter++;
        }

        public override uint TouchPage(uint prevKey)
        {
            return _counter++;
        }
    }

    /// <summary>
    /// Evicts the most recently used pages
    /// </summary>
    internal class MostRecentlyUsed : MostPageReplacementPolicy<uint>
    {
        private uint _counter = 0;
        
        public override uint GetInitialKeyValue()
        {
            return _counter++;
        }

        public override uint TouchPage(uint prevKey)
        {
            return _counter++;
        }
    }
}
