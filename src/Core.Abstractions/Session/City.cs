using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Session
{
    public class City
    {
        public City()
        {
        }

        public City(string id) : this()
        {
            this.Id = id;
        }
        public string Id { get; }
    }
}
