using System;
using System.Linq;
using System.Reflection;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace App
{
    public static class Extensions
    {
        public static string GetVersion(this Type type)
        {
            return type
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
        }

        public static void LogValidationFailures(this ILogger logger, ValidationResult result)
        {
            var errors = result.Errors;
            if (result.IsValid || !errors.Any()) return;
            foreach (var error in errors)
            {
                logger.LogError("Failed to validate [{property}]: {reason}", error.PropertyName, error.ErrorMessage);
            }
        }
    }
}
