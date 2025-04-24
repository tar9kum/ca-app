using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using CA.WEB.API.Interface;

namespace CA.WEB.API.Service
{
    /// <summary>
    /// This class simulates a stock price monitor that updates stock prices at regular intervals.
    /// </summary>
    public class StockPriceUpdateService : IHostedService
    {
        private readonly IStockPriceMonitor _stockPriceMonitor;

        public StockPriceUpdateService(IStockPriceMonitor stockPriceMonitor)
        {
            _stockPriceMonitor = stockPriceMonitor;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _stockPriceMonitor.StartUpdatingPrices();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stockPriceMonitor.StopUpdatingPrices();
            return Task.CompletedTask;
        }
    }
}