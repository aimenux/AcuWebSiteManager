using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using CommonServiceLocator;
using PX.Api.ContractBased.Models;
using PX.Api.ContractBased.UI.DAC;
using PX.Commerce.Core;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.Data.Maintenance.GI;
using PX.Objects.CR;

namespace PX.Commerce.Objects
{
	#region ConnectorService
	public class ConnectorService : IConnectorService
	{
		public class Definition : IPrefetchable
		{
			public const string SLOT = "BCConnectorsDef";

			public List<Tuple<String, String, Boolean>> AllEntities = new List<Tuple<String, String, Boolean>>();
			public Dictionary<String, ConnectorInfo> Connectors = new Dictionary<String, ConnectorInfo>();
			public Dictionary<String, List<EntityInfo>> ConnectorEntities = new Dictionary<String, List<EntityInfo>>();
			public Dictionary<String, List<BindingInfo>> ConnectorStores = new Dictionary<String, List<BindingInfo>>();
			
			public void Prefetch()
			{
				AllEntities = new List<Tuple<String, String, Boolean>>();
				Connectors = new Dictionary<String, ConnectorInfo>();
				ConnectorEntities = new Dictionary<String, List<EntityInfo>>();
				ConnectorStores = new Dictionary<String, List<BindingInfo>>();

				IReadOnlyDictionary<string, IConnectorFactory> factories = ServiceLocator.Current.GetInstance<IReadOnlyDictionary<string, IConnectorFactory>>();
				foreach (KeyValuePair<string, IConnectorFactory> pair in factories)
				{
					Connectors[pair.Key] = new ConnectorInfo() { ConnectorType = pair.Key, ConnectorName = pair.Value.Description, IsActive = (pair.Value?.Enabled ?? false) };

					List<EntityInfo> entities = ConnectorEntities[pair.Key] = pair.Value.GetProcessors();

					List<BindingInfo> stores = ConnectorStores[pair.Key] = new List<BindingInfo>();
					foreach (PXDataRecord rec in PXDatabase.SelectMulti<BCBinding>(
						new PXDataField<BCBinding.bindingID>(),
						new PXDataField<BCBinding.bindingName>(),
						new PXDataField<BCBinding.isActive>(),
						new PXDataField<BCBinding.isDefault>(),
						new PXDataFieldValue<BCBinding.connectorType>(pair.Key)
						))
					{
						stores.Add(new BindingInfo() 
						{ 
							BindingID = rec.GetInt32(0).Value, 
							BindingName = rec.GetString(1), 
							IsActive = (rec.GetBoolean(2) ?? false) && (pair.Value?.Enabled ?? false), 
							IsDefault = (rec.GetBoolean(3) ?? false) });
					}
					foreach (PXDataRecord rec in PXDatabase.SelectMulti<BCEntity>(
						new PXDataField<BCEntity.bindingID>(),
						new PXDataField<BCEntity.entityType>(),
						new PXDataField<BCEntity.direction>(),
						new PXDataField<BCEntity.primarySystem>(),
						new PXDataFieldValue<BCEntity.isActive>(true),
						new PXDataFieldValue<BCEntity.connectorType>(pair.Key)
						))
					{
						Int32? bindingID = rec.GetInt32(0);
						String entityType = rec.GetString(1);
						String direction = rec.GetString(2);
						String primary = rec.GetString(3);

						EntityInfo entity = entities.FirstOrDefault(s => s.EntityType == entityType);
						if (entity != null)
						{
							if (direction != null) entity.ActualDirection = BCSyncDirectionAttribute.Convert(direction);
							if (primary != null) entity.ActualPrimarySystem = BCSyncSystemAttribute.Convert(primary);
						}

						BindingInfo binding = stores.FirstOrDefault(s => s.BindingID == bindingID);
						if (binding != null && (pair.Value?.Enabled ?? false)) 
							binding.ActiveEntities.Add(entityType);
					}

					foreach (EntityInfo entity in entities)
					{
						Tuple<String, String, Boolean> found = AllEntities.FirstOrDefault(s => s.Item1 == entity.EntityType);
						if (found == null) AllEntities.Add(Tuple.Create(entity.EntityType, entity.EntityName, true));

						if (entity.DetailTypes != null && entity.DetailTypes.Length > 0 && entity.DetailTypes.Length % 2 == 0)
						{
							for (int i = 0; i < entity.DetailTypes.Length; i = i + 2)
							{
								String key = entity.DetailTypes[i];
								String value = entity.DetailTypes[i + 1];

								found = AllEntities.FirstOrDefault(s => s.Item1 == key);
								if (found == null) AllEntities.Add(Tuple.Create(key, value, false));
							}
						}
					}
				}
			}
		}
		public class SchemaDefiniton : IPrefetchable<SchemaDefiniton.SchemaRequest>
		{
			public const string SLOT = "BCConnectorsDef";
			public const string StockItemScreenID = "IN202500";
			public const string NonStockItemScreenID = "IN202000";

			public class SchemaRequest
			{
				public String ConnectorType;
				public Int32? BindingID;
				public EntityInfo Entity;
				public IConnectorDescriptor Descriptor;
			}
			public SchemaInfo ConnectorSchema;

			public void Prefetch(SchemaRequest request)
			{
				ConnectorSchema = new SchemaInfo();
				ConnectorSchema.EntityType = request.Entity.EntityType;
                ConnectorSchema.FormFields = request.Descriptor.GetExternalFields(request.ConnectorType, request.BindingID, request.Entity.EntityType);
				ConnectorSchema.CustomFields = GetCustomFields(request);
                if ( request.Entity.AcumaticaPrimaryType != null)
                {
                    ConnectorSchema.Attributes = CRAttribute.EntityAttributes(request.Entity.AcumaticaPrimaryType.FullName).Select(x => new Tuple<string, string>(x.Description, x.Description)).ToList();
                }

			}

			//TODO Need review and improve this method
			private static List<CustomFieldInfo> GetCustomFields(SchemaRequest request)
			{
				Type cbApiType = request.Entity.LocalTypes.FirstOrDefault();
				if (cbApiType == null) return new List<CustomFieldInfo>();

				string screenID;
				string sEndPoint = String.Empty;
				string sEndPointVersion = String.Empty;
				using (PXDataRecord rec = PXDatabase.SelectSingle<BCBinding>(
											new PXDataField<BCBinding.webServicesEndpoint>(),
											new PXDataField<BCBinding.webServiceVersion>(),
											new PXDataFieldValue<BCBinding.connectorType>(request.ConnectorType),
											new PXDataFieldValue<BCBinding.bindingID>(request.BindingID)))
				{
					sEndPoint = rec?.GetString(0);
					sEndPointVersion = rec?.GetString(1);
				}
				//get screen ID 
				using (PXDataRecord rec = PXDatabase.SelectSingle<EntityDescription>(
											new PXDataField<EntityDescription.screenID>(),
											new PXDataFieldValue<EntityDescription.interfaceName>(sEndPoint),
											new PXDataFieldValue<EntityDescription.gateVersion>(sEndPointVersion),
											new PXDataFieldValue<EntityDescription.objectName>(cbApiType.Name)))
				{
					screenID = rec?.GetString(0);
				}


				ICBAPIServiceFactory factory = ServiceLocator.Current.GetInstance<ICBAPIServiceFactory>();
				List<CustomFieldInfo> customFieldsDict = factory.GetService(sEndPoint, sEndPointVersion, String.Empty, 0, null).GetCustomFieldsSchema(cbApiType, screenID);
				if (request.Entity.PrimaryGraph != null && customFieldsDict?.Count > 0)
				{
					var fieldsList = customFieldsDict.Select(x => new { x.Container, x.Field.ViewName, x.Field.FieldName }).Distinct().ToList();

					PXSiteMap.ScreenInfo screen = screenID != null ? Api.ScreenUtils.GetScreenInfoWithoutHttpContext(screenID) : null;
					if (screen != null)
					{
						IEnumerable<KeyValuePair<string, Data.Description.PXViewDescription>> fieldContainers = screen.Containers.Where(x => fieldsList.Any(f => f.ViewName == x.Key || x.Key.StartsWith($"{f.ViewName}:"))).Distinct();
						fieldsList.ForEach(item =>
						{
							List<Data.Description.PXViewDescription> viewDescription = fieldContainers.Where(x => x.Key == item.ViewName || x.Key.StartsWith($"{item.ViewName}:")).Select(f => f.Value).ToList();
							Data.Description.FieldInfo fieldInfo = viewDescription?.SelectMany(x => x.Fields).FirstOrDefault(f => f.FieldName == item.FieldName);
							if (fieldInfo != null)
							{
								customFieldsDict.Where(x => x.Container == item.Container && x.Field.FieldName == item.FieldName && x.Field.ViewName == item.ViewName).All(x =>
								{
									x.DisplayName = fieldInfo.DisplayName;
									return true;
								});
							}
						});
					}
				}
				return customFieldsDict;
			}
		}
		
		protected Definition GetDefinition()
		{
			return PXDatabase.GetSlot<Definition>(Definition.SLOT, typeof(BCBinding), typeof(BCEntity));
		}
		protected SchemaDefiniton GetSchemaDefinition(SchemaDefiniton.SchemaRequest request)
		{
			return PXDatabase.GetSlot<SchemaDefiniton, SchemaDefiniton.SchemaRequest>(Definition.SLOT + request.Entity.EntityType,
				request, typeof(BCBinding), typeof(BCEntity), typeof(PX.Objects.CS.CSAttributeGroup), typeof(GIResult));
		}

		public List<ConnectorInfo> GetConnectors()
		{
			Definition def = GetDefinition();

			return def?.Connectors?.Values?.ToList();
		}
		public IConnector GetConnector(String code)
		{
			if (String.IsNullOrEmpty(code)) return null;

			if (ServiceLocator.IsLocationProviderSet)
			{
				IReadOnlyDictionary<string, IConnectorFactory> factories = ServiceLocator.Current.GetInstance<IReadOnlyDictionary<string, IConnectorFactory>>();
				if (factories.ContainsKey(code))
				{
					return factories[code].GetConnector();
				}
			}
			return null;
		}
		public IConnectorDescriptor GetConnectorDescriptor(String code)
		{
			if (String.IsNullOrEmpty(code)) return null;

			if (ServiceLocator.IsLocationProviderSet)
			{
				IReadOnlyDictionary<string, IConnectorFactory> factories = ServiceLocator.Current.GetInstance<IReadOnlyDictionary<string, IConnectorFactory>>();
				if (factories.ContainsKey(code))
				{
					return factories[code].GetDescriptor();
				}
			}
			return null;
		}

		public List<EntityInfo> GetConnectorEntites(String code)
		{
			if (String.IsNullOrEmpty(code)) return null;

			Definition def = GetDefinition();

			return def.ConnectorEntities.ContainsKey(code) ? def.ConnectorEntities[code] : null;
		}
		public EntityInfo GetConnectorEntity(String connectorCode, String entityType)
		{
			List<EntityInfo> entities = GetConnectorEntites(connectorCode);
			return entities == null ? null : entities.Where(x => x.EntityType == entityType).FirstOrDefault();
		}
		
		public List<Tuple<String, String, Boolean>> GetAllEntities()
		{
			Definition def = GetDefinition();

			return def.AllEntities;
		}

		public List<BindingInfo> GetConnectorBindings(String code)
		{
			if (String.IsNullOrEmpty(code)) return null;

			Definition def = GetDefinition();

			return def.ConnectorStores.ContainsKey(code) ? def.ConnectorStores[code] : null;
		}
		public BindingInfo GetConnectorBinding(String code, Int32? bindingID)
		{
			List<BindingInfo> bindings = GetConnectorBindings(code);
			return bindings == null ? null : bindings.Where(x => x.BindingID == bindingID).FirstOrDefault();
		}

		public SchemaInfo GetConnectorSchema(String code, Int32? binding, String entityType)
		{
			if (String.IsNullOrEmpty(code)) return null;

			EntityInfo entity = GetConnectorEntity(code, entityType);
			IConnectorDescriptor descriptor = GetConnectorDescriptor(code);
			if (entity != null && descriptor != null)
			{
				SchemaDefiniton def = GetSchemaDefinition(new SchemaDefiniton.SchemaRequest() { Descriptor = descriptor, ConnectorType = code, BindingID = binding, Entity = entity });

				return def.ConnectorSchema;
			}
			return null;
		}
	}
	#endregion

	#region ConnectorServiceRegistration
	internal class ConnectorServiceRegistration : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<ConnectorService>()
				.As<IConnectorService>()
				.SingleInstance();
		}
	}
	#endregion
}
