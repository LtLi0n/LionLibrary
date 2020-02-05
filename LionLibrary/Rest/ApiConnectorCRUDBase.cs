using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

using RestRequest = RestSharp.RestRequest;

namespace LionLibrary
{
    public abstract class ApiConnectorCRUDBase<EntityT, KeyT> : ApiConnectorBase
            where EntityT : class, IEntity<EntityT, KeyT>
            where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        public const int MAX_BULK_POST = 50;

        public ApiConnectorCRUDBase(
            ConnectorServiceBase connector,
            Logger logger,
            string route) : base(connector, logger, route) { }

        public async Task<IRestResponse<EntityT>> PostAsync<T>(T entity)
            where T : IEntity<EntityT, KeyT>
        {
            RestRequest request = new RestRequest(Route, Method.POST) { RequestFormat = DataFormat.Json };

            string json = JsonConvert.SerializeObject(entity);
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = await Client.ExecuteAsync<EntityT>(request).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.Created)
            {
                Logger?.Error($"Failed POST request: {response.StatusCode} ({response.StatusDescription})\n{response.Content}");
            }

            return response;
        }

        public async IAsyncEnumerable<IRestResponse<EntityT>> PostAsync<T>(IEnumerable<T> entities)
            where T : IEntity<EntityT, KeyT>
        {
            var tasks = new Queue<Task<IRestResponse<EntityT>>>(MAX_BULK_POST);
            foreach (var entity in entities)
            {
                tasks.Enqueue(PostAsync(entity));

                if (tasks.Count >= MAX_BULK_POST)
                {
                    yield return await tasks.Dequeue().ConfigureAwait(false);
                }
            }

            foreach (var postTask in tasks)
            {
                yield return await postTask.ConfigureAwait(false);
            }
        }


        public Task<EntityT?> GetAsync(
            KeyT id,
            IDictionary<KeyT, EntityT>? cache = null, 
            Action<EntityT>? initFunc = null,
            CancellationToken cancelToken = default) => 
            GetAsync<EntityT>(id, cache, initFunc, cancelToken);

        public async Task<T?> GetAsync<T>(
            KeyT id,
            IDictionary<KeyT, T>? cache = null, 
            Action<T>? initFunc = null,
            CancellationToken cancelToken = default)
            where T : class, IEntity<EntityT, KeyT>
        {
            SpinWait sw = new SpinWait();

            while (cache != null)
            {
                if(cancelToken != default)
                {
                    if(cancelToken.IsCancellationRequested)
                    {
                        return null;
                    }
                }

                try
                {
                    if (cache.TryGetValue(id, out T? cachedEntity))
                    {
                        return cachedEntity;
                    }
                }
                catch(Exception) { }

                sw.SpinOnce();
            }

            RestRequest request = new RestRequest($"{Route}/{id}", Method.GET);


            IRestResponse response = await Client.ExecuteAsync(request, cancelToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var entity = JsonConvert.DeserializeObject<T>(response.Content);
                initFunc?.Invoke(entity);
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            else
            {
                Logger?.Error($"Failed GET request: {response.StatusCode} ({response.StatusDescription})");
                return default;
            }
        }

        public async Task<PaginatedList<EntityT, KeyT>> GetAsync(
            ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>> req,
            int? page = null)
        {
            var request = req.Request;

            if (page != null)
            {
                request.AddParameter("page", page.Value);
            }

            IRestResponse response = await Client.ExecuteAsync(request).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<PaginatedList<EntityT, KeyT>>(response.Content);
            }
            else
            {
                Logger?.Error($"Failed GET request: {response.StatusCode} ({response.StatusDescription})");
                return new PaginatedList<EntityT, KeyT>();
            }
        }

        public async Task<PaginatedList<EntityT, KeyT>> GetAsync(
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? reqExtras = null,
            int? page = null)
        {
            var req = CreateGetRequest();
            reqExtras?.Invoke(req);

            var request = req.Request;

            if (page != null)
            {
                request.AddParameter("page", page.Value);
            }

            IRestResponse response = await Client.ExecuteAsync(request).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<PaginatedList<EntityT, KeyT>>(response.Content);
            }
            else
            {
                Logger?.Error($"Failed GET request: {response.StatusCode} ({response.StatusDescription})");
                return new PaginatedList<EntityT, KeyT>();
            }
        }

        public async Task<T?> GetAsyncWithRest<T>(
            KeyT id,
            IDictionary<KeyT, T>? cache = null, 
            Action<T>? initFunc = null,
            CancellationToken cancelToken = default)
            where T : RestEntity<EntityT, KeyT>, IEntity<EntityT, KeyT>
        {
            var entity = await GetAsync(id, cache, initFunc, cancelToken).ConfigureAwait(false);
            if (entity != null)
            {
                entity.Connector = Connector;
                entity.ConnectorCRUD = this;
                initFunc?.Invoke(entity);
            }
            return entity;
        }

        public Task<IRestResponse> PutAsync<T>(T entity)
            where T : IEntity<EntityT, KeyT>
            => PutAsync(entity.Id, entity);

        public async Task<IRestResponse> PutAsync<T>(KeyT key, T entity)
            where T : IEntity<EntityT, KeyT>
        {
            RestRequest request = new RestRequest($"{Route}/{key}", Method.PUT) { RequestFormat = DataFormat.Json };
            string json = JsonConvert.SerializeObject(entity);
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = await Client.ExecuteAsync(request).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                Logger?.Error($"Failed PUT request: {response.StatusCode} ({response.StatusDescription})\n{response.Content}");
            }

            return response;
        }

        public Task<IRestResponse> DeleteAsync(IEntity<EntityT, KeyT> entity) => DeleteAsync(entity.Id);

        public async Task<IRestResponse> DeleteAsync(KeyT entityKey)
        {
            RestRequest request = new RestRequest($"{Route}/{entityKey}", Method.DELETE);

            var response = await Client.ExecuteAsync(request).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Logger?.Error($"Failed DELETE request: {response.StatusCode} ({response.StatusDescription})\n{response.Content}");
            }

            return response;
        }

        public ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>> CreateGetRequest() =>
            new ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>(this, new RestRequest(Route, Method.GET, DataFormat.Json));
    }
}
