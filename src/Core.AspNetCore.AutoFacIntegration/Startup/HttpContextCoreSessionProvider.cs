using System;
using System.Linq;
using Core.Abstractions.Dependency;
using Core.Session;
using Microsoft.AspNetCore.Http;
using Core.Extensions;

namespace Core.Web.Startup
{
    public class HttpContextCoreSessionProvider : ICoreSessionProvider, ILifestyleSingleton
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCoreSessionProvider(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        public ICoreSession Session => new HttpContextCoreSession(_httpContextAccessor);


    }
}
