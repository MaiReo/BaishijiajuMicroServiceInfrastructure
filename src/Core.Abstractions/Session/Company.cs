using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Session
{
    public class Company
    {
        public Company()
        {
        }

        public Company(Guid? id) : this()
        {
            this.Id = id;
        }

        public Guid? Id { get; }
    }
}
