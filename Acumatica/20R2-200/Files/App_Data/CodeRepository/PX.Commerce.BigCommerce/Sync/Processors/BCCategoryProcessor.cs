using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.Common;
using Newtonsoft.Json;
using PX.Api.ContractBased.Models;

namespace PX.Commerce.BigCommerce
{
	public class BCCategoryEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Category;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };
		public override IMappedEntity[] PreProcessors { get => new IMappedEntity[] { LocalParent, ExternParent }; }

		public MappedCategory Category;
		public MappedCategory LocalParent;
		public MappedCategory ExternParent;
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.SalesCategory, BCCaptions.SalesCategory,
		IsInternal = false,
		Direction = SyncDirection.Bidirect,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.IN.INCategoryMaint),
		ExternTypes = new Type[] { typeof(ProductCategoryData) },
		LocalTypes = new Type[] { typeof(BCItemSalesCategory) },
		AcumaticaPrimaryType = typeof(PX.Objects.IN.INCategory),
		AcumaticaPrimarySelect = typeof(PX.Objects.IN.INCategory.categoryID),
		URL = "products/categories/{0}/edit"
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = true,
		PushSources = new String[] { "BC-PUSH-Category" },
		WebHookType = typeof(WebHookProductCategory),
		WebHooks = new String[]
		{
			"store/category/created",
			"store/category/updated",
			"store/category/deleted"
		})]
	public class BCCategoryProcessor : BCProcessorSingleBase<BCCategoryProcessor, BCCategoryEntityBucket, MappedCategory>, IProcessor
	{
		protected IParentRestDataProvider<ProductCategoryData> categoryDataProvider;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);

			cbapi.UseNoteID = true;
			categoryDataProvider = new ProductCategoryRestDataProvider(BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>()));
		}
		#endregion

		#region Common
		public override MappedCategory PullEntity(Guid? localID, Dictionary<string, object> fields)
		{
			BCItemSalesCategory impl = cbapi.GetByID<BCItemSalesCategory>(localID);
			if (impl == null) return null;

			MappedCategory obj = new MappedCategory(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override MappedCategory PullEntity(String externID, String jsonObject)
		{
			ProductCategoryData data = categoryDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedCategory obj = new MappedCategory(data, data.Id.ToString(), data.CalculateHash());

			return obj;
		}

		public override IEnumerable<MappedCategory> PullSimilar(IExternEntity entity, out String uniqueField)
		{
			uniqueField = ((ProductCategoryData)entity)?.Name;
			var parent = ((ProductCategoryData)entity)?.ParentId;
			if (uniqueField == null) return null;

			PX.Objects.IN.INCategory incategory = PXSelectJoin<PX.Objects.IN.INCategory,
					LeftJoin<BCSyncStatus, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Current<BCEntity.entityType>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, parent);
			int parentId = incategory?.CategoryID ?? 0;
			BCItemSalesCategory[] impls = cbapi.GetAll<BCItemSalesCategory>(new BCItemSalesCategory() { Description = uniqueField.SearchField(), ParentCategoryID = parentId.SearchField() },
				filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>().ToArray());
			if (impls == null) return null;

			return impls.Select(impl => new MappedCategory(impl, impl.SyncID, impl.SyncTime));
		}
		public override IEnumerable<MappedCategory> PullSimilar(ILocalEntity entity, out String uniqueField)
		{
			uniqueField = ((BCItemSalesCategory)entity)?.Description?.Value;
			var parent = ((BCItemSalesCategory)entity)?.ParentCategoryID?.Value;
			if (uniqueField == null) return null;

			BCSyncStatus parentStatus = PXSelectJoin<BCSyncStatus,
					LeftJoin<PX.Objects.IN.INCategory, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<PX.Objects.IN.INCategory.categoryID, Equal<Required<PX.Objects.IN.INCategory.categoryID>>>>>>>
						.Select(this, BCEntitiesAttribute.SalesCategory, parent);
			int parentId = parentStatus?.ExternID?.ToInt() ?? 0;
			IEnumerable<ProductCategoryData> datas = categoryDataProvider.GetAll(new FilterProductCategories() { Name = uniqueField, ParentId = parentId });
			if (datas == null) return null;

			return datas.Select(data => new MappedCategory(data, data.Id.ToString(), data.CalculateHash()));
		}

		public override void NavigateLocal(IConnector connector, ISyncStatus status)
		{
			PX.Objects.IN.INCategoryMaint extGraph = PXGraph.CreateInstance<PX.Objects.IN.INCategoryMaint>();
			PX.Commerce.Objects.BCINCategoryMaintExt extGraphExt = extGraph.GetExtension<PX.Commerce.Objects.BCINCategoryMaintExt>();
			extGraphExt.SelectedCategory.Current = PXSelect<PX.Commerce.Objects.BCINCategoryMaintExt.SelectedINCategory,
			  Where<PX.Commerce.Objects.BCINCategoryMaintExt.SelectedINCategory.noteID, Equal<Required<PX.Commerce.Objects.BCINCategoryMaintExt.SelectedINCategory.noteID>>>>.Select(extGraph, status.LocalID);

			throw new PXRedirectRequiredException(extGraph, true, "Navigation");
		}
		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			FilterProductCategories filter = null; //No DateTime filtering for Category

			IEnumerable<ProductCategoryData> datas = categoryDataProvider.GetAll(filter);

			foreach (ProductCategoryData data in datas)
			{
				if (data == null) continue;

				BCCategoryEntityBucket bucket = CreateBucket();

				MappedCategory obj = bucket.Category = bucket.Category.Set(data, data.Id?.ToString(), data.CalculateHash());
				EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
			}
		}
		public override EntityStatus GetBucketForImport(BCCategoryEntityBucket bucket, BCSyncStatus bcstatus)
		{
			ProductCategoryData data = categoryDataProvider.GetByID(bcstatus.ExternID);
			if (data == null) return EntityStatus.None;

			MappedCategory obj = bucket.Category = bucket.Category.Set(data, data.Id?.ToString(), data.CalculateHash());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			Int32? parent = obj.Extern.ParentId;
			if (parent != null && parent > 0)
			{
				ProductCategoryData parentData = categoryDataProvider.GetByID(parent.ToString());
				if (parentData != null)
				{
					MappedCategory parentObj = bucket.ExternParent = bucket.ExternParent.Set(parentData, parentData.Id?.ToString(), data.CalculateHash());

					EntityStatus parentStatus = EnsureStatus(parentObj);
				}
			}

			return status;
		}

		public override void MapBucketImport(BCCategoryEntityBucket bucket, IMappedEntity existing)
		{
			MappedCategory obj = bucket.Category;

			ProductCategoryData data = obj.Extern;
			BCItemSalesCategory impl = obj.Local = new BCItemSalesCategory();
			impl.Custom = GetCustomFieldsForImport();

			//Category
			impl.Description = data.Name.ValueField();
			impl.SortOrder = data.SortOrder.ValueField();

			if (data.ParentId != null && data.ParentId > 0 && impl.Description?.Value != null)
			{
				PX.Objects.IN.INCategory incategory = PXSelectJoin<PX.Objects.IN.INCategory,
					LeftJoin<BCSyncStatus, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Current<BCEntity.entityType>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, data.ParentId);
				if (incategory == null) throw new PXException(BCMessages.CategoryNotSyncronizedParent, impl.Description.Value);
				obj.Local.ParentCategoryID = incategory.CategoryID.ValueField();
			}
			else obj.Local.ParentCategoryID = 0.ValueField();
		}
		public override void SaveBucketImport(BCCategoryEntityBucket bucket, IMappedEntity existing, String operation)
		{
			//Category
			MappedCategory obj = bucket.Category;

			if (existing?.Local != null) obj.Local.CategoryID = ((BCItemSalesCategory)existing.Local).CategoryID.Value.SearchField();

			// Prevent to save of category with no changes. Workaround for a bug in Acumatica, where it returns the wrong record in case not changes.
			if (existing != null && ((BCItemSalesCategory)existing.Local).Description?.Value == obj.Local.Description?.Value
				&& (((BCItemSalesCategory)existing.Local).ParentCategoryID?.Value ?? 0) == (obj.Local.ParentCategoryID?.Value ?? 0))
			{
				UpdateStatus(obj, operation);
				return;
			}

			BCItemSalesCategory impl = cbapi.Put<BCItemSalesCategory>(obj.Local, obj.LocalID);

			bucket.Category.AddLocal(impl, impl.SyncID, impl.SyncTime);
			UpdateStatus(obj, operation);
		}
		#endregion

		#region Export
		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			IEnumerable<BCItemSalesCategory> impls = cbapi.GetAll<BCItemSalesCategory>(new BCItemSalesCategory() { Path = new StringReturn() }, minDateTime, maxDateTime, filters, GetCustomFieldsForExport());
			var invIDs = new List<int>();

			foreach (BCItemSalesCategory impl in impls)
			{
				if (impl.SyncID == null) continue; //We need to skip the root node, which does not have a ID

				MappedCategory obj = new MappedCategory(impl, impl.SyncID, impl.SyncTime);
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
				if (status == EntityStatus.Pending) invIDs.Add(impl?.CategoryID?.Value ?? 0);
			}
		}
		public override EntityStatus GetBucketForExport(BCCategoryEntityBucket bucket, BCSyncStatus bcstatus)
		{
			BCItemSalesCategory impl = cbapi.GetByID<BCItemSalesCategory>(bcstatus.LocalID, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			MappedCategory obj = bucket.Category = bucket.Category.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			Int32? parent = obj.Local.ParentCategoryID?.Value;
			if (parent != null && parent > 0)
			{
				BCItemSalesCategory parentImpl = cbapi.Get<BCItemSalesCategory>(new BCItemSalesCategory() { CategoryID = parent.ValueField() });
				if (parentImpl != null)
				{
					MappedCategory parentObj = bucket.LocalParent = bucket.LocalParent.Set(parentImpl, parentImpl.SyncID, parentImpl.SyncTime);
					EntityStatus parentStatus = EnsureStatus(parentObj);
				}
			}
			return status;
		}

		public override void MapBucketExport(BCCategoryEntityBucket bucket, IMappedEntity existing)
		{
			MappedCategory obj = bucket.Category;

			BCItemSalesCategory impl = obj.Local;
			ProductCategoryData data = obj.Extern = new ProductCategoryData();

			//Contact
			data.Name = impl.Description?.Value;
			data.SortOrder = impl.SortOrder?.Value;

			if (impl.ParentCategoryID?.Value != null && impl.ParentCategoryID?.Value > 0 && data.Name != null)
			{
				BCSyncStatus parentStatus = PXSelectJoin<BCSyncStatus,
					LeftJoin<PX.Objects.IN.INCategory, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<PX.Objects.IN.INCategory.categoryID, Equal<Required<PX.Objects.IN.INCategory.categoryID>>>>>>>
						.Select(this, BCEntitiesAttribute.SalesCategory, impl.ParentCategoryID?.Value);
				if (parentStatus == null) throw new PXException(BCMessages.CategoryNotSyncronizedParent, data.Name);

				data.ParentId = parentStatus.ExternID.ToInt();
			}
			else data.ParentId = 0;
		}


		public override void SaveBucketExport(BCCategoryEntityBucket bucket, IMappedEntity existing, String operation)
		{
			//Category
			MappedCategory obj = bucket.Category;

			ProductCategoryData data = null;
			if (obj.ExternID == null)
				data = categoryDataProvider.Create(obj.Extern);
			else
				data = categoryDataProvider.Update(obj.Extern, obj.ExternID.ToInt().Value);
			obj.AddExtern(data, data.Id?.ToString(), data.CalculateHash());
			UpdateStatus(obj, operation);
		}
		#endregion
	}
}
