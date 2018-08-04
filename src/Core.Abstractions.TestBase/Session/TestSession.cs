using Core.Session;

namespace Core.Abstractions.TestBase
{
    internal class TestSession : ICoreSession
    {
        public TestSession()
        {
            City = new City();
            Company = new Company();
        }
        public City City { get; set; }

        public static TestSession Instance { get; }

        public Company Company { get; set; }

        static TestSession()
        {
            Instance = new TestSession();
        }
    }

}
