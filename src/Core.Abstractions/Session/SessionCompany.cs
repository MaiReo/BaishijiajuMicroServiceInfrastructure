using System;

namespace Core.Session
{
    public class SessionCompany
    {
        public SessionCompany()
        {
        }

        public SessionCompany(Guid? id, string name = default) : this()
        {
            Id = id;
            Name = name;
        }

        public Guid? Id { get; protected set; }

        public string Name { get; protected set; }
    }
}
