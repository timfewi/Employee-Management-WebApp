using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController(IUserAccount userAccount) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(Register register)
        {
            if (register == null)
                return BadRequest("Model is null");
            var result = await userAccount.CreateAsync(register);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Login login)
        {
            if (login == null)
                return BadRequest("Model is null");
            var result = await userAccount.SignInAsync(login);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshToken refreshToken)
        {
            if (refreshToken == null)
                return BadRequest("Model is empty");
            var result = await userAccount.RefreshTokenAsync(refreshToken);
            return Ok(result);
        }
    }
}
