using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Compilation;
using PX.Common;
using PX.Data;

namespace PX.Objects.CR
{
	[Obsolete("Will be removed in 7.0 version")]
	public class MergeMaint : PXGraph<MergeMaint, CRMerge>
	{
		#region FieldInfo

		internal class FieldInfo
		{
			private readonly string _name;
			private readonly string _label;
			private readonly Type _type;
			private readonly string _view;

			public FieldInfo(string name, string label, Type type, string view)
			{
				if (string.IsNullOrEmpty(name))
					throw new ArgumentNullException("name");
				if (type == null) throw new ArgumentNullException("type");
				
				_name = name;
				_label = string.IsNullOrEmpty(label) ? name : label;
				_type = type;
				_view = view;
			}

			public string Name
			{
				get { return _name; }
			}

			public string Label
			{
				get { return _label; }
			}

			public Type Type
			{
				get { return _type; }
			}

			public string ViewName
			{
				get { return _view; }
			}
		}

		#endregion

		#region DacInfo

		internal class DacInfo : IEnumerable<FieldInfo>
		{
			private readonly string _name;
			private readonly HybridDictionary _map;

			public readonly static DacInfo Empty = new DacInfo();

			private string[] _fieldNames;
			private string[] _fieldLabels;

			private DacInfo() { }

			private DacInfo(string name)
			{
				if (name == null) throw new ArgumentNullException("name");

				_name = name;
				_map = new HybridDictionary();
			}

			public string Name
			{
				get { return _name; }
			}

			public FieldInfo this[string name]
			{
				get
				{
					if (string.IsNullOrEmpty(name)) return null;
					return _map[name] as FieldInfo;
				}
			}

			public static DacInfo Create(string name, params PXFieldState[] fieldStates)
			{
				if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

				var properties = new List<FieldInfo>();
				if (fieldStates != null)
					foreach (var state in fieldStates)
						if (state.Visibility != PXUIVisibility.Invisible && !string.IsNullOrEmpty(state.DisplayName))
							properties.Add(new FieldInfo(state.Name, state.DisplayName, state.DataType, state.ViewName));
				properties.Sort((info, fieldInfo) => StringComparer.OrdinalIgnoreCase.Compare(info.Name, fieldInfo.Name));

				var res = new DacInfo(name);
				foreach (FieldInfo field in properties)
					res._map.Add(field.Name, field);

				return res;
			}

			public string[] FieldNames
			{
				get
				{
					EnshureCaches();
					return _fieldNames;
				}
			}

			private void EnshureCaches()
			{
				if (_fieldNames == null || _fieldLabels == null)
				{
					var labels = new List<string>();
					var values = new List<string>();

					foreach (FieldInfo field in this)
					{
						labels.Add(field.Label);
						values.Add(field.Name);
					}

					_fieldNames = values.ToArray();
					_fieldLabels = labels.ToArray();
				}
			}

			public string[] FieldLabels
			{
				get
				{
					EnshureCaches();
					return _fieldLabels;
				}
			}

			public IEnumerator<FieldInfo> GetEnumerator()
			{
				return IterateMap().GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			private IEnumerable<FieldInfo> IterateMap()
			{
				foreach (FieldInfo item in _map.Values)
					yield return item;
			}
		}

		#endregion

		#region Fields

		private readonly static HybridDictionary _dacMap;

		#endregion

		#region Selects

		[PXViewName(Messages.Document)]
		public PXSelect<CRMerge>
			Document;

		[PXViewName(Messages.Criteria)]
		public PXSelect<CRMergeCriteria,
			Where<CRMergeCriteria.mergeID, Equal<Current<CRMerge.mergeID>>>>
			Criteria;

		[PXViewName(Messages.Methods)]
		public PXSelect<CRMergeMethod,
			Where<CRMergeMethod.mergeID, Equal<Current<CRMergeMethod.mergeID>>>>
			Methods;

		#endregion

		#region Ctors

		static MergeMaint()
		{
			_dacMap = new HybridDictionary();
		}

		#endregion

		#region Event Handlers

		[PXDefault]
		[PXDBString(50)]
		[PXStringList]
		[PXUIField(DisplayName = "Property")]
		protected virtual void CRMergeCriteria_DataField_CacheAttached(PXCache sender)
		{
			
		}


		[PXDefault]
		[PXDBString(50, IsKey = true)]
		[PXStringList]
		[PXUIField(DisplayName = "Property")]
		protected virtual void CRMergeMethod_DataField_CacheAttached(PXCache sender)
		{

		}

		protected virtual void CRMerge_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRMerge;
			if (row == null) return;

			SetDataFieldLists(row);
			CheckEditRights(row);
		}

		protected virtual void CRMerge_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CRMerge;
			if (row == null) return;

			if (row.EntityType != null)
				InsertMethods(row);
		}

		protected virtual void CRMerge_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRMerge;
			var oldRow = e.OldRow as CRMerge;
			if (row == null || oldRow == null) return;

			if (row.EntityType != oldRow.EntityType)
			{
				RemoveAllCriteria(row);
				RemoveAllMethods(row);
				InsertMethods(row);
			}
		}

		protected virtual void CRMergeCriteria_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRMergeCriteria;
			if (row == null) return;

			SetConditionList(Document.Current, row);
			CorrectCriteriaValue(Document.Current, row);
		}

		protected virtual void CRMergeCriteria_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRMergeCriteria;
			var oldRow = e.OldRow as CRMergeCriteria;
			if (row == null || oldRow == null) return;

			if (row.DataField != oldRow.DataField)
			{
				object defaultMatching;
				sender.RaiseFieldDefaulting<CRMergeCriteria.matching>(row, out defaultMatching);
				row.Matching = (int?)defaultMatching;
			}

			if (row.DataField != oldRow.DataField || row.Matching != oldRow.Matching)
			{
				row.Value = null;
			}
		}

		protected virtual void CRMergeMethod_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRMergeMethod;
			if (row == null) return;

			SetMethodList(Document.Current, row);
		}

		protected virtual void CRMergeMethod_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRMergeMethod;
			var oldRow = e.OldRow as CRMergeMethod;
			if (row == null || oldRow == null) return;

			if (row.DataField != oldRow.DataField)
			{
				object defaultMethod;
				sender.RaiseFieldDefaulting<CRMergeMethod.method>(row, out defaultMethod);
				row.Method = (int?)defaultMethod;
			}
		}

		#region Protected Method

		protected void CorrectCriteriaValue(CRMerge document, CRMergeCriteria row)
		{
			var fieldInfo = ReadProperties(document).With(_ => _[row.DataField]);
			if (fieldInfo == null || 
				row.Matching == MergeMatchingTypesAttribute._THE_SAME)
			{
				PXUIFieldAttribute.SetEnabled<CRMergeCriteria.value>(Criteria.Cache, row, false);
			}
			else
				PXUIFieldAttribute.SetEnabled<CRMergeCriteria.value>(Criteria.Cache, row, true);
		}

		protected void SetConditionList(CRMerge document, CRMergeCriteria row)
		{
			var values = MergeMatchingTypesAttribute.CommonValues;
			var labels = MergeMatchingTypesAttribute.CommonLabels;
			var fieldInfo = ReadProperties(document).With(_ => _[row.DataField]);
			if (fieldInfo != null && IsNumericField(fieldInfo))
			{
				values = MergeMatchingTypesAttribute.ComparableValues;
				labels = MergeMatchingTypesAttribute.ComparableLabels;
			}
			PXIntListAttribute.SetList<CRMergeCriteria.matching>(Criteria.Cache, row, values, labels);
			if (row.Matching != null && Array.IndexOf(values, (int)row.Matching) < 0)
				PXUIFieldAttribute.SetWarning<CRMergeCriteria.matching>(Criteria.Cache, row, Messages.IncorrectMatching);
		}

		protected void SetMethodList(CRMerge document, CRMergeMethod row)
		{
			var fieldInfo = ReadProperties(document).With(_ => _[row.DataField]);
			if (fieldInfo != null && string.IsNullOrEmpty(fieldInfo.ViewName))
			{
				var code = Type.GetTypeCode(fieldInfo.Type);
				switch (code)
				{
					case TypeCode.SByte:
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
					case TypeCode.Single:
					case TypeCode.Double:
					case TypeCode.Decimal:
						MergeMethodsAttribute.SetNumberList<CRMergeMethod.method>(Methods.Cache, row);
						break;
					case TypeCode.DateTime:
						MergeMethodsAttribute.SetDateList<CRMergeMethod.method>(Methods.Cache, row);
						break;
					case TypeCode.Char:
					case TypeCode.String:
						MergeMethodsAttribute.SetStringList<CRMergeMethod.method>(Methods.Cache, row);
						break;
					default:
						MergeMethodsAttribute.SetCommonList<CRMergeMethod.method>(Methods.Cache, row);
						break;
				}
			}
			else
			{
				MergeMethodsAttribute.SetCommonList<CRMergeMethod.method>(Methods.Cache, row);
			}
		}

		protected void SetDataFieldLists(CRMerge document)
		{
			var dacInfo = ReadProperties(document);
			if (dacInfo != null)
			{
				PXStringListAttribute.SetList<CRMergeCriteria.dataField>(Criteria.Cache, null, 
					dacInfo.FieldNames, dacInfo.FieldLabels);
				PXStringListAttribute.SetList<CRMergeMethod.dataField>(Methods.Cache, null, 
					dacInfo.FieldNames, dacInfo.FieldLabels);
			}
		}

		protected void CheckEditRights(CRMerge document)
		{
			var isEntitySpecified = !string.IsNullOrEmpty(document.EntityType);

			Criteria.Cache.AllowDelete =
				Criteria.Cache.AllowInsert =
				Criteria.Cache.AllowUpdate = isEntitySpecified;

			Methods.Cache.AllowDelete =
				Methods.Cache.AllowInsert = false;
			Methods.Cache.AllowUpdate = isEntitySpecified;
		}

		#endregion

		#endregion

		#region Private Methods

		private bool IsNumericField(FieldInfo field)
		{
			if (string.IsNullOrEmpty(field.ViewName))
			{
				var code = Type.GetTypeCode(field.Type);
				switch (code)
				{
					case TypeCode.SByte:
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
					case TypeCode.Single:
					case TypeCode.Double:
					case TypeCode.Decimal:
					case TypeCode.DateTime:
						return true;
				}
			}
			return false;
		}

		internal static DacInfo ReadProperties(CRMerge document)
		{
			var dac = document.With(_ => _.EntityType);
			return ReadProperties(dac);
		}

		private static DacInfo ReadProperties(string dacName)
		{
			if (string.IsNullOrEmpty(dacName)) return null;

			lock (_dacMap.SyncRoot)
			{
				var res = _dacMap[dacName] as DacInfo;
				if (res == null)
				{
					PXFieldState[] fieldStates = null;
					var dac = PXBuildManager.GetType(dacName, false);
					if (dac != null)
					{
						var graph = new PXGraph();
						fieldStates = PXFieldState.GetFields(graph, new Type[] {dac}, false);
					}
					res = DacInfo.Create(dacName, fieldStates);
					_dacMap.Add(dacName, res);
				}
				return res;
			}
		}

		private void RemoveAllMethods(CRMerge row)
		{
			PXSelect<CRMergeMethod,
				Where<CRMergeMethod.mergeID, Equal<Required<CRMergeMethod.mergeID>>>>.
				Clear(this);
			foreach (CRMergeMethod item in
				PXSelect<CRMergeMethod,
				Where<CRMergeMethod.mergeID, Equal<Required<CRMergeMethod.mergeID>>>>.
					Select(this, row.MergeID))
			{
				Methods.Cache.Delete(item);
			}
		}

		private void RemoveAllCriteria(CRMerge document)
		{
			PXSelect<CRMergeCriteria,
				Where<CRMergeCriteria.mergeID, Equal<Required<CRMerge.mergeID>>>>.
				Clear(this);
			foreach (CRMergeCriteria item in
				PXSelect<CRMergeCriteria,
					Where<CRMergeCriteria.mergeID, Equal<Required<CRMerge.mergeID>>>>.
					Select(this, document.MergeID))
			{
				Criteria.Cache.Delete(item);
			}
		}

		private void InsertMethods(CRMerge document)
		{
			var dacInfo = ReadProperties(document);
			if (dacInfo != null)
			{
				var cache = Methods.Cache;
				foreach (string name in dacInfo.FieldNames)
				{
					var newItem = (CRMergeMethod)cache.CreateInstance();
					newItem.MergeID = document.MergeID;
					newItem.DataField = name;
					/*object defaultMethod;
					cache.RaiseFieldDefaulting<CRMergeMethod.method>(newItem, out defaultMethod);
					newItem.Method = (int?)defaultMethod;*/
					cache.Insert(newItem);
				}
			}
		}

		#endregion
	}
}
