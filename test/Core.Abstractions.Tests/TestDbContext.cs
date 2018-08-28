using System;
using Core.PersistentStore;
using Core.PersistentStore.Auditing;
using Core.Session;
using Microsoft.EntityFrameworkCore;

namespace Core.Abstractions.Tests
{
    public class TestDbContext : CorePersistentStoreDbContext
    {
        public virtual DbSet<TestEntityOne> TestEntityOnes { get; set; }

        public virtual DbSet<TestEntityTwo> TestEntityTwos { get; set; }

        public virtual DbSet<TestEntityHasCity> TestEntityHasCities { get; set; }


        public virtual DbSet<TestEntityHasCompany> TestEntityHasCompanies { get; set; }

        public virtual DbSet<TestEntityFullAudited> TestEntityFullAuditeds { get; set; }

        public TestDbContext(DbContextOptions options) : base(options)
        {
        }
    }

    

    public class TestEntityHasCity : Entity, IMayHaveCity
    {
        public string CityId { get; set; }

        public string Name { get; set; }
    }

    public class TestEntityOne : Entity
    {
        public int TestEntityTwoId { get; set; }
    }

    public class TestEntityTwo : Entity
    {

    }

    public class TestEntityHasCompany : Entity, IMayHaveCompany, IMayHaveCity
    {
        public string Name { get; set; }

        public string CityId { get; set; }
        
        public Guid? CompanyId { get; set; }

        public string CompanyName { get; set; }
    }

    public class TestEntityFullAudited : FullAuditedEntity, IMayHaveCompany, IMayHaveCity
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public Guid? CompanyId { get; set; }

        public string CompanyName { get; set; }

        public string CityId { get; set; }
    }
}