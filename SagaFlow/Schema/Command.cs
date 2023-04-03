using System;
using System.Collections.Generic;

namespace SagaFlow.Schema
{
    public class Command
    {
        public string Name { get; internal set; }
        public string DisplayName { get; internal set; }
        public Type CommandType { get; internal set; }
        public Type EventType { get; internal set; }
        public string RouteTemplate { get; internal set; }
        public IReadOnlyList<CommandParameter> Parameters { get; internal set; }
    }

    public class CommandParameter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Type InputType { get; set; }
    }
}
