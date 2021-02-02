using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Operation time span attribute for correctly displaying the operation time format based on setup
    /// </summary>
    public class OperationDBTimeAttribute : PXDBTimeSpanLongAttribute
    {
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            Format = GetFormat(((AMBSetup)PXSelect<AMBSetup>.Select(sender.Graph))?.OperationTimeFormat);
        }

        protected virtual TimeSpanFormatType GetFormat(int? format)
        {
            return AMTimeFormatAttribute.GetFormat(format);
        }
    }

    /// <summary>
    /// Operation time span attribute for correctly displaying the operation time format based on setup
    /// (For unbound/non DB fields)
    /// </summary>
    public class OperationNonDBTimeAttribute : PXTimeSpanLongAttribute
    {
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            Format = GetFormat(((AMBSetup)PXSelect<AMBSetup>.Select(sender.Graph))?.OperationTimeFormat);
        }

        protected virtual TimeSpanFormatType GetFormat(int? format)
        {
            return AMTimeFormatAttribute.GetFormat(format);
        }
    }
}