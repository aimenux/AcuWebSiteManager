using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	/// <summary>A supplementary class to receive specific transaction data from the Acumatica ERP core.</summary>
	public class TranProcessingInput
	{
		/// <summary>The internal unique transaction identifier.</summary>
		public int TranID;
		/// <summary>The internal Acumatica ERP unique credit card identifier.</summary>
		/// <remarks>Information on credit cards might be obtained by using the <tt>ReadData</tt> method of the <see cref="ICreditCardDataReader" /> interface.</remarks>
		public int PMInstanceID;
		/// <summary>The unique customer identifier.</summary>
		public string CustomerCD;
		/// <summary>The type of the payment document.</summary>
		/// <remarks>Document information might be obtained by using <see cref="IDocDetailsDataReader" /> interface.</remarks>
		public string DocType;
		/// <summary>The reference number of the payment document.</summary>
		/// <remarks>Document information might be obtained by using <see cref="IDocDetailsDataReader" /> interface.</remarks>
		public string DocRefNbr;
		/// <summary>The original reference number of the document.</summary>
		public string OrigRefNbr;
		/// <summary>The ISO code of the transaction currency.</summary>
		public string CuryID;
		/// <summary>The amount of transaction.</summary>
		public decimal Amount;
		/// <summary>A field that indicates (if set to <tt>true</tt>) that verification of the CVV is required.</summary>
		public bool VerifyCVV;
	}
}
