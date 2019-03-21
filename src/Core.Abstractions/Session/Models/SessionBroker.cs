namespace Core.Session
{
    [System.Obsolete]
    internal class SessionBroker : CoreSessionContainer<string>
    {
        public SessionBroker()
        {
        }

        public SessionBroker(string id, string name = default) : base(id, name)
        {
            
        }
    }
}
