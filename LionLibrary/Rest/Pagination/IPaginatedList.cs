using System;
using System.Collections.Generic;

namespace LionLibrary
{
    public interface IPaginatedList<EntityT, KeyT>
        where EntityT : class, IEntity<EntityT, KeyT>
        where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        IEnumerable<EntityT> Entities { get; }
        int Count { get; }
        int PageIndex { get; }
        int TotalPages { get; }
    }
}
