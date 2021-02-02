using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    /// <summary>
    /// This class should be used in case graph already has cache with type <see cref="CSAnswers"/>.
    /// </summary>
    [PXCacheName("Compliance Answer")]
    public class ComplianceAnswer : CSAnswers
    {
        [PXDBGuidNotNull]
        public virtual Guid? NoteId
        {
            get;
            set;
        }

        public abstract class noteId : IBqlField
        {
        }
    }
}
