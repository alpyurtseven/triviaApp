using System;
using System.ComponentModel.DataAnnotations;

namespace triviaApp.Models
{
	public class Competition
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yarışma adı gereklidir.")]
        public string Name { get; set; }

        public string JoinLink { get; set; }

        public bool isOver { get; set; } = false;

        public ICollection<Participant> Participants { get; set; }
        public ICollection<Score> Scores { get; set; }
        public ICollection<CompetitionCategory> CompetitionCategories { get; set; }
        public ICollection<CompetitionQuestion> CompetitionQuestions { get; set; }
    }
}

