using Core.Session;
using System;

namespace Core.TestBase
{
    public class UnitTestCoreSession : ICoreSession
    {
        public UnitTestCoreSession(string cityId, Guid? brokerCompanyId, User user)
        {
            City = new City(cityId);
            Company = new Company(brokerCompanyId);
            User = user ?? new User(null, null);
        }
        public City City { get; }

        public Company Company { get; }

        public User User { get; }
    }
}
