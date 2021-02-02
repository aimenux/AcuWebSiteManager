using System;
using System.Diagnostics;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.Common.MigrationMode;
using PX.Objects.TX;

namespace PX.Objects.AR.Standalone
{
	/// <exclude/>
	[PXHidden]
	[Serializable]
	public partial class ARRegister : AR.ARRegister
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ARRegister>.By<docType, refNbr>
		{
			public static ARRegister Find(PXGraph graph, string docType, string refNbr) => FindBy(graph, docType, refNbr);
		}
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		[PXDBLong]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region ClosedFinPeriodID
		public new abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }
		#endregion

		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		public new abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }

		#region ARAccountID
		[PXDefault]
		[Account(typeof(ARRegister.branchID), typeof(Search<Account.accountID,
			Where2<Match<Current<AccessInfo.userName>>,
				And<Account.active, Equal<True>,
					And<Account.isCashAccount, Equal<False>,
						And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
							Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AR Account")]
		public override Int32? ARAccountID
		{
			get;
			set;
		}
		#endregion
		#region ARSubID
		public new abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }

		[PXDefault]
		[SubAccount(typeof(ARRegister.aRAccountID), DescriptionField = typeof(Sub.description), DisplayName = "AR Subaccount", Visibility = PXUIVisibility.Visible)]
		public override Int32? ARSubID
		{
			get;
			set;
		}
		#endregion
	}
}
