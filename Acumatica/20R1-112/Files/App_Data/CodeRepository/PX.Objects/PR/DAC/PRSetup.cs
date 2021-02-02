using PX.Data;
using PX.Objects.CS;
using System;

namespace PX.Objects.PR.Standalone
{
    [Serializable]
	[PXCacheName("Payroll Preferences")]
	public partial class PRSetup : PX.Data.IBqlTable
    {
        #region BatchNumberingID
        public abstract class batchNumberingID : IBqlField
        {
        }
        protected String _BatchNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Batch Numbering Sequence")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        public virtual String BatchNumberingID
        {
            get
            {
                return this._BatchNumberingID;
            }
            set
            {
                this._BatchNumberingID = value;
            }
        }
        #endregion

        #region BatchNumberingCD
        public abstract class batchNumberingCD : PX.Data.IBqlField { }
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Payroll Batch Numbering Sequence")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        public virtual string BatchNumberingCD { get; set; }
        #endregion
    }
}