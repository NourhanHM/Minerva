//DTOs/QuestionDetailsModel.cs
namespace Minerva.DTOs
{
    public class QuestionDetailsModel
    {
        public int QuestionId { get; set; }
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public int QuestionGrade { get; set; }
        public string? ModelAnswer { get; set; }
        public List<OptionDetailsModel> Options { get; set; } = new();
    }
}