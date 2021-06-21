using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LionLibrary
{
    public class EntityEqualityComparer<EntityT, KeyT> : EqualityComparer<EntityT>
        where EntityT : class, IEntity<EntityT, KeyT>
        where KeyT : notnull, IEquatable<KeyT>, IComparable, new()
    {
        public override bool Equals([AllowNull] EntityT x, [AllowNull] EntityT y) => Equals(x.Id, y.Id);
        public override int GetHashCode([DisallowNull] EntityT obj) => obj.Id.GetHashCode();
    }
}
