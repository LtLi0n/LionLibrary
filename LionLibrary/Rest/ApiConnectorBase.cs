using NLog;
using RestSharp;

namespace LionLibrary
{
    public abstract class ApiConnectorBase
    {
        public Logger Logger { get; }
        public string Route { get; }
        public ConnectorServiceBase ConnectorService { get; }
        public RestClient Client => ConnectorService.Client;

        public ApiConnectorBase(
            ConnectorServiceBase connectorService,
            Logger logger,
            string route)
        {
            ConnectorService = connectorService;
            Logger = logger;
            Route = route;
        }
    }
}
