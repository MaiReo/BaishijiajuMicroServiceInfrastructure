using Core.Session;

namespace Core.Abstractions.Tests
{
    public class UnitTestCurrentUser : SessionUser
    {
        public UnitTestCurrentUser() : base(null, null)
        {

        }

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
