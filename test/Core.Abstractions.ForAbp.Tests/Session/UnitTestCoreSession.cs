using Core.Session;
using System;

namespace Core.Abstractions.Tests
{
    public class UnitTestCoreSession : ICoreSession
    {
        public UnitTestCoreSession(string cityId,
            SessionCompany company,
            SessionUser user,
            SessionOrganization organization = default,
            SessionStore store = default,
            SessionBroker broker = default)
        {
            City = new SessionCity(cityId);
            Company = company;
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
