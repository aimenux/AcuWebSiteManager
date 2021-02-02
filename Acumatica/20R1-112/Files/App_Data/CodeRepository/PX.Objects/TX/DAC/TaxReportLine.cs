namespace PX.Objects.TX
{
	using System;
	using System.Collections.Generic;
	using PX.Data;
	using PX.Objects.AP;
	using PX.Objects.Common;


	/// <summary>
	/// Represents a line of a tax report. The class defines the structure of the tax report for a particular tax agency, and is a part of formation amount rules.
	/// The line is mapped as many-to-many to reporting groups(TaxBucket) and through them to taxes.
    /// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxReportLine)]
	public partial class TaxReportLine : IBqlTable, ISortOrder
	{
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;

		/// <summary>
		/// The foreign key to <see cref="PX.Objects.AP.Vendor">, which specifies a tax agency to which the report line belongs.
		/// The field is a part of the primary key.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;

		/// <summary>
		/// The number of the report line. The field is a part of the primary key.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(VendorMaster.lineCntr), DecrementOnDelete = false)]
		[PXParent(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<TaxReportLine.vendorID>>>>))]
		[PXUIField(DisplayName="Report Line Nbr.", Visibility=PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual Int32? LineNbr
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
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;

		/// <summary>
		/// The type of the report line, which indicates whether the tax amount or taxable amount should be used to update the line.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"P"</c>: Tax amount.
		/// <c>"A"</c>: Taxable amount.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxReportLineType.TaxAmount)]
		[PXUIField(DisplayName="Update With", Visibility=PXUIVisibility.Visible)]
		[TaxReportLineType.List]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region LineMult
		public abstract class lineMult : PX.Data.BQL.BqlShort.Field<lineMult> { }
		protected Int16? _LineMult;

		/// <summary>
		/// The rule (sign) of updating the report line.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"1"</c>: +Output-Input.
		/// <c>"-1"</c>: +Input-Output.
		/// </value>
		[PXDBShort()]
		[PXDefault((short)1)]
		[PXUIField(DisplayName="Update Rule", Visibility=PXUIVisibility.Visible)]
		[PXIntList(new int[] { 1, -1 }, new string[] { "+Output-Input", "+Input-Output" })]
		public virtual Int16? LineMult
		{
			get
			{
				return this._LineMult;
			}
			set
			{
				this._LineMult = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;

		/// <summary>
		/// The foreign key to <see cref="TaxZone"/>.
		/// If the field contains NULL, the report line contains aggregated data for all tax zones.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName="Tax Zone ID", Visibility=PXUIVisibility.Visible, Required=false)]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>))]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region NetTax
		public abstract class netTax : PX.Data.BQL.BqlBool.Field<netTax> { }
		protected Boolean? _NetTax;

		/// <summary>
		/// The field indicates (if set to <c>true</c>) that the line shows the net tax amount.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Net Tax", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual Boolean? NetTax
		{
			get
			{
				return this._NetTax;
			}
			set
			{
				this._NetTax = value;
			}
		}
		#endregion
		#region TempLine
		public abstract class tempLine : PX.Data.BQL.BqlBool.Field<tempLine> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the report line should be sliced by tax zones, and that the line is parent.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Detail by Tax Zones", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual bool? TempLine
		{
			get;
			set;
		}
		#endregion
		#region TempLineNbr
		public abstract class tempLineNbr : PX.Data.BQL.BqlInt.Field<tempLineNbr> { }
		protected Int32? _TempLineNbr;

		/// <summary>
		/// The reference to the parent line (<see cref="TaxReportLine.LineNbr"/>).
		/// The child lines are created for each tax zone if the <see cref="TempLine"/> of the parent line is set to <c>true</c>.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Parent Line")]
		public virtual Int32? TempLineNbr
		{
			get
			{
				return this._TempLineNbr;
			}
			set
			{
				this._TempLineNbr = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

		/// <summary>
		/// The description of the report line, which can be specified by the user.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName="Description", Visibility=PXUIVisibility.SelectorVisible)]
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
        #region ReportLineNbr
        public abstract class reportLineNbr : PX.Data.BQL.BqlString.Field<reportLineNbr> { }
        protected String _ReportLineNbr;

		/// <summary>
		/// The number of the corresponding box of the original report form; the number is unique for each tax agency.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Box Number", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String ReportLineNbr
        {
            get
            {
                return this._ReportLineNbr;
            }
            set
            {
                this._ReportLineNbr = value;
            }
        }
        #endregion
        #region BucketSum
        public abstract class bucketSum : PX.Data.BQL.BqlString.Field<bucketSum> { }

		/// <summary>
		/// The calculation rule, which is filled in by the system automatically if the report line is an aggregate line and the appropriate settings have been specified.
		/// </summary>
		[PXString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Calc. Rule", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
        public virtual String BucketSum { get; set; }
        #endregion
		#region HideReportLine
		public abstract class hideReportLine : PX.Data.BQL.BqlBool.Field<hideReportLine> { }
		protected Boolean? _HideReportLine;

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the line will not be included in the tax report during generation of the report.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hide Report Line", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual Boolean? HideReportLine
		{
			get
			{
				return this._HideReportLine;
			}
			set
			{
				this._HideReportLine = value;
			}
		}
		#endregion

		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Report Line Order", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? SortOrder
		{
			get;
			set;
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
	}

	public class TaxReportLineType : ILabelProvider
	{
		public const string TaxAmount = "P";
		public const string TaxableAmount = "A";
		public const string ExemptedAmount = "E";

		protected static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ TaxAmount, Messages.TaxAmountLine },
			{ TaxableAmount, Messages.TaxableAmountLine },
			{ ExemptedAmount, Messages.ExemptedAmountLine }
		};

		public static readonly string[] Values =
		{
			TaxAmount,
			TaxableAmount,
			ExemptedAmount
		};

		public static readonly string[] Labels =
		{
			Messages.TaxAmountLine,
			Messages.TaxableAmountLine,
			Messages.ExemptedAmountLine
		};

		public class ListAttribute : LabelListAttribute
		{
			public ListAttribute() : base(_valueLabelPairs) { }
		}

		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

		#region Constant classes

		public class taxAmount : PX.Data.BQL.BqlString.Constant<taxAmount>
		{
			public taxAmount() : base(TaxAmount) { }
		}

		public class taxableAmount : PX.Data.BQL.BqlString.Constant<taxableAmount>
		{
			public taxableAmount() : base(TaxableAmount) { }
		}

		public class exemptedAmount : Constant<string>
		{
			public exemptedAmount() : base(ExemptedAmount) { }
		}

		#endregion
	}
}
