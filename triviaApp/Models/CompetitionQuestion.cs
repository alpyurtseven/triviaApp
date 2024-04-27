namespace triviaApp.Models
{
	public class CompetitionQuestion
	{
        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}

