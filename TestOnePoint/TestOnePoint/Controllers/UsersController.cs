using Microsoft.AspNetCore.Mvc;
using TestOnePoint.Models;
using TestOnePoint.Services;
using TestOnePoint.Dtos;

namespace TestOnePoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService service) : ControllerBase
    {
        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUser()
        {
            var users = await service.GetAllUsersAsync();

            var responses = users.Select(u => ToResponse(u)).ToList();

            return Ok(responses);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUserByID(int id)
        {
            var user = await service.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User with this ID was not found");
            }

            return Ok(ToResponse(user));
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserRequest request)
        {
            // [ApiController] assure ModelState validation automatique
            var model = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password // plain password -> will be hashed in service
            };

            var createdUser = await service.CreateUserAsync(model);

            return CreatedAtAction(nameof(GetUserByID), new { id = createdUser.Id }, ToResponse(createdUser));
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var existing = await service.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound("User not found.");

            // Appliquer seulement les champs fournis
            var toUpdate = new User
            {
                Id = id,
                Name = request.Name ?? existing.Name,
                Email = request.Email ?? existing.Email,
                Password = request.Password // null = pas de changement
            };

            var updated = await service.UpdateUserAsync(id, toUpdate);
            if (updated == null)
                return NotFound();

            return Ok(ToResponse(updated));
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await service.DeleteUserAsync(id);
            return deleted ? NoContent() : NotFound("Character with the given Id was not found.");
        }

        // Mapping helper — centraliser pour éviter fuite de PasswordHash
        private static UserResponse ToResponse(User u) =>
            new UserResponse
            {
                Id = u.Id,
                Name = u.Name ?? string.Empty,
                Email = u.Email ?? string.Empty
            };
    }
}