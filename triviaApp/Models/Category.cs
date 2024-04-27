using System;
using System.ComponentModel.DataAnnotations;

namespace triviaApp.Models
{
	public class Category
	{
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı gereklidir.")]
        public string Name { get; set; }

        public ICollection<Question> Questions { get; set; }

        public ICollection<CompetitionCategory> CompetitionCategories { get; set; }

    }
}

