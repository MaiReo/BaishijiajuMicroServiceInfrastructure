using Core.Abstractions.Tests.Messages;
using Core.Messages;
using Core.Messages.Bus;
using Core.Session;
using Core.Session.Providers;
using Shouldly;
using System;
using Xunit;
using static System.Web.HttpUtility;

namespace Core.Abstractions.Tests
{
    public class ScopeCreatorTests : AbstractionTestBase
    {
        private readonly IMessageScopeCreator _messageScopeCreator;

        public ScopeCreatorTests()
        {
            _messageScopeCreator = Resolve<IMessageScopeCreator>();
        }

        [Fact]
        public void MessageScopeTest()
        {
            //Arrange
            var message = new TestMessage
            {
                TestTitle = "TEST"
            };
            var descriptor = new RichMessageDescriptor("", nameof(TestMessage).ToLowerInvariant());
            descriptor.Headers.Add(SessionConsts.CityId, TestConsts.CITY_ID);

            descriptor.Headers.Add(SessionConsts.CompanyId, TestConsts.COMPANY_ID);
            descriptor.Headers.Add(SessionConsts.CompanyName, UrlEncode(TestConsts.COMPANY_NAME));

            descriptor.Headers.Add(SessionConsts.StoreId, TestConsts.STORE_ID);
            descriptor.Headers.Add(SessionConsts.StoreName, UrlEncode(TestConsts.STORE_NAME));

            descriptor.Headers.Add(SessionConsts.BrokerId, TestConsts.BROKER_ID);
            descriptor.Headers.Add(SessionConsts.BrokerName, UrlEncode(TestConsts.BROKER_NAME));

            descriptor.Headers.Add(SessionConsts.OrganizationId, TestConsts.ORGANIZATION_ID);
            descriptor.Headers.Add(SessionConsts.OrganizationName, UrlEncode(TestConsts.ORGANIZATION_NAME));

            descriptor.Headers.Add(SessionConsts.CurrentUserId, TestConsts.CURRENT_USER_ID);
            descriptor.Headers.Add(SessionConsts.CurrentUserName, UrlEncode(TestConsts.CURRENT_USER_NAME));

            //Action
            var scope = _messageScopeCreator.CreateScope(message, descriptor);

            var coreSessionProvider = (ICoreSessionProvider)scope.Resolve(typeof(ICoreSessionProvider));

            //Assert
            coreSessionProvider.ShouldNotBeNull();

            Resolve<ICoreSessionProvider>().ShouldNotBeOfType<MessageCoreSessionProvider>();

            coreSessionProvider.ShouldBeOfType<MessageCoreSessionProvider>();
            (coreSessionProvider as MessageCoreSessionProvider).MessageDescriptor.ShouldBe(descriptor);

            var session = coreSessionProvider.Session;

            session.City.ShouldNotBeNull();
            session.City.Id.ShouldBe(TestConsts.CITY_ID);

            session.Company.ShouldNotBeNull();
            session.Company.Id.ShouldNotBeNull();
            session.Company.Id.Value.ShouldBe(Guid.Parse(TestConsts.COMPANY_ID));
            session.Company.Name.ShouldNotBeNullOrWhiteSpace();
            session.Company.Name.ShouldBe(TestConsts.COMPANY_NAME);

            session.Store.ShouldNotBeNull();
            session.Store.Id.ShouldNotBeNull();
            session.Store.Id.Value.ShouldBe(Guid.Parse(TestConsts.STORE_ID));
            session.Store.Name.ShouldNotBeNullOrWhiteSpace();
            session.Store.Name.ShouldBe(TestConsts.STORE_NAME);

            session.Broker.ShouldNotBeNull();
            session.Broker.Id.ShouldNotBeNullOrWhiteSpace();
            session.Broker.Id.ShouldBe(TestConsts.BROKER_ID);
            session.Broker.Name.ShouldNotBeNullOrWhiteSpace();
            session.Broker.Name.ShouldBe(TestConsts.BROKER_NAME);

            session.Organization.ShouldNotBeNull();
            session.Organization.Id.ShouldNotBeNullOrWhiteSpace();
            session.Organization.Id.ShouldBe(TestConsts.ORGANIZATION_ID);
            session.Organization.Name.ShouldNotBeNullOrWhiteSpace();
            session.Organization.Name.ShouldBe(TestConsts.ORGANIZATION_NAME);

            session.User.ShouldNotBeNull();
            session.User.Id.ShouldNotBeNullOrWhiteSpace();
            session.User.Id.ShouldBe(TestConsts.CURRENT_USER_ID);
            session.User.Name.ShouldNotBeNullOrWhiteSpace();
            session.User.Name.ShouldBe(TestConsts.CURRENT_USER_NAME);

            scope.Dispose();

            Resolve<ICoreSessionProvider>().ShouldNotBeOfType<MessageCoreSessionProvider>();

            try
            {
                scope.Resolve(typeof(ICoreSessionProvider));
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            throw new InvalidOperationException("Code must not reach here");
        }
    }
}
