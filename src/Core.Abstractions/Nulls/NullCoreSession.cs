namespace Core.Session
{
    public class NullCoreSession : ICoreSession
    {
        public NullCoreSession()
        {
            City = new SessionCity();
            Company = new SessionCompany();
            Organization = new SessionOrganization();
            User = new SessionUser();
            Store = new SessionStore();
            Broker = new SessionBroker();
        }
        
        public SessionCity City { get; }

        public static NullCoreSession Instance => new NullCoreSession();

        public SessionCompany Company { get; }

        public SessionUser User { get; }

        public SessionOrganization Organization { get; }

        public SessionStore Store { get; }

        public SessionBroker Broker { get; }
    }
}