using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PX.Objects.FS
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SkipSetExtensionVisibleInvisibleAttribute : Attribute { }

    public class DACHelper
    {
        public static List<string> GetFieldsName(Type DACType)
        {
            List<string> fieldList = new List<string>();
            foreach (PropertyInfo prop in DACType.GetProperties())
            {
                if (prop.GetCustomAttributes(true).Where(atr => atr is SkipSetExtensionVisibleInvisibleAttribute).Count() == 0)
                {
                    fieldList.Add(prop.Name);
                }
            }

            return fieldList;
        }

        public static void SetExtensionVisibleInvisible<DAC>(PXCache cache, PXRowSelectedEventArgs e,
                                                            bool isVisible, bool isGrid)
            where DAC : PXCacheExtension
        {
            foreach (string fieldName in DACHelper.GetFieldsName(typeof(DAC)))
            {
                PXUIFieldAttribute.SetVisible(cache, null, fieldName, isVisible);
            }
        }
    }
}
