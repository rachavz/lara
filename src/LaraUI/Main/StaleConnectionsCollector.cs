﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Integrative.Lara
{
    internal sealed class StaleConnectionsCollector : IDisposable
    {
        public const double DefaultTimerInterval = 5 * 60 * 1000;      // 5 minutes to trigger updates
        public const double DefaultExpireInterval = 4 * 3600 * 1000;   // 4 hours to expire

        private readonly Connections _connections;
        private readonly Timer _timer;

        public double ExpirationInterval { get; set; }

        public double TimerInterval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public StaleConnectionsCollector(Connections connections,
            double timerInterval = DefaultTimerInterval, double expireInternal = DefaultExpireInterval)
        {
            _connections = connections;
            ExpirationInterval = expireInternal;
            _timer = new Timer
            {
                Interval = timerInterval
            };
            _timer.Elapsed += TimerElapsedHandler;
            _timer.Start();
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timer.Stop();
            _timer.Dispose();
        }

        private async void TimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            if (!_disposed && !_cleaning)
            {
                await CleanupExpiredHandler();
            }
        }

        private bool _cleaning;

        internal async Task CleanupExpiredHandler()
        {
            _cleaning = true;
            _timer.Enabled = false;
            await CleanupNonDisposed();
            _timer.Enabled = true;
            _cleaning = false;
        }

        private async Task CleanupNonDisposed()
        {
            var minRequired = DateTime.UtcNow.AddMilliseconds(-ExpirationInterval);
            var list = new List<KeyValuePair<Guid, Connection>>();
            foreach (var pair in _connections.GetConnections())
            {
                await CleanupExpired(pair.Value, minRequired);
                if (pair.Value.IsEmpty)
                {
                    list.Add(pair);
                }
            }
            foreach (var pair in list)
            {
                if (pair.Value.IsEmpty)
                {
                    await _connections.Discard(pair.Key);
                }
            }
        }

        internal static async Task CleanupExpired(Connection connection, DateTime minRequired)
        {
            var list = new List<KeyValuePair<Guid, Document>>();
            foreach (var pair in connection.GetDocuments())
            {
                if (pair.Value.LastUtc < minRequired)
                {
                    list.Add(pair);
                }
            }
            foreach (var pair in list)
            {
                if (pair.Value.LastUtc < minRequired)
                {
                    await connection.Discard(pair.Key);
                }
            }
        }
    }
}
