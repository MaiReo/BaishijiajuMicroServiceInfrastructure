using Core.Session;
using System;

namespace Core.Abstractions.Tests
{
    public class UnitTestCoreSession : ICoreSession
    {
        public UnitTestCoreSession(string cityId,
            ICoreSessionContainer<Guid?, string> company,
            ICoreSessionContainer<string, string> user,
            ISessionOrganization organization = default,
            ICoreSessionContainer<string, string> broker = default)
        {
            City = CoreSessionContainer.Create(cityId);
            Company = company ?? CoreSessionContainer.Create(default(Guid?), null);
            User = user ?? CoreSessionContainer.Create(default(string), null);
            Organization = organization ?? new SessionOrganization();
            Broker = broker ?? CoreSessionContainer.Create(default(string), null);
        }

        public  ICoreSessionContainer<string> City { get; }

        public  ICoreSessionContainer<Guid?, string> Company { get; }

        public  ICoreSessionContainer<string, string> User { get; }

        public ISessionOrganization Organization { get; }

        [Obsolete]
        public  ICoreSessionContainer<Guid?, string> Store { get; }

        public  ICoreSessionContainer<string, string> Broker { get; }
    }
}
