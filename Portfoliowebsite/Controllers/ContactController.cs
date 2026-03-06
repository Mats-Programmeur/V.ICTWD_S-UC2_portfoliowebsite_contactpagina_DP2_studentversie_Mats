using Microsoft.AspNetCore.Mvc;
using Portfoliowebsite.Services;
using System.Net.Mail;

namespace Portfoliowebsite.Controllers
{
    public class ContactController : Controller
    {
        private const int MaxNameLength = 100;
        private const int MaxEmailLength = 150;
        private const int MaxSubjectLength = 150;
        private const int MaxMessageLength = 2000;

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
        [RequestSizeLimit(16_384)]
        [RequestFormLimits(ValueLengthLimit = 4096)]
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
            else if (Name.Length > MaxNameLength)
            {
                errors["Name"] = $"Naam mag maximaal {MaxNameLength} tekens bevatten.";
            }
            else if (Name.Any(char.IsDigit))
            {
                errors["Name"] = "Naam mag geen cijfers bevatten.";
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                errors["Email"] = "Vul je e-mailadres in.";
            }
            else if (Email.Length > MaxEmailLength)
            {
                errors["Email"] = $"E-mail mag maximaal {MaxEmailLength} tekens bevatten.";
            }
            else if (!IsValidEmail(Email))
            {
                errors["Email"] = "Vul een geldig e-mailadres in.";
            }

            if (string.IsNullOrWhiteSpace(Subject))
            {
                errors["Subject"] = "Vul een onderwerp in.";
            }
            else if (Subject.Length > MaxSubjectLength)
            {
                errors["Subject"] = $"Onderwerp mag maximaal {MaxSubjectLength} tekens bevatten.";
            }

            if (string.IsNullOrWhiteSpace(Message))
            {
                errors["Message"] = "Vul een bericht in.";
            }
            else if (Message.Length > MaxMessageLength)
            {
                errors["Message"] = $"Bericht mag maximaal {MaxMessageLength} tekens bevatten.";
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
