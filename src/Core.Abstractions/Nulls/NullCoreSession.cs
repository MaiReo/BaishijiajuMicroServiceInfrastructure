namespace Core.Session
{
    public class NullCoreSession : ICoreSession
    {
        public NullCoreSession()
        {
            City = new City(null);
        }
        public City City { get; }

        public static NullCoreSession Instance => new NullCoreSession();
    }
}