using Microsoft.AspNetCore.Mvc;
using TestOnePoint.Models;
using TestOnePoint.Services;

namespace TestOnePoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService service) : ControllerBase
    {
        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUser()
        {
            var users = await service.GetAllUsersAsync();
            // Do not expose password hashes in responses
            foreach (var u in users)
            {
                u.PasswordHash = null!;
            }
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserByID(int id)
        {
            var user = await service.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User with this ID was not found");
            }

            user.PasswordHash = null!;
            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> AddUser(User user)
        {
            var createdUser = await service.CreateUserAsync(user);
            createdUser.PasswordHash = null!;
            return CreatedAtAction(nameof(GetUserByID), new { id = createdUser.Id }, createdUser);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await service.DeleteUserAsync(id);
            return deleted ? NoContent() : NotFound("Character with the given Id was not found.");
        }
    }
}