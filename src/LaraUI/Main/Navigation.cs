﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 6/2019
Author: Pablo Carbonell
*/

using System;
using System.Threading.Tasks;

namespace Integrative.Lara
{
    internal sealed class Navigation : INavigation
    {
        private readonly PageContext _context;

        public string? RedirectLocation { get; private set; }

        public Navigation(PageContext context)
        {
            _context = context;
        }

        public void Replace(string location)
        {
            if (_context.Http.Request.Method == "GET")
            {
                ReplaceGet(location);
            }
            else
            {
                ReplacePost(location);
            }
        }

        private void ReplaceGet(string location)
        {
            RedirectLocation = location;
        }

        private void ReplacePost(string location)
        {
            ReplaceDelta.Enqueue(_context.Document, location);
        }

        public async Task FlushPartialChanges()
        {
            if (_context.Socket == null)
            {
                throw new InvalidOperationException(Resources.FlushNotAvailable);
            }
            if (_context.Document.HasPendingChanges)
            {
                await PostEventHandler.FlushPartialChanges(_context.Socket, _context.Document);
            }
        }
    }
}
