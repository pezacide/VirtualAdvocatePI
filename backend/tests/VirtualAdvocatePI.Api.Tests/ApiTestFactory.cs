using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VirtualAdvocatePI.Api.Auth;
using VirtualAdvocatePI.Api.Data;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(
            "DATABASE_CONNECTION_STRING",
            "Host=localhost;Port=5432;Database=vapi_test;Username=test;Password=test;SSL Mode=Disable");

        Environment.SetEnvironmentVariable("FIREBASE_PROJECT_ID", "dva-sop-dev");
        Environment.SetEnvironmentVariable("EVIDENCE_BUCKET_NAME", "test-evidence-bucket");

        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<VirtualAdvocateDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<VirtualAdvocateDbContext>>();
            services.AddSingleton(new DbContextOptionsBuilder<VirtualAdvocateDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options);

            services.RemoveAll<IFirebaseAuthService>();
            services.AddSingleton<IFirebaseAuthService, FakeFirebaseAuthService>();
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {FakeFirebaseAuthService.TestBearerToken}");
        return client;
    }
}
