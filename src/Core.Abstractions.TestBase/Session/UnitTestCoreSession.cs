using Core.Session;
using System;

namespace Core.TestBase
{
    public class UnitTestCoreSession : ICoreSession
    {
        public UnitTestCoreSession(string cityId, Guid? brokerCompanyId,
            SessionUser user,
            SessionOrganization organization = default,
            SessionStore store = default,
            SessionBroker broker = default)
        {
            City = new SessionCity(cityId);
            Company = new SessionCompany(brokerCompanyId);
            User = user ?? new SessionUser();
            Organization = organization;
            Store = store;
            Broker = broker;
        }

        public SessionCity City { get; }

        public SessionCompany Company { get; }

        public SessionUser User { get; }

        public SessionOrganization Organization { get; }

        public SessionStore Store { get; }

        public SessionBroker Broker { get; }
    }
}
