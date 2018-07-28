using Autofac;
using Core.Abstractions.TestBase;
using Core.PersistentStore.Repositories;
using Core.Web.Tests;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Core.Abstractions.Tests
{
    public class RepositoryTests : AbstractionTestBase<RepositoryTests, TestDbContext>
    {
        private readonly IAsyncRepository<TestEntityOne, int> _testEntityOneRepository;
        private readonly IAsyncRepository<TestEntityTwo, int> _testEntityTwoRepository;

        public RepositoryTests()
        {
            this._testEntityOneRepository = Resolve<IAsyncRepository<TestEntityOne, int>>();
            this._testEntityTwoRepository = Resolve<IAsyncRepository<TestEntityTwo, int>>();
        }

        protected override void RegisterDependency(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(TestRepository<,>))
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();
        }

        [Fact]
        public void DbContextTest()
        {
            var dbContext1 = _testEntityOneRepository.GetDbContext();
            dbContext1.ShouldBeOfType<TestDbContext>();
            var dbContext2 = _testEntityTwoRepository.GetDbContext();
            dbContext2.ShouldBeOfType<TestDbContext>();

            object.ReferenceEquals(dbContext1, dbContext2).ShouldBeTrue();
        }

        [Fact]
        public async Task DbContextDifferentRepositoryTest()
        {
            var testEntityTwo = new TestEntityTwo();
            await _testEntityTwoRepository.InsertAsync(testEntityTwo);

            var testEntityOne = new TestEntityOne { TestEntityTwoId = testEntityTwo.Id };
            await _testEntityOneRepository.InsertAsync(testEntityOne);

            var entityTwoIds = _testEntityTwoRepository.Query(q => q.Select(x => x.Id));
            var entityOnes = _testEntityTwoRepository.Query(q => q.Where(x => entityTwoIds.Any(id => id == x.Id)));

            var list = entityOnes.ToList();
            list.ShouldNotBeEmpty();
        }


    }

}
