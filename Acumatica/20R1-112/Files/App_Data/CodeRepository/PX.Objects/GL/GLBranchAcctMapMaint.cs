using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	public class GLBranchAcctMapMaint : PXGraph<GLBranchAcctMapMaint,Branch>
	{
		public PXSelect<Branch> Branches;
		public PXSelect<Ledger> Ledgers;

		public PXSelect<BranchAcctMapFrom, Where<BranchAcctMapFrom.branchID, Equal<Current<Branch.branchID>>>> MapFrom;
		public PXSelect<BranchAcctMapTo, Where<BranchAcctMapTo.branchID, Equal<Current<Branch.branchID>>>> MapTo;

		public GLBranchAcctMapMaint()
			:base()
		{
			this.Branches.Cache.AllowInsert = false;
			this.Branches.Cache.AllowDelete = false;

			PXUIFieldAttribute.SetEnabled<Branch.ledgerID>(Branches.Cache, null, false);
		}

        [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDBLiteDefault(typeof(CR.BAccount.acctCD))]
		[PXDimensionSelector("BRANCH", typeof(Search<Branch.branchCD, Where<Match<Current<AccessInfo.userName>>>>), typeof(Branch.branchCD))]
        [PXUIField(DisplayName = "Originating Branch", Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<Branch.active, Equal<True>>), GL.Messages.BranchInactive)]
        public virtual void Branch_BranchCD_CacheAttached(PXCache sender)
        {
        }

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (viewName.ToLower() == "mapfrom" || viewName.ToLower() == "mapto")
			{
				using (new PXReadBranchRestrictedScope())
				{
					return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
				}
			}

			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		public override void Persist()
		{
			base.Persist();
			Branches.Cache.RaiseRowSelected(Branches.Current);
		}

		public virtual void BranchAcctMapFrom_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			e.Cancel = !ValidateDuplicateMap<BranchAcctMap.toBranchID>(sender, MapFrom.View, e.Row, null);		
		}
		public virtual void BranchAcctMapFrom_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if(!sender.ObjectsEqual<BranchAcctMapFrom.fromAccountCD, BranchAcctMapFrom.toAccountCD>(e.Row, e.NewRow))
			{
				e.Cancel = !ValidateDuplicateMap<BranchAcctMap.toBranchID>(sender, MapFrom.View, e.NewRow, e.Row);
			}
		}
		public virtual void BranchAcctMapTo_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			e.Cancel = !ValidateDuplicateMap<BranchAcctMap.fromBranchID>(sender, MapTo.View, e.Row, null);
		}
		public virtual void BranchAcctMapTo_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (!sender.ObjectsEqual<BranchAcctMapTo.fromAccountCD, BranchAcctMapTo.toAccountCD>(e.Row, e.NewRow))
			{
				e.Cancel = !ValidateDuplicateMap<BranchAcctMap.fromBranchID>(sender, MapTo.View, e.NewRow, e.Row);
			}
		}

		protected virtual void BranchAcctMapTo_FromAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void BranchAcctMapTo_ToAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void BranchAcctMapFrom_FromAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void BranchAcctMapFrom_ToAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		private bool ValidateDuplicateMap<BranchField>(PXCache sender, PXView view, object row, object oldrow)
			where BranchField : IBqlField
		{
            int? branch = (int?)sender.GetValue<BranchField>(row);
            string fromMask = (string)sender.GetValue<BranchAcctMap.fromAccountCD>(row);
			string toMask = (string) sender.GetValue<BranchAcctMap.toAccountCD>(row);

			if (string.IsNullOrEmpty(fromMask) || string.IsNullOrEmpty(toMask)) return false;

			PXSetPropertyException ex = null;
			if (string.Compare(fromMask, toMask, true) > 0)
				ex = new PXSetPropertyException(Messages.MapAccountError);

			if(ex == null)
				foreach (var item in view.SelectMulti())
				{
					if (item == row || item == oldrow) continue;
                    int? itemBranch = (int?)sender.GetValue<BranchField>(item);
                    if (branch == itemBranch)
                    { // same branch
                        string itemFromMask = (string)sender.GetValue<BranchAcctMap.fromAccountCD>(item);
                        string itemToMask = (string)sender.GetValue<BranchAcctMap.toAccountCD>(item);
                        if (string.Compare(fromMask, itemToMask, true) < 0 &&
                                string.Compare(itemFromMask, toMask, true) < 0)
                        {
                            ex = new PXSetPropertyException(Messages.MapAccountDuplicate);
                            break;
                        }
                    }
				}

			if (ex != null)
			{
				if (!sender.ObjectsEqual<BranchAcctMapFrom.toAccountCD>(row, oldrow) || (oldrow == null && sender.GetValue<BranchAcctMap.toAccountCD>(row) != null))
					sender.RaiseExceptionHandling<BranchAcctMapFrom.toAccountCD>(row,
					                                                             sender.GetValue<BranchAcctMap.toAccountCD>(
					                                                             	row), ex);
				else
					sender.RaiseExceptionHandling<BranchAcctMapFrom.fromAccountCD>(row,
					                                                               sender.GetValue<BranchAcctMap.fromAccountCD>(
					                                                               	row), ex);
			}
			return ex == null;
		}

	}
}
