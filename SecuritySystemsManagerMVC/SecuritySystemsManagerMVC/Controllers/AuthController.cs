using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Security.Claims;
using SecuritySystemsManager.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using SecuritySystemsManagerMVC.ViewModels;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Security;

namespace SecuritySystemsManagerMVC.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserService _usersService;
        private readonly IRoleService _rolesService;
        private readonly IMapper _mapper;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            IUserService usersService,
            IRoleService rolesService,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _usersService = usersService;
            _rolesService = rolesService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);
                
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    }
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account is locked. Please try again later.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, Constants.InvalidCredentials);
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account is locked. Please try again later.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RoleId = (int)RoleType.Client
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Add user to Client role
                    await _userManager.AddToRoleAsync(user, RoleType.Client.ToString());
                    
                    // Handle profile image
                    if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
                    {
                        string imageUrl = await _usersService.UploadUserProfileImageAsync(model.ProfileImageFile);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            user.ProfileImage = imageUrl;
                            await _userManager.UpdateAsync(user);
                        }
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    }
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult ConfirmLogout()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
