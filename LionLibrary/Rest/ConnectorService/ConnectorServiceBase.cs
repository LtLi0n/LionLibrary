using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using System;
using System.Linq;
using System.Reflection;

namespace LionLibrary
{
    public abstract class ConnectorServiceBase
    {
        public string Name { get; set; } = "undefined";

        public RestClient Client { get; }

        protected IServiceProvider _controllers;

        public T GetConnector<T>()
            where T : ApiConnectorBase =>
            _controllers.GetRequiredService<T>();

        public ConnectorServiceBase(string host)
        {
            Client = new RestClient(host);
            _controllers = new ServiceCollection().BuildServiceProvider();
        }
    }

    public abstract class ConnectorServiceBase<T> : ConnectorServiceBase
        where T : ConnectorServiceBase<T>
    {
        public ConnectorServiceBase(string host) : base(host) { }

        public void WithApiConnectors(T owner, Action<ConnectorServiceRoutesBuilder> builderAction)
        {
            var builder = new ConnectorServiceRoutesBuilder();
            builderAction(builder);
            builder.ServiceCollection.AddSingleton(owner);
            _controllers = builder.ServiceCollection.BuildServiceProvider();
        }

        public void AddApiConnectors<ConnectorAttributeT>(T owner, Assembly assembly)
            where ConnectorAttributeT : Attribute
        {
            var connectorTypes = assembly.DefinedTypes
                .Where(x => x.CustomAttributes.Any(attr => attr.AttributeType == typeof(ConnectorAttributeT)));

            var builder = new ConnectorServiceRoutesBuilder();
            
            builder.ServiceCollection.AddSingleton(owner);

            foreach(var connectorType in connectorTypes)
            {
                builder.ServiceCollection.AddScoped(connectorType);
            }

            _controllers = builder.ServiceCollection.BuildServiceProvider();
        }
    }
}
