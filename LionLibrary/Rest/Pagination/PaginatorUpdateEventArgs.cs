using System;
using System.Collections.Generic;
using System.Linq;

namespace LionLibrary
{
    public class PaginatorUpdateEventArgs<EntityT, KeyT> :
    EventArgs, IPaginatedList<EntityT, KeyT>
    where EntityT : class, IEntity<EntityT, KeyT>
    where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        ///<summary>Represents a paginator obtained through a connector</summary>
        public IPaginatedList<EntityT, KeyT>? LastPaginator { get; }

        public IEnumerable<EntityT> Entities { get; private set; }
        public int Count { get; private set; }
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatorUpdateEventArgs(IPaginatedList<EntityT, KeyT>? paginator)
        {
            LastPaginator = paginator;

            if (paginator != null)
            {
                Entities = paginator.Entities;
                PageIndex = paginator.PageIndex;
                Count = paginator.Count;
                TotalPages = paginator.TotalPages;
            }
            else
            {
                Entities = Enumerable.Empty<EntityT>();
                PageIndex = 1;
                Count = 0;
                TotalPages = 1;
            }
        }
    }
}
