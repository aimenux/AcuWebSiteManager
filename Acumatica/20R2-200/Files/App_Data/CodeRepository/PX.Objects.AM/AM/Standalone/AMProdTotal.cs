using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM.Standalone
{
    [PXHidden]
    [Serializable]
    [PXCacheName(Messages.ProductionTotals)]
    public class AMProdTotal : IBqlTable, IProdOrder
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        [PXDefault]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr(IsKey = true)]
        [PXDefault]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion

        //Planned Values
        #region PlanLabor

        public abstract class planLabor : PX.Data.BQL.BqlDecimal.Field<planLabor> { }

        protected Decimal? _PlanLabor;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Labor", Enabled = false)]
        public virtual Decimal? PlanLabor
        {
            get
            {
                return _PlanLabor;
            }
            set
            {
                _PlanLabor = value;
            }
        }
        #endregion
        #region PlanLaborTime 
        public abstract class planLaborTime : PX.Data.BQL.BqlInt.Field<planLaborTime> { }

        protected Int32? _PlanLaborTime;
        [ProductionTotalTimeDB]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Labor Time", Enabled = false)]
        public virtual Int32? PlanLaborTime
        {
            get
            {
                return this._PlanLaborTime;
            }
            set
            {
                this._PlanLaborTime = value;
            }
        }
        #endregion
        #region PlanMachine

        public abstract class planMachine : PX.Data.BQL.BqlDecimal.Field<planMachine> { }

        protected Decimal? _PlanMachine;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
        public virtual Decimal? PlanMachine
        {
            get
            {
                return _PlanMachine;
            }
            set
            {
                _PlanMachine = value;
            }
        }
        #endregion
        #region PlanMaterial

        public abstract class planMaterial : PX.Data.BQL.BqlDecimal.Field<planMaterial> { }

        protected Decimal? _PlanMaterial;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        public virtual Decimal? PlanMaterial
        {
            get
            {
                return _PlanMaterial;
            }
            set
            {
                _PlanMaterial = value;
            }
        }
        #endregion
        #region PlanTool

        public abstract class planTool : PX.Data.BQL.BqlDecimal.Field<planTool> { }

        protected Decimal? _PlanTool;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Tool", Enabled = false)]
        public virtual Decimal? PlanTool
        {
            get
            {
                return _PlanTool;
            }
            set
            {
                _PlanTool = value;
            }
        }
        #endregion
        #region PlanFixedOverhead

        public abstract class planFixedOverhead : PX.Data.BQL.BqlDecimal.Field<planFixedOverhead> { }

        protected Decimal? _PlanFixedOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        public virtual Decimal? PlanFixedOverhead
        {
            get
            {
                return _PlanFixedOverhead;
            }
            set
            {
                _PlanFixedOverhead = value;
            }
        }
        #endregion
        #region PlanVariableOverhead

        public abstract class planVariableOverhead : PX.Data.BQL.BqlDecimal.Field<planVariableOverhead> { }

        protected Decimal? _PlanVariableOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        public virtual Decimal? PlanVariableOverhead
        {
            get
            {
                return _PlanVariableOverhead;
            }
            set
            {
                _PlanVariableOverhead = value;
            }
        }
        #endregion
        #region PlanQtyToProduce

        public abstract class planQtyToProduce : PX.Data.BQL.BqlDecimal.Field<planQtyToProduce> { }

        protected Decimal? _PlanQtyToProduce;
        [PXDBQuantity]
        [PXDefault(typeof(Search<AMProdItem.baseQtytoProd,
            Where<AMProdItem.prodOrdID, Equal<Current<AM.Standalone.AMProdTotal.prodOrdID>>,
                And<AMProdItem.orderType, Equal<Current<AM.Standalone.AMProdTotal.orderType>>>>>))]
        [PXUIField(DisplayName = "Qty to Produce", Enabled = false)]
        public virtual Decimal? PlanQtyToProduce
        {
            get
            {
                return this._PlanQtyToProduce;
            }
            set
            {
                this._PlanQtyToProduce = value;
            }
        }
        #endregion
        #region PlanTotal

        public abstract class planTotal : PX.Data.BQL.BqlDecimal.Field<planTotal> { }

        protected Decimal? _PlanTotal;
        [PXBaseCury]
        [PXFormula(typeof(Add<AM.Standalone.AMProdTotal.planLabor, Add<AM.Standalone.AMProdTotal.planMachine, Add<AM.Standalone.AMProdTotal.planMaterial, Add<AM.Standalone.AMProdTotal.planTool,
            Add<AM.Standalone.AMProdTotal.planFixedOverhead, AM.Standalone.AMProdTotal.planVariableOverhead>>>>>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Plan Total", Enabled = false)]
        public virtual Decimal? PlanTotal
        {
            get
            {
                return _PlanTotal;
            }
            set
            {
                _PlanTotal = value;
            }
        }
        #endregion
        #region PlanUnitCost

        public abstract class planUnitCost : PX.Data.BQL.BqlDecimal.Field<planUnitCost> { }

        protected Decimal? _PlanUnitCost;
        [PXBaseCury]
        [PXFormula(typeof(Switch<Case<Where<AM.Standalone.AMProdTotal.planQtyToProduce, IsNull, Or<AM.Standalone.AMProdTotal.planQtyToProduce, Equal<decimal0>>>, decimal0>,
            Div<AM.Standalone.AMProdTotal.planTotal, AM.Standalone.AMProdTotal.planQtyToProduce>>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unit Cost", Enabled = false)]
        public virtual Decimal? PlanUnitCost
        {
            get
            {
                return _PlanUnitCost;
            }
            set
            {
                _PlanUnitCost = value;
            }
        }
        #endregion
        #region Plan Cost Date

        public abstract class planCostDate : PX.Data.BQL.BqlDateTime.Field<planCostDate> { }

        protected DateTime? _PlanCostDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Plan Cost Date", Enabled = false)]
        public virtual DateTime? PlanCostDate
        {
            get
            {
                return this._PlanCostDate;
            }
            set
            {
                this._PlanCostDate = value;
            }
        }
        #endregion

        //Actual Values
        #region ActualLabor

        public abstract class actualLabor : PX.Data.BQL.BqlDecimal.Field<actualLabor> { }

        protected Decimal? _ActualLabor;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Labor", Enabled = false)]
        public virtual Decimal? ActualLabor
        {
            get
            {
                return _ActualLabor;
            }
            set
            {
                _ActualLabor = value;
            }
        }
        #endregion
        #region ActualLaborTime 
        public abstract class actualLaborTime : PX.Data.BQL.BqlInt.Field<actualLaborTime> { }

        protected Int32? _ActualLaborTime;
        [ProductionTotalTimeDB]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Labor Time", Enabled = false)]
        public virtual Int32? ActualLaborTime
        {
            get
            {
                return this._ActualLaborTime;
            }
            set
            {
                this._ActualLaborTime = value;
            }
        }
        #endregion
        #region ActualMachine

        public abstract class actualMachine : PX.Data.BQL.BqlDecimal.Field<actualMachine> { }

        protected Decimal? _ActualMachine;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
        public virtual Decimal? ActualMachine
        {
            get
            {
                return _ActualMachine;
            }
            set
            {
                _ActualMachine = value;
            }
        }
        #endregion
        #region ActualMaterial

        public abstract class actualMaterial : PX.Data.BQL.BqlDecimal.Field<actualMaterial> { }

        protected Decimal? _ActualMaterial;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        public virtual Decimal? ActualMaterial
        {
            get
            {
                return _ActualMaterial;
            }
            set
            {
                _ActualMaterial = value;
            }
        }
        #endregion
        #region ActualTool

        public abstract class actualTool : PX.Data.BQL.BqlDecimal.Field<actualTool> { }

        protected Decimal? _ActualTool;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Tool", Enabled = false)]
        public virtual Decimal? ActualTool
        {
            get
            {
                return _ActualTool;
            }
            set
            {
                _ActualTool = value;
            }
        }
        #endregion
        #region ActualFixedOverhead

        public abstract class actualFixedOverhead : PX.Data.BQL.BqlDecimal.Field<actualFixedOverhead> { }

        protected Decimal? _ActualFixedOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        public virtual Decimal? ActualFixedOverhead
        {
            get
            {
                return _ActualFixedOverhead;
            }
            set
            {
                _ActualFixedOverhead = value;
            }
        }
        #endregion
        #region ActualVariableOverhead

        public abstract class actualVariableOverhead : PX.Data.BQL.BqlDecimal.Field<actualVariableOverhead> { }

        protected Decimal? _ActualVariableOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        public virtual Decimal? ActualVariableOverhead
        {
            get
            {
                return _ActualVariableOverhead;
            }
            set
            {
                _ActualVariableOverhead = value;
            }
        }
        #endregion
        #region WIPAdjustment
        public abstract class wIPAdjustment : PX.Data.BQL.BqlDecimal.Field<wIPAdjustment> { }

        protected Decimal? _WIPAdjustment;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Adjustments", Enabled = false)]
        public virtual Decimal? WIPAdjustment
        {
            get
            {
                return this._WIPAdjustment;
            }
            set
            {
                this._WIPAdjustment = value;
            }
        }
        #endregion
        #region WIPTotal
        public abstract class wIPTotal : PX.Data.BQL.BqlDecimal.Field<wIPTotal> { }

        protected Decimal? _WIPTotal;
        [PXBaseCury]
        [PXUIField(DisplayName = "WIP Total", Enabled = false)]
        // Calced values required for upgrade script to self calculate the values and to be used in a query
        [PXDBCalced(typeof(Add<actualLabor, Add<actualMachine, Add<actualMaterial, Add<actualTool, Add<actualFixedOverhead, Add<actualVariableOverhead, Sub<wIPAdjustment, scrapAmount>>>>>>>), typeof(Decimal))]
        public virtual Decimal? WIPTotal { get; set; }
        #endregion

        // Logging fields
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
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID]
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
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
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
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp]
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
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
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

        // Scrap Amount - Total Scrap Write Off and Scrap Quarantine Transactions
        #region ScrapAmount

        public abstract class scrapAmount : PX.Data.BQL.BqlDecimal.Field<scrapAmount> { }

        protected Decimal? _ScrapAmount;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Scrap", Enabled = false)]
        public virtual Decimal? ScrapAmount
        {
            get
            {
                return _ScrapAmount;
            }
            set
            {
                _ScrapAmount = value;
            }
        }
        #endregion
    }
}