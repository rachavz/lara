﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using Integrative.Lara.Tests.Middleware;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Integrative.Lara.Tests.Main
{
    public class StaleTesting : DummyContextTesting
    {
        [Fact]
        public async void CleanupLeavesUnexpiredDocument()
        {
            var connections = new Connections();
            var cnx = connections.CreateConnection(IPAddress.Loopback);
            var doc1 = cnx.CreateDocument(new MyPage(), BaseModeController.DefaultKeepAliveInterval);
            var doc2 = cnx.CreateDocument(new MyPage(), BaseModeController.DefaultKeepAliveInterval);
            doc2.ModifyLastUtcForTesting(DateTime.UtcNow.AddHours(-10));
            await Task.Delay(200);
            using (var collector = new StaleConnectionsCollector(connections))
            {
                await collector.CleanupExpiredHandler();
            }
            Assert.True(cnx.TryGetDocument(doc1.VirtualId, out _));
            Assert.False(cnx.TryGetDocument(doc2.VirtualId, out _));
        }

        [Fact]
        public async void EmptyConnectionGetsCollected()
        {
            var connections = new Connections();
            var cnx = connections.CreateConnection(IPAddress.Loopback);
            using (var collector = new StaleConnectionsCollector(connections))
            {
                await collector.CleanupExpiredHandler();
            }
            Assert.False(connections.TryGetConnection(cnx.Id, out _));
        }

        [Fact]
        public void TimerInterval()
        {
            using var x = new Connections
            {
                StaleCollectionInterval = 100,
                StaleExpirationInterval = 200
            };
            Assert.Equal(100, x.StaleCollectionInterval);
            Assert.Equal(200, x.StaleExpirationInterval);
        }
    }
}
