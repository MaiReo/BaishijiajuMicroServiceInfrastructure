namespace Core.Messages
{
    public interface IMessageWithSession : IMessage
    {
        string CityId { get; }

        string CompanyId { get; }

        string CompanyName { get; }

        string StoreId { get; }

        string StoreName { get; }

        string BrokerId { get; }

        string BrokerName { get; }

        string OrganizationId { get; }

        string OrganizationName { get; }

        string CurrentUserId { get; }

        string CurrentUserName { get; }
    }
}
