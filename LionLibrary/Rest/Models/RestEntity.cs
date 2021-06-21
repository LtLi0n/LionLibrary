using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace LionLibrary
{
    [DataContract]
    public abstract class RestEntity<EntityT, KeyT> : 
        IEntity<EntityT, KeyT>, 
        IComparable<RestEntity<EntityT, KeyT>>,
        IEquatable<RestEntity<EntityT, KeyT>>
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable, new()
    {
        public ConnectorServiceBase? ConnectorService { get; set; }
        public ApiConnectorCRUDBase<EntityT, KeyT>? ConnectorCRUD { get; set; }

        [DataMember]
        public KeyT Id { get; set; } = new();

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

        public Task<IRestResponse> PutAsync(CancellationToken cancellationToken = default) => 
            GetConnector(false)!.PutAsync(this, cancellationToken);
        
        public Task<IRestResponse<RestEntity<EntityT, KeyT>>> PostAsync(CancellationToken cancellationToken = default) => 
            GetConnector(false)!.PostAsync(this, cancellationToken);
        
        public Task<IRestResponse> DeleteAsync(CancellationToken cancellationToken = default) => 
            GetConnector(false)!.DeleteAsync(Id, cancellationToken);

        public Task<IRestResponse>? TryPutAsync(CancellationToken cancellationToken = default) => 
            GetConnector(true)?.PutAsync(this, cancellationToken);
        
        public Task<IRestResponse<RestEntity<EntityT, KeyT>>>? TryPostAsync(CancellationToken cancellationToken = default) => 
            GetConnector(true)?.PostAsync(this, cancellationToken);
        
        public Task<IRestResponse>? TryDeleteAsync(CancellationToken cancellationToken = default) => 
            GetConnector(true)?.DeleteAsync(Id, cancellationToken);

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

        public int CompareTo([AllowNull] RestEntity<EntityT, KeyT> other)
        {
            if (other != null)
            {
                return Id.CompareTo(other.Id);
            }

            return -1;
        }

        public override bool Equals(object? obj)
        {
            if(obj == null)
            {
                return false;
            }

            if(obj is RestEntity<EntityT, KeyT> entity)
            {
                return Equals(entity);
            }
            else
            {
                return false;
            }
        }

        public bool Equals([AllowNull] RestEntity<EntityT, KeyT> other) =>
            other != null && other.Id.CompareTo(Id) == 0;

        public override int GetHashCode() => 
            HashCode.Combine(Id);

        public static bool operator ==(RestEntity<EntityT, KeyT>? left, RestEntity<EntityT, KeyT>? right) =>
            EqualityComparer<RestEntity<EntityT, KeyT>>.Default.Equals(left, right);

        public static bool operator !=(RestEntity<EntityT, KeyT>? left, RestEntity<EntityT, KeyT>? right) =>
            !(left == right);
    }
}
