using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Web.Compilation;
using PX.SM;
using PX.Data.Update;
using System.Collections;
using PX.DbServices;
using PX.TaxProvider;


namespace PX.Objects.TX
{
	public class TaxPluginMaint : PXGraph<TaxPluginMaint, TaxPlugin>
	{
		public PXSelect<TaxPlugin> Plugin;
		public PXSelectOrderBy<TaxPluginDetail, OrderBy<Asc<TaxPluginDetail.sortOrder>>> Details;
		public PXSelect<TaxPluginDetail, Where<TaxPluginDetail.taxPluginID, Equal<Current<TaxPlugin.taxPluginID>>>, OrderBy<Asc<TaxPluginDetail.sortOrder>>> SelectDetails;
		protected IEnumerable details()
		{
			ImportSettings();

			return SelectDetails.Select();
		}

		public PXSelect<TaxPluginMapping, Where<TaxPluginMapping.taxPluginID, Equal<Current<TaxPlugin.taxPluginID>>>> Mapping;

		public virtual void ImportSettings()
		{
			if (Plugin.Current != null)
			{
				var provider = CreateTaxProvider(this, Plugin.Current);
				if (provider != null)
				{
					var settings = provider.DefaultSettings;

					InsertDetails(settings);
				}
			}
		}

		public PXAction<TaxPlugin> test;
		[PXUIField(DisplayName = "Test connection", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual void Test()
		{
			if (Plugin.Current != null)
			{
				Save.Press();
				
				var provider = CreateTaxProvider(this, Plugin.Current);
				if (provider != null)
				{
					var result = provider.Ping();
					if (result.IsSuccess)
					{
						Plugin.Ask(Plugin.Current, Messages.ConnectionTaxAskSuccessHeader, Messages.ConnectionTaxAskSuccess, MessageButtons.OK, MessageIcon.Information);
					}
					else
					{
						StringBuilder errorMessages = new StringBuilder();

						foreach (var message in result.Messages)
						{
							errorMessages.AppendLine(message);
						}

						if (errorMessages.Length > 0)
						{
							throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.TestFailed, errorMessages.ToString()));
						}

					}
				}
			}
		}
		
		private void InsertDetails(IList<ITaxProviderSetting> list)
		{
			var existingRecords = new Dictionary<string, TaxPluginDetail>();

			foreach (TaxPluginDetail detail in SelectDetails.Select())
			{
				existingRecords.Add(detail.SettingID.ToUpper(), detail);
			}

			foreach (var item in list)
			{
				TaxPluginDetail existingRecord;
				if (existingRecords.TryGetValue(item.SettingID.ToUpper(), out existingRecord))
				{
					var copy = PXCache<TaxPluginDetail>.CreateCopy(existingRecord);

					if (!string.IsNullOrEmpty(item.Description))
						copy.Description = item.Description;
					copy.ControlType = item.ControlType;
					copy.ComboValues = item.ComboValues;
					copy.SortOrder = item.SortOrder;

					if (existingRecord.Description != copy.Description || existingRecord.ControlTypeValue != copy.ControlTypeValue ||
					    existingRecord.ComboValuesStr != copy.ComboValuesStr || existingRecord.SortOrder != copy.SortOrder)
						SelectDetails.Update(copy);
				}
				else
				{
					var taxPluginDetail = new TaxPluginDetail
					{
						SettingID = item.SettingID.ToUpper(),
						Description = item.Description,
						Value = item.Value,
						SortOrder = item.SortOrder,
						ControlType = item.ControlType,
						ComboValues = item.ComboValues
					};

					var row = SelectDetails.Insert(taxPluginDetail);
				}
			}

		}

		public static ITaxProvider CreateTaxProvider(PXGraph graph, TaxPlugin plugin)
		{
			ITaxProvider service = null;
			if (!string.IsNullOrEmpty(plugin.PluginTypeName))
			{
				try
				{
					Type taxType = PXBuildManager.GetType(plugin.PluginTypeName, true);
					service = (ITaxProvider)Activator.CreateInstance(taxType);
                                       
					PXSelectBase<TaxPluginDetail> select = new PXSelect<TaxPluginDetail, Where<TaxPluginDetail.taxPluginID, Equal<Required<TaxPluginDetail.taxPluginID>>>>(graph);
					PXResultset<TaxPluginDetail> resultset = select.Select(plugin.TaxPluginID);
					var list = new List<ITaxProviderSetting>(resultset.Count);

					foreach (TaxPluginDetail item in resultset)
					{
						list.Add(item);
					}
                                        
					service.Initialize(list);

				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCreateTaxPlugin, ex.Message);
				}
			}


			return service;
		}

		public static IReadOnlyList<string> GetTaxPluginAttributes(PXGraph graph, string taxPluginID)
		{
			TaxPlugin plugin = PXSelect<TaxPlugin,
				Where<TaxPlugin.taxPluginID, Equal<Required<TaxPlugin.taxPluginID>>>>.Select(graph, taxPluginID);

			if (plugin == null)
				throw new PXException(Messages.FailedToFindTaxPlugin, taxPluginID);

			ITaxProvider provider = null;
			if (!string.IsNullOrEmpty(plugin.PluginTypeName))
			{
				try
				{
					Type taxType = PXBuildManager.GetType(plugin.PluginTypeName, true);
					provider = (ITaxProvider)Activator.CreateInstance(taxType);
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCreateTaxPlugin, ex.Message);
				}
			}

			return provider == null ? new List<string>().AsReadOnly() : provider.Attributes;
		}

		public static ITaxProvider CreateTaxProvider(PXGraph graph, string taxPluginID)
		{
			TaxPlugin plugin = PXSelect<TaxPlugin,
				Where<TaxPlugin.taxPluginID, Equal<Required<TaxPlugin.taxPluginID>>>>.Select(graph, taxPluginID);

			if (plugin == null)
				throw new PXException(Messages.FailedToFindTaxPlugin, taxPluginID);

			return CreateTaxProvider(graph, plugin);
		}

		public static bool IsActive(PXGraph graph, string taxZoneID)
		{
			TaxPlugin m = PXSelectJoin<TaxPlugin, InnerJoin<TaxZone, On<TaxPlugin.taxPluginID, Equal<TaxZone.taxPluginID>>>, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(graph, taxZoneID);
			return m?.IsActive ?? false;
		}

		protected virtual void TaxPlugin_PluginTypeName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TaxPlugin row = e.Row as TaxPlugin;
			if (row == null) return;

			foreach (TaxPluginDetail detail in SelectDetails.Select())
			{
				SelectDetails.Delete(detail);
			}
		}

		protected virtual void TaxPluginDetail_Value_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			TaxPluginDetail row = e.Row as TaxPluginDetail;
			if (row != null)
			{
				string fieldName = typeof(TaxPluginDetail.value).Name;

				switch (row.ControlTypeValue)
				{
					case TaxPluginDetail.Combo:
						List<string> labels = new List<string>();
						List<string> values = new List<string>();
						foreach (var kv in row.ComboValues)
						{
							values.Add(kv.Key);
							labels.Add(kv.Value);
						}
						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, TaxPluginDetail.ValueFieldLength, null, fieldName, false, 1, null,
																values.ToArray(), labels.ToArray(), true, null);
						break;
					case TaxPluginDetail.CheckBox:
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), false, null, -1, null, null, null, fieldName,
								null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
						break;
					case TaxPluginDetail.Password:
						if (e.ReturnState != null)
						{
							string strValue = e.ReturnState.ToString();
							string encripted = new string('*', strValue.Length);

							e.ReturnState = PXFieldState.CreateInstance(encripted, typeof(string), false, null, -1, null, null, null, fieldName,
									null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
						}
						break;
					default:
						break;
				}
			}
		}



		protected virtual void TaxPlugin_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			TaxPlugin row = e.Row as TaxPlugin;
			if (row == null) return;

			TaxZone shipVia = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<TaxPlugin.taxPluginID>>>>.SelectWindowed(this, 0, 1);
			if (shipVia != null)
			{
				throw new PXException(Messages.TaxZoneFK);
			}

		}
	}
}
