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
<<<<<<< HEAD
    public abstract class RestEntity<EntityT, KeyT> : IEntity<EntityT, KeyT>
        where EntityT : class, IEntity<EntityT, KeyT>
        where KeyT : notnull, IEquatable<KeyT>, IComparable, new()
=======
    public abstract class RestEntity<EntityT, KeyT> : 
        IEntity<EntityT, KeyT>, 
        IComparable<RestEntity<EntityT, KeyT>>,
        IEquatable<RestEntity<EntityT, KeyT>>
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable
>>>>>>> 8ad56222f39897a8f82b44be0bd26009eedec5b3
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

            RestEntity<EntityT, KeyT>? entity = obj as RestEntity<EntityT, KeyT>;
            if(entity == null)
            {
                return false;
            }
            else
            {
                return Equals(entity);
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
