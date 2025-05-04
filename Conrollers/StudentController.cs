using Microsoft.AspNetCore.Mvc;
using Minerva.Data;
using Minerva.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
//using Swashbuckle.Swagger.Annotations;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace Minerva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<StudentController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentController(AppDbContext context, HttpClient httpClient, IHttpClientFactory httpClientFactory, ILogger<StudentController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClient;
            _logger = logger;

        }
         


        [HttpGet]
        public IActionResult GetAllStudents()
        {
            var students = _context.Students.ToList();
            return Ok(students);
        }




        [HttpGet("/subjects/{studentId}")]
        public IActionResult GetSubjectsForStudent(int studentId)
        {
            var subjects = _context.Subjects
                .Where(s => s.Student_ids.Contains($"[{studentId}]") ||
                            s.Student_ids.Contains($",{studentId},") ||
                            s.Student_ids.Contains($"[{studentId},") ||
                            s.Student_ids.Contains($",{studentId}]"))  // ✅ Only one ID
                .Select(s => new
                {
                    s.Subject_id,   // ✅ Subject ID
                    s.Name,         // ✅ Subject Name
                    s.Description   // ✅ Subject Description
                })
                .ToList();

            if (!subjects.Any())
                return NotFound("No subjects found for this student.");

            return Ok(subjects);
        }

        [HttpGet]
        [Route("student/{studentId}/profile")]
        public IActionResult GetStudentProfile(int studentId)
        {
            try
            {
                var student = _context.Students.FirstOrDefault(s => s.Student_id == studentId);

                if (student == null)
                {
                    return NotFound("Student not found.");
                }

                var profile = new
                {
                    studentId = student.Student_id,
                    username = student.Username,
                    email = student.Email,
                    nationalId = student.National_id, // Add National ID
                    major = student.Major,       // Add Major
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while retrieving the student profile.");
            }
        }
        [HttpGet]
        [Route("student/{studentId}/grades")]
        public IActionResult GetStudentGrades(int studentId)
        {
            try
            {
                // Get assignment grades
                var assignmentGrades = (from attemp in _context.Attemps
                                        join assignment in _context.Assignments on attemp.Assignment_id equals assignment.Assignment_id
                                        join subject in _context.Subjects on assignment.Subject_id equals subject.Subject_id
                                        where attemp.Student_id == studentId
                                        select new
                                        {
                                            itemName = assignment.Title,
                                            subjectName = subject.Name,
                                            grade = attemp.Grade,
                                            feedback = attemp.Feedback,
                                            itemId = assignment.Assignment_id,
                                            totalGrade = assignment.Total_grade,
                                            submissionDate = attemp.Enrollment_date,
                                            itemType = "Assignment"
                                        }).ToList();

                // Get quiz grades - but first group by quiz to get only the latest attempt for each quiz
                var quizGrades = (from attempt in _context.QAttempts
                                  join quiz in _context.Quizzes on attempt.Quiz_id equals quiz.Quiz_id
                                  join subject in _context.Subjects on quiz.Subject_id equals subject.Subject_id
                                  where attempt.Student_id == studentId
                                  // Group by quiz ID to get only one entry per quiz
                                  group attempt by new { quiz.Quiz_id, quiz.Title, subject.Name, quiz.Final_grade } into g
                                  select new
                                  {
                                      itemName = g.Key.Title,
                                      subjectName = g.Key.Name,
                                      // Get the latest attempt's score
                                      grade = g.OrderByDescending(a => a.Enrollment_date).First().Question_score,
                                      // Get the latest attempt's feedback
                                      feedback = g.OrderByDescending(a => a.Enrollment_date).First().Feedback,
                                      itemId = g.Key.Quiz_id,
                                      totalGrade = g.Key.Final_grade,
                                      // Get the latest submission date
                                      submissionDate = g.OrderByDescending(a => a.Enrollment_date).First().Enrollment_date,
                                      itemType = "Quiz"
                                  }).ToList();

                // Combine both types of grades and convert to a list
                var allGrades = assignmentGrades.Cast<object>()
                    .Concat(quizGrades.Cast<object>())
                    .ToList();

                return Ok(allGrades);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error getting student grades: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving grades.");
            }
        }


        [HttpGet("subjects/{subjectId}/lectures")]
        public IActionResult GetLecturesForSubject(int subjectId)
        {
            var subject = _context.Subjects.FirstOrDefault(s => s.Subject_id == subjectId);
            if (subject == null)
                return NotFound("Subject not found.");

            var lectures = _context.Lectures
                .Where(l => l.Subject_id == subjectId)
                .Select(l => new
                {
                    LectureId = l.Lec_id,
                    Name = l.Name,
                    Description = l.Description,
                    UploadDate = l.Upload_Date,
                    DoctorId = l.Doctor_id,
                    DownloadUrl = $"{Request.Scheme}://{Request.Host}/api/Student/lectures/download/{l.Lec_id}" // ✅ New download API
                })
                .ToList();

            return Ok(lectures);
        }
        [HttpGet("lectures/download/{lectureId}")]
        public IActionResult DownloadLecture(int lectureId)
        {
            var lecture = _context.Lectures.FirstOrDefault(l => l.Lec_id == lectureId);
            if (lecture == null)
                return NotFound("Lecture not found.");

            if (lecture.FilePath == null || lecture.FilePath.Length == 0)
                return NotFound("Lecture file not found in the database.");

            var fileType = "application/pdf";  // Change if needed
            var fileName = $"{lecture.Name}.pdf";
            return File(lecture.FilePath, fileType, fileName);  // ✅ Returns the lecture file
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

       



        [HttpPost("student/{studentId}/assignments/{assignmentId}/submit")]
     
        public async Task<IActionResult> SubmitAssignment(int studentId, int assignmentId, IFormFile studentFile)
        {
            try
            {
                // ✅ Retrieve student and assignment from the database
                var student = _context.Students.FirstOrDefault(s => s.Student_id == studentId);
                if (student == null)
                    return NotFound("Student not found.");

                var assignment = _context.Assignments.FirstOrDefault(a => a.Assignment_id == assignmentId);
                if (assignment == null)
                    return NotFound("Assignment not found.");

                // ✅ Read the uploaded student file into a byte array
                byte[] studentAnswerBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await studentFile.CopyToAsync(memoryStream);
                    studentAnswerBytes = memoryStream.ToArray();
                }

                // ✅ Check if the student already submitted this assignment
                var existingSubmission = _context.Attemps.FirstOrDefault(a => a.Student_id == studentId && a.Assignment_id == assignmentId);

                if (existingSubmission != null)
                {
                    return Conflict("Student has already submitted this assignment.");
                }

                // ✅ Create a new submission record
                var submission = new Attemp
                {
                    Student_id = studentId,
                    Assignment_id = assignmentId,
                    Feedback = "Pending grading...",
                    Grade = 0,
                    Answer = studentAnswerBytes,
                    Enrollment_date = DateTime.Now
                };

                _context.Attemps.Add(submission);
                await _context.SaveChangesAsync(); // ✅ Save the submission before calling AI

                // ✅ Prepare files for sending to the AI model
                using (var formData = new MultipartFormDataContent())
                {
                    // ✅ Add the model answer
                    var modelAnswerContent = new ByteArrayContent(assignment.ModelAnswer ?? Array.Empty<byte>());
                    modelAnswerContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    formData.Add(modelAnswerContent, "model_answer", "model_answer.pdf");

                    // ✅ Add the student's answer
                    var studentAnswerContent = new ByteArrayContent(studentAnswerBytes);
                    studentAnswerContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    formData.Add(studentAnswerContent, "student_answer", "student_submission.pdf");

                    // ✅ Send the files to the AI model
                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.PostAsync("http://127.0.0.1:8066/grade/", formData);

                    if (!response.IsSuccessStatusCode)
                        return StatusCode((int)response.StatusCode, "Error communicating with AI model");

                    // ✅ Parse the AI model's response
                    var aiResult = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"AI Model Response: {aiResult}"); // ✅ Print AI response

                    var aiResponse = JsonSerializer.Deserialize<AIResponse>(aiResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                    // ✅ Update the submission with the AI-generated grade and feedback
                    int gr = (int)Math.Round(aiResponse.Grade);
                    submission.Grade = (gr / 100.0) * assignment.Total_grade;
                    

                    submission.Feedback = aiResponse.Feedback;

                    _context.Attemps.Update(submission);
                    await _context.SaveChangesAsync(); // ✅ Save the updated submission

                    // ✅ Return the results to the client
                    return Ok(new
                    {
                        Message = "Assignment submitted and graded successfully",
                        Grade = submission.Grade,
                        Feedback = submission.Feedback
                    });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // AIResponse class to match the FastAPI response structure
        public class AIResponse
        {
            public float Grade { get; set; }
            public string Feedback { get; set; }
        }

        [HttpGet("{studentId}/assignments/{assignmentId}/submission-details")]
        public IActionResult GetSubmissionDetails(int studentId, int assignmentId)
        {
            // ✅ Retrieve the submission from the database based on studentId and assignmentId
            var submission = _context.Attemps
                .Where(a => a.Student_id == studentId && a.Assignment_id == assignmentId)
                .Select(s => new
                {
                    SubmissionDate = s.Enrollment_date, // Or your submission date property (adjust to match your model)
                    Grade = s.Grade,                      // Grade of the submission (adjust to match your model)
                    Feedback = s.Feedback,                // Feedback on the submission (adjust to match your model)
                    AnswerUrl = "placeholder_download_url_logic", // ⚠️ IMPORTANT: Implement your download URL generation logic here! Placeholder for now.
                    // Add other relevant details you want to send to Flutter
                })
                .FirstOrDefault();

            if (submission == null)
            {
                // ✅ Return 404 Not Found if no submission exists for this student and assignment
                return NotFound("Submission not found for this assignment and student.");
            }

            // ✅ Return 200 OK with the submission details as JSON
            return Ok(submission);
        }




        [HttpPost("add")]

      
        public IActionResult AddStudent([FromBody] Student student)
        {
            if (student == null)
                return BadRequest("Invalid student data.");

            _context.Students.Add(student);
            _context.SaveChanges();

            return Ok("Student added successfully.");
        }





        [HttpPut("modify/{id}")]
        public IActionResult ModifyStudent(int id, [FromBody] Student updatedStudent)
        {
            var student = _context.Students.FirstOrDefault(s => s.Student_id == id);

            if (student == null)
                return NotFound($"Student with ID {id} not found.");

            student.Username = updatedStudent.Username;
            student.Email = updatedStudent.Email;
            student.Major = updatedStudent.Major;

            _context.SaveChanges();

            return Ok("Student updated successfully.");
        }

        [HttpDelete("delete/{id}")]
        public IActionResult DeleteStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Student_id == id);

            if (student == null)
                return NotFound($"Student with ID {id} not found.");

            _context.Students.Remove(student);
            _context.SaveChanges();

            return Ok("Student deleted successfully.");
        }
    }
}
