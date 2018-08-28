using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Session
{
    public class SessionCity
    {
        public SessionCity()
        {
        }

        public SessionCity(string id) : this()
        {
            this.Id = id;
        }
        public string Id { get; protected set; }
    }
}
