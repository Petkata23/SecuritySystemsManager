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

            var email = await _userManager.GetEmailAsync(user);
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    foreach (var error in setPhoneResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            // Update additional fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            // Handle profile image
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                string imageUrl = await _userService.UploadUserProfileImageAsync(model.ProfileImageFile);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    user.ProfileImage = imageUrl;
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
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

            var hasPassword = await _userManager.HasPasswordAsync(user);
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

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
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

            var hasPassword = await _userManager.HasPasswordAsync(user);
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

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
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

            var model = new TwoFactorAuthenticationViewModel
            {
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
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

            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(authenticatorKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var model = new TwoFactorAuthenticationViewModel
            {
                SharedKey = FormatKey(authenticatorKey),
                AuthenticatorUri = GenerateQrCodeUri(user.Email, authenticatorKey)
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
                var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
                model.SharedKey = FormatKey(authenticatorKey);
                model.AuthenticatorUri = GenerateQrCodeUri(user.Email, authenticatorKey);
                return View(model);
            }

            // Strip spaces and hyphens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Code", "Verification code is invalid.");
                var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
                model.SharedKey = FormatKey(authenticatorKey);
                model.AuthenticatorUri = GenerateQrCodeUri(user.Email, authenticatorKey);
                TempData["Error"] = "Verification code is invalid. Please try again.";
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            
            TempData["Success"] = "Your authenticator app has been verified and two-factor authentication has been enabled.";

            // Generate recovery codes for the user
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            if (recoveryCodes != null)
            {
                var recoveryCodesString = string.Join(", ", recoveryCodes);
                TempData["RecoveryCodes"] = recoveryCodesString;
                TempData["Message"] = "Make sure to save these recovery codes in a safe place. You won't be able to see them again.";
            }

            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        // Helper method to format the authenticator key
        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        // Helper method to generate QR code URI
        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                _urlEncoder.Encode("Security Systems Manager"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        [HttpGet]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
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

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{user.Id}'.");
            }

            TempData["Success"] = "2FA has been disabled. You can re-enable 2FA when you setup an authenticator app.";
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

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";
            return RedirectToAction(nameof(EnableAuthenticator));
        }
    }
} 