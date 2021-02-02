using System;
using PX.Data;

namespace PX.Objects.AM.Reports
{
    public static class ReportHelper
    {
        public static string GetDacFieldNameString<TField>() where TField : class, IBqlField
        {
            var fieldName = typeof(TField).Name.ToCapitalized();
            var dacName = typeof(TField).DeclaringType.Name;

            return $"{dacName}.{fieldName}";
        }
    }
}
