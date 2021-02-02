using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.GL.Reclassification.Common
{
	public abstract class ReclassifyTransactionsBase<TGraph> : PXGraph<TGraph> 
		where TGraph : PXGraph
	{
		public PXFilter<ReclassGraphState> StateView;
		public PXSetup<GLSetup> GLSetup;

        protected ReclassGraphState State
		{
			get { return StateView.Current; }
			set { StateView.Current = value; }
		}

		protected ReclassifyTransactionsBase()
		{
			var setup = GLSetup.Current;
        }

		public static bool IsReclassAttrChanged(GLTranForReclassification tranForReclass)
		{
			return tranForReclass.NewBranchID != tranForReclass.BranchID ||
					tranForReclass.NewAccountID != tranForReclass.AccountID ||
					tranForReclass.NewSubID != tranForReclass.SubID;
		}

		public GLTranForReclassification GetGLTranForReclassByKey(GLTranKey key)
		{
			return new GLTranForReclassification
			{
				Module = key.Module,
				BatchNbr = key.BatchNbr,
				LineNbr = key.LineNbr
			};
		}

		protected virtual IEnumerable<GLTran> GetTransReclassTypeSorted(PXGraph graph, string module, string batchNbr)
		{
			return PXSelect<GLTran,
							Where<GLTran.module, Equal<Required<GLTran.module>>,
									And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>>>,
							OrderBy<Desc<GLTran.reclassType>>>
							.Select(graph, module, batchNbr)
							.RowCast<GLTran>();
			}
	}
}
