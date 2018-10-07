using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Session
{
    public class CoreSession : ICoreSession
    {
        public CoreSession()
        {
        }

        public CoreSession(string cityId, 
            Guid? companyId, string companyName,
            Guid? storeId, string storeName, 
            string brokerId, string brokerName,
            string organizationId, string organizationName,
            string currentUserId, string currentUserName)
        {
            City = new SessionCity(cityId);
            Company = new SessionCompany(companyId, companyName);
            Store = new SessionStore(storeId, storeName);
            Organization = new SessionOrganization(organizationId, organizationName);
            User = new SessionUser(currentUserId, currentUserName);
        }

        public SessionCity City { get; }

        public SessionCompany Company { get; }

        public SessionStore Store { get; }

        public SessionBroker Broker { get; }

        public SessionOrganization Organization { get; }

        public SessionUser User { get; }

        
    }
}
