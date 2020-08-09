using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LionLibrary
{
    public abstract class ConnectorServiceBase
    {
        public string Name { get; set; } = "undefined";

        public RestClient Client { get; }

        protected IServiceProvider _connectors;

        protected ConcurrentDictionary<Type, Type> _derivedToChildConnectorReferences =
            new ConcurrentDictionary<Type, Type>();

        ///<summary>
        ///<para>Storage of derived and child connector types.</para>
        ///<para> Item1: <see cref="ApiConnectorCRUDBase{EntityT, KeyT}"/> type.</para>
        ///<para> Item2: Connector type.</para></summary>
        public IReadOnlyDictionary<Type, Type> DerivedToChildConnectorReferences => 
            _derivedToChildConnectorReferences;

        public T GetConnector<T>()
            where T : ApiConnectorBase =>
            _connectors.GetRequiredService<T>();

        public ApiConnectorBase? GetConnector(Type type) =>
            _connectors.GetService(type) as ApiConnectorBase;

        public ApiConnectorCRUDBase<EntityT, KeyT> GetConnector<EntityT, KeyT>()
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable =>
                (ApiConnectorCRUDBase<EntityT, KeyT>)_connectors.GetRequiredService(
                    _derivedToChildConnectorReferences[typeof(ApiConnectorCRUDBase<EntityT, KeyT>)]);

        protected ConnectorServiceBase(string host)
        {
            Client = new RestClient(host);
            _connectors = new ServiceCollection().BuildServiceProvider();
        }
    }

    public abstract class ConnectorServiceBase<T> : ConnectorServiceBase
        where T : ConnectorServiceBase<T>
    {
        protected ConnectorServiceBase(string host) : base(host) { }

        public void WithApiConnectors(Action<ConnectorServiceRoutesBuilder> builderAction)
        {
            var builder = new ConnectorServiceRoutesBuilder();
            builderAction(builder);
            builder.ServiceCollection.AddSingleton((T)this);
            _connectors = builder.ServiceCollection.BuildServiceProvider();
        }

        protected Task AddApiConnectorsAsync<ConnectorAttributeT>(Assembly assembly)
            where ConnectorAttributeT : Attribute
        {
            var connectorTypes = assembly.DefinedTypes
                .Where(x => x.CustomAttributes.Any(attr => attr.AttributeType == typeof(ConnectorAttributeT)));

            var builder = new ConnectorServiceRoutesBuilder();

            builder.ServiceCollection.AddSingleton((T)this);

            foreach (var connectorType in connectorTypes)
            {
                builder.ServiceCollection.AddScoped(connectorType);
                _derivedToChildConnectorReferences[connectorType.BaseType!] = connectorType;
            }

            _connectors = builder.ServiceCollection.BuildServiceProvider();

            return Task.CompletedTask;
        }
    }
}
