﻿using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace LionLibrary.Utils
{
    public class BackgroundServiceStarter<T> : IHostedService 
        where T : IHostedService
    {
        private readonly T _backgroundService;

        public BackgroundServiceStarter(T backgroundService)
        {
            _backgroundService = backgroundService;
        }

        public Task StartAsync(CancellationToken cancellationToken) =>
            _backgroundService.StartAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) =>
            _backgroundService.StopAsync(cancellationToken);
    }
}
