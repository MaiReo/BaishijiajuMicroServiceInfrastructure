namespace Core.Session
{
    public interface ICoreSession
    {
        SessionCity City { get; }

        SessionCompany Company { get; }

        SessionOrganization Organization { get; }

        SessionUser User { get; }

        SessionStore Store { get; }

        SessionBroker Broker { get; }
    }
}
