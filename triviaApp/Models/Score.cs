using System.ComponentModel.DataAnnotations;

namespace triviaApp.Models
{
	public class Score
	{
        public int Id { get; set; }

        [Required]
        public int Points { get; set; }

        public int ParticipantId { get; set; }
        public Participant Participant { get; set; }

        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }
    }
}

