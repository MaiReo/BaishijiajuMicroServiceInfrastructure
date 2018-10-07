using Core.Extensions;
using Core.Messages;

namespace Core.Session.Providers
{
    public class MessageCoreSessionProvider : ICoreSessionProvider
    {
        private readonly IMessage _message;
        public IRichMessageDescriptor MessageDescriptor { get; }

        public MessageCoreSessionProvider(IMessage message, IRichMessageDescriptor messageDescriptor)
        {
            _message = message;
            MessageDescriptor = messageDescriptor;
        }

        public ICoreSession Session => GetSession();

        private ICoreSession GetSession()
        {
            if (_message is IMessageWithSession withSessionMessage)
            {
                return ParseSession(withSessionMessage);
            }
            return ParseSession(MessageDescriptor);
        }

        private ICoreSession ParseSession(IRichMessageDescriptor messageDescriptor)
        {

            if (messageDescriptor.Headers is null)
            {
                return null;
            }
            messageDescriptor.Headers.TryGetValue(SessionConsts.CityId, out var city);
            messageDescriptor.Headers.TryGetValue(SessionConsts.CompanyId, out var companyId);
            messageDescriptor.Headers.TryGetValue(SessionConsts.CompanyName, out var companyName);
            messageDescriptor.Headers.TryGetValue(SessionConsts.StoreId, out var storeId);
            messageDescriptor.Headers.TryGetValue(SessionConsts.StoreName, out var storeName);
            messageDescriptor.Headers.TryGetValue(SessionConsts.BrokerId, out var brokerId);
            messageDescriptor.Headers.TryGetValue(SessionConsts.BrokerName, out var brokerName);
            messageDescriptor.Headers.TryGetValue(SessionConsts.OrganizationId, out var organizationId);
            messageDescriptor.Headers.TryGetValue(SessionConsts.OrganizationName, out var organizationName);
            messageDescriptor.Headers.TryGetValue(SessionConsts.CurrentUserId, out var currentUserId);
            messageDescriptor.Headers.TryGetValue(SessionConsts.CurrentUserName, out var currentUserName);
            var session = new CoreSession((city as string),
                (companyId as string).AsGuidOrNull(),
                TryUriDecode(companyName as string),
                (storeId as string).AsGuidOrNull(),
                TryUriDecode(storeName as string),
                brokerId as string,
                TryUriDecode(brokerName as string),
                organizationId as string,
                TryUriDecode(organizationName as string),
                currentUserId as string,
                TryUriDecode(currentUserName as string));

            return session;
        }

        private string TryUriDecode(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            try
            {
                return System.Web.HttpUtility.UrlDecode(str);
            }
            catch (System.Exception)
            {
                
            }
            return null;
        }


        private ICoreSession ParseSession(IMessageWithSession withSessionMessage)
        {

            var session = new CoreSession(withSessionMessage.CityId,
                withSessionMessage.CompanyId?.AsGuidOrNull(), string.IsNullOrWhiteSpace(withSessionMessage.CompanyName) ? null : System.Web.HttpUtility.UrlDecode(withSessionMessage.CompanyName),
                withSessionMessage.StoreId?.AsGuidOrNull(), string.IsNullOrWhiteSpace(withSessionMessage.StoreName) ? null : System.Web.HttpUtility.UrlDecode(withSessionMessage.StoreName),
                withSessionMessage.BrokerId, string.IsNullOrWhiteSpace(withSessionMessage.BrokerName) ? null : System.Web.HttpUtility.UrlDecode(withSessionMessage.BrokerName),
                withSessionMessage.OrganizationId, string.IsNullOrWhiteSpace(withSessionMessage.OrganizationName) ? null : System.Web.HttpUtility.UrlDecode(withSessionMessage.OrganizationName),
                withSessionMessage.CurrentUserId, string.IsNullOrWhiteSpace(withSessionMessage.CurrentUserName) ? null : System.Web.HttpUtility.UrlDecode(withSessionMessage.CurrentUserName));
            return session;
        }
    }
}
