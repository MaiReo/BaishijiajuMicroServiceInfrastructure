using Core.Abstractions.Dependency;
using Core.Extensions;
using Core.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Core.Web.Startup
{
    public class HttpContextCoreSession : ICoreSession, ILifestyleSingleton
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCoreSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public virtual SessionCity City => new SessionCity(GetCityId());

        public virtual SessionCompany Company => new SessionCompany(GetCompanyId(), GetCompanyName());

        public virtual SessionUser User => new SessionUser(GetUserId(), GetUserName());

        public virtual SessionStore Store => new SessionStore(GetStoreId(), GetStoreName());

        public virtual SessionBroker Broker => new SessionBroker(GetBrokerId(), GetBrokerName());

        public virtual SessionOrganization Organization => new SessionOrganization(GetOrganizationId(), GetOrganizationName());

        private string GetCityId()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.CityId, out var value))
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
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.CompanyId, out var value))
            {
                return value.ToArray()?.FirstOrDefault().AsGuidOrDefault();
            }
            return default;
        }

        private string GetCompanyName()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.CompanyName, out var value))
            {
                return TryUrlDecode(value);
            }
            return default;
        }

       

        private string GetOrganizationId()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.OrganizationId, out var value))
            {
                return value;
            }
            return default;
        }

        private string GetOrganizationName()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.OrganizationName, out var value))
            {
                return TryUrlDecode(value);
            }
            return default;
        }


        private string GetUserId()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.CurrentUserId, out var value))
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
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.CurrentUserName, out var value))
            {
                return TryUrlDecode(value);
            }
            return default;
        }

        private Guid? GetStoreId()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.StoreId, out var value))
            {
                return value.ToArray()?.FirstOrDefault().AsGuidOrDefault();
            }
            return default;
        }

        private string GetStoreName()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.StoreName, out var value))
            {
                return TryUrlDecode(value);
            }
            return default;
        }

        private string GetBrokerId()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.BrokerId, out var value))
            {
                return value;
            }
            return default;
        }

        private string GetBrokerName()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default;
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(SessionConsts.BrokerName, out var value))
            {
                return TryUrlDecode(value);
            }
            return default;
        }


        private string TryUrlDecode(StringValues values)
        {
            var urlEncoded = values.ToArray()?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(urlEncoded))
            {
                return default;
            }
            try
            {
                var urlDecoded = WebUtility.UrlDecode(urlEncoded);
                return urlDecoded;
            }
            catch
            {
                
            }
            return default;
        }

    }
}
