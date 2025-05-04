namespace Minerva.DTOs
{
    public class QuizSubmissionResponse
    {
        public int AttemptId { get; set; }
        public double Grade { get; set; }
        public int TotalPossible { get; set; }
        public string Feedback { get; set; }
    }
}