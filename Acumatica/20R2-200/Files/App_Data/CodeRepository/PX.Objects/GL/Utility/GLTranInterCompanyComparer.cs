using System.Collections.Generic;

namespace PX.Objects.GL
{
	public class GLTranInterCompanyComparer : IEqualityComparer<GLTran>
	{
		public bool Equals(GLTran t1, GLTran t2)
		{
			return
				t1.Module == t2.Module &&
				t1.BatchNbr == t2.BatchNbr &&
				t1.RefNbr == t2.RefNbr &&
				t1.CuryInfoID == t2.CuryInfoID &&
				t1.BranchID == t2.BranchID &&
				t1.AccountID == t2.AccountID &&
				t1.SubID == t2.SubID &&
				t1.IsInterCompany == t2.IsInterCompany;
		}

		public int GetHashCode(GLTran tran)
		{
			unchecked
			{
				int ret = 37;
				ret = ret * 397 + tran.Module.GetHashCode();
				ret = ret * 397 + tran.BatchNbr.GetHashCode();
				ret = ret * 397 + (tran.RefNbr ?? string.Empty).GetHashCode();
				ret = ret * 397 + tran.CuryInfoID.GetHashCode();
				ret = ret * 397 + tran.BranchID.GetHashCode();
				ret = ret * 397 + tran.AccountID.GetHashCode();
				ret = ret * 397 + tran.SubID.GetHashCode();
				ret = ret * 397 + tran.IsInterCompany.GetHashCode();

				return ret;
			}
		}
	}
}
