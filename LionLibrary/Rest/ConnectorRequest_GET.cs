using RestSharp;

namespace LionLibrary
{
    public class ConnectorRequest_GET<ConnectorT>
        where ConnectorT : ApiConnectorBase
    {
        public ConnectorT Connector { get; }
        public IRestRequest Request { get; }

        public ConnectorRequest_GET(ConnectorT connector, IRestRequest request)
        {
            Connector = connector;
            Request = request;
        }

        ///<inheritdoc cref="IRestRequest.AddParameter(string, object)"/>
        public ConnectorRequest_GET<ConnectorT> AddParameter(string name, object value)
        {
            Request.AddParameter(name, value);
            return this;
        }
    }
}
