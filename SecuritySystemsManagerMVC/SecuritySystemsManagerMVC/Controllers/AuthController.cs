using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.Security.Claims;
using SecuritySystemsManager.Shared;
using Microsoft.AspNetCore.Authentication;
using SecuritySystemsManagerMVC.ViewModels;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Security;

namespace SecuritySystemsManagerMVC.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IUserService usersService;
        private readonly IRoleService rolesService;
        private readonly IMapper mapper;

        public AuthController(IUserService usersService, IRoleService rolesService, IMapper mapper)
        {
            this.usersService = usersService;
            this.rolesService = rolesService;
            this.mapper = mapper;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginVm model)
        {
            string loggedUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            if (loggedUsername != null)
            {
                return Forbid();
            }

            if (!await this.usersService.CanUserLoginAsync(model.Username, model.Password))
            {
                ModelState.AddModelError(string.Empty, Constants.InvalidCredentials);
                return View(model);
            }
            await LoginUser(model.Username);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private async Task LoginUser(string username)
        {
            var user = await this.usersService.GetByUsernameAsync(username);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.Name),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principle = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principle);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterVm userCreateModel)
        {
            string loggedUsername = User.FindFirst(ClaimTypes.Name)?.Value;

            if (loggedUsername != null)
            {
                return Forbid();
            }

            if (await this.usersService.GetByUsernameAsync(userCreateModel.Username) != default)
            {
                ModelState.AddModelError(string.Empty, Constants.UserAlreadyExists);
                return View(userCreateModel);
            }

            var userDto = this.mapper.Map<UserDto>(userCreateModel);
            userDto.RoleId = (await rolesService.GetByNameIfExistsAsync(RoleType.Client.ToString())).Id;
            
            // Използваме сервисния метод за създаване на потребител с профилна снимка
            await this.usersService.CreateUserWithDetailsAsync(userDto, userCreateModel.Password, userCreateModel.ProfileImageFile);
            await LoginUser(userDto.Username);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            string loggedUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            if (loggedUsername != null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult ConfirmLogout()
        {
            return View();
        }
    }
}
