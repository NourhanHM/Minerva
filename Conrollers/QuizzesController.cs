using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minerva.Data;
using Minerva.Models;
using Minerva.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minerva.Controllers
{
    [ApiController]
    [Route("api/Quizzes")]
    public class QuizzesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizzesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Quiz>> CreateQuiz([FromBody] QuizCreateModel quizDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Doctor ID validation
            var doctor = await _context.Doctors.FindAsync(quizDto.DoctorId);
            if (doctor == null)
            {
                return NotFound($"Doctor with ID {quizDto.DoctorId} not found.");
            }
            // Subject ID validation
            var subject = await _context.Subjects.FindAsync(quizDto.SubjectId);
            if (subject == null)
            {
                return NotFound($"Subject with ID {quizDto.SubjectId} not found.");
            }
            var quiz = new Quiz
            {
                Subject_id = quizDto.SubjectId, // CORRECTLY using SubjectId from DTO
                Title = quizDto.Title,
                Doctor_id = quizDto.DoctorId,
                Final_grade = quizDto.FinalGrade,
                Deadline = quizDto.Deadline,
                Questions = new List<QQuestionTb>()
            };
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            foreach (var questionDto in quizDto.Questions)
            {
                var question = new QQuestionTb
                {
                    Quiz_id = quiz.Quiz_id,
                    Question_type = questionDto.QuestionType,
                    Question_text = questionDto.QuestionText,
                    Question_grade = questionDto.QuestionGrade,
                    Model_answer = questionDto.ModelAnswer, // Default from DTO
                    Options = new List<MCQOptionTb>()
                };
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                if (questionDto.QuestionType.Equals("MCQ", StringComparison.OrdinalIgnoreCase))
                {
                    // List to store the options and their IDs
                    var optionsWithIds = new List<(int Id, bool IsCorrect)>();

                    foreach (var optionDto in questionDto.Options)
                    {
                        var option = new MCQOptionTb
                        {
                            Question_id = question.Question_id,
                            Option_text = optionDto.OptionText
                        };
                        _context.MCQOptions.Add(option);
                        await _context.SaveChangesAsync(); // Need to save to get the option ID

                        optionsWithIds.Add((option.Option_id, optionDto.IsCorrect));
                    }

                    // Find the correct option and update the question's Model_answer
                    var correctOption = optionsWithIds.FirstOrDefault(o => o.IsCorrect);
                    if (correctOption != default)
                    {
                        question.Model_answer = correctOption.Id.ToString();
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return CreatedAtAction(nameof(GetQuizById), new { quizId = quiz.Quiz_id }, quiz);
        }

        // --- Get Quizzes by Subject ID ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizListModel>>> GetQuizzesBySubject([FromQuery] int subjectId) // Use [FromQuery]
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.Subject_id == subjectId)
                .Select(q => new QuizListModel
                {
                    QuizId = q.Quiz_id,
                    Title = q.Title,
                    Deadline = q.Deadline
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        // --- Get Quiz by ID (Detailed) ---
        [HttpGet("{quizId}")]
        public async Task<ActionResult<QuizDetailsModel>> GetQuizById(int quizId)  // Remove subjectId
        {
             var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Quiz_id == quizId);

        if (quiz == null)
        {
            return NotFound($"Quiz with id {quizId} not found.");  // Simpler message
        }

        var quizDetails = new QuizDetailsModel
        {
            QuizId = quiz.Quiz_id,
            Title = quiz.Title,
            SubjectId = quiz.Subject_id,
            DoctorId = quiz.Doctor_id,
            FinalGrade = quiz.Final_grade,
            Deadline = quiz.Deadline,
            Questions = quiz.Questions.Select(q => new QuestionDetailsModel
            {
                QuestionId = q.Question_id,
                QuestionType = q.Question_type,
                QuestionText = q.Question_text,
                QuestionGrade = q.Question_grade,
                ModelAnswer = q.Model_answer,
                Options = q.Options.Select(o => new OptionDetailsModel
                {
                    OptionId = o.Option_id,
                    OptionText = o.Option_text
                }).ToList()
            }).ToList()
        };

        return Ok(quizDetails);

        }


        // --- Update Quiz ---
        [HttpPut("{quizId}")]
        public async Task<IActionResult> UpdateQuiz(int quizId, [FromBody] QuizCreateModel model) // Remove subjectId
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Quiz_id == quizId); // Only quizId needed

            if (quiz == null)
            {
                return NotFound($"Quiz with ID {quizId} not found."); // Simpler message
            }

            quiz.Title = model.Title;
            quiz.Final_grade = model.FinalGrade;
            quiz.Deadline = model.Deadline;
             // Update Subject_id and Doctor_id if they are provided in the DTO
            if (model.SubjectId != 0)
            {
                quiz.Subject_id = model.SubjectId;
            }
            if (model.DoctorId != 0)
            {
                quiz.Doctor_id = model.DoctorId;
            }

            _context.MCQOptions.RemoveRange(quiz.Questions.SelectMany(q => q.Options));
            _context.Questions.RemoveRange(quiz.Questions);

            foreach (var questionModel in model.Questions)
            {
                var question = new QQuestionTb
                {
                    Quiz_id = quizId,
                    Question_type = questionModel.QuestionType,
                    Question_text = questionModel.QuestionText,
                    Question_grade = questionModel.QuestionGrade,
                    Model_answer = questionModel.ModelAnswer
                };
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                if (questionModel.QuestionType.Equals("MCQ", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var optionModel in questionModel.Options)
                    {
                        var option = new MCQOptionTb
                        {
                            Question_id = question.Question_id,
                            Option_text = optionModel.OptionText,
                        };
                        _context.MCQOptions.Add(option);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Delete Quiz ---
        [HttpDelete("{quizId}")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            try
            {
                // First, find the quiz with all related entities
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Quiz_id == quizId);

                if (quiz == null)
                {
                    return NotFound($"Quiz with ID {quizId} not found.");
                }

                // Find all attempts related to this quiz
                var attempts = await _context.QAttempts
                    .Where(a => a.Quiz_id == quizId)
                    .ToListAsync();

                // For each attempt, find all answers and delete them first
                foreach (var attempt in attempts)
                {
                    var attemptAnswers = await _context.QAttemptAnswers
                        .Where(a => a.Attempt_id == attempt.Q_attemp_id)
                        .ToListAsync();

                    // Remove all answers for this attempt
                    _context.QAttemptAnswers.RemoveRange(attemptAnswers);
                }

                // Save changes to delete all answers
                await _context.SaveChangesAsync();

                // Now remove all attempts for this quiz
                _context.QAttempts.RemoveRange(attempts);
                await _context.SaveChangesAsync();

                // Now we can safely remove MCQ options
                foreach (var question in quiz.Questions)
                {
                    var options = await _context.MCQOptions
                        .Where(o => o.Question_id == question.Question_id)
                        .ToListAsync();

                    _context.MCQOptions.RemoveRange(options);
                }
                await _context.SaveChangesAsync();

                // Now remove all questions
                _context.Questions.RemoveRange(quiz.Questions);
                await _context.SaveChangesAsync();

                // Finally, remove the quiz itself
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the error and return a detailed message
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the quiz.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // Add this method to your QuizzesController
        [HttpGet("{quizId}/grades")]
        public async Task<ActionResult<IEnumerable<object>>> GetQuizGrades(int quizId)
        {
            try
            {
                // Check if the quiz exists
                var quiz = await _context.Quizzes.FindAsync(quizId);
                if (quiz == null)
                {
                    return NotFound($"Quiz with ID {quizId} not found.");
                }

                // Get all quiz attempts for this quiz
                var attempts = await _context.QAttempts
                    .Where(a => a.Quiz_id == quizId)
                    .OrderByDescending(a => a.Enrollment_date)
                    .ToListAsync();

                // Group by student to get only the latest attempt for each student
                var latestAttemptsByStudent = attempts
                    .GroupBy(a => a.Student_id)
                    .Select(g => g.First()) // Take the first (most recent) attempt
                    .ToList();

                // Get student information for each attempt
                var gradesData = new List<object>();
                foreach (var attempt in latestAttemptsByStudent)
                {
                    var student = await _context.Students.FindAsync(attempt.Student_id);
                    if (student != null)
                    {
                        gradesData.Add(new
                        {
                            StudentId = student.Student_id,
                            StudentName = $"{student.Username} ",
                            Email = student.Email,
                            Grade = attempt.Question_score,
                            TotalPossible = quiz.Final_grade,
                            SubmissionDate = attempt.Enrollment_date.ToString("yyyy-MM-dd HH:mm"),
                            Feedback = attempt.Feedback
                        });
                    }
                }

                return Ok(gradesData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving quiz grades.", error = ex.Message });
            }
        }

        // Add this method to export quiz grades to CSV
        [HttpGet("{quizId}/grades/csv")]
        public async Task<IActionResult> ExportQuizGradesToCsv(int quizId)
        {
            try
            {
                // Check if the quiz exists
                var quiz = await _context.Quizzes
                    .Include(q => q.Subject)
                    .FirstOrDefaultAsync(q => q.Quiz_id == quizId);

                if (quiz == null)
                {
                    return NotFound($"Quiz with ID {quizId} not found.");
                }

                // Get all quiz attempts for this quiz
                var attempts = await _context.QAttempts
                    .Where(a => a.Quiz_id == quizId)
                    .OrderByDescending(a => a.Enrollment_date)
                    .ToListAsync();

                // Group by student to get only the latest attempt for each student
                var latestAttemptsByStudent = attempts
                    .GroupBy(a => a.Student_id)
                    .Select(g => g.First()) // Take the first (most recent) attempt
                    .ToList();

                // Create CSV content
                var csv = new System.Text.StringBuilder();

                // Add header
                csv.AppendLine("Quiz Name,Subject,Student ID,Student Name,Email,Grade,Total Possible,Submission Date");

                // Add data rows
                foreach (var attempt in latestAttemptsByStudent)
                {
                    var student = await _context.Students.FindAsync(attempt.Student_id);
                    if (student != null)
                    {
                        csv.AppendLine(string.Join(",",
                            quiz.Title,
                            quiz.Subject?.Name ?? "Unknown Subject",
                            student.Student_id,
                            $"{student.Username} ".Replace(",", " "), // Replace commas in names
                            student.Email,
                            attempt.Question_score,
                            quiz.Final_grade,
                            attempt.Enrollment_date.ToString("yyyy-MM-dd HH:mm")
                        ));
                    }
                }

                // Return as CSV file
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"quiz_{quizId}_grades.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while exporting quiz grades.", error = ex.Message });
            }
        }
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionModel submission)
        {
            if (submission == null || submission.Answers == null || !submission.Answers.Any())
            {
                return BadRequest(new { message = "Invalid submission data. Answer field is required." });
            }

            try
            {
                // Validate the quiz and student information
                var quiz = await _context.Quizzes.FindAsync(submission.QuizId);
                var student = await _context.Students.FindAsync(submission.StudentId);

                if (quiz == null || student == null)
                {
                    return BadRequest(new { message = "Invalid quiz or student." });
                }

                // Check if quiz deadline has passed
                if (quiz.Deadline < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Quiz deadline has passed." });
                }

                // Generate a unique attempt ID (since the table doesn't have IDENTITY)
                int attemptId = await GetNextAttemptId();

                // Initialize feedback for the student
                string quizFeedback = "";

                // Create a new attempt record with the manually generated ID
                var attempt = new QAttempTb
                {
                    Q_attemp_id = attemptId,
                    Student_id = submission.StudentId,
                    Quiz_id = submission.QuizId,
                    Enrollment_date = DateTime.UtcNow,
                    Feedback = "", // We'll set this after grading all questions
                    Question_score = 0,
                    Answer = Array.Empty<byte>() // Initialize with empty bytes as required by schema
                };

                _context.QAttempts.Add(attempt);
                await _context.SaveChangesAsync();

                int totalGrade = 0;
                int totalPossibleGrade = 0;
                List<string> questionFeedbacks = new List<string>();
                List<object> questionResults = new List<object>(); // For detailed results

                foreach (var answer in submission.Answers)
                {
                    var question = await _context.Questions.FindAsync(answer.QuestionId);
                    if (question == null)
                    {
                        continue;  // Skip invalid questions
                    }

                    totalPossibleGrade += question.Question_grade;
                    string questionFeedback = "";

                    var newAnswer = new QAttemptAnswerTb
                    {
                        Attempt_id = attemptId,
                        Question_id = answer.QuestionId,
                        Is_correct = false,
                        Answer_grade = 0
                    };

                    // Handle MCQ answers
                    if (question.Question_type.Equals("MCQ", StringComparison.OrdinalIgnoreCase) && answer.OptionId.HasValue)
                    {
                        var selectedOption = await _context.MCQOptions.FindAsync(answer.OptionId);
                        if (selectedOption != null)
                        {
                            // Store the selected option ID
                            newAnswer.Option_id = answer.OptionId;

                            // Get all options for this question sorted by ID
                            var options = await _context.MCQOptions
                                .Where(o => o.Question_id == question.Question_id)
                                .OrderBy(o => o.Option_id)
                                .ToListAsync();

                            // Try to interpret model_answer as either:
                            // 1. Direct option ID
                            // 2. 1-based index of the option in the list
                            int correctOptionId = 0;
                            if (int.TryParse(question.Model_answer, out correctOptionId))
                            {
                                // First try to find by direct option ID
                                var correctOption = await _context.MCQOptions.FindAsync(correctOptionId);

                                // If not found, try to interpret as 1-based index
                                if (correctOption == null && correctOptionId > 0 && correctOptionId <= options.Count)
                                {
                                    correctOption = options[correctOptionId - 1];
                                    // Check if selected matches correct by index
                                    newAnswer.Is_correct = (answer.OptionId == correctOption.Option_id);
                                }
                                else
                                {
                                    // Check by direct ID
                                    newAnswer.Is_correct = (answer.OptionId == correctOptionId);
                                }

                                newAnswer.Answer_grade = newAnswer.Is_correct ? question.Question_grade : 0;

                                // Add feedback for this question
                                if (newAnswer.Is_correct)
                                {
                                    questionFeedback = $"Question {question.Question_id}: Correct! You earned {newAnswer.Answer_grade} points.";
                                }
                                else
                                {
                                    // Get the correct option text
                                    var correctOptionText = "Not Found";
                                    if (correctOption != null)
                                    {
                                        correctOptionText = correctOption.Option_text;
                                    }
                                    else if (correctOptionId > 0 && correctOptionId <= options.Count)
                                    {
                                        correctOptionText = options[correctOptionId - 1].Option_text;
                                    }

                                    questionFeedback = $"Question {question.Question_id}: Incorrect. The correct answer was: {correctOptionText}";
                                }
                            }
                            else
                            {
                                // Invalid model answer format
                                Console.WriteLine($"Invalid model_answer format for MCQ question {question.Question_id}: {question.Model_answer}");
                                questionFeedback = $"Question {question.Question_id}: Error in grading. Please contact support.";
                            }

                            totalGrade += newAnswer.Answer_grade;
                            _context.QAttemptAnswers.Add(newAnswer);

                            // Create a response object with mapped option data
                            var optionsList = options.Select(o => new
                            {
                                optionId = o.Option_id,
                                optionText = o.Option_text
                            }).ToList();

                            // Add this question result to our detailed results list
                            questionResults.Add(new
                            {
                                questionId = question.Question_id,
                                questionText = question.Question_text,
                                questionType = question.Question_type,
                                questionGrade = question.Question_grade,
                                earnedGrade = newAnswer.Answer_grade,
                                isCorrect = newAnswer.Is_correct,
                                modelAnswer = question.Model_answer, // Include the correct answer
                                selectedOptionId = answer.OptionId,
                                options = optionsList // Include all options
                            });
                        }
                    }
                    // Handle Essay/Text-based answers
                    else if (question.Question_type.Equals("Essay", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(answer.Answer))
                    {
                        newAnswer.Answer_text = answer.Answer;

                        // Store the answer first
                        _context.QAttemptAnswers.Add(newAnswer);
                        await _context.SaveChangesAsync();

                        // Grade essay via FastAPI and get feedback
                        var essayGradingResult = await GradeEssayAnswer(newAnswer, question, answer.Answer);

                        // Normalize the grade from the API (which is 0-100) to match the question's max grade
                        int normalizedGrade = (int)Math.Round((essayGradingResult.Grade / 100.0) * question.Question_grade);

                        // Update the answer with the normalized grade
                        newAnswer.Answer_grade = normalizedGrade;
                        newAnswer.Is_correct = normalizedGrade > 0;
                        await _context.SaveChangesAsync();

                        totalGrade += normalizedGrade;

                        questionFeedback = $"Question {question.Question_id}: {essayGradingResult.Feedback} You earned {normalizedGrade} out of {question.Question_grade} points.";

                        // Add this question result to our detailed results list
                        questionResults.Add(new
                        {
                            questionId = question.Question_id,
                            questionText = question.Question_text,
                            questionType = question.Question_type,
                            questionGrade = question.Question_grade,
                            earnedGrade = newAnswer.Answer_grade,
                            isCorrect = newAnswer.Is_correct,
                            modelAnswer = question.Model_answer,
                            studentAnswer = answer.Answer
                        });
                    }
                    // Handle True/False question types
                    else if (question.Question_type.Equals("TrueFalse", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(answer.Answer))
                    {
                        newAnswer.Answer_text = answer.Answer;

                        // Compare using case-insensitive exact string matching
                        bool isExactMatch = answer.Answer.Equals(question.Model_answer, StringComparison.OrdinalIgnoreCase);

                        // Also check if they're using different formats (e.g., "True" vs "true" vs "1")
                        bool isCorrectAsTrueFormat = (answer.Answer.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                                     answer.Answer.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                                     answer.Answer.Equals("yes", StringComparison.OrdinalIgnoreCase)) &&
                                                     (question.Model_answer.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                                     question.Model_answer.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                                     question.Model_answer.Equals("yes", StringComparison.OrdinalIgnoreCase));

                        bool isCorrectAsFalseFormat = (answer.Answer.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                                                     answer.Answer.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                                                     answer.Answer.Equals("no", StringComparison.OrdinalIgnoreCase)) &&
                                                     (question.Model_answer.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                                                     question.Model_answer.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                                                     question.Model_answer.Equals("no", StringComparison.OrdinalIgnoreCase));

                        // Combine the checks
                        newAnswer.Is_correct = isExactMatch || isCorrectAsTrueFormat || isCorrectAsFalseFormat;
                        newAnswer.Answer_grade = newAnswer.Is_correct ? question.Question_grade : 0;

                        // Add feedback for this question
                        if (newAnswer.Is_correct)
                        {
                            questionFeedback = $"Question {question.Question_id}: Correct! You earned {newAnswer.Answer_grade} points.";
                        }
                        else
                        {
                            questionFeedback = $"Question {question.Question_id}: Incorrect. The correct answer was {question.Model_answer}.";
                        }

                        totalGrade += newAnswer.Answer_grade;
                        _context.QAttemptAnswers.Add(newAnswer);

                        // Add this question result to our detailed results list
                        questionResults.Add(new
                        {
                            questionId = question.Question_id,
                            questionText = question.Question_text,
                            questionType = question.Question_type,
                            questionGrade = question.Question_grade,
                            earnedGrade = newAnswer.Answer_grade,
                            isCorrect = newAnswer.Is_correct,
                            modelAnswer = question.Model_answer,
                            studentAnswer = answer.Answer
                        });
                    }
                    else
                    {
                        // For other question types
                        if (!string.IsNullOrEmpty(answer.Answer))
                        {
                            newAnswer.Answer_text = answer.Answer;
                            _context.QAttemptAnswers.Add(newAnswer);
                            questionFeedback = $"Question {question.Question_id}: Your answer has been recorded.";

                            // Add this question result to our detailed results list
                            questionResults.Add(new
                            {
                                questionId = question.Question_id,
                                questionText = question.Question_text,
                                questionType = question.Question_type,
                                studentAnswer = answer.Answer
                            });
                        }
                    }

                    // Add feedback for this question to our list
                    questionFeedbacks.Add(questionFeedback);
                }

                // Calculate percentage score
                double percentageScore = totalPossibleGrade > 0 ?
                    (double)totalGrade / totalPossibleGrade * 100 : 0;

                // Generate overall feedback based on score
                if (percentageScore >= 90)
                {
                    quizFeedback = "Excellent job! You've demonstrated outstanding understanding of this material.";
                }
                else if (percentageScore >= 80)
                {
                    quizFeedback = "Great work! You have a strong grasp of this material.";
                }
                else if (percentageScore >= 70)
                {
                    quizFeedback = "Good effort! You've shown a solid understanding of most concepts.";
                }
                else if (percentageScore >= 60)
                {
                    quizFeedback = "Satisfactory. You've grasped some of the key concepts, but there's room for improvement.";
                }
                else
                {
                    quizFeedback = "You might need to review this material further to strengthen your understanding.";
                }

                // Combine overall feedback with question-specific feedback
                quizFeedback += "\n\n" + string.Join("\n", questionFeedbacks);

                // Update the attempt with the final score and feedback
                attempt.Question_score = totalGrade;
                attempt.Feedback = quizFeedback;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Quiz submitted successfully",
                    attemptId = attempt.Q_attemp_id,
                    grade = totalGrade,
                    totalPossible = quiz.Final_grade,
                    feedback = attempt.Feedback,
                    submissionDate = attempt.Enrollment_date.ToString("yyyy-MM-dd HH:mm"),
                    questions = questionResults // Include detailed question results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the quiz", error = ex.Message });
            }
        }

        // Helper method to generate the next attempt ID
        private async Task<int> GetNextAttemptId()
        {
            // Get the highest current ID and add 1
            var maxId = await _context.QAttempts
                .OrderByDescending(a => a.Q_attemp_id)
                .Select(a => a.Q_attemp_id)
                .FirstOrDefaultAsync();

            return maxId + 1;
        }

        // Helper method to grade essay answers via your existing FastAPI service
        // Helper method to grade essay answers via your FastAPI service
        private async Task<EssayGradingResult> GradeEssayAnswer(QAttemptAnswerTb answerRecord, QQuestionTb question, string studentAnswer)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Create the exact same format as the working curl example
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                new KeyValuePair<string, string>("model_answer", question.Model_answer),
                new KeyValuePair<string, string>("student_answer", studentAnswer)
            });

                    // Add the exact same headers as the working curl example
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // Debug info
                    var requestContent = await formContent.ReadAsStringAsync();
                    Console.WriteLine($"Sending to API: {requestContent}");

                    // Make the POST request to your FastAPI endpoint
                    var response = await client.PostAsync("http://127.0.0.1:8068/grade-form/", formContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API Success Response: {responseContent}");

                        // Deserialize the response
                        var options = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var gradingResult = System.Text.Json.JsonSerializer.Deserialize<EssayGradingResult>(responseContent, options);

                        // Update the answer record with the grade
                        answerRecord.Answer_grade = gradingResult.Grade;
                        answerRecord.Is_correct = gradingResult.Grade > 0;

                        await _context.SaveChangesAsync();

                        return gradingResult;
                    }
                    else
                    {
                        // Handle API error
                        answerRecord.Answer_grade = 0;
                        await _context.SaveChangesAsync();

                        // Log the error response for debugging
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API Error: {response.StatusCode}, Details: {errorResponse}");

                        return new EssayGradingResult
                        {
                            Grade = 0,
                            Feedback = $"Unable to grade essay. API Error: {response.StatusCode}"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error and continue
                Console.WriteLine($"Error grading essay: {ex.Message}");
                answerRecord.Answer_grade = 0;
                await _context.SaveChangesAsync();

                return new EssayGradingResult
                {
                    Grade = 0,
                    Feedback = $"Error grading essay: {ex.Message}"
                };
            }
        }

        // Class to receive FastAPI response from your existing API
        public class EssayGradingResult
        {
            public int Grade { get; set; }
            public string Feedback { get; set; }
            // Add any other fields your API returns here
        }


        // Add this method to your controller for direct testing
        [HttpGet("test-fastapi-direct")]
        public async Task<IActionResult> TestFastApiDirect()
        {
            try
            {
                // Direct HttpClient approach without any libraries
                using (var httpClient = new HttpClient())
                {
                    // Create content matching the curl example
                    var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model_answer", "bananas are yellow"),
                new KeyValuePair<string, string>("student_answer", "bananas are green")
            };

                    var content = new FormUrlEncodedContent(postData);

                    // Set the header like in the curl example
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // Print what we're sending
                    Console.WriteLine($"Sending form data: {await content.ReadAsStringAsync()}");

                    // Send the request
                    var response = await httpClient.PostAsync("http://127.0.0.1:8068/grade-form/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Success! Response: {responseString}");
                        return Ok(new { success = true, response = responseString });
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error: {response.StatusCode}, Message: {errorResponse}");
                        return StatusCode((int)response.StatusCode, new { error = errorResponse });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // GET: api/Quizzes/student/{studentId}/quiz/{quizId}/submission
        // GET: api/Quizzes/student/{studentId}/quiz/{quizId}/submission
        [HttpGet("student/{studentId}/quiz/{quizId}/submission")]
        public async Task<ActionResult<object>> GetStudentQuizSubmission(int studentId, int quizId)
        {
            try
            {
                // Find the quiz with all questions and options
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Quiz_id == quizId);

                if (quiz == null)
                {
                    return NotFound(new { message = "Quiz not found." });
                }

                // Find the latest attempt for this student and quiz
                var attempt = await _context.QAttempts
                    .Where(a => a.Student_id == studentId && a.Quiz_id == quizId)
                    .OrderByDescending(a => a.Enrollment_date)
                    .FirstOrDefaultAsync();

                if (attempt == null)
                {
                    return NotFound(new { message = "No submission found for this student and quiz." });
                }

                // Get all answers for this attempt
                var answers = await _context.QAttemptAnswers
                    .Where(a => a.Attempt_id == attempt.Q_attemp_id)
                    .ToListAsync();

                // Format the questions with student answers
                var questionsWithAnswers = quiz.Questions.Select(q => {
                    // Find the student's answer for this question
                    var studentAnswer = answers.FirstOrDefault(a => a.Question_id == q.Question_id);

                    // Base question info
                    var questionDetails = new
                    {
                        QuestionId = q.Question_id,
                        QuestionType = q.Question_type,
                        QuestionText = q.Question_text,
                        QuestionGrade = q.Question_grade,
                        ModelAnswer = q.Model_answer,

                        // Student's answer details
                        StudentAnswer = studentAnswer?.Answer_text,
                        SelectedOptionId = studentAnswer?.Option_id,
                        IsCorrect = studentAnswer?.Is_correct ?? false,
                        EarnedGrade = studentAnswer?.Answer_grade ?? 0,

                        // For MCQ, include all options with correctness
                        Options = q.Question_type.Equals("MCQ", StringComparison.OrdinalIgnoreCase) ?
                            q.Options.Select(o => new {
                                OptionId = o.Option_id,
                                OptionText = o.Option_text,
                                IsCorrect = o.Option_id.ToString() == q.Model_answer,
                                IsSelected = o.Option_id == studentAnswer?.Option_id
                            }).ToList() : null
                    };

                    return questionDetails;
                }).ToList();

                // Return detailed submission data
                return Ok(new
                {
                    AttemptId = attempt.Q_attemp_id,
                    Grade = attempt.Question_score,
                    TotalPossible = quiz.Final_grade,
                    Feedback = attempt.Feedback,
                    SubmissionDate = attempt.Enrollment_date,
                    Questions = questionsWithAnswers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the quiz submission.", error = ex.Message });
            }
        }

    }
}