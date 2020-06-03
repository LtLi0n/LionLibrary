using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace LionLibrary
{
    public abstract class ConnectorServiceBase
    {
        public string Name { get; set; } = "undefined";

        public RestClient Client { get; }

        protected IServiceProvider _connectors;

        ///<summary>
        ///<para>Storage of derived and child connector types.</para>
        ///<para> Item1: <see cref="ApiConnectorCRUDBase{EntityT, KeyT}"/> type.</para>
        ///<para> Item2: Connector type.</para></summary>
        protected ConcurrentDictionary<Type, Type> _derivedToChildConnectorReferences =
            new ConcurrentDictionary<Type, Type>();

        public T GetConnector<T>()
            where T : ApiConnectorBase =>
            _connectors.GetRequiredService<T>();

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

        public void WithApiConnectors(T owner, Action<ConnectorServiceRoutesBuilder> builderAction)
        {
            var builder = new ConnectorServiceRoutesBuilder();
            builderAction(builder);
            builder.ServiceCollection.AddSingleton(owner);
            _connectors = builder.ServiceCollection.BuildServiceProvider();
        }

        public void AddApiConnectors<ConnectorAttributeT>(T owner, Assembly assembly)
            where ConnectorAttributeT : Attribute
        {
            var connectorTypes = assembly.DefinedTypes
                .Where(x => x.CustomAttributes.Any(attr => attr.AttributeType == typeof(ConnectorAttributeT)));

            var builder = new ConnectorServiceRoutesBuilder();

            builder.ServiceCollection.AddSingleton(owner);

            foreach (var connectorType in connectorTypes)
            {
                builder.ServiceCollection.AddScoped(connectorType);
                _derivedToChildConnectorReferences[connectorType.BaseType!] = connectorType;
            }

            _connectors = builder.ServiceCollection.BuildServiceProvider();
        }
    }
}
