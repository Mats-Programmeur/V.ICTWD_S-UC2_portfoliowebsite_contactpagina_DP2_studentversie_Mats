using Microsoft.AspNetCore.Mvc;
using Portfoliowebsite.Services;
using System.Net.Mail;

namespace Portfoliowebsite.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailSender _email;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IEmailSender email, ILogger<ContactController> logger)
        {
            _email = email;
            _logger = logger;
        }

        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string Name, string Email, string Subject, string Message, string? website)
        {
            Name = Name?.Trim() ?? string.Empty;
            Email = Email?.Trim() ?? string.Empty;
            Subject = Subject?.Trim() ?? string.Empty;
            Message = Message?.Trim() ?? string.Empty;

            var errors = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(website))
            {
                errors["Form"] = "Je formulier kon niet worden verwerkt. Probeer het opnieuw.";
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors["Name"] = "Vul je naam in.";
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                errors["Email"] = "Vul je e-mailadres in.";
            }
            else if (!IsValidEmail(Email))
            {
                errors["Email"] = "Vul een geldig e-mailadres in.";
            }

            if (string.IsNullOrWhiteSpace(Subject))
            {
                errors["Subject"] = "Vul een onderwerp in.";
            }

            if (string.IsNullOrWhiteSpace(Message))
            {
                errors["Message"] = "Vul een bericht in.";
            }
            else if (Message.Length < 10)
            {
                errors["Message"] = "Je bericht moet minimaal 10 tekens bevatten.";
            }

            if (errors.Count > 0)
            {
                SetFormState(Name, Email, Subject, Message, errors);
                return View();
            }

            try
            {
                await _email.SendAsync(Name, Email, Subject, Message);

                TempData["ThanksName"] = Name;
                TempData["ThanksEmail"] = Email;
                TempData["ThanksSubject"] = Subject;

                return RedirectToAction(nameof(Thanks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verzenden van contactformulier.");
                errors["Form"] = "Er ging iets mis bij het verzenden. Controleer je invoer en probeer opnieuw.";
                SetFormState(Name, Email, Subject, Message, errors);
                return View();
            }
        }

        public IActionResult Thanks()
        {
            return View();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var parsed = new MailAddress(email);
                return parsed.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void SetFormState(string name, string email, string subject, string message, Dictionary<string, string> errors)
        {
            ViewData["Name"] = name;
            ViewData["Email"] = email;
            ViewData["Subject"] = subject;
            ViewData["Message"] = message;
            ViewData["Errors"] = errors;
        }
    }
}
