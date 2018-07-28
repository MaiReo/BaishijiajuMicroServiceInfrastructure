using Core.Abstractions.TestBase;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Core.Abstractions.Tests
{
    public class CityFilterTests : AbstractionTestBase<RepositoryTests, TestDbContext>
    {
        [Fact(DisplayName = "城市过滤器")]
        public void CityId_Filter_Tests()
        {
            //Assert
            UsingDbContext(TestConsts.CITY_ID, context =>
            {
                context.TestEntityHasCities.ShouldBeEmpty();
            });
            UsingDbContext(TestConsts.CITY_ID2, context =>
            {
                context.TestEntityHasCities.ShouldBeEmpty();
            });

            //Act
            UsingDbContext(TestConsts.CITY_ID, context =>
            {
                context.TestEntityHasCities.Add(new TestEntityHasCity
                {
                    Name = "Test1",
                });
            });
            UsingDbContext(TestConsts.CITY_ID2, context =>
            {
                context.TestEntityHasCities.Add(new TestEntityHasCity
                {
                    Name = "Test2",
                });
            });

            //Assert
            UsingDbContext(TestConsts.CITY_ID, context =>
            {
                context.TestEntityHasCities.Single().Name.ShouldBe("Test1");
            });

            UsingDbContext(TestConsts.CITY_ID2, context =>
            {
                context.TestEntityHasCities.Single().Name.ShouldBe("Test2");
            });
            UsingDbContext(TestConsts.ANY_CITY, context =>
            {
                context.TestEntityHasCities.Count().ShouldBe(2);
            });
        }
    }
}
