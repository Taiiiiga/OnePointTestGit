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
            => Ok(await service.GetAllUsersAsync());


        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserByID(int id)
        {
            var user = await service.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User with this ID was not found");
            }

            return Ok(user);
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> AddUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            var createdUser = await service.CreateUserAsync(user);
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
