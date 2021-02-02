using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.SM;
using PX.Web.UI;

namespace PX.Objects.IN
{
	public class InventoryHelper
	{
		/// <summary>
		/// For a given data record containing a deferral code and a default deferral term,
		/// generates a warning message when the specified deferral code is flexible, but the specified
		/// default deferral term is zero.
		/// </summary>
		/// <param name="sender">The cache containing the records.</param>
		/// <param name="row">The currently processed data record.</param>
		/// <typeparam name="DeferralCode">The DAC field type for deferral code.</param>
		/// <typeparam name="DefaultTerm">The DAC field type for default deferral term.</param>
		public static void CheckZeroDefaultTerm<DeferralCode, DefaultTerm>(PXCache sender, object row)
			where DeferralCode : IBqlField
			where DefaultTerm : IBqlField
		{
			string deferralCodeValue = sender.GetValue<DeferralCode>(row) as string;
			decimal? defaultTermValue = sender.GetValue<DefaultTerm>(row) as decimal?;

			bool displayWarning = false;

			if (deferralCodeValue != null)
			{
				DRDeferredCode deferralCodeRecord =
					PXSelect<
						DRDeferredCode,
						Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>
					.Select(sender.Graph, deferralCodeValue);

				if (deferralCodeRecord != null &&
					DeferredMethodType.RequiresTerms(deferralCodeRecord) &&
					defaultTermValue.HasValue &&
					defaultTermValue == 0)
				{
					displayWarning = true;
				}
			}

			sender.RaiseExceptionHandling(
				typeof(DefaultTerm).Name,
				row,
				defaultTermValue,
				displayWarning ? new PXSetPropertyException(Messages.NoDefaultTermSpecified, PXErrorLevel.Warning) : null);
		}

		public static List<CacheEntityItem> TemplateEntity(PXGraph graph, string parent, PXSiteMap.ScreenInfo _info)
		{
			List<CacheEntityItem> ret = new List<CacheEntityItem>();
			if (parent == null)
			{
				int i = 0;
				if (_info.GraphName != null)
				{
					foreach (Data.Description.PXViewDescription viewdescr in _info.Containers.Values)
					{
						CacheEntityItem item = new CacheEntityItem();
						item.Key = viewdescr.ViewName;
						item.SubKey = viewdescr.ViewName;
						item.Path = null;
						item.Name = viewdescr.DisplayName;
						item.Number = i++;
						item.Icon = Sprite.Main.GetFullUrl(Sprite.Main.Box);
						ret.Add(item);
					}
				}
			}
			else
			{
				string[] viewname = null;
				_info.Views.TryGetValue(parent, out viewname);
				if (viewname != null)
				{
					int f = 0;
					var tempgraph = (PXGraph)PXGraph.CreateInstance(GraphHelper.GetType(_info.GraphName));
					if (!tempgraph.Views.ContainsKey(parent))
						return null;

					foreach (PXFieldState field in PXFieldState.GetFields(graph, tempgraph.Views[parent].BqlSelect.GetTables(), false))
					{
						CacheEntityItem item = new CacheEntityItem();
						item.Key = parent + "." + field.Name;
						item.SubKey = field.Name;
						item.Path = "((" +
													(string.IsNullOrEmpty(parent)
													? field.Name
													: parent + "." + field.Name) + "))";
						item.Name = field.DisplayName;
						item.Number = f++;
						item.Icon = Sprite.Main.GetFullUrl(Sprite.Main.BoxIn);
						ret.Add(item);
					}
				}
			}
			return ret;
		}

		public static bool CanCreateStockItem(PXGraph graph)
			=> PXAccess.FeatureInstalled<FeaturesSet.inventory>()
				&& PXSelect<INSetup>.Select(graph).RowCast<INSetup>().Any();
	}
}
