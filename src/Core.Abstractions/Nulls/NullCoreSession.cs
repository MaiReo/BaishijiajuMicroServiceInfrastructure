namespace Core.Session
{
    public class NullCoreSession : ICoreSession
    {
        public NullCoreSession()
        {
            City = new City(null);
            Company = new Company(null);
        }
        public City City { get; }

        public static NullCoreSession Instance => new NullCoreSession();

        public Company Company { get; }
    }
}