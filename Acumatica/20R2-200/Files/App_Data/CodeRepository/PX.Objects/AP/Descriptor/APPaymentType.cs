using System;
using System.Collections.Generic;

using PX.Common;
using PX.Data;
using PX.Data.EP;

using PX.Objects.CS;

namespace PX.Objects.AP
{
	public class APPaymentType : APDocType
	{
		/// <summary>
		/// Specialized selector for APPayment RefNbr.<br/>
		/// By default, defines the following set of columns for the selector:<br/>
		/// APPayment.refNbr,APPayment.docDate, APPayment.finPeriodID, APPayment.vendorID,<br/>
		/// APPayment.vendorID_Vendor_acctName, APPayment.vendorLocationID, APPayment.curyID,<br/>
		/// APPayment.curyOrigDocAmt, APPayment.curyDocBal, APPayment.status, <br/>
		/// APPayment.cashAccountID, APPayment.paymentMethodID, APPayment.extRefNbr <br/>
		/// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(APRegister.refNbr),
				typeof(APPayment.extRefNbr),
				typeof(APRegister.docDate),
				typeof(APRegister.finPeriodID),
				typeof(APRegister.vendorID),
				typeof(APRegister.vendorID_Vendor_acctName),
				typeof(APRegister.vendorLocationID),
				typeof(APRegister.curyID),
				typeof(APRegister.curyOrigDocAmt),
				typeof(APRegister.curyDocBal),
				typeof(APRegister.status),
				typeof(APPayment.cashAccountID),
				typeof(APPayment.paymentMethodID))
			{
			}
		}

		public class AdjgRefNbrAttribute : PXSelectorAttribute
		{
			public AdjgRefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(APPayment.refNbr),
				typeof(APPayment.docDate),
				typeof(APPayment.finPeriodID),
				typeof(APPayment.vendorID),
				typeof(APPayment.vendorLocationID),
				typeof(APPayment.curyID),
				typeof(APPayment.curyOrigDocAmt),
				typeof(APPayment.curyDocBal),
				typeof(APPayment.status),
				typeof(APPayment.cashAccountID),
				typeof(APPayment.paymentMethodID),
				typeof(APPayment.extRefNbr),
				typeof(APPayment.docDesc))
			{
			}
		}

		/// <summary>
		/// Specialized for APPayments version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the AP Payment. <br/>
		/// References APPayment.docType and APPayment.docDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in AP Setup and APPayment types:<br/>
		/// namely - APSetup.checkNumberingID for all the types<br/>
		/// </summary>		
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(APPayment.docType), typeof(APPayment.docDate),
					_DocTypes,
					_SetupFields)
			{ }

			private static string[] _DocTypes
			{
				get
				{
					return new string[] { Check, DebitAdj, Prepayment, Refund, VoidRefund, VoidCheck };
				}
			}

			private static Type[] _SetupFields
			{
				get
				{
					return new Type[]
					{
					 typeof(APSetup.checkNumberingID), null, typeof(APSetup.checkNumberingID), typeof(APSetup.checkNumberingID), null, null
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
		}

		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Check, DebitAdj, Prepayment, Refund, VoidRefund, VoidCheck },
				new string[] { Messages.Check, Messages.DebitAdj, Messages.Prepayment, Messages.Refund, Messages.VoidRefund, Messages.VoidCheck })
			{ }
		}

		public static bool VoidAppl(string DocType)
		{
			//VoidQuickCheck is excluded
			return (DocType == VoidCheck || DocType == VoidRefund);
		}

		public static bool VoidEnabled(string docType)
		{
			return docType == Check || docType == Prepayment || docType == Refund;
		}

		public static bool CanHaveBalance(string DocType)
		{
			return (DocType == DebitAdj || DocType == Prepayment || DocType == QuickCheck || DocType == VoidCheck);
		}

		public static string DrCr(string DocType)
		{
			switch (DocType)
			{
				case Check:
				case VoidCheck:
				case DebitAdj:
				case Prepayment:
				case QuickCheck:
					return GL.DrCr.Credit;
				case Refund:
				case VoidRefund:
				case VoidQuickCheck:
					return GL.DrCr.Debit;
				default:
					return null;
			}
		}

		protected static readonly Dictionary<string, string> VoidingTypes = new Dictionary<string, string>
		{
			{ Check, VoidCheck },
			{ Prepayment, VoidCheck },
			{ QuickCheck, VoidQuickCheck },
			{ Refund, VoidRefund },
		};

		protected static readonly Dictionary<string, HashSet<string>> VoidedTypes = Common.Extensions.CollectionExtensions.ReverseDictionary(VoidingTypes);

		public static string[] GetVoidedAPDocType(string docType)
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

		public static string GetVoidingAPDocType(string docType)
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
