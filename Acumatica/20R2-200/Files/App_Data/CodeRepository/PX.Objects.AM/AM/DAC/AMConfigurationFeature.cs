using System;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Serializable]
    [PXCacheName(Messages.ConfigurationFeature)]
    public class AMConfigurationFeature : IBqlTable
	{
        internal string DebuggerDisplay => $"[{ConfigurationID}:{Revision}:{LineNbr}] Label={Label}";

        #region ConfigurationID
        public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

		protected string _ConfigurationID;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(AMConfiguration.configurationID))]
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
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMConfiguration.revision))]
        [PXParent(typeof(Select<AMConfiguration, Where<AMConfiguration.configurationID, Equal<Current<AMConfigurationFeature.configurationID>>,
            And<AMConfiguration.revision, Equal<Current<AMConfigurationFeature.revision>>>>>))]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		protected int? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault]
        [PXUIField(DisplayName = "Line Nbr", Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMConfiguration.lineCntrFeature))]
        public virtual int? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region FeatureID
		public abstract class featureID : PX.Data.BQL.BqlString.Field<featureID> { }

		protected string _FeatureID;
		[PXDBString(30, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Feature ID")]
        [PXSelector(typeof(AMFeature.featureID))]
        public virtual string FeatureID
		{
			get
			{
				return this._FeatureID;
			}
			set
			{
				this._FeatureID = value;
			}
		}
		#endregion
		#region Label
		public abstract class label : PX.Data.BQL.BqlString.Field<label> { }

		protected string _Label;
		[PXDBString(30, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Label")]
		[PXCheckUnique(typeof(AMConfigurationFeature.configurationID), typeof(AMConfigurationFeature.revision), IgnoreDuplicatesOnCopyPaste = true)]

		public virtual string Label
		{
			get
			{
				return this._Label;
			}
			set
			{
				this._Label = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected string _Descr;
		[PXDBString(256, IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

		protected int? _SortOrder;
		[PXDBInt]
        [SortOrderDefault(typeof(AMConfigurationFeature.lineNbr))]
        [PXUIField(DisplayName = "Sort Order")]
		public virtual int? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region MinSelection
		public abstract class minSelection : PX.Data.BQL.BqlString.Field<minSelection> { }

		protected string _MinSelection;
        [FormulaString]
        [PXUIField(DisplayName = "Min Selection")]
		public virtual string MinSelection
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
		public abstract class maxSelection : PX.Data.BQL.BqlString.Field<maxSelection> { }

		protected string _MaxSelection;
        [FormulaString]
        [PXUIField(DisplayName = "Max Selection")]
		public virtual string MaxSelection
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
		public abstract class minQty : PX.Data.BQL.BqlString.Field<minQty> { }

		protected string _MinQty;
        [FormulaString]
        [PXUIField(DisplayName = "Min Qty")]
		public virtual string MinQty
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
		public abstract class maxQty : PX.Data.BQL.BqlString.Field<maxQty> { }

		protected string _MaxQty;
        [FormulaString]
        [PXUIField(DisplayName = "Max Qty")]
		public virtual string MaxQty
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
		public abstract class lotQty : PX.Data.BQL.BqlString.Field<lotQty> { }

		protected string _LotQty;
        [FormulaString]
        [PXUIField(DisplayName = "Lot Qty")]
		public virtual string LotQty
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
		#region Visible
		public abstract class visible : PX.Data.BQL.BqlBool.Field<visible> { }

		protected bool? _Visible;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Visible")]
		public virtual bool? Visible
		{
			get
			{
				return this._Visible;
			}
			set
			{
				this._Visible = value;
			}
		}
		#endregion
		#region ResultsCopy
		public abstract class resultsCopy : PX.Data.BQL.BqlBool.Field<resultsCopy> { }

		protected bool? _ResultsCopy;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Results Copy")]
		public virtual bool? ResultsCopy
		{
			get
			{
				return this._ResultsCopy;
			}
			set
			{
				this._ResultsCopy = value;
			}
		}
        #endregion
        #region LineCntrOption
        public abstract class lineCntrOption : PX.Data.BQL.BqlInt.Field<lineCntrOption> { }

        protected int? _LineCntrOption;
        [PXDBInt]
        [PXDefault(0)]
        public virtual int? LineCntrOption
        {
            get
            {
                return this._LineCntrOption;
            }
            set
            {
                this._LineCntrOption = value;
            }
        }
        #endregion
        #region LineCntrRule
        public abstract class lineCntrRule : PX.Data.BQL.BqlInt.Field<lineCntrRule> { }

        protected int? _LineCntrRule;
        [PXDBInt]
        [PXDefault(0)]
        public virtual int? LineCntrRule
        {
            get
            {
                return this._LineCntrRule;
            }
            set
            {
                this._LineCntrRule = value;
            }
        }
        #endregion
        #region PrintResults
        /// <summary>
        /// Flag used for reporting
        /// </summary>
        public abstract class printResults : PX.Data.BQL.BqlBool.Field<printResults> { }

        protected bool? _PrintResults;
        /// <summary>
        /// Flag used for reporting
        /// </summary>
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Print Results")]
        public virtual bool? PrintResults
        {
            get
            {
                return this._PrintResults;
            }
            set
            {
                this._PrintResults = value;
            }
        }
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
    }
}