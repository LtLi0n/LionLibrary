using System;

namespace LionLibrary
{
    public interface IEntityIdentity<KeyT>
        where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        KeyT Id { get; set; }
    }
}
