using System;

using PX.Data;

using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents an overdue charge code, which is used to calculate overdue charges
	/// used for late payments in the collection process. The record encapsulates
	/// various information about the overdue charge, such as the calculation method,
	/// overdue fee rates, and overdue GL accounts. The entities of this type are 
	/// created and edited on the Overdue Charges (AR204500) form, which corresponds
	/// to the <see cref="ARFinChargesMaint"/> graph. The overdue charge codes are
	/// then used in the Calculate Overdue Charges (AR507000) processing, which
	/// corresponds to the <see cref="ARFinChargesApplyMaint"/> graph.
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(ARFinChargesMaint))]
	[PXCacheName(Messages.ARFinCharge)]
	public partial class ARFinCharge : IBqlTable
	{
		#region FinChargeID
		public abstract class finChargeID : PX.Data.BQL.BqlString.Field<finChargeID> { }
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Overdue Charge ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(ARFinCharge.finChargeID))]
		public virtual string FinChargeID
		{
			get;
			set;
		}
		#endregion
		#region FinChargeDesc
		public abstract class finChargeDesc : PX.Data.BQL.BqlString.Field<finChargeDesc> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string FinChargeDesc
		{
			get;
			set;
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Terms.termsID,
			Where<Terms.visibleTo, Equal<TermsVisibleTo.all>,
			Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		public virtual string TermsID
		{
			get;
			set;
		}
		#endregion
		#region BaseCurFlag
		public abstract class baseCurFlag : PX.Data.BQL.BqlBool.Field<baseCurFlag> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Base Currency")]
		public virtual bool? BaseCurFlag
		{
			get;
			set;
		}
		#endregion
		#region MinFinChargeFlag
		public abstract class minFinChargeFlag : PX.Data.BQL.BqlBool.Field<minFinChargeFlag> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Line Minimum Amount")]
		public virtual bool? MinFinChargeFlag
		{
			get;
			set;
		}
		#endregion
		#region MinFinChargeAmount
		public abstract class minFinChargeAmount : PX.Data.BQL.BqlDecimal.Field<minFinChargeAmount> { }
		[PXDBBaseCury(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Min. Amount")]
		public virtual decimal? MinFinChargeAmount
		{
			get;
			set;
		}
		#endregion
		#region LineThreshold
		public abstract class lineThreshold : PX.Data.BQL.BqlDecimal.Field<lineThreshold> { }
		[PXBaseCury(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Threshold")]
		public virtual decimal? LineThreshold
		{
			get
			{
				return this.MinFinChargeAmount;
			}

			set
			{
				this.MinFinChargeAmount = value;
			}
		}
		#endregion
		#region FixedAmount
		public abstract class fixedAmount : PX.Data.BQL.BqlDecimal.Field<fixedAmount> { }
		[PXBaseCury(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? FixedAmount
		{
			get
			{
				return this.MinFinChargeAmount;
			}

			set
			{
				this.MinFinChargeAmount = value;
			}
		}
		#endregion
		#region MinChargeDocumentAmt
		public abstract class minChargeDocumentAmt : PX.Data.BQL.BqlDecimal.Field<minChargeDocumentAmt> { }
		[PXDBBaseCury(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Threshold")]
		public virtual decimal? MinChargeDocumentAmt
		{
			get;
			set;
		}
		#endregion
		#region PercentFlag
		public abstract class percentFlag : PX.Data.BQL.BqlBool.Field<percentFlag> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Percent Rate")]
		public virtual bool? PercentFlag
		{
			get;
			set;
		}
		#endregion
		#region FinChargeAcctID
		public abstract class finChargeAccountID : PX.Data.BQL.BqlInt.Field<finChargeAccountID> { }

		[PXDefault]
		[PXNonCashAccount(DisplayName = "Overdue Charge Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = true, AvoidControlAccounts = true)]
		public virtual int? FinChargeAccountID
		{
			get;
			set;
		}
		#endregion
		#region FinChargeSubID
		public abstract class finChargeSubID : PX.Data.BQL.BqlInt.Field<finChargeSubID> { }

		[PXDefault]
		[SubAccount(typeof(ARFinCharge.finChargeAccountID), DisplayName = "Overdue Charge Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = true)]
		public virtual int? FinChargeSubID
		{
			get;
			set;
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<ARFinCharge.taxCategoryID>.IsRelatedTo<TaxCategory.taxCategoryID>))]
		public virtual string TaxCategoryID
		{
			get;
			set;
		}
		#endregion
		#region FeeAcctID
		public abstract class feeAccountID : PX.Data.BQL.BqlInt.Field<feeAccountID> { }
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXNonCashAccount(DisplayName = "Fee Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual int? FeeAccountID
		{
			get;
			set;
		}
		#endregion
		#region FeeSubID
		public abstract class feeSubID : PX.Data.BQL.BqlInt.Field<feeSubID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(ARFinCharge.finChargeAccountID), DisplayName = "Fee Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual int? FeeSubID
		{
			get;
			set;
		}
		#endregion
		#region FeeAmount
		public abstract class feeAmount : PX.Data.BQL.BqlDecimal.Field<feeAmount> { }
		[PXDBDecimal(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Fee Amount", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual decimal? FeeAmount
		{
			get;
			set;
		}
		#endregion
		#region FeeDesc
		public abstract class feeDesc : PX.Data.BQL.BqlString.Field<feeDesc> { }
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Fee Description")]
		public virtual string FeeDesc
		{
			get;
			set;
		}
		#endregion
		#region CalculationMethod
		public abstract class calculationMethod : PX.Data.BQL.BqlInt.Field<calculationMethod> { }
		[PXDBInt]
		[PXDefault(0)]
		[PXIntList(new int[]
		{
			OverdueCalculationMethod.InterestOnBalance,
			OverdueCalculationMethod.InterestOnProratedBalance,
			OverdueCalculationMethod.InterestOnArrears
		}, new string[]
		{
			Messages.InterestOnBalance,
			Messages.InterestOnProratedBalance,
			Messages.InterestOnArrears
		})]
		[PXUIField(DisplayName = "Calculation Method", Visibility = PXUIVisibility.Visible)]
		public virtual int? CalculationMethod
		{
			get;
			set;
		}
		#endregion
		#region ChargingMethod
		public abstract class chargingMethod : PX.Data.BQL.BqlInt.Field<chargingMethod> { }
		[PXInt]
		[PXDefault(1)]
		[PXIntList(new int[]
		{
			OverdueChargingMethod.FixedAmount,
			OverdueChargingMethod.PercentWithThreshold,
			OverdueChargingMethod.PercentWithMinAmount
		}, new string[]
		{
			Messages.FixedAmount,
			Messages.PercentWithThreshold,
			Messages.PercentWithMinAmount
		})]
		[PXUIField(DisplayName = "Charging Method")]
		public virtual int? ChargingMethod
		{
			get
			{
				if (MinFinChargeFlag == true && PercentFlag == false)
				{
					return OverdueChargingMethod.FixedAmount;
				}
				else if (MinFinChargeFlag == false && PercentFlag == true)
				{
					return OverdueChargingMethod.PercentWithThreshold;
				}
				else if (MinFinChargeFlag == true && PercentFlag == true)
				{
					return OverdueChargingMethod.PercentWithMinAmount;
				}
				else
				{
					return null;
				}
			}

			set
			{
				switch (value)
				{
					case OverdueChargingMethod.FixedAmount:
						MinFinChargeFlag = true;
						PercentFlag = false;
						break;
					case OverdueChargingMethod.PercentWithThreshold:
						MinFinChargeFlag = false;
						PercentFlag = true;
						break;
					case OverdueChargingMethod.PercentWithMinAmount:
						MinFinChargeFlag = true;
						PercentFlag = true;
						break;
				}
			}
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}

	public class OverdueChargingMethod
	{
		public const int FixedAmount = 1;
		public const int PercentWithThreshold = 2;
		public const int PercentWithMinAmount = 3;
	}

	public class OverdueCalculationMethod
	{
		public const int InterestOnBalance = 0;
		public const int InterestOnProratedBalance = 1;
		public const int InterestOnArrears = 2;
	}
}
