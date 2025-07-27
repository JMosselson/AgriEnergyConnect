// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services; // Ensure you have an implementation for this registered in Program.cs if you use it
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ST10270629_PROG7311_POE.Models; // Your models namespace

namespace ST10270629_PROG7311_POE.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender; // Make sure you have configured and registered an IEmailSender service

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        // ****************************************************
        // VV             ADD THIS PROPERTY BELOW             VV
        // ****************************************************
        /// <summary>
        ///     This property is used to capture the URL the user was trying to access before
        ///     being redirected here, so they can be sent back after resetting password/logging in.
        ///     It's bound from the query string on GET requests.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        // ****************************************************
        // ^^             ADD THIS PROPERTY ABOVE             ^^
        // ****************************************************

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // --- Optional: Handle the ReturnUrl if needed within the POST ---
            // If you need to pass ReturnUrl to the ResetPassword page via the email link,
            // you might need to include it in the 'values' anonymous object below.
            // However, typically the ResetPassword page itself might receive it independently
            // if the user was redirected there. Let's keep the standard logic for now.
            // string localReturnUrl = ReturnUrl ?? Url.Content("~/"); // Example: Default if null

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword", // Path to the ResetPassword page
                    pageHandler: null,
                    // VV Include 'returnUrl = localReturnUrl' if ResetPassword needs it VV
                    values: new { area = "Identity", code /*, returnUrl = localReturnUrl */ },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}