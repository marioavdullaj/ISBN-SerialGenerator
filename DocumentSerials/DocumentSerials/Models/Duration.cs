using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSerials.Models
{
    class Duration
    {
        public int Id { get; set; }
        public string Description { get; set; }
        
        public Duration(int id, string description)
        {
            Id = id; Description = description;
        }
    }
}
