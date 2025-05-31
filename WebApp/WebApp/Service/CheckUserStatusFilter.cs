using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using WebApp.Abstract;

namespace WebApp.Service
{
    public class CheckUserStatusFilter : IActionFilter
    {
        private readonly IUser _userService;
        private readonly ILogger<CheckUserStatusFilter> _logger;

        public CheckUserStatusFilter(IUser userService, ILogger<CheckUserStatusFilter> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            _logger.LogInformation($"CheckUserStatusFilter: Executing for {controller}/{action}");

            if (string.Equals(controller, "Auth", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Skipping filter for Auth controller action: {action}");
                return;
            }

            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogInformation("User not authenticated, skipping filter");
                return;
            }

            var userIdClaim = context.HttpContext.User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("User ID claim not found or invalid");
                RedirectToLogin(context);
                return;
            }

            _logger.LogInformation($"Checking status for user ID: {userId}");

            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found in database");
                RedirectToLogin(context);
                return;
            }

            if (user.IsBlocked)
            {
                _logger.LogWarning($"Blocked user {userId} ({user.Email}) attempted to access {controller}/{action}");
                RedirectToLogin(context);
                return;
            }

            _logger.LogInformation($"User {userId} ({user.Email}) status check passed");

            try
            {
                _userService.UpdateLastSeen(userId, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update last seen for user {userId}");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private async void RedirectToLogin(ActionExecutingContext context)
        {
            try
            {
                await context.HttpContext.SignOutAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign out");
            }

            context.Result = new RedirectToActionResult("Login", "Auth", null);
        }
    }
}