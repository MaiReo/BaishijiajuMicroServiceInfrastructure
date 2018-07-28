using Core.PersistentStore;
using Core.Session;
using Microsoft.EntityFrameworkCore;

namespace Core.Abstractions.Tests
{
    public class TestDbContext : CorePersistentStoreDbContext
    {
        public virtual DbSet<TestEntityOne> TestEntityOnes { get; set; }

        public virtual DbSet<TestEntityTwo> TestEntityTwos { get; set; }

        public virtual DbSet<TestEntityHasCity> TestEntityHasCities { get; set; }

        public TestDbContext(DbContextOptions options, ICoreSession session) : base(options, session)
        {
        }
    }

    public class TestEntityHasCity : Entity, IHasCity
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
}