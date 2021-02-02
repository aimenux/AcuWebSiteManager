using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AM
{
    public class CreateECOsProcess : PXGraph<CreateECOsProcess>
    {
        public PXCancel<CreateECOsFilter> Cancel;
        public PXFilter<CreateECOsFilter> Filter;

        [PXFilterable]
        public PXProcessingJoin<AMECRItem, InnerJoin<InventoryItem,
            On<AMECRItem.inventoryID, Equal<InventoryItem.inventoryID>>>,
            Where<AMECRItem.status, Equal<AMECRStatus.approved>>> ECRRecords;

        public CreateECOsProcess()
        {
            var filter = Filter.Current;
            ECRRecords.SetProcessDelegate(
                delegate (List<AMECRItem> list)
                {
                    ProcessECR(list, filter);
                });
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIField(DisplayName = "ECR Description")]
        protected virtual void AMECRItem_Descr_CacheAttached(PXCache sender) { }

        public static void ProcessECR(List<AMECRItem> list, CreateECOsFilter filter)
        {
            var merge = filter.MergeECRs == true;
            var ecoGraph = CreateInstance<ECOMaint>();
            var createECOGraph = CreateInstance<CreateECOsProcess>();
            createECOGraph.Filter.Current = filter;
            var failed = false;
            var mergedECRs = new HashSet<string>();

            for(var i = 0; i < list.Count; i++)
            {
                ecoGraph.Clear();
                var ecr = list[i];
                try
                {
                    if (merge)
                    {
                        if(mergedECRs.Contains(ecr.ECRID))
                        {
                            PXProcessing<AMECRItem>.SetInfo(i, ActionsMessages.RecordProcessed);
                            continue;
                        }

                        var allEcrs = list.Where(x => x.BOMID == ecr.BOMID && x.BOMRevisionID == ecr.BOMRevisionID).ToList();                       
                        foreach (var ecrUnMerged in allEcrs)
                        {
                            var existingECO = createECOGraph.GetCachedEcoItem(ecoGraph.Documents.Cache, ecr);
                            if(existingECO == null)
                                createECOGraph.CreateECO(ecoGraph, ecrUnMerged);
                            else
                                createECOGraph.MergeECO(ecoGraph, existingECO, ecrUnMerged);
                        }

                        ecoGraph.Actions.PressSave();

                        foreach (var ecrMerged in allEcrs)
                        {
                            mergedECRs.Add(ecrMerged.ECRID);
                        }

                        PXProcessing<AMECRItem>.SetInfo(i, ActionsMessages.RecordProcessed);
                        continue;
                    }

                    createECOGraph.CreateECO(ecoGraph, ecr);
                    ecoGraph.Actions.PressSave();
                    PXProcessing<AMECRItem>.SetInfo(i, ActionsMessages.RecordProcessed);
                }
                catch (Exception e)
                {
                    PXProcessing<AMECRItem>.SetError(i, e);
                    PXTraceHelper.PxTraceException(e);

                    if (list.Count == 1)
                    {
                        throw new PXOperationCompletedSingleErrorException(e);
                    }

                    failed = true;
                }
            }

            if (failed)
            {
                throw new PXOperationCompletedException(PX.Data.ErrorMessages.SeveralItemsFailed);
            }
        }

        protected virtual AMECOItem GetCachedEcoItem(PXCache cache, AMECRItem ecrItem)
        {
            var ecoItem = cache.Cached.ToArray<AMECOItem>()
                .FirstOrDefault(x => x.BOMID == ecrItem?.BOMID && x.BOMRevisionID == ecrItem?.BOMRevisionID);
            
            return ecoItem == null ? null : (AMECOItem) cache.CreateCopy(ecoItem);
        }

        protected virtual AMBomOper GetCachedEcoOper(PXCache cache, AMECOItem ecoItem, string operationCD)
        {
            var ecoOper = cache.Cached.ToArray<AMBomOper>()
                .FirstOrDefault(x => x.BOMID == ecoItem?.ECOID && x.RevisionID == AMECOItem.ECORev && x.OperationCD == operationCD);

            return ecoOper == null ? null : (AMBomOper)cache.CreateCopy(ecoOper);
        }

        protected virtual T GetCachedECORow<T>(PXCache cache, AMECOItem ecoItem, int? operationId, int? lineid)
            where T : IBqlTable, IBomDetail
        {
            var newT = Activator.CreateInstance<T>();
            newT.BOMID = ecoItem?.ECOID;
            newT.RevisionID = AMECOItem.ECORev;
            newT.OperationID = operationId;
            newT.LineID = lineid;

            var row = (T)cache.Locate(newT);
            
            return row == null ? row : (T)cache.CreateCopy(row);
        }

        protected virtual T GetCachedECORow<T>(PXCache cache, AMECOItem ecoItem, int? operationId, int? lineid, int? matlLineId)
            where T : IBqlTable, IBomMatlDetail
        {
            var newT = Activator.CreateInstance<T>();
            newT.BOMID = ecoItem?.ECOID;
            newT.RevisionID = AMECOItem.ECORev;
            newT.OperationID = operationId;
            newT.LineID = lineid;
            newT.MatlLineID = matlLineId;

            var row = (T)cache.Locate(newT);

            return row == null ? row : (T)cache.CreateCopy(row);
        }

        protected virtual T GetCachedECORow<T>(PXCache cache, AMECOItem ecoItem, int? lineNbr)
            where T : IBqlTable, IBomAttr
        {
            var newT = Activator.CreateInstance<T>();
            newT.BOMID = ecoItem?.ECOID;
            newT.RevisionID = AMECOItem.ECORev;
            newT.LineNbr = lineNbr;

            var row = (T)cache.Locate(newT);

            return row == null ? row : (T)cache.CreateCopy(row);
        }

        protected virtual void MergeECO(ECOMaint ecoGraph, AMECOItem existingECO, AMECRItem ecr)
        {
            MergeECOOper(ecoGraph, existingECO, ecr);
            MergeECOMatl(ecoGraph,existingECO, ecr);
            MergeECOStep(ecoGraph,existingECO, ecr);
            MergeECORef(ecoGraph, existingECO, ecr);
            MergeECOTool(ecoGraph,existingECO, ecr);
            MergeECOOvhd(ecoGraph, existingECO, ecr);
            MergeECOAttr(ecoGraph, existingECO, ecr);
            ecoGraph.UpdateECRStatus(ecr, AMECRStatus.Completed);
        }

        // Allows customization override to fields excluded
        protected virtual string[] OperExcludeFields()
        {
            return new []
            {
                typeof(AMBomOper.bOMID).Name.ToCapitalized(),
                typeof(AMBomOper.revisionID).Name.ToCapitalized(),
                typeof(AMBomOper.lineCntrMatl).Name.ToCapitalized(),
                typeof(AMBomOper.lineCntrOvhd).Name.ToCapitalized(),
                typeof(AMBomOper.lineCntrStep).Name.ToCapitalized(),
                typeof(AMBomOper.lineCntrTool).Name.ToCapitalized()
            };
        }

        protected virtual string[] ChildExcludeFields()
        {
            return new[]
            {
                "BOMID",  "RevisionID", "OperationID"
            };
        }

        protected virtual void MergeECOOper(ECOMaint graph, AMECOItem existingECO, AMECRItem ecr)
        {
            var operExcludeFields = OperExcludeFields();

            foreach (AMBomOper fromRow in SelectFrom<AMBomOper>
                .Where<AMBomOper.bOMID.IsEqual<@P.AsString>
                .And<AMBomOper.revisionID.IsEqual<AMECRItem.eCRRev>>>
                .View.Select(graph, ecr.ECRID))
            {
                if (fromRow.RowStatus == AMRowStatus.Unchanged)
                    continue;

                // Can use the getcached for child rows of oper but not oper because we need to see if the user added the same OperactionCD to mult ECR...
                //var existing = GetCachedECORow<AMBomOper>(graph.BomOperRecords.Cache, existingECO, fromRow.OperationID);
                var existing = GetCachedEcoOper(graph.BomOperRecords.Cache, existingECO, fromRow.OperationCD);
                if (existing == null)
                {
                    var toRow = (AMBomOper)graph.BomOperRecords.Cache.CreateCopy(fromRow);
                    toRow.BOMID = existingECO.ECOID;
                    toRow.RevisionID = AMECOItem.ECORev;
                    toRow.OperationID = null;
                    toRow.LineCntrMatl = null;
                    toRow.LineCntrOvhd = null;
                    toRow.LineCntrStep = null;
                    toRow.LineCntrTool = null;
                    graph.BomOperRecords.Cache.Insert(toRow);
                    continue;
                }

                var ecoRow = PXCache<AMBomOper>.CreateCopy(existing);
                var bomRow = SelectFrom<AMBomOper>
                    .Where<AMBomOper.bOMID.IsEqual<@P.AsString>
                        .And<AMBomOper.revisionID.IsEqual<@P.AsString>
                            .And<AMBomOper.operationID.IsEqual<@P.AsInt>>>>
                    .View.Select(graph, ecr.BOMID, ecr.BOMRevisionID, fromRow.OperationID).FirstOrDefault();
                if (bomRow == null)
                {
                    graph.BomOperRecords.Cache.TwoWayMerge(ecoRow, fromRow, operExcludeFields);
                }
                else
                {
                    graph.BomOperRecords.Cache.ThreeWayMerge(bomRow, ecoRow, fromRow, operExcludeFields);
                }

                graph.BomOperRecords.Update(ecoRow);
            }
        }

        private void MergeECOMatl(ECOMaint graph, AMECOItem existingECO, AMECRItem ecr)
        {
            var childExcludeFields = ChildExcludeFields();

            foreach (AMBomMatl fromRow in SelectFrom<AMBomMatl>
                .Where<AMBomMatl.bOMID.IsEqual<@P.AsString>
                // Can use constant vs parameter
                .And<AMBomMatl.revisionID.IsEqual<AMECRItem.eCRRev>>>
                .View.Select(graph, ecr.ECRID))
            {
                if (fromRow.RowStatus == AMRowStatus.Unchanged)
                    continue;


                var existing = GetCachedECORow<AMBomMatl>(graph.BomMatlRecords.Cache, existingECO, fromRow.OperationID, fromRow.LineID);
                if (existing == null)
                {
                    var toRow = (AMBomMatl)graph.BomMatlRecords.Cache.CreateCopy(fromRow);
                    toRow.BOMID = existingECO.ECOID;
                    toRow.RevisionID = AMECOItem.ECORev;
                    graph.BomMatlRecords.Cache.Insert(toRow);
                    continue;
                }

                var ecoRow = PXCache<AMBomMatl>.CreateCopy(existing);
                var bomRow = SelectFrom<AMBomMatl>
                    .Where<AMBomMatl.bOMID.IsEqual<@P.AsString>
                        .And<AMBomMatl.revisionID.IsEqual<@P.AsString>
                            .And<AMBomMatl.operationID.IsEqual<@P.AsInt>>>
                            .And<AMBomMatl.lineID.IsEqual<@P.AsInt>>>
                    .View.Select(graph, ecr.BOMID, ecr.BOMRevisionID, fromRow.OperationID, fromRow.LineID).FirstOrDefault();
                if (bomRow == null)
                {
                    graph.BomMatlRecords.Cache.TwoWayMerge(ecoRow, fromRow, childExcludeFields);
                }
                else
                {
                    graph.BomMatlRecords.Cache.ThreeWayMerge(bomRow, ecoRow, fromRow, childExcludeFields);
                }

                graph.BomMatlRecords.Update(ecoRow);
            }
        }

        private void MergeECOTool(ECOMaint graph, AMECOItem existingECO, AMECRItem ecr)
        {
            var childExcludeFields = ChildExcludeFields();

            foreach (AMBomTool fromRow in SelectFrom<AMBomTool>
                .Where<AMBomTool.bOMID.IsEqual<@P.AsString>
                // Can use constant vs parameter
                .And<AMBomTool.revisionID.IsEqual<AMECRItem.eCRRev>>>
                .View.Select(graph, ecr.ECRID))
            {
                if (fromRow.RowStatus == AMRowStatus.Unchanged)
                    continue;


                var existing = GetCachedECORow<AMBomTool>(graph.BomToolRecords.Cache, existingECO, fromRow.OperationID, fromRow.LineID);
                if (existing == null)
                {
                    var toRow = (AMBomTool)graph.BomToolRecords.Cache.CreateCopy(fromRow);
                    toRow.BOMID = existingECO.ECOID;
                    toRow.RevisionID = AMECOItem.ECORev;
                    graph.BomToolRecords.Cache.Insert(toRow);
                    continue;
                }

                var ecoRow = PXCache<AMBomTool>.CreateCopy(existing);
                var bomRow = SelectFrom<AMBomTool>
                    .Where<AMBomTool.bOMID.IsEqual<@P.AsString>
                        .And<AMBomTool.revisionID.IsEqual<@P.AsString>
                            .And<AMBomTool.operationID.IsEqual<@P.AsInt>>>
                            .And<AMBomTool.lineID.IsEqual<@P.AsInt>>>
                    .View.Select(graph, ecr.BOMID, ecr.BOMRevisionID, fromRow.OperationID, fromRow.LineID).FirstOrDefault();
                if (bomRow == null)
                {
                    graph.BomToolRecords.Cache.TwoWayMerge(ecoRow, fromRow, childExcludeFields);
                }
                else
                {
                    graph.BomToolRecords.Cache.ThreeWayMerge(bomRow, ecoRow, fromRow, childExcludeFields);
                }

                graph.BomToolRecords.Update(ecoRow);
            }
        }

        private void MergeECOAttr(ECOMaint graph, AMECOItem existingECO, AMECRItem ecr)
        {
            var childExcludeFields = ChildExcludeFields();

            foreach (AMBomAttribute fromRow in SelectFrom<AMBomAttribute>
                .Where<AMBomAttribute.bOMID.IsEqual<@P.AsString>
                // Can use constant vs parameter
                .And<AMBomAttribute.revisionID.IsEqual<AMECRItem.eCRRev>>>
                .View.Select(graph, ecr.ECRID))
            {
                if (fromRow.RowStatus == AMRowStatus.Unchanged)
                    continue;


                var existing = GetCachedECORow<AMBomAttribute>(graph.BomAttributes.Cache, existingECO, fromRow.LineNbr);
                if (existing == null)
                {
                    var toRow = (AMBomAttribute)graph.BomAttributes.Cache.CreateCopy(fromRow);
                    toRow.BOMID = existingECO.ECOID;
                    toRow.RevisionID = AMECOItem.ECORev;
                    graph.BomAttributes.Cache.Insert(toRow);
                    continue;
                }

                var ecoRow = PXCache<AMBomAttribute>.CreateCopy(existing);
                var bomRow = SelectFrom<AMBomAttribute>
                    .Where<AMBomAttribute.bOMID.IsEqual<@P.AsString>
                        .And<AMBomAttribute.revisionID.IsEqual<@P.AsString>
                            .And<AMBomAttribute.lineNbr.IsEqual<@P.AsInt>>>>
                    .View.Select(graph, ecr.BOMID, ecr.BOMRevisionID, fromRow.LineNbr).FirstOrDefault();
                if (bomRow == null)
                {
                    graph.BomAttributes.Cache.TwoWayMerge(ecoRow, fromRow, childExcludeFields);
                }
                else
                {
                    graph.BomAttributes.Cache.ThreeWayMerge(bomRow, ecoRow, fromRow, childExcludeFields);
                }

                graph.BomAttributes.Update(ecoRow);
            }
        }

        private void MergeECOStep(ECOMaint graph, AMECOItem existingECO, AMECRItem ecr)
        {
            var childExcludeFields = ChildExcludeFields();

            foreach (AMBomStep fromRow in SelectFrom<AMBomStep>
                .Where<AMBomStep.bOMID.IsEqual<@P.AsString>
                // Can use constant vs parameter
                .And<AMBomStep.revisionID.IsEqual<AMECRItem.eCRRev>>>
                .View.Select(graph, ecr.ECRID))
            {
                if (fromRow.RowStatus == AMRowStatus.Unchanged)
                    continue;


                var existing = GetCachedECORow<AMBomStep>(graph.BomStepRecords.Cache, existingECO, fromRow.OperationID, fromRow.LineID);
                if (existing == null)
                {
                    var toRow = (AMBomStep)graph.BomStepRecords.Cache.CreateCopy(fromRow);
                    toRow.BOMID = existingECO.ECOID;
                    toRow.RevisionID = AMECOItem.ECORev;
                    graph.BomStepRecords.Cache.Insert(toRow);
                    continue;
                }

                var ecoRow = PXCache<AMBomStep>.CreateCopy(existing);
                var bomRow = SelectFrom<AMBomStep>
                    .Where<AMBomStep.bOMID.IsEqual<@P.AsString>
                        .And<AMBomStep.revisionID.IsEqual<@P.AsString>
                            .And<AMBomStep.operationID.IsEqual<@P.AsInt>>>
                            .And<AMBomStep.lineID.IsEqual<@P.AsInt>>>
                    .View.Select(graph, ecr.BOMID, ecr.BOMRevisionID, fromRow.OperationID, fromRow.LineID).FirstOrDefault();
                if (bomRow == null)
                {
                    graph.BomStepRecords.Cache.TwoWayMerge(ecoRow, fromRow, childExcludeFields);
                }
                else
                {
                    graph.BomStepRecords.Cache.ThreeWayMerge(bomRow, ecoRow, fromRow, childExcludeFields);
                }

                graph.BomStepRecords.Update(ecoRow);
            }
        }

        private void MergeECOOvhd(ECOMaint graph, AMECOItem existingECO, AMECRItem ecr)
        {
            var childExcludeFields = ChildExcludeFields();

            foreach (AMBomOvhd fromRow in SelectFrom<AMBomOvhd>
                .Where<AMBomOvhd.bOMID.IsEqual<@P.AsString>
                // Can use constant vs parameter
                .And<AMBomOvhd.revisionID.IsEqual<AMECRItem.eCRRev>>>
                .View.Select(graph, ecr.ECRID))
            {
                if (fromRow.RowStatus == AMRowStatus.Unchanged)
                    continue;


                var existing = GetCachedECORow<AMBomOvhd>(graph.BomOvhdRecords.Cache, existingECO, fromRow.OperationID, fromRow.LineID);
                if (existing == null)
                {
                    var toRow = (AMBomOvhd)graph.BomOvhdRecords.Cache.CreateCopy(fromRow);
                    toRow.BOMID = existingECO.ECOID;
                    toRow.RevisionID = AMECOItem.ECORev;
                    graph.BomOvhdRecords.Cache.Insert(toRow);
                    continue;
                }

                var ecoRow = PXCache<AMBomOvhd>.CreateCopy(existing);
                var bomRow = SelectFrom<AMBomOvhd>
                    .Where<AMBomOvhd.bOMID.IsEqual<@P.AsString>
                        .And<AMBomOvhd.revisionID.IsEqual<@P.AsString>
                            .And<AMBomOvhd.operationID.IsEqual<@P.AsInt>>>
                            .And<AMBomOvhd.lineID.IsEqual<@P.AsInt>>>
                    .View.Select(graph, ecr.BOMID, ecr.BOMRevisionID, fromRow.OperationID, fromRow.LineID).FirstOrDefault();
                if (bomRow == null)
                {
                    graph.BomOvhdRecords.Cache.TwoWayMerge(ecoRow, fromRow, childExcludeFields);
                }
                else
                {
                    graph.BomOvhdRecords.Cache.ThreeWayMerge(bomRow, ecoRow, fromRow, childExcludeFields);
                }

                graph.BomOvhdRecords.Update(ecoRow);
            }
        }

        private void MergeECORef(ECOMaint graph, AMECOItem existingECO, AMECRItem ecr)
        {
            var childExcludeFields = ChildExcludeFields();

            foreach (AMBomRef fromRow in SelectFrom<AMBomRef>
                .Where<AMBomRef.bOMID.IsEqual<@P.AsString>
                // Can use constant vs parameter
                .And<AMBomRef.revisionID.IsEqual<AMECRItem.eCRRev>>>
                .View.Select(graph, ecr.ECRID))
            {
                if (fromRow.RowStatus == AMRowStatus.Unchanged)
                    continue;


                var existing = GetCachedECORow<AMBomRef>(graph.BomRefRecords.Cache, existingECO, fromRow.OperationID, fromRow.LineID, fromRow.MatlLineID);
                if (existing == null)
                {
                    var toRow = (AMBomRef)graph.BomRefRecords.Cache.CreateCopy(fromRow);
                    toRow.BOMID = existingECO.ECOID;
                    toRow.RevisionID = AMECOItem.ECORev;
                    graph.BomRefRecords.Cache.Insert(toRow);
                    continue;
                }

                var ecoRow = PXCache<AMBomRef>.CreateCopy(existing);
                var bomRow = SelectFrom<AMBomRef>
                    .Where<AMBomRef.bOMID.IsEqual<@P.AsString>
                        .And<AMBomRef.revisionID.IsEqual<@P.AsString>
                            .And<AMBomRef.operationID.IsEqual<@P.AsInt>>>
                            .And<AMBomRef.lineID.IsEqual<@P.AsInt>>
                            .And<AMBomRef.matlLineID.IsEqual<@P.AsInt>>>
                    .View.Select(graph, ecr.BOMID, ecr.BOMRevisionID, fromRow.OperationID, fromRow.LineID, fromRow.MatlLineID).FirstOrDefault();
                if (bomRow == null)
                {
                    graph.BomRefRecords.Cache.TwoWayMerge(ecoRow, fromRow, childExcludeFields);
                }
                else
                {
                    graph.BomRefRecords.Cache.ThreeWayMerge(bomRow, ecoRow, fromRow, childExcludeFields);
                }

                graph.BomRefRecords.Update(ecoRow);
            }
        }

        protected virtual AMECOItem CreateECO(ECOMaint ecoGraph, AMECRItem ecr)
        {
            var newEco = ecoGraph.Documents.Insert();
            if (newEco == null)
            {
                return null;
            }
            ecoGraph.CopyECRtoECO(ecoGraph.Documents.Cache, newEco, ecr);
            ecoGraph.UpdateECRStatus(ecr, AMECRStatus.Completed);
            return newEco;
        }
    }

    [Serializable]
    [PXCacheName(Messages.CreateECOFilter)]
    public class CreateECOsFilter : IBqlTable
    {
        #region MergeECRs

        public abstract class mergeECRs : PX.Data.BQL.BqlBool.Field<mergeECRs> { }

        protected Boolean? _MergeECRs;

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Merge ECRs")]
        public virtual Boolean? MergeECRs
        {
            get { return this._MergeECRs; }
            set { this._MergeECRs = value; }
        }

        #endregion
    }
}
