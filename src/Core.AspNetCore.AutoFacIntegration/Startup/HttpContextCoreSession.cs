using Core.Extensions;
using Core.Session;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Core.Web.Startup
{
    public class HttpContextCoreSession : ICoreSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCoreSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public virtual City City => new City(GetCityId());

        public Company Company => new Company(GetCompanyId());

        public User User => new User(GetUserId(), GetUserName());

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
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("CompanyId", out var value))
            {
                return value.ToArray()?.FirstOrDefault().AsGuidOrDefault();
            }
            return default;
        }

        private string GetUserId()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("CurrentUserId", out var value))
            {
                return value;
            }
            return default;
        }

        private string GetUserName()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("CurrentUserName", out var value))
            {
                return value;
            }
            return default;
        }

    }
}
