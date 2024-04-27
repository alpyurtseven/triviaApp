using System;
using System.ComponentModel.DataAnnotations;

namespace triviaApp.Models
{
	public class Question
	{
        public int Id { get; set; }

        [Required(ErrorMessage = "Soru metni gereklidir.")]
        public string Body { get; set; }

        [Required(ErrorMessage = "Doğru cevap gereklidir.")]
        public string RightAnswer { get; set; }

        public string WrongAnswer1 { get; set; }
        public string WrongAnswer2 { get; set; }
        public string WrongAnswer3 { get; set; }

        public int Points { get; set; }
        public int Time { get; set; }
        public int Difficulty { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<CompetitionQuestion> CompetitionQuestions { get; set; }
    }
}

