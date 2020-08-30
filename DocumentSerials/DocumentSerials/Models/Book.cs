using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSerials
{
    class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public Book(int id, string title, string description)
        {
            Id = id;
            Title = title;
            Description = description;
        }
    }
}
