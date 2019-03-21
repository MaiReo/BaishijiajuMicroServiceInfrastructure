using Core.Session;

namespace Core.Abstractions.Tests
{
    public class UnitTestCurrentUser : ICoreSessionContainer<string, string>
    {
        public UnitTestCurrentUser()
        {

        }
        public string Id { get; private set; }
        public string Name { get; private set; }

        public void Set(string id, string name)
        {
            lock (this)
            {
                Id = id;
                Name = name;
            }
        }

    }
}
