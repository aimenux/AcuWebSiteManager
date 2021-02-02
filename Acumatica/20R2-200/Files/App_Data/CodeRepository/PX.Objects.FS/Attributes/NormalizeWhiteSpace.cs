using PX.Data;
using System;
using System.Text.RegularExpressions;

namespace PX.Objects.FS
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]

    // PXEventSubscriberAttribute: Extends the BLC events associated to a DAC attribute which have the [NormalizeWhiteSpace] attribute.
    // IPXFieldUpdatingSubscriber: Allows to overwrite the FieldUpdating event inside the [NormalizeWhiteSpace] class.
    public class NormalizeWhiteSpace : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber
    {
        public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is string && !e.Cancel)
            {
                string strValue = (string)e.NewValue;
                int fieldLengthBeforeNormalize = strValue.Length;
                e.NewValue = Regex.Replace(strValue.Trim(), @"\s+", " ").PadRight(fieldLengthBeforeNormalize, ' ');
            }
        }
    }
}
