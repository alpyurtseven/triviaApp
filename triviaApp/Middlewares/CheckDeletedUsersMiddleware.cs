using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using triviaApp.Models;
using triviaApp.Middlewares;
using Azure;

namespace triviaApp.Middlewares
{
	public class CheckDeletedUsersMiddleware
	{
        private readonly RequestDelegate _next;


        public CheckDeletedUsersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserManager<User> _userManager)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    await context.SignOutAsync(IdentityConstants.ApplicationScheme);

             
                    context.Response.Redirect("/Home/Index");
                    return;
                }
            }

            await _next(context);
        }
    }
}


public static class CheckDeletedUsersMiddlewareExtensions
{
    public static IApplicationBuilder UseCheckDeletedUsers(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CheckDeletedUsersMiddleware>();
    }
}
