namespace Core.Session
{
    public interface ICoreSession
    {
        City City { get; }

        Company Company { get; }

        User User { get; }
    }
}
