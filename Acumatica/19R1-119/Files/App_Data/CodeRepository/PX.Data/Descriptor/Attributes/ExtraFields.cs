// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/

using PX.Common;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;

namespace PX.Data
{

    /// <exclude/>
	public interface IExtraFieldsAnchor
	{
	}


    /// <exclude/>
	public class ExtraFieldsAnchorAttribute : PXEventSubscriberAttribute, IExtraFieldsAnchor, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		protected Dictionary<string, int> _IndexByID; // passed to sql dialect string parsing routine
		// protected bool _IsActive;

		private Type[] _FieldTypes;
	
		public bool DefaultVisible { get; set; }

		public void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Select) 
				return;

			e.FieldName = BqlCommand.Null;
			e.DataType = PXDbType.NVarChar;

			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.Internal)
				return;

			if (!_BqlTable.IsAssignableFrom(sender.BqlTable))
				return;
			
			e.BqlTable = _BqlTable;


			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.External) {
				e.FieldName = BqlCommand.SubSelect + _FieldTypes.Length + ')';
			}
			else if ( (e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null )
			{
				Type extTable = e.Table ?? _BqlTable;

				StringBuilder bld = new StringBuilder();
				e.SqlDialect.prepareNoteAttributesJoined(bld, typeof (ExtraFieldValue.value), extTable, extTable);
				e.FieldName = bld.ToString();
				return;
			}
			

		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			string firstColumn = e.Record.GetString(e.Position);
			var sqlDialect = sender.Graph.SqlDialect;

			string[] fetched;
			if(sqlDialect.tryExtractAttributes(firstColumn, _IndexByID, out fetched)) {
				for (int i = 0; i < _IndexByID.Count; i++) {
					string srcVal = fetched[i];
					if (null == srcVal && _FieldTypes[i].IsValueType)
						continue;

					object val = Convert.ChangeType(fetched[i], _FieldTypes[i]);
					sender.SetValue(e.Row, 1 + _FieldOrdinal + i , val);
				}
			}
			e.Position++;
			return;
		}

		public override void CacheAttached(PXCache sender)
		{
			Type cacheType = sender.GetItemType();
//			sender.Graph.GetType()
			ExtraFieldsDefinition definition = ExtraFieldsDefinition.getForType(cacheType);
			if (definition == null) return;

			if (_IndexByID == null) {
				var di = new Dictionary<string, int>();
				var types = new List<Type>();
				for (int i = 0; i < definition.AttributeIdByIndex.Length; i++) {
					types.Add(definition.GetAttributeType(i));
					di.Add(definition.AttributeIdByIndex[i].ToString(), i);
				}
				_IndexByID = di;
				_FieldTypes = types.ToArray();
			}
		}
	}

    /// <exclude/>
	public class ExtraFieldAttribute : PXEventSubscriberAttribute, IPXCommandPreparingSubscriber, IPXRowPersistedSubscriber, IPXRowPersistingSubscriber, IPXRowDeletedSubscriber
	{
		private int index;			// index in the anchor object that stores our data
		private int ordinal;	// ordinal of anchor object in TNode row
		private int attributeId;	// identifier assigned by DB
		private const string NoteID = "NoteID";

		public ExtraFieldAttribute(int ordinal, int index, int attributeId)
		{
			this.index = index;
			this.ordinal = ordinal;
			this.attributeId = attributeId;
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			// ensure noteID is initialized on main object
			int ixNote = sender.GetFieldOrdinal(NoteID);
			var noteId = sender.GetValue(e.Row, ixNote);
			if (null == noteId)
				sender.SetValue(e.Row, ixNote, SequentialGuid.Generate());
		}



		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
				return; 

			var newVal = sender.GetValue(e.Row, ordinal);
			// we should skip update if the row has not been changed

			int ixNote = sender.GetFieldOrdinal(NoteID);
			var noteId = sender.GetValue(e.Row, ixNote);
			bool updated = PXDatabase.Provider.Update(typeof(ExtraFieldValue), 
				new PXDataFieldRestrict<ExtraFieldValue.noteId>(noteId), 
				new PXDataFieldRestrict<ExtraFieldValue.extFieldId>(attributeId),
				new PXDataFieldAssign<ExtraFieldValue.value>(newVal));
			if( !updated )
				PXDatabase.Provider.Insert(typeof(ExtraFieldValue), 
					new PXDataFieldAssign<ExtraFieldValue.noteId>(noteId),
					new PXDataFieldAssign<ExtraFieldValue.extFieldId>(attributeId),
					new PXDataFieldAssign<ExtraFieldValue.value>(newVal));
		}

		public void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			e.Cancel = true;
			e.FieldName = null;
			e.DataType = PXDbType.NVarChar;

			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Select)
				return;

			if (!_BqlTable.IsAssignableFrom(sender.BqlTable))
				return;

			if (((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
				 (e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null)) {
				e.DataValue = e.Value;
				e.BqlTable = e.Table ?? _BqlTable;

				ISqlDialect sql = e.SqlDialect;

				if ((e.Operation & PXDBOperation.Option) == PXDBOperation.External) {
					System.Text.StringBuilder bld = new StringBuilder(BqlCommand.SubSelect);

					List<IBqlParameter> parameters = new List<IBqlParameter>();
					
					bld.Append(typeof(ExtraFieldValue.value).Name);
					bld.Append(" FROM ");
					bld.Append(typeof(ExtraFieldValue).Name);
					bld.Append(" WHERE ");

					Type sendersNoteID = sender.GetItemType().GetNestedType("noteID");
					Type where = BqlCommand.Compose(
						typeof(Where<,,>), typeof(ExtraFieldValue.extFieldId), typeof(Equal<>), typeof(FieldNameParam),
						typeof(And<,>), typeof(ExtraFieldValue.noteId), typeof(Equal<>), sendersNoteID);
					var conditionBql = Activator.CreateInstance(where) as IBqlCreator;
					conditionBql.Parse(sender.Graph, parameters, null, null, null, bld, null);
					bld.Replace(typeof(FieldNameParam).FullName, attributeId.ToString());

					bld.Append(" ORDER BY ");
					bld.Append(typeof(ExtraFieldValue.value).Name);
					bld.Append(")");
					e.FieldName = bld.ToString();
				}
			}
		}

		public void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			int ixNote = sender.GetFieldOrdinal(NoteID);
			var noteId = sender.GetValue(e.Row, ixNote);

			// if the main table uses DeletedColumn, do not remove the records?
			PXDatabase.Provider.Delete(typeof(ExtraFieldValue),
				new PXDataFieldRestrict<ExtraFieldValue.noteId>(noteId),
				new PXDataFieldRestrict<ExtraFieldValue.extFieldId>(attributeId)
			);
		}
	}

    /// <exclude/>
	public class ExtraFieldInfo
	{
		public int Id;
		public string Name;
		public string DataType;
		public string DefaultAttributes;

		internal readonly List<FieldOnGraph> attributes = new List<FieldOnGraph>();

		public Type GetFieldType()
		{
			switch (DataType.ToLower()) {
				case "int": return typeof(int);
				case "string": return typeof(String);
				//case "string":
				//case "string": return typeof(String);
				case "bool": return typeof(bool);
				case "date": return typeof(DateTime);
			}
			return typeof(object);
		}

		public PXEventSubscriberAttribute GetFailSafeAttribute()
		{
			Type ft = GetFieldType();
			if (ft == typeof(bool) || ft == typeof(bool?))
				return new PXBoolAttribute();
			if (ft == typeof(int) || ft == typeof(int?))
				return new PXIntAttribute();
			if (ft == typeof(string))
				return new PXStringAttribute();
			if (ft == typeof(DateTime) || ft == typeof(DateTime))
				return new PXDateAttribute();
			return null;
		}

        /// <exclude/>
		public struct FieldOnGraph
		{
			public readonly string GraphName;
			public readonly string Attributes;
			public FieldOnGraph(string graphName, string attribs)
			{
				Attributes = attribs;
				GraphName = graphName;
			}
		}

		internal PXEventSubscriberAttribute[] GetAttributes(String graphName)
		{
			var customGraphRule = attributes.FirstOrDefault(a => a.GraphName.Equals(graphName, StringComparison.OrdinalIgnoreCase));
			String myAttributes = customGraphRule.Attributes ?? DefaultAttributes;
			// TODO: Parse string with attributes
			PXEventSubscriberAttribute sa = GetFailSafeAttribute();
			return sa == null ? new PXEventSubscriberAttribute[] {} : new PXEventSubscriberAttribute[] { sa };
		}
	}

    /// <exclude/>
	public class ExtraFieldsDefinition
	{
		public static ExtraFieldsDefinition getForType(Type nodeType)
		{
			return null;
			/*
			// PXCache.getBqlTableAndParents(nodeType);  <- this will collect all base DACs
			string slotKey = typeof(ExtraField).Name + '_' + System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
			Dictionary<string, List<ExtraFieldInfo>> dict = PXDatabase.Provider.GetSlot<Dictionary<String, List<ExtraFieldInfo>>>(slotKey, Prefetch);

			List<ExtraFieldInfo> efis = dict != null && dict.TryGetValue(nodeType.FullName, out efis) ? efis : null;
			return efis == null || efis.Count == 0 ? null : new ExtraFieldsDefinition(efis);
			*/
		}

		static void Prefetch(Dictionary<String, List<ExtraFieldInfo>> result)
		{
			try {
				PXContext.SetSlot(PXSelectorAttribute.selectorBypassInit, true);

				Dictionary<int, ExtraFieldInfo> id2ExtraField = new Dictionary<int,ExtraFieldInfo>();

				IEnumerable<PXDataRecord> records = PXDatabase.Provider.SelectMulti(typeof(ExtraField),
					new PXDataField<ExtraField.extFieldId>(),
					new PXDataField<ExtraField.dataType>(),
					new PXDataField<ExtraField.name>(),
					new PXDataField<ExtraField.dacName>(),
					new PXDataField<ExtraField.defaultAttributes>());
				foreach (PXDataRecord res in records) {
					ExtraFieldInfo efi = new ExtraFieldInfo() { Id = res.GetInt32(0).GetValueOrDefault(0), DataType = res.GetString(1), Name = res.GetString(2)};
					if( !res.IsDBNull(4) )
						efi.DefaultAttributes = res.GetString(4);
					id2ExtraField.Add(efi.Id, efi);

					string dacName = res.GetString(3);
					List<ExtraFieldInfo> le = null;
					if(!result.TryGetValue(dacName, out le))
					{
						le = new List<ExtraFieldInfo>();
						result.Add(dacName, le);
					}
					le.Add(efi);
				}

				IEnumerable<PXDataRecord> records2 = PXDatabase.Provider.SelectMulti(typeof(ExtraFieldOnGraph),
					new PXDataField<ExtraFieldOnGraph.extFieldId>(),
					new PXDataField<ExtraFieldOnGraph.graphName>(),
					new PXDataField<ExtraFieldOnGraph.attributes>());
				foreach (PXDataRecord res in records2) {
					int extFieldId = res.GetInt32(0).GetValueOrDefault(0);
					string graphName = res.GetString(1);
					string attribs = res.GetString(2);

					ExtraFieldInfo efi;
					if (!id2ExtraField.TryGetValue(extFieldId, out efi)) continue;

					efi.attributes.Add(new ExtraFieldInfo.FieldOnGraph(graphName, attribs));
				}
			}
			finally {
				PXContext.SetSlot<bool>(PXSelectorAttribute.selectorBypassInit, false);
			}
		}


		protected List<ExtraFieldInfo> fields;
		public int[] AttributeIdByIndex;
		public string[] AttributeNames;

		public static BqlCommand cmdSelectFields = new Select2<ExtraField, InnerJoin<ExtraFieldOnGraph, On<ExtraFieldOnGraph.extFieldId, Equal<ExtraField.extFieldId>>>>();

		public ExtraFieldsDefinition(List<ExtraFieldInfo> efis)
		{
			this.fields = efis;

			List<int> ids = new List<int>();
			List<String> names = new List<string>();

			foreach (ExtraFieldInfo attr in fields) {
				ids.Add(attr.Id);
				names.Add(attr.Name);
			}

			AttributeIdByIndex = ids.ToArray();
			AttributeNames = names.ToArray();
		}


		internal Type[] GetExtensionGenerics(Type tnode)
		{
			Type[] result = new Type[1 + fields.Count];
			result[0] = tnode;
			for (int i = 1; i <= fields.Count; i++)
				result[i] = fields[i - 1].GetFieldType();
			return result;
		}

		public PXEventSubscriberAttribute[] GetFieldAttributes(int ordinal, int iAttribute)
		{
			PXEventSubscriberAttribute[] definedAttrs = fields[iAttribute].GetAttributes(null);
			PXEventSubscriberAttribute[] result = new PXEventSubscriberAttribute[definedAttrs.Length + 1];
			result[0] = new ExtraFieldAttribute(ordinal, iAttribute, AttributeIdByIndex[iAttribute]);
			Array.Copy(definedAttrs, 0, result, 1, definedAttrs.Length);
			return result;
		}

		internal Type GetAttributeType(int iAttribute)
		{
			return fields[iAttribute].GetFieldType();
		}

		internal string GetFieldName(int iAttribute)
		{
			return fields[iAttribute].Name;
		}
	}

    /// <exclude/>
	[Serializable]
	public class ExtraFieldValue : IBqlTable
	{

        /// <exclude/>
		public abstract class noteId : IBqlField { }
		[PXDBGuid]
		public virtual Guid? NoteID { get; set; }

		//public abstract class tableRef : IBqlField {}
		//[PXDBInt]
		//[PXDBDefault]
		//public virtual int TableRef { get; set; }

        /// <exclude/>
		public abstract class value : IBqlField {}
		[PXDBString]
		public virtual string Value { get; set; }

        /// <exclude/>
		public abstract class extFieldId : IBqlField { }
		[PXDBInt]
		public virtual int ExtFieldId { get; set; }
	}

    /// <exclude/>
	public class ExtraField : IBqlTable
	{
        /// <exclude/>
		public abstract class extFieldId : IBqlField { }
		[PXDBIdentity()]
		public virtual int ExtFieldID { get; set; }

        /// <exclude/>
		public abstract class name : IBqlField { }
		[PXDBString()]
		public virtual string Name { get; set; }

        /// <exclude/>
		public abstract class dacName : IBqlField { }
		[PXDBString]
		public virtual string DacName { get; set; }

        /// <exclude/>
		public abstract class dataType : IBqlField { }
		[PXDBString(16)]
		public virtual string DataType { get; set; }

        /// <exclude/>
		public abstract class defaultAttributes : IBqlField { }
		[PXDBString()]
		public virtual string DefaultAttributes { get; set; }
	}

    /// <exclude/>
	public class ExtraFieldOnGraph : IBqlTable
	{
        /// <exclude/>
		public abstract class extFieldId : IBqlField { }
		[PXDBInt(IsKey=true)]
		public virtual int ExtFieldID { get; set; }

        /// <exclude/>
		public abstract class graphName : IBqlField { }
		[PXDBString(IsKey=true)]
		public virtual string GraphName { get; set; }

        /// <exclude/>
		public abstract class attributes : IBqlField { }
		[PXDBString()]
		public virtual string Attributes { get; set; }
	}

    /// <exclude/>
	public class PXCustomAttributeAttribute : PXEventSubscriberAttribute
	{
		public PXCustomAttributeAttribute(int offset) { Offset = offset; }
		public readonly int Offset;
	}
}