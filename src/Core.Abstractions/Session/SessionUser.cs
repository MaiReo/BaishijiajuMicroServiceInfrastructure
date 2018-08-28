namespace Core.Session
{
    public class SessionUser
    {
        public SessionUser()
        {
        }

        public SessionUser(string id, string name = default) : this()
        {
            Id = id;
            Name = name;
        }
        public string Id { get; protected set; }

        public string Name { get; protected set; }
    }
}