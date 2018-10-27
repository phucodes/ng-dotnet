using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NgTemplate.Data;
using NgTemplate.Models;

namespace NgTemplate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly JobDbContext _context;

        public JobController(JobDbContext context)
        {
            _context = context;
        }

        // GET: api/Job
        [HttpGet]
        public IEnumerable<Job> GetJobs()
        {
            return _context.Jobs;
        }

        // GET: api/Job/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var job = await _context.Jobs.FindAsync(id);

            List<Requirement> requirements = _context.Requirements.Where(r => r.JobId == id).ToList();

            List<Benefit> benefits = _context.Benefits.Where(b => b.JobId == id).ToList();

            if (job == null)
            {
                return NotFound();
            }

            return Ok(new { job, requirements, benefits});
        }

        // PUT: api/Job/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJob([FromRoute] int id, [FromBody] Job job)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != job.Id)
            {
                return BadRequest();
            }

            _context.Entry(job).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Job
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PostJob([FromBody] JObject job)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = HttpContext.User.Identity.Name;

            Job postJob = new Job
            {
                JobName = job.SelectToken("jobName").ToString(),
                Location = job.SelectToken("location").ToString(),
                DatePost = DateTime.UtcNow.ToString("MMMM dd, yyyy"),
                Description = job.SelectToken("description").ToString(),
                Employer = currentUser
            };

            _context.Jobs.Add(postJob);
            await _context.SaveChangesAsync();

            foreach(var item in job.SelectToken("requirements").ToList())
            {
                Debug.WriteLine(item);
                Requirement newRequirement = new Requirement
                {
                    RequirementName = item.ToString(),
                    JobId = postJob.Id,
                    JobName = postJob.JobName
                };

                _context.Requirements.Add(newRequirement);
                await _context.SaveChangesAsync(); 
            }

            foreach (var item in job.SelectToken("benefits").ToList())
            {
                Benefit newBenefit = new Benefit
                {
                    BenefitName = item.ToString(),
                    JobId = postJob.Id,
                    JobName = postJob.JobName
                };

                _context.Benefits.Add(newBenefit);
                await _context.SaveChangesAsync();
            }


            return CreatedAtAction("GetJob", /* new { id = job.Id } , */ job);
        }

        // DELETE: api/Job/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return Ok(job);
        }

        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }
    }
}