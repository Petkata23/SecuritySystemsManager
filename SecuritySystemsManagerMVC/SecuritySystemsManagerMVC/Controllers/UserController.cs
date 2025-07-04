using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Enums;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System.Diagnostics;

namespace SecuritySystemsManagerMVC.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class UserController : BaseCrudController<UserDto, IUserRepository, IUserService, UserEditVm, UserDetailsVm>
    {
        protected readonly IRoleService _roleService;

        public UserController(IMapper mapper, IUserService service, IRoleService roleService)
            : base(service, mapper)
        {
            _roleService = roleService;
        }

        protected override async Task<UserEditVm> PrePopulateVMAsync(UserEditVm editVM)
        {
            editVM.AvailableRoles = (await _roleService.GetAllAsync())
                .Select(r => new SelectListItem(r.Name, r.Id.ToString()));
            
            return editVM;
        }
        
        [HttpPost]
        public override async Task<IActionResult> Create(UserEditVm editVM)
        {
            // Display validation errors for debugging
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                    {
                        TempData[$"Error_{key}"] = error.ErrorMessage;
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check the form for errors.";
                editVM = await PrePopulateVMAsync(editVM);
                return View(editVM);
            }

            try
            {
                // Get password from form
                string password = Request.Form["Password"];
                string confirmPassword = Request.Form["ConfirmPassword"];
                
                // Validate passwords
                if (string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("Password", "Password is required");
                    editVM = await PrePopulateVMAsync(editVM);
                    return View(editVM);
                }
                
                if (password != confirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
                    editVM = await PrePopulateVMAsync(editVM);
                    return View(editVM);
                }

                // Map to DTO
                var userDto = _mapper.Map<UserDto>(editVM);
                
                // Get profile image if provided
                var profileImage = Request.Form.Files.GetFile("profileImageFile");
                
                // Use the service to create the user with all details
                await _service.CreateUserWithDetailsAsync(userDto, password, profileImage);

                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating user: {ex.Message}";
                ModelState.AddModelError("", ex.Message);
                editVM = await PrePopulateVMAsync(editVM);
                return View(editVM);
            }
        }
        
        [HttpPost]
        public override async Task<IActionResult> Edit(int id, UserEditVm editVM)
        {
            if (!ModelState.IsValid)
            {
                editVM = await PrePopulateVMAsync(editVM);
                return View(editVM);
            }
            
            try
            {
                // Get password from form (optional for edit)
                string password = Request.Form["Password"];
                string confirmPassword = Request.Form["ConfirmPassword"];
                
                // If password is provided, validate it matches confirmation
                if (!string.IsNullOrEmpty(password))
                {
                    if (password != confirmPassword)
                    {
                        ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
                        editVM = await PrePopulateVMAsync(editVM);
                        return View(editVM);
                    }
                }
                else
                {
                    // No password change requested
                    password = null;
                }
                
                // Map to DTO
                var userDto = _mapper.Map<UserDto>(editVM);
                
                // Get profile image if provided
                var profileImage = Request.Form.Files.GetFile("profileImageFile");
                if (profileImage != null && profileImage.Length > 0)
                {
                    // Upload profile image
                    string imagePath = await _service.UploadUserProfileImageAsync(profileImage);
                    userDto.ProfileImage = imagePath;
                }
                
                // Update user with optional password change
                await _service.UpdateUserWithPasswordAsync(userDto, password);
                
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
                ModelState.AddModelError("", ex.Message);
                editVM = await PrePopulateVMAsync(editVM);
                return View(editVM);
            }
        }
    }
} 