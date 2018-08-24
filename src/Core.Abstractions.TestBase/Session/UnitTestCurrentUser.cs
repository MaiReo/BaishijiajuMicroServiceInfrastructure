using Core.Session;

namespace Core.TestBase
{
    public class UnitTestCurrentUser : User
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

        public static UnitTestCurrentUser Current { get; }

        static UnitTestCurrentUser()
        {
            Current = new UnitTestCurrentUser();
        }
    }
}
