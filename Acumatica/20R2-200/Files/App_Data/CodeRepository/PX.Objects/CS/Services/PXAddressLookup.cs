using PX.AddressValidator;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CR.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Compilation;
using System.Web.UI;

namespace PX.Objects.CS
{

	public static class PXAddressLookup
	{
		#region Class helper

		[PXHidden]
		public class PortalSetup : IBqlTable
		{
			#region PortalSetupID
			public abstract class portalSetupID : PX.Data.BQL.BqlString.Field<portalSetupID> { }
			[PXDBString(32, IsKey = true)]
			public virtual string PortalSetupID { get; set; }
			#endregion

			#region AddressLookupPluginID
			public abstract class addressLookupPluginID : PX.Data.BQL.BqlString.Field<addressLookupPluginID> { }
			[PXDBString(15, IsUnicode = true)]
			public string AddressLookupPluginID { get; set; }
			#endregion
		}
		#endregion

		public static bool IsEnabled(PXGraph graph)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.addressLookup>()
					? IsAddressLookupActive(graph) : false;
		}

		public static AddressValidatorPlugin GetAddressLookupPlugin(PXGraph graph)
		{
			string sPluginName = "";
			if (PXSiteMap.IsPortal)
			{
				PortalSetup portalSetup = PXSelectReadonly<PortalSetup>.SelectSingleBound(graph, null, null);
				if (!string.IsNullOrEmpty(portalSetup?.AddressLookupPluginID))
				{
					sPluginName = portalSetup.AddressLookupPluginID;
				}
			}
			else
			{
				PX.SM.PreferencesGeneral generalPref = PXSelectReadonly<PX.SM.PreferencesGeneral>.SelectSingleBound(graph, null, null);
				if (!string.IsNullOrEmpty(generalPref?.AddressLookupPluginID))
				{
					sPluginName = generalPref.AddressLookupPluginID;
				}
			}
			if (string.IsNullOrEmpty(sPluginName))
			{
				return null;
			}
			return AddressValidatorPlugin.PK.Find(graph, sPluginName);
		}

		public static bool IsAddressLookupActive(PXGraph graph)
		{
			AddressValidatorPlugin m = PXAddressLookup.GetAddressLookupPlugin(graph);
			return m?.IsActive ?? false;
		}

		public static IAddressLookupService CreateAddressLookup(PXGraph graph)
		{
			AddressValidatorPlugin plugin = PXAddressLookup.GetAddressLookupPlugin(graph);
			if (plugin == null)
			{
				throw new PXException(Messages.FailedToAddressLookupAutoCompletePlugin);
			}
			IAddressLookupService service = null;
			if (!string.IsNullOrEmpty(plugin.PluginTypeName))
			{
				try
				{
					Type addressValidatorType = PXBuildManager.GetType(plugin.PluginTypeName, true);
					service = (IAddressLookupService)Activator.CreateInstance(addressValidatorType);

					PXSelectBase<AddressValidatorPluginDetail> select = new PXSelect<AddressValidatorPluginDetail, Where<AddressValidatorPluginDetail.addressValidatorPluginID, Equal<Required<AddressValidatorPluginDetail.addressValidatorPluginID>>>>(graph);
					PXResultset<AddressValidatorPluginDetail> resultset = select.Select(plugin.AddressValidatorPluginID);
					List<IAddressValidatorSetting> list = new List<IAddressValidatorSetting>(resultset.Count);

					foreach (AddressValidatorPluginDetail item in resultset)
					{
						list.Add(item);
					}
					service.Initialize(list);
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCreateAddressAutoCompletePlugin, ex.Message);
				}
			}
			return service;
		}

		public static void RegisterClientScript(PX.Web.UI.PXPage page, PXGraph graph)
		{
			if (!IsAddressLookupActive(graph))
			{
				return;
			}

			IAddressLookupService service = CreateAddressLookup(graph);
			if (service != null)
			{
				string script = service.GetClientScript(graph);
				if (!string.IsNullOrEmpty(script))
				{
					page.ClientScript.RegisterStartupScript(page.GetType(), "addressLookup", script);
				}
			}
		}
	}
}
