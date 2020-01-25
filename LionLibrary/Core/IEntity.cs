using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LionLibrary
{
    public interface IEntity<EntityT, KeyT> : IEntityBase<EntityT>
    where EntityT : class
    where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        KeyT Id { get; }
    }

    public interface IEntityBase<EntityT>
        where EntityT : class { }

    public class EntityEqualityComparer<EntityT, KeyT> : EqualityComparer<EntityT>
    where EntityT : class, IEntity<EntityT, KeyT>
    where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        public override bool Equals([AllowNull] EntityT x, [AllowNull] EntityT y) => Equals(x.Id, y.Id);
        public override int GetHashCode([DisallowNull] EntityT obj) => obj.Id.GetHashCode();
    }
}
