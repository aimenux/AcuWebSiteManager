using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Bill of Material (BOM) Preferences table
    /// </summary>
    [PXPrimaryGraph(typeof(BOMSetup))]
    [PXCacheName(Messages.BOMSetup)]
    [Serializable]
    public class AMBSetup : IBqlTable
    {
        #region BOMNumberingID

        public abstract class bOMNumberingID : PX.Data.BQL.BqlString.Field<bOMNumberingID> { }

        protected String _BOMNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault]
        [PXSelector(typeof(Numbering.numberingID))]
        [PXUIField(DisplayName = "BOM Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String BOMNumberingID
        {
            get
            {
                return this._BOMNumberingID;
            }
            set
            {
                this._BOMNumberingID = value;
            }
        }
        #endregion
        #region DupInvBOM
		public abstract class dupInvBOM : PX.Data.BQL.BqlString.Field<dupInvBOM> { }

		protected String _DupInvBOM;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(SetupMessage.AllowMsg)]
		[PXUIField(DisplayName = "Duplicates on BOM")]
        [SetupMessage.List]
		public virtual String DupInvBOM
		{
			get
			{
				return this._DupInvBOM;
			}
			set
			{
				this._DupInvBOM = value;
			}
		}
		#endregion
		#region DupInvOper
		public abstract class dupInvOper : PX.Data.BQL.BqlString.Field<dupInvOper> { }

		protected String _DupInvOper;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(SetupMessage.AllowMsg)]
		[PXUIField(DisplayName = "Duplicates on Operation")]
        [SetupMessage.List]
		public virtual String DupInvOper
		{
			get
			{
				return this._DupInvOper;
			}
			set
			{
				this._DupInvOper = value;
			}
		}
		#endregion
        #region AllowEmptyBOMSubItemID
        public abstract class allowEmptyBOMSubItemID : PX.Data.BQL.BqlInt.Field<allowEmptyBOMSubItemID> { }

        protected Boolean? _AllowEmptyBOMSubItemID;
        /// <summary>
        /// Can a BOM be created without a Sub Item ID
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allow Empty BOM Item Sub Item ID", FieldClass = "INSUBITEM")]
        public virtual Boolean? AllowEmptyBOMSubItemID
        {
            get
            {
                return this._AllowEmptyBOMSubItemID;
            }
            set
            {
                this._AllowEmptyBOMSubItemID = value;
            }
        }
        #endregion
        #region LastLowLevelCompletedDateTime
        /// <summary>
        /// Last date the low level values were calculated
        /// </summary>
        public abstract class lastLowLevelCompletedDateTime : PX.Data.BQL.BqlDateTime.Field<lastLowLevelCompletedDateTime> { }

        /// <summary>
        /// Last date the low level values were calculated
        /// </summary>
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Last Low Level Completed At", Enabled = false, IsReadOnly = true, Visible = false)]
        public virtual DateTime? LastLowLevelCompletedDateTime { get; set; }
        
        #endregion
        #region LastMaxLowLevel
        /// <summary>
        /// The max low level calculated from the last completed low level process
        /// </summary>
        public abstract class lastMaxLowLevel : PX.Data.BQL.BqlInt.Field<lastMaxLowLevel> { }

        /// <summary>
        /// The max low level calculated from the last completed low level process
        /// </summary>
        [PXDBInt]
        [PXUIField(DisplayName = "Last Max Low Level", Enabled = false, IsReadOnly = true, Visible = false)]
        public virtual int? LastMaxLowLevel { get; set; }
        
        #endregion
        #region WcID  (Default Work Center)
        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

		protected String _WcID;
        [WorkCenterIDField(DisplayName = "Default Work Center")]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        [PXForeignReference(typeof(Field<AMBSetup.wcID>.IsRelatedTo<AMWC.wcID>))]
        public virtual String WcID
		{
			get
			{
				return this._WcID;
			}
			set
			{
				this._WcID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
        #endregion

        #region OperationTimeFormat
        public abstract class operationTimeFormat : PX.Data.BQL.BqlInt.Field<operationTimeFormat> { }

        protected int? _OperationTimeFormat;
        [PXDBInt]
        [PXDefault(AMTimeFormatAttribute.TimeSpanFormat.ShortHoursMinutesCompact)]
        [PXUIField(DisplayName = "Operation Time Format")]
        [AMTimeFormatAttribute.TimeSpanFormat.List]
        public virtual int? OperationTimeFormat
        {
            get
            {
                return this._OperationTimeFormat;
            }
            set
            {
                this._OperationTimeFormat = value;
            }
        }
        #endregion
        #region ProductionTimeFormat
        public abstract class productionTimeFormat : PX.Data.BQL.BqlInt.Field<productionTimeFormat> { }

        protected int? _ProductionTimeFormat;
        [PXDBInt]
        [PXDefault(AMTimeFormatAttribute.TimeSpanFormat.LongHoursMinutes)]
        [PXUIField(DisplayName = "Total Time Format")]
        [AMTimeFormatAttribute.TimeSpanFormat.List]
        public virtual int? ProductionTimeFormat
        {
            get
            {
                return this._ProductionTimeFormat;
            }
            set
            {
                this._ProductionTimeFormat = value;
            }
        }
        #endregion

        #region DefaultRevisionID
        public abstract class defaultRevisionID : PX.Data.BQL.BqlString.Field<defaultRevisionID> { }

        protected String _DefaultRevisionID;
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "Default Revision", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault("", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String DefaultRevisionID
        {
            get
            {
                return this._DefaultRevisionID;
            }
            set
            {
                this._DefaultRevisionID = value;
            }
        }
        #endregion
        #region ECRNumberingID

        public abstract class eCRNumberingID : PX.Data.BQL.BqlString.Field<eCRNumberingID> { }
        protected String _ECRNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Numbering.numberingID))]
        [PXUIField(DisplayName = "ECR Numbering Sequence", FieldClass = Features.ECCFIELDCLASS)]
        public virtual String ECRNumberingID
        {
            get
            {
                return this._ECRNumberingID;
            }
            set
            {
                this._ECRNumberingID = value;
            }
        }
        #endregion
        #region ECONumberingID

        public abstract class eCONumberingID : PX.Data.BQL.BqlString.Field<eCONumberingID> { }
        protected String _ECONumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Numbering.numberingID))]
        [PXUIField(DisplayName = "ECO Numbering Sequence", FieldClass = Features.ECCFIELDCLASS)]
        public virtual String ECONumberingID
        {
            get
            {
                return this._ECONumberingID;
            }
            set
            {
                this._ECONumberingID = value;
            }
        }
        #endregion

        #region ECRRequestApproval
        public abstract class eCRRequestApproval : PX.Data.BQL.BqlBool.Field<eCRRequestApproval> { }
        protected bool? _ECRRequestApproval;
        [EPRequireApproval]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "ECR Require Approval")]
        public virtual bool? ECRRequestApproval
        {
            get
            {
                return this._ECRRequestApproval;
            }
            set
            {
                this._ECRRequestApproval = value;
            }
        }
        #endregion	
        #region ECORequestApproval
        public abstract class eCORequestApproval : PX.Data.BQL.BqlBool.Field<eCORequestApproval> { }
        protected bool? _ECORequestApproval;
        [EPRequireApproval]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "ECO Require Approval")]
        public virtual bool? ECORequestApproval
        {
            get
            {
                return this._ECORequestApproval;
            }
            set
            {
                this._ECORequestApproval = value;
            }
        }
        #endregion
        #region ForceECR
        public abstract class forceECR : PX.Data.BQL.BqlBool.Field<forceECR> { }
        protected bool? _ForceECR;
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Prevent New Revisions Without ECR", FieldClass = Features.ECCFIELDCLASS)]
        public virtual bool? ForceECR
        {
            get
            {
                return this._ForceECR;
            }
            set
            {
                this._ForceECR = value;
            }
        }
        #endregion	

        #region AllowArchiveWithoutUpdatePending
        public abstract class allowArchiveWithoutUpdatePending : PX.Data.BQL.BqlBool.Field<allowArchiveWithoutUpdatePending> { }
        protected bool? _AllowArchiveWithoutUpdatePending;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allow Archive without Update Pending")]
        public virtual bool? AllowArchiveWithoutUpdatePending
        {
            get
            {
                return this._AllowArchiveWithoutUpdatePending;
            }
            set
            {
                this._AllowArchiveWithoutUpdatePending = value;
            }
        }
        #endregion	
        #region AutoArchiveWhenUpdatePending
        public abstract class autoArchiveWhenUpdatePending : PX.Data.BQL.BqlBool.Field<autoArchiveWhenUpdatePending> { }
        protected bool? _AutoArchiveWhenUpdatePending;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Auto Archive when Update Pending")]
        public virtual bool? AutoArchiveWhenUpdatePending
        {
            get
            {
                return this._AutoArchiveWhenUpdatePending;
            }
            set
            {
                this._AutoArchiveWhenUpdatePending = value;
            }
        }
        #endregion	
    }
}