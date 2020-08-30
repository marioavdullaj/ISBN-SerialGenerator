using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSerials.Models
{
    class Country
    {
        public int Id { get; set; }
        public string Iso { get; set; }
        public string Name { get; set; }

        public Country(int id, string iso, string name)
        {
            Id = id; Iso = iso; Name = name;
        }
    }
}
