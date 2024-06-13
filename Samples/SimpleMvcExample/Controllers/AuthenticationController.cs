// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Identity.Data;
// using Microsoft.AspNetCore.Mvc;
//
// namespace SimpleMvcExample.Controllers;
//
// [ApiController]
// [Route("[controller]")]
// public class AuthenticationController : ControllerBase
// {
//     private readonly UserManager<IdentityUser> _userManager;
//     private readonly SignInManager<IdentityUser> _signInManager;
//
//     public AuthenticationController(
//         UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
//     {
//         _userManager = userManager;
//         _signInManager = signInManager;
//     }
//     
//     [HttpPost("login")]
//     public async Task<IActionResult> Login(LoginRequest loginRequest)
//     {
//         var user = await _signInManager.SignInAsync(loginRequest.Email, loginRequest.Password);
//
//         if (user == null) return Unauthorized();
//
//         return Ok();
//     }
//
//     [HttpPost("logout")]
//     public IActionResult Logout()
//     {
//         // Clear the existing external cookie
//         _userRequestContext.SignOut();
//         
//         return Ok();
//     }
// }