﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Integrative.Lara
{
    internal abstract class BaseHandler
    {
        private readonly RequestDelegate _next;

        protected BaseHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext http)
        {
            try
            {
                await TryInvoke(http);
            }
            catch (StatusCodeException e)
            {
                var text = $"HTTP error {(int)e.StatusCode} '{e.StatusCode}': {e.Message}";
                await MiddlewareCommon.SendStatusReply(http, e.StatusCode, text);
            }
        }

        private async Task TryInvoke(HttpContext http)
        {
            if (!await ProcessRequest(http).ConfigureAwait(false))
            {
                await _next.Invoke(http);
            }
        }

        internal abstract Task<bool> ProcessRequest(HttpContext http);
    }
}
