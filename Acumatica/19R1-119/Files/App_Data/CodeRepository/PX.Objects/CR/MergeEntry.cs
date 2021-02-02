using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using PX.Common;
using PX.Data;

namespace PX.Objects.CR
{
	[Obsolete("Will be removed in 7.0 version")]
	public class MergeEntry : PXGraph<MergeEntry>
	{
		#region DacBase DAC

		[Serializable]
		[PXVirtual]
		[PXHidden]
		[Obsolete("Will be removed in 7.0 version")]
		public partial class DacBase : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

			[PXBool]
			[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected { get; set; }
			#endregion

			#region RowId

			public abstract class rowId : PX.Data.BQL.BqlInt.Field<rowId> { }

			[PXInt]
			[PXUIField(Visible = false)]
			public virtual Int32? RowId { get; set; }

			#endregion
		}

		#endregion

		#region PropertySelection DAC

		[Serializable]
		[PXVirtual]		
		[PXHidden]
		public partial class PropertySelection : IBqlTable
		{
			#region MergeID

			public abstract class mergeID : PX.Data.BQL.BqlInt.Field<mergeID> { }

			[PXInt(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual Int32? MergeID { get; set; }

			#endregion

			#region LineNbr

			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

			[PXInt(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual Int32? LineNbr { get; set; }

			#endregion

			#region PropetyName

			public abstract class propetyName : PX.Data.BQL.BqlString.Field<propetyName> { }

			[PXString(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual String PropetyName { get; set; }

			#endregion

			#region RowId

			public abstract class rowId : PX.Data.BQL.BqlInt.Field<rowId> { }

			[PXInt]
			[PXUIField(Visible = false)]
			public virtual Int32? RowId { get; set; }

			#endregion
		}

		#endregion

		#region RecordExclusion DAC

		[Serializable]
		[PXVirtual]
		[PXHidden]
		public partial class RecordExclusion : IBqlTable
		{
			#region MergeID

			public abstract class mergeID : PX.Data.BQL.BqlInt.Field<mergeID> { }

			[PXInt(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual Int32? MergeID { get; set; }

			#endregion

			#region LineNbr

			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

			[PXInt(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual Int32? LineNbr { get; set; }

			#endregion

			#region RowId

			public abstract class rowId : PX.Data.BQL.BqlInt.Field<rowId> { }

			[PXInt(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual Int32? RowId { get; set; }

			#endregion
		}

		#endregion

		#region Selects

		[PXViewName(Messages.Document)] 
		public PXSelect<CRMerge>
			Document;

		[PXViewName(Messages.Groups)] 
		public PXSelect<CRMergeResult,
			Where<CRMergeResult.mergeID, Equal<Optional<CRMerge.mergeID>>>>
			Groups;

		[PXHidden]
		public PXSelect<PropertySelection,
			Where<PropertySelection.mergeID, Equal<Optional<CRMergeResult.mergeID>>, 
				And<PropertySelection.lineNbr, Equal<Optional<CRMergeResult.lineNbr>>>>>
			PropertySettings;

		[PXHidden]
		public PXSelect<RecordExclusion,
			Where<RecordExclusion.mergeID, Equal<Optional<CRMergeResult.mergeID>>,
				And<RecordExclusion.lineNbr, Equal<Optional<CRMergeResult.lineNbr>>>>>
			ItemsExclusions;

		[PXViewName(Messages.Items)] 
		public PXSelect<DacBase>
			Items;

		#endregion

		#region Ctors

		public MergeEntry()
		{
			Groups.Cache.AllowInsert = false;
			Groups.Cache.AllowDelete = false;

			Items.Cache.AllowInsert = false;
			Items.Cache.AllowDelete = false;
		}

		#endregion

		#region Data Handlers

		protected virtual IEnumerable groups()
		{
			foreach (CRMergeResult group in this.QuickSelect(Groups.View.BqlSelect))
			{
				var extCount = GetItemsByGroup(@group).Count();
				if (extCount > 0)
				{
					group.EstCount = (int)extCount;
					yield return group;
				}
			}
		}

		protected virtual IEnumerable items()
		{
			return GetItemsByGroup(Groups.Current);
		}

		protected virtual IEnumerable itemsExclusions()
		{
			yield break;
		}

		#endregion

		#region Actions

		public PXCancel<CRMerge> Cancel;

		public PXFirst<CRMerge> First;

		public PXPrevious<CRMerge> Previous;

		public PXNext<CRMerge> Next;

		public PXLast<CRMerge> Last;

		public PXAction<CRMerge> Prepare;

		[PXProcessButton]
		[PXUIField(DisplayName = Messages.Prepare)]
		protected virtual IEnumerable prepare(PXAdapter adapter)
		{
			throw new NotImplementedException();
			//return adapter.Get();
		}

		public PXAction<CRMerge> Process;

		[PXUIField(DisplayName = Messages.Process)]
		[PXProcessButton]
		protected virtual IEnumerable process(PXAdapter adapter)
		{
			throw new NotImplementedException();
			//return adapter.Get();
		}

		#endregion

		#region Event Handlers

		public virtual void CRMerge_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRMerge;
			if (row == null) return;

			GenerateDynamicColumns(row);
		}

		public virtual void CRMergeResult_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRMergeResult;
			if (row == null) return;

		}

		#endregion

		#region Private Methods

		private void GenerateDynamicColumns(CRMerge row)
		{
			var dacInfo = MergeMaint.ReadProperties(row);
			if (dacInfo == null) return;

			foreach (MergeMaint.FieldInfo field in dacInfo)
			{
				var fieldName = field.Name;
				if (typeof(DacBase.selected).Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase) ||
					Items.Cache.Fields.Contains(fieldName))
				{
					continue;
				}

				var desciptionFieldName = fieldName + "_description";
				Items.Cache.Fields.Add(fieldName);
				Items.Cache.Fields.Add(desciptionFieldName);

				var displayName = field.Label;
				FieldSelecting.AddHandler(typeof(DacBase), fieldName,
					delegate(PXCache sender, PXFieldSelectingEventArgs e)
					{
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), null, null, null, null, null, null,
							fieldName, desciptionFieldName, displayName, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Dynamic, null, null, null);
					});
				FieldSelecting.AddHandler(typeof(DacBase), desciptionFieldName,
					delegate(PXCache sender, PXFieldSelectingEventArgs e)
					{
						//TODO: need implementation
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), null, null, null, null, null, null,
							desciptionFieldName, null, displayName, null, PXErrorLevel.Undefined, null, false, null, PXUIVisibility.Invisible, null, null, null);
						e.ReturnValue = "test value";
						/*SYData selRow = ((SYData)e.Row);
						if (selRow == null || selRow.LineNbr == null)
							return;
						if (!this.splittedFieldValues.ContainsKey(selRow.LineNbr))
							this.splittedFieldValues.Add(selRow.LineNbr, selRow.FieldValues.Split('\x0'));
						int index = Convert.ToInt32(currfname.Split('_')[1]);
						if (index < splittedFieldValues[selRow.LineNbr].Length)
							e.ReturnValue = this.splittedFieldValues[selRow.LineNbr][index];*/
					});

				//TODO: need implementation
				/*this.FieldUpdating.AddHandler("PreparedData", currfname,
					delegate(PXCache sender, PXFieldUpdatingEventArgs updE)
					{
						this.dynamicFieldValues[currfname] = updE.NewValue as string;
					});*/
			}
		}

		private IEnumerable GetItemsByGroup(CRMergeResult group)
		{
			if (group == null) yield break;

			var exclusionMap = GetExclusionMap(group);
			//TODO: need implementation
			foreach (Contact row in PXSelect<Contact>.Select(this))
			{
				var key = row.ContactID;
				yield return new DacBase
					{
						Selected = !exclusionMap.Contains(key),
						RowId = key
					};
			}
		}

		private HybridDictionary GetExclusionMap(CRMergeResult group)
		{
			var res = new HybridDictionary();
			foreach (RecordExclusion row in ItemsExclusions.Select(group.MergeID, group.LineNbr))
				res.Add(row.RowId, row);
			return res;
		}

		private HybridDictionary GetProptertyMap(CRMergeResult group)
		{
			var res = new HybridDictionary();
			foreach (PropertySelection row in PropertySettings.Select(group.MergeID, group.LineNbr))
			{
				res.Add(row.PropetyName, row);
			}
			return res;
		}

		#endregion
	}
}
