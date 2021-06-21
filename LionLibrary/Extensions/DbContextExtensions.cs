using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;

namespace LionLibrary
{
    public static class DbContextExtensions
    {
        public static async Task<KeyT> AddEntityAsync<EntityT, KeyT>(
            this DbContext context,
            IEntity<EntityT, KeyT> entity,
            DbSet<EntityT>? db_set = null)
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable, new()
        {
            EntityEntry<EntityT> result = await
                (db_set != null ? db_set.AddAsync((EntityT)entity) :
                context.AddAsync((EntityT)entity));
            try
            {
                await context.SaveChangesAsync();
                return result.Entity.Id;
            }
            catch
            {
                context.Remove(entity);
                throw;
            }
        }

        public static async Task<EntityT> AddEntityAsync<EntityT>(
            this DbContext context,
            IEntityBase<EntityT> entity,
            DbSet<EntityT>? db_set = null)
            where EntityT : class
        {
            EntityEntry<EntityT> result = await
                (db_set != null ? db_set.AddAsync((EntityT)entity) :
                context.AddAsync((EntityT)entity));
            try
            {
                await context.SaveChangesAsync();
                return result.Entity;
            }
            catch
            {
                context.Remove(entity);
                throw;
            }
        }

        public static Task UpdateEntityAsync<EntityT, KeyT>(this DbContext context, IEntity<EntityT, KeyT> entity, CancellationToken cancellationToken = default)
                where EntityT : class, IEntity<EntityT, KeyT>
                where KeyT : notnull, IEquatable<KeyT>, IComparable, new() =>
            UpdateEntityFinalAsync(context, entity, cancellationToken);

        public static Task UpdateEntityAsync<EntityT>(this DbContext context, IEntityBase<EntityT> entity, CancellationToken cancellationToken = default)
                where EntityT : class =>
            UpdateEntityFinalAsync(context, entity, cancellationToken);

        public static Task UpdateEntityAsync<EntityT, KeyT>(
            this DbContext context,
            IEntity<EntityT, KeyT> entity,
            IDictionary<string, object> update_values,
            CancellationToken cancellationToken = default)
                where EntityT : class, IEntity<EntityT, KeyT>
                where KeyT : notnull, IEquatable<KeyT>, IComparable, new()
        {
            context.Entry(entity).CurrentValues.SetValues(update_values);
            return AttemptSaveChangesAsync(context, cancellationToken);
        }

        private static Task UpdateEntityFinalAsync<EntityT>(
            this DbContext context,
            EntityT entity,
            CancellationToken cancellationToken = default)
        {
            context.Entry(entity).State = EntityState.Modified;
            return AttemptSaveChangesAsync(context, cancellationToken);
        }

        private static async Task AttemptSaveChangesAsync(
            this DbContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }
    }
}
