# HangFire-CronJob - Creating a Hangfire recurring job in .NET 8 

**Step 1:** Set Up the Project
Create a .NET 8 Project: Open your IDE and create a .NET 8 Web API project.
Install Hangfire Packages: Run the following commands to add Hangfire packages via the .NET CLI or the Package Manager Console:

````
dotnet add package Hangfire
dotnet add package Hangfire.AspNetCore
````
**Step 2:** Configure Hangfire in Program.cs
Since .NET 8 projects typically use a single Program.cs file, you’ll add configuration there.

Configure Hangfire Services: Set up Hangfire to use a persistent storage provider (e.g., SQL Server) and configure it to run jobs in the background.

````
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services with SQL Server storage (replace connection string)
builder.Services.AddHangfire(configuration => 
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseDefaultTypeSerializer()
        .UseSqlServerStorage("YourConnectionStringHere", new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            UsePageLocksOnDequeue = true,
            DisableGlobalLocks = true
        }));

// Add the Hangfire server
builder.Services.AddHangfireServer();

// Add controllers to enable API endpoints
builder.Services.AddControllers();

var app = builder.Build();

// Enable the Hangfire Dashboard for monitoring jobs
app.UseHangfireDashboard("/hangfire");

app.MapControllers();

app.Run();
````

**Step 3:** Create a Background Job Service
Define the method you want to run on a recurring basis in a separate service class.

Create the Job Service: Create a class, e.g., JobService, with a method for the recurring task.

````
public class JobService
{
    public void LogMessage()
    {
        Console.WriteLine($"Recurring job executed at {DateTime.Now}");
    }
}
````
Register the Service in DI Container: In Program.cs, register this service so it can be injected into the controller.

````
builder.Services.AddTransient<JobService>();
````
**Step 4:** Create a Controller to Trigger the Recurring Job Setup
Create the Controller: Add a new API controller, e.g., JobController. In this controller, inject both IRecurringJobManager (to schedule recurring jobs) and JobService (for the job logic). Use an endpoint to start the recurring job when called.

````
using Microsoft.AspNetCore.Mvc;
using Hangfire;

[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly JobService _jobService;

    public JobController(IRecurringJobManager recurringJobManager, JobService jobService)
    {
        _recurringJobManager = recurringJobManager;
        _jobService = jobService;
    }

    // Endpoint to start the recurring job
    [HttpPost("start-recurring-job")]
    public IActionResult StartRecurringJob()
    {
        _recurringJobManager.AddOrUpdate(
            "LogMessageJob",                       // Job ID
            () => _jobService.LogMessage(),        // Job action
            Cron.Minutely                          // Cron schedule (every minute)
        );

        return Ok("Recurring job scheduled successfully.");
    }
}
````
The Cron.Minutely expression sets the job to run every minute. You can adjust this with other expressions like Cron.Daily() or Cron.Hourly().

**Step 5:** Test the Setup
Start the Application: Run the .NET 8 application.

Trigger the Job Setup via API: Use a tool like Postman or curl to call the endpoint and start the recurring job.


POST http://localhost:5000/api/job/start-recurring-job
Check Hangfire Dashboard: Go to http://localhost:5000/hangfire to see the job scheduled under Recurring Jobs.

Verify Logs: Check the console output to confirm that the job runs as scheduled.

Additional Tips
Dependency Injection for JobService: By registering JobService in DI, you ensure that any dependencies it requires can also be injected and managed properly.
Error Handling: Hangfire provides retry logic and error handling, configurable through the Hangfire Dashboard.

Here are some standard Cron expressions:
````
Cron.Daily() - Runs daily at midnight.
Cron.Hourly() - Runs hourly.
Cron.Minutely() - Runs every minute.
Cron.Weekly() - Runs weekly.
Cron.Monthly() - Runs monthly.


*/1 * * * * // Every minute
````

Create Dynamic Job
````
POST http://localhost:5000/api/job/schedule-job
Content-Type: application/json

{
  "JobName": "TestJob1",
  "MethodName": "ProcessJob",
  "CronExpression": "*/1 * * * *",
  "JobInput": "\"Hello, this is a dynamic job!\""
}
````


Using this controller-based approach, you can programmatically control the setup and scheduling of Hangfire jobs in .NET 8.



Reference: https://dev.to/chinonsoike/background-job-scheduling-in-net-using-hangfire-3ehm
