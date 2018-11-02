using Microsoft.AspNetCore.Http;

namespace Core.Session
{
    public class HttpContextCoreSessionProvider : ICoreSessionProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCoreSessionProvider(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        public ICoreSession Session => new HttpContextCoreSession(_httpContextAccessor);


    }
}
