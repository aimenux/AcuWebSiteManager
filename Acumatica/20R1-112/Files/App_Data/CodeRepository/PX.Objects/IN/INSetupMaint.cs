using System;
using System.Collections;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.IN
{
	public class INSetupMaint : PXGraph<INSetupMaint>, IFeatureAccessProvider
	{
		public PXSelect<INSetup> setup;
		public PXSelect<GS1UOMSetup> gs1setup;
		public PXSave<INSetup> Save;
		public PXCancel<INSetup> Cancel;

        public PXSelect<INSite> sites;
        public PXSelect<INLocation> locations;

        public PXSelect<PX.SM.RelationGroup,
			Where<PX.SM.RelationGroup.specificModule, Equal<inventoryModule>,
			And<PX.SM.RelationGroup.specificType, Equal<segmentValueType>>>>
			Groups;
		public PXSelect<INScanSetup, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>> ScanSetup;
		protected virtual IEnumerable scanSetup()
		{
			INScanSetup result = PXSelect<INScanSetup, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
			return new INScanSetup[] { result ?? ScanSetup.Insert() };
		}

		public PXSelect<INScanUserSetup, Where<INScanUserSetup.isOverridden, Equal<False>>> ScanUserSetups;

		public CR.CRNotificationSetupList<INNotification> Notifications;

		public INSetupMaint()
		{
			PXCache setupCache = setup.Cache;
		}

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(INSite.siteID), DefaultForInsert = true, DefaultForUpdate = true)]
        [PXParent(typeof(INLocation.FK.Site))]
        protected virtual void INLocation_SiteID_CacheAttached(PXCache sender)
        {
        }

        protected virtual void INSetup_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            var row = (INSetup)e.Row;
            if (row.TransitSiteID == null)
            {
                var sitecache = sites.Cache;
                INSite transitSite = (INSite)sitecache.CreateInstance();
				transitSite.BranchID = row.TransitSiteID;
                transitSite.SiteCD = "INTR";
                transitSite.Descr = "Transit default warehouse";
                transitSite.OverrideInvtAccSub = true;
                sitecache.Current = transitSite;
				transitSite = (INSite)sitecache.Insert(transitSite);
   
                row.TransitSiteID = transitSite.SiteID;
                sender.Update(row);
                PX.Common.PXContext.ClearSlot(typeof(SiteAttribute.Definition));
            }
        }

		protected virtual void INSetup_TransitBranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (INSetup)e.Row;
			if (row.TransitBranchID != null)
			{
				var transitsite = INSite.PK.Find(this, row.TransitSiteID);
				if (transitsite != null)
					transitsite.BranchID = row.TransitBranchID;
				sites.Cache.Update(transitsite);
			}
		}
		
		protected virtual void INSetup_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			INSetup row = (INSetup)e.Row;
			if (row == null) return;
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			short newValue = (short) row.TurnoverPeriodsPerYear;
			
			if (newValue > 12 || newValue < 1 || (newValue != 0) && ((short)(12/newValue)) * newValue != 12)
			{
				cache.RaiseExceptionHandling<INSetup.turnoverPeriodsPerYear>(row, newValue, new PXSetPropertyException(Messages.PossibleValuesAre));
			}

			PXDefaultAttribute.SetPersistingCheck<INSetup.iNTransitAcctID>(cache, e.Row, PXAccess.FeatureInstalled<FeaturesSet.warehouse>() ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<INSetup.iNTransitSubID>(cache, e.Row, PXAccess.FeatureInstalled<FeaturesSet.warehouse>() ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
		}	

		protected virtual void _(Events.RowUpdated<INScanSetup> e)
		{
			if (e.Row != null)
				foreach (INScanUserSetup userSetup in ScanUserSetups.Select())
					ScanUserSetups.Update(userSetup.ApplyValuesFrom(e.Row));
		}

        #region RelationGroup events
        protected virtual void RelationGroup_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			group.SpecificModule = typeof(INSetup).Namespace;
			group.SpecificType = typeof(SegmentValue).FullName;
			int count = GroupHelper.Count;
			foreach (PX.SM.RelationGroup inserted in sender.Inserted)
			{
				if (inserted.GroupMask != null && inserted.GroupMask.Length > 0)
				{
					int cnt = 0;
					for (int i = 0; i < inserted.GroupMask.Length; i++)
					{
						if (inserted.GroupMask[i] == 0x00)
						{
							cnt += 8;
						}
						else
						{
							for (int j = 1; j <= 8; j++)
							{
								if ((inserted.GroupMask[i] >> j) == 0x00)
								{
									cnt += (9 - j);
									break;
								}
							}
							break;
						}
					}
					if (cnt > count)
					{
						count = cnt;
					}
				}
			}
			byte[] mask;
			if (count == 0)
			{
				mask = new byte[] { (byte)128, (byte)0, (byte)0, (byte)0 };
			}
			else
			{
				if (count == 0 || count % 32 != 0)
				{
					mask = new byte[((count + 31) / 32) * 4];
					mask[count / 8] = (byte)(128 >> (count % 8));
				}
				else
				{
					mask = new byte[((count + 31) / 32) * 4 + 4];
					mask[mask.Length - 4] = (byte)128;
				}
			}
			group.GroupMask = mask;
			if (GroupHelper.Count < mask.Length * 8)
			{
				if (!Views.Caches.Contains(typeof(PX.SM.Neighbour)))
				{
					Views.Caches.Add(typeof(PX.SM.Neighbour));
				}
				PXCache c = Caches[typeof(PX.SM.Neighbour)];
				foreach (PX.SM.Neighbour n in PXSelect<PX.SM.Neighbour>.Select(this))
				{
					byte[] ext = n.CoverageMask;
					Array.Resize<byte>(ref ext, mask.Length);
					n.CoverageMask = ext;
					ext = n.InverseMask;
					Array.Resize<byte>(ref ext, mask.Length);
					n.InverseMask = ext;
					ext = n.WinCoverageMask;
					Array.Resize<byte>(ref ext, mask.Length);
					n.WinCoverageMask = ext;
					ext = n.WinInverseMask;
					Array.Resize<byte>(ref ext, mask.Length);
					n.WinInverseMask = ext;
					c.Update(n);
				}
				c.IsDirty = false;
			}
		}
		protected virtual void RelationGroup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			if (group.SpecificModule != typeof(INSetup).Namespace || group.SpecificType != typeof(SegmentValue).FullName)
			{
				PX.SM.RelationGroup existing = PXSelectReadonly<PX.SM.RelationGroup,
					Where<PX.SM.RelationGroup.groupName, Equal<Required<PX.SM.RelationGroup.groupName>>>>
					.Select(this, group.GroupName);
				if (existing != null)
				{
					sender.RestoreCopy(group, existing);
				}
				group.SpecificModule = typeof(INSetup).Namespace;
				group.SpecificType = typeof(SegmentValue).FullName;
			}
		}
		protected virtual void RelationGroup_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)e.Row;
			group.SpecificModule = null;
			group.SpecificType = null;
			group.Active = false;
			if (sender.GetStatus(group) != PXEntryStatus.InsertedDeleted)
			{
				sender.SetStatus(group, PXEntryStatus.Updated);
			}
		}
		#endregion

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			if (cacheType == typeof(INUnit) && operation == PXDBOperation.Update)
			{
				base.Persist(cacheType, PXDBOperation.Insert);
			}
			return base.Persist(cacheType, operation);
		}

		public override void Persist()
		{
			if (setup.Current != null && string.IsNullOrEmpty(setup.Current.DfltLotSerClassID) && !IsFeatureInstalled<FeaturesSet.lotSerialTracking>())
			{
				setup.Current.DfltLotSerClassID = INLotSerClass.GetDefaultLotSerClass(this);
			}
			base.Persist();
		}

		public PXAction<INSetup> viewRestrictionGroup;
        [PXUIField(DisplayName = Messages.ViewRestrictionGroup, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable ViewRestrictionGroup(PXAdapter adapter)
        {
            if (Groups.Current != null)
            {
				RelationGroups graph = CreateInstance<RelationGroups>();
                graph.HeaderGroup.Current = graph.HeaderGroup.Search<RelationHeader.groupName>(Groups.Current.GroupName);
                throw new PXRedirectRequiredException(graph, false, Messages.ViewRestrictionGroup);
            }
            return adapter.Get();
        }

        public bool IsFeatureInstalled<TFeature>()
        {
            if (typeof(TFeature).Name.ToLower() == "warehouselocation")
                return false;
            return PXAccess.FeatureInstalled(typeof(TFeature).Name);
        }
    }
}
