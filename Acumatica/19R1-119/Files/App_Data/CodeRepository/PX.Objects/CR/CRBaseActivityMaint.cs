using System;
using System.Linq;
using System.Web.Compilation;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	public class CRBaseActivityMaint<TGraph, TMaster> : PXGraph<TGraph>, IActivityMaint 
		where TGraph : PXGraph
		where TMaster : CRActivity, new()
	{
		#region Selects
		[PXHidden]
		public PXSelect<BAccount> BaseBAccount;

		[PXHidden]
		public PXSelect<AP.Vendor> BaseVendor;

		[PXHidden]
		public PXSelect<AR.Customer> BaseCustomer;

		[PXHidden]
		public PXSelect<EPEmployee> BaseEmployee;
		
		[PXHidden]
		public PXSelect<EPView> EPViews;

		[PXHidden]
		public PXSelect<CRActivityStatistics> Stats;
		#endregion

		#region Ctor
		public CRBaseActivityMaint()
		{
			Views.Caches.Remove(typeof(CRActivityStatistics));
			Views.Caches.Add(typeof(CRActivityStatistics));
			
			this.FieldUpdated.AddHandler(typeof(TMaster), typeof(CRActivity.refNoteID).Name, TMasterFieldUpdated);
		}
		#endregion
		
		#region Actions
		public PXSave<TMaster> Save;
		public PXSaveClose<TMaster> SaveClose;
		public PXCancel<TMaster> Cancel;
		public PXInsert<TMaster> Insert;

		public PXAction<TMaster> GotoEntity;
		[PXUIField(DisplayName = Messages.ViewEntity, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.ttipViewEntity)]
		protected virtual void gotoEntity()
		{
			CRActivity row = (CRActivity)Caches[typeof(CRActivity)].Current;
			if (row == null) return;

			new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.NewWindow);
		}
		
		public PXAction<TMaster> GotoParentActivity;
		[PXUIField(DisplayName = "View Parent", MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.ttipViewParentActivity)]
		protected void gotoParentActivity()
		{
			CRActivity row = (CRActivity)Caches[typeof(CRActivity)].Current;
			if (row == null || row.ParentNoteID == null) return;

			CRActivity parentActivity = PXSelect<CRActivity, 
				Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>.Select(this, row.ParentNoteID);
			if (parentActivity != null && parentActivity.NoteID != null)
				new EntityHelper(this).NavigateToRow(parentActivity.NoteID, PXRedirectHelper.WindowMode.NewWindow);
		}
		#endregion

		#region IActivityMaint implementation
		public virtual void CancelRow(CRActivity row) {}

		public virtual void CompleteRow(CRActivity row) {}
		#endregion

		protected virtual void TMasterFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TMaster row = (TMaster) e.Row;
			if (row == null || row.ClassID == null) return;

			RelatedEntity pars = this.Caches<RelatedEntity>().Current as RelatedEntity;
			if (pars == null || pars.Type == null || pars.RefNoteID == null) return;            

            EntityHelper helper = new EntityHelper(this);
            Type entityType = PXBuildManager.GetType(pars.Type, false);
            var related = new EntityHelper(this).GetEntityRow(entityType, pars.RefNoteID);
            if (related == null) return;            

            Type graphType = helper.GetPrimaryGraphType(entityType, related, true);
			if (graphType == null) return;

			TMaster copy = PXCache<TMaster>.CreateCopy(row);
			PXGraph graph = CreateInstance(graphType);

			Type noteType = EntityHelper.GetNoteType(entityType);
			PXView view = new PXView(this, false, BqlCommand.CreateInstance(BqlCommand.Compose(typeof (Select<,>), entityType, typeof (Where<,>), noteType, typeof (Equal<>), typeof (Required<>), noteType)));
			graph.Caches[entityType].Current = view.SelectSingle(pars.RefNoteID);

			PXCache<TMaster> cache = graph.Caches<TMaster>();
			cache.SetDefaultExt(copy, typeof(CRActivity.bAccountID).Name);
			cache.SetDefaultExt(copy, typeof(CRActivity.contactID).Name);
            if(copy.BAccountID != null)
			    row.BAccountID = copy.BAccountID;
            if (copy.ContactID != null)
                row.ContactID = copy.ContactID;
		}

		protected virtual void MarkAs(PXCache cache, CRActivity row, Guid UserID, int status)
		{
			if (IsImport || row.NoteID == null) return;

			var noteidType = typeof(TMaster).GetNestedType(typeof(CRActivity.noteID).Name);
			var ownerType = typeof(TMaster).GetNestedType(typeof(CRActivity.ownerID).Name);
			
			var select = BqlCommand.Compose(
				typeof (Select<,>), typeof (EPView), typeof (Where<,,>),
				typeof (EPView.noteID), typeof (Equal<>), typeof (Required<>), noteidType,
				typeof (And<,>), typeof (EPView.userID), typeof (Equal<>), typeof (Required<>), ownerType
				);
			
			 EPView epview = (EPView)new PXView(this, false,
				BqlCommand.CreateInstance(select)).SelectSingle(row.NoteID, UserID);
			
			if (epview == null)
			{
				epview = new EPView
				{
					NoteID = row.NoteID,
					UserID = UserID,
				};
			}
			else
			{
				epview = PXCache<EPView>.CreateCopy(epview);
			}

			if (status == EPViewStatusAttribute.VIEWED
					? epview.Status != EPViewStatusAttribute.VIEWED
					: epview.Status == EPViewStatusAttribute.VIEWED)
			{
				epview.Status = status;
				EPViews.Update(epview);

				bool isDirty = false;
				foreach (Type t in Views.Caches.Where(t => t != typeof(EPView)).ToList())
				{
					PXCache c = Caches[t];
					isDirty = c.Inserted.ToArray<object>().Any() || c.Updated.ToArray<object>().Any() || c.Deleted.ToArray<object>().Any();
					if (isDirty) break;
				}
				if (!isDirty)
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						Persist();
						ts.Complete();
					}
				}
			    EPViews.Cache.IsDirty = false;

			}
		}
		
	}
}
