using Core.Session;

namespace Core.Abstractions.TestBase
{
    internal class TestSession : ICoreSession
    {
        public TestSession()
        {
            City = new City();
        }
        public City City { get; set; }

        public static TestSession Instance { get; }

        static TestSession()
        {
            Instance = new TestSession();
        }
    }

}
