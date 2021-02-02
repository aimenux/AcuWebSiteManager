using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.TM;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.EstimateClass)]
    [PXPrimaryGraph(typeof(EstimateClassMaint))]
	public class AMEstimateClass : IBqlTable
	{
        #region Estimate Class ID
        public abstract class estimateClassID : PX.Data.BQL.BqlString.Field<estimateClassID> { }

        protected String _EstimateClassID;
        [PXDefault]
        [PXDBString(20, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAAAAAAA", IsKey = true)]
        [PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMEstimateClass.estimateClassID>))]
        public virtual String EstimateClassID
        {
            get
            {
                return this._EstimateClassID;
            }
            set
            {
                this._EstimateClassID = value;
            }
        }
        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                this._Description = value;
            }
        }
        #endregion
        #region Item Class Id
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

        protected int? _ItemClassID;
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        [PXRestrictor(typeof(Where<INItemClass.stkItem, Equal<True>>), PX.Objects.AM.Messages.EstimateItemClassMustBeStockItem, typeof(INItemClass.itemClassCD))]
        public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region Tax Category ID
        public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        protected String _TaxCategoryID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category")]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), PX.Objects.TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault]
        public virtual String TaxCategoryID
        {
            get
            {
                return this._TaxCategoryID;
            }
            set
            {
                this._TaxCategoryID = value;
            }
        }
        #endregion
        #region EngineerID
        public abstract class engineerID : PX.Data.BQL.BqlInt.Field<engineerID> { }

        protected int? _EngineerID;
        [Owner(DisplayName = "Engineer")]
        public virtual int? EngineerID
        {
            get
            {
                return this._EngineerID;
            }
            set
            {
                this._EngineerID = value;
            }
        }
        #endregion
        #region Lead Time
        public abstract class leadTime : PX.Data.BQL.BqlInt.Field<leadTime> { }

        protected int? _LeadTime;
        [PXDBInt]
        [PXDefault(TypeCode.Int32, "0")]
        [PXUIField(DisplayName = "Lead Time (Days)")]
        public virtual int? LeadTime
        {
            get
            {
                return this._LeadTime;
            }
            set
            {
                this._LeadTime = value;
            }
        }
        #endregion
        #region Order Quantity
        public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }

        protected Decimal? _OrderQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "1.0000")]
        [PXUIField(DisplayName = "Order Qty")]
        public virtual Decimal? OrderQty
        {
            get
            {
                return this._OrderQty;
            }
            set
            {
                this._OrderQty = value;
            }
        }
        #endregion
        #region Labor Markup Percent
        public abstract class laborMarkupPct : PX.Data.BQL.BqlDecimal.Field<laborMarkupPct> { }

        protected Decimal? _LaborMarkupPct;
        [PXDBDecimal(6, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Labor Markup Pct.")]
        public virtual Decimal? LaborMarkupPct
        {
            get
            {
                return this._LaborMarkupPct;
            }
            set
            {
                this._LaborMarkupPct = value;
            }
        }
        #endregion
        #region Machine Markup Percent
        public abstract class machineMarkupPct : PX.Data.BQL.BqlDecimal.Field<machineMarkupPct> { }

        protected Decimal? _MachineMarkupPct;
        [PXDBDecimal(6, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine Markup Pct.")]
        public virtual Decimal? MachineMarkupPct
        {
            get
            {
                return this._MachineMarkupPct;
            }
            set
            {
                this._MachineMarkupPct = value;
            }
        }
        #endregion
        #region Material Markup Percent
        public abstract class materialMarkupPct : PX.Data.BQL.BqlDecimal.Field<materialMarkupPct> { }

        protected Decimal? _MaterialMarkupPct;
        [PXDBDecimal(6, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Material Markup Pct.")]
        public virtual Decimal? MaterialMarkupPct
        {
            get
            {
                return this._MaterialMarkupPct;
            }
            set
            {
                this._MaterialMarkupPct = value;
            }
        }
        #endregion
        #region Tool Markup Percent
        public abstract class toolMarkupPct : PX.Data.BQL.BqlDecimal.Field<toolMarkupPct> { }

        protected Decimal? _ToolMarkupPct;
        [PXDBDecimal(6, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Tool Markup Pct.")]
        public virtual Decimal? ToolMarkupPct
        {
            get
            {
                return this._ToolMarkupPct;
            }
            set
            {
                this._ToolMarkupPct = value;
            }
        }
        #endregion
        #region Overhead Markup Percent
        public abstract class overheadMarkupPct : PX.Data.BQL.BqlDecimal.Field<overheadMarkupPct> { }

        protected Decimal? _OverheadMarkupPct;
        [PXDBDecimal(6, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Overhead Markup Pct.")]
        public virtual Decimal? OverheadMarkupPct
        {
            get
            {
                return this._OverheadMarkupPct;
            }
            set
            {
                this._OverheadMarkupPct = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
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
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
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
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
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
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
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
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
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
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
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
        #region Subcontract Markup Percent
        public abstract class subcontractMarkupPct : PX.Data.BQL.BqlDecimal.Field<subcontractMarkupPct> { }

        protected Decimal? _SubcontractMarkupPct;
        [PXDBDecimal(6, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Subcontract Markup Pct.")]
        public virtual Decimal? SubcontractMarkupPct
        {
            get
            {
                return this._SubcontractMarkupPct;
            }
            set
            {
                this._SubcontractMarkupPct = value;
            }
        }
        #endregion
    }
}
