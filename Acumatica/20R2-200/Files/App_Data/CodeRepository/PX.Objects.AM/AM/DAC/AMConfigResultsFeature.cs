using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("[{ConfigResultsID}][{ConfigurationID}:{Revision}:{FeatureLineNbr}]")]
    [Serializable]
    [PXCacheName(Messages.ConfigurationResultFeature)]
    public class AMConfigResultsFeature : IBqlTable, IRuleValid
	{
        #region ConfigResultsID
        public abstract class configResultsID : PX.Data.BQL.BqlInt.Field<configResultsID> { }

        protected int? _ConfigResultsID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Config Results ID", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMConfigurationResults.configResultsID))]
        [PXParent(typeof(Select<AMConfigurationResults, Where<AMConfigurationResults.configResultsID, Equal<Current<configResultsID>>>>))]
        public virtual int? ConfigResultsID
        {
            get
            {
                return this._ConfigResultsID;
            }
            set
            {
                this._ConfigResultsID = value;
            }
        }
        #endregion
        #region FeatureLineNbr
        public abstract class featureLineNbr : PX.Data.BQL.BqlInt.Field<featureLineNbr> { }

		protected int? _FeatureLineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Feature Line Nbr", Visible = false, Enabled = false)]
		public virtual int? FeatureLineNbr
		{
			get
			{
				return this._FeatureLineNbr;
			}
			set
			{
				this._FeatureLineNbr = value;
			}
		}
		#endregion
		#region ConfigurationID
		public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

		protected string _ConfigurationID;
        [PXDBString(15, IsUnicode = true)]
        [PXDefault(typeof(AMConfigurationResults.configurationID))]
        [PXUIField(DisplayName = "Configuration ID", Visible = false, Enabled = false)]
        public virtual string ConfigurationID
		{
			get
			{
				return this._ConfigurationID;
			}
			set
			{
				this._ConfigurationID = value;
			}
		}
		#endregion
		#region Revision
		public abstract class revision : PX.Data.BQL.BqlString.Field<revision> { }

		protected string _Revision;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(AMConfigurationResults.revision))]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        public virtual string Revision
		{
			get
			{
				return this._Revision;
			}
			set
			{
				this._Revision = value;
			}
		}
        #endregion
        #region TotalQty
        public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }

        protected decimal? _TotalQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Total Qty", Enabled = false)]
        public virtual decimal? TotalQty
        {
            get
            {
                return this._TotalQty;
            }
            set
            {
                this._TotalQty = value;
            }
        }
        #endregion
        #region Required
        public abstract class required : PX.Data.BQL.BqlBool.Field<required> { }

		protected bool? _Required;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Required")]
		public virtual bool? Required
		{
			get
			{
				return this._Required;
			}
			set
			{
				this._Required = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }

		protected bool? _Completed;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Completed", Enabled = false)]
		public virtual bool? Completed
		{
			get
			{
				return this._Completed;
			}
			set
			{
				this._Completed = value;
			}
		}
        #endregion
        #region RuleValid
        public abstract class ruleValid : PX.Data.BQL.BqlBool.Field<ruleValid> { }

        [PXDBBool]
        [PXDefault(true)]
        public virtual bool? RuleValid { get; set; }
        #endregion

        #region Formula Calculated Fields
        #region MinSelection
        public abstract class minSelection : PX.Data.BQL.BqlInt.Field<minSelection> { }

        protected int? _MinSelection;
        [PXDBInt]
        [PXUIField(DisplayName = "Min Selection", Enabled = false)]
        public virtual int? MinSelection
        {
            get
            {
                return this._MinSelection;
            }
            set
            {
                this._MinSelection = value;
            }
        }
        #endregion
        #region MaxSelection
        public abstract class maxSelection : PX.Data.BQL.BqlInt.Field<maxSelection> { }

        protected int? _MaxSelection;
        [PXDBInt]
        [PXUIField(DisplayName = "Max Selection", Enabled = false)]
        public virtual int? MaxSelection
        {
            get
            {
                return this._MaxSelection;
            }
            set
            {
                this._MaxSelection = value;
            }
        }
        #endregion
        #region MinQty
        public abstract class minQty : PX.Data.BQL.BqlDecimal.Field<minQty> { }

        protected decimal? _MinQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Min Qty", Enabled = false)]
        public virtual decimal? MinQty
        {
            get
            {
                return this._MinQty;
            }
            set
            {
                this._MinQty = value;
            }
        }
        #endregion
        #region MaxQty
        public abstract class maxQty : PX.Data.BQL.BqlDecimal.Field<maxQty> { }

        protected decimal? _MaxQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Max Qty", Enabled = false)]
        public virtual decimal? MaxQty
        {
            get
            {
                return this._MaxQty;
            }
            set
            {
                this._MaxQty = value;
            }
        }
        #endregion
        #region LotQty
        public abstract class lotQty : PX.Data.BQL.BqlDecimal.Field<lotQty> { }

        protected decimal? _LotQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Lot Qty", Enabled = false)]
        public virtual decimal? LotQty
        {
            get
            {
                return this._LotQty;
            }
            set
            {
                this._LotQty = value;
            }
        }
        #endregion
        #endregion
        #region System Fields
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected string _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected string _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected byte[] _tstamp;
        [PXDBTimestamp]
        public virtual byte[] tstamp
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

        #endregion

        #region Unbound Fields

        public abstract class minMaxSelection : PX.Data.BQL.BqlString.Field<minMaxSelection> { }
        [PXUIField(DisplayName = "Min/Max Selection", Enabled = false)]
        [CombineInfo(typeof(AMConfigResultsFeature.minSelection), typeof(AMConfigResultsFeature.maxSelection))]
        public virtual string MinMaxSelection { get; set; }

        public abstract class minLotMaxQty : PX.Data.BQL.BqlString.Field<minLotMaxQty> { }
        [PXUIField(DisplayName = "Min/Lot/Max Qty", Enabled = false)]
        [CombineInfo(typeof(AMConfigResultsFeature.minQty), typeof(AMConfigResultsFeature.lotQty), typeof(AMConfigResultsFeature.maxQty))]
        public virtual string MinLotMaxQty { get; set; }

        #endregion
    }
}