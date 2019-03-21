using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Session
{
    internal class SessionStore : CoreSessionContainer<Guid?>
    {
        public SessionStore()
        {
        }
        public SessionStore(Guid? id, string name = default) : base(id, name)
        {
        }
    }
}
