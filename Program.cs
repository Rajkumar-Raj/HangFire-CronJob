using Hangfire;
using Hangfire.SqlServer;
using HangFire_WebAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Hangfire services

builder.Services.AddHangfire(configuration =>
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseDefaultTypeSerializer()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

// Add the Hangfire server
builder.Services.AddHangfireServer();

builder.Services.AddTransient<JobService>();
builder.Services.AddTransient<DynamicJobService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseAuthorization();

// Enable the Hangfire Dashboard for monitoring jobs
app.UseHangfireDashboard("/hangfire");

app.MapControllers();

app.Run();
