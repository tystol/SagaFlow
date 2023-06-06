using SimpleMvcExample.Messages.StrongTyping;

namespace SimpleMvcExample.Messages
{
    public record DatabaseServerId(string Value) : StronglyTypedId<string>(Value);
}