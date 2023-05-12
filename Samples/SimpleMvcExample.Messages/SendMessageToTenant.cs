using System;

namespace SimpleMvcExample.Messages
{
    public class SendMessageToTenant : ICommand
    {
        public Guid TenantId { get; init; }
        public string? Message { get; init; }
    }
}