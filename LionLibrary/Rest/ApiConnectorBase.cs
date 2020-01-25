using NLog;
using RestSharp;

namespace LionLibrary
{
    public abstract class ApiConnectorBase
    {
        public Logger Logger { get; }
        public string Route { get; }
        public ConnectorServiceBase Connector { get; }
        public RestClient Client => Connector.Client;

        public ApiConnectorBase(
            ConnectorServiceBase connector,
            Logger logger,
            string route)
        {
            Connector = connector;
            Logger = logger;
            Route = route;
        }
    }
}
