using PX.Data;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing product configurator setup
    /// </summary>
    [System.Serializable]
    [PXPrimaryGraph(typeof(ConfigSetup))]
    [PXCacheName(Messages.ConfiguratorSetup)]
    public class AMConfiguratorSetup : IBqlTable
	{
		#region ConfigNumberingID
		public abstract class configNumberingID : PX.Data.BQL.BqlString.Field<configNumberingID> { }

		protected string _ConfigNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
        [PXSelector(typeof(Numbering.numberingID))]
        [PXUIField(DisplayName = "Config Numbering Sequence")]
		public virtual string ConfigNumberingID
		{
			get
			{
				return this._ConfigNumberingID;
			}
			set
			{
				this._ConfigNumberingID = value;
			}
		}
        #endregion
        #region DefaultsNumberingID
        /// <summary>
        /// setting not implemented
        /// </summary>
        public abstract class defaultsNumberingID : PX.Data.BQL.BqlString.Field<defaultsNumberingID> { }

        protected string _DefaultsNumberingID;
        /// <summary>
        /// setting not implemented
        /// </summary>
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Defaults Numbering Sequence", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public virtual string DefaultsNumberingID
        {
            get
            {
                return this._DefaultsNumberingID;
            }
            set
            {
                this._DefaultsNumberingID = value;
            }
        }
        #endregion
        #region DfltRevisionNbr
        public abstract class dfltRevisionNbr : PX.Data.BQL.BqlString.Field<dfltRevisionNbr> { }

		protected string _DfltRevisionNbr;
        [PXDefault]
        [RevisionIDField(DisplayName = "Default Revision", Required = true)]
        public virtual string DfltRevisionNbr
		{
			get
			{
				return this._DfltRevisionNbr;
			}
			set
			{
				this._DfltRevisionNbr = value;
			}
		}
		#endregion
		#region ConfigKeyFormat
		public abstract class configKeyFormat : PX.Data.BQL.BqlString.Field<configKeyFormat> { }

		protected string _ConfigKeyFormat;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ConfigKeyFormats.NoKey)]
        [PXUIField(DisplayName = "Config Key Format")]
        [ConfigKeyFormats.List]
        public virtual string ConfigKeyFormat
		{
			get
			{
				return this._ConfigKeyFormat;
			}
			set
			{
				this._ConfigKeyFormat = value;
			}
		}
        #endregion
	    #region DefaultKeyNumberingID
	    public abstract class defaultKeyNumberingID : PX.Data.BQL.BqlString.Field<defaultKeyNumberingID> { }

	    protected string _DefaultKeyNumberingID;
	    [PXDBString(10, IsUnicode = true)]
	    [PXSelector(typeof(Numbering.numberingID))]
	    [PXRestrictor(typeof(Where<Numbering.userNumbering, Equal<False>>), Messages.ManualNumberingKeyNumberingDisabled)]
	    [PXUIField(DisplayName = "Default Key Number Sequence")]
	    public virtual string DefaultKeyNumberingID
        {
	        get
	        {
	            return this._DefaultKeyNumberingID;
	        }
	        set
	        {
	            this._DefaultKeyNumberingID = value;
	        }
	    }
	    #endregion
        #region OnTheFlySubItems
        public abstract class onTheFlySubItems : PX.Data.BQL.BqlBool.Field<onTheFlySubItems> { }

		protected bool? _OnTheFlySubItems;
		[PXDBBool]
		[PXDefault(false)]
        //field not visible until feature implemented
		[PXUIField(DisplayName = "On-The-Fly Subitems", FieldClass = "INSUBITEM", Visible = false)]
		public virtual bool? OnTheFlySubItems
		{
			get
			{
				return this._OnTheFlySubItems;
			}
			set
			{
				this._OnTheFlySubItems = value;
			}
		}
		#endregion
		#region HidePriceDetails
		public abstract class hidePriceDetails : PX.Data.BQL.BqlBool.Field<hidePriceDetails> { }

		protected bool? _HidePriceDetails;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hide Price Details")]
		public virtual bool? HidePriceDetails
		{
			get
			{
				return this._HidePriceDetails;
			}
			set
			{
				this._HidePriceDetails = value;
			}
		}
		#endregion
		#region Rollup
		public abstract class rollup : PX.Data.BQL.BqlString.Field<rollup> { }

		protected string _Rollup;
        [PXDBString(2, IsFixed = true)]
        [PXDefault(RollupOptions.ChildrenAll)]
        [PXUIField(DisplayName = "Rollup")]
        [RollupOptions.List]
        public virtual string Rollup
		{
			get
			{
				return this._Rollup;
			}
			set
			{
				this._Rollup = value;
			}
		}
		#endregion
		#region AllowRollupOverride
		public abstract class allowRollupOverride : PX.Data.BQL.BqlBool.Field<allowRollupOverride> { }

		protected bool? _AllowRollupOverride;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Default on Configuration")]
		public virtual bool? AllowRollupOverride
		{
			get
			{
				return this._AllowRollupOverride;
			}
			set
			{
				this._AllowRollupOverride = value;
			}
		}
		#endregion
		#region Calculate
		public abstract class calculate : PX.Data.BQL.BqlString.Field<calculate> { }

		protected string _Calculate;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(CalcOptions.OnCompletion)]
        [PXUIField(DisplayName = "Calculate")]
        [CalcOptions.List]
        public virtual string Calculate
		{
			get
			{
				return this._Calculate;
			}
			set
			{
				this._Calculate = value;
			}
		}
		#endregion
		#region AllowCalculateOverride
		public abstract class allowCalculateOverride : PX.Data.BQL.BqlBool.Field<allowCalculateOverride> { }

		protected bool? _AllowCalculateOverride;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Default on Configuration")]
		public virtual bool? AllowCalculateOverride
		{
			get
			{
				return this._AllowCalculateOverride;
			}
			set
			{
				this._AllowCalculateOverride = value;
			}
		}
		#endregion
		#region EnableWarehouse
		public abstract class enableWarehouse : PX.Data.BQL.BqlBool.Field<enableWarehouse> { }

		protected bool? _EnableWarehouse;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Warehouse", FieldClass = "INSITE")]
        public virtual bool? EnableWarehouse
		{
			get
			{
				return this._EnableWarehouse;
			}
			set
			{
				this._EnableWarehouse = value;
			}
		}
		#endregion
		#region EnableSubItem
		public abstract class enableSubItem : PX.Data.BQL.BqlBool.Field<enableSubItem> { }

		protected bool? _EnableSubItem;
		[PXDBBool]
		[PXDefault(false)]
        //Field is invisible since sub Item configuration implementation hasn't started yet
		[PXUIField(DisplayName = "Enable Sub item", FieldClass = "INSUBITEM", Visible = false)]
		public virtual bool? EnableSubItem
		{
			get
			{
				return this._EnableSubItem;
			}
			set
			{
				this._EnableSubItem = value;
			}
		}
		#endregion
		#region EnableDiscount
		public abstract class enableDiscount : PX.Data.BQL.BqlBool.Field<enableDiscount> { }

		protected bool? _EnableDiscount;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Discount")]
		public virtual bool? EnableDiscount
		{
			get
			{
				return this._EnableDiscount;
			}
			set
			{
				this._EnableDiscount = value;
			}
		}
		#endregion
		#region EnablePrice
		public abstract class enablePrice : PX.Data.BQL.BqlBool.Field<enablePrice> { }

		protected bool? _EnablePrice;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Price")]
		public virtual bool? EnablePrice
		{
			get
			{
				return this._EnablePrice;
			}
			set
			{
				this._EnablePrice = value;
			}
		}
        #endregion
        #region IsCompletionRequired
        public abstract class isCompletionRequired : PX.Data.BQL.BqlBool.Field<isCompletionRequired> { }

        protected bool? _IsCompletionRequired;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Completion Required Before Production")]
        public virtual bool? IsCompletionRequired
        {
            get
            {
                return this._IsCompletionRequired;
            }
            set
            {
                this._IsCompletionRequired = value;
            }
        }
        #endregion IsCompletionRequired
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
	}
}