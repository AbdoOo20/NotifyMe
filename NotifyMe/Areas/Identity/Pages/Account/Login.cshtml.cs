#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Services.Interface;

namespace NotifyMe.App.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ICustomerService _customerService;

        private readonly IConfiguration _configuration;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ICustomerService customerService, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _customerService = customerService;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email Required")]
            [EmailAddress(ErrorMessage = "Enter Email Format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password Required")]
            [DataType(DataType.Password, ErrorMessage = "Password Required")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$", ErrorMessage = "Password must contain at least 8 characters, including upper/lowercase, numbers and symbols")]
            public string Password { get; set; }

            [Display(Name = "Remember Me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    string phone = _configuration["SupportPhoneNumber"];

                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    if (await _userManager.IsInRoleAsync(user, "Customer"))
                    {
                        var customer = await _customerService.GetCustomer(user.Id);

                        if (customer.IsActive)
                        {
                            TempData["SuccessMessage"] = "Login Successfully";

                            user.IsLogin = true;

                            await _userManager.UpdateAsync(user);

                            return RedirectToAction("Index", "Customer");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = $"Your account is not active yet. Please contact the administrator to approve it via WhatsApp at {phone}";

                            await _signInManager.SignOutAsync();

                            return RedirectToPage("/Account/Login");
                        }
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        TempData["SuccessMessage"] = "Login Successfully";

                        return RedirectToAction("Index", "Admin");
                    }

                    return RedirectToPage("/Account/Login");
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");

                    return Page();
                }
            }

            return Page();
        }
    }
}
