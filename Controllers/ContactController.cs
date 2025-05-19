using Microsoft.AspNetCore.Mvc;

namespace ContactApplication.Controllers
{
    using ContactApplication.Models;
    using ContactApplication.Repositories;
    using log4net;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using System.Threading.Tasks;

    public class ContactController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ContactController));
        private readonly ContactFormRepository _repository;

        public ContactController(ContactFormRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Contact()
        {
            log.Info("Contact called (GET)");
            return View(new ContactViewModel());
        }

        [EnableRateLimiting("ContactFormPolicy")]
        [HttpPost]
        public async Task<IActionResult> SubmitAsync(ContactViewModel model)
        {
            log.Info("Submit called with contact form data(POST)");
            if (!ModelState.IsValid) 
            {
                log.Warn("Contact form submission failed validation.");
                return View("Contact", model);
            }
            // Verify reCAPTCHA
            if (string.IsNullOrEmpty(model.RecaptchaToken) || !await VerifyRecaptchaAsync(model.RecaptchaToken))
            {
                log.Error("Recaptcha token is missing.");
                ViewBag.Error = $"reCAPTCHA validation failed.";                
                return View("Contact", model);
            }

            // Save to database 
            _repository.InsertContactForm(
                 model.FirstName, model.LastName, model.Email, model.Phone,
                 model.Zip, model.City, model.State, model.Comments,model.CreatedAt
             );

            ViewBag.Message = $"Thank you, {model.FirstName} {model.LastName}. Your message has been received.";
            return View("Contact", new ContactViewModel());
        }

        public async Task<bool> VerifyRecaptchaAsync(string token)
        {
            var secretKey = await AwsSecretsHelper.GetSecretValueAsync("contactformcredentials", "us-east-2", "SECRET_KEY");

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Recaptcha secret key is not configured.");
            }
            try
            {
                var client = new HttpClient();
                var response = client.PostAsync("https://www.google.com/recaptcha/api/siteverify",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                    { "secret", secretKey },
                    { "response", token }
                    })).Result;

                var result = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(result))
                {
                    log.Error("Recaptcha verification response is empty.");
                    throw new InvalidOperationException("Recaptcha verification response is null or empty.");
                }

                dynamic? json = JsonConvert.DeserializeObject(result);
                if (json == null)
                {
                    log.Error("Failed to deserialize Recaptcha verification response.");
                    throw new InvalidOperationException("Failed to deserialize Recaptcha verification response.");
                }
                bool success = json.success;
                double score = json.score;
                log.Info($"Recaptcha success: {success}, score: {score}");

                return json.success == true && json.score >= 0.9;


            }
            catch (Exception ex)
            {
                log.Error("Exception occurred during reCAPTCHA verification.", ex);
                throw;
            }
        }
    }

}
