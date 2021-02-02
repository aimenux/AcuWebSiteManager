using System;
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.PM;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.EstimateReference)]
    [PXPrimaryGraph(typeof(EstimateMaint))]
    [System.Diagnostics.DebuggerDisplay("EstimateID = {EstimateID}; RevisionID = {RevisionID}")]
    public class AMEstimateReference : IBqlTable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMEstimateReference>.By<estimateID, revisionID>
        {
            public static AMEstimateReference Find(PXGraph graph, string estimateID, string revisionID)
                => FindBy(graph, estimateID, revisionID);
            public static AMEstimateReference FindDirty(PXGraph graph, string estimateID, string revisionID)
                => PXSelect<AMEstimateReference,
                    Where<estimateID, Equal<Required<estimateID>>,
                        And<revisionID, Equal<Required<revisionID>>>>>
                    .SelectWindowed(graph, 0, 1, estimateID, revisionID);
        }

        public static class FK
        {
            public class Estimate : AMEstimateItem.PK.ForeignKeyOf<AMEstimateReference>.By<estimateID, revisionID> { }
        }

        #endregion

        #region Estimate ID
        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

        protected String _EstimateID;
        [PXDBDefault(typeof(AMEstimateItem.estimateID))]
        [EstimateID(IsKey = true, Enabled = false)]
        [EstimateIDSelectAll(ValidateValue = false)]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }
        #endregion
        #region Revision ID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;
        [PXDBDefault(typeof(AMEstimateItem.revisionID))]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">AAAAAAAAAA")]
        [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        // Using optional and current for both the UI/page and to satisfy the report writer (AM641000)
        // Importing estimates and if validation the selector it thinks the revision doesn't exist (so we need validatevalue = false)
        [EstimateRevisionIDSelector(typeof(Search<AMEstimateItem.revisionID,
            Where<AMEstimateItem.estimateID, Equal<Optional<AMEstimateReference.estimateID>>,
                Or<AMEstimateItem.estimateID, Equal<Current<AMEstimateReference.estimateID>>>>>),
            ValidateValue = false)]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }
        #endregion
        #region Branch ID 
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        protected Int32? _BranchID;
        [Branch]
        public virtual Int32? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion
        #region Source (obsolete)
        //LEAVE THIS FIELD FOR C# UPGRADE PROCESS
        [Obsolete("AMEstimateReference.source has moved to AMEstimateItem.quoteSource in 2018R1")]
        public abstract class source : PX.Data.BQL.BqlInt.Field<source> { }

        protected int? _Source;
        [Obsolete("AMEstimateReference.Source has moved to AMEstimateItem.QuoteSource in 2018R1")]
        [PXDBInt]
        [PXUIField(DisplayName = "Source (obsolete)", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual int? Source
        {
            get
            {
                return this._Source;
            }
            set
            {
                this._Source = value;
            }
        }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        protected Int64? _CuryInfoID;
        [PXDBLong]
        [CurrencyInfo]
        public virtual Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion
        #region OpportunityID
#if DEBUG
        //We are not going to mark as obsolete as we will continue to show the opp number for reference since the new key is a guid (OpportunityQuoteID) 
        //[Obsolete("Removed by Acumatica in 2018R2", true)]
#endif
        public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Opportunity ID", Enabled = false)]
        [PXSelector(typeof(Search<CROpportunity.opportunityID>), ValidateValue = false)]
        public virtual String OpportunityID { get; set; }
        #endregion
        #region OpportunityQuoteID
        public abstract class opportunityQuoteID : PX.Data.BQL.BqlGuid.Field<opportunityQuoteID> { }
        [PXDBGuid]
        [PXUIField(DisplayName = "Opportunity Quote ID", Enabled = false, Visible = false)]
        public virtual Guid? OpportunityQuoteID { get; set; }
        #endregion
        #region QuoteType
        public abstract class quoteType : PX.Data.BQL.BqlString.Field<quoteType> { }

        protected String _QuoteType;
        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "Quote Type")]
        public virtual String QuoteType
        {
            get
            {
                return this._QuoteType;
            }
            set
            {
                this._QuoteType = value;
            }
        }
        #endregion
        #region QuoteNbr
        public abstract class quoteNbr : PX.Data.BQL.BqlString.Field<quoteNbr> { }

        protected String _QuoteNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Quote Nbr")]
        public virtual String QuoteNbr
        {
            get
            {
                return this._QuoteNbr;
            }
            set
            {
                this._QuoteNbr = value;
            }
        }
        #endregion
        #region QuoteNbrLink (Unbound)
        /// <summary>
        /// Non-editable version of quoteNbr used on the page for hyper-link
        /// </summary>
        public abstract class quoteNbrLink : PX.Data.BQL.BqlString.Field<quoteNbrLink> { }

        /// <summary>
        /// Non-editable version of QuoteNbr used on the page for hyper-link
        /// </summary>
        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Quote Nbr", Visibility = PXUIVisibility.Invisible, Enabled = false)]
        public virtual String QuoteNbrLink
        {
            get
            {
                return this._QuoteNbr;
            }
            set
            {
                this._QuoteNbr = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "Order Type")]
        [PXSelector(typeof(Search<SOOrderType.orderType>), ValidateValue = false)]
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
        #region OrderNbr
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

        protected String _OrderNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Order Nbr")]
        [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<AMEstimateReference.orderType>>>>), ValidateValue = false)]
        public virtual String OrderNbr
        {
            get
            {
                return this._OrderNbr;
            }
            set
            {
                this._OrderNbr = value;
            }
        }
        #endregion
        #region TaxLineNbr
        public abstract class taxLineNbr : PX.Data.BQL.BqlInt.Field<taxLineNbr> { }

        protected Int32? _TaxLineNbr;
        [PXDBInt]
        [PXUIField(DisplayName = "Tax Line Nbr.", Visible = false, Enabled = false)]
        public virtual Int32? TaxLineNbr
        {
            get
            {
                return this._TaxLineNbr;
            }
            set
            {
                this._TaxLineNbr = value;
            }
        }
        #endregion
        #region Tax Category ID
        public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        protected String _TaxCategoryID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Search<AMEstimateClass.taxCategoryID, Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Tax Category")]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), PX.Objects.TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
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
        #region Order Qty
        public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }

        protected Decimal? _OrderQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0000", typeof(AMEstimateItem.orderQty))]
        [PXUIField(DisplayName = "Order Qty")]
        [PXFormula(null, typeof(SumCalc<SOOrderExt.aMEstimateQty>))]
        //[PXFormula(null, typeof(SumCalc<CRQuoteExt.aMEstimateQty>))]
        [PXFormula(null, typeof(SumCalc<CROpportunityExt.aMEstimateQty>))]
        [PXUnboundFormula(typeof(AMEstimateReference.orderQty), typeof(SumCalc<SOOrder.orderQty>))]
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
        #region Cury Unit Price
        public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

        protected Decimal? _CuryUnitPrice;
        [PXDBCurrency(typeof(Search<PX.Objects.CS.CommonSetup.decPlPrcCst>),typeof(AMEstimateReference.curyInfoID), typeof(AMEstimateReference.unitPrice))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Unit Price", Enabled = false)]
        public virtual Decimal? CuryUnitPrice
        {
            get
            {
                return this._CuryUnitPrice;
            }
            set
            {
                this._CuryUnitPrice = value;
            }
        }
        #endregion
        #region Unit Price
        public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

        protected Decimal? _UnitPrice;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Unit Price", Enabled = false)]
        public virtual Decimal? UnitPrice
        {
            get
            {
                return this._UnitPrice;
            }
            set
            {
                this._UnitPrice = value;
            }
        }
        #endregion
        #region Cury Ext Price
        public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }

        protected Decimal? _CuryExtPrice;
        [PXDBCurrency(typeof(AMEstimateReference.curyInfoID), typeof(AMEstimateReference.extPrice))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Price", Enabled = false)]
        [PXFormula(typeof(Mult<AMEstimateReference.curyUnitPrice, AMEstimateReference.orderQty>),
            typeof(SumCalc<SOOrderExt.aMCuryEstimateTotal>))]
        //[PXFormula(typeof(Mult<AMEstimateReference.curyUnitPrice, AMEstimateReference.orderQty>),
        //    typeof(SumCalc<CRQuoteExt.aMCuryEstimateTotal>))]
        [PXFormula(typeof(Mult<AMEstimateReference.curyUnitPrice, AMEstimateReference.orderQty>),
            typeof(SumCalc<CROpportunityExt.aMCuryEstimateTotal>))]
        public virtual Decimal? CuryExtPrice
        {
            get
            {
                return this._CuryExtPrice;
            }
            set
            {
                this._CuryExtPrice = value;
            }
        }
        #endregion
        #region Ext Price
        public abstract class extPrice : PX.Data.BQL.BqlDecimal.Field<extPrice> { }

        protected Decimal? _ExtPrice;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Price", Enabled = false)]
        public virtual Decimal? ExtPrice
        {
            get
            {
                return this._ExtPrice;
            }
            set
            {
                this._ExtPrice = value;
            }
        }
        #endregion
        #region BAccount ID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        protected Int32? _BAccountID;
        [PX.Objects.CR.CustomerAndProspect]
        [PXForeignReference(typeof(Field<bAccountID>.IsRelatedTo<BAccount.bAccountID>))]
        public virtual Int32? BAccountID
        {
            get
            {
                return this._BAccountID;
            }
            set
            {
                this._BAccountID = value;
            }
        }
        #endregion
        #region External Ref Nbr
        public abstract class externalRefNbr : PX.Data.BQL.BqlString.Field<externalRefNbr> { }

        protected String _ExternalRefNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Ext. Ref. Nbr.")]
        public virtual String ExternalRefNbr
        {
            get
            {
                return this._ExternalRefNbr;
            }
            set
            {
                this._ExternalRefNbr = value;
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
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        protected Int32? _ProjectID;
        [ProjectDefault]
        [ActiveProjectOrContractForProd(FieldClass = ProjectAttribute.DimensionName)]
        public virtual Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

        protected Int32? _TaskID;
        [ActiveOrInPlanningProjectTaskForProd(typeof(AMEstimateReference.projectID))]
        public virtual Int32? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
            }
        }
        #endregion
        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        protected Int32? _CostCodeID;
        [CostCodeForProd(null, typeof(taskID), null)]
        public virtual Int32? CostCodeID
        {
            get
            {
                return this._CostCodeID;
            }
            set
            {
                this._CostCodeID = value;
            }
        }
        #endregion
    }
}