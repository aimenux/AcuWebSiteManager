using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRDuplicateEntities
{
	/// <exclude/>
	public abstract class CRDuplicateContactEntities<TGraph, TMain> : CRDuplicateEntities<TGraph, TMain>
		where TGraph : PXGraph, new()
		where TMain : Contact, IBqlTable, new()
	{
		protected virtual void _(Events.FieldDefaulting<MergeParams, MergeParams.targetEntityID> e)
		{
			TMain current = (TMain)Base.Caches<TMain>().Current;

			List<TMain> duplicates = PXSelectorAttribute.SelectAll<MergeParams.targetEntityID>(e.Cache, e.Row)
				.RowCast<TMain>()
				.ToList();

			e.NewValue =
				current.IsActive == true || duplicates.Count == 0
					? current.ContactID
					: duplicates
						.OrderBy(duplicate => duplicate?.ContactID)
						.FirstOrDefault()
						?.ContactID;
		}

		[PXOverride]
		public virtual void Persist(Action del)
		{
			del();

			DuplicateDocument duplicateDocument = DuplicateDocuments.Current;

			if (Setup.Current?.ValidateContactDuplicatesOnEntry == true && duplicateDocument?.IsActive == true)
			{
				CheckForDuplicates.Press();
			}
		}
	}
}
