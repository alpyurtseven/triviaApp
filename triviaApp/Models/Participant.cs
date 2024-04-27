using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace triviaApp.Models
{
    public class Participant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string Username { get; set; }

        public int CompetitionId { get; set; }

        public Competition Competition { get; set; }

        public ICollection<Score> Scores { get; set; }
    }
}

