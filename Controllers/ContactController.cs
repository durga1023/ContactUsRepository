using Microsoft.AspNetCore.Mvc;

namespace ContactApplication.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using ContactApplication.Models;
    using ContactApplication.Repositories;

    public class ContactController : Controller
    {
        private readonly ContactFormRepository _repository;

        public ContactController(ContactFormRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [EnableRateLimiting("ContactFormPolicy")]
        [HttpPost]
        public async Task<IActionResult> SubmitAsync(ContactViewModel model)
        {
            if (!ModelState.IsValid) 
            {
                return View("Contact", model);
            }
            // Verify reCAPTCHA
            if (string.IsNullOrEmpty(model.RecaptchaToken) || !await VerifyRecaptchaAsync(model.RecaptchaToken))
            {
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
            var secretJson = await AwsSecretsHelper.GetSecretAsync("contactformcredentials");
            var secretData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(secretJson);
            var secretKey = secretData["SECRET_KEY"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Recaptcha secret key is not configured.");
            }

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
                throw new InvalidOperationException("Recaptcha verification response is null or empty.");
            }

            dynamic? json = JsonConvert.DeserializeObject(result);
            if (json == null)
            {
                throw new InvalidOperationException("Failed to deserialize Recaptcha verification response.");
            }

            return json.success == true && json.score >= 0.5;
        }
    }

}
