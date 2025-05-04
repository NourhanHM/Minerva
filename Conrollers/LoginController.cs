using Microsoft.AspNetCore.Mvc;
using Minerva.Data;
using Minerva.Models;
using System.Linq;

namespace Minerva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private const string V = "Doctor Login Successful";
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null)
                return BadRequest("Invalid login request.");

            // Extract the first digit of User_id to determine user type
            char userType = request.User_id.ToString()[0];

            switch (userType)
            {
                case '1': // Student
                    var student = _context.Students
                        .FirstOrDefault(s => s.Student_id == request.User_id && s.Password == request.Password);
                    if (student != null)
                        return Ok(new { Message = "Student Login Successful", UserType = "Student", student });
                    break;

                case '4': // Doctor
                    var doctor = _context.Doctors
                        .FirstOrDefault(d => d.Doctor_id == request.User_id && d.Password == request.Password);
                    if (doctor != null)
                        return Ok(new { Message = V, UserType = "Doctor", doctor });
                    break;

                case '8': // Admin
                    var admin = _context.Admins
                        .FirstOrDefault(a => a.Admin_id == request.User_id && a.Password == request.Password );
                    if (admin != null)
                        return Ok(new { Message = "Admin Login Successful", UserType = "Admin", admin });
                    break;

                default:
                    return BadRequest("Invalid user type. Please check your User ID.");
            }

            return Unauthorized("Invalid credentials. Please try again.");
        }
    }
}
