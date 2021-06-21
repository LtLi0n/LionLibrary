using System;

namespace LionLibrary
{
    public interface IEntity<EntityT, KeyT> : 
        IEntityBase<EntityT>, IEntityIdentity<KeyT>
            where EntityT : class
            where KeyT : notnull, IEquatable<KeyT>, IComparable, new() { }

    public interface IEntityBase<EntityT>
        where EntityT : class { }
}