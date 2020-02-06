using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace LionLibrary
{
    [DataContract]
    public abstract class PaginatedListBase<EntityT, KeyT> : 
        IPaginatedList<EntityT, KeyT>
        where EntityT : class, IEntity<EntityT, KeyT>
        where KeyT : notnull, IEquatable<KeyT>, IComparable
    {
        [DataMember]
        public IEnumerable<EntityT> Entities { get; protected set; }

        [DataMember]
        public int Count { get; protected set; }

        [DataMember]
        public int PageIndex { get; protected set; }

        [DataMember]
        public int TotalPages { get; protected set; }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public event EventHandler? EntityDownloadStart;
        public event EventHandler? EntityDownloadFinish;

        ///<summary>Get raised before the contents of the paginator are changed</summary>
        public Func<PaginatorUpdateEventArgs<EntityT, KeyT>, Task>? PrePageUpdateTask { get; set; }

        ///<summary>Get raised when the contents of the paginator are changed</summary>
        public Func<PaginatorUpdateEventArgs<EntityT, KeyT>, Task>? PageUpdateTask { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; private set; } = 
            new CancellationTokenSource();

        public PaginatedListBase()
        {
            Entities = Enumerable.Empty<EntityT>();
            Count = 0;
            PageIndex = 1;
            TotalPages = 1;
        }

        public PaginatedListBase(IEnumerable<EntityT> items, int count, int pageIndex, int pageSize)
        {
            Entities = items;
            Count = count;
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            
            if (TotalPages == 0)
            {
                TotalPages = 1;
            }
        }

        public async Task PullCurrentPageAsync(
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null) =>
            await PullPageAsync(PageIndex, connector, config).ConfigureAwait(false);

        protected async Task PullPageAsync(
            int page,
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null)
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new CancellationTokenSource();

            try
            {
                EntityDownloadStart?.Invoke(this, new EventArgs());
                IPaginatedList<EntityT, KeyT>? paginator = await GetPaginatorAsync(connector, config, page, CancellationTokenSource.Token).ConfigureAwait(false);
                await UpdatePaginator(paginator).ConfigureAwait(false);
                EntityDownloadFinish?.Invoke(this, new EventArgs());
            }
            catch(TaskCanceledException) { }
        }

        protected async Task PullNextPageAsync(
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null)
        {
            if (HasNextPage)
            {
                PageIndex++;
                await PullPageAsync(PageIndex, connector, config).ConfigureAwait(false);
            };
        }

        protected async Task PullPreviousPageAsync(
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null)
        {
            if (HasPreviousPage)
            {
                PageIndex--;
                await PullPageAsync(PageIndex, connector, config).ConfigureAwait(false);
            };
        }

        private async Task UpdatePaginator(IPaginatedList<EntityT, KeyT>? fromPaginator)
        {
            var args = new PaginatorUpdateEventArgs<EntityT, KeyT>(fromPaginator);

            if (PrePageUpdateTask != null)
            {
                await PrePageUpdateTask.Invoke(args).ConfigureAwait(false);
            }
            
            if (fromPaginator != null)
            {
                Entities = fromPaginator.Entities;
                PageIndex = fromPaginator.PageIndex;
                Count = fromPaginator.Count;
                TotalPages = fromPaginator.TotalPages;
            }
            else
            {
                Entities = Enumerable.Empty<EntityT>();
                PageIndex = 1;
                Count = 0;
                TotalPages = 1;
            }

            if(PageUpdateTask != null)
            {
                await PageUpdateTask.Invoke(args).ConfigureAwait(false);
            }
        }

        public abstract Task<IPaginatedList<EntityT, KeyT>?> GetPaginatorAsync(
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null,
            int? page = null,
            CancellationToken cancelToken = default);
    }
}
