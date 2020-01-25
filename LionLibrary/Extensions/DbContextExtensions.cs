using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LionLibrary
{
    public static class DbContextExtensions
    {
        public static async Task<KeyT> AddEntityAsync<EntityT, KeyT>(
            this DbContext context,
            IEntity<EntityT, KeyT> entity,
            DbSet<EntityT>? db_set = null)
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable
        {
            EntityEntry<EntityT> result = await
                (db_set != null ? db_set.AddAsync((EntityT)entity) :
                context.AddAsync((EntityT)entity));
            try
            {
                await context.SaveChangesAsync();
                return (result.Entity as IEntity<EntityT, KeyT>).Id;
            }
            catch (Exception ex)
            {
                context.Remove(entity);
                throw ex;
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
            catch (Exception ex)
            {
                context.Remove(entity);
                throw ex;
            }
        }

        public static async Task UpdateEntityAsync<EntityT, KeyT>(
            this DbContext context,
            IEntity<EntityT, KeyT> entity)
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable =>
            await UpdateEntityFinalAsync(context, entity);

        public static async Task UpdateEntityAsync<EntityT>(this DbContext context, IEntityBase<EntityT> entity)
            where EntityT : class =>
            await UpdateEntityFinalAsync(context, entity);

        public static async Task UpdateEntityAsync<EntityT, KeyT>(
            this DbContext context,
            IEntity<EntityT, KeyT> entity,
            IDictionary<string, object> update_values)
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable
        {
            context.Entry(entity).CurrentValues.SetValues(update_values);
            await AttemptSaveChangesAsync(context);
        }

        private static async Task UpdateEntityFinalAsync<EntityT>(
            this DbContext context,
            EntityT entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await AttemptSaveChangesAsync(context);
        }

        private static async Task AttemptSaveChangesAsync(this DbContext context)
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
