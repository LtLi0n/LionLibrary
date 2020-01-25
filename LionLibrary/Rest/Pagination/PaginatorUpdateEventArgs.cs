using System;
using System.Collections.Generic;

namespace LionLibrary
{
    public class PaginatorUpdateEventArgs<EntityT, KeyT> :
    EventArgs, IPaginatedList<EntityT, KeyT>
    where EntityT : class, IEntity<EntityT, KeyT>
    where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        public IEnumerable<EntityT> Items { get; private set; }
        public int Count { get; private set; }
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatorUpdateEventArgs(IPaginatedList<EntityT, KeyT> paginator)
        {
            Items = paginator.Items;
            Count = paginator.Count;
            PageIndex = paginator.PageIndex;
            TotalPages = paginator.TotalPages;
        }
    }
}
