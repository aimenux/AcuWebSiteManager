using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("[{ConfigurationID}:{Revision}] BOMID={BOMID}; InventoryID={InventoryID}")]
    [Serializable]
    [PXPrimaryGraph(typeof(ConfigurationMaint))]
    [PXCacheName(Messages.Configuration)]
    public class AMConfiguration : IBqlTable, INotable
    {
#if DEBUG
        // TODO: AutoNumber and dual-keys are causing issues here. A different approach is probably needed since the requirement is not really a standard scenario.
        // They have been commented out for now. The PXSelector on Revision also has been commented out because it was causing issues when entering an unexisting Revision.
        // NOTE: The following describes the requirements, not the actual implementation.
        // When creating a new configuration, numbering sequence should be used for the ConfigurationID, and default revision (from preferences) should be used
        // for Revision.
        // When selecting an existing configuration, the current pending revision should be selected.
        // If there is no current pending revision, the active revision should be selected.
        // If there is no active revision, the first available inactive revision should be selected. 
#endif
		#region ConfigurationID
		public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

		protected string _ConfigurationID;

        [PXDefault]
        [PXUIField(DisplayName = "Configuration ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [Rev.Key(typeof(AMConfiguratorSetup.configNumberingID),
                    typeof(AMConfiguration.configurationID),
                    typeof(AMConfiguration.revision),
                    typeof(AMConfiguration.configurationID),
                    typeof(AMConfiguration.revision),
                    typeof(AMConfiguration.descr),
                    typeof(AMConfiguration.inventoryID),
                    typeof(AMConfiguration.bOMID),
                    typeof(AMConfiguration.bOMRevisionID))]
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
        
        [PXDefault]
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
        [Rev.ID(typeof(AMConfiguratorSetup.dfltRevisionNbr), typeof(configurationID), typeof(revision), typeof(revision), typeof(descr), typeof(status))]
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
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		protected string _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ConfigRevisionStatus.Pending)]
		[PXUIField(DisplayName = "Status")]
        [ConfigRevisionStatus.List]
		public virtual string Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected string _Descr;
		[PXDBString(256, IsUnicode = true)]
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
		#region BOMID
		public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

		protected string _BOMID;
        [BomID]
        [BOMIDSelector]
        [PXDefault]
        public virtual string BOMID
		{
			get
			{
				return this._BOMID;
			}
			set
			{
				this._BOMID = value;
			}
		}
        #endregion
        #region BOMRevisionID
        public abstract class bOMRevisionID : PX.Data.BQL.BqlString.Field<bOMRevisionID> { }

        protected string _BOMRevisionID;
        [RevisionIDField(DisplayName = "BOM Revision")]
        [PXRestrictor(typeof(Where<AMBomItem.status, Equal<AMBomStatus.active>>), Messages.BomRevisionIsNotActive, typeof(AMBomItem.bOMID), typeof(AMBomItem.revisionID), CacheGlobal = true)]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<AMConfiguration.bOMID>>>>)
            , typeof(AMBomItem.revisionID)
            , typeof(AMBomItem.status)
            , typeof(AMBomItem.descr)
            , typeof(AMBomItem.effStartDate)
            , typeof(AMBomItem.effEndDate)
            , DescriptionField = typeof(AMBomItem.descr))]
        [PXDefault(typeof(Search<AMBomItemActiveAggregate.revisionID, 
            Where<AMBomItemActiveAggregate.bOMID, Equal<Current<AMConfiguration.bOMID>>>>))]
        [PXFormula(typeof(Default<AMConfiguration.bOMID>))]
        [PXForeignReference(typeof(CompositeKey<Field<AMConfiguration.bOMID>.IsRelatedTo<AMBomItem.bOMID>, Field<AMConfiguration.bOMRevisionID>.IsRelatedTo<AMBomItem.revisionID>>))]
        public virtual string BOMRevisionID
        {
            get
            {
                return this._BOMRevisionID;
            }
            set
            {
                this._BOMRevisionID = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [Inventory(Enabled = false)]
        [PXDefault(typeof(Search<AMBomItem.inventoryID, Where<AMBomItem.bOMID, Equal<Current<AMConfiguration.bOMID>>, And<AMBomItem.revisionID, Equal<Current<AMConfiguration.bOMRevisionID>>>>>))]
        [PXFormula(typeof(Default<AMConfiguration.bOMID, AMConfiguration.bOMRevisionID>))]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region Rollup
        public abstract class priceRollup : PX.Data.BQL.BqlString.Field<priceRollup> { }

        protected string _PriceRollup;
        [PXDBString(2, IsFixed = true)]
        [PXDefault(typeof(AMConfiguratorSetup.rollup))]
        [PXUIField(DisplayName = "Rollup")]
        [RollupOptions.List]
        public virtual string PriceRollup
        {
            get
            {
                return this._PriceRollup;
            }
            set
            {
                this._PriceRollup = value;
            }
        }
        #endregion
        #region Calculate
        public abstract class priceCalc : PX.Data.BQL.BqlString.Field<priceCalc> { }

        protected string _PriceCalc;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(AMConfiguratorSetup.calculate))]
        [PXUIField(DisplayName = "Calculate")]
        [CalcOptions.List]
        public virtual string PriceCalc
        {
            get
            {
                return this._PriceCalc;
            }
            set
            {
                this._PriceCalc = value;
            }
        }
        #endregion

        #region Key Related Fields

        #region KeyFormat
        public abstract class keyFormat : PX.Data.BQL.BqlString.Field<keyFormat> { }

        protected string _KeyFormat;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(AMConfiguratorSetup.configKeyFormat))]
        [PXUIField(DisplayName = "Format")]
        [ConfigKeyFormats.List]
        public virtual string KeyFormat
        {
            get
            {
                return this._KeyFormat;
            }
            set
            {
                this._KeyFormat = value;
            }
        }
        #endregion
        #region KeyEquation
        // Had to rename KeyFormula to KeyEquation to change the size of the field. (field was not yet in use)
        // Double the results size to allow for formula writing. Results size is 120. An ID of MAX String is unnecessary (KeyFormula).
        public abstract class keyEquation : PX.Data.BQL.BqlString.Field<keyEquation> { }

        protected string _KeyEquation;
        [FormulaString(240)]
        [PXUIField(DisplayName = "Formula")]
        public virtual string KeyEquation
        {
            get
            {
                return this._KeyEquation;
            }
            set
            {
                this._KeyEquation = value;
            }
        }
        #endregion
        #region KeyNumberingID
        public abstract class keyNumberingID : PX.Data.BQL.BqlString.Field<keyNumberingID> { }

        protected string _KeyNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Search<AMConfiguratorSetup.defaultKeyNumberingID, Where<Current<AMConfiguration.keyFormat>, Equal<ConfigKeyFormats.numberSequence>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Numbering.numberingID))]
        [PXRestrictor(typeof(Where<Numbering.userNumbering, Equal<False>>), Messages.ManualNumberingKeyNumberingDisabled)]
        [PXUIField(DisplayName = "Number Sequence")]
        [PXFormula(typeof(Default<AMConfiguration.keyFormat>))]
        public virtual string KeyNumberingID
        {
            get
            {
                return this._KeyNumberingID;
            }
            set
            {
                this._KeyNumberingID = value;
            }
        }
        #endregion
        #region KeyDescription
        public abstract class keyDescription : PX.Data.BQL.BqlString.Field<keyDescription> { }

        protected string _KeyDescription;
        [FormulaString]
        [PXUIField(DisplayName = "Key Description")]
        public virtual string KeyDescription
        {
            get
            {
                return this._KeyDescription;
            }
            set
            {
                this._KeyDescription = value;
            }
        }
        #endregion
        #region TranDescription
        public abstract class tranDescription : PX.Data.BQL.BqlString.Field<tranDescription> { }

        protected string _TranDescription;
        // Double the results size to allow for formula writing. Results size is 256 to match transaction description of Sales Order & item description
        /// <summary>
        /// Formula field to configure a custom transaction description for sales order tran description.
        /// </summary>
        [FormulaString(512)]
        [PXUIField(DisplayName = "Tran Description")]
        public virtual string TranDescription
        {
            get
            {
                return this._TranDescription;
            }
            set
            {
                this._TranDescription = value;
            }
        }
        #endregion
        #region OnTheFlySubItems
        public abstract class onTheFlySubItems : PX.Data.BQL.BqlBool.Field<onTheFlySubItems> { }

        protected bool? _OnTheFlySubItems;
        [PXDBBool]
        [PXDefault(typeof(AMConfiguratorSetup.onTheFlySubItems))]
        [PXUIField(DisplayName = "On-The-Fly Subitems", FieldClass = "INSUBITEM")]
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

        #endregion

        #region LineCntrFeature
        public abstract class lineCntrFeature : PX.Data.BQL.BqlInt.Field<lineCntrFeature> { }

        protected int? _LineCntrFeature;
        [PXDBInt]
        [PXDefault(0)]
        public virtual int? LineCntrFeature
        {
            get
            {
                return this._LineCntrFeature;
            }
            set
            {
                this._LineCntrFeature = value;
            }
        }
        #endregion
        #region LineCntrAttribute
        public abstract class lineCntrAttribute : PX.Data.BQL.BqlInt.Field<lineCntrAttribute> { }

        protected int? _LineCntrAttribute;
        [PXDBInt]
        [PXDefault(0)]
        public virtual int? LineCntrAttribute
        {
            get
            {
                return this._LineCntrAttribute;
            }
            set
            {
                this._LineCntrAttribute = value;
            }
        }
        #endregion

        #region IsCompletionRequired
        public abstract class isCompletionRequired : PX.Data.BQL.BqlBool.Field<isCompletionRequired> { }

        protected bool? _IsCompletionRequired;
        [PXDBBool]
        [PXDefault(typeof(AMConfiguratorSetup.isCompletionRequired))]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
        [AutoNote]
        public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
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