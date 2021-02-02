using System;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX.Descriptor;

namespace PX.Objects.TX
{
	public class TaxType : ILabelProvider
	{
		private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ Sales, Messages.Output },
			{ Purchase, Messages.Input },
		};
		
		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

		public const string Sales = "S";
        public const string Purchase = "P";
        public const string PendingSales = "A";
        public const string PendingPurchase = "B";

        public class sales : PX.Data.BQL.BqlString.Constant<sales>
		{
            public sales() : base(Sales) { ;}
        }

        public class purchase : PX.Data.BQL.BqlString.Constant<purchase>
		{
            public purchase() : base(Purchase) { ;}
        }

        public class pendingSales : PX.Data.BQL.BqlString.Constant<pendingSales>
		{
            public pendingSales() : base(PendingSales) { ;}
        }

        public class pendingPurchase : PX.Data.BQL.BqlString.Constant<pendingPurchase>
		{
            public pendingPurchase() : base(PendingPurchase) { ;}
        }
    }

    [System.SerializableAttribute()]
    public partial class TaxBucketMaster : IBqlTable
    {
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;

        [TaxAgencyActive]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #region BucketID
        public abstract class bucketID : PX.Data.BQL.BqlInt.Field<bucketID>
		{
            public class TaxBucketSelectorAttribute : PXSelectorAttribute
            {
                public TaxBucketSelectorAttribute()
                    : base(typeof(Search<TaxBucket.bucketID,
                        Where<TaxBucket.vendorID, Equal<Current<TaxBucketMaster.vendorID>>>>))
                {
                    DescriptionField = typeof(TaxBucket.name);
                    _UnconditionalSelect = new Search<TaxBucket.bucketID,
                        Where<TaxBucket.vendorID, Equal<Current<TaxBucketMaster.vendorID>>,
                            And<TaxBucket.bucketID, Equal<Required<TaxBucket.bucketID>>>>>();
                }
            }
        }
        protected Int32? _BucketID;
        [PXDBInt()]
        //[PXIntList(new int[] { 0 }, new string[] { "undefined" })]
        [bucketID.TaxBucketSelector]
        [PXUIField(DisplayName = "Reporting Group", Visibility = PXUIVisibility.Visible)]
        public virtual Int32? BucketID
        {
            get
            {
                return this._BucketID;
            }
            set
            {
                this._BucketID = value;
            }
        }
        #endregion
        #region BucketType
        public abstract class bucketType : PX.Data.BQL.BqlString.Field<bucketType> { }
        protected String _BucketType;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(TaxType.Sales)]
        [PXUIField(DisplayName = "Group Type", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [LabelList(typeof(TaxType))]
        public virtual String BucketType
        {
            get
            {
                return this._BucketType;
            }
            set
            {
                this._BucketType = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected String _Descr;
        [PXDBString(60, IsUnicode = true)]
        public virtual String Descr
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
    }

    public class TaxBucketMaint : PXGraph<TaxBucketMaint>
    {
        public PXSave<TaxBucketMaster> Save;
        public PXCancel<TaxBucketMaster> Cancel;

        public PXFilter<TaxBucketMaster> Bucket;
        public PXSelectJoin<TaxBucketLine, 
        	InnerJoin<TaxReportLine, 
        		On<TaxReportLine.vendorID, Equal<TaxBucketLine.vendorID>, 
        		And<TaxReportLine.lineNbr, Equal<TaxBucketLine.lineNbr>>>>,
        	Where<TaxBucketLine.vendorID, Equal<Current<TaxBucketMaster.vendorID>>,
        		And<TaxBucketLine.bucketID, Equal<Current<TaxBucketMaster.bucketID>>,
        		And<TaxReportLine.tempLineNbr, IsNull>>>,
			OrderBy<
				Asc<TaxReportLine.sortOrder>>> BucketLine;

	    public PXSelect<TaxReportLine> ReportLineView;

    	public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Current<TaxBucketMaster.vendorID>>>> Vendor; 
       
        public TaxBucketMaint()
        {
		    FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.VendorType; });
        }

        private void ValidateBucket()
        {
            TaxBucketMaster currBucket = this.Bucket.Current;
			TaxReportMaint.TaxBucketAnalizer TestAnalyzerTax = new TaxReportMaint.TaxBucketAnalizer(this, (int)currBucket.VendorID, TaxReportLineType.TaxAmount);
			TestAnalyzerTax.DoChecks((int)currBucket.BucketID);
			TaxReportMaint.TaxBucketAnalizer TestAnalyzerTaxable = new TaxReportMaint.TaxBucketAnalizer(this, (int)currBucket.VendorID, TaxReportLineType.TaxableAmount);
			TestAnalyzerTaxable.DoChecks((int)currBucket.BucketID);
        }

        public override void Persist()
        {
            ValidateBucket();
	        TaxReportMaint.CheckReportSettingsEditable(this, Bucket.Current.VendorID);
        	
			base.Persist();
        }

	    protected virtual void TaxBucketMaster_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
	    {
		    var taxBucket = e.Row as TaxBucketMaster;

		    if (taxBucket == null)
			    return;

		    TaxReportMaint.CheckReportSettingsEditableAndSetWarningTo<TaxBucketMaster.vendorID>(this, cache, taxBucket,
			    taxBucket.VendorID);
	    }

        protected virtual void TaxBucketLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            foreach (PXResult<TaxReportLine, TaxBucketLine> res in PXSelectJoin<TaxReportLine, LeftJoin<TaxBucketLine, On<TaxBucketLine.vendorID, Equal<TaxReportLine.vendorID>, And<TaxBucketLine.lineNbr, Equal<TaxReportLine.lineNbr>>>>, Where<TaxReportLine.vendorID, Equal<Required<TaxReportLine.vendorID>>, And<TaxReportLine.tempLineNbr, Equal<Required<TaxReportLine.tempLineNbr>>, And<TaxBucketLine.bucketID, Equal<Required<TaxBucketLine.bucketID>>>>>>.Select(this, ((TaxBucketLine)e.Row).VendorID, ((TaxBucketLine)e.Row).LineNbr, ((TaxBucketLine)e.Row).BucketID))
            {
                if (((TaxBucketLine)res).BucketID != null)
                {
                    BucketLine.Cache.Delete((TaxBucketLine) res);
                }
            }
        }

        protected virtual void TaxBucketLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            foreach (PXResult<TaxReportLine, TaxBucketLine> res in PXSelectJoin<TaxReportLine, LeftJoin<TaxBucketLine, On<TaxBucketLine.vendorID, Equal<TaxReportLine.vendorID>, And<TaxBucketLine.lineNbr, Equal<TaxReportLine.lineNbr>>>>, Where<TaxReportLine.vendorID, Equal<Required<TaxReportLine.vendorID>>, And<TaxReportLine.tempLineNbr, Equal<Required<TaxReportLine.tempLineNbr>>>>>.Select(this, ((TaxBucketLine)e.Row).VendorID, ((TaxBucketLine)e.Row).LineNbr))
            {
                TaxBucketLine new_bucket = PXCache<TaxBucketLine>.CreateCopy((TaxBucketLine)e.Row);
                new_bucket.LineNbr = ((TaxReportLine)res).LineNbr;
                BucketLine.Cache.Insert(new_bucket);
            }
            BucketLine.Cache.Current = e.Row;
        }

		protected virtual void TaxBucketLine_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			var bucketLine = (TaxBucketLine) e.Row;

			var reportLine = new TaxReportLine()
			{
				VendorID = bucketLine.VendorID,
				LineNbr = bucketLine.LineNbr
			};

			reportLine = (TaxReportLine)ReportLineView.Cache.Locate(reportLine);

			if (reportLine?.TempLineNbr == null)//it is parent line
			{
			CheckUnique(cache, bucketLine);
		}
		}

		protected virtual void TaxBucketLine_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var bucketLine = e.Row as TaxBucketLine;

			if (bucketLine == null)
				return;

			PXUIFieldAttribute.SetReadOnly<TaxBucketLine.lineNbr>(cache, bucketLine, true);
		}

	    protected void CheckUnique(PXCache cache, TaxBucketLine bucketLine)
	    {
			var dataRowWithSameLineNbr = PXSelectJoin<TaxBucketLine,
														InnerJoin<TaxReportLine,
															On<TaxReportLine.vendorID, Equal<TaxBucketLine.vendorID>,
																And<TaxReportLine.lineNbr, Equal<TaxBucketLine.lineNbr>>>>,
														Where<TaxBucketLine.vendorID, Equal<Current<TaxBucketMaster.vendorID>>,
															And<TaxBucketLine.bucketID, Equal<Current<TaxBucketMaster.bucketID>>,
															And<TaxReportLine.tempLineNbr, IsNull,
															And<TaxBucketLine.lineNbr, Equal<Required<TaxBucketLine.lineNbr>>>>>>>
														.Select(this, bucketLine.LineNbr).AsEnumerable()
														.Cast<PXResult<TaxBucketLine, TaxReportLine>>()
														.ToArray();

			if (dataRowWithSameLineNbr.Any())
			{
				var reportLine = (TaxReportLine)dataRowWithSameLineNbr.First();

				var bucket = (TaxBucket)PXSelect<TaxBucket,
											Where<TaxBucket.vendorID, Equal<Current<TaxBucketMaster.vendorID>>,
												And<TaxBucket.bucketID, Equal<Current<TaxBucketMaster.bucketID>>>>>
											.Select(this);

				throw new PXException(Messages.TheReportingGroupAlreadyContainsTheReportLine, bucket.Name,
					reportLine.Descr);
			}
	    }

        protected virtual void TaxBucketMaster_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            TaxBucket bucket = (TaxBucket)PXSelect<TaxBucket, Where<TaxBucket.vendorID, Equal<Required<TaxBucket.vendorID>>, And<TaxBucket.bucketID, Equal<Required<TaxBucket.bucketID>>>>>.Select(this, ((TaxBucketMaster)e.Row).VendorID, ((TaxBucketMaster)e.Row).BucketID);

            if (bucket != null)
            {
                ((TaxBucketMaster)e.Row).BucketType = bucket.BucketType;
            }
        }       

        public PXSetup<APSetup> APSetup;
    }
}
