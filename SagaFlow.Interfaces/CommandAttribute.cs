using System;

namespace SagaFlow.Interfaces
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name)
        {
            Name = name;
        }
        
        /// <summary>
        /// A user-friendly name for the command/job.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Optional property if this command/job should run on a periodic schedule, specify the cron expression when the job
        /// should run.
        /// </summary>
        public string Cron { get; set; }
    }
}