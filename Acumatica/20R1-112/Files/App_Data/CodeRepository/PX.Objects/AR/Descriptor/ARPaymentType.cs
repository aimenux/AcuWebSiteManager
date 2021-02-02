using System;
using System.Collections.Generic;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Common;

namespace PX.Objects.AR
{
	public class ARPaymentType : ARDocType
	{
		/// <summary>
		/// Specialized selector for ARPayment RefNbr.<br/>
		/// By default, defines the following set of columns for the selector:<br/>
		/// ARPayment.refNbr,ARPayment.docDate, ARPayment.finPeriodID,<br/>
		/// ARPayment.customerID, ARPayment.customerID_Customer_acctName,<br/>
		/// ARPayment.customerLocationID, ARPayment.curyID, ARPayment.curyOrigDocAmt,<br/>
		/// ARPayment.curyDocBal,ARPayment.status, ARPayment.cashAccountID, ARPayment.pMInstanceID, ARPayment.extRefNbr<br/>
		/// </summary>		
		public class RefNbrAttribute : PXSelectorAttribute
		{
			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="SearchType">Must be IBqlSearch, returning ARPayment.refNbr</param>
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(ARRegister.refNbr),
				typeof(ARPayment.extRefNbr),
				typeof(ARRegister.docDate),
				typeof(ARRegister.finPeriodID),
				typeof(ARRegister.customerID),
				typeof(ARRegister.customerID_Customer_acctName),
				typeof(ARRegister.customerLocationID),
				typeof(ARRegister.curyID),
				typeof(ARRegister.curyOrigDocAmt),
				typeof(ARRegister.curyDocBal),
				typeof(ARRegister.status),
				typeof(ARPayment.cashAccountID),
				typeof(ARPayment.pMInstanceID_CustomerPaymentMethod_descr))
			{
			}

			public RefNbrAttribute(Type SearchType, params Type[] fieldList)
				: base(SearchType, fieldList)
			{
			}
		}

		/// <summary>
		/// Specialized selector for ARPayment RefNbr.<br/>
		/// By default, defines the following set of columns for the selector:<br/>
		/// ARPayment.refNbr,ARPayment.docDate, ARPayment.finPeriodID,<br/>
		/// ARPayment.customerID, ARPayment.customerID_Customer_acctName,<br/>
		/// ARPayment.customerLocationID, ARPayment.curyID, ARPayment.curyOrigDocAmt,<br/>
		/// ARPayment.curyDocBal,ARPayment.status, ARPayment.cashAccountID, ARPayment.pMInstanceID, ARPayment.extRefNbr<br/>
		/// </summary>	
		public class AdjgRefNbrAttribute : PXSelectorAttribute
		{
			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="SearchType">Must be IBqlSearch, returning ARPayment.refNbr</param>
			public AdjgRefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(ARPayment.refNbr),
				typeof(ARPayment.docDate),
				typeof(ARPayment.finPeriodID),
				typeof(ARPayment.customerID),
				typeof(ARPayment.customerLocationID),
				typeof(ARPayment.curyID),
				typeof(ARPayment.curyOrigDocAmt),
				typeof(ARPayment.curyDocBal),
				typeof(ARPayment.status),
				typeof(ARPayment.cashAccountID),
				typeof(ARPayment.pMInstanceID),
				typeof(ARPayment.extRefNbr),
				typeof(ARPayment.docDesc))
			{
			}
		}

		/// <summary>
		/// Specialized for ARPayments version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the AR Payment. <br/>
		/// References ARInvoice.docType and ARInvoice.docDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in AR Setup and ARInvoice types:<br/>
		/// namely ARSetup.paymentNumberingID - for ARPayment, ARPrepayment, AR Refund 
		/// and null for others.
		/// </summary>		
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(ARPayment.docType), typeof(ARPayment.docDate),
					_DocTypes,
					_SetupFields)
			{ }

			private static string[] _DocTypes
			{
				get
				{
					return new string[] { Payment, CreditMemo, Prepayment, Refund, VoidRefund, VoidPayment, SmallBalanceWO };
				}
			}

			private static Type[] _SetupFields
			{
				get
				{
					return new Type[]
					{
						typeof(ARSetup.paymentNumberingID), null, typeof(ARSetup.paymentNumberingID), typeof(ARSetup.paymentNumberingID), null, null, null
					};
				}
			}

			public static Type GetNumberingIDField(string docType)
			{
				foreach (var pair in _DocTypes.Zip(_SetupFields))
					if (pair.Item1 == docType)
						return pair.Item2;

				return null;
			}

			public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				bool isManualNumbering = this.UserNumbering == true;

				base.RowPersisting(sender, e);

				// Prevent saving Payments and Prepayments with the same RefNbr.
				// -
				string currentDocumentType = sender.GetValue<ARPayment.docType>(e.Row) as string;
				string otherDocumentType = null;

				if (currentDocumentType == ARDocType.Payment)
				{
					otherDocumentType = ARDocType.Prepayment;
				}
				else if (currentDocumentType == ARDocType.Prepayment)
				{
					otherDocumentType = ARDocType.Payment;
				}

				var otherDocumentView = new PXSelect<ARRegister,
					Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>,
					And<Where<ARRegister.docType, Equal<Required<ARRegister.docType>>>>>>(sender.Graph);

				string newNumber = isManualNumbering ? (e.Row as ARRegister).RefNbr : this.NewNumber;

				if (otherDocumentType != null &&
					otherDocumentView.Select(newNumber, otherDocumentType).Count > 0)
				{
					if (isManualNumbering)
					{
						sender.RaiseExceptionHandling<ARRegister.refNbr>(
							e.Row,
							newNumber,
							new PXSetPropertyException(
								Messages.DocumentAlreadyExistsWithTheSameReferenceNumber,
								PXErrorLevel.Error,
								otherDocumentType == ARDocType.Payment ? Messages.Payment : Messages.Prepayment));
					}
					else
					{
						// Hack. This exception type is thrown to force the system to retry the transaction
						// with the next available Ref Number.
						// -
						throw new PXLockViolationException(typeof(ARRegister), PXDBOperation.Insert, null);
					}
				}
			}
		}

		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Payment, CreditMemo, Prepayment, Refund, VoidRefund, VoidPayment, SmallBalanceWO },
				new string[] { Messages.Payment, Messages.CreditMemo, Messages.Prepayment, Messages.Refund, Messages.VoidRefund, Messages.VoidPayment, Messages.SmallBalanceWO })
			{ }
		}

		public class ListExAttribute : PXStringListAttribute
		{
			public ListExAttribute()
				: base(
				new string[] { Payment, CreditMemo, Prepayment, Refund, VoidRefund, VoidPayment, SmallBalanceWO, CashSale, CashReturn },
				new string[] { Messages.Payment, Messages.CreditMemo, Messages.Prepayment, Messages.Refund, Messages.VoidRefund, Messages.VoidPayment, Messages.SmallBalanceWO, Messages.CashSale, Messages.CashReturn })
			{ }
		}

		public new class SOListAttribute : PXStringListAttribute
		{
			public SOListAttribute()
				: base(
				new string[] { Payment, CreditMemo, Prepayment },
				new string[] { Messages.Payment, Messages.CreditMemo, Messages.Prepayment })
			{ }
		}

		public static bool VoidAppl(string DocType)
		{
			//CashReturn is excluded
			return (DocType == VoidPayment || DocType == VoidRefund);
		}

		public static bool VoidEnabled(ARPayment payment)
		{
			return
				payment.DocType == Payment ||
				payment.DocType == Prepayment ||
				payment.DocType == Refund ||
				payment.SelfVoidingDoc == true;
		}

		public static bool CanHaveBalance(string DocType)
		{
			return (DocType == CreditMemo || DocType == Payment || DocType == VoidPayment || DocType == Prepayment);
		}

		public static string DrCr(string DocType)
		{
			switch (DocType)
			{
				case Payment:
				case VoidPayment:
				case CreditMemo:
				case SmallBalanceWO:
				case CashSale:
				case Prepayment:
					return GL.DrCr.Debit;
				case Refund:
				case VoidRefund:
				case CashReturn:
					return GL.DrCr.Credit;
				default:
					return null;
			}
		}

		protected static readonly Dictionary<string, string> VoidingTypes = new Dictionary<string, string>
		{
			{ Payment, VoidPayment },
			{ CashSale,  null}, //Cash Return is not considered as a void for CashSale                    
            { Prepayment, VoidPayment },
			{ Refund, VoidRefund },
		};

		protected static readonly Dictionary<string, HashSet<string>> VoidedTypes = Common.Extensions.CollectionExtensions.ReverseDictionary(VoidingTypes);

		public static readonly HashSet<string> AllVoidingTypes = new HashSet<string>
		{
			VoidPayment, VoidRefund
		};

		public static string[] GetVoidedARDocType(string docType)
		{
			if (VoidedTypes.ContainsKey(docType))
			{
				return VoidedTypes[docType].ToArray<string>();
			}
			else
			{
				return new string[] { };
			}
		}

		public static string GetVoidingARDocType(string docType)
		{
			string value = null;

			if (docType != null)
			{
				VoidingTypes.TryGetValue(docType, out value);
			}
			return value;
		}
	}
}
