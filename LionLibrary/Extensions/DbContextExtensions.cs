using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace LionLibrary
{
    public static class DbContextExtensions
    {
        private static readonly MethodInfo ContainsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(object));

        public static Task<T[]> FindAllAsync<T>(this DbContext dbContext, IEnumerable<object> keyValues)
            where T : class
        {
            var entityType = dbContext.Model.FindEntityType(typeof(T));
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey.Properties.Count != 1)
                throw new NotSupportedException("Only a single primary key is supported");

            var pkProperty = primaryKey.Properties[0];
            var pkPropertyType = pkProperty.ClrType;

            // retrieve member info for primary key
            var pkMemberInfo = typeof(T).GetProperty(pkProperty.Name);
            if (pkMemberInfo == null)
                throw new ArgumentException("Type does not contain the primary key as an accessible property");

            // build lambda expression
            var parameter = Expression.Parameter(typeof(T), "e");
            var body = Expression.Call(null, ContainsMethod,
                Expression.Constant(keyValues),
                Expression.Convert(Expression.MakeMemberAccess(parameter, pkMemberInfo), typeof(object)));
            var predicateExpression = Expression.Lambda<Func<T, bool>>(body, parameter);

            // run query
            return dbContext.Set<T>().Where(predicateExpression).ToArrayAsync();
        }

        public static async Task<KeyT> AddEntityAsync<EntityT, KeyT>(this DbContext context, IEntity<EntityT, KeyT> entity, DbSet<EntityT>? db_set = null, CancellationToken cancellationToken = default)
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable, new()
        {
            EntityEntry<EntityT> result = await (db_set != null ?
                db_set.AddAsync((EntityT)entity, cancellationToken) :
                context.AddAsync((EntityT)entity, cancellationToken));
            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return result.Entity.Id;
            }
            catch
            {
                context.Remove(entity);
                throw;
            }
        }

        public static async Task<EntityT> AddEntityAsync<EntityT>(this DbContext context, IEntityBase<EntityT> entity, DbSet<EntityT>? db_set = null, CancellationToken cancellationToken = default)
            where EntityT : class
        {
            EntityEntry<EntityT> result = await (db_set != null ? 
                db_set.AddAsync((EntityT)entity, cancellationToken) :
                context.AddAsync((EntityT)entity, cancellationToken));
            try
            {
                await context.SaveChangesAsync(cancellationToken);
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

        public static Task UpdateEntityAsync<EntityT, KeyT>(this DbContext context, IEntity<EntityT, KeyT> entity, IDictionary<string, object> update_values, CancellationToken cancellationToken = default)
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable, new()
        {
            context.Entry(entity).CurrentValues.SetValues(update_values);
            return context.SaveChangesAsync(cancellationToken);
        }

        private static Task UpdateEntityFinalAsync<EntityT>(
            this DbContext context,
            EntityT entity,
            CancellationToken cancellationToken = default)
        {
            context.Entry(entity).State = EntityState.Modified;
            return context.SaveChangesAsync(cancellationToken);
        }
    }
}
