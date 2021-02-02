using System;
using PX.Data;

namespace PX.Objects.GL
{
    public class ReclassAction
    {
        public class List : PXStringListAttribute
        {
            public List() : base(
                new string[] { ReclassType.Common, ReclassType.Split },
                new string[] { Messages.Reclassification, Messages.Split })
            { }
        }
    }
}
