using System;
using System.Linq;
using Core.Abstractions.Dependency;
using Core.Session;
using Microsoft.AspNetCore.Http;
using Core.Extensions;

namespace Core.Web.Startup
{
    public class HttpContextCoreSession : ICoreSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCoreSession(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }
        public virtual City City => new City(GetCityId());

        public Company Company => new Company(GetCompanyId());

        private string GetCityId()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("CityId", out var value))
            {
                return value;
            }
            return default;
        }

        private Guid? GetCompanyId()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("BrokerCompanyId", out var value))
            {
                return value.ToArray()?.FirstOrDefault().AsGuidOrDefault();
            }
            return default;
        }

    }
}
