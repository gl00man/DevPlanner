using System;
using System.Collections.Generic;
using System.Text;

namespace DevPlanner
{
    public class NewProject
    {
        public string title { get; set; }
        public string deadline { get; set; }
        public string privacy { get; set; }
        public string description { get; set; }

        public NewProject(string t, string dea, string p, string des)
        {
            title = t;
            deadline = dea;
            privacy = p;
            description = des;
        }

        public override string ToString()
        {
            return title;
        }

    }
}
