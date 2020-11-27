using System;
using System.Collections.Generic;
using System.Text;

namespace DevPlanner
{
    public class NewApi
    {
        public string name { get; set; }
        public string path { get; set; }
        public string id { get; set; }

        public NewApi(string n, string p, string i)
        {
            name = n;
            path = p;
            id = i;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
