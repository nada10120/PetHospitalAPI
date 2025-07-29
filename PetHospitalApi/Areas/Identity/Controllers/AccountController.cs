using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;
using Models.DTOs.Request;
using Mapster;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using ECommerce513.Models.ViewModels;

namespace PetHospitalApi.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IEmailSender emailSender, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequests registerRequest)
        {
            User applicationUser = registerRequest.Adapt<User>();
            var result = await _userManager.CreateAsync(applicationUser, registerRequest.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/Identity/Account/ConfirmEmail?userId={applicationUser.Id}&token={Uri.EscapeDataString(token)}";
                Console.WriteLine($"Generated Confirmation Link: {confirmationLink}");
                await _emailSender.SendEmailAsync(registerRequest.Email, "Confirm Your Account", $"Please Confirm Your Account By Clicking <a href='{confirmationLink}'>Here</a>");
                Console.WriteLine($"Confirmation Link: {confirmationLink}");
                
                await _userManager.AddToRoleAsync(applicationUser, SD.Client);
                return Ok(new { Message = "User created successfully, please check your email to confirm your account." });
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequests loginRequest)
        {
            var applicationUser = await _userManager.FindByEmailAsync(loginRequest.EmailOrUserName);
            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByNameAsync(loginRequest.EmailOrUserName);
            }
            ModelStateDictionary keyValuePairs = new ModelStateDictionary();

            if (applicationUser is not null)
            {
                if (applicationUser.LockoutEnabled)
                {
                    var result = await _userManager.CheckPasswordAsync(applicationUser, loginRequest.Password);
                    if (result)
                    {
                        await _signInManager.SignInAsync(applicationUser, loginRequest.RememberMe);
                        return Ok(new { Message = "User Logged in successfully" });
                    }
                    else
                    {
                        keyValuePairs.AddModelError("EmailOrUserName", "Invalid Email Or User Name");
                        keyValuePairs.AddModelError("Password", "Invalid Password");
                    }
                }
                else
                {
                    keyValuePairs.AddModelError(string.Empty, $"You Have Block Until {applicationUser.LockoutEnd}");
                }
            }
            else
            {
                keyValuePairs.AddModelError("EmailOrUserName", "Invalid Email Or User Name");
                keyValuePairs.AddModelError("Password", "Invalid Password");
            }
            return BadRequest(keyValuePairs);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "the user logged out " });
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var applicationUser = await _userManager.FindByIdAsync(userId);
            if (applicationUser is not null)
            {
                var result = await _userManager.ConfirmEmailAsync(applicationUser, token);
                if (result.Succeeded)
                {
                    return Ok(new { Message = "Your Email Has Been Confirmed" });
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPost("ResendEmail")]
        public async Task<IActionResult> ResendEmail(ResendEmailRequest resendEmailrequest)
        {
          

            var applicationUser = await _userManager.FindByEmailAsync(resendEmailrequest.EmailOrUserName);

            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByNameAsync(resendEmailrequest.EmailOrUserName);
            }

            if (applicationUser is not null)
            {

                if (!applicationUser.EmailConfirmed)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                    var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/Identity/Account/ConfirmEmail?userId={applicationUser.Id}&token={Uri.EscapeDataString(token)}";
                    Console.WriteLine($"Generated Confirmation Link: {confirmationLink}");
                    await _emailSender.SendEmailAsync(applicationUser!.Email, "Confirm Your Account", $"Please Confirm Your Account By Clicking <a href='{confirmationLink}'>Here</a>");
                    return Ok (new { message = "a confirmation link has been sent to you"});
                }
                else
                {

                    return Ok( new { message = "your email is already confirmed" });
                }

            }

            return NotFound("Invalid Email Or User Name");
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest)
        {
            var applicationUser = await _userManager.FindByEmailAsync(forgetPasswordRequest.EmailOrUserName);

            if (applicationUser is null)
            {
                applicationUser = await _userManager.FindByNameAsync(forgetPasswordRequest.EmailOrUserName);
            }

            if (applicationUser is not null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
                var resetPasswordLink = $"{Request.Scheme}://{Request.Host}/api/Identity/Account/ResetPassword?userId={applicationUser.Id}&token={Uri.EscapeDataString(token)}";
                Console.WriteLine($"Password Reset Link for user {applicationUser.Id}: {resetPasswordLink}");
                await _emailSender.SendEmailAsync(applicationUser!.Email ?? "", "Reset Password",
                    $"Please reset your password by clicking this <a href='{resetPasswordLink}'>Reset Password</a>.");
                return Ok("A password reset email has been sent to your email");
            }
            return BadRequest("Invalid email or username");
        }




        [HttpPost("ConfirmResetPassword")]
        public async Task<IActionResult> ConfirmResetPassword(ResetPasswordRequests resetPasswordRequest)
        {
            
            var applicationUser = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);

            if (applicationUser != null && applicationUser.Id == resetPasswordRequest.UserId)
            {
                var result = await _userManager.ResetPasswordAsync(applicationUser, resetPasswordRequest.Token, resetPasswordRequest.Password);

                if (result.Succeeded)
                {
                    await _emailSender.SendEmailAsync(resetPasswordRequest.Email, "Reset Password Successfully", $"Reset Password Successfully");


                    return NoContent();
                }
                else
                {
                    
                    return BadRequest(result.Errors);
                }
            }

            return NotFound("Invalid Email");
        }
    }
}