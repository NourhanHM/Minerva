using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minerva.Data;
using Minerva.Models;
using System.Text;
using Newtonsoft.Json;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace Minerva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AdminController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("upload-doctors")]
        public async Task<IActionResult> UploadDoctors(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var doctors = new List<Doctor>();

            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            bool isFirstLine = true;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                // Skip the header row
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                var values = line.Split(',');

                if (values.Length < 5)
                {
                    return BadRequest("Invalid CSV format. Expected columns: Doctor_id, Name, Email, Password, Department.");
                }

                var doctor = new Doctor
                {
                    Doctor_id = int.Parse(values[0]),  // Now it only reads actual numbers
                    Name = values[1],
                    Email = values[2],
                    Password = values[3],
                    Department = values[4]
                };

                doctors.Add(doctor);
            }

            await _dbContext.Doctors.AddRangeAsync(doctors);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Doctors uploaded successfully!", Count = doctors.Count });
        }

        [HttpPost("add-or-update-doctor")]
        public async Task<IActionResult> AddOrUpdateDoctor([FromBody] Doctor doctor)
        {
            if (doctor == null)
            {
                return BadRequest("Invalid doctor data.");
            }

            var existingDoctor = await _dbContext.Doctors.FindAsync(doctor.Doctor_id);

            if (existingDoctor != null)
            {
                // Update existing doctor
                existingDoctor.Name = doctor.Name;
                existingDoctor.Email = doctor.Email;
                existingDoctor.Password = doctor.Password;
                existingDoctor.Department = doctor.Department;

                _dbContext.Doctors.Update(existingDoctor);
                await _dbContext.SaveChangesAsync();
                return Ok(new { Message = "Doctor updated successfully!", Doctor = existingDoctor });
            }
            else
            {
                // Insert new doctor
                await _dbContext.Doctors.AddAsync(doctor);
                await _dbContext.SaveChangesAsync();
                return Ok(new { Message = "Doctor added successfully!", Doctor = doctor });
            }
        }


        [HttpPost("upload-students")]
        public async Task<IActionResult> UploadStudents(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var students = new List<Student>();

            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            bool isFirstLine = true;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                // Skip the header row
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                var values = line.Split(',');

                if (values.Length < 6)  // Ensure all columns are present
                {
                    return BadRequest("Invalid CSV format. Expected: Student_id, Username, National_id, Email, Password, Major.");
                }

                var student = new Student
                {
                    Student_id = int.Parse(values[0]),
                    Username = values[1],   // If it's INT, but should it be a string?
                    National_id = int.Parse(values[2]),
                    Email = values[3],
                    Password = values[4],
                    Major = values[5]
                };

                students.Add(student);
            }

            await _dbContext.Students.AddRangeAsync(students);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Students uploaded successfully!", Count = students.Count });
        }

        [HttpPost("add-or-update-student")]
        public async Task<IActionResult> AddOrUpdateStudent([FromBody] Student student)
        {
            if (student == null)
            {
                return BadRequest("Invalid student data.");
            }

            var existingStudent = await _dbContext.Students.FindAsync(student.Student_id);

            if (existingStudent != null)
               
            {
                // Update existing student
                existingStudent.Username = student.Username;
                existingStudent.National_id = student.National_id;
                existingStudent.Email = student.Email;
                existingStudent.Password = student.Password;
                existingStudent.Major = student.Major;

                _dbContext.Students.Update(existingStudent);
                await _dbContext.SaveChangesAsync();
                return Ok(new { Message = "Student updated successfully!", Student = existingStudent });
            }
            else
            {
                // Insert new student
                await _dbContext.Students.AddAsync(student);
                await _dbContext.SaveChangesAsync();
                return Ok(new { Message = "Student added successfully!", Student = student });
            }
        }

        [HttpPost("upload-subjects")]
        public async Task<IActionResult> UploadSubjects(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid CSV file!");

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

            var subjects = csv.GetRecords<Subject>().ToList();

            foreach (var subject in subjects)
            {
                // Validate doctor exists
                var doctorExists = await _dbContext.Doctors.AnyAsync(d => d.Doctor_id == subject.Doctor_id);
                if (!doctorExists)
                    return BadRequest($"Doctor ID {subject.Doctor_id} not found!");

                // Validate students
                var studentIds = JsonConvert.DeserializeObject<List<int>>(subject.Student_ids);
                var validStudents = await _dbContext.Students
                                                    .Where(s => studentIds.Contains(s.Student_id))
                                                    .Select(s => s.Student_id)
                                                    .ToListAsync();

                if (validStudents.Count != studentIds.Count)
                    return BadRequest("One or more Student IDs are invalid!");

                // Save subject
                subject.Student_ids = JsonConvert.SerializeObject(validStudents);
                _dbContext.Subjects.Add(subject);
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { Message = "Subjects uploaded successfully!" });
        }

        [HttpPost("add-or-update-subject")]
        public async Task<IActionResult> AddOrUpdateSubject([FromBody] Subject subject)
        {
            if (subject == null)
            {
                return BadRequest("Invalid subject data.");
            }

            // Validate that the Doctor exists
            var doctorExists = await _dbContext.Doctors.AnyAsync(d => d.Doctor_id == subject.Doctor_id);
            if (!doctorExists)
            {
                return BadRequest($"Doctor ID {subject.Doctor_id} not found!");
            }

            // Validate students
            var studentIds = JsonConvert.DeserializeObject<List<int>>(subject.Student_ids);
            var validStudents = await _dbContext.Students
                                                .Where(s => studentIds.Contains(s.Student_id))
                                                .Select(s => s.Student_id)
                                                .ToListAsync();

            if (validStudents.Count != studentIds.Count)
            {
                return BadRequest("One or more Student IDs are invalid!");
            }

            var existingSubject = await _dbContext.Subjects.FindAsync(subject.Subject_id);

            if (existingSubject != null)
            {
                // Update existing subject
                existingSubject.Name = subject.Name;
                existingSubject.Description = subject.Description;
                existingSubject.Doctor_id = subject.Doctor_id;
                existingSubject.Student_ids = JsonConvert.SerializeObject(validStudents);

                _dbContext.Subjects.Update(existingSubject);
                await _dbContext.SaveChangesAsync();
                return Ok(new { Message = "Subject updated successfully!", Subject = existingSubject });
            }
            else
            {
                // Insert new subject
                subject.Student_ids = JsonConvert.SerializeObject(validStudents);
                await _dbContext.Subjects.AddAsync(subject);
                await _dbContext.SaveChangesAsync();
                return Ok(new { Message = "Subject added successfully!", Subject = subject });
            }
        }

        [HttpGet("get-students")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _dbContext.Students.ToListAsync();
            return Ok(students);
        }

        [HttpGet("get-doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _dbContext.Doctors.ToListAsync();
            return Ok(doctors);
        }

        [HttpGet("get-subjects")]
        public async Task<IActionResult> GetSubjects()
        {
            var subjects = await _dbContext.Subjects.ToListAsync();
            return Ok(subjects);
        }


        // Unique Route for Admin Dashboard
        [HttpGet("dashboard")] // Ensure this route is unique
        public IActionResult GetAdminDashboard()
        {
            var universities = _dbContext.Universities.ToList();

            return Ok(new
            {
                Universities = _dbContext.Universities.ToList(),  // Now correctly mapped to UniversityTb
                TotalSubjects = _dbContext.Subjects.Count(),
                TotalDoctors = _dbContext.Doctors.Count(),
                TotalStudents = _dbContext.Students.Count()
            });
        }

        // Additional Route Example
        [HttpGet("details")]
        public IActionResult GetAdminDetails()
        {
            var admins = _dbContext.Admins.ToList();
            return Ok(admins);
        }
        [HttpGet("check-db")]
        public IActionResult CheckDatabaseConnection()
        {
            var databaseName = _dbContext.Database.GetDbConnection().Database;
            return Ok($"Connected to Database: {databaseName}");
        }

    }
}
