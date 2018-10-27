using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NgTemplate.Models
{
    public class Requirement
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string RequirementName { get; set; }
        public string JobName { get; set; }
    }
}
