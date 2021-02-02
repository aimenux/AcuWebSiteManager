using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.MassProcess;
using PX.Objects.Common;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Api;
using FieldValue = PX.Data.MassProcess.FieldValue;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Common.Mail;
using System.Reflection;

namespace PX.Objects.CR.Extensions.Relational
{
	/// <summary>
	/// Extension that is used for linking the single shared <see cref="Child"/> to two same-level entities: <see cref="Document"/> and <see cref="Related"/>.
	/// Inserts <see cref="Child"/> on <see cref="Document"/> inserting.
	/// Deletes <see cref="Child"/> on <see cref="Document"/> deleting (if it wasn't shred with the <see cref="Related"/>).
	/// </summary>
	public abstract class SharedChildOverrideGraphExt<TGraph, TThis> : CRParentChild<TGraph, TThis>
		where TGraph : PXGraph
		where TThis : SharedChildOverrideGraphExt<TGraph, TThis>
	{
		#region DACs

		[PXHidden]
		public class Related : Related<TThis> { }

		#endregion

		#region Views

		public PXSelectExtension<Related> RelatedDocument;

		#endregion

		#region ctor

		// the Enable state should be set inside the view delegate then
		public virtual bool ViewHasADelegate { get; set; }

		public override void Initialize()
		{
			base.Initialize();

			// unlink two views (to not to currupt by the next statement)
			RelatedDocument.View = new PXView(Base, false, BqlCommand.CreateInstance(typeof(Select<>), RelatedDocument.View.GetItemType()));

			// convert Child.childID into CacheExt.DAC.field
			var originalField = RelatedDocument.Cache.GetType()
					.GetMethod("GetBaseBqlField", BindingFlags.Instance | BindingFlags.NonPublic)
					?.Invoke(RelatedDocument.Cache, new object[] { nameof(Related.relatedID) })
				as Type;

			if (originalField == null)
				return;

			RelatedDocument.View.WhereNew(BqlCommand.Compose(typeof(Where<,>),
				originalField,
				typeof(Equal<>), typeof(Required<>), originalField));
		}

		#endregion

		#region Implementation

		public virtual Related GetRelatedByID(int? relatedID)
		{
			if (relatedID == null)
				return null;

			if (RelatedDocument.Current != null && RelatedDocument.Cache.GetValue(RelatedDocument.Current, nameof(RelatedDocument.Current.RelatedID)) as int? == relatedID)
			{
				return RelatedDocument.Current;
			}

			return RelatedDocument.SelectSingle(relatedID);
		}

		#endregion

		#region Events

		protected virtual void _(Events.RowSelected<Document> e)
		{
			var row = e.Row as Document;
			if (row == null)
				return;

			var isOverrideRelated = true;

			var related = GetRelatedByID(row.RelatedID);

			if ((row.ChildID ?? e.Cache.GetValue(row, nameof(row.ChildID)) as int?) == related?.ChildID)
			{
				isOverrideRelated = false;
			}

			// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [only for UI]
			PrimaryDocument.Cache.SetValue(row, nameof(row.IsOverrideRelated), isOverrideRelated);

			if (!ViewHasADelegate)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, nameof(row.IsOverrideRelated), related?.ChildID != null);
			}
		}

		protected virtual void _(Events.RowSelected<Child> e)
		{
			var row = e.Row as Child;
			if (row == null)
				return;

			var isOverrideRelated = true;

			var related = GetRelatedByID(row.RelatedID);

			if (row.ChildID == related?.ChildID)
			{
				isOverrideRelated = false;
			}

			if (!ViewHasADelegate)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, row, isOverrideRelated);
			}
		}

		protected virtual void _(Events.RowDeleted<Document> e)
		{
			var row = e.Row as Document;
			if (row == null)
				return;

			if (row.IsOverrideRelated == false)
				return;

			var child = GetChildByID(row.ChildID);

			if (child != null)
			{
				ChildDocument.Delete(child);
			}
		}

		protected virtual void _(Events.RowInserting<Document> e)
		{
			if (!this.GetDocumentMapping().Table.IsAssignableFrom(this.GetRelatedMapping().Table))
				ProcessInsert(e.Row);
		}

		protected virtual void _(Events.RowInserted<Document> e)
		{
			if (this.GetDocumentMapping().Table.IsAssignableFrom(this.GetRelatedMapping().Table))
				ProcessInsert(e.Row);
		}

		protected virtual void ProcessInsert(Document row)
		{
			if (row == null)
				return;

			var isOverrideRelated = row.IsOverrideRelated;

			var related = GetRelatedByID(row.RelatedID);
			var relatedChild = GetChildByID(related?.ChildID);

			if (relatedChild == null)
			{
				// no Child to link to as smth "wrong" with Related - leave self-sufficient and create a Child

				isOverrideRelated = true;
			}
			else if (isOverrideRelated != true)
			{
				// should share the same Child as we didn't set Override manually to true

				if (row.ChildID != null && row.ChildID != related.ChildID)
				{
					// if the Document already has the Child, and it differes from the Child of the Related entity - Delete this Child

					var child = GetChildByID(row.ChildID);

					using (new ReadOnlyScope(ChildDocument.View.Cache))
					{
						ChildDocument.Delete(child);
					}
				}

				PrimaryDocument.Cache.SetValue(row, nameof(row.ChildID), related.ChildID);
			}

			if (isOverrideRelated == true)
			{
				// if the Child is self-sufficient

				Child child = GetChildByID(row.ChildID);

				if (child == null)
				{
					// if there's no preinserted Child in Cache -> Insert new Child

					if (relatedChild != null)
					{
						// if there's something that can actually be overridden

						child = (Child)ChildDocument.Cache.CreateCopy(relatedChild);
					}
					else
					{
						// or just create

						child = (Child)ChildDocument.Cache.CreateInstance();
					}

					child.ChildID = null;
					child.RelatedID = row.RelatedID;

					using (new ReadOnlyScope(ChildDocument.View.Cache))
					{
						child = ChildDocument.Insert(child);
					}

					PrimaryDocument.Cache.SetValue(row, nameof(row.ChildID), child.ChildID);
				}
				else
				{
					// there's a preinserted Child -> need to set links correctly (just in case)

					child.RelatedID = row.RelatedID;

					using (new ReadOnlyScope(ChildDocument.View.Cache))
					{
						child = ChildDocument.Update(child);
					}
				}

				if (related != null && relatedChild == null)
				{
					// update the Related since it doesn't contain a proper Child in it.
					// now both Document and Related share the same Child

					// Acuminator disable once PX1048 RowChangesInEventHandlersAllowedForArgsOnly [Justification]
					RelatedDocument.Cache.SetValue(related, nameof(related.ChildID), child.ChildID);
				}
			}
		}


		protected virtual void _(Events.RowUpdating<Document> e)
		{
			var row = e.NewRow as Document;
			var oldRow = e.Row as Document;
			if (row == null || oldRow == null)
				return;

			// the entire logic if about these fields -> skip
			if (row.RelatedID == null && oldRow.RelatedID == null)
				return;

			// nothing to do if there's no changes in significant fields
			if (row.RelatedID == oldRow.RelatedID
				&& row.ChildID == oldRow.ChildID
				&& row.IsOverrideRelated == oldRow.IsOverrideRelated)
				return;

			if (row.RelatedID != oldRow.RelatedID)
			{
				// link to Related has been changed

				var related = GetRelatedByID(row.RelatedID);
				var oldRelated = GetRelatedByID(oldRow.RelatedID);

				Child child;

				if (row.RelatedID == null)
				{
					// Related is cleared - need to store local copy of old Child

					var relatedChild = GetChildByID(oldRelated.ChildID);

					if (relatedChild != null && oldRelated.ChildID == row.ChildID)
					{
						// was linked to Related and Override was False 

						child = (Child)ChildDocument.Cache.CreateCopy(relatedChild);

						child.ChildID = null;
						child.RelatedID = row.RelatedID;

						using (new ReadOnlyScope(ChildDocument.View.Cache))
						{
							child = ChildDocument.Insert(child);
						}

						PrimaryDocument.Cache.SetValue(row, nameof(row.ChildID), child.ChildID);
					}
				}

				if (related != null && row.IsOverrideRelated == false)
				{
					// there's a new Related and Override = False - should share the same Child with new Related

					var relatedChild = GetChildByID(related.ChildID);

					PrimaryDocument.Cache.SetValue(row, nameof(row.ChildID), relatedChild.ChildID);
				}
				else
				{
					// no Related or overridden - do nothing. Old Child should stay where it is.
				}

				child = GetChildByID(row.ChildID);

				child.RelatedID = row.RelatedID;

				using (new ReadOnlyScope(ChildDocument.View.Cache))
				{
					ChildDocument.Update(child);
				}
			}


			if (row.IsOverrideRelated != oldRow.IsOverrideRelated)
			{
				// Override has been changed

				if (row.IsOverrideRelated == true)
				{
					// Override = True now - clear the link to Child, it will be handled below

					PrimaryDocument.Cache.SetValue(row, nameof(row.ChildID), null);
				}
				else if (row.IsOverrideRelated == false)
				{
					// Override = False now - delete a self-sufficient Child and link to shared Child
					// deletion will be handled below

					var related = GetRelatedByID(row.RelatedID);
					var relatedChild = GetChildByID(related?.ChildID);

					if (relatedChild != null)
					{
						// link to the shared Child
						PrimaryDocument.Cache.SetValue(row, nameof(row.ChildID), relatedChild.ChildID);

						// increment the RevisionID
						ChildDocument.Cache.MarkUpdated(relatedChild);
					}
				}
			}


			if (row.ChildID != oldRow.ChildID)
			{
				// link to Child has been changed

				if (row.ChildID == null)
				{
					// Child was cleared - need to insert new one with Override = True

					var related = GetRelatedByID(row.RelatedID);
					var relatedChild = GetChildByID(related.ChildID);

					Child child;
					if (relatedChild != null && related.ChildID == oldRow.ChildID)
					{
						// was linked to Related and Override was False 

						child = (Child)ChildDocument.Cache.CreateCopy(relatedChild);
					}
					else
					{
						child = (Child)ChildDocument.Cache.CreateInstance();
					}

					child.ChildID = null;
					child.RelatedID = row.RelatedID;

					using (new ReadOnlyScope(ChildDocument.View.Cache))
					{
						child = ChildDocument.Insert(child);
					}

					PrimaryDocument.Cache.SetValue(row, nameof(row.ChildID), child.ChildID);
				}

				// check if we can delete the old Child (that is without links)

				var oldChild = GetChildByID(oldRow.ChildID);

				if (oldChild != null)
				{
					var parentOfOldChild = GetRelatedByID(oldChild.RelatedID);

					if (parentOfOldChild == null || parentOfOldChild.ChildID != oldRow.ChildID)
					{
						// Related is already referencing another Child
						// Child is alone - delete it

						using (new ReadOnlyScope(ChildDocument.View.Cache))
						{
							ChildDocument.Delete(oldChild);
						}
					}
				}
			}
		}

		#endregion

		#region Mappings

		#region Related Mapping
		protected class RelatedMapping : IBqlMapping
		{
			public Type Extension => typeof(Related);
			protected Type _table;
			public Type Table => _table;

			public RelatedMapping(Type table)
			{
				_table = table;
			}
			// Types are used as nameof, so .A is ok here
			public Type RelatedID = typeof(Related.relatedID);
			public Type ChildID = typeof(Related.childID);
		}
		protected abstract RelatedMapping GetRelatedMapping();
		#endregion

		#endregion
	}
}
