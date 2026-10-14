#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Services.Interface;

namespace NotifyMe.App.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUserStore<ApplicationUser> _userStore;

        private readonly IUserEmailStore<ApplicationUser> _emailStore;

        private readonly ICommonService _commonService;

        private readonly ICustomerService _customerService;

        private readonly ISubscriptionService _subscriptionService;

        private readonly IConfiguration _configuration;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ICommonService commonService,
            ICustomerService customerService,
            ISubscriptionService subscriptionService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _commonService = commonService;
            _customerService = customerService;
            _subscriptionService = subscriptionService;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email Required")]
            [EmailAddress(ErrorMessage = "Enter Email Format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Phone Required")]
            [Phone(ErrorMessage = "Enter Valid Phone With Country Code")]
            public string Phone { get; set; }

            [Required(ErrorMessage = "Password Required")]
            [DataType(DataType.Password, ErrorMessage = "Password Required")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$", ErrorMessage = "Password must contain at least 8 characters, including upper/lowercase, numbers and symbols")]
            public string Password { get; set; }

            [DataType(DataType.Password, ErrorMessage = "Password Required")]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (!_commonService.IsValidPhoneNumber(Input.Phone))
                {
                    TempData["ErrorMessage"] = "Invalid phone number. Please enter a valid number with country code";

                    return Page();
                }

                var existingEmail = await _userManager.FindByEmailAsync(Input.Email);

                if (existingEmail != null)
                {
                    ModelState.AddModelError(string.Empty, "This email address is already registered");

                    return Page();
                }

                var existingPhone = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == Input.Phone);

                if (existingPhone != null)
                {
                    ModelState.AddModelError(string.Empty, "This phone number is already associated with another account");

                    return Page();
                }

                var user = CreateUser();

                user.PhoneNumber = Input.Phone;

                user.Email = Input.Email;

                user.NotificationEmail = Input.Email;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);

                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    string phone = _configuration["SupportPhoneNumber"];

                    await _userManager.AddToRoleAsync(user, "Customer");

                    await _customerService.CreateCustomer(await _userManager.GetUserIdAsync(user), Input.Email, Input.Phone);

                    await _subscriptionService.SetUserSubscriptionFree(await _userManager.GetUserIdAsync(user));

                    TempData["SuccessMessage"] = $"Account created! You're on a Free Trial. To renew or activate permanently, please contact the System Admin on WhatsApp: {phone}";

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Customer");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
