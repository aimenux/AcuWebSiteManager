using PX.Data;
using PX.Objects.CS;
using System;

namespace PX.Objects.PR
{
	public class PayrollType
	{
		public class BatchListAttribute : PXStringListAttribute
		{
			public BatchListAttribute()
				: base(
				new string[] { Regular, Special },
				new string[] { Messages.Regular, Messages.Special })
			{ }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Regular, Special, Adjustment, VoidCheck },
				new string[] { Messages.Regular, Messages.Special, Messages.Adjustment, Messages.VoidCheck })
			{ }
		}



		public class regular : PX.Data.BQL.BqlString.Constant<regular>
		{
			public regular() : base(Regular) { }
		}

		public class special : PX.Data.BQL.BqlString.Constant<special>
		{
			public special() : base(Special) { }
		}

		public class adjustment : PX.Data.BQL.BqlString.Constant<adjustment>
		{
			public adjustment() : base(Adjustment) { }
		}

		public class voidCheck : PX.Data.BQL.BqlString.Constant<voidCheck>
		{
			public voidCheck() : base(VoidCheck) { }
		}

		public const string Regular = "REG";
		public const string Special = "SPC";
		public const string Adjustment = "ADJ";
		public const string VoidCheck = "VCK";

		public static string DrCr(string docType)
		{
			switch (docType)
			{
				case Regular:
				case Special:
				case Adjustment:
				case VoidCheck:
					return GL.DrCr.Debit;
				default:
					return null;
			}
		}

		/// <summary>
		/// Specialized for PRPayments version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the PR Payment. <br/>
		/// References PRPayment.docType and PRPayment.transactionDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in PR Setup and APPayment types:<br/>
		/// namely - PRSetup.tranNumberingCD for all the types beside VoidCheck <br/>
		/// </summary>
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(PRPayment.docType), typeof(PRPayment.transactionDate),
					_DocTypes,
					_SetupFields)
			{ }

			private static string[] _DocTypes
			{
				get
				{
					return new string[] { Regular, Special, Adjustment, VoidCheck };
				}
			}

			private static Type[] _SetupFields
			{
				get
				{
					return new Type[]
					{
						typeof(PRSetup.tranNumberingCD),
						typeof(PRSetup.tranNumberingCD),
						typeof(PRSetup.tranNumberingCD),
						null
					};
				}
			}
		}
	}
}
