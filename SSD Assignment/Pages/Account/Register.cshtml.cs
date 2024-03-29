using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SSD_Assignment.Models;
using SSD_Assignment.Services;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SSD_Assignment.Pages.Account
{
    [ValidateAntiForgeryToken]
    public class RegisterModel : PageModel
    {
        public string FileName { get; set; }

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Display(Name = "Profile Picture")]
            [BindProperty]
            public IFormFile ProfilePic { get; set; }

            [Required]
            [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Please enter valid name.")]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Birth Date")]
            public DateTime BirthDate { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Please enter a valid password using only alphanumeric characters.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                var newuser = new ApplicationUser {
                    ProfilePic = "default.png",
                    Name = Input.Name,
                    BirthDate = Input.BirthDate,
                    UserName = Input.Email,
                    Email = Input.Email,

                };

                if (Input.ProfilePic != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Input.ProfilePic.FileName;
                    var uploadsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads/ProfilePics");
                    var uploadedfilePath = Path.Combine(uploadsDirectoryPath, fileName);

                    using (var fileStream = new FileStream(uploadedfilePath, FileMode.Create))
                    {
                        newuser.ProfilePic = fileName;
                        await Input.ProfilePic.CopyToAsync(fileStream);
                    }
                    FileName = fileName;
                }

                var result = await _userManager.CreateAsync(newuser, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newuser);
                    var callbackUrl = Url.EmailConfirmationLink(newuser.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(Input.Email, callbackUrl);

                    await _signInManager.SignInAsync(newuser, isPersistent: false);
                    return LocalRedirect(Url.GetLocalUrl(returnUrl));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
