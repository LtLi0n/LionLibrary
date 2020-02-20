using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using System;

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
    }
}
