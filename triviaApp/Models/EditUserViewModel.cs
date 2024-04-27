using System;
namespace triviaApp.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}

