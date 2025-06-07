﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 9/2019
Author: Pablo Carbonell
*/

using Integrative.Lara.Tests.Main;
using Moq;
using System;
using System.Net;
using Xunit;

namespace Integrative.Lara.Tests.Middleware
{
    public class ToolsTesting : DummyContextTesting
    {
        [Fact]
        public void DocumentLocalException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            LaraUI.InternalContext.Value = null;  // on purpose for testing purposes
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            var error = false;
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
#pragma warning disable CA1806 // Do not ignore method results
                new DocumentLocal<int>
                {
                    Value = 5
                };
#pragma warning restore CA1806 // Do not ignore method results
            }
            catch (InvalidOperationException)
            {
                error = true;
            }
            Assert.True(error);
        }

        [Fact]
        public void SessionLocalException()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            LaraUI.InternalContext.Value = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            var error = false;
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
#pragma warning disable CA1806 // Do not ignore method results
                new SessionLocal<int>
                {
                    Value = 5
                };
#pragma warning restore CA1806 // Do not ignore method results
            }
            catch (InvalidOperationException)
            {
                error = true;
            }
            Assert.True(error);
        }

        [Fact]
        public void DocumentLocalDefaultValue()
        {
            var local = BuildLocal();
            Assert.Equal(0, local.Value);
        }

        [Fact]
        public void DocumentLocalWrites()
        {
            var local = BuildLocal();
            local.Value = 3;
            Assert.Equal(3, local.Value);
        }

        [Fact]
        public void DocumentLocalSkipsReplacement()
        {
            var local = BuildLocal();
            local.Value = 6;
            local.Value = 6;
            Assert.Equal(6, local.Value);
        }

        [Fact]
        public void DocumentLocalReplaces()
        {
            var local = BuildLocal();
            local.Value = 7;
            local.Value = 8;
            Assert.Equal(8, local.Value);
        }

        [Fact]
        public async void DocumentLocalUnloads()
        {
            var local = BuildLocal(out var document);
            local.Value = 5;
            await document.NotifyUnload();
            Assert.Equal(0, local.Value);
        }

        private static DocumentLocal<int> BuildLocal()
        {
            return BuildLocal(out _);
        }

        private static DocumentLocal<int> BuildLocal(out Document document)
        {
            var context = new Mock<IPageContext>();
            document = new Document(new MyPage(), BaseModeController.DefaultKeepAliveInterval);
            context.Setup(x => x.Document).Returns(document);
            LaraUI.InternalContext.Value = context.Object;
            return new DocumentLocal<int>();
        }

        private static SessionLocal<int> GetSessionLocal(out Session session)
        {
            var context = new Mock<IPageContext>();
            var connection = new Connection(Guid.Parse("{B064124D-154D-4F49-89CF-CFC117509807}"), IPAddress.Loopback);
            session = new Session(connection);
            context.Setup(x => x.Session).Returns(session);
            LaraUI.InternalContext.Value = context.Object;
            return new SessionLocal<int>();
        }

        private static SessionLocal<int> GetSessionLocal()
        {
            return GetSessionLocal(out _);
        }

        [Fact]
        public void SessionLocalDefaultValue()
        {
            var local = GetSessionLocal();
            Assert.Equal(0, local.Value);
        }

        [Fact]
        public void SessionLocalWrites()
        {
            var local = GetSessionLocal();
            local.Value = 3;
            Assert.Equal(3, local.Value);
        }

        [Fact]
        public void SessionLocalSkipsReplacement()
        {
            var local = GetSessionLocal();
            local.Value = 6;
            local.Value = 6;
            Assert.Equal(6, local.Value);
        }

        [Fact]
        public void SessionLocalReplaces()
        {
            var local = GetSessionLocal();
            local.Value = 7;
            local.Value = 8;
            Assert.Equal(8, local.Value);
        }

        [Fact]
        public void SessionLocalUnloads()
        {
            var local = GetSessionLocal(out var session);
            local.Value = 5;
            session.Close();
            Assert.Equal(0, local.Value);
        }

        [Fact]
        public void SessionLocalService()
        {
            var context = new Mock<IWebServiceContext>();
            var connection = new Connection(Guid.Parse("{B064124D-154D-4F49-89CF-CFC117509807}"), IPAddress.Loopback);
            Session? session = new Session(connection);
            context.Setup(x => x.TryGetSession(out session)).Returns(true);
            LaraUI.InternalContext.Value = context.Object;
            var local = new SessionLocal<int>();
            Assert.Equal(0, local.Value);
            local.Value = 11;
            Assert.Equal(11, local.Value);
        }

        [Fact]
        public void SmaValueNullTrue()
        {
            Assert.True(LaraTools.SameValue((string?) null, null));
        }

        [Fact]
        [Obsolete("Old methods")]
        public void LaraFlushEvent()
        {
            var x = new HtmlInputElement();
            var builder = new LaraBuilder(x);
            builder.FlushEvent("click");
            x.NotifyEvent("click");
            Assert.True(x.Events.TryGetValue("click", out var ev));
            Assert.Equal("click", ev!.EventName);
        }
    }
}
