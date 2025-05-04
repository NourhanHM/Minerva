using Microsoft.AspNetCore.Mvc;
using Minerva.Data;
using Minerva.Models;

namespace Minerva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubjectController(AppDbContext context)
        {
            _context = context;
        }

        // Example: Get All Subjects
        [HttpGet]
        public IActionResult GetAllSubjects()
        {
            var subjects = _context.Subjects.ToList();
            return Ok(subjects);
        }

        // Example: Add a New Subject
        [HttpPost("add")]
        public IActionResult AddSubject([FromBody] Subject subject)
        {
            if (subject == null)
                return BadRequest("Invalid subject data.");

            _context.Subjects.Add(subject);
            _context.SaveChanges();

            return Ok("Subject added successfully.");
        }

        // Example: Modify Subject by ID
        [HttpPut("modify/{id}")]
        public IActionResult ModifySubject(int id, [FromBody] Subject updatedSubject)
        {
            var subject = _context.Subjects.FirstOrDefault(s => s.Subject_id == id);

            if (subject == null)
                return NotFound($"Subject with ID {id} not found.");

            subject.Name = updatedSubject.Name;
            subject.Doctor_id = updatedSubject.Doctor_id;
         //   subject.Student_id = updatedSubject.Student_id;
            subject.Description = updatedSubject.Description;

            _context.SaveChanges();

            return Ok("Subject updated successfully.");
        }

        // Example: Delete Subject by ID
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteSubject(int id)
        {
            var subject = _context.Subjects.FirstOrDefault(s => s.Subject_id == id);

            if (subject == null)
                return NotFound($"Subject with ID {id} not found.");

            _context.Subjects.Remove(subject);
            _context.SaveChanges();

            return Ok("Subject deleted successfully.");
        }
    }
}
