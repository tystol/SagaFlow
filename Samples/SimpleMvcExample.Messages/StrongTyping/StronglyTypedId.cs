using System.ComponentModel;

namespace SimpleMvcExample.Messages.StrongTyping
{
    /// <summary>
    /// Strong Typed ID plumbing.
    /// Source: https://thomaslevesque.com/2020/11/23/csharp-9-records-as-strongly-typed-ids-part-2-aspnet-core-route-and-query-parameters/
    /// </summary>
    [TypeConverter(typeof(StronglyTypedIdConverter))]
    public abstract record StronglyTypedId<TValue>(TValue Value) where TValue : notnull
    {
        public override string ToString() => Value.ToString();
    }
}