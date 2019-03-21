namespace Core.Session
{

    [System.Obsolete]
    internal class SessionUser : CoreSessionContainer<string>
    {
        public SessionUser()
        {
        }

        public SessionUser(string id, string name = default) : base(id, name)
        {
        }
    }
}