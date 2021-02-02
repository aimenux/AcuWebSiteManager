using PX.Api.ContractBased.Models;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce
{
	public class BCPriceClassEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Group;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedGroup Group;
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.CustomerPriceClass, BCCaptions.CustomerPriceClass,
		IsInternal = false,
		Direction = SyncDirection.Bidirect,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.AR.ARPriceClassMaint),
		ExternTypes = new Type[] { typeof(CustomerGroupData) },
		LocalTypes = new Type[] { typeof(CustomerPriceClass) },
		AcumaticaPrimaryType = typeof(PX.Objects.AR.ARPriceClass),
		AcumaticaPrimarySelect = typeof(PX.Objects.AR.ARPriceClass.priceClassID),
		URL = "customers/groups/{0}/edit"
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-PriceClass" })]
	public class BCPriceClassProcessor : BCProcessorSingleBase<BCPriceClassProcessor, BCPriceClassEntityBucket, MappedGroup>, IProcessor
	{
		private CustomerPriceClassRestDataProvider customerPriceClassDataProvider;

		#region Constructor
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);

			customerPriceClassDataProvider = new CustomerPriceClassRestDataProvider(BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>()));
		}
		#endregion

		#region Common
		public override MappedGroup PullEntity(Guid? localID, Dictionary<string, object> externalInfo)
		{
			CustomerPriceClass impl = cbapi.GetByID<CustomerPriceClass>(localID);
			if (impl == null) return null;

			MappedGroup obj = new MappedGroup(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}

		public override MappedGroup PullEntity(string externID, String externalInfo)
		{
			CustomerGroupData data = customerPriceClassDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedGroup obj = new MappedGroup(data, data.Id?.ToString(), data.CalculateHash());

			return obj;
		}

		public override IEnumerable<MappedGroup> PullSimilar(IExternEntity entity, out string uniqueField)
		{
			uniqueField = ((CustomerGroupData)entity)?.Name;
			if (uniqueField == null) return null;

			CustomerPriceClass[] impls = cbapi.GetAll<CustomerPriceClass>(new CustomerPriceClass() { PriceClassID = uniqueField.SearchField() },
				filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>().ToArray());
			if (impls == null) return null;

			return impls.Select(impl => new MappedGroup(impl, impl.SyncID, impl.SyncTime));
		}

		public override IEnumerable<MappedGroup> PullSimilar(ILocalEntity entity, out string uniqueField)
		{
			uniqueField = ((CustomerPriceClass)entity)?.PriceClassID?.Value;
			if (uniqueField == null) return null;

			IEnumerable<CustomerGroupData> datas = customerPriceClassDataProvider.GetAll(new FilterGroups() { Name = uniqueField });
			if (datas == null) return null;

			return datas.Select(data => new MappedGroup(data, data.Id.ToString(), data.CalculateHash()));
		}


		#endregion

		#region Import
		public override EntityStatus GetBucketForImport(BCPriceClassEntityBucket bucket, BCSyncStatus bcstatus)
		{
			CustomerGroupData data = customerPriceClassDataProvider.GetByID(bcstatus.ExternID);
			if (data == null) return EntityStatus.None;

			MappedGroup obj = bucket.Group = bucket.Group.Set(data, data.Id?.ToString(), data.CalculateHash());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			return status;
		}
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			FilterGroups filter = null; 

			IEnumerable<CustomerGroupData> datas = customerPriceClassDataProvider.GetAll(filter);

			foreach (CustomerGroupData data in datas)
			{
				var bucket = CreateBucket();
				MappedGroup obj = bucket.Group = bucket.Group.Set(data, data.Id?.ToString(), data.CalculateHash());
				EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
			}
		}

		public override void MapBucketImport(BCPriceClassEntityBucket entity, IMappedEntity existing)
		{
			MappedGroup obj = entity.Group;
			CustomerPriceClass impl = obj.Local = new CustomerPriceClass();
			CustomerGroupData data = obj.Extern;

			impl.PriceClassID = data.Name.ValueField();
		}

		public override void SaveBucketImport(BCPriceClassEntityBucket bucket, IMappedEntity existing, string operation)
		{
			MappedGroup obj = bucket.Group;

			CustomerPriceClass impl = cbapi.Put<CustomerPriceClass>(obj.Local, obj.LocalID);

			if (obj.LocalID != impl.SyncID) obj.LocalID = null;
			bucket.Group.AddLocal(impl, impl.SyncID, impl.SyncTime);
			UpdateStatus(obj, operation);
		}
		#endregion

		#region Export
		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			IEnumerable<CustomerPriceClass> impls = cbapi.GetAll<CustomerPriceClass>(new CustomerPriceClass() { PriceClassID = new StringReturn() }, minDateTime, maxDateTime, filters);

			foreach (CustomerPriceClass impl in impls)
			{
				if (impl.SyncID == null) continue; //We need to skip the root node, which does not have a ID

				MappedGroup obj = new MappedGroup(impl, impl.SyncID, impl.SyncTime);
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
			}
		}
		public override EntityStatus GetBucketForExport(BCPriceClassEntityBucket bucket, BCSyncStatus bcstatus)
		{
			CustomerPriceClass impl = cbapi.GetByID<CustomerPriceClass>(bcstatus.LocalID);
			if (impl == null) return EntityStatus.None;

			MappedGroup obj = bucket.Group = bucket.Group.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			return status;
		}

		public override void MapBucketExport(BCPriceClassEntityBucket entity, IMappedEntity existing)
		{
			MappedGroup obj = entity.Group;
			CustomerGroupData data = obj.Extern = new CustomerGroupData();
			CustomerPriceClass impl = obj.Local;
			data.Name = impl.PriceClassID?.Value;
		}

		public override void SaveBucketExport(BCPriceClassEntityBucket bucket, IMappedEntity existing, string operation)
		{
			MappedGroup obj = bucket.Group;
			CustomerGroupData data;
			if (obj.ExternID == null)
				data = customerPriceClassDataProvider.Create(obj.Extern);
			else
				data = customerPriceClassDataProvider.Update(obj.Extern, obj.ExternID);

			obj.AddExtern(data, data.Id?.ToString(), data.CalculateHash());
			UpdateStatus(obj, operation);
		}
		#endregion
	}
}
