using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Abstract;

namespace WebApp.Controllers
{
    [Authorize] 
    public class AdminController : Controller
    {
        private readonly IUser _service;

        public AdminController(IUser service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var users = _service.GetUsers();

            var sortedUsers = users
             .OrderByDescending(u => u.LastSeen.HasValue)
              .ThenByDescending(u => u.LastSeen)
                .ToList();

            return View(sortedUsers);
        }

        [HttpGet]
        public IActionResult UserDetails(int id)
        {
            var user = _service.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUsers([FromBody] List<int> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    _service.DeleteUser(id);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BlockUsers([FromBody] List<int> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    _service.BlockUser(id);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UnblockUsers([FromBody] List<int> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    _service.UnblockUser(id);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

}
