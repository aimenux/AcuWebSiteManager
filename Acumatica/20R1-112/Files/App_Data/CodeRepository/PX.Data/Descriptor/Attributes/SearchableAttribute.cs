// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2016 All rights reserved.             *
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PX.Api.Soap.Screen;
using PX.BulkInsert.Provider;
using PX.Common;
using PX.Data.Update;
using PX.DbServices.Model.DataSet;
using PX.DbServices.Points.DbmsBase;
using PX.DbServices.Points.PXDataSet;

namespace PX.Data
{
    /// <summary>
    /// Adds fields to the search index and configures the search result.
    /// </summary>
    /// <remarks>This attribute is assigned to the <tt>NoteID</tt> DAC field.
    /// You can make a search in the fields listed in this attribute and
    /// in the key fields. </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class PXSearchableAttribute : PXEventSubscriberAttribute
    {
        protected int category;
        protected Type[] fields;
        protected string titlePrefix;
        protected Type[] titleFields;
        protected PXView searchView;

        /// <summary>
        /// The format of the first line of the search result that is displayed.
        /// Numbers in curly braces reference to the fields listed in <tt>Line1Fields</tt>.
        /// </summary>
        public string Line1Format { get; set; }
        /// <summary>
        /// The format of the second line of the search result that is displayed.
        /// Numbers in curly braces reference to the fields listed in <tt>Line2Fields</tt>.
        /// </summary>
        public string Line2Format { get; set; }
        /// <summary>
        /// The fields that are referenced from <tt>Line1Format</tt>.
        /// </summary>
        public Type[] Line1Fields { get; set; }
        /// <summary>
        /// The fields that are referenced from <tt>Line2Format</tt>.
        /// </summary>
        public Type[] Line2Fields { get; set; }

        /// <summary>
        /// List of the fields to be indexed that contain numbers with prefixes (for example, <tt>"CT00000040"</tt>).
        /// </summary>
        /// <remarks>If a field contains a number with a prefix (like <tt>"CT00000040"</tt>), then searching for <tt>"0040"</tt>
        /// can pose a problem. All fields that are listed in <tt>NumberFields</tt> will get special treatment
        /// and will be indexed both with a prefix and without one (<tt>"CT00000040"</tt>, <tt>"00000040"</tt>).</remarks>
        public Type[] NumberFields { get; set; }

        /// <summary>
        /// The <tt>Select&lt;&gt;</tt> construction that selects the users of the documents
        /// which must be available in the search result.
        /// </summary>
        /// <remarks>If this <tt>Select&lt;&gt;</tt> construction is specified,
        /// then only those records or documents are shown that were
        /// created either by the current user or by a user from the returned set.
        /// The company tree hierarchy is not traversed.
        /// For example, you can use this setting to filter expense claims, timecards, etc.
        /// </remarks>
        public Type SelectDocumentUser { get; set; }

        /// <summary>
        /// Constraint that defines whether the given DAC instance is searchable or not.
        /// </summary>
        /// <remarks>For example, the <tt>Contact</tt> DAC is used to represent different types of records:
        /// the <tt>lead</tt> contacts are searchable, while the <tt>accountProperty</tt> contacts are not.</remarks>
        public Type WhereConstraint { get; set; }

        /// <summary>
        /// When a <tt>MatchWith</tt> type is used, the <tt>MatchWithJoin</tt> property
        /// must contain a join for the entity containing the <tt>GroupMask</tt> column.
        /// </summary>
        /// <remarks>For example, the graph that manages <tt>ARInvoice</tt> objects
        /// contains the following <tt>Match</tt> operator in the <tt>Document</tt> object:
        /// <para><tt>Match&lt;Customer, Current&lt;AccessInfo.userName&gt;&gt;</tt></para>
        /// <para>So the <tt>MatchWithJoin</tt> property must contain a join to the <tt>Customer</tt> table:</para>
        /// <para><tt>typeof(InnerJoin&lt;Customer, On&lt;Customer.bAccountID, Equal&lt;ARInvoice.customerID&gt;&gt;&gt;)</tt></para></remarks>
        public Type MatchWithJoin { get; set; }

        /// <summary>
        /// The <tt>SelectForFastIndexing</tt> request is used to define the relationship
        /// between the searchable fields and thus make it possible to rebuild the Full Text Search index.
        /// </summary>
        /// <remarks>A search can involve additional joined tables, so that all searchable fields
        /// are retrieved by a single select request, and this prevents lazy loading of rows using the selector.</remarks>
        public Type SelectForFastIndexing { get; set; }

        /// <summary>
        /// Initializes the search parameters.
        /// </summary>
        /// <param name="category">The search category. This is one of the integer
        /// constants defined in the <tt>PX.Objects.SM.SearchCategory</tt> class.</param>
        /// <param name="titlePrefix">The format of the search result title.</param>
        /// <param name="titleFields">The fields whose values are used in the search result title.
        /// These fields are referenced from <tt>titlePrefix</tt>.</param>
        /// <param name="fields">The fields for which the index will be built.</param>
        public PXSearchableAttribute(int category, string titlePrefix, Type[] titleFields, Type[] fields)
        {
            this.category = category;
            this.fields = fields;
            this.titleFields = titleFields;
            this.titlePrefix = titlePrefix;
        }

        /// <summary>
        /// Returns all searchable fields including dependent fields and key fields.
        /// </summary>
        /// <remarks>For example, since <tt>Contact.DisplayName</tt> depends on <tt>FirstName</tt>,
        /// <tt>LastName</tt>, and other fields, all these fields will also be returned.</remarks>
        /// <returns>All searchable fields.</returns>
        public ICollection<Type> GetSearchableFields(PXCache cache)
        {
            HashSet<Type> result = new HashSet<Type>();

            foreach (Type item in titleFields.Union(fields))
            {
                result.Add(item);

                foreach (Type dependable in PXDependsOnFieldsAttribute.GetDependsRecursive(cache, item.Name).Select(cache.GetBqlField))
                {
                    result.Add(dependable);
                }

                //Note: Keys can be removed once 43383 is resolved.
                Type dacType = BqlCommand.GetItemType(item);
                foreach (Type key in cache.Graph.Caches[dacType].BqlKeys)
                {
                    result.Add(key);
                }
            }

            if (WhereConstraint != null)
            {
                foreach (Type type in BqlCommand.Decompose(WhereConstraint))
                {
                    if ((typeof(IBqlField)).IsAssignableFrom(type))
                    {
                        result.Add(type);
                    }
                }
            }

            return result;
        }


        private Dictionary<Type, int> isListAttributeTable = new Dictionary<Type, int>();
        private static object _forLock = new object();

        private int IsListAttributeField(PXCache cache, Type field)
        {
            int result;
            lock (((ICollection) isListAttributeTable).SyncRoot)
            {
                if (isListAttributeTable.TryGetValue(field, out result))
                {
                    return result;
                }

                var list = cache.GetAttributes(field.Name);
                int isListAttribute = 0;

                foreach (var attr in list)
                {
                    if (attr is PXStringListAttribute)
                    {
                        isListAttribute = ((PXStringListAttribute) attr).IsLocalizable ? 2 : 1;
                        break;
                    }
                    else if (attr is PXIntListAttribute)
                    {
                        isListAttribute = ((PXIntListAttribute) attr).IsLocalizable ? 2 : 1;
                        break;
                    }
                }

                isListAttributeTable.Add(field, isListAttribute);
                return isListAttribute;
            }
        }

        [PXInternalUseOnly]
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            sender.RowPersisting += sender_RowPersisting;
            sender.RowPersisted += sender_RowPersisted;
        }

        [PXInternalUseOnly]
        public virtual bool IsSearchable(PXCache sender, object row)
        {
            if (WhereConstraint == null)
                return true;

            EnsureSearchView(sender);

            object[] par = searchView.PrepareParameters(new[] { row }, null);

            return searchView.BqlSelect.Meet(sender, row, par);
        }

        protected virtual void EnsureSearchView(PXCache sender)
        {
            if (searchView == null)
            {
                List<Type> list = new List<Type>();
                list.Add(typeof(Select<,>));
                list.Add(sender.GetItemType());
                list.AddRange(BqlCommand.Decompose(WhereConstraint));

                BqlCommand cmd = BqlCommand.CreateInstance(list.ToArray());
                searchView = new PXView(sender.Graph, true, cmd);
            }
        }

        void sender_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            object val = sender.GetValue(e.Row, _FieldOrdinal);
            if (val == null)
            {
                Guid noteID = SequentialGuid.Generate();
                sender.SetValue(e.Row, _FieldOrdinal, noteID);
            }
        }

        void sender_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if (!IsSearchable(sender, e.Row))
                return;

            Guid? noteID = (Guid?) sender.GetValue(e.Row, _FieldOrdinal);

            Dictionary<Guid, SearchIndex> dict = PX.Common.PXContext.GetSlot<Dictionary<Guid, SearchIndex>>("SearchIndexSlot");
            if (dict == null)
            {
                dict = new Dictionary<Guid, SearchIndex>();
                PX.Common.PXContext.SetSlot("SearchIndexSlot", dict);
            }
            SearchIndex si = null;
            if (noteID.HasValue)
            {
                dict.TryGetValue(noteID.Value, out si);

                if (si == null)
                {
                    Note note = PXSelect<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>.SelectSingleBound(sender.Graph, null, noteID);
                    si = BuildSearchIndex(sender, e.Row, null, note != null ? note.NoteText : null);
                    dict[noteID.Value] = si;
                }
            }

            if (e.TranStatus == PXTranStatus.Completed)
            {
                if (noteID == null)
                    throw new PXException(MsgNotLocalizable.SearchIndexCannotBeSaved);

                if (e.Operation == PXDBOperation.Delete)
                {
                    PXDatabase.Delete(typeof(SearchIndex),
                                      new PXDataFieldRestrict(typeof(SearchIndex.noteID).Name, PXDbType.UniqueIdentifier, si.NoteID));
                }
                else
                {
                    if (!Update(si))
                    {
                        Insert(si);
                    }
                }
            }
        }

        [PXInternalUseOnly]
        public static bool Insert(SearchIndex record)
        {
            return PXDatabase.Insert(typeof(SearchIndex),
                                      new PXDataFieldAssign(typeof(SearchIndex.noteID).Name, PXDbType.UniqueIdentifier, record.NoteID),
                                      new PXDataFieldAssign(typeof(SearchIndex.indexID).Name, PXDbType.UniqueIdentifier, record.IndexID),
                                      new PXDataFieldAssign(typeof(SearchIndex.category).Name, PXDbType.Int, record.Category),
                                      new PXDataFieldAssign(typeof(SearchIndex.entityType).Name, PXDbType.NVarChar, record.EntityType),
                                      new PXDataFieldAssign(typeof(SearchIndex.content).Name, PXDbType.NText, record.Content));
        }

        [PXInternalUseOnly]
        public static bool Update(SearchIndex record)
        {
            return PXDatabase.Update(typeof(SearchIndex),
                                      new PXDataFieldRestrict(typeof(SearchIndex.noteID).Name, PXDbType.UniqueIdentifier, record.NoteID),
                                      new PXDataFieldAssign(typeof(SearchIndex.category).Name, PXDbType.Int, record.Category),
                                      new PXDataFieldAssign(typeof(SearchIndex.entityType).Name, PXDbType.NVarChar, record.EntityType),
                                      new PXDataFieldAssign(typeof(SearchIndex.content).Name, PXDbType.NText, record.Content));
        }

        [PXInternalUseOnly]
        public static bool Delete(SearchIndex record)
        {
            return PXDatabase.Delete(typeof(SearchIndex),
                                      new PXDataFieldRestrict(typeof(SearchIndex.noteID).Name, PXDbType.UniqueIdentifier, record.NoteID));
        }

        [PXInternalUseOnly]
        public static void BulkInsert(IEnumerable<SearchIndex> records)
        {
            PointDbmsBase point = PXDatabase.Provider.CreateDbServicesPoint();
            PxDataTable recordsToInsert = new PxDataTable(point.Schema.GetTable(typeof(SearchIndex).Name));

            TransferTableTask task = new TransferTableTask();
            task.Source = new PxDataTableAdapter(recordsToInsert);
            task.Destination = point.GetTable(typeof(SearchIndex).Name);
            task.AppendData = true;

            BatchTransferExecutorSync bex = new BatchTransferExecutorSync(new SimpleDataTransferObserver());
            bex.Tasks.Enqueue(task);

            var timestamp = new byte[] { };
            Stopwatch sw= new Stopwatch();
            sw.Start();
            int currentCompany = PXInstanceHelper.CurrentCompany;
            foreach (SearchIndex record in records)
            {
                // see database_schema.xml for correct columns order.
                recordsToInsert.AddRow(new object[]{currentCompany, record.NoteID.Value, record.IndexID.Value, record.EntityType, record.Category, record.Content, timestamp});
            }
            Debug.Print("DataTable filled in {0} sec.", sw.Elapsed.TotalSeconds);
            sw.Restart();
            bex.StartSync();
            Debug.Print("DataImport in {0} sec.", sw.Elapsed.TotalSeconds);
        }

        [PXInternalUseOnly]
        public virtual SearchIndex BuildSearchIndex(PXCache sender, object row, PXResult res, string noteText)
        {
            SearchIndex si = new SearchIndex();
            si.IndexID = Guid.NewGuid();
            si.NoteID = (Guid?)sender.GetValue(row, typeof(Note.noteID).Name);
            si.Category = category;
            si.Content = BuildContent(sender, row, res) + " " + noteText;
            si.EntityType = row.GetType().FullName;

            return si;
        }

        [PXInternalUseOnly]
        public virtual RecordInfo BuildRecordInfo(PXCache sender, object row)
        {
            List<Type> allFields = new List<Type>();
            allFields.AddRange(titleFields);
            if (Line1Fields != null)
            {
                foreach (Type field in Line1Fields)
                {
                    if (!allFields.Contains(field))
                    {
                        allFields.Add(field);
                    }
                }
            }

            if (Line2Fields != null)
            {
                foreach (Type field in Line2Fields)
                {
                    if (!allFields.Contains(field))
                    {
                        allFields.Add(field);
                    }
                }
            }

            Dictionary<Type, object> values = ExtractValues(sender, row, null, allFields);

            //Title:
            List<object> titleArgs = new List<object>();
            string title = string.Empty;
            if (titleFields != null && titleFields.Length > 0)
            {
                foreach (Type field in titleFields)
                {
                    if (values.ContainsKey(field))
                    {
                        titleArgs.Add(values[field]);
                    }
                    else
                    {
                        titleArgs.Add(string.Empty);
                    }
                }
            }
            if (titlePrefix != null)
            {
                title = string.Format(PXMessages.LocalizeNoPrefix(titlePrefix), titleArgs.ToArray());
                if (title.Trim().EndsWith("-"))
                {
                    title = title.Trim().TrimEnd('-');
                }
            }
            
            //Line 1:
            List<object> line1Args = new List<object>();
            List<string> line1DisplayNames = new List<string>();
            string line1 = string.Empty;
            if (Line1Fields != null && Line1Fields.Length > 0)
            {
                for (int i = 0; i < Line1Fields.Length; i++)
                {
                    Type field = Line1Fields[i];

                    if (values.ContainsKey(field) && values[field] != null && !string.IsNullOrWhiteSpace(values[field].ToString()))
                    {
                        string displayName = PXUIFieldAttribute.GetDisplayName(sender.Graph.Caches[BqlCommand.GetItemType(field)], field.Name);
                        if (string.IsNullOrWhiteSpace(displayName))
                            displayName = field.Name;

                        line1Args.Add(values[field]);
                        line1DisplayNames.Add(displayName);
                    }
                    else
                    {
                        line1Args.Add(null);
                        line1DisplayNames.Add(string.Empty);
                    }

                }
            }
            line1 = BuildFormatedLine(Line1Format, line1Args, line1DisplayNames);


            //Line 2:
            List<object> line2Args = new List<object>();
            List<string> line2DisplayNames = new List<string>();
            string line2 = string.Empty;
            if (Line2Fields != null && Line2Fields.Length > 0)
            {
                for (int i = 0; i < Line2Fields.Length; i++)
                {
                    Type field = Line2Fields[i];

                    if (values.ContainsKey(field) && values[field] != null && !string.IsNullOrWhiteSpace(values[field].ToString()))
                    {
                        string displayName = PXUIFieldAttribute.GetDisplayName(sender.Graph.Caches[BqlCommand.GetItemType(field)], field.Name);
                        if (string.IsNullOrWhiteSpace(displayName))
                            displayName = field.Name;

                        line2Args.Add(values[field]);
                        line2DisplayNames.Add(displayName);
                    }
                    else
                    {
                        line2Args.Add(null);
                        line2DisplayNames.Add(string.Empty);
                    }

                }
            }
            line2 = BuildFormatedLine(Line2Format, line2Args, line2DisplayNames);

            return new RecordInfo(title, line1, line2);
        }

        private string BuildFormatedLine(string compositeFormat, List<object> argValues, List<string> displayNames )
        {
            StringBuilder sb = new StringBuilder();

            Regex ComposedFormatArgsRegex = new Regex(@"(?<!(?<!\{)\{)\{(?<index>\d+)(,(?<alignment>\d+))?(:(?<formatString>[^\}]+))?\}(?!\}(?!\}))",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            MatchCollection matches = ComposedFormatArgsRegex.Matches(compositeFormat);

            for (int i = 0; i < matches.Count; i++)
            {
                if (argValues.Count > i)
                {
                    string formatedDisplayname = string.Format(matches[i].Value, displayNames.ToArray());
                    string formatedValue = string.Format(matches[i].Value, argValues.ToArray());
                    if (!string.IsNullOrWhiteSpace(formatedDisplayname) && !string.IsNullOrWhiteSpace(formatedValue))
                    {
                        sb.AppendFormat("{0}: {1} - ", formatedDisplayname, formatedValue);
                    }
                }
            }

            string result = sb.ToString();

            if (result.Length > 1)//remove trailing - 
            {
                result = result.Substring(0, result.Length - 3);
            }

            return result;
        }

        [PXInternalUseOnly]
        public virtual string BuildContent(PXCache sender, object row, PXResult res)
        {
            List<Type> allFields = new List<Type>();
            allFields.AddRange(titleFields);
            if (fields != null)
            {
                foreach (Type field in fields)
                {
                    if (!allFields.Contains(field))
                    {
                        allFields.Add(field);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            Dictionary<Type, object> values = ExtractValues(sender, row, res, allFields, true);
            
            //Title:
            List<string> titleNumbers = new List<string>();
            List<object> titleArgs = new List<object>();
            if (titleFields != null && titleFields.Length > 0)
            {
                foreach (Type field in titleFields)
                {
                    if (values.ContainsKey(field))
                    {
                        object fieldValue = values[field];
                        titleArgs.Add(fieldValue);
                        
                        if (fieldValue != null && NumberFields != null && NumberFields.Contains(field))
                        {
                            string strValue = fieldValue.ToString();
                            string numberWithoutPrefix = RemovePrefix(strValue);

                            if (numberWithoutPrefix.Length != strValue.Length)
                                titleNumbers.Add(numberWithoutPrefix);
                        }

                    }
                    else
                    {
                        titleArgs.Add(string.Empty);
                    }
                }
            }
            if (titlePrefix != null)
            {
                sb.Append(string.Format(titlePrefix, titleArgs.ToArray()));
            }
            sb.Append(" ");

            foreach (string num in titleNumbers )
            {
                sb.AppendFormat("{0} ", num);
            }
            
            if (fields != null)
            {
                foreach (Type field in fields)
                {
                    if (values.ContainsKey(field))
                    {
                        if (values[field] != null)
                        {
                            sb.Append(values[field].ToString());
                            sb.Append(" ");
                        }
                    }
                }
            }
            
            return sb.ToString();
        }

        private string RemovePrefix(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return string.Empty;

            int firstDigitIndex = 0;
            for (int i = 0; i < strValue.Length; i++)
            {
                if (char.IsDigit(strValue[i]) && strValue[i] != '0')
                {
                    firstDigitIndex = i;
                    break;
                }
            }

            return strValue.Substring(firstDigitIndex);
        }

        protected virtual object GetFieldValue(PXCache sender, object row, Type field, bool disableLazyLoading)
        {
            return GetFieldValue(sender, row, field, disableLazyLoading, false);
        }

        private object GetFieldValue(PXCache sender, object row, Type field, bool disableLazyLoading, bool buildTranslations)
        {
            var dbLocalizableAttr = sender.GetAttributes(field.Name).FirstOrDefault(attr => attr is PXDBLocalizableStringAttribute);
            var listAttr = IsListAttributeField(sender, field);
            if (!buildTranslations)
            {
                if (listAttr == 2)
                {
                    listAttr = 1;
                }
                dbLocalizableAttr = null;
            }

            if (disableLazyLoading)
            {							
                if (listAttr == 0 && dbLocalizableAttr == null)
                {
                    return sender.GetValue(row, field.Name);
                }
            }

            object val = sender.GetStateExt(row, field.Name);
            PXFieldState state = val as PXFieldState;

            if (state != null)
            {
                if (dbLocalizableAttr != null)
                {
                    var translations = sender.GetStateExt(row, field.Name + "Translations") as string[];
                    if (translations != null)
                    {
                        return string.Join(" ", translations);
                    }
                }

                val = state.Value;
            }

            if (state is PXIntState)
            {
                PXIntState istate = (PXIntState)state;
                if (istate.AllowedValues != null && istate._NeutralLabels != null)
                    for (int i = 0; i < istate.AllowedValues.Length && i < istate.AllowedLabels.Length && i < istate._NeutralLabels.Length; i++)
                        if (istate.AllowedValues[i] == (int)val)
                        {
                            if (listAttr == 2)
                            {
                                val = GetAllTranslations(sender, istate.Name, i, istate._NeutralLabels, istate.AllowedLabels);
                            }
                            else
                            {
                                val = istate.AllowedLabels[i];
                            }
                            break;
                        }
            }
            else if (state is PXStringState)
            {
                PXStringState sstate = (PXStringState)state;
                if (sstate.AllowedValues != null && sstate._NeutralLabels != null)
                    for (int i = 0; i < sstate.AllowedValues.Length && i < sstate.AllowedLabels.Length && i < sstate._NeutralLabels.Length; i++)
                        if (sstate.AllowedValues[i] == (string)val)
                        {
                            if (listAttr == 2)
                            {
                                val = GetAllTranslations(sender, sstate.Name, i, sstate._NeutralLabels, sstate.AllowedLabels);
                            }
                            else
                            {
                                val = sstate.AllowedLabels[i];
                            }
                            break;
                        }
            }


            PXStringState strState = state as PXStringState;
            //Following is a hack to get FinPeriod to format as it is visible to the user... couldn't find any other way to do it ((.
            if (strState != null && strState.InputMask == "##-####")
            {
                string strFinPeriod = val.ToString();
                if (strFinPeriod.Length == 6)
                {
                    val = string.Format("{0}-{1}", strFinPeriod.Substring(0, 2), strFinPeriod.Substring(2, 4));
                }
            }

            
            return val;
        }

        private string GetAllTranslations(PXCache sender, string field, int i, string[] neutral, string[] theonly)
        {
            PXLocale[] locales = Common.PXContext.GetSlot<PXLocale[]>("SILocales");
            if (locales == null)
            {
                Common.PXContext.SetSlot("SILocales", locales = PXLocalesProvider.GetLocales());
            }
            if (locales.Length <= 1)
            {
                return theonly[i];
            }
            HashSet<string> list = new HashSet<string>();
            foreach (var locale in locales)
            {
                if (!String.Equals(locale.Name, System.Threading.Thread.CurrentThread.CurrentCulture.Name))
                {
                    using (new Common.PXCultureScope(new System.Globalization.CultureInfo(locale.Name)))
                    {
                        string[] labels = new string[neutral.Length];
                        PXLocalizerRepository.ListLocalizer.Localize(field, sender, neutral, labels);
                        if (!String.IsNullOrWhiteSpace(labels[i]))
                        {
                            list.Add(labels[i]);
                        }
                    }
                }
                else if (!String.IsNullOrWhiteSpace(theonly[i]))
                {
                    list.Add(theonly[i]);
                }
            }
            if (list.Count > 1)
            {
                return String.Join(" ", list);
            }
            return theonly[i];
        }

        [PXInternalUseOnly]
        public static List<Type> GetAllSearchableEntities(PXGraph graph)
        {
            List<Type> result = new List<Type>(30);

            foreach (ServiceManager.TypeInfo table in ServiceManager.TableList)
            {
                Type noteIdField = table.Type.GetNestedType(typeof(Note.noteID).Name);

                if (noteIdField != null)
                {
                    var list = graph.Caches[table.Type].GetAttributes(typeof(Note.noteID).Name);
                    foreach (PXEventSubscriberAttribute att in list)
                    {
                        PXSearchableAttribute attribute = att as PXSearchableAttribute;
                        if (attribute != null)
                        {
                            result.Add(table.Type);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        [PXInternalUseOnly]
        public virtual Dictionary<Type, object> ExtractValues(PXCache sender, object row, PXResult res, IEnumerable<Type> fieldTypes)
        {
            return ExtractValues(sender, row, res, fieldTypes, false);
        }

        private Dictionary<Type, object> ExtractValues(PXCache sender, object row, PXResult res, IEnumerable<Type> fieldTypes, bool buildTranslations)
        {
            Dictionary<Type, object> result = new Dictionary<Type, object>();
            Dictionary<Type, Type> selectorFieldByTable = new Dictionary<Type, Type>();

            Type lastField = null;
            foreach (Type field in fieldTypes)
            {
                Type tableType = BqlCommand.GetItemType(field);

                if (tableType != null)
                {
                    if (sender.GetItemType().IsAssignableFrom(tableType) || tableType.IsAssignableFrom(sender.GetItemType()))//field of the given table or a base dac/table
                    {
                        if (!result.ContainsKey(field))
                            result.Add(field, GetFieldValue(sender, row, field, res != null, buildTranslations));

                        lastField = field;
                    }
                    else if (lastField != null && typeof(IBqlTable).IsAssignableFrom(BqlCommand.GetItemType(field)))//field of any other table
                    {
                        object foreign = null;
                        if (res != null)
                        {
                            //mass processing - The values are searched in the joined resultset.
                            foreign = res[BqlCommand.GetItemType(field)];

                            if (foreign != null)
                            {
                                PXCache fcache = sender.Graph.Caches[foreign.GetType()];
                                if (!result.ContainsKey(field))
                                    result.Add(field, GetFieldValue(fcache, foreign, field, false, buildTranslations));
                            }
                        }

                        if (foreign == null)
                        {
                            //lazy loading - The values are selected through the selectors, with a call to DB
                            
                            string selectorFieldName;
                            if (selectorFieldByTable.ContainsKey(tableType))
                            {
                                selectorFieldName = selectorFieldByTable[tableType].Name;
                            }
                            else
                            {
                                selectorFieldName = lastField.Name;
                            }

                            foreign = PXSelectorAttribute.Select(sender, row, selectorFieldName);
                            if (foreign == null)
                            {
                                foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(selectorFieldName))
                                {
                                    PXAggregateAttribute aggatt = attr as PXAggregateAttribute;

                                    if (aggatt != null)
                                    {
                                        PXDimensionSelectorAttribute dimAttr = aggatt.GetAttribute<PXDimensionSelectorAttribute>();
                                        PXSelectorAttribute selAttr = aggatt.GetAttribute<PXSelectorAttribute>();
                                        if (dimAttr != null)
                                        {
                                            selAttr = dimAttr.GetAttribute<PXSelectorAttribute>();
                                        }
                                        
                                        if (selAttr != null)
                                        {
                                            PXView select = sender.Graph.TypedViews.GetView(selAttr.PrimarySelect, !selAttr.DirtyRead);
                                            object[] pars = new object[selAttr.ParsCount + 1];
                                            pars[pars.Length - 1] = sender.GetValue(row, selAttr.FieldOrdinal);
                                            foreign = sender._InvokeSelectorGetter(row, selAttr.FieldName, select, pars, true) ?? PXSelectorAttribute.SelectSingleBound(select, new object[] { row, sender.Graph.Accessinfo }, pars);
                                        }
                                    }
                                }
                            }

                            if (foreign is PXResult)
                                foreign = ((PXResult)foreign)[0];

                            if (foreign != null)
                            {
                                if (!selectorFieldByTable.ContainsKey(tableType))
                                {
                                    selectorFieldByTable.Add(tableType, lastField);
                                    //result.Remove(lastField);
                                }

                                PXCache fcache = sender.Graph.Caches[foreign.GetType()];
                                if (!result.ContainsKey(field))
                                    result.Add(field, GetFieldValue(fcache, foreign, field, false, buildTranslations));
                            }
                        }
                        
                    }
                }
            }

            return result;
        }

        /// <exclude/>
        [DebuggerDisplay("{Title} / {Line1}; {Line2}")]
        public class RecordInfo
        {
            public string Title { get; private set; }
            public string Line1 { get; private set; }
            public string Line2 { get; private set; }

            public RecordInfo(string title, string line1, string line2)
            {
                this.Title = title;
                this.Line1 = line1;
                this.Line2 = line2;
            }
        }
    }
    
}
