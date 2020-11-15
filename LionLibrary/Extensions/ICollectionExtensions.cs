using System.Collections.Generic;

namespace LionLibrary
{
    public static class ICollectionExtensions
    {
        public static void CopyFrom<T>(this ICollection<T> collection, IEnumerable<T>? entities)
        {
            collection.Clear();

            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    collection.Add(entity);
                }
            }
        }
    }
}
