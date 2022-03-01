using System;
using System.Collections.Generic;
using System.Text;

namespace MyClient.Entity
{
    public class Blog
    {
        public int Id { get;}
        public string Title { get; private set; }
        public string Url { get; private set; }

        public Blog(string title, string url)
        {
            Title = title;
            Url = url;
        }
    }
}
