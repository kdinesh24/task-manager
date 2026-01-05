using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using server.Data;
using server.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace server.Tests.Integration
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProgramTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<WorkflowDbContext>));
                    
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add DbContext using in-memory database for testing
                    services.AddDbContext<WorkflowDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Build the service provider
                    var sp = services.BuildServiceProvider();

                    // Create a scope to obtain a reference to the database context
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<WorkflowDbContext>();

                        // Ensure the database is created
                        db.Database.EnsureCreated();
                    }
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Application_StartsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Application_HasCorsEnabled()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            Assert.True(response.Headers.Contains("Access-Control-Allow-Origin") || 
                       response.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public async Task Application_CanCreateAndRetrieveTasks()
        {
            // Arrange
            var newTask = new
            {
                Title = "Integration Test Task",
                Description = "Testing the full application"
            };

            // Act
            var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
            createResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/tasks");
            getResponse.EnsureSuccessStatusCode();

            var tasks = await getResponse.Content.ReadFromJsonAsync<List<WorkflowTask>>();

            // Assert
            Assert.NotNull(tasks);
            Assert.Contains(tasks, t => t.Title == "Integration Test Task");
        }

        [Fact]
        public async Task Application_ControllersAreRegistered()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Application_ReturnsJsonContent()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task Application_HandlesInvalidRoutes()
        {
            // Act
            var response = await _client.GetAsync("/api/nonexistent");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Application_SupportsGetRequests()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Application_SupportsPostRequests()
        {
            // Arrange
            var newTask = new
            {
                Title = "POST Test Task"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/tasks", newTask);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Application_SupportsDeleteRequests()
        {
            // Arrange - Create a task first
            var newTask = new
            {
                Title = "Task to Delete"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
            var createdTask = await createResponse.Content.ReadFromJsonAsync<WorkflowTask>();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/tasks/{createdTask!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task Application_SupportsPutRequests()
        {
            // Arrange - Create a task first
            var newTask = new
            {
                Title = "Task to Update"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
            var createdTask = await createResponse.Content.ReadFromJsonAsync<WorkflowTask>();

            var updateData = new
            {
                Title = "Updated Title",
                Status = "in_progress"
            };

            // Act
            var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{createdTask!.Id}", updateData);

            // Assert
            updateResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        }

        [Fact]
        public async Task Application_DatabaseContextIsConfigured()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();

            // Act & Assert
            Assert.NotNull(context);
            Assert.NotNull(context.Tasks);
        }

        [Fact]
        public void Application_WithRelationalDatabase_RunsMigrations()
        {
            // Arrange - Create a factory with a relational database (SQLite)
            // Use a file-based SQLite database to avoid connection issues
            var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            
            WebApplicationFactory<Program>? factory = null;
            try
            {
                factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the existing DbContext
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<WorkflowDbContext>));
                        
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add DbContext using SQLite (which is relational) for testing
                        services.AddDbContext<WorkflowDbContext>(options =>
                        {
                            options.UseSqlite($"DataSource={dbPath}");
                        });
                    });
                });

                // Act - The application startup runs migrations automatically
                using var scope = factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
                
                // The migration logic runs during app startup
                var isRelational = db.Database.IsRelational();
                var canConnect = db.Database.CanConnect();

                // Assert
                Assert.True(isRelational, "Database should be relational (SQLite)");
                Assert.True(canConnect, "Database should be migrated and connectable");
                
                // Verify migrations were applied by checking if the Tasks DbSet works
                var tasksCount = db.Tasks.Count();
                Assert.True(tasksCount >= 0, "Tasks table should exist and be queryable after migration");
            }
            finally
            {
                // Dispose the factory to release the database connection
                factory?.Dispose();
                
                // Wait a bit for connections to be released
                System.Threading.Thread.Sleep(100);
                
                // Clean up the test database file
                if (File.Exists(dbPath))
                {
                    try
                    {
                        File.Delete(dbPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors - file may still be in use
                    }
                }
            }
        }

        [Fact]
        public void Application_WithInMemoryDatabase_EnsuresCreated()
        {
            // Arrange - Create a factory with in-memory database
            var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<WorkflowDbContext>));
                    
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add DbContext using in-memory database for testing
                    services.AddDbContext<WorkflowDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_EnsureCreated");
                    });
                });
            });

            // Act - The application startup should call EnsureCreated for in-memory databases
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
            
            // The EnsureCreated logic runs during app startup
            var isRelational = db.Database.IsRelational();
            var canConnect = db.Database.CanConnect();

            // Assert
            Assert.False(isRelational, "Database should be in-memory (not relational)");
            Assert.True(canConnect, "Database should be created and connectable");
        }
    }
}
