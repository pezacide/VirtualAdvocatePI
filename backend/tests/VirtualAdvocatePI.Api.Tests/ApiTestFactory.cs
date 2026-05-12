using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(
            "DATABASE_CONNECTION_STRING",
            "Host=localhost;Port=5432;Database=vapi_test;Username=test;Password=test;SSL Mode=Disable");

        Environment.SetEnvironmentVariable("FIREBASE_PROJECT_ID", "dva-sop-dev");
        Environment.SetEnvironmentVariable("EVIDENCE_BUCKET_NAME", "test-evidence-bucket");

        builder.UseEnvironment("Testing");
    }
}
