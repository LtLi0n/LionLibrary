using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LionLibrary
{
    [DataContract]
    public class PaginatedList<EntityT, KeyT> : PaginatedListBase<EntityT, KeyT>
        where EntityT : class, IEntity<EntityT, KeyT>
        where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        public PaginatedList() : base() { }

        public PaginatedList(IEnumerable<EntityT> items, int count, int pageIndex, int pageSize) :
            base(items, count, pageIndex, pageSize) { }

        public static async Task<PaginatedList<EntityT, KeyT>> CreateAsync(
            IQueryable<EntityT> source,
            int? _pageIndex,
            int pageSize)
        {
            int pageIndex = _pageIndex ?? 1;

            int count = await source.CountAsync();

            EntityT[] items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();

            return new PaginatedList<EntityT, KeyT>(items, count, pageIndex, pageSize);
        }

        public virtual async IAsyncEnumerable<IPaginatedList<EntityT, KeyT>> PullAllPagesAsync(ApiConnectorCRUDBase<EntityT, KeyT> connector)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var token = cts.Token;

            for (int page_i = 1; page_i <= TotalPages; page_i++)
            {
                var paginator = await GetPaginatorAsync(connector, page: page_i, cancelToken: token);

                if (paginator != null)
                {
                    yield return paginator;
                }
                else
                {
                    yield break;
                }
            }
        }

        public override async Task<IPaginatedList<EntityT, KeyT>?> GetPaginatorAsync(
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null,
            int? page = null,
            CancellationToken cancelToken = default) =>
                await connector.GetAsync(config, page: page, cancelToken: cancelToken);
    }
}
