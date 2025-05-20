using ContactApplication.Controllers;
using ContactApplication.Models;
using ContactApplication.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace TestProject1
{
    public class ContactControllerTests
    {
        [Fact]
        public async Task SubmitAsync_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var repoMock = new Mock<IContactFormRepository>();
            var controller = new TestableContactController(repoMock.Object, recaptchaResult: true);
            controller.ModelState.AddModelError("FirstName", "Required");
            var model = new ContactViewModel();

            // Act
            var result = await controller.SubmitAsync(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contact", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task SubmitAsync_InvalidRecaptcha_ReturnsViewWithError()
        {
            // Arrange
            var repoMock = new Mock<IContactFormRepository>();
            var controller = new TestableContactController(repoMock.Object, recaptchaResult: false);
            var model = new ContactViewModel
            {
                FirstName = "John",
                Email = "john@example.com",
                RecaptchaToken = "invalid"
            };

            // Act
            var result = await controller.SubmitAsync(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contact", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("reCAPTCHA validation failed.", controller.ViewBag.Error);
        }

        [Fact]
        public async Task SubmitAsync_ValidModel_CallsRepositoryAndReturnsSuccess()
        {
            // Arrange
            var repoMock = new Mock<IContactFormRepository>();
            repoMock.Setup(r => r.InsertContactForm(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.DateTime>()));

            var controller = new TestableContactController(repoMock.Object, recaptchaResult: true);
            var model = new ContactViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                RecaptchaToken = "valid"
            };

            // Act
            var result = await controller.SubmitAsync(model);

            // Assert
            repoMock.Verify(r => r.InsertContactForm(
                model.FirstName, model.LastName, model.Email, model.Phone,
                model.Zip, model.City, model.State, model.Comments, model.CreatedAt), Times.Once);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contact", viewResult.ViewName);
            Assert.IsType<ContactViewModel>(viewResult.Model);
            Assert.Equal($"Thank you, {model.FirstName} {model.LastName}. Your message has been received.", controller.ViewBag.Message);
        }

        private class TestableContactController : ContactController
        {
            private readonly bool _recaptchaResult;

            public TestableContactController(IContactFormRepository repo, bool recaptchaResult = true)
                : base(repo)
            {
                _recaptchaResult = recaptchaResult;
            }

            public override async Task<bool> VerifyRecaptchaAsync(string token)
            {
                await Task.Yield();
                return _recaptchaResult;
            }
        }
    }
}