using System;

namespace PX.Objects.PJ.Common.Descriptor.Attributes
{
    public class DefaultWorkingTimeStartAttribute : DefaultWorkingTimeAttribute
    {
        protected override TimeSpan DefaultTime => new TimeSpan(9, 0, 0);
    }
}