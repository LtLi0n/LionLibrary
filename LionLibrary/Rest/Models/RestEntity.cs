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
        public ConnectorServiceBase? ConnectorService { get; set; }
        public ApiConnectorCRUDBase<EntityT, KeyT>? ConnectorCRUD { get; set; }

        [DataMember]
#pragma warning disable CS8653 // If used If used with a serializer, this should never be null.
        public KeyT Id { get; set; } = default;
#pragma warning restore CS8653 // If used If used with a serializer, this should never be null.

        public T? GetConnector<T>()
            where T : ApiConnectorBase =>
            ConnectorService?.GetConnector<T>();

        [JsonConstructor]
        protected RestEntity() { }

        public RestEntity(
            ConnectorServiceBase connectorService, 
            ApiConnectorCRUDBase<EntityT, KeyT> connectorCrud)
        {
            ConnectorService = connectorService;
            ConnectorCRUD = connectorCrud;
        }

        public Task<IRestResponse> PutAsync() => GetConnector(false)!.PutAsync(this);
        public Task<IRestResponse<RestEntity<EntityT, KeyT>>> PostAsync() => GetConnector(false)!.PostAsync(this);
        public Task<IRestResponse> DeleteAsync() => GetConnector(false)!.DeleteAsync(Id);

        public Task<IRestResponse>? TryPutAsync() => GetConnector(true)?.PutAsync(this);
        public Task<IRestResponse<RestEntity<EntityT, KeyT>>>? TryPostAsync() => GetConnector(true)?.PostAsync(this);
        public Task<IRestResponse>? TryDeleteAsync() => GetConnector(true)?.DeleteAsync(Id);

        private ApiConnectorCRUDBase<EntityT, KeyT>? GetConnector(bool ignoreIfNullConnector)
        {
            var conn = ConnectorCRUD;
            if (conn == null && !ignoreIfNullConnector)
            {
                throw new NullReferenceException("Connector cannot be null.\nIf you serialize this entity, initialize the Connector property after.");
            }
            return conn;
        }
    }
}
