using Core.Abstractions.Dependency;
using Core.Session;
using Microsoft.AspNetCore.Http;

namespace Core.Web.Startup
{
    public class HttpContextCoreSession : ICoreSession, ILifestyleSingleton
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCoreSession(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }
        public virtual City City => new City(GetCityId());

        private string GetCityId()
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers == null)
            {
                return default(string);
            }
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("CityId", out var value))
            {
                return value;
            }
            return default(string);
        }
    }
}
