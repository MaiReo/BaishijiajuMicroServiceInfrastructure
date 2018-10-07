namespace Core.Session
{
    public class SessionBroker
    {
        public SessionBroker()
        {
        }

        public SessionBroker(string id, string name = default) : this()
        {
            Id = id;
            Name = name;
        }

        public string Id { get; protected set; }
        public string Name { get; protected set; }
    }
}
