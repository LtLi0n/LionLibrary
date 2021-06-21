using Newtonsoft.Json;
using RestSharp;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace LionLibrary
{
    [DataContract]
    public abstract class RestEntity<EntityT, KeyT> : IEntity<EntityT, KeyT>
        where EntityT : class, IEntity<EntityT, KeyT>
        where KeyT : notnull, IEquatable<KeyT>, IComparable, new()
    {
        public ConnectorServiceBase? ConnectorService { get; set; }
        public ApiConnectorCRUDBase<EntityT, KeyT>? ConnectorCRUD { get; set; }

        [DataMember]
#pragma warning disable CS8653 // If used If used with a serializer, this should never be null.
        public KeyT Id { get; set; } = new();
#pragma warning restore CS8653 // If used If used with a serializer, this should never be null.

        public T GetConnector<T>()
            where T : ApiConnectorBase =>
                ConnectorService!.GetConnector<T>();

        [JsonConstructor]
        protected RestEntity() { }

        protected RestEntity(
            ConnectorServiceBase connectorService, 
            ApiConnectorCRUDBase<EntityT, KeyT> connectorCrud)
        {
            ConnectorService = connectorService;
            ConnectorCRUD = connectorCrud;
        }

        public Task<IRestResponse> PutAsync(CancellationToken cancellationToken) => 
            GetConnector(false)!.PutAsync(this);
        
        public Task<IRestResponse<RestEntity<EntityT, KeyT>>> PostAsync(CancellationToken cancellationToken) => 
            GetConnector(false)!.PostAsync(this);
        
        public Task<IRestResponse> DeleteAsync(CancellationToken cancellationToken) => 
            GetConnector(false)!.DeleteAsync(Id);

        public Task<IRestResponse>? TryPutAsync(CancellationToken cancellationToken) => 
            GetConnector(true)?.PutAsync(this);
        
        public Task<IRestResponse<RestEntity<EntityT, KeyT>>>? TryPostAsync(CancellationToken cancellationToken) => 
            GetConnector(true)?.PostAsync(this);
        
        public Task<IRestResponse>? TryDeleteAsync(CancellationToken cancellationToken) => 
            GetConnector(true)?.DeleteAsync(Id);

        ///<summary> Assign property values containing <see cref="DataMemberAttribute"/> attributes from the supplied entity. </summary>
        public void UpdateFrom(RestEntity<EntityT, KeyT> other)
        {
            var props = other.GetType().GetProperties().Where(
                x => x.CustomAttributes.Any(x => x.AttributeType == typeof(DataMemberAttribute)));

            // init DataMember values
            foreach (var prop in props)
            {
                if (prop != null)
                {
                    object? value = prop.GetValue(other);
                    prop.SetValue(this, value);
                }
            }
        }

        protected ApiConnectorCRUDBase<EntityT, KeyT>? GetConnector(bool ignoreIfNullConnector)
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
