using Microsoft.Extensions.DependencyInjection;

namespace LionLibrary
{
    public class ConnectorServiceRoutesBuilder
    {
        internal ServiceCollection ServiceCollection { get; } = new ServiceCollection();
        internal ConnectorServiceRoutesBuilder() { }

        public void AddConnector<T>()
            where T : ApiConnectorBase =>
            ServiceCollection.AddScoped<T>();
    }
}
