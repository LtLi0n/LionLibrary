using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        ///<summary>Get raised before the contents of the paginator are changed</summary>
        public event EventHandler<PaginatorUpdateEventArgs<EntityT, KeyT>>? PrePageUpdate;

        ///<summary>Get raised when the contents of the paginator are changed</summary>
        public event EventHandler<PaginatorUpdateEventArgs<EntityT, KeyT>>? PageUpdate;

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
            if (TotalPages == 0) TotalPages = 1;
        }

        public async Task PullPageAsync(
            int page,
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null)
        {
            IPaginatedList<EntityT, KeyT>? paginator = await GetPaginatorAsync(connector, config, page);
            SyncWith(paginator);
        }

        public Task PullCurrentPageAsync(
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null) =>
            PullPageAsync(PageIndex, connector, config);

        public async Task GotoNextPageAsync(
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null)
        {
            if (HasNextPage)
            {
                PageIndex++;
                await PullCurrentPageAsync(connector, config).ConfigureAwait(false);
            };
        }

        public async Task GotoPreviousPageAsync(
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null)
        {
            if (HasPreviousPage)
            {
                PageIndex--;
                await PullCurrentPageAsync(connector, config).ConfigureAwait(false);
            };
        }

        private void SyncWith(IPaginatedList<EntityT, KeyT>? paginator)
        {
            var args = new PaginatorUpdateEventArgs<EntityT, KeyT>(paginator);

            PrePageUpdate?.Invoke(this, args);

            if (paginator != null)
            {
                Entities = paginator.Entities;
                PageIndex = paginator.PageIndex;
                Count = paginator.Count;
                TotalPages = paginator.TotalPages;
            }
            else
            {
                Entities = Enumerable.Empty<EntityT>();
                PageIndex = 1;
                Count = 0;
                TotalPages = 1;
            }

            PageUpdate?.Invoke(this, args);
        }

        public abstract Task<IPaginatedList<EntityT, KeyT>?> GetPaginatorAsync(
            ApiConnectorCRUDBase<EntityT, KeyT> connector,
            Action<ConnectorRequest_GET<ApiConnectorCRUDBase<EntityT, KeyT>>>? config = null,
            int? page = null);
    }
}
