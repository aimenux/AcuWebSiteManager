using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR.Extensions
{
	/// <exclude/>
	public class CRDefaultDocumentOwner<TGraph, TMaster, FClassID, FOwnerID, FWorkgroupID>
		: PXGraphExtension<TGraph>
			where TGraph : PXGraph
			where TMaster : class, IAssign, IBqlTable, new()
			where FClassID : class, IBqlField
			where FOwnerID : class, IBqlField
			where FWorkgroupID : class, IBqlField
	{
		protected virtual void _(Events.FieldUpdated<FClassID> e)
		{
			if (e.Row != null
				&& e.NewValue != e.OldValue
				&& !Base.IsCopyPasteContext
				&& !Base.IsImport
				&& e.Cache.RaiseFieldDefaulting<FOwnerID>(e.Row, out var newValue))
			{
				e.Cache.SetValue<FOwnerID>(e.Row, newValue);
				e.Cache.SetValue<FWorkgroupID>(e.Row, null);
			}
		}

		protected virtual void _(Events.RowPersisting<TMaster> e)
		{
			if (e.Row == null
				|| Object.Equals(e.Cache.GetValue<FClassID>(e.Row), e.Cache.GetValueOriginal<FClassID>(e.Row)))
				return;

			PXView view = null;
			foreach (PXSelectorAttribute attribute in e.Cache.GetAttributesOfType<PXSelectorAttribute>(e.Row, typeof(FClassID).Name))
			{
				view = new PXView(Base, true, attribute.PrimarySelect);
			}
			if (view == null) return;

			string classID = e.Cache.GetValue<FClassID>(e.Row) as string;
			CRBaseClass cls = view.SelectSingle(classID) as CRBaseClass;
			if (cls == null) return;

			switch (cls.DefaultOwner)
			{
				case CRDefaultOwnerAttribute.AssignmentMap:

					var copy = e.Cache.CreateCopy(e.Row) as TMaster;

					// Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [graph is needed here as a service]
					PXGraph.CreateInstance<EPAssignmentProcessor<TMaster>>().Assign(copy, cls.DefaultAssignmentMapID);

					e.Cache.SetValueExt<FOwnerID>(e.Row, copy.OwnerID);
					e.Cache.SetValueExt<FWorkgroupID>(e.Row, copy.WorkgroupID);

					break;
			}
		}

		protected virtual void _(Events.FieldDefaulting<FOwnerID> e)
		{
			if (e.Row == null) return;

			PXView view = null;
			foreach (PXSelectorAttribute attribute in e.Cache.GetAttributesOfType<PXSelectorAttribute>(e.Row, typeof(FClassID).Name))
			{
				view = new PXView(Base, true, attribute.PrimarySelect);
			}
			if (view == null) return;

			string classID = e.Cache.GetValue<FClassID>(e.Row) as string;
			CRBaseClass cls = view.SelectSingle(classID) as CRBaseClass;
			if (cls == null) return;

			switch (cls.DefaultOwner)
			{
				case CRDefaultOwnerAttribute.Creator:

					// if user is not employee it will just clear the field
					e.NewValue = SelectFrom<Contact>
								.InnerJoin<BAccountR>
									.On<BAccountR.defContactID.IsEqual<Contact.contactID>
									.And<BAccountR.parentBAccountID.IsEqual<Contact.bAccountID>>>
								.Where<
									Contact.contactID.IsEqual<AccessInfo.contactID.FromCurrent>>
								.View.ReadOnly
								.Select(Base)
								.FirstOrDefault()
								?.GetItem<Contact>()
								?.ContactID;
					break;

				default:

					e.NewValue = null;
					break;
			}
		}

		protected virtual void _(Events.FieldSelecting<FOwnerID> e)
		{
			FieldSelectingOwnerOrWorkgroup(e);
		}

		protected virtual void _(Events.FieldSelecting<FWorkgroupID> e)
		{
			FieldSelectingOwnerOrWorkgroup(e);
		}

		private void FieldSelectingOwnerOrWorkgroup<TField>(Events.FieldSelecting<TField> e) where TField : class, IBqlField
		{
			if (e.Row == null) return;

			bool isEnabled = true;

			PXView view = null;
			foreach (PXSelectorAttribute attribute in e.Cache.GetAttributesOfType<PXSelectorAttribute>(e.Row, typeof(FClassID).Name))
			{
				view = new PXView(Base, true, attribute.PrimarySelect);
			}
			if (view == null) return;

			string classID = e.Cache.GetValue<FClassID>(e.Row) as string;
			CRBaseClass cls = view.SelectSingle(classID) as CRBaseClass;
			if (cls == null) return;

			isEnabled = cls.DefaultOwner != CRDefaultOwnerAttribute.AssignmentMap
						|| Object.Equals(e.Cache.GetValue<FClassID>(e.Row), e.Cache.GetValueOriginal<FClassID>(e.Row));

			e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, null, null, null, null, null, null, null, PXErrorLevel.Undefined,
				isEnabled,
				null, null, PXUIVisibility.Undefined, null, null, null);
		}
	}
}
