namespace Core.Session
{
    public interface ICoreSession
    {
        SessionCity City { get; }

        SessionCompany Company { get; }

        SessionStore Store { get; }

        SessionBroker Broker { get; }

        SessionOrganization Organization { get; }

        SessionUser User { get; }
    }
}
