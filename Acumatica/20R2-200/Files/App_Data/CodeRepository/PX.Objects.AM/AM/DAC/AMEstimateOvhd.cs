using System;
using PX.Data;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.EstimateOverhead)]
    public class AMEstimateOvhd : IBqlTable, IEstimateOper, INotable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMEstimateOvhd>.By<estimateID, revisionID, operationID, lineID>
        {
            public static AMEstimateOvhd Find(PXGraph graph, string estimateID, string revisionID, int? operationID, int? lineID)
                => FindBy(graph, estimateID, revisionID, operationID, lineID);
            public static AMEstimateOvhd FindDirty(PXGraph graph, string estimateID, string revisionID, int? operationID, int? lineID)
                => PXSelect<AMEstimateOvhd,
                    Where<estimateID, Equal<Required<estimateID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>>>>>>
                    .SelectWindowed(graph, 0, 1, estimateID, revisionID, operationID, lineID);
        }

        public static class FK
        {
            public class Estimate : AMEstimateItem.PK.ForeignKeyOf<AMEstimateOvhd>.By<estimateID, revisionID> { }
            public class Operation : AMEstimateOper.PK.ForeignKeyOf<AMEstimateOvhd>.By<estimateID, revisionID, operationID> { }
            public class Overhead : AMOverhead.PK.ForeignKeyOf<AMEstimateOvhd>.By<ovhdID> { }
        }

        #endregion

        #region Estimate ID
        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

        protected String _EstimateID;
        [PXDBDefault(typeof(AMEstimateOper.estimateID))]
        [EstimateID(IsKey = true, Enabled = false, Visible = false)]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }
        #endregion
        #region Revision ID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;
        [PXDBDefault(typeof(AMEstimateOper.revisionID))]
        [PXDBString(10, IsUnicode = true, InputMask = ">AAAAAAAAAA", IsKey = true)]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }
        #endregion
        #region Operation ID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected Int32? _OperationID;
        [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMEstimateOper.operationID))]
        [PXParent(typeof(Select<AMEstimateOper, Where<AMEstimateOper.estimateID, Equal<Current<AMEstimateOvhd.estimateID>>,
            And<AMEstimateOper.revisionID, Equal<Current<AMEstimateOvhd.revisionID>>,
            And<AMEstimateOper.operationID, Equal<Current<AMEstimateOvhd.operationID>>>>>>))]
        public virtual Int32? OperationID
        {
            get
            {
                return this._OperationID;
            }
            set
            {
                this._OperationID = value;
            }
        }
        #endregion
        #region Line ID
        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

        protected Int32? _LineID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMEstimateOper.lineCntrOvhd))]
        public virtual Int32? LineID
        {
            get
            {
                return this._LineID;
            }
            set
            {
                this._LineID = value;
            }
        }
        #endregion
        #region OvhdID
        public abstract class ovhdID : PX.Data.BQL.BqlString.Field<ovhdID> { }

        protected String _OvhdID;
        [PXDefault]
        [OverheadIDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMOverhead.ovhdID>))]
        public virtual String OvhdID
        {
            get
            {
                return this._OvhdID;
            }
            set
            {
                this._OvhdID = value;
            }
        }
        #endregion
        #region OvhdType
        public abstract class ovhdType : PX.Data.BQL.BqlString.Field<ovhdType> { }

        protected string _OvhdType;
        [PXDefault(typeof(Search<AMOverhead.ovhdType, Where<AMOverhead.ovhdID, Equal<Current<AMEstimateOvhd.ovhdID>>>>))]
        [OverheadType.List]
        [PXDBString]
        [PXUIField(DisplayName = "Type", Enabled = false)]
        public virtual string OvhdType
        {
            get
            {
                return this._OvhdType;
            }
            set
            {
                this._OvhdType = value;
            }
        }
        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDefault(typeof(Search<AMOverhead.descr, Where<AMOverhead.ovhdID, Equal<Current<AMEstimateOvhd.ovhdID>>>>))]
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public virtual String Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }
        #endregion
        #region Overhead Cost Rate 
        public abstract class overheadCostRate : PX.Data.BQL.BqlDecimal.Field<overheadCostRate> { }

        protected Decimal? _OverheadCostRate;
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Overhead Cost Rate", Enabled = false)]
        [PXDefault(typeof(Search<AMOverhead.costRate, Where<AMOverhead.ovhdID, Equal<Current<AMEstimateOvhd.ovhdID>>>>))]
        public virtual Decimal? OverheadCostRate
        {
            get
            {
                return this._OverheadCostRate;
            }
            set
            {
                this._OverheadCostRate = value;
            }
        }
        #endregion
        #region OFactor
        public abstract class oFactor : PX.Data.BQL.BqlDecimal.Field<oFactor> { }

        protected Decimal? _OFactor;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Factor")]
        public virtual Decimal? OFactor
        {
            get
            {
                return this._OFactor;
            }
            set
            {
                this._OFactor = value;
            }
        }
        #endregion
        #region Fixed Overhead Oper Cost
        public abstract class fixedOvhdOperCost : PX.Data.BQL.BqlDecimal.Field<fixedOvhdOperCost> { }

        protected Decimal? _FixedOvhdOperCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(IsNull<Switch<Case<Where<AMEstimateOvhd.ovhdType, Equal<OverheadType.fixedType>>, Mult<AMEstimateOvhd.overheadCostRate,
            AMEstimateOvhd.oFactor>>, Case<Where<AMEstimateOvhd.ovhdType, NotEqual<OverheadType.fixedType>>, decimal0>>, decimal0>), 
            typeof(SumCalc<AMEstimateOper.fixedOverheadCalcCost>))]
        public virtual Decimal? FixedOvhdOperCost
        {
            get
            {
                return this._FixedOvhdOperCost;
            }
            set
            {
                this._FixedOvhdOperCost = value;
            }
        }
        #endregion
        #region Variable Overhead Oper Cost
        public abstract class variableOvhdOperCost : PX.Data.BQL.BqlDecimal.Field<variableOvhdOperCost> { }

        protected Decimal? _VariableOvhdOperCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(IsNull<Switch<Case<Where<AMEstimateOvhd.ovhdType, Equal<OverheadType.fixedType>>, decimal0,
            Case<Where<AMEstimateOvhd.ovhdType, Equal<OverheadType.varLaborHrs>>, Mult<AMEstimateOvhd.overheadCostRate,
                Mult<AMEstimateOvhd.oFactor, Add<Div<Mult<decimal1, Parent<AMEstimateOper.setupTime>>, 
                    Common.BQLConstants.decimal60>, Parent<AMEstimateOper.runTimeHours>>>>>>,
            Case<Where<AMEstimateOvhd.ovhdType, Equal<OverheadType.varLaborCost>>, Mult<AMEstimateOvhd.overheadCostRate,
                Mult<AMEstimateOvhd.oFactor, Add<Parent<AMEstimateOper.variableLaborCost>, Parent<AMEstimateOper.fixedLaborCost>>>>,
            Case<Where<AMEstimateOvhd.ovhdType, Equal<OverheadType.varMatlCost>>, Mult<AMEstimateOvhd.overheadCostRate,
                Mult<AMEstimateOvhd.oFactor, Parent<AMEstimateOper.materialCost>>>,
            Case<Where<AMEstimateOvhd.ovhdType, Equal<OverheadType.varMachHrs>>, Mult<AMEstimateOvhd.overheadCostRate,
                Mult<AMEstimateOvhd.oFactor, Parent<AMEstimateOper.machineTimeHours>>>,
            Case<Where<AMEstimateOvhd.ovhdType, Equal<OverheadType.varQtyComp>>, Mult<AMEstimateOvhd.overheadCostRate,
                Mult<AMEstimateOvhd.oFactor, Parent<AMEstimateOper.baseOrderQty>>>,
            Case<Where<AMEstimateOvhd.ovhdType, Equal<OverheadType.varQtyTot>>, Mult<AMEstimateOvhd.overheadCostRate,
                Mult<AMEstimateOvhd.oFactor, Parent<AMEstimateOper.baseOrderQty>>>>>>>>>, decimal0>),
            typeof(SumCalc<AMEstimateOper.variableOverheadCalcCost>))]
        public virtual Decimal? VariableOvhdOperCost
        {
            get
            {
                return this._VariableOvhdOperCost;
            }
            set
            {
                this._VariableOvhdOperCost = value;
            }
        }
        #endregion
        #region WCFlag
        public abstract class wCFlag : PX.Data.BQL.BqlBool.Field<wCFlag> { }

        protected Boolean? _WCFlag;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "WC Flag", Visible = true, Enabled = false)]
        public virtual Boolean? WCFlag
        {
            get
            {
                return this._WCFlag;
            }
            set
            {
                this._WCFlag = value;
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

#pragma warning disable PX1031 // DACs cannot contain instance methods
        /// <summary>
        /// Makes a copy of the object excluding created by, last mod by, and timestamps fields
        /// </summary>
        /// <returns>new object with copied values</returns>
        [Obsolete("Use PXCache<>.CreateCopy. " + InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual AMEstimateOvhd Copy()
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            return new AMEstimateOvhd
            {
                EstimateID = this.EstimateID,
                RevisionID = this.RevisionID,
                OperationID = this.OperationID,
                LineID = this.LineID,
                OvhdID = this.OvhdID,
                OFactor = this.OFactor,
                OvhdType = this.OvhdType,
                Description = this.Description,
                OverheadCostRate = this.OverheadCostRate,
                FixedOvhdOperCost = this.FixedOvhdOperCost,
                VariableOvhdOperCost = this.VariableOvhdOperCost,
                WCFlag = this.WCFlag
            };
        }
    }
}