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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PX.Api;
using PX.Api.Soap.Screen;
using PX.Common;
using PX.SM;
using PX.Data.Automation;
using PX.Data.SQLTree;
using PX.DbServices.QueryObjectModel;
using PX.DbServices.Commands.Data;
using System.Web;

namespace PX.Data
{

    /// <summary>Binds the <tt>NoteID</tt> DAC field of <tt>Guid?</tt> type to the database column that keeps note identifiers and enables attachment of text comments, files, and activity items
    /// to a data record.</summary>
    /// <remarks>
    ///   <para>The attribute should be placed on the DAC field that will hold the identifier of the related note. A note is a data record in the <tt>Note</tt> database
    /// table. A note data record contains the note identifier, the text comment, the DAC name of the related data record, and some other fields.</para>
    ///   <para>Only one data record can reference a note. So the identifier of this note can be used as the global identifier of the data record. Thanks to this fact, in
    /// addition to adding text comments to a data record notes are used to implement:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <i>Full-text search of data records</i>: A note can be used to store the specified fields of the related data record, which can be found by these
    ///     fields through the website search.</item>
    ///     <item>
    ///       <i>File attachments</i>: The relationships between files and notes are kept in a separate table, <tt>NoteDoc</tt>, as pairs of a file identifier and
    ///     note identifier. The <tt>UploadFile</tt> stores general information about files, and the <tt>UploadRevision</tt> stores specific revisions of files.</item>
    ///     <item>
    ///       <i>Association of activity items with a data record</i>.</item>
    ///     <item>
    ///       <i>Multi-language fields</i>: For more information about how to create fields with localizable values, see <see cref="!:https://help.acumatica.com/(W(8))/Main?ScreenId=ShowWiki&amp;amp;amp;amp;amp;pageid=a1de7d38-cd06-4497-a5b7-61b1a01b7d16">To Work with Multi-Language
    ///     Fields</see>.</item>
    ///   </list>
    ///   <para>For any of these features to work, the given DAC should define a field marked with the <tt>PXNote</tt> attribute.</para>
    /// </remarks>
    /// <example>
    ///   <para></para>
    ///   <code title="Example" description="The attribute below indicates that the DAC field references a note. Here, new Type[0] as parameter is used to force creation of the note on saving of a data record even if the used did not create a note manually. " lang="CS">
    /// [PXNote(new Type[0])]
    /// public virtual Guid? NoteID { get; set; }</code>
    ///   <code title="Example2" description="&lt;para&gt;The attribute below indicates that the DAC field holds note identifier, sets the lists of fields (from different tables) that will be saved in the note, and allows association of a data record with activity items. It will be possible to find the &lt;tt&gt;Vendor&lt;/tt&gt; data record through the application website search by the values of these fields. &lt;/para&gt;&#xD;&#xA;&lt;para&gt;The first few parameters specify fields to save in the note. The &lt;tt&gt;ForeignRelations&lt;/tt&gt; property specifies the &lt;tt&gt;Vendor&lt;/tt&gt; fields that reference the related &lt;tt&gt;Contact&lt;/tt&gt; and &lt;tt&gt;Address&lt;/tt&gt; data records. Fields from these tables are also provided among the field to save in the note. &lt;/para&gt;&#xD;&#xA;&lt;para&gt;The &lt;tt&gt;ShowInReferenceSelector&lt;/tt&gt; allows attaching activity items to &lt;tt&gt;Vendor&lt;/tt&gt; data records. On the activity webpage, the lookup field for selecting a related data record will display the &lt;tt&gt;Vendor.AcctCD&lt;/tt&gt; (configured by &lt;tt&gt;DescriptionField&lt;/tt&gt;) when a &lt;tt&gt;Vendor&lt;/tt&gt; data record is selected and use the same field (due to &lt;tt&gt;Selector&lt;/tt&gt;) as the reference value.&lt;/para&gt;" groupname="Example" lang="CS">
    /// [PXNote(
    ///     typeof(Vendor.acctCD),
    ///     typeof(Vendor.acctName),
    ///     typeof(Contact.eMail),
    ///     typeof(Contact.phone1),
    ///     typeof(Contact.fax),
    ///     typeof(Address.addressLine1),
    ///     typeof(Address.city),
    ///     typeof(Address.countryID),
    ///     typeof(Address.postalCode),
    ///     ForeignRelations =
    ///         new Type[] { typeof(Vendor.defContactID),
    ///                      typeof(Vendor.defAddressID) },
    ///     ExtraSearchResultColumns =
    ///         new Type[] { typeof(CR.Contact) },
    ///  
    ///     ShowInReferenceSelector = true,
    ///     DescriptionField = typeof(Vendor.acctCD),
    ///     Selector = typeof(Vendor.acctCD)
    /// )]
    /// public virtual Guid? NoteID { get; set; }</code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
    public class PXNoteAttribute : PXDBGuidAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, IPXRowDeletedSubscriber, IPXReportRequiredField
    {
        #region State
        internal const string _NoteTextField = "NoteText";
        internal string _NoteTextFieldDisplayName = PX.Data.EP.Messages.NoteTextDisplayName;
        public const string NotePopupTextField = "NotePopupText";
        internal const string _NoteFilesField = "NoteFiles";
        protected const string _NoteImagesField = "NoteImages";
        internal const string _NoteActivityField = "NoteActivity";
        protected const string _NoteImagesViewPrefix = "$NoteImages$";

        internal const string _NoteTextExistsField = "NoteTextExists";
        public const string NotePopupTextExistsField = "NotePopupTextExists";
        internal const string _NoteFilesCountField = "NoteFilesCount";
        internal const string _NoteActivitiesCountField = "NoteActivitiesCount";

        private PXView _noteNoteID;
        private PXView _noteDocNoteID;
        private PXView _noteFileID;
        protected bool _PassThrough;
        protected bool _ForceRetain;
        protected Type _ParentNoteID;
        protected Type[] extraSearchResultColumns;
        protected Type[] foreignRelations;

        protected bool _TextRequired;
        protected bool _PopupTextRequired;
        protected bool _FilesRequired;
        protected bool _ActivityRequired;
        private string _declaringType;

        /// <summary>Initializes a new instance of the attribute that will be used
        /// to attach notes to data record but won't save values of the fields in
        /// a note.</summary>
		public PXNoteAttribute() : base(true)
        {
        }

        /// <summary>Initializes an instance of the attribute that will save
        /// values of the provided fields in the note. The values saved in a note
        /// will be updated each time the data record is saved.</summary>
        /// <remarks>If you don't need to save fields in the note, but need to have a
        /// note automatically created for each data record of the current DAC
        /// type, provide an empty array as the parameter:
        /// <code>[Note(new Type[0])]</code></remarks>
		public PXNoteAttribute(params Type[] searches) : base(true)
        {
        }

        /// <exclude/>
		public virtual Type ParentNoteID
        {
            get
            {
                return this._ParentNoteID;
            }
            set
            {
                this._ParentNoteID = value;
            }
        }

        /// <summary>Gets or sets the value that indicates whether activity items
        /// can be associated with the DAC where the <tt>PXNote</tt> attribute is
        /// used. If the property equals <tt>true</tt>, the DAC will appear in the
        /// list of types in the lookup that selects the related data record for
        /// an activity. If the property equals <tt>false</tt>, activity
        /// attributes cannot be associated with data records of the DAC. By
        /// default the property equals <tt>false</tt>.</summary>
		public bool ShowInReferenceSelector { get; set; }

        /// <summary>Gets or sets the value that indicates whether the calculation
        /// of activities will be suppressed for object
        /// </summary>
        public bool SuppressActivitiesCount { get; set; }
        /// <summary>Gets or sets the value that indicates whether the calculation
        /// of activities will be calculated by parent link
        /// </summary>
        public bool ActivitiesCountByParent { get; set; }
        /// <summary>Gets or sets the list of fields that will be displayed in a
        /// separate column when rendering search results.</summary>
		public Type[] ExtraSearchResultColumns
        {
            get { return this.extraSearchResultColumns; }
            set { this.extraSearchResultColumns = value; }
        }

        /// <summary>Gets or sets the list of fields that connect the current
        /// table with foreign tables. The fields from the foreign tables can be
        /// specified along with current table fields in the <tt>Searches</tt>
        /// list.</summary>
		public Type[] ForeignRelations
        {
            get { return this.foreignRelations; }
            set { this.foreignRelations = value; }
        }

        /// <exclude/>
		public bool ForceFileCorrection { get; set; }

        public virtual bool PopupTextEnabled { get; set; }

        [PXInternalUseOnly]
        public virtual bool DoNotUseAsRecordID { get; set; }

        protected Note GetNote(PXGraph graph, Guid? id)
        {
            if (!id.HasValue) return null;
            foreach (PXDataRecord result in PXDatabase.Select(graph, new Select<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>(), 1, new PXDataValue(id)))
            {
				return new Note
                {
                    NoteID = result.GetGuid(0),
                    NoteText = result.GetString(1),
                    EntityType = result.GetString(2),
                    GraphType = result.GetString(3),
                    ExternalKey = result.GetString(4),
                    NotePopupText = result.GetString(5)
                };
            }
            return null;
        }
        protected PXView GetView(PXGraph graph)
        {
            return _noteNoteID ??
                   (_noteNoteID = new PXView(graph, false, new Select<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>()));
        }

        protected int GetActivityCount(PXGraph graph, Guid? id)
        {
            if (id == null)
            {
                return 0;
            }

            PXDataFieldValue field = null;

            if (ActivitiesCountByParent)
            {
                EntityHelper helper = new EntityHelper(graph);
                var entity = helper.GetEntityRow(id);

                if (entity == null) return 0;

                var fieldName = EntityHelper.GetIDField(entity.GetType());

                if (!String.IsNullOrWhiteSpace(fieldName))
                {
                    int parentID = (int)helper.GetField(entity, null, fieldName);

                field = new PXDataFieldValue(new Column("BAccountID", "CRActivity"), PXDbType.Int, 4, parentID);
            }
            }
            if (field == null)
            {
                field = new PXDataFieldValue(new Column("RefNoteID", "CRActivity"), PXDbType.UniqueIdentifier, 16, id.Value);
            }

            using (var record = PXDatabase.SelectSingle<CRActivity>(
                new PXDataField(SQLExpression.Count()),
                field,
                //new PXDataFieldValue(graph.SqlDialect.quoteDbIdentifier("CRActivity") + "." + graph.SqlDialect.quoteDbIdentifier("DeletedDatabaseRecord"), PXDbType.Bit, 1, 0),
                new PXDataFieldValue(new Column("UIStatus", "CRActivity"), PXDbType.Char, 2, "CL", PXComp.NE),
                new PXDataFieldValue(new Column("UIStatus", "CRActivity"), PXDbType.Char, 2, "RL", PXComp.NE)
                ))
            {
                if (record != null)
                {
                    return record.GetInt32(0) ?? 0;
                }
                return 0;
            }
        }
        protected PXView GetDocView(PXGraph graph)
        {
            if (_noteDocNoteID == null)
            {
                _noteDocNoteID = new PXView(graph, false, new Select<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>());
            }
            return _noteDocNoteID;
        }

        protected PXView GetFileByID(PXGraph graph)
        {
            if (_noteFileID == null)
                _noteFileID = new PXView(graph, false,
                    new Select2<UploadFile,
                        InnerJoin<UploadFileRevisionNoData, On<UploadFileRevisionNoData.fileID, Equal<UploadFile.fileID>,
                        And<UploadFileRevisionNoData.fileRevisionID, Equal<UploadFile.lastRevisionID>>>>>()
                        .WhereAnd(typeof(Where<UploadFile.fileID, Equal<Required<UploadFile.fileID>>>)));
            //_noteFileID = new PXView(graph, false,
            //	new Select2<UploadFile,
            //		InnerJoin<UploadFileRevision, On<UploadFileRevision.fileRevisionID, Equal<UploadFile.lastRevisionID>>>>()
            //		.WhereAnd(typeof(Where<UploadFile.fileID, Equal<Required<UploadFile.fileID>>>)));
            return _noteFileID;
        }

        /// <summary>Returns the identifier of the note attached to the provided
        /// object and inserts a new note into the cache if the note does not
        /// exist.</summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="name">The name of the field that stores note identifier.
        /// If <tt>null</tt>, the method will search attributes on all fields and
        /// use the first <tt>PXNote</tt> attribute it finds.</param>
		public static Guid? GetNoteID(PXCache cache, object data, string name)
        {
            data = CastRow(cache, data);
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
            {
                if (attr is PXNoteAttribute)
                {
                    return ((PXNoteAttribute)attr).GetNoteID(cache, data);
                }
            }
            return null;
        }

        /// <summary>Returns the identifier of the note attached to the provided
        /// object if note exists/
        /// </summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="name">The name of the field that stores note identifier.
        /// If <tt>null</tt>, the method will search attributes on all fields and
        /// use the first <tt>PXNote</tt> attribute it finds.</param>
        public static Guid? GetNoteIDIfExists(PXCache cache, object data, string name)
        {
            data = CastRow(cache, data);
            foreach (var noteAttribute in cache.GetAttributes(data, name).OfType<PXNoteAttribute>())
            {
                return noteAttribute.GetNoteIDIfExist(cache, data);
            }
            return null;
        }

        /// <summary>Returns the identifier of the note attached to the provided
        /// object if note exists/
        /// </summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        public static Guid? GetNoteIDIfExists(PXCache cache, object data)
        {
            data = CastRow(cache, data);
            foreach (var noteAttribute in cache.GetAttributesReadonly(null).OfType<PXNoteAttribute>())
            {
                return noteAttribute.GetNoteIDIfExist(cache, data);
            }
            return null;
        }

		/// <summary>
		/// Checks if note id record actually exists in the database
		/// </summary>
		public static bool NoteExists(PXGraph graph, Guid? id)
		{
			if (!id.HasValue) return false;
			foreach (PXDataRecord result in PXDatabase.Select(graph, new Select<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>(), 1, new PXDataValue(id)))
			{
				return true;
			}
			return false;

		}

		/// <summary>Returns the identifier of the note attached to the provided
		/// object and inserts a new note into the cache if the note does not
		/// exist. The field that stores note identifier is specified in the type
		/// parameter.</summary>
		/// <param name="sender">The cache object to search for the attributes of
		/// <tt>PXNote</tt> type.</param>
		/// <param name="data">The data record the method is applied to.</param>
		public static Guid? GetNoteID<Field>(PXCache cache, object data)
            where Field : IBqlField
        {
            data = CastRow(cache, data);
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
            {
                if (attr is PXNoteAttribute)
                {
                    return ((PXNoteAttribute)attr).GetNoteID(cache, data);
                }
            }
            return null;
        }

        internal static bool ImportEnsureNewNoteID(PXCache cache, object data, string externalKey)
        {
            return cache.GetAttributes(data, "NoteID").OfType<PXNoteAttribute>().Any(attr => (attr).EnsureNoteID(cache, data, externalKey) != Guid.Empty);
        }

        /// <summary>Returns the identifier of the note attached to the provided
        /// object or <tt>null</tt> if the note does not exist.</summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="name">The name of the field that stores note identifier.
        /// If <tt>null</tt>, the method will search attributes on all fields and
        /// use the first <tt>PXNote</tt> attribute it finds.</param>
		public static Guid? GetNoteIDReadonly(PXCache cache, object data, string name)
        {
            data = CastRow(cache, data);

            foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
            {
                if (attr is PXNoteAttribute)
                {
                    return (Guid?)cache.GetValue(data, (attr as PXNoteAttribute)._FieldOrdinal);
                }
            }
            return null;
        }

        private static object CastRow(PXCache cache, object data)
        {
            var itemType = cache.GetItemType();
            var res = data as PXResult;
            if (res != null) data = DynamicalyChangeType(res[itemType], itemType);
            data = DynamicalyChangeType(data, itemType);
            return data;
        }

        private static readonly Dictionary<string, MethodInfo> _castDic = new Dictionary<string, MethodInfo>();
        private static readonly object _syncObj = new object();

        private static object DynamicalyChangeType(object obj, Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (obj == null) return null;

            var objType = obj.GetType();

            if (type.IsAssignableFrom(objType)) return obj;

            MethodInfo meth = null;
            lock (_syncObj)
            {
                var key = string.Concat(objType.Name, "->", type.Name);
                if (!_castDic.TryGetValue(key, out meth))
                {
                    meth = GetMethod(objType, "op_Implicit", type,
                        BindingFlags.Static | BindingFlags.Public);
                    if (meth == null)
                    {
                        meth = GetMethod(objType, "op_Explicit", type,
                            BindingFlags.Static | BindingFlags.Public);
                    }
                    _castDic.Add(key, meth);
                }
            }

            if (meth == null) throw new InvalidCastException("Invalid cast: " + objType.GetLongName() + " to " + type.GetLongName());

            return meth.Invoke(null, new object[] { obj });
        }

        private static MethodInfo GetMethod(Type toSearch, string methodName,
            Type returnType, BindingFlags bindingFlags)
        {
            return Array.Find(
                toSearch.GetMethods(bindingFlags),
                delegate (MethodInfo inf)
                {
                    return ((inf.Name == methodName) && (inf.ReturnType == returnType));
                });
        }

        /// <summary>Returns the identifier of the note attached to the provided
        /// object or <tt>null</tt> if the note does not exist. The field that
        /// stores note identifier is specified in the type parameter.</summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
		public static Guid? GetNoteIDReadonly<Field>(PXCache cache, object data)
            where Field : IBqlField
        {
            data = CastRow(cache, data);
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
            {
                if (attr is PXNoteAttribute)
                {
                    return (Guid?)cache.GetValue(data, (attr as PXNoteAttribute)._FieldOrdinal);
                }
            }
            return null;
        }

        /// <summary>Sets the DAC type of the data record to which the note is
        /// attached. The full name of the DAC is saved in the database in the
        /// note record. This information is used, for example, to determine the
        /// webpage to open to show full details of the data record associated
        /// with a note.</summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="noteFieldName">The name of the field that stores note
        /// identifier.</param>
        /// <param name="newEntityType">New DAC type to associate with the
        /// note.</param>
		public static void UpdateEntityType(PXCache cache, object data, string noteFieldName, Type newEntityType)
        {
            data = CastRow(cache, data);
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, noteFieldName))
            {
                if (attr is PXNoteAttribute)
                {
                    Note note = PXSelect<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>.SelectWindowed(cache.Graph, 0, 1, ((PXNoteAttribute)attr).GetNoteID(cache, data));
                    if (note != null)
                    {
                        note.EntityType = ((PXNoteAttribute)attr)._declaringType;
                        cache.Graph.Caches[typeof(Note)].Update(note);
                    }
                }
            }
        }

        /// <exclude/>
		public static long? GetParentNoteID<Field>(PXCache cache, object data)
            where Field : IBqlField
        {
            data = CastRow(cache, data);
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
            {
                if (attr is PXNoteAttribute)
                {
                    return ((PXNoteAttribute)attr).GetParentNoteID(cache, data);
                }
            }
            return null;
        }


        /// <exclude/>
		public static void ForcePassThrow<Field>(PXCache cache)
            where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(typeof(Field).Name))
            {
                if (attr is PXNoteAttribute)
                {
                    ((PXNoteAttribute)attr)._PassThrough = true;
                    break;
                }
            }
        }

        /// <exclude/>
		public static void ForcePassThrow(PXCache cache, string name)
        {
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(name))
            {
                if (attr is PXNoteAttribute)
                {
                    ((PXNoteAttribute)attr)._PassThrough = true;
                    break;
                }
            }
        }

        /// <exclude/>
        public static void ForceRetain<Field>(PXCache cache)
            where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(typeof(Field).Name))
            {
                if (attr is PXNoteAttribute)
                {
                    ((PXNoteAttribute)attr)._ForceRetain = true;
                    break;
                }
            }
        }

        /// <exclude/>
        public static void ForceRetain(PXCache cache, string name)
        {
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(name))
            {
                if (attr is PXNoteAttribute)
                {
                    ((PXNoteAttribute)attr)._ForceRetain = true;
                    break;
                }
            }
        }

        /// <summary>Returns the identifier of the note attached to the provided
        /// object and inserts a new note into the database if the note does not
        /// exist.</summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
		public static Guid? GetNoteIDNow(PXCache cache, object data)
        {
            data = CastRow(cache, data);
            Guid? id = null;
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
            {
                if (!(attr is PXNoteAttribute)) continue;

                PXNoteAttribute noteattr = (PXNoteAttribute)attr;
                id = (Guid?)cache.GetValue(data, noteattr.FieldName);
                if (id == null)
                {
                    id = GenerateId();
                    using (PXTransactionScope tran = new PXTransactionScope())
                    {
                        InsertNoteRecord(cache, id.Value, string.Empty);

                        cache.SetValue(data, noteattr.FieldName, id);
                        noteattr.updateTableWithId(cache, data, id);
                        tran.Complete();

                        cache.Graph.TimeStamp = PXDatabase.SelectTimeStamp();
                    }
                }
                else if (!NoteExists(cache.Graph, id))
                {
                    using (PXTransactionScope tran = new PXTransactionScope())
                    {
                        InsertNoteRecord(cache, id.Value, string.Empty);
                        tran.Complete();
                        cache.Graph.TimeStamp = PXDatabase.SelectTimeStamp();
                    }
                }
                break;
            }
            return id;
        }

        internal static Guid GenerateId() => SequentialGuid.Generate();

        private void updateTableWithId(PXCache sender, object data, Guid? id)
        {
            List<PXDataFieldParam> pars = new List<PXDataFieldParam>();
            PXDataFieldAssign assign = new PXDataFieldAssign(_DatabaseFieldName, PXDbType.UniqueIdentifier, 16, id, sender.ValueToString(_FieldName, id));
            pars.Add(assign);
            foreach (string field in sender.Fields)
            {
                PXCommandPreparingEventArgs.FieldDescription description = null;
                try
                {
                    sender.RaiseCommandPreparing(field, data, sender.GetValue(data, field), PXDBOperation.Update, null, out description);
                }
                catch (PXDBTimestampAttribute.PXTimeStampEmptyException)
                {
                }
                if (description != null && description.IsRestriction && description.DataType != PXDbType.Timestamp
                    && (sender.BqlSelect == null
                        || description.Expr != null
                        && ((description.Expr as Column)?.Table() as SimpleTable).Name == _BqlTable.Name))
                {
                    pars.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
                }
            }
            if (pars.Count > 1 || sender.Keys.Count == 0)
            {
                PXDatabase.Update(BqlTable, pars.ToArray());
            }
        }

        /// <summary>
        /// Sets the flags to behavior control of extended virtual fields selecting
        /// </summary>
        /// <param name="cache">The cache object to search for the attributes of <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to. If <tt>null</tt>, the method is applied to all data records in the cache object.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="isTextRequired">Flag of selecting 'NoteText' fields</param>
        /// <param name="isFilesRequired">Flag of selecting 'NoteDoc' fields</param>
        /// <param name="isActivitiesRequired">Flag of selecting 'NoteActivity' fields</param>
        public static void SetTextFilesActivitiesRequired(PXCache cache, object data, string fieldName, bool isTextRequired = true, bool isFilesRequired = true, bool isActivitiesRequired = false)
        {
            if (data == null)
            {
                cache.SetAltered(fieldName, true);
            }
            foreach (PXNoteAttribute noteattr in cache.GetAttributes(data, fieldName).OfType<PXNoteAttribute>())
            {
                noteattr._TextRequired = isTextRequired;
                noteattr._PopupTextRequired = isTextRequired;
                noteattr._FilesRequired = isFilesRequired;
                noteattr._ActivityRequired = isActivitiesRequired;
            }
        }

        /// <summary>
        /// Sets the flags to behavior control of extended virtual fields selecting
        /// </summary>
        /// <param name="cache">The cache object to search for the attributes of <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to. If <tt>null</tt>, the method is applied to all data records in the cache object.</param>
        /// <param name="isTextRequired">Flag of selecting 'NoteText' fields</param>
        /// <param name="isFilesRequired">Flag of selecting 'NoteDoc' fields</param>
        /// <param name="isActivitiesRequired">Flag of selecting 'NoteActivity' fields</param>
        public static void SetTextFilesActivitiesRequired<Field>(PXCache cache, object data, bool isTextRequired = true, bool isFilesRequired = true, bool isActivitiesRequired = false)
            where Field : IBqlField
        {
            SetTextFilesActivitiesRequired(cache, data, typeof(Field).Name, isTextRequired, isFilesRequired, isActivitiesRequired);
        }

        /// <summary>Sets the list of identifiers of files that are shown in the
        /// <b>Files</b> pop-up window.</summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="fileIDs">The indetifiers of files to display.</param>
		public static void SetFileNotes(PXCache cache, object data, params Guid[] fileIDs)
        {
            Guid[] oldfiles = GetFileNotes(cache, data);
            if ((oldfiles != null && oldfiles.Length > 0) ||
                (fileIDs != null && fileIDs.Length > 0))
                cache.SetValueExt(data, _NoteFilesField, fileIDs);            
        }

        /// <summary>Returns the list of identifiers of files that are shown in
        /// the <b>Files</b> pop-up window.</summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
		public static Guid[] GetFileNotes(PXCache sender, object data)
        {
            foreach (PXNoteAttribute noteattr in sender.GetAttributesReadonly(null).OfType<PXNoteAttribute>())
            {
                Guid? id = (Guid?)sender.GetValue(data, noteattr._FieldName);
                return noteattr.GetDocView(sender.Graph).SelectMulti(id).Cast<NoteDoc>().Where(doc => doc.FileID.HasValue).Select(doc => doc.FileID.Value).ToArray();
            }
            return null;
        }
        /// <summary>Returns the text comment of the note attached to the provided
        /// object.</summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
		public static string GetNote(PXCache sender, object data)
        {
            string result = (string)PXFieldState.UnwrapValue(sender.GetValueExt(data, _NoteTextField));
            return string.IsNullOrEmpty(result) ? null : result;
        }

        public static string GetPopupNote(PXCache cache, object data)
        {
            return PXFieldState.UnwrapValue(cache.GetValueExt(data, NotePopupTextField)) as string;
        }

        /// <summary>Sets the text of the note attached to the provided data
        /// record.</summary>
        /// <param name="sender">The cache object to search for the attributes of
        /// <tt>PXNote</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="note">The text to place in the note.</param>
		public static void SetNote(PXCache sender, object data, string note)
        {
            if (GetNote(sender, data) != note)
                sender.SetValueExt(data, _NoteTextField, note);
        }

        /// <exclude/>
	    public interface IPXCopySettings
        {
            bool? CopyNotes { get; }
            bool? CopyFiles { get; }
        }

        /// <exclude />
        public static void CopyNoteAndFiles(PXCache src_cache, object src_row, PXCache dst_cache, object dst_row, bool? copyNotes, bool? copyFiles)
        {
            object NoteID = GetNoteIDIfExists(src_cache, src_row);
            if (copyNotes == true && NoteID != null)
                SetNote(dst_cache, dst_row, GetNote(src_cache, src_row));
            if (copyFiles == true && NoteID != null)
            {
                Guid[] files = GetFileNotes(src_cache, src_row);
                if (files != null && files.Any())
                    SetFileNotes(dst_cache, dst_row, files);
            }
        }

        /// <exclude />
        public static void CopyNoteAndFiles(PXCache src_cache, object src_row, PXCache dst_cache, object dst_row, IPXCopySettings settings = null)
        {
            CopyNoteAndFiles(src_cache, src_row, dst_cache, dst_row, settings == null || settings.CopyNotes == true, settings == null || settings.CopyFiles == true);
        }
        #endregion

        #region Implementation
        protected static string GetGraphType(PXCache cache)
        {
            Type graphType = CustomizedTypeManager.GetTypeNotCustomized(cache.Graph);
            return graphType.FullName;
        }
		protected virtual string GetEntityType(PXCache cache, Guid? noteId) => _declaringType;
		protected virtual bool IsVirtualTable(Type table) => PXDatabase.IsVirtualTable(table);
        protected Guid? GetNoteIDIfExist(PXCache sender, object row)
        {
            return (Guid?)sender.GetValue(row, _FieldOrdinal);
        }
        protected Guid GetNoteID(PXCache sender, object row)
        {
            return EnsureNoteID(sender, row, null);
        }
        protected Guid EnsureNoteID(PXCache sender, object row, string externalKey)
        {
            Guid? id = (Guid?)sender.GetValue(row, _FieldOrdinal);

            PXView view = GetView(sender.Graph);
            if (id.HasValue && null != view.SelectSingle(id.Value)) // when note already exists
                return id.Value;

            Note note = new Note
            {
                NoteID = id,
                NoteText = string.Empty,
                EntityType = GetEntityType(sender, id),
                GraphType = GetGraphType(sender),
                ExternalKey = externalKey
            };

            note = (Note)view.Cache.Insert(note);
            if (!id.HasValue)
            {
                id = note.NoteID;
                sender.SetValue(row, _FieldOrdinal, id);
            }

            //Status of row on insert is "NotChanged", working around it
            if (sender.Locate(row) != null)
            {
                sender.Graph.EnsureRowPersistence(row);
            }
            sender.IsDirty = true;
            return id.Value;
        }

        protected long? GetParentNoteID(PXCache sender, object row)
        {
            if (_ParentNoteID == null)
                return null;
            PXCache parentCache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
            if (parentCache.Current == null)
                return null;
            return (long?)parentCache.GetValue(parentCache.Current, _ParentNoteID.Name);
        }

        /// <exclude/>
        public virtual void noteTextFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            Func<Note, string> getText = n => n.NoteText;

            NoteTextGenericFieldSelecting(sender, e, GetOriginalCountsItem, SetNoteTextExists, getText, _NoteTextField);
        }

        public virtual void notePopupTextFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (PopupTextEnabled)
            {
                Func<Note, string> getText = n => n.NotePopupText;

                NoteTextGenericFieldSelecting(sender, e, GetPopupOriginalCountsItem, SetPopupNoteTextExists, getText, NotePopupTextField);
            }
        }

        private void NoteTextGenericFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, Func<PXCache, IBqlTable, string> getOriginalCountsItem, Action<PXCache, object, string> setNoteTextExists,
                                                   Func<Note, string> getText, string fieldName)
        {
            //Checking for virtual dac
            if (e.Row == null && IsVirtualTable(sender.BqlTable))
            {
                e.ReturnValue = null;
                e.ReturnState = null;
                e.Cancel = true;
                return;
            }

            Guid? id = null;
            if (e.Row != null)
            {
                id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
            }
            if (id.HasValue)
            {
                string result = getOriginalCountsItem(sender, e.Row as IBqlTable);
                if (result == null)
                {
                    Note note = GetNote(sender.Graph, id);
                    if (note != null)
                    {
                        e.ReturnValue = NormalizeNoteText(getText(note) ?? string.Empty);
                    }
                }
                else
                {
                    e.ReturnValue = result;
                }
                setNoteTextExists(sender, e.Row, (string)e.ReturnValue);
            }
            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                e.ReturnState = PXNoteState.CreateInstance(e.ReturnState, fieldName, _FieldName, _NoteTextField, _NoteFilesField, _NoteActivityField, _NoteTextExistsField, _NoteFilesCountField, _NoteTextFieldDisplayName);
            }
        }

        /// <exclude />
        public static string NormalizeNoteText(string noteText)
        {
            if (noteText == null)
                return null;
            var split = noteText.Split('\0');
            return split.Length > 0 ? split[0] : string.Empty;
        }

        private string GetValueFromState(PXCache cache, object row, Type field)
        {
            object val = cache.GetStateExt(row, field.Name);
            PXFieldState state = val as PXFieldState;

            if (state != null)
                val = state.Value;

            if (state is PXIntState)
            {
                PXIntState istate = (PXIntState)state;
                if (istate.AllowedValues != null && istate.AllowedLabels != null)
                    for (int i = 0; i < istate.AllowedValues.Length && i < istate.AllowedLabels.Length; i++)
                        if (istate.AllowedValues[i] == (int)val)
                        {
                            val = istate.AllowedLabels[i];
                            break;
                        }
            }
            else if (state is PXStringState)
            {
                PXStringState sstate = (PXStringState)state;
                if (sstate.AllowedValues != null && sstate.AllowedLabels != null)
                    for (int i = 0; i < sstate.AllowedValues.Length && i < sstate.AllowedLabels.Length; i++)
                        if (sstate.AllowedValues[i] == (string)val)
                        {
                            val = sstate.AllowedLabels[i];
                            break;
                        }
            }

            return val is string ? (string)val : string.Empty;
        }

        /// <exclude/>
		public virtual void noteTextFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            Action<PXCache, Guid, string> updateNoteRecord = (c, g, s) => UpdateNote(c, g, s);
            Action<Note, string> assignText = (n, s) => n.NoteText = s;
            Action<bool> textRequired = r => _TextRequired = r;

            NoteTextGenericFieldUpdating(sender, e, SetNoteTextExists, updateNoteRecord, assignText, _NoteTextField, textRequired);
		}

        public virtual void notePopupTextFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (!PopupTextEnabled)
            {
                return;
            }

			Action<PXCache, Guid, string> updateNoteRecord = (c, g, s) => UpdateNote(c, g, popupText: s);
			Action<Note, string> assignText = (n, s) => n.NotePopupText = s;
			Action<bool> textRequired = r => _PopupTextRequired = r;

            NoteTextGenericFieldUpdating(sender, e, SetPopupNoteTextExists, updateNoteRecord, assignText, _NoteTextField, textRequired);

            var popupTextValue = e.NewValue as string;
            var assignDefaultTextValue = !string.IsNullOrEmpty(popupTextValue) && NeedToAssignDefaultNoteTextValue(sender, e.Row);
            if (!assignDefaultTextValue)
            {
                return;
            }

            sender.SetValueExt(e.Row, _NoteTextField, string.Empty);
        }

        private bool NeedToAssignDefaultNoteTextValue(PXCache cache, object row)
        {
            var id = cache.GetValue(row, _FieldOrdinal) as Guid?;
            if (id == null)
            {
                return false;
            }

            var noteView = GetView(cache.Graph);
            var note = noteView.SelectSingle(id) as Note;
            if (note == null)
            {
                return false;
            }

            var noteStatus = noteView.Cache.GetStatus(note);
            if (noteStatus != PXEntryStatus.Inserted || note.NoteText != null)
            {
                return false;
            }

            return true;
        }

        private void NoteTextGenericFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e, Action<PXCache, object, string> setNoteTextExists,
                                                  Action<PXCache, Guid, string> updateNoteRecord, Action<Note, string> assignText, string noteTextField, Action<bool> textRequired)
        {
            if (e.Row != null)
            {
                Guid? id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
                setNoteTextExists(sender, e.Row, ((string)e.NewValue) ?? "");
                if (_PassThrough || !sender.AllowUpdate && sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
                {
                    if (String.IsNullOrWhiteSpace((string)e.NewValue))
                    {
                        if (!id.HasValue) return;
                        using (PXTransactionScope tran = new PXTransactionScope())
                        {
                            updateNoteRecord(sender, id.Value, string.Empty);
                            tran.Complete();
                        }
                    }
                    else
                    {
                        string noteText = (string)e.NewValue;
                        bool generateGuid = !id.HasValue;
                        if (generateGuid)
                            id = GenerateId();

                        using (PXTransactionScope tran = new PXTransactionScope())
                        {
                            updateNoteRecord(sender, id.Value, noteText);
                            if (generateGuid)
                            {
                                sender.SetValue(e.Row, _FieldOrdinal, id);
                                updateTableWithId(sender, e.Row, id);
                            }
                            tran.Complete();
                        }
                    }
                }
                else
                {
                    bool referencedNoteExists = id != null;
                    if (referencedNoteExists)
                    {
                        Note note = GetView(sender.Graph).SelectSingle(id) as Note;
                        if (note != null)
                        {
                            assignText(note, (string)(e.NewValue ?? String.Empty));
                            note.EntityType = GetEntityType(sender, note.NoteID);
                            note.GraphType = GetGraphType(sender);
                            GetView(sender.Graph).Cache.Update(note);
                        }
                        else
                            referencedNoteExists = false; // guid was assigned to host object, but no record in Note table
                    }
                    if (!referencedNoteExists && !string.IsNullOrEmpty(e.NewValue as string) /*!= null*/)
                    {
                        Note note = new Note();
                        bool noteIdWasAssigned = id != null;
                        if (noteIdWasAssigned)
                            note.NoteID = id;
                        assignText(note, (string)(e.NewValue ?? String.Empty));
                        note.EntityType = GetEntityType(sender, note.NoteID);
                        note.GraphType = GetGraphType(sender);
                        note = GetView(sender.Graph).Cache.Insert(note) as Note;
                        if (note != null)
                        {
                            id = note.NoteID;
                            setNoteTextExists(sender, e.Row, note.NoteText);
                            if (!noteIdWasAssigned)
                                sender.SetValue(e.Row, _FieldOrdinal, id);
                        }
                    }
                    if (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged /*&& sender.Locate(e.Row) != null*/)
                    {
                        sender.SetStatus(e.Row, PXEntryStatus.Modified);
                    }
                    sender.IsDirty = true;
                }
            }
            else
            {
                textRequired(true);
                PXCache cache = sender.Graph.Caches[_BqlTable];
                if (cache != sender)
                {
                    object val = null;
                    cache.RaiseFieldUpdating(noteTextField, null, ref val);
                }
            }
        }

        /// <exclude/>
		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            Guid? id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
            if (id == null || _ForceRetain) return;
            PXCache cache = sender.Graph.Caches[typeof(Note)];
            Note note = new Note { NoteID = id };
            cache.Delete(note);

            PXCache dcache = sender.Graph.Caches[typeof(NoteDoc)];
            foreach (NoteDoc doc in GetDocView(sender.Graph).SelectMulti(id))
            {
                dcache.Delete(doc);
            }
        }
        /// <exclude/>

        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (e.NewValue == null)
                e.NewValue = GenerateId();
        }

        /// <exclude />
        public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            Guid? id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
            PXCache cache = sender.Graph.Caches[typeof(Note)];
            PXCache dcache = sender.Graph.Caches[typeof(NoteDoc)];

            if (id != null)
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    foreach (Note note in cache.Inserted)
                    {
                        if (note.NoteID != id) continue;

                        cache.PersistInserted(note);
                    }

                    foreach (Note note in cache.Updated)
                    {
                        if (note.NoteID == id)
                        {
                            cache.PersistUpdated(note);
                            break;
                        }
                    }

                    foreach (NoteDoc doc in dcache.Inserted)
                    {
                        if (doc.NoteID == id)
                        {
                            dcache.PersistInserted(doc);
                            string screenID = PX.Common.PXContext.GetScreenID();
                            screenID = string.IsNullOrEmpty(screenID) ? screenID : screenID.Replace(".", "");
                            if (!string.IsNullOrEmpty(screenID) && doc.FileID != null)
                                PX.SM.UploadFileMaintenance.SetAccessSource(doc.FileID.Value, null, screenID);
                            else if (screenID == null && doc.FileID != null)
                            {
                                var sm = (PXSiteMap.Provider as PXDatabaseSiteMapProvider).
                                    With(_ => _.FindSiteMapNodeByGraphType(sender.Graph.GetType().FullName));
                                //SiteMap sm = PXSelect<SiteMap, Where<SiteMap.graphtype, Equal<Required<SiteMap.graphtype>>>>.Select(sender.Graph, sender.Graph.GetType().FullName);
                                if (sm != null)
                                    PX.SM.UploadFileMaintenance.SetAccessSource(doc.FileID.Value, null, sm.ScreenID);
                            }
                        }

                    }
                    ts.Complete();
                }
            }

            foreach (Note note in cache.Deleted)
            {
                cache.PersistDeleted(note);
            }
            foreach (NoteDoc doc in dcache.Deleted)
            {
                dcache.PersistDeleted(doc);
            }
        }
        /// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(Note)];
            PXCache dcache = sender.Graph.Caches[typeof(NoteDoc)];
            Guid? id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
            if (!id.HasValue)
            {
                if (e.TranStatus == PXTranStatus.Completed)
                {
                    foreach (Note note in cache.Deleted)
                    {
                        cache.SetStatus(note, PXEntryStatus.Notchanged);
                    }
                    cache.IsDirty = false;
                    foreach (NoteDoc doc in dcache.Deleted)
                    {
                        dcache.SetStatus(doc, PXEntryStatus.Notchanged);
                    }
                    dcache.IsDirty = false;
                }
                return;
            }
            if (e.TranStatus != PXTranStatus.Open)
            {
                if (e.TranStatus != PXTranStatus.Aborted)
                {
                    if (id.HasValue && id.Value != Guid.Empty)
                        foreach (Note note in cache.Inserted)
                        {
                            if (id == note.NoteID)
                            {
                                cache.SetStatus(note, PXEntryStatus.Notchanged);
                                break;
                            }
                        }
                    foreach (Note note in cache.Updated)
                    {
                        if (id == note.NoteID)
                        {
                            cache.SetStatus(note, PXEntryStatus.Notchanged);
                            break;
                        }
                    }
                    foreach (Note note in cache.Deleted)
                    {
                        cache.SetStatus(note, PXEntryStatus.Notchanged);
                    }
                    cache.IsDirty = false;

                    foreach (NoteDoc doc in dcache.Inserted)
                    {
                        dcache.SetStatus(doc, PXEntryStatus.Notchanged);
                    }
                    foreach (NoteDoc doc in dcache.Deleted)
                    {
                        dcache.SetStatus(doc, PXEntryStatus.Notchanged);
                    }
                    dcache.IsDirty = false;
                }
            }
            else if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
            {
                string prefix = null;
                if (!ForceFileCorrection)
                    foreach (NoteDoc doc in dcache.Inserted)
                    {
                        if (doc.FileID != null && doc.NoteID == id)
                        {
                            if (prefix == null)
                            {
                                string screenID = PX.Common.PXContext.GetScreenID();
                                PXSiteMapNode node;
                                if (!String.IsNullOrEmpty(screenID) && (node = PXSiteMap.Provider.FindSiteMapNodeByScreenIDUnsecure(screenID.Replace(".", ""))) != null && !String.IsNullOrEmpty(node.Title))
                                {
                                    StringBuilder bld = new StringBuilder(node.Title);
                                    bld.Append(" (");
                                    for (int k = 0; k < sender.Keys.Count; k++)
                                    {
                                        if (k > 0)
                                        {
                                            bld.Append(", ");
                                        }
                                        bld.Append(sender.GetValue(e.Row, sender.Keys[k]));
                                    }
                                    bld.Append(")");
                                    prefix = bld.ToString();
                                }
                                else
                                {
                                    return;
                                }
                            }
                            string fileName = null;
                            using (PXDataRecord record = PXDatabase.SelectSingle<UploadFile>(
                                new PXDataField(typeof(UploadFile.name).Name),
                                new PXDataFieldValue(typeof(UploadFile.fileID).Name, PXDbType.UniqueIdentifier, 16, doc.FileID)))
                            {
                                if (record != null)
                                {
                                    fileName = record.GetString(0);
                                }
                            }
                            if (fileName != null)
                            {
                                Guid? existsNoteID = null;
                                int idx = fileName.IndexOf(")\\");
                                if (idx > 0)
                                {
                                    using (PXDataRecord record = PXDatabase.SelectSingle<NoteDoc>(
                                        new PXDataField(typeof(NoteDoc.noteID).Name),
                                        new PXDataFieldValue(typeof(NoteDoc.fileID).Name, PXDbType.UniqueIdentifier, 16, doc.FileID),
                                        new PXDataFieldValue(typeof(NoteDoc.noteID).Name, PXDbType.UniqueIdentifier, 16, id, PXComp.NE)))
                                    {
                                        if (record != null)
                                            existsNoteID = record.GetGuid(0);
                                    }
                                }
                                if (existsNoteID == null)
                                {
                                    if (idx > fileName.IndexOf(" ("))
                                    {
                                        fileName = prefix + fileName.Substring(idx + 1);
                                    }
                                    else
                                    {
                                        fileName = prefix + "\\" + fileName;
                                    }
                                    PXDatabase.Update<UploadFile>(
                                        new PXDataFieldAssign(typeof(UploadFile.name).Name, PXDbType.NVarChar, 255, fileName),
                                        new PXDataFieldRestrict(typeof(UploadFile.fileID).Name, PXDbType.UniqueIdentifier, 16, doc.FileID),
                                        PXDataFieldRestrict.OperationSwitchAllowed);
                                }
                            }
                        }
                    }
            }
        }
        /// <exclude/>
		public virtual void noteFilesFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            //Checking for virtual dac
			if (e.Row == null && IsVirtualTable(sender.BqlTable))
            {
                e.ReturnValue = null;
                e.ReturnState = null;
                e.Cancel = true;
                return;
            }

            Guid? id = null;
            if (e.Row != null)
            {
                id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
            }
            if (id != null)
            {
                //Set NoteID to the session to allow work with file even if user hasn't enough access rights
                PXContext.Session.SetString(string.Format("{0}+{1}", typeof(Note).FullName, id), id.ToString());

                int? retVal = sender._GetOriginalCounts(e.Row as IBqlTable).Item2;
                if (retVal == 0)
                {
                    id = null;
                }
                if (id != null)
                {
                    List<string> ret = new List<string>();
                    foreach (NoteDoc doc in GetDocView(sender.Graph).SelectMulti(id))
                    {
                        if (doc.FileID != null)
                        {
                            Guid fileId = doc.FileID.Value;
                            var result = (PXResult<UploadFile, UploadFileRevisionNoData>)GetFileByID(sender.Graph).SelectSingle(fileId);
                            UploadFile file = (UploadFile)result;
                            UploadFileRevision fileRev = (UploadFileRevision)result;
                            if (file != null && fileRev != null && !string.IsNullOrEmpty(file.Name))
                            {
                                string size = fileRev.Size.HasValue ? fileRev.Size.Value.ToString() : string.Empty;
                                string comment = fileRev.Comment != null ? TextUtils.EscapeString(fileRev.Comment, '@', '$') : string.Empty;
                                ret.Add(fileId + "$" + file.Name + "$" + comment + "$" + size + "$" + fileRev.CreatedDateTime.Value.ToFileTimeUtc());
                            }
                        }
                    }
                    if (ret.Count > 0)
                    {
                        e.ReturnValue = ret.ToArray();
                    }
                    SetFilesExists(sender, e.Row, ret.Count);
                }
            }
            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                e.ReturnState = PXNoteState.CreateInstance(e.ReturnState, _NoteTextField, _FieldName, _NoteTextField, _NoteFilesField, _NoteActivityField, _NoteTextExistsField, _NoteFilesCountField);
            }
        }
        /// <exclude/>
		public static string UnescapeComment(string comment)
        {
            return TextUtils.UnEscapeString(comment, '@', '$');
        }
        /// <exclude/>
		public virtual void noteImagesFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            Guid? id = null;
            if (e.Row != null)
            {
                id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
            }
            if (id != null)
            {
                int? retVal = sender._GetOriginalCounts(e.Row as IBqlTable).Item2;
                if (retVal == 0)
                {
                    id = null;
                }
                if (id != null)
                {

                    List<Guid> ret = new List<Guid>();
                    var docs = GetDocView(sender.Graph).SelectMulti(id);
                    if (docs.Count > 0)
                    {
                        var imageExts = new HashSet<string>(SitePolicy.AllowedImageTypesExt.Select(ext => ext.TrimStart('.')), StringComparer.OrdinalIgnoreCase);
                        foreach (NoteDoc doc in docs)
                        {
                            if (doc.FileID != null)
                            {
                                PX.SM.UploadFile file = GetFileByID(sender.Graph).SelectSingle(doc.FileID.Value) as PX.SM.UploadFile;
                                if (file != null && !string.IsNullOrEmpty(file.Name) && (imageExts.Count == 0 || imageExts.Contains(file.Extansion)))
                                {
                                    ret.Add((Guid)file.FileID);
                                }
                            }
                        }
                    }
                    if (ret.Count > 0)
                    {
                        e.ReturnValue = ret.ToArray();
                    }
                }
            }
            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                e.ReturnState = PXNoteState.CreateInstance(e.ReturnState, _NoteTextField, _FieldName, _NoteTextField, _NoteFilesField, _NoteActivityField, _NoteTextExistsField, _NoteFilesCountField);
            }
        }

        /// <exclude/>
		public virtual void noteFilesFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            Guid[] docs = e.NewValue as Guid[];
            if (docs != null && e.Row != null)
            {
                Guid? id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);

                sender._ResetOriginalCounts((IBqlTable)e.Row, false, true, false);

                if (_PassThrough || !sender.AllowUpdate && sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
                {
                    using (PXTransactionScope tran = new PXTransactionScope())
                    {
                        if (id == null)
                        {
                            id = GenerateId();
                            InsertNote(sender, id.Value, String.Empty);
                            sender.SetValue(e.Row, _FieldOrdinal, id);
                            updateTableWithId(sender, e.Row, id);
                        }

                        UpdateNote(sender, id.Value);
                        sender.Graph.Caches[typeof(Note)].ClearQueryCache();

                        foreach (Guid did in docs)
                        {
                            PXDatabase.Delete(typeof(NoteDoc),
                                new PXDataFieldRestrict(typeof(NoteDoc.noteID).Name, PXDbType.UniqueIdentifier, id),
                                new PXDataFieldRestrict(typeof(NoteDoc.fileID).Name, PXDbType.UniqueIdentifier, did));

                            try
                            {
                                PXDatabase.Insert(typeof(NoteDoc),
                                    new PXDataFieldAssign(typeof(NoteDoc.noteID).Name, PXDbType.UniqueIdentifier, id),
                                    new PXDataFieldAssign(typeof(NoteDoc.fileID).Name, PXDbType.UniqueIdentifier, did),
                                    PXDataFieldAssign.OperationSwitchAllowed
                                    );
                            }
                            catch (PXDbOperationSwitchRequiredException)
                            {
                                PXDatabase.Update<NoteDoc>(
                                    new PXDataFieldAssign(typeof(NoteDoc.noteID).Name, PXDbType.UniqueIdentifier, id),
                                    new PXDataFieldAssign(typeof(NoteDoc.fileID).Name, PXDbType.UniqueIdentifier, did),
                                    new PXDataFieldRestrict(typeof(NoteDoc.noteID).Name, PXDbType.UniqueIdentifier, id),
                                    new PXDataFieldRestrict(typeof(NoteDoc.fileID).Name, PXDbType.UniqueIdentifier, did)
                                    );
                            }

                            string screenID = PX.Common.PXContext.GetScreenID();
                            if (!String.IsNullOrEmpty(screenID))
                                PX.SM.UploadFileMaintenance.SetAccessSource(did, null, screenID.Replace(".", ""));
                        }
                        if (docs.Length > 0 && !PXTimeTagAttribute.SyncScope.IsScoped())
                        {
                            PXDBTimestampAttribute tstampAttribute = sender.GetAttributes(e.Row, null).FirstOrDefault(a => a is PXDBTimestampAttribute) as PXDBTimestampAttribute;
                            foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(e.Row, null))
                            {
                                if (attr is PXTimeTagAttribute)
                                {
                                    Guid? noteid = PXNoteAttribute.GetNoteID(sender, e.Row, null);
                                    if (noteid != null)
                                    {
                                        List<PXDataFieldParam> list = new List<PXDataFieldParam>();
                                        list.Add(new PXDataFieldAssign<SyncTimeTag.timeTag>(PXDbType.DateTime, DateTime.UtcNow));
                                        list.Add(new PXDataFieldRestrict<SyncTimeTag.noteID>(PXDbType.UniqueIdentifier, noteid));

                                        PXDatabase.Update<SyncTimeTag>(list.ToArray());
                                    }

                                    #region Old Entity Column Update. Need review and remove.
#if false
									List<PXDataFieldParam> list = new List<PXDataFieldParam>();
									list.Add(new PXDataFieldAssign(attr.FieldName, PXDbType.DateTime, DateTime.UtcNow));
									foreach (String key in sender.Keys)
									{
										PXCommandPreparingEventArgs.FieldDescription descr;
										sender.RaiseCommandPreparing(key, e.Row, sender.GetValue(e.Row, key), PXDBOperation.Update, sender.GetItemType(), out descr);
										if (descr != null && descr.Expr != null)
										{
											list.Add(new PXDataFieldRestrict(descr.FieldName, descr.DataType, descr.DataLength, descr.DataValue));
										}
									}

									if (list.Count > 1)
									{
										Boolean wasUpdated = false;
										if (tstampAttribute != null)
										{
											PXCommandPreparingEventArgs.FieldDescription tstampDescr;
											sender.RaiseCommandPreparing(tstampAttribute.FieldName, e.Row, sender.GetValue(e.Row, tstampAttribute.FieldName), PXDBOperation.Update, sender.GetItemType( ), out tstampDescr);
											PXDataFieldRestrict tstampRestriction = new PXDataFieldRestrict(tstampAttribute.FieldName, tstampDescr.DataType, tstampDescr.DataLength, tstampDescr.DataValue);

											list.Add(tstampRestriction);
											if (wasUpdated = PXDatabase.Update(sender.GetItemType( ), list.ToArray( )))
												sender.Graph.SelectTimeStamp( );
											list.Remove(tstampRestriction);
										}

										if(!wasUpdated) PXDatabase.Update(sender.GetItemType( ), list.ToArray( ));
									}
#endif
                                    #endregion
                                }
                            }

                            sender.Graph.Caches[typeof(NoteDoc)].ClearQueryCache();
                        }
                        tran.Complete();
                    }
                }
                else
                {
                    if (id == null)
                    {
                        PXCache cache = sender.Graph.Caches[typeof(Note)];
                        Note note = cache.Insert() as Note;
                        if (note != null)
                        {
                            note.NoteText = String.Empty;
                            note.EntityType = GetEntityType(sender, note.NoteID);
                            note.GraphType = GetGraphType(sender);
                            id = note.NoteID;
                            sender.SetValue(e.Row, _FieldOrdinal, id);
                        }
                    }
                    if (id != null)
                    {
                        PXCache dcache = sender.Graph.Caches[typeof(NoteDoc)];
                        PXResultset<NoteDoc> resultset = PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(sender.Graph, id);
                        Dictionary<Guid, NoteDoc> existing = new Dictionary<Guid, NoteDoc>(resultset.Count);
                        bool washere = false;
                        foreach (NoteDoc nd in resultset)
                            existing[nd.FileID.Value] = nd;

                        foreach (Guid docID in docs)
                        {
                            if (existing.ContainsKey(docID))
                                continue;

                            NoteDoc doc = new NoteDoc();
                            doc.NoteID = id;
                            doc.FileID = docID;
                            dcache.Insert(doc);
                            washere = true;
                        }
                        if (washere)
                        {
							sender.MarkUpdated(e.Row);
                            sender.IsDirty = true;
                        }
                    }
                }
            }
            else if (e.Row == null)
            {
                _FilesRequired = true;
                PXCache cache = sender.Graph.Caches[_BqlTable];
                if (cache != sender)
                {
                    object val = null;
                    cache.RaiseFieldUpdating(_NoteFilesField, null, ref val);
                }
            }
        }
        /// <exclude/>
		public virtual void noteActivityFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            Guid? id = null;
            if (e.Row != null)
                id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);

            e.ReturnValue = null;
            if (id.HasValue && id != Guid.Empty)
            {
                int count = PX.Data.EP.ActivityService.GetCount(id);
                e.ReturnValue = count;
                SetActivitiesFound(sender, e.Row, count);
            }

            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                e.ReturnState = PXNoteState.CreateInstance(e.ReturnState, _NoteTextField, _FieldName, _NoteTextField, _NoteFilesField, _NoteActivityField, _NoteTextExistsField, _NoteFilesCountField);
            }
        }

		protected virtual void InsertNote(PXCache sender, Guid id, string text = "")
			=> PXNoteAttribute.InsertNoteRecord(sender, GetEntityType(sender, id), id, text);

		/// <exclude/>
		public static void InsertNoteRecord(PXCache sender, Guid id, string text = "")
		{
			var noteAttribute = sender.GetAttributesReadonly(null).OfType<PXNoteAttribute>().FirstOrDefault();
			if (noteAttribute != null)
				noteAttribute.InsertNote(sender, id, text);
			else
				PXNoteAttribute.InsertNoteRecord(sender, sender.GetItemType().FullName, id, text);
		}

		/// <exclude/>
		public static void InsertNoteRecord(PXCache sender, string entityType, Guid id, string text = "")
        {
			try
			{
                PXDatabase.Insert(typeof(Note),
                    new PXDataFieldAssign<Note.noteText>(PXDbType.NVarChar, 0, text),
                    new PXDataFieldAssign<Note.entityType>(PXDbType.VarChar, entityType),
                    new PXDataFieldAssign<Note.graphType>(PXDbType.VarChar, GetGraphType(sender)),
                    new PXDataFieldAssign<Note.noteID>(PXDbType.UniqueIdentifier, id),
                PXDataFieldAssign.OperationSwitchAllowed);
            }
            catch (PXVisibiltyUpdateRequiredException)
            {
                PXDatabase.Update<Note>(
                    new PXDataFieldAssign<Note.noteText>(PXDbType.NVarChar, 0, text),
                    new PXDataFieldAssign<Note.entityType>(PXDbType.VarChar, entityType),
                    new PXDataFieldAssign<Note.graphType>(PXDbType.VarChar, GetGraphType(sender)),
                    new PXDataFieldRestrict<Note.noteID>(PXDbType.UniqueIdentifier, id)
                );
            }
        }

		protected virtual void UpdateNote(PXCache sender, Guid id, string text = null, string popupText = null)
			=> PXNoteAttribute.UpdateNoteRecord(sender, GetEntityType(sender, id), id, text, popupText);

		/// <exclude/>
		public static void UpdateNoteRecord(PXCache sender, Guid id, string text = "", string popupText = null)
		{
			var noteAttribute = sender.GetAttributesReadonly(null).OfType<PXNoteAttribute>().FirstOrDefault();
			if (noteAttribute != null)
				noteAttribute.UpdateNote(sender, id, text);
			else
				PXNoteAttribute.UpdateNoteRecord(sender, sender.GetItemType().FullName, id, text, popupText);
		}

        /// <exclude/>
		public static void UpdateNoteRecord(PXCache sender, string entityType, Guid id, String text = null, String popupText = null)
        {
            Note note = new Note { NoteID = id };
            if (sender.Graph.Caches[typeof(Note)].GetStatus(note) == PXEntryStatus.Inserted)
            {
                return;
            }
			bool haveToInsert = false;
            try
            {
                List<PXDataFieldParam> paramz = new List<PXDataFieldParam>() {
                    new PXDataFieldAssign<Note.entityType>(PXDbType.VarChar, entityType),
                    new PXDataFieldAssign<Note.graphType>(PXDbType.VarChar, GetGraphType(sender)),
                    new PXDataFieldRestrict<Note.noteID>(PXDbType.UniqueIdentifier, id),
                    PXDataFieldRestrict.OperationSwitchAllowed
                };
                if (text != null)
                    paramz.Insert(0, new PXDataFieldAssign<Note.noteText>(PXDbType.NVarChar, text));
                if (popupText != null)
                    paramz.Add(new PXDataFieldAssign<Note.notePopupText>(PXDbType.VarChar, popupText));
                bool updated = PXDatabase.Update(typeof(Note), paramz.ToArray());
                if (!updated)
                    haveToInsert = true;
            }
            catch (PXInsertSharedRecordRequiredException)
            {
                haveToInsert = true;
            }
            if (haveToInsert)
                PXDatabase.Insert<Note>(
                    new PXDataFieldAssign<Note.noteText>(PXDbType.NVarChar, 0, text ?? ""),
                    new PXDataFieldAssign<Note.entityType>(PXDbType.VarChar, entityType),
                    new PXDataFieldAssign<Note.graphType>(PXDbType.VarChar, GetGraphType(sender)),
					new PXDataFieldAssign<Note.noteID>(PXDbType.UniqueIdentifier, id),
                    new PXDataFieldAssign<Note.notePopupText>(PXDbType.NVarChar, 0, popupText ?? "")
                );
        }


        /// <exclude/>
		public virtual void noteActivityFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (e.Row != null)
            {
                Guid? id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
                if (_PassThrough || !sender.AllowUpdate && sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
                {
                    using (PXTransactionScope tran = new PXTransactionScope())
                    {
                        if (id == null)
                        {
                            id = GenerateId();
                            InsertNote(sender, id.Value, String.Empty);

                            sender.SetValue(e.Row, _FieldOrdinal, id);
                            updateTableWithId(sender, e.Row, id);
                        }

                        UpdateNote(sender, id.Value);

                        tran.Complete();
                    }
                }
                else
                {
                    if (id == null)
                    {
                        PXCache cache = sender.Graph.Caches[typeof(Note)];
                        Note note = cache.Insert() as Note;
                        if (note != null)
                        {
                            note.NoteText = String.Empty;
                            note.EntityType = GetEntityType(sender, note.NoteID);
                            note.GraphType = GetGraphType(sender);
                            id = note.NoteID;
                            sender.SetValue(e.Row, _FieldOrdinal, id);
                        }
                    }
                    if (id != null)
                    {
						sender.MarkUpdated(e.Row);
                        sender.IsDirty = true;
                    }
                }
            }
            else if (e.Row == null)
            {
                _ActivityRequired = true;
                PXCache cache = sender.Graph.Caches[_BqlTable];
                if (cache != sender)
                {
                    object val = null;
                    cache.RaiseFieldUpdating(_NoteFilesField, null, ref val);
                }
            }
        }

        /// <summary>Gets or set the field whose value will be displayed as value
        /// in the lookup that selects the related data record for an
        /// activity.</summary>
		public Type DescriptionField { get; set; }

        /// <summary>Gets or sets the BQL expression that selects the data records
        /// to be displayed in the pop-up window of the lookup that selects the
        /// related data record for an activity. As the BQL expression, you can
        /// specify a <tt>Search&lt;&gt;</tt> command or just a field. This field,
        /// or the main field of the <tt>Search&lt;&gt;</tt> command, will be the
        /// value that identifies a data record in the activity item.</summary>
		public Type Selector { get; set; }

        /// <summary>Gets or set the list of columns that will be displayed in the
        /// pop-up window of the lookup that selects the related data record for
        /// an activity.</summary>
		public Type[] FieldList { get; set; }


        public static IEnumerable<Type> PXNoteTypes
        {
            get
            {
                foreach (Type type in ServiceManager.Tables)
                {
                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        if (prop.DeclaringType == type && prop.IsDefined(typeof(PXNoteAttribute), false))
                            yield return type;
                    }
                }

                var extensions = PXCache.FindExtensionTypes().Where(_ => PXCache.IsActiveExtension(_)).ToArray();
                foreach (Type type in extensions)
                {
                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        if (prop.DeclaringType == type && prop.IsDefined(typeof(PXNoteAttribute), false))
                            yield return type;
                    }
                }
            }
        }

        protected virtual void noteTextCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            NoteTextGenericCommandPreparing(sender, e, _TextRequired, _NoteTextField);
		}

        protected virtual void notePopupTextCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if (PopupTextEnabled)
            {
                NoteTextGenericCommandPreparing(sender, e, _PopupTextRequired, NotePopupTextField);
            }
        }

        private void NoteTextGenericCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e, bool textRequired, string noteTextField)
        {
            var subqueryKey = GetValueForSubquery(sender, e, textRequired, false);
            if (null != subqueryKey)
            {
				Query q = new Query();
				SimpleTable noteDoc = new SimpleTable("Note");
				q.Field(new Column(noteTextField, noteDoc)).From(noteDoc).Where(new Column("NoteId", noteDoc).EQ(GetExpressionForSubquery(sender, e, _TextRequired, false)));
				e.Expr = new SubQuery(q).Embrace();
            }
            else
            {
				e.Expr = SQLExpression.Null();
            }
        }

        protected virtual void noteActivitiesCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            bool tableFound = !SuppressActivitiesCount && null != PXDatabase.Provider.SchemaCache.GetTableHeader("CRActivity");
            if (tableFound)
            {
                var subqueryKey = GetValueForSubquery(sender, e, _ActivityRequired, true);
                if (null != subqueryKey)
                {

					SQLExpression keyExpr = GetExpressionForSubquery(sender, e, _ActivityRequired, true);
					Query q = new Query();
					SimpleTable act = new SimpleTable("CRActivity");
					q.Field(SQLExpression.Count()).From(act).Where(
						(ActivitiesCountByParent
						? new Column("BAccountID", act).EQ(keyExpr)
						: new Column("RefNoteID", act).EQ(keyExpr)).And(
						new Column("UIStatus", act).NE("CL").And(
						new Column("UIStatus", act).NE("RL")))
					);
					e.Expr = new SubQuery(q);
                }
                else
                {
					e.Expr = SQLTree.SQLExpression.Null();
                }
            }
            else
            {
				e.Expr = SQLTree.SQLExpression.Null();
            }
        }
        private YaqlScalar GetValueForSubquery(PXCache sender, PXCommandPreparingEventArgs e, bool fieldRequired, bool forCRActivity)
        {

            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && fieldRequired)
            {
                // When this field becomes a subquery, use the external table name to look up NoteID
                string tableName = null;
                if ((e.Operation & PXDBOperation.Option) == PXDBOperation.External)
                    tableName = sender.GetItemType().Name;

                if (!_BqlTable.IsAssignableFrom(sender.BqlTable))
                {
                    if (sender.Graph.Caches[_BqlTable].BqlSelect != null &&
                        ((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
                            (e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null))
                    {
                        e.Cancel = true;
                        e.DataType = PXDbType.NVarChar;
                        e.DataValue = e.Value;
                        e.BqlTable = _BqlTable;
                        return Yaql.column(_DatabaseFieldName, tableName ?? _BqlTable.Name);
                    }
                    else
                    {
                        PXCommandPreparingEventArgs.FieldDescription description;
                        e.Cancel = !sender.Graph.Caches[_BqlTable].RaiseCommandPreparing(_DatabaseFieldName, e.Row, e.Value, e.Operation, e.Table, out description);
                        if (description != null)
                        {
                            e.DataType = description.DataType;
                            e.DataValue = description.DataValue;
                            e.BqlTable = _BqlTable;
                            return Yaql.raw(description.Expr.SQLQuery(sender.Graph.SqlDialect.GetConnection()).ToString());
                        }
                    }
                }
                else if (((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
                            (e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null))
                {
                    e.Cancel = true;
                    e.DataType = PXDbType.NVarChar;
                    e.DataValue = e.Value;
                    e.BqlTable = _BqlTable;

                    if (forCRActivity && ActivitiesCountByParent)
                    {
                        string field = EntityHelper.GetIDField(sender.GetItemType());
                        return Yaql.column(field, tableName ?? (e.Table != null ? e.Table.Name : _BqlTable.Name));
                    }
                    else
                    {
                        return Yaql.column(_DatabaseFieldName, tableName ?? (e.Table != null ? e.Table.Name : _BqlTable.Name));
                    }
                }
            }
            return null;
        }

		private Column GetExpressionForSubquery(PXCache sender, PXCommandPreparingEventArgs e, bool fieldRequired, bool forCRActivity) {
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && fieldRequired) {
				// When this field becomes a subquery, use the external table name to look up NoteID
				Type table = null;
				if ((e.Operation & PXDBOperation.Option) == PXDBOperation.External)
					table = sender.GetItemType();

				if (!_BqlTable.IsAssignableFrom(sender.BqlTable)) {
					if (sender.Graph.Caches[_BqlTable].BqlSelect != null &&
						((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
							(e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null)) {
						e.Cancel = true;
						e.DataType = PXDbType.NVarChar;
						e.DataValue = e.Value;
						e.BqlTable = _BqlTable;
						return new Column(_DatabaseFieldName, new SimpleTable(table ?? _BqlTable));
					}
					else {
						PXCommandPreparingEventArgs.FieldDescription description;
						e.Cancel = !sender.Graph.Caches[_BqlTable].RaiseCommandPreparing(_DatabaseFieldName, e.Row, e.Value, e.Operation, e.Table, out description);
						if (description != null) {
							e.DataType = description.DataType;
							e.DataValue = description.DataValue;
							e.BqlTable = _BqlTable;
							return new Column((description.Expr as Column).Name, new SimpleTable(_BqlTable));
						}
					}
				}
				else if (((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
							(e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null)) {
					e.Cancel = true;
					e.DataType = PXDbType.NVarChar;
					e.DataValue = e.Value;
					e.BqlTable = _BqlTable;

					if (forCRActivity && ActivitiesCountByParent) {
						string field = EntityHelper.GetIDField(sender.GetItemType());
						return new Column(field, new SimpleTable(table ?? (e.Table ?? _BqlTable)));
					}
					else {
						return new Column(_DatabaseFieldName, new SimpleTable(table ?? (e.Table ?? _BqlTable)));
					}
				}
			}
			return null;
		}

        protected virtual void noteFilesCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            var subqueryKey = GetValueForSubquery(sender, e, _FilesRequired, false);
            if (null != subqueryKey)
            {
				Query q=new Query();
				SimpleTable noteDoc=new SimpleTable("NoteDoc");
				q.Field(SQLExpression.Count()).From(noteDoc).Where(new Column("NoteId", noteDoc).EQ(GetExpressionForSubquery(sender, e, _FilesRequired, false)));
				e.Expr = new SubQuery(q).Embrace();
            }
            else
            {
				e.Expr = SQLExpression.Null();
            }
        }

        /// <exclude/>
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            base.CommandPreparing(sender, e);
            if ((e.Operation & PXDBOperation.Option) == PXDBOperation.Internal && e.Expr != null)
            {
                e.BqlTable = _BqlTable;
				e.Expr = new NoteIdExpression(e.Expr, PopupTextEnabled);
			}
        }

        /// <exclude/>
		public void SetFilesExists(PXCache sender, object row, int? count)
        {
            if (!(row is IBqlTable)) return;
			sender._SetOriginalCounts((IBqlTable)row, null, count, null, null);
        }

        /// <exclude/>
        public void SetActivitiesFound(PXCache sender, object row, int? count)
        {
            if (!(row is IBqlTable)) return;
			sender._SetOriginalCounts((IBqlTable)row, null, null, count, null);
        }

        /// <exclude/>
		public void SetNoteTextExists(PXCache sender, object row, string text)
        {
            if (!(row is IBqlTable)) return;
			sender._SetOriginalCounts((IBqlTable)row, text, null, null, null);
		}

        /// <exclude/>
        public void SetPopupNoteTextExists(PXCache sender, object row, string text)
        {
            if (!(row is IBqlTable)) return;
            sender._SetOriginalCounts((IBqlTable)row, null, null, null, text);
        }

        /// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            base.RowSelecting(sender, e);
            if (e.Row != null)
            {
                Guid? id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
                string text = e.Record.GetString(e.Position);
                e.Position++;
                int? count = e.Record.GetInt32(e.Position);
                e.Position++;
                int? aCount = e.Record.GetInt32(e.Position);
                e.Position++;

                string popupText = null;
                if (PopupTextEnabled)
                {
                    popupText = e.Record.GetString(e.Position);
                    e.Position++;
                }

				if (!id.HasValue || (text == null && count == null && (!PopupTextEnabled || popupText == null))) return;

                if (!String.IsNullOrEmpty(text))
                {
                    string[] split = text.Split('\0');
                    if (split.Length > 0)
                    {
                        text = split[0];
                    }
                }
				else if (text == null && (count != null || popupText != null))
                {
                    text = "";
                }

                if (!string.IsNullOrEmpty(popupText))
                {
                    string[] split = popupText.Split('\0');
                    if (split.Length > 0)
                    {
                        popupText = split[0];
                    }
                }
                else if (PopupTextEnabled && popupText == null && (text != null || count != null))
                {
                    popupText = "";
                }

                SetNoteTextExists(sender, e.Row, text);
                SetFilesExists(sender, e.Row, count);
                SetActivitiesFound(sender, e.Row, aCount);

                if (PopupTextEnabled)
                {
                    SetPopupNoteTextExists(sender, e.Row, popupText);
                }
            }
            else
            {
				e.Position += PopupTextEnabled ? 4 : 3;
            }
        }

        /// <exclude />
        public static void AttachFile(PXCache sender, object data, PX.SM.FileInfo fileInfo)
        {
            string prefix = string.Empty;
            //string fileName = fileId.ToString();
            string screenID = PX.Common.PXContext.GetScreenID();
            PXSiteMapNode node;
            if (!String.IsNullOrEmpty(screenID) && (node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID.Replace(".", ""))) != null && !String.IsNullOrEmpty(node.Title))
            {
                prefix = node.Title + "\\";

                var values = sender.Keys.Select(key => sender.GetValue(data, key)).WhereNotNull();
                prefix += String.Join(" ", values);
                if (!string.IsNullOrEmpty(prefix))
                {
                    PXDatabase.Update<PX.SM.UploadFile>(new PXDataFieldAssign<PX.SM.UploadFile.name>(PXDbType.NVarChar, 255, prefix + "\\" + fileInfo.Name), new PXDataFieldRestrict<PX.SM.UploadFile.fileID>(PXDbType.UniqueIdentifier, 16, fileInfo.UID.Value));
                    sender.SetValueExt(data, _NoteFilesField, new Guid[] { fileInfo.UID.Value });
                }
            }
        }
        #endregion

        #region Initialization
        /// <exclude/>
        public override void CacheAttached(PXCache sender)
        {
            sender._NoteIDOrdinal = _FieldOrdinal;
            sender._NoteIDName = _FieldName;

            base.CacheAttached(sender);

            sender.Graph.Views.Caches.Remove(typeof(Note));
            if (!sender.Graph.Views.RestorableCaches.Contains(typeof(Note)))
                sender.Graph.Views.RestorableCaches.Add(typeof(Note));

            int fieldIdx = sender.Fields.IndexOf(_FieldName);
            var propertyInfo = sender.GetItemType().GetProperty(_FieldName);

            if (propertyInfo != null)
            {
                _declaringType = propertyInfo.DeclaringType.FullName;
            }
            else
            {
                foreach (Type extension in sender.GetExtensionTypes())
                {
                    PropertyInfo extPropInfo = extension.GetProperty(_FieldName);
                    if (extPropInfo != null)
                    {
                        var extensionType = extPropInfo.DeclaringType;
                        if (extensionType.BaseType != null && extensionType.BaseType.IsGenericType)
                        {
                            _declaringType = extensionType.BaseType.GetGenericArguments().Last().FullName;
                        }
                        break;
                    }
                }
            }

            sender.Fields.Insert(++fieldIdx, _NoteTextField);
            string field = _NoteTextField.ToLower();
            sender.FieldSelectingEvents[field] += noteTextFieldSelecting;
            sender.FieldUpdatingEvents[field] += noteTextFieldUpdating;
            sender.CommandPreparingEvents[field] += noteTextCommandPreparing;
            // PXCache cache = sender.Graph.Caches[typeof(Note)];

            sender.Fields.Insert(++fieldIdx, _NoteFilesField);
            field = _NoteFilesField.ToLower();
            sender.FieldSelectingEvents[field] += noteFilesFieldSelecting;
            sender.FieldUpdatingEvents[field] += noteFilesFieldUpdating;
            sender.CommandPreparingEvents[field] += noteFilesCommandPreparing;

            sender.Fields.Insert(++fieldIdx, _NoteImagesField);
            sender.FieldSelectingEvents[_NoteImagesField.ToLower()] += noteImagesFieldSelecting;

            sender.Fields.Insert(++fieldIdx, _NoteActivityField);
            field = _NoteActivityField.ToLower();
            sender.FieldSelectingEvents[field] += noteActivityFieldSelecting;
            sender.FieldUpdatingEvents[field] += noteActivityFieldUpdating;
            sender.CommandPreparingEvents[field] += noteActivitiesCommandPreparing;
            // cache = sender.Graph.Caches[typeof(NoteDoc)];

            sender.Fields.Insert(++fieldIdx, _NoteTextExistsField);
            sender.FieldSelectingEvents[_NoteTextExistsField.ToLower()] += noteTextExistsFieldSelecting;

            sender.Fields.Insert(++fieldIdx, _NoteFilesCountField);
            sender.FieldSelectingEvents[_NoteFilesCountField.ToLower()] += noteFilesCountFieldSelecting;

            sender.Fields.Insert(++fieldIdx, _NoteActivitiesCountField);
            sender.FieldSelectingEvents[_NoteActivitiesCountField.ToLower()] += noteActivitiesCountFieldSelecting;

            if (PopupTextEnabled)
            {
                sender.Fields.Insert(++fieldIdx, NotePopupTextField);
                field = NotePopupTextField.ToLower();
                sender.FieldSelectingEvents[field] += notePopupTextFieldSelecting;
                sender.FieldUpdatingEvents[field] += notePopupTextFieldUpdating;
                sender.CommandPreparingEvents[field] += notePopupTextCommandPreparing;

                sender.Fields.Insert(++fieldIdx, NotePopupTextExistsField);
                sender.FieldSelectingEvents[NotePopupTextExistsField.ToLower()] += notePopupTextExistsFieldSelecting;
            }

            _NoteTextFieldDisplayName = PXMessages.LocalizeNoPrefix(PX.Data.EP.Messages.NoteTextDisplayName);

            var imagesSelect = new Select<UploadFile>();
            var imagesView = new PXView(sender.Graph, true, new Select<UploadFile>(),
                new PXSelectDelegate(
                    () =>
                    {
                        var list = new List<UploadFile>();
                        var state = sender.GetStateExt(sender.Current, _NoteImagesField) as PXNoteState; //TODO: need review 'sender.Current'
                        Guid[] fileIDs;
                        if (state != null && (fileIDs = state.Value as Guid[]) != null)
                        {
                            var inWhere = InHelper.Create(typeof(UploadFile.fileID), fileIDs.Length);
                            var select = imagesSelect.WhereNew(inWhere);
                            var view = new PXView(sender.Graph, true, select);
                            var parameters = Array.ConvertAll(fileIDs, input => (object)input);
                            foreach (UploadFile file in view.SelectMulti(parameters))
                            {
                                var index = string.IsNullOrEmpty(file.Name) ? -1 : file.Name.LastIndexOf('\\');
                                var copy = file;
                                if (index > -1 && index < file.Name.Length - 1)
                                {
                                    copy = (UploadFile)view.Cache.CreateCopy(file);
                                    copy.Name = file.Name.Substring(index + 1);
                                }
                                list.Add(copy);
                            }
                        }
                        return list;
                    }));
            var imagesViewName = _NoteImagesViewPrefix + sender.GetItemType().Name;
            sender.Graph.Views.Add(imagesViewName, imagesView);

            var fileSelect = new Select2<UploadFile, InnerJoin<NoteDoc, On<UploadFile.fileID, Equal<NoteDoc.fileID>>,
                                                    InnerJoin<UploadFileRevisionNoData, On<UploadFile.fileID, Equal<UploadFileRevisionNoData.fileID>,
                                                        And<UploadFileRevisionNoData.fileRevisionID, Equal<UploadFile.lastRevisionID>>>>>,
                                                    Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>();
            var filesView = new PXView(sender.Graph, true, fileSelect);
            sender.Graph.Views.Add(fileListView, filesView);
        }
        internal const string fileListView = "fileListView";

		public static void ResetFileListCache(PXCache sender)
		{
			sender.Graph.Views[fileListView].Cache.ClearQueryCache();

			var notedocView = new PXView(sender.Graph, false, new Select<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>());
			notedocView.Cache.ClearQueryCache();

			if (null != sender.Current)
				sender._ResetOriginalCounts(sender.Current, false, true, false);
		}

        private void noteActivitiesCountFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
			if (e.Row == null && IsVirtualTable(sender.BqlTable))
            {
                e.ReturnValue = null;
                e.ReturnState = null;
                e.Cancel = true;
                return;
            }

            Guid? id = null;
            if (e.Row != null)
            {
                id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
            }

            if (!id.HasValue) return;

            int? retVal = sender._GetOriginalCounts(e.Row as IBqlTable).Item3;
            if (retVal == null)
            {
                bool tableFound = null != PXDatabase.Provider.SchemaCache.GetTableHeader("CRActivity");
                if (tableFound)
                    SetActivitiesFound(sender, e.Row, retVal = GetActivityCount(sender.Graph, id));
            }

            e.ReturnValue = retVal ?? 0;
        }

        private void noteFilesCountFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
			if (e.Row == null && IsVirtualTable(sender.BqlTable))
            {
                e.ReturnValue = null;
                e.ReturnState = null;
                e.Cancel = true;
                return;
            }

            Guid? id = null;
            if (e.Row != null)
            {
                id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
            }

            if (!id.HasValue) return;

            int? retVal = sender._GetOriginalCounts(e.Row as IBqlTable).Item2;
            if (retVal == null)
            {
                SetFilesExists(sender, e.Row, GetDocView(sender.Graph).SelectMulti(id).Count());
                retVal = sender._GetOriginalCounts(e.Row as IBqlTable).Item2;
            }

            e.ReturnValue = retVal ?? 0;
        }

        private readonly Func<PXCache, IBqlTable, string> GetOriginalCountsItem = (c, t) => c._GetOriginalCounts(t).Item1;
        private readonly Func<PXCache, IBqlTable, string> GetPopupOriginalCountsItem = (c, t) => c._GetOriginalCounts(t).Item4;

        private void noteTextExistsFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            Func<Note, string> getNoteText = n => n.NoteText;

            NoteTextExistsGenericFieldSelecting(sender, e, GetOriginalCountsItem, getNoteText, SetNoteTextExists);
		}

        private void notePopupTextExistsFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (PopupTextEnabled)
            {
                Func<Note, string> getNoteText = n => n.NotePopupText;

                NoteTextExistsGenericFieldSelecting(sender, e, GetPopupOriginalCountsItem, getNoteText, SetPopupNoteTextExists);
            }
        }

        private void NoteTextExistsGenericFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, Func<PXCache, IBqlTable, string> getOriginalCountsItem,
                                                         Func<Note, string> getNoteText, Action<PXCache, object, string> setNoteTextExists)
        {
			if (e.Row == null && IsVirtualTable(sender.BqlTable))
            {
                e.ReturnValue = null;
                e.ReturnState = null;
                e.Cancel = true;
                return;
            }

            Guid? id = null;
            if (e.Row != null)
            {
                id = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
            }

            if (id.HasValue)
            {
                string result = getOriginalCountsItem(sender, e.Row as IBqlTable);

                if (result == null)
                {
                    Note note = GetNote(sender.Graph, id);
                    string noteText = (note != null ? getNoteText(note) : String.Empty) ?? String.Empty;
                    string[] split = noteText.Split('\0');
                    setNoteTextExists(sender, e.Row, split.Length > 0 ? (split[0] ?? String.Empty) : String.Empty);
                    result = getOriginalCountsItem(sender, e.Row as IBqlTable);
                }

                e.ReturnValue = !String.IsNullOrEmpty(result);
            }
        }

        #endregion
    }

    /// <exclude/>
	public class PXNoteTextAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			String value = e.ReturnValue as String;
			if (String.IsNullOrWhiteSpace(value)) return;

			String[] parts = value.Split('\0');
			if (parts.Length <= 1) return;

			e.ReturnValue = parts[0];
		}
	}

    /// <summary>
    /// Marks a DAC field that holds a reference to a data record
    /// of any DAC type through the note ID.
    /// </summary>
    /// <remarks>
    /// <para>The field marked with the <tt>PXRefNote</tt> attribute is typically
    /// represented by the <tt>PXRefNoteSelector</tt> control on the ASPX page.
    /// Through this control, a user can select a data records from any table.
    /// A data record is referenced through its <tt>NoteID</tt> field, whose
    /// value is written to the field associated with the <tt>PXRefNoteSelector</tt> control
    /// (and marked with the <tt>PXRefNote</tt> attribute). The field should
    /// be of the <tt>Guid?</tt> data type.</para>
    /// <para>In the UI, the <tt>PXRefNoteSelector</tt> control displays the
    /// selected data record's description, which is composed of the fields
    /// that are marked with the
    /// <see cref="PX.Data.EP.PXFieldDescriptionAttribute">PXFieldDescription</see>
    /// attribute.</para>
    /// </remarks>
    /// <seealso cref="PXNoteAttribute"/>
    /// <seealso cref="PX.Data.EP.PXFieldDescriptionAttribute"/>
    /// <example>
    /// The code below shows the usage of the <tt>PXRefNote</tt> attribute in
    /// the defition of a DAC field.
    /// <code>
    /// [PXRefNote]
    /// [PXUIField(DisplayName = "References Nbr.")]
    /// public virtual Guid? RefNoteID { get; set; }
    /// </code>
    /// </example>
	public class PXRefNoteAttribute : PXDBGuidAttribute
	{
        /// <summary>Get, set.</summary>
		public bool FullDescription { get; set; }
		public bool LastKeyOnly { get; set; }

		#region Initialization
        /// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			helper = new EntityHelper(sender.Graph);
		}
		#endregion

		#region Implementation
        /// <exclude/>
        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
	        using (new PXReadBranchRestrictedScope())
	        {
		        Guid? refNoteID = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);
                string entityDescription = string.Empty;

                if (refNoteID.HasValue)
				{
                    entityDescription = 
                        FullDescription
                            ? helper.GetEntityRowValues(refNoteID.Value)
                            : LastKeyOnly
                                 ? helper.GetEntityRowID(refNoteID.Value, separator: null)
                                 : helper.GetEntityRowID(refNoteID.Value);
                }

                e.ReturnValue = PXStringState.CreateInstance(entityDescription, length: null, isUnicode: null, _FieldName, isKey: _IsKey, required: null,
                                                             inputMask: null, allowedValues: null, allowedLabels: null, exclusiveValues: null, 
                                                             defaultValue: null, neutralLabels: null);
            }
		}

        /// <exclude />
        public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.External) == PXDBOperation.External)
                return;

            base.CommandPreparing(sender, e);
        }

        protected EntityHelper helper;
		#endregion
	}

	/// <exclude/>
	public class PXUniqueNoteAttribute : PXNoteAttribute, IPXRowInsertingSubscriber
	{
		public PXUniqueNoteAttribute() { }

		public PXUniqueNoteAttribute(params Type[] searches) : base(searches) { }

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (!this.IsKey)
				sender.SetValue(e.Row, _FieldOrdinal, GenerateId());
		}
	}

	/// <exclude/>
	public class PXSequentialNoteAttribute : PXNoteAttribute
	{
		public PXSequentialNoteAttribute() { }

		public PXSequentialNoteAttribute(params Type[] searches) : base(searches) { }
		
		protected override Guid newGuid()
		{
			return SequentialGuid.Generate();
		}
	}

	/// <exclude/>
	public class PXSequentialSelfRefNoteAttribute : PXSequentialNoteAttribute
	{
		public PXSequentialSelfRefNoteAttribute() { }

		public PXSequentialSelfRefNoteAttribute(params Type[] searches) : base(searches) { }

		public Type NoteField { get; set; }
		
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (NoteField != null && e.NewValue == null)
			{
				e.NewValue = sender.GetValue(e.Row, NoteField.Name);
			}
			else
			{
				base.FieldDefaulting(sender, e);
			}
		}
	}
	/// <exclude/>
	class CRActivity : IBqlTable
	{
	}

    public static class PopupNoteManager
    {
        private const string _slotMessageKey = nameof(PopupNoteManager) + "MessageKey";
        private const string _slotPreserveCopyPasteWarningsKey = nameof(PopupNoteManager) + "PreserveWarningsKey";
        private const string _longRunWarningsKey = nameof(PopupNoteManager) + "LongRunWarningsKey";
		private const string _sessionMessageKey = nameof(PopupNoteManager) + "SessionMessageKey";
        private const string _sessingWarningsKey = nameof(PopupNoteManager) + "SessionWarningsKey";

        private static string GenericMessage
        {
            get
            {
                return PXLocalizer.Localize(Messages.GenericPopupText, typeof(Messages).FullName);
            }
        }

        public static string Message
        {
            get
            {
                return PXContext.GetSlot<string>(_slotMessageKey);
            }
            set
            {
                if (!string.IsNullOrEmpty(PXContext.GetSlot<string>(_slotMessageKey)))
                {
                    PXContext.SetSlot(_slotMessageKey, GenericMessage);
                }
                else
                {
                    PXContext.SetSlot(_slotMessageKey, value);
                }
            }
        }

		public static void StoreMessageForRedirect()
		{
			var message = Message;
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			PXContext.Session.SetString(_sessionMessageKey, message);
		}

		public static void ShowMessageAfterRedirect()
		{
			var message = PXContext.Session[_sessionMessageKey] as string;
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			PXContext.Session.Remove(_sessionMessageKey);
			Message = message;
		}

        internal static void StoreWarningsForRedirect(IEnumerable<PXUIWarningInfo> warnings)
        {
            PXContext.Session[_sessingWarningsKey] = warnings;
        }

        internal static void ShowWarningsAfterRedirect(PXGraph graph)
        {
            var warnings = PXContext.Session[_sessingWarningsKey] as IEnumerable<PXUIWarningInfo>;
            if (warnings == null)
            {
                return;
            }

            var succeeded = ShowWarnings(graph, warnings);
            if (succeeded)
            {
                PXContext.Session[_sessingWarningsKey] = null;
            }
        }

        public static List<PXUIWarningInfo> CopyPasteWarnings
        {
            get
            {
                List<PXUIWarningInfo> warnings = PXContext.GetSlot<List<PXUIWarningInfo>>(_slotPreserveCopyPasteWarningsKey);

                if (warnings == null)
                {
                    warnings = new List<PXUIWarningInfo>();
                    PXContext.SetSlot(_slotPreserveCopyPasteWarningsKey, warnings);
                }

                return warnings;
            }
            set
            {
                PXContext.SetSlot(_slotPreserveCopyPasteWarningsKey, value);
            }
        }

        public static bool PreserveErrors(PXCache cache, string fieldName)
        {
            List<PXUIWarningInfo> warnings = CopyPasteWarnings;

            return warnings != null && warnings.Any(w => w.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase) &&
                                                    w.CacheItemType == cache.GetItemType());
        }

        private static void StoreCopyPasteWarning(PXCache cache, string fieldName)
        {
            CopyPasteWarnings.Add(new PXUIWarningInfo(cache.GetItemType(), null, fieldName, null));
        }

        internal static void StoreLongRunWarning(PXCache cache, object data, string fieldName, string text)
        {
            List<PXUIWarningInfo> warnings = PXLongOperation.GetCustomInfoForCurrentThread(_longRunWarningsKey) as List<PXUIWarningInfo>;

            if (warnings == null)
            {
                warnings = new List<PXUIWarningInfo>();
                PXLongOperation.SetCustomInfoInternal(PXLongOperation.GetOperationKey(), _longRunWarningsKey, warnings);
            }

            warnings.Add(new PXUIWarningInfo(cache.GetItemType(), data, fieldName, text));
        }

        [Obsolete("Use StoreLongRunWarning instead of this method")]
        public static void StoreImportFromExcelWarning(PXCache cache, object data, string fieldName, string text)
        {
            StoreLongRunWarning(cache, data, fieldName, text);
        }

        internal static List<PXUIWarningInfo> GetLongRunWarnings(object key)
        {
            return PXLongOperation.GetCustomInfo(key, _longRunWarningsKey) as List<PXUIWarningInfo>;
        }

        [Obsolete("Use GetLongRunWarning instead of this method")]
        public static List<PXUIWarningInfo> GetImportFromExcelWarning(object key)
        {
            return GetLongRunWarnings(key);
        }

        internal static bool ShowWarnings(PXGraph graph, IEnumerable<PXUIWarningInfo> warnings)
        {
            var warningCount = 0;
            var warningText = default(string);

            foreach (var w in warnings)
            {
                if (!graph.Caches.ContainsKey(w.CacheItemType))
                {
                    continue;
                }

                var cache = graph.Caches[w.CacheItemType];
                var locatedRow = cache.Locate(w.CacheItem);

                if (locatedRow == null)
                {
                    continue;
                }

                PXUIFieldAttribute.SetWarning(cache, locatedRow, w.FieldName, w.WarningText);
                warningText = w.WarningText;
                warningCount++;
            }

            if (warningCount > 1)
            {
                Message = PXLocalizer.Localize(Messages.GenericPopupText, typeof(Messages).FullName);
            }
            else if (!string.IsNullOrWhiteSpace(warningText))
            {
                Message = warningText;
            }

            return warningCount > 0;
        }

        [Obsolete("Use ShowLongRunWarning instead of this method")]
        public static void ShowImportFromExcelWarnings(PXGraph graph, IEnumerable<PXUIWarningInfo> warnings)
        {
            ShowWarnings(graph, warnings);
        }

        private static void ShowPopupWarning(PXCache cache, string fieldName)
        {
            IEnumerable<PXSelectorAttribute> selectors = cache.GetAttributes(fieldName)
                                                         .OfType<PXSelectorAttribute>();
            foreach (PXSelectorAttribute s in selectors)
            {
                s.ShowPopupWarning = true;
            }
        }

        public static void RegisterText(PXCache cache, object data, string fieldName, string text)
        {
            PXGraph graph = cache.Graph;
            bool ignorePopup = string.IsNullOrEmpty(text) || graph.IsContractBasedAPI || (graph.IsImport && !graph.IsCopyPasteContext) ||
                               (graph.IsExport && !graph.IsCopyPasteContext) || graph.IsMobile || PXSiteMap.IsPortal || graph.IsPageGeneratorRequest;
			if (ignorePopup)
			{
				return;
			}

			bool errorTextIsEmpty = string.IsNullOrEmpty(PXUIFieldAttribute.GetError(cache, data, fieldName));

			if (graph.IsImportFromExcel && errorTextIsEmpty)
			{
                StoreLongRunWarning(cache, data, fieldName, text);
            }
			else if (graph.IsCopyPasteContext && errorTextIsEmpty)
			{
				ShowPopupWarning(cache, fieldName);
				StoreCopyPasteWarning(cache, fieldName);

				Message = text;
			}
			else
			{
				if (errorTextIsEmpty)
				{
					ShowPopupWarning(cache, fieldName);
				}

				if (!graph.IsCreatedFromSession)
				{
                    StoreLongRunWarning(cache, data, fieldName, text);
				}

				Message = HttpUtility.HtmlEncode(text);
			}
		}
	}
}
