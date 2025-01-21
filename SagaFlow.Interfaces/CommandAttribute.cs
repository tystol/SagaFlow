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
        
        public string Description { get; set; }
        
        /// <summary>
        /// Optional property if this command/job should run on a periodic schedule, specify the cron expression when the job
        /// should run.
        /// </summary>
        public string Cron { get; set; }
        
        // TODO: Rename NameTemplate to Summary? ie. a command has a Name (non-templatable) and a Summary (which is templatable) 
        /// <summary>
        /// Optional property template to resolve a running name of a command. Use {CommandProperty} syntax to define
        /// the command's values to inject into the final string.
        /// </summary>
        /// <example>Hello, my name is {Name} and I'm turning the server {RequestedPowerState}.</example>
        public string NameTemplate { get; set; }
    }
}