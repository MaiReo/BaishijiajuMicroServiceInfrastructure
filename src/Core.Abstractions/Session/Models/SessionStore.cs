using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Session
{
    public class SessionStore
    {
        public SessionStore()
        {
        }
        public SessionStore(Guid? id, string name = default) : this()
        {
            this.Id = id;
            this.Name = name;
        }

        public Guid? Id { get; }
        public string Name { get; }
    }
}
