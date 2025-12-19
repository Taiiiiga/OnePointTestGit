using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestOnePoint.Dtos;
using TestOnePoint.Services;

namespace TestOnePoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
        {
            var token = await authService.AuthenticateAsync(request.Email, request.Password);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }
            var response = new AuthResponse
            {
                Token = token,
                ExpiresIn = 1
            };
            return Ok(response);
        }

        

    }
}
