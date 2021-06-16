using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TennisBookings.Web.Data;
using TennisBookings.Web.External;
using TennisBookings.Web.IntegrationTests.Fakes;
using TennisBookings.Web.IntegrationTests.Helpers;
using TennisBookings.Web.Services;

namespace TennisBookings.Web.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        // Defines a unique in-memory Database root to be used with the "options.UseInMemoryDatabase"
        // to avoid the same in-memory store to be used by all itnegration tests (which might result
        // in problemswhen tests are run in parallel)
        private readonly InMemoryDatabaseRoot _dbRoot = new InMemoryDatabaseRoot();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IWeatherApiClient, FakeWithDataWeatherApiClient>();
                services.AddSingleton<IDateTime, FixedDateTime>();
            });
        }

        private void AddInMemoryDatabase(IServiceCollection services)
        {
            // Tries to find an existing DbContex registered
            var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<TennisBookingDbContext>));

            // If it was found, removes its registration
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Adds the "InMemory" database
            services.AddDbContext<TennisBookingDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting", databaseRoot: _dbRoot);
            });

            // Setting up some default data to be used in Tests

            // Gets a service provider
            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<TennisBookingDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                // Ensures the in-memory database is created and has the expected schema produced by
                // any migrations
                db.Database.EnsureCreated();

                try
                {
                    // Seed the database with some Test data
                    DatabaseHelper.InitialiseDbForTests(db);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the " +
                                        "database with test data. Error: {Message}", ex.Message);
                }
            }
        }
    }
}
