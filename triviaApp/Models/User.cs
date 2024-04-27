using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace triviaApp.Models
{
	public class User:IdentityUser
	{
        public byte Status { get; set; } = 1;
    }
}

