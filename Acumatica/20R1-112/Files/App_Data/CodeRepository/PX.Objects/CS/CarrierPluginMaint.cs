using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Web.Compilation;
using PX.SM;
using PX.Data.Update;
using PX.Objects.IN;
using System.Collections;
using PX.CarrierService;
using PX.DbServices;

namespace PX.Objects.CS
{
	public class CarrierPluginMaint : PXGraph<CarrierPluginMaint, CarrierPlugin>
	{
		public PXSelect<CarrierPlugin> Plugin;
		public PXSelect<CarrierPluginDetail> Details;
		public PXSelect<CarrierPluginDetail, Where<CarrierPluginDetail.carrierPluginID, Equal<Current<CarrierPlugin.carrierPluginID>>>> SelectDetails;
		protected IEnumerable details()
		{
			ImportSettings();

			return SelectDetails.Select();
		}


		public PXSelect<CarrierPluginCustomer, Where<CarrierPluginCustomer.carrierPluginID, Equal<Current<CarrierPlugin.carrierPluginID>>>> CustomerAccounts;


		
		public virtual void ImportSettings()
		{
			if (Plugin.Current != null)
			{
				ICarrierService plugin = CreateCarrierService(this, Plugin.Current);
				if (plugin != null)
				{
					IList<ICarrierDetail> details = plugin.ExportSettings();
					InsertDetails(details);
				}
			}
		}

		public PXAction<CarrierPlugin> certify;
		[PXUIField(DisplayName = "Prepare Certification Files", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual void Certify()
		{
			if (Plugin.Current != null)
			{
				Save.Press();

				PXLongOperation.StartOperation(this, delegate()
				{
					CarrierPluginMaint docgraph = PXGraph.CreateInstance<CarrierPluginMaint>();
					docgraph.PrepareCertificationData(Plugin.Current);
				});
			}

		}

		public PXAction<CarrierPlugin> test;
		[PXUIField(DisplayName = "Test connection", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual void Test()
		{
			if (Plugin.Current != null)
			{
				Save.Press();
				
				ICarrierService plugin = CreateCarrierService(this, Plugin.Current);
				if (plugin != null)
				{
					CarrierResult<string> result = plugin.Test();
					if (result.IsSuccess)
					{
						Plugin.Ask(Plugin.Current, Messages.ConnectionCarrierAskSuccessHeader, Messages.ConnectionCarrierAskSuccess, MessageButtons.OK, MessageIcon.Information);
					}
					else
					{
						StringBuilder errorMessages = new StringBuilder();

						foreach (Message message in result.Messages)
						{
							errorMessages.AppendLine(message.Description);
						}

						if (errorMessages.Length > 0)
						{
							throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.TestFailed, errorMessages.ToString()));
						}

					}
				}
			}

		}

		private void PrepareCertificationData(CarrierPlugin cp)
		{
			ICarrierService plugin = CreateCarrierService(this, cp);
			if (plugin != null)
			{
				UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();
				CarrierResult<IList<CarrierCertificationData>> result = plugin.GetCertificationData();

				if (result != null)
				{
					StringBuilder sb = new StringBuilder();
					foreach (Message message in result.Messages)
					{
						sb.AppendFormat("{0}:{1} ", message.Code, message.Description);
					}

					if (result.IsSuccess)
					{
						CarrierPlugin copy = (CarrierPlugin) Plugin.Cache.CreateCopy(cp);

						using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
						{
							ZipArchive zip = new ZipArchive(ms);
							foreach (CarrierCertificationData d in result.Result)
							{
								using (System.IO.Stream zipStream = zip.OpenWrite(string.Format("{0}.{1}", d.Description, d.Format)))
								{
									zipStream.Write(d.File, 0, d.File.Length);
								}
							}

							FileInfo file = new FileInfo("CertificationData.zip", null, ms.ToArray());
							upload.SaveFile(file, FileExistsAction.CreateVersion);
							PXNoteAttribute.SetFileNotes(Plugin.Cache, copy, file.UID.Value);
						}

						Plugin.Update(copy);

						this.Save.Press();
					}
					else
					{
						throw new PXException(SO.Messages.CarrierServiceError, sb.ToString());
					}
				}
			}
		}
		
		public virtual void InsertDetails(IList<ICarrierDetail> list)
		{
			Dictionary<string, CarrierPluginDetail> existingRecords = new Dictionary<string, CarrierPluginDetail>();
			foreach (CarrierPluginDetail detail in SelectDetails.Select())
			{
				existingRecords.Add(detail.DetailID.ToUpper(), detail);
			}

			foreach (ICarrierDetail item in list)
			{
				if (!existingRecords.ContainsKey(item.DetailID.ToUpper()))
				{
					CarrierPluginDetail row = (CarrierPluginDetail)SelectDetails.Insert(new CarrierPluginDetail { DetailID = item.DetailID });
					row.Descr = item.Descr;
					row.Value = item.Value;
					row.ControlType = item.ControlType;
					row.SetComboValues(item.GetComboValues());
				}
				else
				{
					CarrierPluginDetail cd = existingRecords[item.DetailID];
					CarrierPluginDetail copy = PXCache<CarrierPluginDetail>.CreateCopy(cd);
					
					if (!string.IsNullOrEmpty(item.Descr))
						copy.Descr = item.Descr;
					copy.ControlType = item.ControlType;
					copy.SetComboValues(item.GetComboValues());
					
					if ( cd.Descr != copy.Descr || cd.ControlType != copy.ControlType || cd.ComboValues != copy.ComboValues )
						SelectDetails.Update(copy);
				}
			}

		}

		public static ICarrierService CreateCarrierService(PXGraph graph, CarrierPlugin plugin)
		{
			ICarrierService service = null;
			if (!string.IsNullOrEmpty(plugin.PluginTypeName))
			{
				try
				{
					Type carrierType = PXBuildManager.GetType(plugin.PluginTypeName, true);
					service = (ICarrierService)Activator.CreateInstance(carrierType);
                                       
					PXSelectBase<CarrierPluginDetail> select = new PXSelect<CarrierPluginDetail, Where<CarrierPluginDetail.carrierPluginID, Equal<Required<CarrierPluginDetail.carrierPluginID>>>>(graph);
					PXResultset<CarrierPluginDetail> resultset = select.Select(plugin.CarrierPluginID);
					IList<ICarrierDetail> list = new List<ICarrierDetail>(resultset.Count);

					foreach (CarrierPluginDetail item in resultset)
					{
						list.Add(item);
					}
                                        
					service.LoadSettings(list);

				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCreateCarrierPlugin, ex.Message);
				}
			}


			return service;
		}

		public static IList<string> GetCarrierPluginAttributes(PXGraph graph, string carrierPluginID)
		{
			CarrierPlugin plugin = CarrierPlugin.PK.Find(graph, carrierPluginID);

			if (plugin == null)
				throw new PXException(Messages.FailedToFindCarrierPlugin, carrierPluginID);

			ICarrierService service = null;
			if (!string.IsNullOrEmpty(plugin.PluginTypeName))
			{
				try
				{
					Type carrierType = PXBuildManager.GetType(plugin.PluginTypeName, true);
					service = (ICarrierService)Activator.CreateInstance(carrierType);
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCreateCarrierPlugin, ex.Message);
				}
			}

			return service == null ? new List<String>() : service.Attributes;
		}

		public static ICarrierService CreateCarrierService(PXGraph graph, string carrierPluginID)
		{
			CarrierPlugin plugin = CarrierPlugin.PK.Find(graph, carrierPluginID);

			if (plugin == null)
				throw new PXException(Messages.FailedToFindCarrierPlugin, carrierPluginID);

			return CreateCarrierService(graph, plugin);
		}

		protected virtual void CarrierPlugin_PluginTypeName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CarrierPlugin row = e.Row as CarrierPlugin;
			if (row == null) return;

			foreach (CarrierPluginDetail detail in SelectDetails.Select())
			{
				SelectDetails.Delete(detail);
			}
		}

		protected virtual void CarrierPluginDetail_Value_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CarrierPluginDetail row = e.Row as CarrierPluginDetail;
			if (row != null)
			{
				string fieldName = typeof(CarrierPluginDetail.value).Name;

				switch (row.ControlType)
				{
					case CarrierPluginDetail.Combo:
						List<string> labels = new List<string>();
						List<string> values = new List<string>();
						foreach (KeyValuePair<string, string> kv in row.GetComboValues())
						{
							values.Add(kv.Key);
							labels.Add(kv.Value);
						}
						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CarrierDetail.ValueFieldLength, null, fieldName, false, 1, null,
																values.ToArray(), labels.ToArray(), true, null);
						break;
					case CarrierPluginDetail.CheckBox:
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), false, null, -1, null, null, null, fieldName,
								null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
						break;
					case CarrierPluginDetail.Password:
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

		protected virtual void CarrierPlugin_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CarrierPlugin row = e.Row as CarrierPlugin;
			if (row == null) return;

			Carrier shipVia = PXSelect<Carrier, Where<Carrier.carrierPluginID, Equal<Current<CarrierPlugin.carrierPluginID>>>>.SelectWindowed(this, 0, 1);
			if (shipVia != null)
			{
				throw new PXException(Messages.ShipViaFK);
			}

		}

		protected virtual void CarrierPlugin_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CarrierPlugin row = e.Row as CarrierPlugin;
			if (row == null) return;

			ICarrierService service = CreateCarrierService(this, row);
			if ( service != null)
				certify.SetVisible(service.Attributes.Contains("CERTIFICATE"));
		}

		protected virtual void _(Events.FieldUpdated<CarrierPlugin, CarrierPlugin.unitType> args)
		{
			Plugin.Cache.SetValueExt<CarrierPlugin.kilogramUOM>(args.Row, null);
			Plugin.Cache.SetValueExt<CarrierPlugin.poundUOM>(args.Row, null);
			Plugin.Cache.SetValueExt<CarrierPlugin.centimeterUOM>(args.Row, null);
			Plugin.Cache.SetValueExt<CarrierPlugin.inchUOM>(args.Row, null);
		}
	}
}
