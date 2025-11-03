using System;
using System.Linq;
using CrimeReportApp.Data;
using CrimeReportApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CrimeReportApp.Tests2
{
    [TestFixture]
    public class IntegrationTests
    {
        private ServiceProvider _serviceProvider;
        private ApplicationDbContext _context;

        [SetUp]
        public void Setup()
        {
            // Setup in-memory database for testing
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void Test_AddUserAndIncident_Integration()
        {
            // Arrange: create test user and incident
            _context.Users.Add(new ApplicationUser { UserName = "testuser" });
            _context.Incidents.Add(new Incident { Title = "Test Incident", Description = "Test Description" });

            // Act: save changes
            _context.SaveChanges();

            // Assert: verify incident was persisted
            var incident = _context.Incidents.FirstOrDefault(i => i.Title == "Test Incident");
            Assert.IsNotNull(incident, "Integration Test Failed: Incident not found in database.");
        }

        [Test]
        public void SampleTest_Pass()
        {
            Assert.Pass(); // Example simple test
        }
    }
}

