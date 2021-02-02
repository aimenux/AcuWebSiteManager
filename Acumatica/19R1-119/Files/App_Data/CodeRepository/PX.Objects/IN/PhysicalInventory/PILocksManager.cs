using PX.Common;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.PhysicalInventory
{
	public class PILocksInspector
	{
		private class SitePILocks : IPrefetchable<Func<List<PILocks>>>
		{
			public List<PILocks> Locks { get; private set; }

			public void Prefetch(Func<List<PILocks>> getPILocksFunc)
			{
				Locks = getPILocksFunc();
			}
		}

		protected int _siteId;

		protected virtual bool CacheLocks => true;

		public PILocksInspector(int siteId)
		{
			_siteId = siteId;
		}

		public virtual bool IsInventoryLocationIncludedInPI(int? inventoryID, int? locationID, string piID)
		{
			if (inventoryID == null || locationID == null)
				return false;

			List<PILocks> locks = GetLocks();
			return locks.Any(l
				=> l.PIID == piID
				&& l.HasCollision(inventoryID.Value, locationID.Value));
		}

		public virtual bool IsInventoryLocationLocked(int? inventoryID, int? locationID, string excludePIID)
		{
			if (inventoryID == null || locationID == null)
				return false;

			List<PILocks> locks = GetLocks();
			return locks.Any(l
				=> l.IsActive
				&& l.PIID != excludePIID
				&& l.HasCollision(inventoryID.Value, locationID.Value));
		}

		protected virtual List<PILocks> GetLocks()
		{
			if (CacheLocks)
			{
				return GetSitePILocksFromSlot(_siteId, LoadExistingLocks).Locks;
			}
			else
			{
				return LoadExistingLocks();
			}
		}

		private static SitePILocks GetSitePILocksFromSlot(int siteID, Func<List<PILocks>> getPILocksFunc)
		{
			return PXDatabase.GetSlot<SitePILocks, Func<List<PILocks>>>(
				$"{typeof(SitePILocks).FullName}~{siteID}",
				getPILocksFunc,
				GetTablesToWatch());
		}
		private static Type[] GetTablesToWatch() => new Type[] { typeof(INPIStatusItem), typeof(INPIStatusLoc) };

		protected virtual List<PILocks> LoadExistingLocks()
		{
			var dummyGraph = new PXGraph();
			var statusItemsQuery = new PXSelectReadonly<INPIStatusItem,
				Where<INPIStatusItem.siteID, Equal<Required<INPIStatusItem.siteID>>>,
				OrderBy<Asc<INPIStatusItem.pIID>>>(dummyGraph); // the sorting required for ToPiIdDictionary method

			Dictionary<string, List<INPIStatusItem>> itemLocksByPiId;
			using (new PXFieldScope(statusItemsQuery.View,
				typeof(INPIStatusItem.recordID),
				typeof(INPIStatusItem.inventoryID),
				typeof(INPIStatusItem.pIID),
				typeof(INPIStatusItem.excluded),
				typeof(INPIStatusItem.active)))
			{
				itemLocksByPiId = ToPiIdDictionary(
					statusItemsQuery
						.Select(_siteId)
						.RowCast<INPIStatusItem>());
			}

			var statusLocQuery = new PXSelectReadonly<INPIStatusLoc,
				Where<INPIStatusLoc.siteID, Equal<Required<INPIStatusLoc.siteID>>>,
				OrderBy<Asc<INPIStatusLoc.pIID>>>(dummyGraph); // the sorting required for ToPiIdDictionary method

			Dictionary<string, List<INPIStatusLoc>> locationLocksByPiId;
			using (new PXFieldScope(statusLocQuery.View,
				typeof(INPIStatusLoc.recordID),
				typeof(INPIStatusLoc.locationID),
				typeof(INPIStatusLoc.pIID),
				typeof(INPIStatusLoc.excluded),
				typeof(INPIStatusLoc.active)))
			{
				locationLocksByPiId = ToPiIdDictionary(
					statusLocQuery
						.Select(_siteId)
						.RowCast<INPIStatusLoc>());
			}

			if (itemLocksByPiId.Count != locationLocksByPiId.Count)
				PXTrace.WriteError(Messages.PIDBInconsistency);

			var existingLocks = new List<PILocks>();
			foreach (var itemLocksKvp in itemLocksByPiId)
			{
				var piId = itemLocksKvp.Key;
				var locationLocks = locationLocksByPiId[piId];
				existingLocks.Add(new PILocks(piId, itemLocksKvp.Value, locationLocks));
			}
			return existingLocks;
		}

		protected Dictionary<string, List<TLock>> ToPiIdDictionary<TLock>(IEnumerable<TLock> sortedLocks)
			where TLock : class, IBqlTable, IPILock, new()
		{
			string currentPiId = null;
			var result = new Dictionary<string, List<TLock>>();
			foreach (var piLock in sortedLocks)
			{
				var itemPiId = piLock.PIID;
				if (currentPiId != itemPiId)
				{
					result.Add(itemPiId, new List<TLock>());
					currentPiId = itemPiId;
				}

				result[currentPiId].Add(piLock);
			}
			return result;
		}
	}

	public class PILocksManager : PILocksInspector
	{
		protected PXGraph _graph;
		protected PXSelectBase<INPIStatusItem> _inPIStatusItem;
		protected PXSelectBase<INPIStatusLoc> _inPIStatusLocation;
		protected string _piId;

		protected override bool CacheLocks => false;

		public PILocksManager(
			PXGraph graph,
			PXSelectBase<INPIStatusItem> inPIStatusItem,
			PXSelectBase<INPIStatusLoc> inPIStatusLocation,
			int siteId,
			string piId)
			: base(siteId)
		{
			_graph = graph;
			_inPIStatusItem = inPIStatusItem;
			_inPIStatusLocation = inPIStatusLocation;
			_piId = piId;
		}

		public virtual void Lock(
			bool fullItemsLock, ICollection<int> inventoryItemIds,
			bool fullLocationsLock, ICollection<int> locationIds,
			string siteCD)
		{
			var existingLocks = GetLocks();

			var collisions = GetCollisions(existingLocks, fullItemsLock, inventoryItemIds, fullLocationsLock, locationIds);
			if (collisions.Any())
			{
				foreach (var collision in collisions)
				{
					collision.Notify(_graph, siteCD);
				}

				throw new PXException(Messages.PICollision, string.Join(", ", collisions.Select(c  => c.PIID)), siteCD);
			}

			if (fullItemsLock)
			{
				InsertInventoryLock(null);
			}
			foreach (var inventoryId in inventoryItemIds)
			{
				InsertInventoryLock(inventoryId, fullItemsLock);
			}

			if (fullLocationsLock)
			{
				InsertLocationLock(null);
			}
			foreach (var locationId in locationIds)
			{
				InsertLocationLock(locationId, fullLocationsLock);
			}
		}

		public virtual void UnlockInventory(bool deleteLock = true)
		{
			_inPIStatusItem.Cache.Clear();
			foreach (PXResult<INPIStatusItem> it in PXSelect<INPIStatusItem,
							 Where<INPIStatusItem.pIID, Equal<Required<INPIStatusItem.pIID>>>>.Select(_graph, _piId))
			{
				if(deleteLock)
				{
					_inPIStatusItem.Delete(it);
				}
				else
				{
					INPIStatusItem upd = PXCache<INPIStatusItem>.CreateCopy(it);
					upd.Active = false;
					_inPIStatusItem.Update(upd);
				}
			}

			_inPIStatusLocation.Cache.Clear();
			foreach (PXResult<INPIStatusLoc> it in PXSelect<INPIStatusLoc,
							 Where<INPIStatusLoc.pIID, Equal<Required<INPIStatusLoc.pIID>>>>.Select(_graph, _piId))
			{
				if (deleteLock)
				{
					_inPIStatusLocation.Delete(it);
				}
				else
				{
					INPIStatusLoc upd = PXCache<INPIStatusLoc>.CreateCopy(it);
					upd.Active = false;
					_inPIStatusLocation.Update(upd);
				}
			}
		}

		protected virtual void InsertInventoryLock(int? inventoryId, bool excluded = false)
		{
			if (inventoryId == null && excluded == true)
			{
				throw new PXArgumentException(nameof(excluded));
			}
			var item = new INPIStatusItem
			{
				SiteID = _siteId,
				PIID = _piId,
				InventoryID = inventoryId,
				Excluded = excluded,
			};
			_inPIStatusItem.Insert(item);
		}

		protected virtual void InsertLocationLock(int? locationId, bool excluded = false)
		{
			if (locationId == null && excluded == true)
			{
				throw new PXArgumentException(nameof(excluded));
			}
			var loc = new INPIStatusLoc
			{
				SiteID = _siteId,
				PIID = _piId,
				LocationID = locationId,
				Excluded = excluded,
			};
			_inPIStatusLocation.Insert(loc);
		}

		protected virtual List<PICollision> GetCollisions(
			List<PILocks> existingLocks,
			bool fullItemsLock, ICollection<int> inventoryItemIds,
			bool fullLocationsLock, ICollection<int> locationIds)
		{
			var collisions = new List<PICollision>();
			foreach (var piLock in existingLocks)
			{
				var collision = piLock.Intersect(fullItemsLock, inventoryItemIds, fullLocationsLock, locationIds);
				if (collision == null)
					continue;

				collisions.Add(collision);
			}
			return collisions;
		}
	}
}
