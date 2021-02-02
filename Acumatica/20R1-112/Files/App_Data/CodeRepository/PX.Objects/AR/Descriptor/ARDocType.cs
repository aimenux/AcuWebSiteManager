using System;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;


namespace PX.Objects.AR
{
	public class ARDocType : ILabelProvider
	{
		public const string Invoice = "INV";
		public const string DebitMemo = "DRM";
		public const string CreditMemo = "CRM";
		public const string Payment = "PMT";
		public const string VoidPayment = "RPM";
		public const string Prepayment = "PPM";
		public const string Refund = "REF";
        public const string VoidRefund = "VRF";
        public const string FinCharge = "FCH";
		public const string SmallBalanceWO = "SMB";
		public const string SmallCreditWO = "SMC";
		public const string CashSale = "CSL";
		public const string CashReturn = "RCS";
		public const string Undefined = "UND";
		public const string NoUpdate = Undefined;

		protected static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ Invoice, Messages.Invoice },
			{ DebitMemo, Messages.DebitMemo },
			{ CreditMemo, Messages.CreditMemo },
			{ Payment, Messages.Payment },
			{ VoidPayment, Messages.VoidPayment },
			{ Prepayment, Messages.Prepayment },
			{ Refund, Messages.Refund },
            { VoidRefund, Messages.VoidRefund },
            { FinCharge, Messages.FinCharge },
			{ SmallBalanceWO, Messages.SmallBalanceWO },
			{ SmallCreditWO, Messages.SmallCreditWO },
			{ CashSale, Messages.CashSale },
			{ CashReturn, Messages.CashReturn },
		};

		protected static readonly IEnumerable<ValueLabelPair> _valuePrintLabelPairs = new ValueLabelList
		{
			{ Invoice, Messages.PrintInvoice },
			{ DebitMemo, Messages.PrintDebitMemo },
			{ CreditMemo, Messages.PrintCreditMemo },
			{ Payment, Messages.PrintPayment },
			{ VoidPayment, Messages.PrintVoidPayment },
			{ Prepayment, Messages.PrintPrepayment },
			{ Refund, Messages.PrintRefund },
            { VoidRefund, Messages.PrintVoidRefund },
            { FinCharge, Messages.PrintFinCharge },
			{ SmallBalanceWO, Messages.PrintSmallBalanceWO },
			{ SmallCreditWO, Messages.PrintSmallCreditWO },
			{ CashSale, Messages.PrintCashSale },
			{ CashReturn, Messages.PrintCashReturn },
		};

		public static readonly string[] Values = 
		{
			Invoice,
			DebitMemo,
			CreditMemo,
			Payment,
			VoidPayment,
			Prepayment,
			Refund,
            VoidRefund,
			FinCharge,
			SmallBalanceWO,
			SmallCreditWO,
			CashSale,
			CashReturn
		};

		public static readonly string[] Labels = 
		{
			Messages.Invoice,
			Messages.DebitMemo,
			Messages.CreditMemo,
			Messages.Payment,
			Messages.VoidPayment,
			Messages.Prepayment,
			Messages.Refund,
            Messages.VoidRefund,
            Messages.FinCharge,
			Messages.SmallBalanceWO,
			Messages.SmallCreditWO,
			Messages.CashSale,
			Messages.CashReturn
		};

		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

		public class CustomListAttribute : LabelListAttribute
				{
			public string[] AllowedValues => _AllowedValues;

			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(IEnumerable<ValueLabelPair> valueLabelPairs)
				: base (valueLabelPairs)
			{ }
		}

		public class ListAttribute : LabelListAttribute
		{
			public ListAttribute() : base(_valueLabelPairs)
			{ }
		}

		/// <summary>
		/// Defines a Selector of the AR Document types with shorter description.<br/>
		/// In the screens displayed as combo-box.<br/>
		/// Mostly used in the reports.<br/>
		/// </summary>
		public class PrintListAttribute : LabelListAttribute
		{
			public PrintListAttribute() : base(_valuePrintLabelPairs)
			{ }
		}

		public class SOListAttribute  : LabelListAttribute
		{
			private static readonly IEnumerable<ValueLabelPair> _soValueLabelPairs = new ValueLabelList
			{
				{ Invoice, Messages.Invoice },
				{ DebitMemo, Messages.DebitMemo },
				{ CreditMemo, Messages.CreditMemo },
				{ CashSale, Messages.CashSale },
				{ CashReturn, Messages.CashReturn },
				{ NoUpdate, Messages.NoUpdate }
			};

			public SOListAttribute() : base(_soValueLabelPairs)
			{ }
		}
		/// <summary>
		/// Defines the list of AR document types that can be approved.
		/// </summary>
		public class ARApprovalDocTypeListAttribute : LabelListAttribute
		{
			private static readonly IEnumerable<ValueLabelPair> _approvalValueLabelPairs = new ValueLabelList
			{
				{ Invoice, Messages.Invoice },
				{ DebitMemo, Messages.DebitMemo },
				{ CreditMemo, Messages.CreditMemo },
				{ Refund, Messages.Refund },
				{ CashReturn, Messages.CashReturn }
			};

			public ARApprovalDocTypeListAttribute() : base(_approvalValueLabelPairs)
			{ }
		}

		/// <summary>
		/// Defines a list of AR document types that can be used in the SO module.
		/// </summary>
		public class SOEntryListAttribute : CustomListAttribute
		{
			private static readonly string[] _soEntryListValues = { Invoice, DebitMemo, CreditMemo, CashSale, CashReturn };

			public SOEntryListAttribute()
				: base(_valueLabelPairs.Where(pair => _soEntryListValues.Contains(pair.Value)))
			{ }
		}

		public class invoice : PX.Data.BQL.BqlString.Constant<invoice>
		{
			public invoice() : base(Invoice) { ;}
		}

		public class debitMemo : PX.Data.BQL.BqlString.Constant<debitMemo>
		{
			public debitMemo() : base(DebitMemo) { ;}
		}

		public class creditMemo : PX.Data.BQL.BqlString.Constant<creditMemo>
		{
			public creditMemo() : base(CreditMemo) { ;}
		}

		public class payment : PX.Data.BQL.BqlString.Constant<payment>
		{
			public payment() : base(Payment) { ;}
		}

		public class voidPayment : PX.Data.BQL.BqlString.Constant<voidPayment>
		{
			public voidPayment() : base(VoidPayment) { ;}
		}
		public class prepayment : PX.Data.BQL.BqlString.Constant<prepayment>
		{
			public prepayment() : base(Prepayment) { ;}
		}
		public class refund : PX.Data.BQL.BqlString.Constant<refund>
		{
			public refund() : base(Refund) { ;}
		}

        public class voidRefund : PX.Data.BQL.BqlString.Constant<voidRefund>
		{
            public voidRefund() : base(VoidRefund) { }
        }

        public class finCharge : PX.Data.BQL.BqlString.Constant<finCharge>
		{
			public finCharge() : base(FinCharge) { ;}
		}

		public class smallBalanceWO : PX.Data.BQL.BqlString.Constant<smallBalanceWO>
		{
			public smallBalanceWO() : base(SmallBalanceWO) { ;}
		}

		public class smallCreditWO : PX.Data.BQL.BqlString.Constant<smallCreditWO>
		{
			public smallCreditWO() : base(SmallCreditWO) { ;}
		}
				
		public class undefined : PX.Data.BQL.BqlString.Constant<undefined>
		{
			public undefined() : base(Undefined) { ;}
		}

		public class noUpdate : PX.Data.BQL.BqlString.Constant<noUpdate>
		{
			public noUpdate() : base(NoUpdate) { ;}
		}

		public class cashSale : PX.Data.BQL.BqlString.Constant<cashSale>
		{
			public cashSale() : base(CashSale) { ;}
		}

		public class cashReturn : PX.Data.BQL.BqlString.Constant<cashReturn>
		{
			public cashReturn() : base(CashReturn) { ;}
		}

		public static bool? Payable(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case SmallCreditWO: 
					return true;
				case Payment:
				case Prepayment:
				case CreditMemo:
				case VoidPayment:
				case Refund:
                case VoidRefund:
                case SmallBalanceWO:
				case CashSale:
				case CashReturn:
					return false;
				default:
					return null;
			}
		}

		public static Int16? SortOrder(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case CashSale:
					return 0;
				case CreditMemo:
					return 1;
				case Prepayment:
					return 2;
				case Payment:
				case SmallBalanceWO:
					return 3;
				case SmallCreditWO:
				case Refund:
                    return 4;
				case VoidRefund:
					return 5;
				case VoidPayment:
				case CashReturn:
					return 6;
				default:
					return null;
			}
		}

		public static Decimal? SignBalance(string DocType)
		{
			switch (DocType)
			{
				case Refund:
                case VoidRefund:
                case Invoice:
				case DebitMemo:
				case FinCharge:
				case SmallCreditWO: 
					return 1m;
				case CreditMemo:
				case Payment :
				case Prepayment:
				case VoidPayment:
				case SmallBalanceWO:
					return -1m;
				case CashSale:
				case CashReturn:
					return 0;
				default:
					return null;
			}
		}
		
		public static Decimal? SignAmount(string DocType)
		{
			switch (DocType)
			{
				case Refund:
                case VoidRefund:
                case Invoice:
				case DebitMemo:
				case FinCharge:
				case SmallCreditWO: 
				case CashSale:
					return 1m;
				case CreditMemo:
				case Payment :
				case Prepayment:
				case VoidPayment:
				case SmallBalanceWO:
				case CashReturn:
					return -1m;
				default:
					return null;
			}
		}

		public static string TaxDrCr(string DocType)
		{
			switch (DocType)
			{
			  //Invoice Types
				case Invoice:
				case DebitMemo:
				case FinCharge:
				case CashSale:
					return DrCr.Credit;
				case CreditMemo:
				case CashReturn:
					return DrCr.Debit;
				default:
					return DrCr.Credit;
			}
		}

		public static string DocClass(string DocType)
		{
			switch (DocType) 
			{
				case Invoice:
				case DebitMemo:
				case CreditMemo:
				case FinCharge:
				case CashSale:
				case CashReturn:
					return GLTran.tranClass.Normal;
				case Payment:
				case VoidPayment:
				case Refund:
                case VoidRefund:
                    return GLTran.tranClass.Payment;
				case SmallBalanceWO:
				case SmallCreditWO:
				case Prepayment:
					return GLTran.tranClass.Charge;
				default:
					return null;
			}
		}

		/// <summary>
		/// Returns <c>true</c> if the specified document type
		/// corresponds to self-voiding documents, i.e. one for which
		/// the void process does not create a separate document, but just
		/// reverses all of its applications in full.
		/// </summary>
		public static bool IsSelfVoiding(string docType)
		{
			if (!_valueLabelPairs.Any(pair => pair.Value == docType))
			{
				throw new PXException(Messages.DocTypeNotSupported);
			}

			switch (docType)
			{
				case SmallBalanceWO:
				case SmallCreditWO:
					return true;
				default:
					return false;
			}
		}

		public static bool? HasNegativeAmount(string docType)
		{
			switch (docType)
			{
				case Invoice:
				case DebitMemo:
				case CreditMemo:
				case FinCharge:
				case CashSale:
				case Payment:
				case Refund:
				case SmallBalanceWO:
				case SmallCreditWO:
				case CashReturn:
					return false;
				case VoidPayment:
                case VoidRefund:
                    return true;
				default:
					return null;
			}
		}
	}
}
