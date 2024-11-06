using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace HangFire_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly JobService _jobService;
        private readonly DynamicJobService _dynamicJobService;

        public JobController(IRecurringJobManager recurringJobManager, JobService jobService, DynamicJobService dynamicJobService)
        {
            _recurringJobManager = recurringJobManager;
            _jobService = jobService;
            _dynamicJobService = dynamicJobService;
        }

        /// <summary>
        /// Static
        /// </summary>
        /// <returns></returns>
        [HttpPost("start-recurring-job")]
        public IActionResult StartRecurringJob()
        {
            _recurringJobManager.AddOrUpdate("LogMessage", () => _jobService.LogMessage(), Cron.Minutely);

            return Ok("Recurring job scheduled Sucessfully");
        }

        /// <summary>
        /// Dynamic
        /// </summary>
        /// <param name="jobRequest"></param>
        /// <returns></returns>
        [HttpPost("schedule-job")]
        public IActionResult ScheduleJob([FromBody] JobRequest jobRequest)
        {
            try
            {
                // Check if a valid cron expression is provided
                if (string.IsNullOrEmpty(jobRequest.CronExpression))
                {
                    return BadRequest("Cron expression is required.");
                }

                // Get the method from the DynamicJobService class
                var methodInfo = typeof(DynamicJobService).GetMethod(jobRequest.MethodName);
                if (methodInfo == null)
                {
                    return BadRequest($"Method '{jobRequest.MethodName}' not found in DynamicJobService.");
                }

                // Determine the method to execute based on the method name provided
                switch (jobRequest.MethodName)
                {
                    case "ProcessJob":
                        // Deserialize the input and schedule the job
                        var jobInput = JsonConvert.DeserializeObject<string>(jobRequest.JobInput);
                        _recurringJobManager.AddOrUpdate(
                            jobRequest.JobName,
                            () => _dynamicJobService.ProcessJob(jobInput), // Direct method invocation
                            jobRequest.CronExpression
                        );
                        break;

                    default:
                        return BadRequest($"Method '{jobRequest.MethodName}' is not recognized.");
                }

                // Prepare the input for the method (assuming the input is serialized JSON)
                //var parameters = new object[] { JsonConvert.DeserializeObject(jobRequest.JobInput, methodInfo.GetParameters().FirstOrDefault()?.ParameterType) };

                // Schedule the job dynamically
                //_recurringJobManager.AddOrUpdate(
                //    jobRequest.JobName,  // Unique job identifier
                //    () => methodInfo.Invoke(_dynamicJobService, parameters), // Invoke the dynamic method
                //    jobRequest.CronExpression // Cron schedule
                //);

                return Ok($"Job '{jobRequest.JobName}' scheduled successfully with the cron expression '{jobRequest.CronExpression}'.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error scheduling job: {ex.Message}");
            }
        }

    }
}
