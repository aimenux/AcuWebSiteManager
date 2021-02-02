using System;

namespace PX.Objects.PJ.Common.Descriptor.Attributes
{
    public class DefaultWorkingTimeEndAttribute : DefaultWorkingTimeAttribute
    {
        protected override TimeSpan DefaultTime => new TimeSpan(18, 0, 0);
    }
}