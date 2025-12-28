using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using UniversalReservationMVC.Controllers;
using UniversalReservationMVC.Models;
using UniversalReservationMVC.Repositories;
using UniversalReservationMVC.Services;
using UniversalReservationMVC.ViewModels;
using Xunit;

namespace UniversalReservationMVC.Tests
{
    public class CompanyControllerTests
    {
        [Fact]
        public async Task Reports_ReturnsViewWithCalculatedMetrics()
        {
            // Arrange
            var companyService = new Mock<ICompanyService>();
            var companyMemberService = new Mock<ICompanyMemberService>();
            var seatMapService = new Mock<ISeatMapService>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var logger = Mock.Of<ILogger<CompanyController>>();

            var company = new Company
            {
                Id = 1,
                OwnerId = "owner-1",
                Name = "Test Co",
                Resources = new List<Resource>
                {
                    new Resource
                    {
                        Id = 10,
                        Name = "Sala A",
                        ResourceType = ResourceType.ConferenceRoom,
                        Seats = new List<Seat>
                        {
                            new Seat { Id = 1 },
                            new Seat { Id = 2 }
                        }
                    }
                }
            };

            companyService.Setup(s => s.GetCompanyByOwnerAsync("owner-1")).ReturnsAsync(company);

            var reservations = new List<Reservation>
            {
                new Reservation { ResourceId = 10, Status = ReservationStatus.Confirmed, StartTime = DateTime.UtcNow.AddDays(1) },
                new Reservation { ResourceId = 10, Status = ReservationStatus.Confirmed, StartTime = DateTime.UtcNow.AddDays(-1) },
                new Reservation { ResourceId = 10, Status = ReservationStatus.Pending, StartTime = DateTime.UtcNow.AddDays(2) }
            };

            unitOfWork.Setup(u => u.Reservations.GetByResourceIdAsync(10, null, null)).ReturnsAsync(reservations);

            var controller = BuildController(companyService, companyMemberService, seatMapService, unitOfWork, logger, userId: "owner-1");

            // Act
            var result = await controller.Reports(null, null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<CompanyReportViewModel>(view.Model);
            Assert.Equal(1, vm.TotalResources);
            Assert.Equal(2, vm.TotalSeats);
            Assert.Equal(3, vm.TotalReservations);
            Assert.Equal(2, vm.UpcomingReservations);
            Assert.True(vm.OccupancyRate > 0);
            Assert.Single(vm.Resources);
            Assert.Equal("Sala A", vm.Resources[0].Name);
        }

        private static CompanyController BuildController(
            Mock<ICompanyService> companyService,
            Mock<ICompanyMemberService> companyMemberService,
            Mock<ISeatMapService> seatMapService,
            Mock<IUnitOfWork> unitOfWork,
            ILogger<CompanyController> logger,
            string userId)
        {
            var controller = new CompanyController(
                companyService.Object,
                seatMapService.Object,
                companyMemberService.Object,
                unitOfWork.Object,
                logger)
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
    }
}
