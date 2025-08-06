using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SecuritySystemsManagerMVC.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserService _userService;
        private readonly UrlEncoder _urlEncoder;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserService userService,
            UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _urlEncoder = urlEncoder;
        }

        public async Task<IActionResult> MyProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Error404", "Error");
                }

                // Get profile data from service
                var profileData = await _userService.GetUserProfileDataAsync(user.Id);
                
                // Map to ViewModel
                var model = new UserProfileViewModel
                {
                    Username = profileData.Username,
                    Email = profileData.Email,
                    PhoneNumber = profileData.PhoneNumber,
                    FirstName = profileData.FirstName,
                    LastName = profileData.LastName,
                    ProfileImage = profileData.ProfileImage,
                    TwoFactorEnabled = profileData.TwoFactorEnabled,
                    HasAuthenticator = profileData.HasAuthenticator,
                    RecoveryCodesLeft = profileData.RecoveryCodesLeft,
                    CreatedAt = profileData.CreatedAt,
                    UpdatedAt = profileData.UpdatedAt,
                    LastLoginTime = profileData.LastLoginTime,
                    TotalOrders = profileData.TotalOrders,
                    TotalLocations = profileData.TotalLocations,
                    UserRole = profileData.UserRole
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        public async Task<IActionResult> EditProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Error404", "Error");
                }

                var model = new EditProfileViewModel
                {
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Error404", "Error");
                }

                // Use service to update profile
                var (success, errors) = await _userService.UpdateUserProfileAsync(
                    user.Id, 
                    model.Email, 
                    model.PhoneNumber, 
                    model.FirstName, 
                    model.LastName, 
                    model.ProfileImageFile);

                if (!success)
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View(model);
                }

                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Your profile has been updated successfully.";
                return RedirectToAction(nameof(MyProfile));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Възникна грешка при обновяването на профила. Моля, опитайте отново.");
                return View(model);
            }
        }

        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var hasPassword = await _userService.UserHasPasswordAsync(user.Id);
            if (!hasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Use service to change password
            var (success, errors) = await _userService.ChangeUserPasswordAsync(user.Id, model.OldPassword, model.NewPassword);
            
            if (!success)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Your password has been changed successfully.";
            return RedirectToAction(nameof(MyProfile));
        }

        public async Task<IActionResult> SetPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var hasPassword = await _userService.UserHasPasswordAsync(user.Id);
            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            return View(new SetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Use service to set password
            var (success, errors) = await _userService.SetUserPasswordAsync(user.Id, model.NewPassword);
            
            if (!success)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Your password has been set successfully.";
            return RedirectToAction(nameof(MyProfile));
        }

        public async Task<IActionResult> TwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Use service to get 2FA info
            var (hasAuthenticator, is2faEnabled, recoveryCodesLeft) = await _userService.GetTwoFactorAuthInfoAsync(user.Id);

            var model = new TwoFactorAuthenticationViewModel
            {
                HasAuthenticator = hasAuthenticator,
                Is2faEnabled = is2faEnabled,
                RecoveryCodesLeft = recoveryCodesLeft,
            };

            return View(model);
        }

        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Use service to get setup info
            var (sharedKey, authenticatorUri) = await _userService.GetTwoFactorSetupInfoAsync(user.Id);

            var model = new TwoFactorAuthenticationViewModel
            {
                SharedKey = sharedKey,
                AuthenticatorUri = authenticatorUri
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(TwoFactorAuthenticationViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid || string.IsNullOrEmpty(model.Code))
            {
                ModelState.AddModelError("Code", "Verification code is required.");
                var (sharedKey, authenticatorUri) = await _userService.GetTwoFactorSetupInfoAsync(user.Id);
                model.SharedKey = sharedKey;
                model.AuthenticatorUri = authenticatorUri;
                return View(model);
            }

            // Use service to enable 2FA
            var (success, errors, recoveryCodes) = await _userService.EnableTwoFactorAuthAsync(user.Id, model.Code);
            
            if (!success)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError("Code", error);
                }
                var (sharedKey, authenticatorUri) = await _userService.GetTwoFactorSetupInfoAsync(user.Id);
                model.SharedKey = sharedKey;
                model.AuthenticatorUri = authenticatorUri;
                TempData["Error"] = "Verification code is invalid. Please try again.";
                return View(model);
            }

            TempData["Success"] = "Your authenticator app has been verified and two-factor authentication has been enabled.";

            // Generate recovery codes for the user
            if (recoveryCodes != null && recoveryCodes.Length > 0)
            {
                var recoveryCodesString = string.Join(", ", recoveryCodes);
                TempData["RecoveryCodes"] = recoveryCodesString;
                TempData["Message"] = "Make sure to save these recovery codes in a safe place. You won't be able to see them again.";
            }

            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        [HttpGet]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var (hasAuthenticator, is2faEnabled, recoveryCodesLeft) = await _userService.GetTwoFactorAuthInfoAsync(user.Id);
            if (!is2faEnabled)
            {
                TempData["Error"] = "Cannot disable 2FA as it's not currently enabled.";
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2faPost()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Use service to disable 2FA
            var (success, errors) = await _userService.DisableTwoFactorAuthAsync(user.Id);
            
            if (!success)
            {
                foreach (var error in errors)
                {
                    TempData["Error"] = error;
                }
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            TempData["Success"] = "Two-factor authentication has been disabled.";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        [HttpGet]
        public async Task<IActionResult> ResetAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticatorPost()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Use service to reset authenticator
            var (success, errors) = await _userService.ResetAuthenticatorAsync(user.Id);
            
            if (!success)
            {
                foreach (var error in errors)
                {
                    TempData["Error"] = error;
                }
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            TempData["Success"] = "Your authenticator app key has been reset.";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }
    }
} 