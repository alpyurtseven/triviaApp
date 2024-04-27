using System;
namespace triviaApp.Models
{
	public class CompetitionCategory
	{
        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}

