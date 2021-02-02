using System;

namespace PX.Objects.AR.CustomerStatements
{
	public class ARStatementKey : Tuple<int, string, int, DateTime>
	{
		public int BranchID => Item1;
		public string CurrencyID => Item2;
		public int CustomerID => Item3;
		public DateTime StatementDate => Item4;

		[Obsolete("This constructor is not used anymore and will be removed in Acumatica 8.0. Use other constructor overloads.")]
		public ARStatementKey(int branchID, string currencyID)
			: base(branchID, currencyID, -1, DateTime.Now)
		{
			throw new NotImplementedException();
		}

		public ARStatementKey(int branchID, string currencyID, int customerID, DateTime statementDate)
			: base(branchID, currencyID, customerID, statementDate)
		{ }

		public ARStatementKey(ARStatement statement)
			: this(
				  statement.BranchID.Value, 
				  statement.CuryID, 
				  statement.CustomerID.Value, 
				  statement.StatementDate.Value)
		{ }
	}
}
