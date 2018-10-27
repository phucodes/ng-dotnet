using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NgTemplate.Models
{
    public class Benefit
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string BenefitName { get; set; }
        public string JobName { get; set; }
    }
}
