using Microsoft.AspNetCore.Mvc;
using Minerva.Data;
using Minerva.Models;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Minerva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class DoctorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("{doctorId}/profile")]
        public IActionResult GetDoctorProfile(int doctorId)
        {
            try
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.Doctor_id == doctorId);

                if (doctor == null)
                {
                    return NotFound("Doctor not found.");
                }

                // Get the subject IDs associated with this doctor
                var subjectIds = _context.Subjects
                    .Where(s => s.Doctor_id == doctorId)
                    .Select(s => s.Subject_id)
                    .ToList();

                var profile = new
                {
                    doctorId = doctor.Doctor_id,
                    username = doctor.Name,  // Assuming you have a Username
                    name = doctor.Name,        // and Name property.
                    email = doctor.Email,
                    department = doctor.Department,
                    subjectIds = subjectIds
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while retrieving the doctor profile.");
            }
        }

        [HttpGet("subjects/{doctorId}")]
        public IActionResult GetSubjectsForDoctor(int doctorId)
        {
            var subjects = _context.Subjects
                .Where(s => s.Doctor_id == doctorId)
                .Select(s => new
                {
                    s.Subject_id,  // ✅ Only return ID
                    s.Name,
                    s.Description
                    // ✅ Only return Name
                })
                .ToList();

            if (!subjects.Any())
                return NotFound("No subjects found for this doctor.");

            return Ok(subjects);
        }

        // Add these methods to your DoctorController class

        // DELETE: api/Doctor/lectures/{lectureId}
        [HttpDelete("lectures/{lectureId}")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            var lecture = await _context.Lectures.FindAsync(lectureId);

            if (lecture == null)
            {
                return NotFound($"Lecture with ID {lectureId} not found.");
            }

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content for successful deletion
        }

        // DELETE: api/Doctor/assignments/{assignmentId}
        [HttpDelete("assignments/{assignmentId}")]
        public async Task<IActionResult> DeleteAssignment(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Attempts) // Include related attempts for cascade deletion
                .FirstOrDefaultAsync(a => a.Assignment_id == assignmentId);

            if (assignment == null)
            {
                return NotFound($"Assignment with ID {assignmentId} not found.");
            }

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content for successful deletion
        }
        [HttpGet("subjects/{subjectId}/lectures")]
        public IActionResult GetLecturesForSubject(int subjectId)
        {
            var lectures = _context.Lectures
                .Where(l => l.Subject_id == subjectId)
                .Select(l => new
                {
                    l.Lec_id,
                    l.Name,
                    l.Description,
                    l.Upload_Date,
                    FileUrl = $"http://localhost:61320/api/lectures/download/{l.Lec_id}"
                })
                .ToList();

            return Ok(lectures); // ✅ Always return 200 OK with an array (empty if no lectures)
        }

        // Upload lecture for a subject

        [HttpPost("lectures/upload/{subjectId}")]
        public IActionResult UploadLecture(
        int subjectId,
        string name,
        string description,
        IFormFile lectureFile
    )
        {
            if (lectureFile == null || lectureFile.Length == 0)
                return BadRequest("Invalid file. Please upload a valid lecture file.");

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Lecture name is required.");

            // ✅ Fetch the subject
            var subject = _context.Subjects.FirstOrDefault(s => s.Subject_id == subjectId);
            if (subject == null)
                return NotFound($"Subject with ID {subjectId} not found.");

            int doctorId = subject.Doctor_id;

            // ✅ Convert file to byte array
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                lectureFile.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            // ✅ Store file in database
            var lecture = new Lecture
            {
                Upload_Date = DateTime.Now,
                Subject_id = subjectId,
                Name = name,
                Description = description,
                Doctor_id = doctorId,
                FilePath = fileBytes
            };

            _context.Lectures.Add(lecture);
            _context.SaveChanges();

            return Ok(new
            {
                Message = "Lecture uploaded successfully!",
                Lecture = new
                {
                    lecture.Lec_id,
                    lecture.Name,
                    lecture.Description,
                    lecture.Upload_Date,
                    lecture.Subject_id,
                    lecture.Doctor_id
                }
            });
        }




       


        [HttpGet("subjects/{subjectId}/assignments")]
        public IActionResult GetAssignments(int subjectId)
        {
            // ✅ Get the doctor's ID from the subject
            var subject = _context.Subjects.FirstOrDefault(s => s.Subject_id == subjectId);
            if (subject == null)
            {
                return NotFound("Subject not found");
            }
            int doctorId = subject.Doctor_id;

            var assignments = _context.Assignments
                .Where(a => a.Subject_id == subjectId && a.Doctor_id == doctorId) // ✅ Filter by doctor
                .Select(a => new
                {
                    assignment_id = a.Assignment_id,
                    title = a.Title,
                    deadline = a.Deadline,
                    total_grade = a.Total_grade,
                    file_url = $"{Request.Scheme}://{Request.Host}/api/assignments/download/{a.Assignment_id}",
                    model_answer_url = $"{Request.Scheme}://{Request.Host}/api/assignments/modelanswer/{a.Assignment_id}" // ✅ Model Answer URL
                })
                .ToList();

            return Ok(assignments);
        }

        // Upload an assignment
        [HttpPost("assignments/upload")]
        public IActionResult UploadAssignment(
     IFormFile assignmentFile,
     IFormFile modelAnswerFile,
     int subjectId,
     string title,
     DateTime? deadline, // Nullable DateTime
     int totalGrade)
        {
            // ✅ Ensure deadline is always set to current time if null
            var finalDeadline = deadline ?? DateTime.UtcNow;

            // ✅ Validate files
            if (assignmentFile == null || assignmentFile.Length == 0)
                return BadRequest("Invalid assignment file.");
            if (modelAnswerFile == null || modelAnswerFile.Length == 0)
                return BadRequest("Invalid model answer file.");
            if (totalGrade <= 0)
                return BadRequest("Total grade must be greater than zero.");

            // ✅ Convert assignment file to byte array
            byte[] assignmentFileBytes;
            using (var ms = new MemoryStream())
            {
                assignmentFile.CopyTo(ms);
                assignmentFileBytes = ms.ToArray();
            }

            // ✅ Convert model answer file to byte array
            byte[] modelAnswerBytes;
            using (var ms2 = new MemoryStream())
            {
                modelAnswerFile.CopyTo(ms2);
                modelAnswerBytes = ms2.ToArray();
            }

            // ✅ Fetch subject to get Doctor_id
            var subject = _context.Subjects.FirstOrDefault(s => s.Subject_id == subjectId);
            if (subject == null)
                return NotFound("Subject not found.");

            int doctorId = subject.Doctor_id;

            // ✅ Always use finalDeadline, not deadline.Value
            var assignment = new Assignment
            {
                Subject_id = subjectId,
                Title = title,
                Deadline = finalDeadline, // ✅ Always assigned
                AssignmentFile = assignmentFileBytes,
                ModelAnswer = modelAnswerBytes,
                Total_grade = totalGrade,
                Doctor_id = doctorId
            };

            // ✅ Save to Database
            _context.Assignments.Add(assignment);
            _context.SaveChanges();

            return Ok("Assignment uploaded successfully.");
        }




        [HttpGet("{assignmentId}/grades")]
        public IActionResult GetAssignmentGrades(int assignmentId)
        {
            // ✅ Get the assignment
            var assignment = _context.Assignments.FirstOrDefault(a => a.Assignment_id == assignmentId);

            if (assignment == null)
            {
                return NotFound("Assignment not found.");
            }

            // ✅ Get all attempts for the assignment including Student information
            var attempts = _context.Attemps
                .Where(at => at.Assignment_id == assignmentId)
                .Include(at => at.Student)
                .ToList();

            if (attempts == null || attempts.Count == 0)
            {
                return NotFound("No attempts found for this assignment.");
            }

            // ✅ Structure the data
            var gradesData = attempts.Select(attempt => new
            {
                StudentId = attempt.Student.Student_id,
                Grade = attempt.Grade,
                AnswerUrl = $"{Request.Scheme}://{Request.Host}/api/Doctor/downloadAnswer/{attempt.Attemp_id}",
                Feedback = attempt.Feedback
            }).ToList();

            return Ok(gradesData);
        }

        [HttpGet("downloadAnswer/{attemptId}")]
        public IActionResult DownloadAnswer(int attemptId)
        {
            var attempt = _context.Attemps.FirstOrDefault(l => l.Attemp_id == attemptId);
            if (attempt == null)
                return NotFound("Lecture not found.");

            if (attempt.Answer == null || attempt.Answer.Length == 0)
                return NotFound("Lecture file not found in the database.");

            var fileType = "application/pdf";  // Change if needed
            var fileName = $"Answer.pdf";
            return File(attempt.Answer, fileType, fileName);  // ✅ Returns the lecture file
        }

        [HttpGet("{assignmentId}/grades/csv")]
        public IActionResult ExportGradesToCsv(int assignmentId)
        {
            // ✅ Get the assignment
            var assignment = _context.Assignments
                .Include(a => a.Attempts)
                 
                .ThenInclude(at => at.Student)
                .FirstOrDefault(a => a.Assignment_id == assignmentId);

            if (assignment == null)
            {
                return NotFound("Assignment not found.");
            }

            // ✅ Get all attempts for the assignment
            var attempts = _context.Attemps
                .Where(at => at.Assignment_id == assignmentId)
                .Include(at => at.Student)
                .ToList();

            if (attempts == null || attempts.Count == 0)
            {
                return NotFound("No attempts found for this assignment.");
            }

            // ✅ Build the CSV string
            var csvBuilder = new StringBuilder();

            // Add header row
            csvBuilder.AppendLine("Student ID,Grade,Feedback");

            // Add data rows
            foreach (var attempt in attempts)
            {
                // Ensure Student is not null
                if (attempt.Student != null)
                {
                    csvBuilder.AppendLine(
                        $"{attempt.Student.Student_id},{attempt.Grade},{attempt.Feedback}"
                    );
                }
                else
                {
                    // Handle case where Student is null (log, skip, etc.)
                    Console.WriteLine($"Warning: Student is null for attempt ID {attempt.Attemp_id}");
                }
            }

            // Calculate the average grade
            double averageGrade = attempts.Average(at => at.Grade);
            csvBuilder.AppendLine($",,Average Grade,{averageGrade}");

            // ✅ Convert to bytes and return as file
            var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(csvBytes, "text/csv", $"Assignment_{assignmentId}_Grades.csv");
        }


        // Export assignment grades to CSV
        [HttpGet("assignments/grades/{assignmentId}")]
        public IActionResult ExportAssignmentGrades(int assignmentId)
        {
            var grades = _context.Grades
                .Where(g => g.AssignmentId == assignmentId)
                .Select(g => new
                {
                    g.StudentId,
                    g.GradeValue
                })
                .ToList();

            if (grades == null || !grades.Any())
                return NotFound("No grades found.");

            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(grades);
                return File(new MemoryStream(Encoding.UTF8.GetBytes(writer.ToString())), "text/csv", "AssignmentGrades.csv");
            }
        }




        // Get all students in a subject and export to CSV
        [HttpGet("subject/students/{subjectId}")]
        public IActionResult GetStudentsForSubject(int subjectId)
        {
            var students = _context.Students
                .Where(s => _context.StudentSubjects
                    .Any(ss => ss.Student_id == s.Student_id && ss.Subject_id == subjectId))
                .Select(s => new
                {
                    s.Student_id,
                    s.Username
                })
                .ToList();

            if (!students.Any())
                return NotFound("No students found for this subject.");

            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(students);
                return File(new MemoryStream(Encoding.UTF8.GetBytes(writer.ToString())), "text/csv", "StudentsList.csv");
            }
        }


        [HttpGet("{subjectId}/enrolled-students")]
        public IActionResult GetEnrolledStudents(int subjectId)
        {
            var subject = _context.Subjects
                .FirstOrDefault(s => s.Subject_id == subjectId);

            if (subject == null)
            {
                return NotFound($"Subject with ID {subjectId} not found.");
            }

            if (string.IsNullOrEmpty(subject.Student_ids))
            {
                return Ok(new List<object>()); // Return empty list if no students enrolled
            }

            List<int> studentIds;
            try
            {
                // ✅ Deserialize JSON array string to List<int>
                studentIds = JsonSerializer.Deserialize<List<int>>(subject.Student_ids);
            }
            catch (JsonException)
            {
                // Handle potential JSON deserialization error (e.g., invalid format)
                return BadRequest("Invalid Student_ids format in Subject data.");
            }

            if (studentIds == null || !studentIds.Any())
            {
                return Ok(new List<object>()); // Return empty list if deserialization failed or list is empty
            }

            var enrolledStudents = _context.Students
                .Where(s => studentIds.Contains(s.Student_id))
                .Select(s => new
                {
                    studentId = s.Student_id,
                    major = s.Major,
                    email = s.Email
                })
                .ToList();

            return Ok(enrolledStudents);
        }

        // GET: api/Doctor/subjects/{subjectId}/enrolled-students/csv
        [HttpGet("{subjectId}/enrolled-students/csv")]
        public IActionResult GetEnrolledStudentsCsv(int subjectId)
        {
            var subject = _context.Subjects
                .FirstOrDefault(s => s.Subject_id == subjectId);

            if (subject == null)
            {
                return NotFound($"Subject with ID {subjectId} not found.");
            }
            if (string.IsNullOrEmpty(subject.Student_ids))
            {
                return File(Encoding.UTF8.GetBytes("Student ID,Major,Email\n"), "text/csv", $"enrolled_students_subject_{subjectId}.csv"); // Empty CSV header
            }

            List<int> studentIds;
            try
            {
                // ✅ Deserialize JSON array string to List<int>
                studentIds = JsonSerializer.Deserialize<List<int>>(subject.Student_ids);
            }
            catch (JsonException)
            {
                // Handle potential JSON deserialization error (e.g., invalid format)
                return BadRequest("Invalid Student_ids format in Subject data.");
            }
            if (studentIds == null || !studentIds.Any())
            {
                return File(Encoding.UTF8.GetBytes("Student ID,Major,Email\n"), "text/csv", $"enrolled_students_subject_{subjectId}.csv"); // Empty CSV header
            }


            var enrolledStudents = _context.Students
                .Where(s => studentIds.Contains(s.Student_id))
                .Select(s => new
                {
                    studentId = s.Student_id,
                    major = s.Major,
                    email = s.Email
                })
                .ToList();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Student ID,Major,Email"); // CSV Header

            foreach (var student in enrolledStudents)
            {
                csvBuilder.AppendLine($"{student.studentId},{student.major},{student.email}");
            }

            return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), "text/csv", $"enrolled_students_subject_{subjectId}.csv");
        }
    }
}