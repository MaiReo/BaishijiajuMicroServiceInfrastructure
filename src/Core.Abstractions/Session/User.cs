namespace Core.Session
{
    public class User
    {
        public User(string id, string name)
        {
            Id = id;
            Name = name;
        }
        public string Id { get; protected set; }

        public string Name { get; protected set; }
    }
}