using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Logging;
using UniversalReservationMVC.Common;
using UniversalReservationMVC.Controllers;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.ViewModels;
using Xunit;

namespace UniversalReservationMVC.Tests
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task Register_OwnerWithoutCompanyName_AddsModelErrorAndReturnsView()
        {
            // Arrange
            var controller = CreateController(out var userManagerMock, out var signInManagerMock, out var companyServiceMock);
            var model = BuildValidRegisterModel(accountType: "owner", companyName: " ");
            ValidateModel(model, controller);

            // Act
            var result = await controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.True(controller.ModelState.ContainsKey("CompanyName"));
            userManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            companyServiceMock.Verify(m => m.CreateCompanyAsync(It.IsAny<Company>()), Times.Never);
            signInManagerMock.Verify(m => m.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), null), Times.Never);
        }

        [Fact]
        public async Task Register_UserAccount_CreatesUserAndRedirectsHome()
        {
            // Arrange
            var controller = CreateController(out var userManagerMock, out var signInManagerMock, out var companyServiceMock);
            var model = BuildValidRegisterModel(accountType: "user");
            ValidateModel(model, controller);

            userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppConstants.Roles.User))
                .ReturnsAsync(IdentityResult.Success);
            signInManagerMock.Setup(m => m.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.Register(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(HomeController.Index), redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            companyServiceMock.Verify(m => m.CreateCompanyAsync(It.IsAny<Company>()), Times.Never);
            userManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), model.Password), Times.Once);
            userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppConstants.Roles.User), Times.Once);
            signInManagerMock.Verify(m => m.SignInAsync(It.IsAny<ApplicationUser>(), false, null), Times.Once);
        }

        private static AccountController CreateController(
            out Mock<UserManager<ApplicationUser>> userManagerMock,
            out Mock<SignInManager<ApplicationUser>> signInManagerMock,
            out Mock<ICompanyService> companyServiceMock)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            signInManagerMock = new Mock<SignInManager<ApplicationUser>>(userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null!, null!, null!, null!);
            companyServiceMock = new Mock<ICompanyService>();
            var loggerMock = new Mock<ILogger<AccountController>>();

            var controller = new AccountController(signInManagerMock.Object, userManagerMock.Object, companyServiceMock.Object, loggerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return controller;
        }

        private static RegisterViewModel BuildValidRegisterModel(string accountType, string? companyName = null)
        {
            return new RegisterViewModel
            {
                FirstName = "Jan",
                LastName = "Kowalski",
                Email = "jan@example.com",
                Password = "Haslo123!",
                ConfirmPassword = "Haslo123!",
                AccountType = accountType,
                CompanyName = companyName
            };
        }

        private static void ValidateModel(object model, Controller controller)
        {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, true);

            foreach (var validationResult in validationResults)
            {
                var memberName = validationResult.MemberNames.FirstOrDefault() ?? string.Empty;
                controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? string.Empty);
            }
        }
    }
}
