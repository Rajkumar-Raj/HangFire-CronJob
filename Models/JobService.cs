using System;

namespace HangFire_WebAPI
{
    public class JobRequest
    {
        public string JobName { get; set; }
        public string MethodName { get; set; }
        public string CronExpression { get; set; } // Cron format for scheduling
        public string JobInput { get; set; } // Input parameters for the job (can be serialized JSON)
    }

    public class DynamicJobService
    {
        // Job method that accepts a string input
        public void ProcessJob(string jobInput)
        {
            File.AppendAllText("Sample2.txt",
                  $"Processing job with input: {jobInput} at {DateTime.Now}" + Environment.NewLine);
            
        }
    }


    public class JobService
    {
        public void LogMessage()
        { 

            File.AppendAllText("Sample3.txt",
                   $"Recurring job executed at {DateTime.Now}" + Environment.NewLine);
            //Console.WriteLine($"Recurring job executed at {DateTime.Now}");
        }
    }
}
