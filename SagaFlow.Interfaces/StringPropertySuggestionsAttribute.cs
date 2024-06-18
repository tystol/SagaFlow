using System;

namespace SagaFlow.Interfaces
{
    /// <summary>
    /// Associates a Resource Provider to a text property for a message.  The resulting command form will render a data
    /// options list for the text input.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StringPropertySuggestionsAttribute : Attribute
    {
        /// <summary>
        /// Specifies the resource provider to use as the text suggestion source by type.
        /// </summary>
        public Type ResourceProviderType { get; set; }
        
        /// <summary>
        /// Specifies the resource provider to use as the text suggestion source by name. Useful when you want to keep
        /// you messages library loosely decoupled from your resources, if not then use the ResourceProviderType to
        /// specify the resource provider.
        /// </summary>
        public string ResourceProviderName { get; set; }

        public StringPropertySuggestionsAttribute()
        {
        }
    }
}