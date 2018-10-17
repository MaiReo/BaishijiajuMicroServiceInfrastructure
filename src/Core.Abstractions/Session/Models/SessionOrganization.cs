using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Session
{
    public class SessionOrganization
    {
        public SessionOrganization()
        {

        }

        public SessionOrganization(string id, string name = default):this()
        {
            this.Id = id;
            this.Name = name;
        }

        public string Id { get; protected set; }
        public string Name { get; protected set; }
    }
}
