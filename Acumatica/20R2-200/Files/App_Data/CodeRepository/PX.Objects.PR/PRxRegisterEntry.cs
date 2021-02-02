using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.PM.GraphExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class PRxRegisterEntry : PXGraphExtension<RegisterEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(LocationIDAttribute))]
		[HybridLocationID(typeof(Where<Location.bAccountID, Equal<Current<PMTran.bAccountID>>>), typeof(PMTran.origModule), DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		protected virtual void _(Events.CacheAttached<PMTran.locationID> e) { }
	}

	public class HybridLocationIDAttribute : LocationIDAttribute, IPXFieldSelectingSubscriber
	{
		Type _ModuleField;

		public HybridLocationIDAttribute(Type WhereType, Type moduleField)
			: base(WhereType)
		{
			_ModuleField = moduleField;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object module = sender.GetValue(e.Row, _ModuleField.Name);
			object locationID = sender.GetValue(e.Row, _FieldName);
			if (module?.Equals(BatchModule.PR) == true && locationID is int?)
			{
				PRLocation location = new SelectFrom<PRLocation>.Where<PRLocation.locationID.IsEqual<P.AsInt>>.View(sender.Graph).SelectSingle(locationID);
				if (location != null)
				{
					e.ReturnValue = location.LocationCD;
				}
			}
		}
	}
}
