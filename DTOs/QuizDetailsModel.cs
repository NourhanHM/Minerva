//DTOs/QuizDetailsModel.cs
namespace Minerva.DTOs
{
    public class QuizDetailsModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
        public int SubjectId { get; set; }
        public int DoctorId { get; set; }
        public int FinalGrade { get; set; }
        public DateTime Deadline { get; set; }
        public List<QuestionDetailsModel> Questions { get; set; } = new();
    }
}