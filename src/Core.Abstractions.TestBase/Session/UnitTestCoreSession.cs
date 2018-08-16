using Core.Session;
using System;

namespace Core.TestBase
{
    public class UnitTestCoreSession : ICoreSession
    {
        public UnitTestCoreSession(string cityId, Guid? brokerCompanyId)
        {
            this.City = new City(cityId);
            this.Company = new Company(brokerCompanyId);
        }
        public City City { get; }

        public Company Company { get; }
    }
}
