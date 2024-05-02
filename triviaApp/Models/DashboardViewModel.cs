namespace triviaApp.Models
{
	public class DashboardViewModel
	{
		public List<string> CompetitonNames { get; set; }

        public List<NameAndScore> LastWinners { get; set; }

        public List<NameAndScore> Leaders { get; set; }
    }

    public class NameAndScore
    {
        public string Name { get; set; }
        public int Points { get; set; }
    }
}

