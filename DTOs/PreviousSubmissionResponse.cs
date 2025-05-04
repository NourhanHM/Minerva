using System.Collections.Generic;

namespace Minerva.DTOs
{
    public class PreviousSubmissionResponse
    {
        public double? Grade { get; set; } // Make Grade nullable
        public List<PreviousAnswerModel> Answers { get; set; }
        public string Feedback { get; internal set; }
    }
    public class PreviousAnswerModel
    {
        public int QuestionId { get; set; }
        public dynamic? Answer { get; set; }  // Use dynamic to handle different answer types
    }
}