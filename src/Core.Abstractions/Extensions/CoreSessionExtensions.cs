using Core.Extensions;
using System.Collections.Generic;

namespace Core.Session.Extensions
{
    public static class CoreSessionExtensions
    {
        public static string GetCurrentUserId(this ICoreSession session) => session?.User?.Id;

        public static string GetCurrentUserName(this ICoreSession session) => session?.User?.Name;


        public static IDictionary<string, string> ToHeaders(this ICoreSession session)
        {
            if (session is null)
            {
                return null;
            }
            var dictionary = new Dictionary<string, string>
            {
                { SessionConsts.CityId, session?.City?.Id },

                { SessionConsts.CompanyId, session?.Company?.Id?.AsStringOrDefault() },
                { SessionConsts.CompanyName, session?.Company?.Name },

                { SessionConsts.StoreId, session?.Store?.Id?.AsStringOrDefault() },
                { SessionConsts.StoreName, session?.Store?.Name },

                { SessionConsts.BrokerId, session?.Broker?.Id },
                { SessionConsts.BrokerName, session?.Broker?.Name },

                { SessionConsts.OrganizationId, session?.Organization?.Id },
                { SessionConsts.OrganizationName, session?.Organization?.Name },
                { SessionConsts.CurrentUserId, session?.User?.Id },
                { SessionConsts.CurrentUserName, session?.User?.Name }
            };

            return dictionary;
        }

        public static ICoreSession ToSession(this IDictionary<string, string> dictionary)
        {
            if (dictionary is null)
            {
                return null;
            }
            dictionary.TryGetValue(SessionConsts.CityId, out var cityId);
            dictionary.TryGetValue(SessionConsts.CompanyId, out var companyId);
            dictionary.TryGetValue(SessionConsts.CompanyName, out var companyName);
            dictionary.TryGetValue(SessionConsts.StoreId, out var storeId);
            dictionary.TryGetValue(SessionConsts.StoreName, out var storeName);
            dictionary.TryGetValue(SessionConsts.BrokerId, out var brokerId);
            dictionary.TryGetValue(SessionConsts.BrokerName, out var brokerName);

            dictionary.TryGetValue(SessionConsts.OrganizationId, out var organizationId);
            dictionary.TryGetValue(SessionConsts.OrganizationName, out var organizationName);
            dictionary.TryGetValue(SessionConsts.CurrentUserId, out var currentUserId);
            dictionary.TryGetValue(SessionConsts.CurrentUserName, out var currentUserName);

            var session = new CoreSession(
                cityId,
                companyId?.AsGuidOrNull(), companyName,
                storeId?.AsGuidOrNull(), storeName,
                brokerId, brokerName,
                organizationId, organizationName,
                currentUserId, currentUserName);

            return session;
        }
    }
}
