using System;
using System.Collections.Generic;

namespace SagaFlow.Schema
{
    public class Command
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string CommandNameTemplate { get; internal set; }
        public string Description { get; internal set; }
        public Type CommandType { get; internal set; }
        public Type EventType { get; internal set; }
        public string RouteTemplate { get; internal set; }
        public IReadOnlyList<CommandParameter> Parameters { get; internal set; }
        public string CronExpression { get; internal set; }
        public bool IsRecurringCommand => CronExpression != null;
    }

    public class CommandParameter
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Type InputType { get; set; }
        public ResourceProvider ResourceProvider { get; set; }
    }
}
