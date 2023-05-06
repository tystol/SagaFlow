using System.ComponentModel;

namespace SimpleMvcExample.Messages
{
	[DisplayName("Start Task")]
	public record StartSimpleTask : ICommand
	{
		[Description("A simple message to send to the server.")]
		public string? Message { get; init; }
	}
}

