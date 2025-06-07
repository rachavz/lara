﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using Integrative.Lara.Tests.Main;
using Integrative.Lara.Tests.Middleware;
using System;
using Xunit;

namespace Integrative.Lara.Tests.Delta
{
    public class AttributeEditTesting : DummyContextTesting
    {
        [Fact]
        public void AttributeEdited()
        {
            var doc = CreateDocument();
            var div = Element.Create("div", "mydiv");
            doc.Body.AppendChild(div);
            doc.OpenEventQueue();
            div.SetAttribute("data-test", "x");
            var queue = doc.GetQueue();
            Assert.Single(queue);
            var step = queue.Peek() as AttributeEditedDelta;
            Assert.Equal("data-test", step!.Attribute);
            Assert.Equal("x", step.Value);
            Assert.Equal("mydiv", step.ElementId);
        }

        [Fact]
        public void UnchangedIdNoSteps()
        {
            var doc = CreateDocument();
            var div = Element.Create("div", "mydiv");
            doc.Body.AppendChild(div);
            doc.OpenEventQueue();
            div.Id = "mydiv";
            var queue = doc.GetQueue();
            Assert.Empty(queue);
        }

        [Fact]
        public void SetValueDeltaProperties()
        {
            var div = Element.Create("div", "mydiv");
            var doc = CreateDocument();
            doc.Body.AppendChild(div);
            doc.OpenEventQueue();
            div.SetAttribute("value", "x");
            var queue = doc.GetQueue();
            Assert.NotEmpty(queue);
            var step = queue.Peek() as SetValueDelta;
            Assert.NotNull(step);
            Assert.Equal("mydiv", step!.ElementId);
            Assert.Equal("x", step.Value);
        }

        private static Document CreateDocument()
        {
            var page = new MyPage();
            var doc = new Document(page,
                Connections.CreateCryptographicallySecureGuid(),
                BaseModeController.DefaultKeepAliveInterval);
            return doc;
        }

        [Fact]
        public void PlugOptionsHasEmptyConstructor()
        {
            var instance = Activator.CreateInstance<PlugOptions>();
            Assert.NotNull(instance);
        }

        [Fact]
        public void ClearChildrenOnEvent()
        {
            var div = Element.Create("div");
            var document = new Document(new MyPage(), BaseModeController.DefaultKeepAliveInterval);
            document.Body.AppendChild(div);
            document.OpenEventQueue();
            document.Body.ClearChildren();
            var queue = document.GetQueue();
            Assert.NotEmpty(queue);
            var first = queue.Peek() as ClearChildrenDelta;
            Assert.NotNull(first);
        }

        [Fact]
        public void ToggleClassToggles()
        {
            var button = new HtmlButtonElement();
            button.ToggleClass("red", true);
            Assert.True(button.HasClass("red"));
            button.ToggleClass("red", false);
            Assert.False(button.HasClass("red"));
        }

        [Fact]
        public void DocumentCreatesElements()
        {
            var button = Document.CreateElement("button");
            Assert.NotNull(button);
        }

        [Fact]
        public void DocumentCreatesText()
        {
            var text = Document.CreateTextNode("hello");
            Assert.NotNull(text);
            Assert.Equal("hello", text.Data);
        }

        [Fact]
        public void ClearChildrenElement()
        {
            var x = new ClearChildrenDelta
            {
                ElementId = "x"
            };
            Assert.Equal("x", x.ElementId);
        }

        [Fact]
        public void PlugOptionsBlocking()
        {
            var settings = new EventSettings
            {
                BlockOptions = new BlockOptions
                {
                    BlockedElementId = "a",
                    ShowElementId = "b",
                    ShowHtmlMessage = "c"
                }
            };
            var x = new PlugOptions(settings);
            Assert.Equal("a", x.BlockElementId);
            Assert.Equal("b", x.BlockShownId);
            Assert.Equal("c", x.BlockHTML);
        }

        [Fact]
        public void ServerEventsDeltaCorrectType()
        {
            var delta = new ServerEventsDelta();
            Assert.Equal(DeltaType.ServerEvents, delta.Type);
        }
    }
}
