using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Session
{
    [Obsolete]
    internal class SessionCity : CoreSessionContainer<string>
    {
        public SessionCity()
        {
        }

        public SessionCity(string id) : base(id)
        {
        }
    }
}
