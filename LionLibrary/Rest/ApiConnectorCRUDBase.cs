﻿using Newtonsoft.Json;
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

        protected ApiConnectorCRUDBase(
            ConnectorServiceBase connector,
            Logger logger,
            string route) : base(connector, logger, route) { }

        public virtual async Task<IRestResponse<T>> PostAsync<T>(T entity)
            where T : IEntity<EntityT, KeyT>
        {
            RestRequest request = new RestRequest(Route, Method.POST) { RequestFormat = DataFormat.Json };

            string json = JsonConvert.SerializeObject(entity);
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = await Client.ExecuteAsync<T>(request).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.Created)
            {
                Logger?.Error($"Failed POST request: {response.StatusCode} ({response.StatusDescription})\n{response.Content}");
            }

            if (response.StatusCode == 0)
            {
                throw new Exception($"[{ConnectorService.Name}] backend is offline.");
            }

            entity.Id = response.Data.Id;

            return response;
        }

        public virtual async IAsyncEnumerable<IRestResponse<T>> PostAsync<T>(IEnumerable<T> entities)
            where T : IEntity<EntityT, KeyT>
        {
            var tasks = new Queue<Task<IRestResponse<T>>>(MAX_BULK_POST);
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


        public virtual Task<EntityResult<EntityT>> GetAsync(
            KeyT id,
            IDictionary<KeyT, EntityT>? cache = null, 
            Action<EntityT>? initFunc = null,
            CancellationToken cancelToken = default) => 
            GetAsync<EntityT>(id, cache, initFunc, cancelToken);

        public virtual async Task<EntityResult<DerivedEntityT>> GetAsync<DerivedEntityT>(
            KeyT id,
            IDictionary<KeyT, DerivedEntityT>? cache = null, 
            Action<DerivedEntityT>? initFunc = null,
            CancellationToken cancelToken = default)
            where DerivedEntityT : class, IEntity<EntityT, KeyT>
        {
            if(cache == null)
            {
                CreateGetRequest();
                RestRequest request = new RestRequest($"{Route}/{id}", Method.GET);

                IRestResponse response = await Client.ExecuteAsync(request, cancelToken).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var entity = JsonConvert.DeserializeObject<DerivedEntityT>(response.Content);
                    initFunc?.Invoke(entity);
                    return new EntityResult<DerivedEntityT>(response, JsonConvert.DeserializeObject<DerivedEntityT>(response.Content));
                }
                else
                {
                    Logger?.Error($"Failed GET request: {response.StatusCode} ({response.StatusDescription})");
                    return new EntityResult<DerivedEntityT>(default, default);
                }
            }
            else
            {
                while (cache != null)
                {
                    if (cancelToken != default)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return new EntityResult<DerivedEntityT>(default, default);
                        }
                    }

                    try
                    {
                        if (cache.ContainsKey(id))
                        {
                            return new EntityResult<DerivedEntityT>(default, cache[id]);
                        }
                    }
                    catch { }

                    await Task.Delay(1);
                    //sw.SpinOnce();
                }

                Logger?.Error($"Failed to obtain object from the cache.");
                return new EntityResult<DerivedEntityT>(default, default);
            }
        }

        public virtual async Task<PaginatedList<EntityT, KeyT>> GetAsync(
            ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>> req,
            IEnumerable<Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>>? reqExtras = null,
            int? page = null,
            CancellationToken cancelToken = default)
        {
            if (reqExtras != null)
            {
                foreach (var reqExtra in reqExtras)
                {
                    reqExtra.Invoke(req);
                }
            }

            var request = req.Request;

            if (page != null)
            {
                request.AddParameter("page", page.Value);
            }

            IRestResponse response = await Client.ExecuteAsync(request, cancelToken).ConfigureAwait(false);

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

        public virtual async Task<PaginatedList<EntityT, KeyT>> GetAsync(
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? action = null,
            IEnumerable<Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>>? reqExtras = null,
            int? page = null,
            CancellationToken cancelToken = default)
        {
            var req = CreateGetRequest();
            action?.Invoke(req);

            if (reqExtras != null)
            {
                foreach (var reqExtra in reqExtras)
                {
                    reqExtra.Invoke(req);
                }
            }

            var request = req.Request;

            if (page != null)
            {
                request.AddParameter("page", page.Value);
            }

            IRestResponse response = await Client.ExecuteAsync(request, cancelToken).ConfigureAwait(false);

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

        public virtual async Task<EntityResult<DerivedEntityT>> GetAsyncWithRest<DerivedEntityT>(
            KeyT id,
            IDictionary<KeyT, DerivedEntityT>? cache = null, 
            Action<DerivedEntityT>? initFunc = null,
            CancellationToken cancelToken = default)
                where DerivedEntityT : RestEntity<EntityT, KeyT>, IEntity<EntityT, KeyT>
        {
            var result = await GetAsync(id, cache, initFunc, cancelToken).ConfigureAwait(false);
            if (result.Entity != null)
            {
                result.Entity.ConnectorService = ConnectorService;
                result.Entity.ConnectorCRUD = this;
                initFunc?.Invoke(result.Entity);
            }
            return result;
        }

        public virtual Task<IRestResponse> PutAsync<T>(T entity)
            where T : IEntity<EntityT, KeyT>
            => PutAsync(entity.Id, entity);

        public virtual async Task<IRestResponse> PutAsync<T>(KeyT key, T entity)
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

        public virtual Task<IRestResponse> DeleteAsync(IEntity<EntityT, KeyT> entity)
        {
            return DeleteAsync(entity.Id);
        }

        public virtual async Task<IRestResponse> DeleteAsync(KeyT entityKey)
        {
            RestRequest request = new RestRequest($"{Route}/{entityKey}", Method.DELETE);

            var response = await Client.ExecuteAsync(request).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Logger?.Error($"Failed DELETE request: {response.StatusCode} ({response.StatusDescription})\n{response.Content}");
            }

            return response;
        }

        public virtual ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>> CreateGetRequest(string? customRoute = null) =>
            new ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>(this, new RestRequest(customRoute ?? Route, Method.GET, DataFormat.Json));
    }
}
