using Newtonsoft.Json;
using RestSharp;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LionLibrary
{
    [DataContract]
    public abstract class RestEntity<EntityT, KeyT> : IEntity<EntityT, KeyT>
        where EntityT : class, IEntity<EntityT, KeyT>
        where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        public ConnectorServiceBase? Connector { get; set; }
        public ApiConnectorCRUDBase<EntityT, KeyT>? ConnectorCRUD { get; set; }

        [DataMember]
#pragma warning disable CS8653 // If used If used with a serializer, this should never be null.
        public KeyT Id { get; set; } = default;
#pragma warning restore CS8653 // If used If used with a serializer, this should never be null.

        public T? GetConnector<T>()
            where T : ApiConnectorBase =>
            Connector?.GetConnector<T>();

        [JsonConstructor]
        protected RestEntity() { }

        public RestEntity(
            KeyT id, 
            ConnectorServiceBase connector, 
            ApiConnectorCRUDBase<EntityT, KeyT> connectorCrud)
        {
            Id = id;
            Connector = connector;
            ConnectorCRUD = connectorCrud;
        }

        public Task<IRestResponse> PutAsync() => GetConnector().PutAsync(this);
        public Task<IRestResponse<EntityT>> PostAsync() => GetConnector().PostAsync(this);
        public Task<IRestResponse> DeleteAsync() => GetConnector().DeleteAsync(Id);

        private ApiConnectorCRUDBase<EntityT, KeyT> GetConnector()
        {
            var conn = ConnectorCRUD;
            if (conn == null)
            {
                throw new NullReferenceException("Connector cannot be null.\nIf you serialize this entity, initialize the Connector property after.");
            }
            return conn;
        }
    }
}
