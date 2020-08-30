using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSerials.Models
{
    class Code
    {
        public string Actcode { get; set; }
        public string Country { get; set; }
        public string Book { get; set; }
        public string Duration { get; set; }

        public Code(string code, string country, string book, string duration)
        {
            Actcode = code;
            Country = country;
            Book = book;
            Duration = duration;
        }
    }
}
