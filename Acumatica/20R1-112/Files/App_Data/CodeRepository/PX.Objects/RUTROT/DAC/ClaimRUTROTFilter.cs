using System;
using PX.Data;

namespace PX.Objects.RUTROT
{
	[Serializable]
	public partial class ClaimRUTROTFilter : IBqlTable
	{
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }

		[ClaimActions.List]
		[PXDBString(1)]
		[PXDefault(ClaimActions.Claim)]
		[PXUIField(DisplayName = "Action", Visible = true)]
		public virtual string Action
		{
			get;
			set;
		}
		#endregion

		#region DeductionType
		public abstract class rUTROTType : PX.Data.BQL.BqlString.Field<rUTROTType> { }

		[RUTROTTypes.List]
        [PXDefault(RUTROTTypes.RUT, typeof(Search<BranchRUTROT.defaultRUTROTType, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Deduction Type", Visible = true)]
		public string RUTROTType
		{
			get;
			set;
		}
		#endregion
	}

	public class ClaimActions
	{
		public class List : PXStringListAttribute
		{
			public List()
				: base(new string[] { Balance, Claim, Export }, new string[] { "Balance", "Claim", "Export" })
			{
			}
		}

        public const string Balance = "B";
        public const string Export = "E";
		public const string Claim = "C";

        public class balance : PX.Data.BQL.BqlString.Constant<balance>
		{
            public balance() : base(Balance) { }
        }

        public class export : PX.Data.BQL.BqlString.Constant<export>
		{
			public export() : base(Export) { }
		}

		public class claim : PX.Data.BQL.BqlString.Constant<claim>
		{
			public claim() : base(Claim) { }
		}
	}
}
