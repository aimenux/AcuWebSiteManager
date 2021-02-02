using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Reports;
using PX.SM;
using PX.TM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PX.Objects.FS
{
    [PXDBInt]
    [PXUIField(DisplayName = "Service ID", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
                                And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>), PX.Objects.IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus))]
    public class FSInventoryAttribute : AcctSubAttribute
    {
        #region State
        public const string DimensionName = "INVENTORY";

        public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
		{
            public dimensionName() : base(DimensionName)
            {
            }
        }
        #endregion

        public FSInventoryAttribute(Type searchType, Type substituteKey, Type descriptionField, Type[] listField)
            : base()
        {
            PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, searchType, substituteKey, listField);
            attr.DescriptionField = descriptionField;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public FSInventoryAttribute(Type searchType, Type substituteKey, Type descriptionField)
            : base()
        {
            PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, searchType, substituteKey);
            attr.DescriptionField = descriptionField;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public FSInventoryAttribute()
            : this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
        {
        }
    }


    public class FSAttributeList<TEntity, BEntity> : CRAttributeList<TEntity>
    {
        public FSAttributeList(PXGraph graph): base(graph)
        {
            PXDBAttributeAttribute.Activate(_Graph.Caches[typeof(BEntity)]);
            _Graph.RowPersisting.AddHandler<BEntity>(ReferenceRowPersistingHandler);
            _Graph.RowUpdating.AddHandler<BEntity>(ReferenceRowUpdatingHandler);
            _Graph.RowDeleted.AddHandler<BEntity>(ReferenceRowDeletedHandler);
            _Graph.RowInserted.AddHandler<BEntity>(RowInsertedHandler);
        }

        protected IEnumerable<CSAnswers> SelectInternal(PXGraph graph, object row)
        {
            if (row == null)
                yield break;

            var noteId = GetNoteId(row);

            if (!noteId.HasValue)
                yield break;

            var answerCache = graph.Caches[typeof(CSAnswers)];
            var entityCache = graph.Caches[row.GetType()];

            List<CSAnswers> answerList;

            var status = entityCache.GetStatus(row);

            if (status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted)
            {
                answerList = answerCache.Inserted.Cast<CSAnswers>().Where(x => x.RefNoteID == noteId).ToList();
            }
            else
            {
                answerList = PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
                    .Select(graph, noteId).FirstTableItems.ToList();
            }

            var classId = base.GetClassId(row);

            CRAttribute.ClassAttributeList classAttributeList = new CRAttribute.ClassAttributeList();

            if (classId != null)
            {
                classAttributeList = CRAttribute.EntityAttributes(base.GetEntityTypeFromAttribute(row), classId);
            }
            //when coming from Import scenarios there might be attributes which don't belong to entity's current attribute class or the entity might not have any attribute class at all
            if (graph.IsImport && PXView.SortColumns.Any() && PXView.Searches.Any())
            {
                var columnIndex = Array.FindIndex(PXView.SortColumns,
                    x => x.Equals(typeof(CSAnswers.attributeID).Name, StringComparison.OrdinalIgnoreCase));

                if (columnIndex >= 0 && columnIndex < PXView.Searches.Length)
                {
                    var searchValue = PXView.Searches[columnIndex];

                    if (searchValue != null)
                    {
                        //searchValue can be either AttributeId or Description
                        var attributeDefinition = CRAttribute.Attributes[searchValue.ToString()] ??
                                             CRAttribute.AttributesByDescr[searchValue.ToString()];

                        if (attributeDefinition == null)
                        {
                            throw new PXSetPropertyException(PX.Objects.CR.Messages.AttributeNotValid);
                        }
                        //avoid duplicates
                        else if (classAttributeList[attributeDefinition.ToString()] == null)
                        {
                            classAttributeList.Add(new CRAttribute.AttributeExt(attributeDefinition, null, false, true));
                        }
                    }
                }
            }

            if (answerList.Count == 0 && classAttributeList.Count == 0)
                yield break;

            //attribute identifiers that are contained in CSAnswers cache/table but not in class attribute list
            List<string> attributeIdListAnswers =
                answerList.Select(x => x.AttributeID)
                    .Except(classAttributeList.Select(x => x.ID))
                    .Distinct()
                    .ToList();

            //attribute identifiers that are contained in class attribute list but not in CSAnswers cache/table
            List<string> attributeIdListClass =
                classAttributeList.Select(x => x.ID)
                    .Except(answerList.Select(x => x.AttributeID))
                    .ToList();

            //attribute identifiers which belong to both lists
            List<string> attributeIdListIntersection =
                classAttributeList.Select(x => x.ID)
                    .Intersect(answerList.Select(x => x.AttributeID))
                    .Distinct()
                    .ToList();


            var cacheIsDirty = answerCache.IsDirty;

            List<CSAnswers> output = new List<CSAnswers>();

            //attributes contained only in CSAnswers cache/table should be added "as is"
            output.AddRange(answerList.Where(x => attributeIdListAnswers.Contains(x.AttributeID)));

            //attributes contained only in class attribute list should be created and initialized with default value
            foreach (var attributeId in attributeIdListClass)
            {
                var classAttributeDefinition = classAttributeList[attributeId];

                if (PXSiteMap.IsPortal && classAttributeDefinition.IsInternal)
                    continue;

                if (!classAttributeDefinition.IsActive)
                    continue;

                CSAnswers answer = (CSAnswers)answerCache.CreateInstance();
                answer.AttributeID = classAttributeDefinition.ID;
                answer.RefNoteID = noteId;
                answer.Value = GetDefaultAnswerValue(classAttributeDefinition);
                if (classAttributeDefinition.ControlType == CSAttribute.CheckBox)
                {
                    bool value;
                    if (bool.TryParse(answer.Value, out value))
                        answer.Value = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
                    else if (answer.Value == null)
                        answer.Value = 0.ToString();
                }

                answer.IsRequired = classAttributeDefinition.Required;

                Dictionary<string, object> keys = new Dictionary<string, object>();
                foreach (string key in answerCache.Keys.ToArray())
                {
                    keys[key] = answerCache.GetValue(answer, key);
                }

                if (answerCache.Locate(keys) == 0)
                {
                    answer = (CSAnswers)(answerCache.Locate(answer) ?? answerCache.Insert(answer));
                    output.Add(answer);
                }
            }

            //attributes belonging to both lists should be selected from CSAnswers cache/table with and additional IsRequired check against class definition
            foreach (CSAnswers answer in answerList.Where(x => attributeIdListIntersection.Contains(x.AttributeID)).ToList())
            {
                var classAttributeDefinition = classAttributeList[answer.AttributeID];

                if (PXSiteMap.IsPortal && classAttributeDefinition.IsInternal)
                    continue;

                if (!classAttributeDefinition.IsActive)
                    continue;

                if (answer.Value == null && classAttributeDefinition.ControlType == CSAttribute.CheckBox)
                    answer.Value = bool.FalseString;

                if (answer.IsRequired == null || classAttributeDefinition.Required != answer.IsRequired)
                {
                    answer.IsRequired = classAttributeDefinition.Required;

                    var fieldState = View.Cache.GetValueExt<CSAnswers.isRequired>(answer) as PXFieldState;
                    var fieldValue = fieldState != null && ((bool?)fieldState.Value).GetValueOrDefault();

                    answer.IsRequired = classAttributeDefinition.Required || fieldValue;
                }



                output.Add(answer);
            }

            answerCache.IsDirty = cacheIsDirty;

            output =
                output.OrderBy(
                    x =>
                        classAttributeList.Contains(x.AttributeID)
                            ? classAttributeList.IndexOf(x.AttributeID)
                            : (x.Order ?? 0))
                    .ThenBy(x => x.AttributeID)
                    .ToList();

            short attributeOrder = 0;

            foreach (CSAnswers answer in output)
            {
                answer.Order = attributeOrder++;
                yield return answer;
            }
        }

        public void CopyAttributes(PXGraph destGraph, object destination, PXGraph srcGraph, object source, bool copyall)
        {
            if (destination == null || source == null) return;

            var sourceAttributes = SelectInternal(srcGraph, source).RowCast<CSAnswers>().ToList();
            var targetAttributes = SelectInternal(destGraph, destination).RowCast<CSAnswers>().ToList();

            var answerCache = _Graph.Caches<CSAnswers>();
            
            foreach (var targetAttribute in targetAttributes)
            {
                var sourceAttr = sourceAttributes.SingleOrDefault(x => x.AttributeID == targetAttribute.AttributeID);

                if (sourceAttr == null || string.IsNullOrEmpty(sourceAttr.Value) ||
                    sourceAttr.Value == targetAttribute.Value)
                    continue;

                if (string.IsNullOrEmpty(targetAttribute.Value) || copyall)
                {
                    var answer = PXCache<CSAnswers>.CreateCopy(targetAttribute);
                    answer.Value = sourceAttr.Value;
                    answerCache.Update(answer);
                }
            }
        }
    }
}
