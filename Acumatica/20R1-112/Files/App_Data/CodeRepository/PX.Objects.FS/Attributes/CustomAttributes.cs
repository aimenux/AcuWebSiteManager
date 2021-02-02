using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PX.Objects.FS
{
    [PXDBInt]
    [PXUIField(DisplayName = "Service ID", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<
			InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
		And<
			InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>), 
        PX.Objects.IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus))]
	[PXRestrictor(typeof(Where<InventoryItem.isTemplate, Equal<False>>), IN.Messages.InventoryItemIsATemplate, ShowWarning = true)]
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
            attr.CacheGlobal = true;
            attr.DescriptionField = descriptionField;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public FSInventoryAttribute(Type searchType, Type substituteKey, Type descriptionField)
            : base()
        {
            PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, searchType, substituteKey);
			attr.CacheGlobal = true;
            attr.DescriptionField = descriptionField;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public FSInventoryAttribute()
            : this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
        {
        }
    }

    public class FSAttributeGroupList<TClass, TEntity1, TEntity2, TEntity3> : 
                 CSAttributeGroupList<TClass, TEntity1>, IPXRowInsertedSubscriber, IPXRowDeletedSubscriber, IPXRowUpdatedSubscriber
        where TClass : class
    {
        public FSAttributeGroupList(PXGraph graph) 
            : base(graph)
        {
            graph.RowInserted.AddHandler<CSAttributeGroup>(RowInserted);
            graph.RowDeleted.AddHandler<CSAttributeGroup>(RowDeleted);
            graph.RowUpdated.AddHandler<CSAttributeGroup>(RowUpdated);
        }

        public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            CacheEventHandler(sender, e.Row, PXDBOperation.Update, e.ExternalCall);
        }

        public void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            CacheEventHandler(sender, e.Row, PXDBOperation.Delete, e.ExternalCall);
        }

        public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            CacheEventHandler(sender, e.Row, PXDBOperation.Insert, e.ExternalCall);
        }

        private void CacheEventHandler(PXCache sender, object row, PXDBOperation operation, bool externalCall)
        {
            if (row == null)
            {
                return;
            }

            CSAttributeGroup cSAttributeGroup = (CSAttributeGroup)sender.CreateCopy(row);

            if (externalCall == true)
            {
                if (cSAttributeGroup.EntityType == typeof(TEntity1).FullName)
                {
                    cSAttributeGroup.EntityType = typeof(TEntity2).FullName;
                    UpdateCacheRecord(sender, cSAttributeGroup, operation);
                }
            }
            else
            {
                if (cSAttributeGroup.EntityType == typeof(TEntity2).FullName)
                {
                    cSAttributeGroup.EntityType = typeof(TEntity3).FullName;
                    UpdateCacheRecord(sender, cSAttributeGroup, operation);
                }
            }
        }

        private void UpdateCacheRecord(PXCache sender, CSAttributeGroup cSAttributeGroup, PXDBOperation operation)
        {
            if (operation == PXDBOperation.Insert)
            {
                sender.Insert(cSAttributeGroup);
            }
            else if (operation == PXDBOperation.Update)
            {
                sender.Update(cSAttributeGroup);
            }
            else if (operation == PXDBOperation.Delete)
            {
                sender.Delete(cSAttributeGroup);
            }
        }
    }

    public class FSAttributeList<TEntity> : CRAttributeList<TEntity>
    {
        public FSAttributeList(PXGraph graph): base(graph)
        {
        }

        protected IEnumerable<CSAnswers> SelectInternal(PXGraph graph, object row)
        {
            if (row == null)
            {
                yield break;
            }

            var noteID = GetNoteId(row);

            if (!noteID.HasValue)
            {
                yield break;
            }

            var answerCache = graph.Caches[typeof(CSAnswers)];
            var entityCache = graph.Caches[row.GetType()];

            List<CSAnswers> answerList;

            var status = entityCache.GetStatus(row);

            if (status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted)
            {
                answerList = answerCache.Inserted.Cast<CSAnswers>().Where(x => x.RefNoteID == noteID).ToList();
            }
            else
            {
                answerList = PXSelect<CSAnswers,
                             Where<
                                 CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
                             .Select(graph, noteID).FirstTableItems.ToList();
            }

            var classID = base.GetClassId(row);

            CRAttribute.ClassAttributeList classAttributeList = new CRAttribute.ClassAttributeList();

            if (classID != null)
            {
                classAttributeList = CRAttribute.EntityAttributes(base.GetEntityTypeFromAttribute(row), classID);
            }

            //when coming from Import scenarios there might be attributes which don't belong to entity's current attribute class or the entity might not have any attribute class at all
            if (graph.IsImport && PXView.SortColumns.Any() && PXView.Searches.Any())
            {
                var columnIndex = Array.FindIndex(PXView.SortColumns, x => x.Equals(typeof(CSAnswers.attributeID).Name, StringComparison.OrdinalIgnoreCase));

                if (columnIndex >= 0 && columnIndex < PXView.Searches.Length)
                {
                    var searchValue = PXView.Searches[columnIndex];

                    if (searchValue != null)
                    {
                        //searchValue can be either AttributeId or Description
                        var attributeDefinition = CRAttribute.Attributes[searchValue.ToString()] ?? CRAttribute.AttributesByDescr[searchValue.ToString()];

                        if (attributeDefinition == null)
                        {
                            throw new PXSetPropertyException(PX.Objects.CR.Messages.AttributeNotValid);
                        }
                        else if (classAttributeList[attributeDefinition.ToString()] == null) //avoid duplicates
                        {
                            classAttributeList.Add(new CRAttribute.AttributeExt(attributeDefinition, null, false, true));
                        }
                    }
                }
            }

            if (answerList.Count == 0 && classAttributeList.Count == 0)
            {
                yield break;
            }

            //attribute identifiers that are contained in CSAnswers cache/table but not in class attribute list
            List<string> attributeIdListAnswers = answerList.Select(x => x.AttributeID)
                                                  .Except(classAttributeList.Select(x => x.ID))
                                                  .Distinct()
                                                  .ToList();

            //attribute identifiers that are contained in class attribute list but not in CSAnswers cache/table
            List<string> attributeIdListClass = classAttributeList.Select(x => x.ID)
                                                .Except(answerList.Select(x => x.AttributeID))
                                                .ToList();

            //attribute identifiers which belong to both lists
            List<string> attributeIdListIntersection = classAttributeList.Select(x => x.ID)
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
                {
                    continue;
                }

                if (!classAttributeDefinition.IsActive)
                {
                    continue;
                }

                CSAnswers answer = (CSAnswers)answerCache.CreateInstance();
                answer.AttributeID = classAttributeDefinition.ID;
                answer.RefNoteID = noteID;
                answer.Value = GetDefaultAnswerValue(classAttributeDefinition);

                if (classAttributeDefinition.ControlType == CSAttribute.CheckBox)
                {
                    bool value;

                    if (bool.TryParse(answer.Value, out value))
                    {
                        answer.Value = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (answer.Value == null)
                    {
                        answer.Value = 0.ToString();
                    }
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
                {
                    continue;
                }

                if (!classAttributeDefinition.IsActive)
                {
                    continue;
                }

                if (answer.Value == null && classAttributeDefinition.ControlType == CSAttribute.CheckBox)
                {
                    answer.Value = bool.FalseString;
                }

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

            output = output.OrderBy(x => classAttributeList.Contains(x.AttributeID) ? classAttributeList.IndexOf(x.AttributeID) : (x.Order ?? 0))
                     .ThenBy(x => x.AttributeID)
                     .ToList();

            short attributeOrder = 0;

            foreach (CSAnswers answer in output)
            {
                answer.Order = attributeOrder++;
                yield return answer;
            }
        }

        public void CopyAttributes(PXGraph destGraph, object destination, PXGraph srcGraph, object source, bool copyAll)
        {
            if (destination == null || source == null)
            {
                return;
            }

            var sourceAttributes = SelectInternal(srcGraph, source).RowCast<CSAnswers>().ToList();
            var targetAttributes = SelectInternal(destGraph, destination).RowCast<CSAnswers>().ToList();

            var answerCache = _Graph.Caches<CSAnswers>();
            
            foreach (var targetAttribute in targetAttributes)
            {
                var sourceAttr = sourceAttributes.SingleOrDefault(x => x.AttributeID == targetAttribute.AttributeID);

                if (sourceAttr == null
                    || string.IsNullOrEmpty(sourceAttr.Value)
                    || sourceAttr.Value == targetAttribute.Value)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(targetAttribute.Value) || copyAll)
                {
                    var answer = PXCache<CSAnswers>.CreateCopy(targetAttribute);
                    answer.Value = sourceAttr.Value;
                    answerCache.Update(answer);
                }
            }
        }
    }
}
