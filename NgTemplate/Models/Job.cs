using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NgTemplate.Models
{
    public class Job
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string JobName { get; set; }

        [Required]
        public string Location { get; set; }

        public string Employer { get; set; }

        [Required]
        public string DatePost { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
        // public IList<Requirement> Requirements { get; set; }
    }
}
