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
	/// Extension that is used for mapping creation only. Not so usefull by itself. The whole logic is implemented inside the derived extensions
	/// </summary>
	public abstract class CRParentChild<TGraph, TThis> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TThis : CRParentChild<TGraph, TThis>
	{
		#region DACs

		[PXHidden]
		public class Document : Document<TThis> { }

		[PXHidden]
		public class Child : Child<TThis> { }

		#endregion

		#region Views

		public PXSelectExtension<Document> PrimaryDocument;
		public PXSelectExtension<Child> ChildDocument;

		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			// unlink two views (to not to currupt by the next statement)
			ChildDocument.View = new PXView(Base, false, BqlCommand.CreateInstance(typeof(Select<>), ChildDocument.View.GetItemType()));

			// convert Child.childID into CacheExt.DAC.field
			var originalField = ChildDocument.Cache.GetType()
				.GetMethod("GetBaseBqlField", BindingFlags.Instance | BindingFlags.NonPublic)
				?.Invoke(ChildDocument.Cache, new object[] { nameof(Document.childID) })
				as Type;

			if (originalField == null)
				return;

			ChildDocument.View.WhereNew(BqlCommand.Compose(typeof(Where<,>),
				originalField,
				typeof(Equal<>), typeof(Required<>), originalField));
		}

		#endregion

		#region Implementation

		public virtual Child GetChildByID(int? childID)
		{
			if (childID == null)
				return null;

			if (ChildDocument.Current != null && ChildDocument.Cache.GetValue(ChildDocument.Current, nameof(ChildDocument.Current.ChildID)) as int? == childID)
			{
				return ChildDocument.Current;
			}

			return ChildDocument.SelectSingle(childID);
		}
		#endregion

		#region Mappings

		#region Document Mapping
		protected class DocumentMapping : IBqlMapping
		{
			public Type Extension => typeof(Document);
			protected Type _table;
			public Type Table => _table;

			public DocumentMapping(Type table)
			{
				_table = table;
			}
			// Types are used as nameof, so .A is ok here
			public Type RelatedID = typeof(Document.relatedID);
			public Type ChildID = typeof(Document.childID);
			public Type IsOverrideRelated = typeof(Document.isOverrideRelated);
		}
		protected virtual DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(Document));
		}
		#endregion

		#region Child Mapping
		protected class ChildMapping : IBqlMapping
		{
			public Type Extension => typeof(Child);
			protected Type _table;
			public Type Table => _table;

			public ChildMapping(Type table)
			{
				_table = table;
			}
			// Types are used as nameof, so .A is ok here
			public Type ChildID = typeof(Child.childID);
			public Type RelatedID = typeof(Child.relatedID);
		}
		protected abstract ChildMapping GetChildMapping();
		#endregion

		#endregion
	}
}
