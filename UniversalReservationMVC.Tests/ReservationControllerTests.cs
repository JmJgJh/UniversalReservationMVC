using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Microsoft.Extensions.Logging;
using UniversalReservationMVC.Controllers;
using UniversalReservationMVC.Extensions;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.ViewModels;
using Xunit;

namespace UniversalReservationMVC.Tests
{
    public class ReservationControllerTests
    {
        [Fact]
        public async Task Create_Post_HappyPath_RedirectsAndCallsService()
        {
            // Arrange
            var reservationService = new Mock<IReservationService>();
            var eventService = new Mock<IEventService>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var logger = Mock.Of<ILogger<ReservationController>>();

            reservationService
                .Setup(s => s.CreateReservationAsync(It.IsAny<Reservation>()))
                .ReturnsAsync((Reservation r) => { r.Id = 42; return r; });

            var controller = BuildController(reservationService, eventService, unitOfWork, logger, userId: "user-1");

            var vm = new ReservationCreateViewModel
            {
                ResourceId = 10,
                SeatId = 5,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            };
            ValidateModel(vm, controller);

            // Act
            var result = await controller.Create(vm);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyReservations", redirect.ActionName);
            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));

            reservationService.Verify(s => s.CreateReservationAsync(
                It.Is<Reservation>(r => r.ResourceId == vm.ResourceId && r.SeatId == vm.SeatId && r.UserId == "user-1")),
                Times.Once);
        }

        [Fact]
        public async Task Edit_Post_HappyPath_UpdatesAndRedirects()
        {
            // Arrange
            var reservationService = new Mock<IReservationService>();
            var eventService = new Mock<IEventService>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var logger = Mock.Of<ILogger<ReservationController>>();

            var existing = new Reservation
            {
                Id = 7,
                ResourceId = 3,
                SeatId = 4,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                UserId = "user-2"
            };

            reservationService.Setup(s => s.GetReservationByIdAsync(existing.Id)).ReturnsAsync(existing);
            reservationService.Setup(s => s.UpdateReservationAsync(It.IsAny<Reservation>())).ReturnsAsync(existing);

            var controller = BuildController(reservationService, eventService, unitOfWork, logger, userId: "user-2");

            var vm = new ReservationEditViewModel
            {
                Id = existing.Id,
                ResourceId = existing.ResourceId,
                SeatId = existing.SeatId,
                StartTime = existing.StartTime.AddHours(1),
                EndTime = existing.EndTime.AddHours(1)
            };
            ValidateModel(vm, controller);

            // Act
            var result = await controller.Edit(vm);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyReservations", redirect.ActionName);

            reservationService.Verify(s => s.UpdateReservationAsync(
                It.Is<Reservation>(r => r.Id == existing.Id && r.StartTime == vm.StartTime && r.EndTime == vm.EndTime)),
                Times.Once);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithErrors()
        {
            // Arrange
            var reservationService = new Mock<IReservationService>();
            var eventService = new Mock<IEventService>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var logger = Mock.Of<ILogger<ReservationController>>();

            // Mock resource/seat lookups used when re-rendering
            unitOfWork.Setup(u => u.Resources.GetByIdAsync(10)).ReturnsAsync(new Resource { Id = 10, Name = "Sala" });
            unitOfWork.Setup(u => u.Seats.GetByIdAsync(5)).ReturnsAsync(new Seat { Id = 5, Label = "A1" });

            var controller = BuildController(reservationService, eventService, unitOfWork, logger, userId: "user-1");

            var vm = new ReservationCreateViewModel
            {
                ResourceId = 10,
                SeatId = 5,
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(1) // invalid: end before start
            };

            ValidateModel(vm, controller);

            // Act
            var result = await controller.Create(vm);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(vm, view.Model);
            Assert.False(controller.ModelState.IsValid);
            reservationService.Verify(s => s.CreateReservationAsync(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var reservationService = new Mock<IReservationService>();
            var eventService = new Mock<IEventService>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var logger = Mock.Of<ILogger<ReservationController>>();

            var existing = new Reservation
            {
                Id = 9,
                ResourceId = 4,
                SeatId = 2,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                UserId = "user-3"
            };

            reservationService.Setup(s => s.GetReservationByIdAsync(existing.Id)).ReturnsAsync(existing);
            unitOfWork.Setup(u => u.Resources.GetByIdAsync(existing.ResourceId)).ReturnsAsync(new Resource { Id = existing.ResourceId, Name = "Sala" });
            unitOfWork.Setup(u => u.Seats.GetByIdAsync(existing.SeatId.Value)).ReturnsAsync(new Seat { Id = existing.SeatId.Value, Label = "B2" });

            var controller = BuildController(reservationService, eventService, unitOfWork, logger, userId: "user-3");

            var vm = new ReservationEditViewModel
            {
                Id = existing.Id,
                ResourceId = existing.ResourceId,
                SeatId = existing.SeatId,
                StartTime = DateTime.UtcNow.AddHours(3),
                EndTime = DateTime.UtcNow.AddHours(2) // invalid
            };

            ValidateModel(vm, controller);

            // Act
            var result = await controller.Edit(vm);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(vm, view.Model);
            Assert.False(controller.ModelState.IsValid);
            reservationService.Verify(s => s.UpdateReservationAsync(It.IsAny<Reservation>()), Times.Never);
        }

        private static ReservationController BuildController(
            Mock<IReservationService> reservationService,
            Mock<IEventService> eventService,
            Mock<IUnitOfWork> unitOfWork,
            ILogger<ReservationController> logger,
            string userId)
        {
            var controller = new ReservationController(reservationService.Object, eventService.Object, unitOfWork.Object, logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId)
                        }, "TestAuth"))
                    }
                },
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };

            return controller;
        }

        private static void ValidateModel(object model, Controller controller)
        {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            if (model is IValidatableObject validatable)
            {
                validationResults.AddRange(validatable.Validate(validationContext));
            }

            foreach (var validationResult in validationResults)
            {
                var memberName = validationResult.MemberNames.FirstOrDefault() ?? string.Empty;
                controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? string.Empty);
            }
        }
    }
}
