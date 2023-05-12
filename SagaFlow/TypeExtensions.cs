using System;
using System.Collections.Generic;
using System.Linq;

namespace SagaFlow
{
    internal static class TypeExtensions
    {
        public static Type GetUnderlyingTypeIfNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
        
        public static bool IsImplementationOfOpenGenericInterface(this Type type, Type openGenericType)
        {
            if (!openGenericType.IsGenericTypeDefinition)
                throw new ArgumentException("This method must be supplied an open generic type definition", nameof(openGenericType));

            return type
                .GetInterfaces()
                .Any(i => i.MatchesOpenGenericDefinition(openGenericType));
        }

        public static IEnumerable<Type> GetInterfacesOfOpenGenericInterface(this Type type, Type openGenericType)
        {
            if (!openGenericType.IsGenericTypeDefinition)
                throw new ArgumentException("This method must be supplied an open generic type definition", nameof(openGenericType));

            return type
                .GetInterfaces()
                .Where(i => i.MatchesOpenGenericDefinition(openGenericType));
        }

        private static bool MatchesOpenGenericDefinition(this Type type, Type openGenericType)
        {
            return type.IsGenericType && openGenericType.IsAssignableFrom(type.GetGenericTypeDefinition());
        }
    }
}
