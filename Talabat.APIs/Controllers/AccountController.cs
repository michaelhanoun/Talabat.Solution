using System.Net;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Extensions;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Models;
using Talabat.Core.Services.Contract;

namespace Talabat.APIs.Controllers
{

    public class AccountController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ISendEmail _emailSender;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAuthService authService,IMapper mapper,ISendEmail emailSender,ILogger<AccountController>logger,IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
           _mapper = mapper;
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return Unauthorized(new ApiResponse(401, "Invalid Login"));
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new ApiResponse(401, "Invalid Login"));
            var auth = await _authService.AddRefreshTokenToUser(user.Id);
            if(auth is null)
                return Unauthorized(new ApiResponse(401, "Could not issue tokens"));
            Response.Cookies.Append("refreshToken", auth.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = auth.ExpireOfRefreshToken
            });
            return Ok(new UserDto() { DisplayName = user.DisplayName, Email = user.Email, JwtToken = auth.JwtToken,RefreshToken = auth.RefreshToken });
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto model)
        {
            var user = new ApplicationUser()
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                UserName = model.Email.Split("@")[0],
                PhoneNumber = model.Phone
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new ApiValidationErrorResponse() { Errors = result.Errors.Select(E => E.Description) });
            var auth = await _authService.AddRefreshTokenToUser(user.Id);
            if (auth is null)
                return Unauthorized(new ApiResponse(401, "Could not issue tokens"));
            Response.Cookies.Append("refreshToken", auth.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = auth.ExpireOfRefreshToken
            });
            return Ok(new UserDto() { DisplayName = user.DisplayName, Email = user.Email, JwtToken = auth.JwtToken,RefreshToken = auth.RefreshToken });
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<MeDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email)??string.Empty;

            var user =await _userManager.FindByEmailAsync(email);
            return Ok(new MeDto ()
            {
                DisplayName = user?.DisplayName??string.Empty,
                Email = user?.Email??string.Empty,
                Token = await _authService.CreateTokenAsync(user)
            });
        }
        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<Address>> GetUserAddress()
        {
            var user =await _userManager.FindUserWithAddressAsync(User);
            return Ok(user.Address);
        }
        [Authorize]
        [HttpPut("address")]
        public async Task<ActionResult<Address>> UpdateUserAddress(AddressDto address)
        {
            var updatedAddress = _mapper.Map<Address>(address);
            var user = await _userManager.FindUserWithAddressAsync(User);
            updatedAddress.Id = user.Address.Id;
            user.Address = updatedAddress;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ApiValidationErrorResponse() {Errors=result.Errors.Select(E=>E.Description) });
            return Ok(address);
        }
        [Authorize]
        [HttpPost("address")]
        public async Task<ActionResult<Address>> AddUserAddress(AddressDto address)
        {
            var updatedAddress = _mapper.Map<Address>(address);
            var user = await _userManager.FindUserWithAddressAsync(User);
            user.Address = updatedAddress;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ApiValidationErrorResponse() { Errors = result.Errors.Select(E => E.Description) });
            return Ok(address);
        }
        [HttpPost("forget-password")]
        public async Task<ActionResult> ForgetPassword(EmailDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordUrl = _configuration["ApiBaseUrl"] + Url.Action("ResetPassword", "Account", new { email = user.Email, token  });
                await _emailSender.SendMail(model.Email, "Reset Your Password", resetPasswordUrl);

            }
            return Ok(new { message = "the reset email will send to you if you registerd" });
        }
        [HttpPut("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetEmailPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                await Task.Delay(500);
                return Ok(new { message = "If the provided data is valid, your password has been reset." });
            }

            var result = await _userManager.ResetPasswordAsync(user,WebUtility.UrlDecode(model.Token), model.Password);
            if (!result.Succeeded)
                _logger.LogWarning($"Password reset failed for {model.Email}. Error: {string.Join(",", result.Errors.Select(E => E.Description))}");
            return Ok(new { message = "If the provided data is valid, your password has been reset." });
        }

        [HttpGet("external-login")]
        public ActionResult ExternalLogin(string provider, string? returnUrl = "/")
        {
            var redirecdUrl = _configuration["ApiBaseUrl"] + Url.Action("ExternalLoginCallback", "Account", returnUrl);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirecdUrl);
            return Challenge(properties, provider);
        }
        [HttpGet("external-login-callback")]
        public async Task<ActionResult<UserDto>> ExternalLoginCallback(string? returnUrl = "/")
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return Unauthorized(new ApiResponse(401, "Invalid Login"));
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            ApplicationUser user;
            if (result.Succeeded)
            {
                user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var displayName = info.Principal.FindFirstValue(ClaimTypes.Name);
                var phone = info.Principal.FindFirstValue(ClaimTypes.MobilePhone);
                user = new ApplicationUser()
                {
                    Email = email,
                    DisplayName = displayName,
                    UserName = email.Split("@")[0],
                    PhoneNumber = phone ?? "01xxxxxxxxx"
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                { return BadRequest(new ApiValidationErrorResponse() { Errors = createResult.Errors.Select(E => E.Description) }); }
                await _userManager.AddLoginAsync(user, info);
            }
            var auth = await _authService.AddRefreshTokenToUser(user.Id);
            if (auth is null)
                return Unauthorized(new ApiResponse(401, "Could not issue tokens"));
            Response.Cookies.Append("refreshToken", auth.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = auth.ExpireOfRefreshToken
            });
            return Ok(new UserDto() { DisplayName = user.DisplayName, Email = user.Email, JwtToken = auth.JwtToken, RefreshToken = auth.RefreshToken });
        }
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] string refreshToken)
        {
            var auth = await _authService.RefreshAsync(refreshToken);
            if (auth is null) return Unauthorized(new ApiResponse(401, "Could not issue tokens"));
            
            return Ok(auth);
        }
        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return Unauthorized(new ApiResponse(401));
            var success = await _authService.Revoke(refreshToken, userId); 
            if (!success) return BadRequest(new ApiResponse(400, "Could not revoke token"));
            return NoContent();
        }
        [Authorize]
        [HttpPost("revoke-all")]
        public async Task<IActionResult> RevokeAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users.Include(U=>U.RefreshTokens).FirstOrDefaultAsync(U=>U.Id==userId);
            if (user is null) return Unauthorized(new ApiResponse(401));
            var success = await _authService.RevokeAllTokensForUser(user);
            if (!success) return BadRequest(new ApiResponse(400, "Could not revoke tokens"));
            return NoContent();
        }

    }
}
