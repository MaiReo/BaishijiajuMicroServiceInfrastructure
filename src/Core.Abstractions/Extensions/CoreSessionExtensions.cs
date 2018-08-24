namespace Core.Session.Extensions
{
    public static class CoreSessionExtensions
    {
        public static string GetCurrentUserId(this ICoreSession session) => session?.User?.Id;

        public static string GetCurrentUserName(this ICoreSession session) => session?.User?.Name;
    }
}
