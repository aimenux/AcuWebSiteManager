using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
	[Serializable]
	public class CostCodeMaint : PXGraph<CostCodeMaint>, PXImportAttribute.IPXPrepareItems
	{
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void PMCostCode_IsProjectOverride_CacheAttached(PXCache sender)
		{
		}

		[PXImport(typeof(PMCostCode))]
		[PXViewName(Messages.CostCode)]
		[PXFilterable]
		public PXSelect<PMCostCode> Items;
		public PXSavePerRow<PMCostCode> Save;
		public PXCancel<PMCostCode> Cancel;

		public ChangeCostCode changeID;
		[PXUIField(DisplayName = "Change ID", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable ChangeID(PXAdapter adapter)
		{
			return (new ChangeCostCode(this, "changeID")).Press(adapter);
		}

		public PXSetup<PMSetup> Setup;

		protected virtual void _(Events.RowDeleting<PMCostCode> e)
		{
			if ( e.Row.IsDefault == true )
			{
				throw new PXException(Messages.CannotDeleteDefaultCostCode);
			}
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (IsKeyChanged(keys, values))
			{
				throw new PXException(Messages.CannotModifyCostCode);
			}

			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public virtual bool IsKeyChanged(IDictionary keys, IDictionary values)
		{			
			if (keys.Contains(nameof(PMCostCode.CostCodeCD)))
			{
				string keyValue = (string) keys[nameof(PMCostCode.CostCodeCD)];
				if (!string.IsNullOrEmpty(keyValue))
				{
					if (values.Contains(nameof(PMCostCode.CostCodeCD)))
					{
						string valValue = (string)values[nameof(PMCostCode.CostCodeCD)];

						if (keyValue != valValue)
						{
							return true;
						}
					}
				}
			}

			return false;
			
		}

		#region PMImport Implementation
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }
		#endregion
	}

	public class ChangeCostCode : PXChangeID<PMCostCode, PMCostCode.costCodeCD>
	{
		public ChangeCostCode(PXGraph graph, string name)
			: base(graph, name)
		{
		}

		public ChangeCostCode(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
		}

		protected override IEnumerable Handler(PXAdapter adapter)
		{
			if (this.Graph.Views[ChangeIdDialogView].Answer == WebDialogResult.None)
				this.Graph.Views[ChangeIdDialogView].Cache.Clear();

			string newcd;
			if (adapter.View.Cache.Current != null && adapter.View.Cache.GetStatus(adapter.View.Cache.Current) != PXEntryStatus.Inserted)
			{
				var dialogResult = adapter.View.Cache.Graph.Views[ChangeIdDialogView].AskExt();
				if ((dialogResult == WebDialogResult.OK || (dialogResult == WebDialogResult.Yes && this.Graph.IsExport))
					&& !String.IsNullOrWhiteSpace(newcd = GetNewCD(adapter)))
				{
					ChangeCD(adapter.View.Cache, GetOldCD(adapter), newcd);
				}
			}

			if (this.Graph.IsContractBasedAPI)
				this.Graph.Actions.PressSave();
			
			return adapter.Get();
		}
	}
}
