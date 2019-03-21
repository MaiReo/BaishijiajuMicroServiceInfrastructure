using Abp.Dependency;
using Core.Messages;
using Core.Session;
using Shouldly;
using System;

namespace Core.Abstractions.Tests.Messages
{
    public class TestMessage2 : IMessage
    {
        public string TestTitle2 { get; set; }
    }

    public interface ITestServiceForTestMessage2
    {

    }
    public class TestServiceForTestMessage2 : ITestServiceForTestMessage2, ISingletonDependency
    {
        private readonly ICoreSessionProvider _coreSessionProvider;

        public TestServiceForTestMessage2(ICoreSessionProvider coreSessionProvider)
        {
            _coreSessionProvider = coreSessionProvider;
        }
    }

    public class TestMessage2Handler : IMessageHandler<TestMessage2>, ITransientDependency
    {
        private readonly ICoreSession _session;
        private readonly ICoreSessionProvider _coreSessionProvider;
        private readonly ITestServiceForTestMessage2 _testServiceForTestMessage2;

        public TestMessage2Handler(ICoreSession session,
            ICoreSessionProvider coreSessionProvider,
            ITestServiceForTestMessage2 testServiceForTestMessage2)
        {
            _session = session;
            _coreSessionProvider = coreSessionProvider;
            _testServiceForTestMessage2 = testServiceForTestMessage2;
        }
        public void HandleMessage(TestMessage2 message)
        {
            _coreSessionProvider.ShouldNotBeOfType<UnitTestCoreSessionProvider>();

            _session.City.ShouldNotBeNull();
            _session.City.Id.ShouldBe(TestConsts.CITY_ID);

            _session.Company.ShouldNotBeNull();
            _session.Company.Id.ShouldNotBeNull();
            _session.Company.Id.Value.ShouldBe(Guid.Parse(TestConsts.COMPANY_ID));
            _session.Company.Name.ShouldNotBeNullOrWhiteSpace();
            _session.Company.Name.ShouldBe(TestConsts.COMPANY_NAME);

            _session.Organization.ShouldNotBeNull();
            _session.Organization.Store.ShouldNotBeNull();
            _session.Organization.Store.Id.ShouldNotBeNull();
            _session.Organization.Store.Id.Value.ShouldBe(Guid.Parse(TestConsts.STORE_ID));
            _session.Organization.Store.Name.ShouldNotBeNullOrWhiteSpace();
            _session.Organization.Store.Name.ShouldBe(TestConsts.STORE_NAME);

            _session.Broker.ShouldNotBeNull();
            _session.Broker.Id.ShouldNotBeNullOrWhiteSpace();
            _session.Broker.Id.ShouldBe(TestConsts.BROKER_ID);
            _session.Broker.Name.ShouldNotBeNullOrWhiteSpace();
            _session.Broker.Name.ShouldBe(TestConsts.BROKER_NAME);

           

            _session.User.ShouldNotBeNull();
            _session.User.Id.ShouldNotBeNullOrWhiteSpace();
            _session.User.Id.ShouldBe(TestConsts.CURRENT_USER_ID);
            _session.User.Name.ShouldNotBeNullOrWhiteSpace();
            _session.User.Name.ShouldBe(TestConsts.CURRENT_USER_NAME);
        }
    }
}
