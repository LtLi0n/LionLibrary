﻿using System;
using System.Collections.Generic;

namespace LionLibrary
{
    ///<summary>Methods to assign connectors for RestEntities, that were serialized.</summary>
    public static class RestEntityExtensions
    {
        public static RestEntity<EntityT, KeyT>? AssignRest<EntityT, KeyT>(
            this RestEntity<EntityT, KeyT>? entity,
            ApiConnectorCRUDBase<EntityT, KeyT> conn)
                where EntityT : class, IEntity<EntityT, KeyT>
                where KeyT : IEquatable<KeyT>, IComparable, new()
        {
            if (entity != null)
            {
                entity.ConnectorService = conn.ConnectorService;
                entity.ConnectorCRUD = conn;
            }
            return entity;
        }

        public static IEnumerable<RestEntity<EntityT, KeyT>>? AssignRest<EntityT, KeyT>(
            this IEnumerable<RestEntity<EntityT, KeyT>>? entities,
            ApiConnectorCRUDBase<EntityT, KeyT> conn)
                where EntityT : class, IEntity<EntityT, KeyT>
                where KeyT : IEquatable<KeyT>, IComparable, new()
        {
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    entity.ConnectorService = conn.ConnectorService;
                    entity.ConnectorCRUD = conn;
                }
            }
            return entities;
        }
    }
}
