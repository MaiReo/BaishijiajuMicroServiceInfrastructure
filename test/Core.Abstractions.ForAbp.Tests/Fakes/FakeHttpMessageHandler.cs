using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Abstractions.Tests.Fakes
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public const string NOT_FOUND_URL = "http://unit.test/notfound";

        public class FakeHttpMessageHandlerContext
        {
            public string RequestUrl { get; set; }

            public HttpStatusCode ResponseStatusCode { get; set; }

            public HttpContent ResponseContent { get; set; }

            public string ResponseContentType { get; set; }
        }

        private readonly IDictionary<string, Action<FakeHttpMessageHandlerContext>> _handlers;

        public FakeHttpMessageHandler()
        {
            _handlers = new Dictionary<string, Action<FakeHttpMessageHandlerContext>>();
            RegisterPreDefiendUrls();
        }

        private void RegisterPreDefiendUrls()
        {
            Register(NOT_FOUND_URL, x =>
            {
                x.ResponseStatusCode = HttpStatusCode.NotFound;
                x.ResponseContent = null;
                x.ResponseContentType = null;
            });
        }

        public void Register(string url, Action<FakeHttpMessageHandlerContext> setupAction)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }
            url = url.ToLowerInvariant();
            if (_handlers.ContainsKey(url))
            {
                _handlers[url] = setupAction;
                return;
            }
            _handlers.Add(url, setupAction);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            var url = request.RequestUri.ToString().ToLowerInvariant();
            if (_handlers.TryGetValue(url, out var setupAction))
            {
                var context = new FakeHttpMessageHandlerContext
                {
                    RequestUrl = url,
                    ResponseStatusCode = HttpStatusCode.OK,
                    ResponseContent = new ByteArrayContent(new byte[0], 0, 0),
                    ResponseContentType = "application/octet-steam"
                };
                setupAction?.Invoke(context);
                response.StatusCode = context.ResponseStatusCode;
                response.Content = context.ResponseContent;
                if (string.IsNullOrWhiteSpace(context.ResponseContentType) && response.Content != null && response.Content.Headers.ContentType == null)
                {
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(context.ResponseContentType);
                }
            }
            return Task.FromResult(response);
        }

        public static FakeHttpMessageHandler Instance { get; }

        static FakeHttpMessageHandler() => Instance = new FakeHttpMessageHandler();
    }
}