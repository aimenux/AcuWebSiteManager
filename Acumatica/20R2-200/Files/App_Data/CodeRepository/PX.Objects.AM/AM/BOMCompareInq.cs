using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Web.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AM
{
    public class BOMCompareInq : PXGraph<BOMCompareInq>
    {
        public PXFilter<BOMCompareFilter> Filter;

        public PXSelect<
            AMBomOper,
            Where<AMBomOper.bOMID, Equal<Current<BOMCompareFilter.bOMID1>>,
                And<AMBomOper.revisionID, Equal<Current<BOMCompareFilter.revisionID1>>>>>
            OperTree1;

        public PXSelect<
            AMBomOper,
            Where<AMBomOper.bOMID, Equal<Current<BOMCompareFilter.bOMID2>>,
                And<AMBomOper.revisionID, Equal<Current<BOMCompareFilter.revisionID2>>>>>
            OperTree2;

        public PXSelectJoin<
            AMBomMatl,
            InnerJoin<InventoryItem,
                On<AMBomMatl.inventoryID,
                    Equal<InventoryItem.inventoryID>>>,
            Where<AMBomMatl.bOMID, Equal<Current<BOMCompareFilter.bOMID1>>,
                And<AMBomMatl.revisionID, Equal<Current<BOMCompareFilter.revisionID1>>,
                And<AMBomMatl.operationID, Equal<Optional<AMBomMatl.operationID>>>>>> 
            MatlTree1;

        public PXSelectJoin<
            AMBomMatl,
            InnerJoin<InventoryItem,
                On<AMBomMatl.inventoryID,
                    Equal<InventoryItem.inventoryID>>>,
            Where<AMBomMatl.bOMID, Equal<Current<BOMCompareFilter.bOMID2>>,
                And<AMBomMatl.revisionID, Equal<Current<BOMCompareFilter.revisionID2>>,
                And<AMBomMatl.operationID, Equal<Optional<AMBomMatl.operationID>>>>>> 
            MatlTree2;

        public PXSelect<
            AMBomStep,
            Where<AMBomStep.bOMID, Equal<Current<BOMCompareFilter.bOMID1>>,
                And<AMBomStep.revisionID, Equal<Current<BOMCompareFilter.revisionID1>>,
                And<AMBomStep.operationID, Equal<Optional<AMBomStep.operationID>>>>>> 
            StepTree1;

        public PXSelect<
            AMBomStep,
            Where<AMBomStep.bOMID, Equal<Current<BOMCompareFilter.bOMID2>>,
                And<AMBomStep.revisionID, Equal<Current<BOMCompareFilter.revisionID2>>,
                And<AMBomStep.operationID, Equal<Optional<AMBomStep.operationID>>>>>> 
            StepTree2;

        public SelectFrom<AMBomTool>.InnerJoin<AMToolMst>
                    .On<AMBomTool.toolID.IsEqual<AMToolMst.toolID>>
                    .Where<AMBomTool.bOMID.IsEqual<BOMCompareFilter.bOMID1.FromCurrent>
                        .And<AMBomTool.revisionID.IsEqual<BOMCompareFilter.revisionID1.FromCurrent>
                            .And<AMBomTool.operationID.IsEqual<AMBomTool.operationID.AsOptional>>>>.View
            ToolTree1;

        public SelectFrom<AMBomTool>.InnerJoin<AMToolMst>
                    .On<AMBomTool.toolID.IsEqual<AMToolMst.toolID>>
                    .Where<AMBomTool.bOMID.IsEqual<BOMCompareFilter.bOMID2.FromCurrent>
                        .And<AMBomTool.revisionID.IsEqual<BOMCompareFilter.revisionID2.FromCurrent>
                            .And<AMBomTool.operationID.IsEqual<AMBomTool.operationID.AsOptional>>>>.View
            ToolTree2;

        public SelectFrom<AMBomOvhd>.InnerJoin<AMOverhead>
            .On<AMBomOvhd.ovhdID.IsEqual<AMOverhead.ovhdID>>
            .Where<AMBomOvhd.bOMID.IsEqual<BOMCompareFilter.bOMID1.FromCurrent>
                .And<AMBomOvhd.revisionID.IsEqual<BOMCompareFilter.revisionID1.FromCurrent>>
                .And<AMBomOvhd.operationID.IsEqual<AMBomOvhd.operationID.AsOptional>>>.View
            OvhdTree1;
                

        public SelectFrom<AMBomOvhd>.InnerJoin<AMOverhead>
            .On<AMBomOvhd.ovhdID.IsEqual<AMOverhead.ovhdID>>
            .Where<AMBomOvhd.bOMID.IsEqual<BOMCompareFilter.bOMID2.FromCurrent>
                .And<AMBomOvhd.revisionID.IsEqual<BOMCompareFilter.revisionID2.FromCurrent>>
                .And<AMBomOvhd.operationID.IsEqual<AMBomOvhd.operationID.AsOptional>>>.View
            OvhdTree2;

        public PXSelect<
            AMBomOper,
            Where<AMBomOper.noteID, Equal<Argument<string>>,
                Or<AMBomOper.noteID, Equal<Argument<string>>>>>
            BomOperRecords;


        public PXSelect<
            AMBomMatl, 
            Where<AMBomMatl.noteID, Equal<Argument<string>>,
                Or<AMBomMatl.noteID, Equal<Argument<string>>>>> 
            BomMatlRecords;

        public BOMCompareInq()
        {
            PXUIFieldAttribute.SetVisible<AMBomMatl.bOMID>(BomMatlRecords.Cache, null);
            PXUIFieldAttribute.SetVisible<AMBomMatl.revisionID>(BomMatlRecords.Cache, null);

            PXUIFieldAttribute.SetVisible<AMBomStep.bOMID>(BomStepRecords.Cache, null);
            PXUIFieldAttribute.SetVisible<AMBomStep.revisionID>(BomStepRecords.Cache, null);

            PXUIFieldAttribute.SetVisible<AMBomTool.bOMID>(BomToolRecords.Cache, null);
            PXUIFieldAttribute.SetVisible<AMBomTool.revisionID>(BomToolRecords.Cache, null);

            PXUIFieldAttribute.SetVisible<AMBomOvhd.bOMID>(BomOvhdRecords.Cache, null);
            PXUIFieldAttribute.SetVisible<AMBomOvhd.revisionID>(BomOvhdRecords.Cache, null);

            if (Features.ECCEnabled())
            {
                // To show ECR,ECO, BOM as a generic ID field
                PXUIFieldAttribute.SetDisplayName<AMBomMatl.bOMID>(BomMatlRecords.Cache, "ID");
                PXUIFieldAttribute.SetDisplayName<AMBomStep.bOMID>(BomStepRecords.Cache, "ID");
                PXUIFieldAttribute.SetDisplayName<AMBomTool.bOMID>(BomToolRecords.Cache, "ID");
                PXUIFieldAttribute.SetDisplayName<AMBomOvhd.bOMID>(BomOvhdRecords.Cache, "ID");
            }
        }

        protected virtual IEnumerable bomMatlRecords([PXString]string detailLineNbr1, [PXString]string detailLineNbr2)
        {
            var type1 = string.Empty;
            var note1 = string.Empty;
            var type2 = string.Empty;
            var note2 = string.Empty;

            if (string.IsNullOrEmpty(detailLineNbr1) && string.IsNullOrEmpty(detailLineNbr2))
                return new List<AMBomMatl>();
            if (!string.IsNullOrEmpty(detailLineNbr1))
            {
                var parms1 = detailLineNbr1.Split(';');
                type1 = parms1[0];
                note1 = parms1[1];
            }
            if (!string.IsNullOrEmpty(detailLineNbr2))
            {
                var parms2 = detailLineNbr2.Split(';');
                type2 = parms2[0];
                note2 = parms2[1];
            }

            var list = new List<AMBomMatl>();
            if (type1 == OperCategories.MATL.ToString() && type2 == OperCategories.MATL.ToString())
            {
                var rows = SelectFrom<AMBomMatl>
                    .Where<AMBomMatl.noteID.IsEqual<@P.AsGuid>
                    .Or<AMBomMatl.noteID.IsEqual<@P.AsGuid>>>
                    .View.Select(this, new Guid(note1), new Guid(note2));
                foreach (AMBomMatl row in rows)
                {
                    row.Selected = (row.NoteID.ToString() == note2);
                    list.Add(row);
                }
            }
            else if (type1 == OperCategories.MATL.ToString())
            {
                AMBomMatl row1 = SelectFrom<AMBomMatl>.Where<AMBomMatl.noteID.IsEqual<@P.AsGuid>>.View.Select(this, new Guid(note1)).First();
                list.Add(row1);
            }
            else if (type2 == OperCategories.MATL.ToString())
            {
                AMBomMatl row2 = SelectFrom<AMBomMatl>.Where<AMBomMatl.noteID.IsEqual<@P.AsGuid>>.View.Select(this, new Guid(note2)).First();
                row2.Selected = true;
                list.Add(row2);
            }

            return list;

        }

        public PXSelect<
            AMBomStep, 
            Where<AMBomStep.bOMID, Equal<Current<BOMCompareFilter.bOMID1>>,
                And<AMBomStep.revisionID, Equal<Current<BOMCompareFilter.revisionID1>>,
                And<AMBomStep.lineID, Equal<Argument<string>>>>>> 
            BomStepRecords;

        protected virtual IEnumerable bomStepRecords([PXString]string detailLineNbr1, [PXString]string detailLineNbr2)
        {
            var type1 = string.Empty;
            var note1 = string.Empty;
            var type2 = string.Empty;
            var note2 = string.Empty;


            if (string.IsNullOrEmpty(detailLineNbr1) && string.IsNullOrEmpty(detailLineNbr2))
                return new List<AMBomStep>();
            if (!string.IsNullOrEmpty(detailLineNbr1))
            {
                var parms1 = detailLineNbr1.Split(';');
                type1 = parms1[0];
                note1 = parms1[1];
            }
            if (!string.IsNullOrEmpty(detailLineNbr2))
            {
                var parms2 = detailLineNbr2.Split(';');
                type2 = parms2[0];
                note2 = parms2[1];
            }

            var list = new List<AMBomStep>();
            if (type1 == OperCategories.STEP.ToString() && type2 == OperCategories.STEP.ToString())
            {
                var rows = SelectFrom<AMBomStep>
                    .Where<AMBomStep.noteID.IsEqual<@P.AsGuid>
                    .Or<AMBomStep.noteID.IsEqual<@P.AsGuid>>>
                    .View.Select(this, new Guid(note1), new Guid(note2));
                foreach (AMBomStep row in rows)
                {
                    row.Selected = (row.NoteID.ToString() == note2);
                    list.Add(row);
                }
            }
            else if (type1 == OperCategories.STEP.ToString())
            {
                AMBomStep row1 = SelectFrom<AMBomStep>.Where<AMBomStep.noteID.IsEqual<@P.AsGuid>>.View.Select(this, new Guid(note1)).First();
                list.Add(row1);
            }
            else if (type2 == OperCategories.STEP.ToString())
            {
                AMBomStep row2 = SelectFrom<AMBomStep>.Where<AMBomStep.noteID.IsEqual<@P.AsGuid>>.View.Select(this, new Guid(note2)).First();
                row2.Selected = true;
                list.Add(row2);
            }

            return list;
        }

        public SelectFrom<AMBomTool>.InnerJoin<AMToolMst>
                    .On<AMBomTool.toolID.IsEqual<AMToolMst.toolID>>
                    .Where<AMBomTool.toolID.IsEqual<Argument.AsString>
                        .Or<AMBomTool.toolID.IsEqual<Argument.AsString>>>.View BomToolRecords;

        protected virtual IEnumerable bomToolRecords([PXString]string detailLineNbr1, [PXString]string detailLineNbr2)
        {
            var type1 = string.Empty;
            var note1 = string.Empty;
            var type2 = string.Empty;
            var note2 = string.Empty;


            if (string.IsNullOrEmpty(detailLineNbr1) && string.IsNullOrEmpty(detailLineNbr2))
                return new List<PXResult<AMBomTool, AMToolMst>>();
            if (!string.IsNullOrEmpty(detailLineNbr1))
            {
                var parms1 = detailLineNbr1.Split(';');
                type1 = parms1[0];
                note1 = parms1[1];
            }
            if (!string.IsNullOrEmpty(detailLineNbr2))
            {
                var parms2 = detailLineNbr2.Split(';');
                type2 = parms2[0];
                note2 = parms2[1];
            }

            var list = new List<PXResult<AMBomTool, AMToolMst>>();
            if (type1 == OperCategories.TOOL.ToString() && type2 == OperCategories.TOOL.ToString())
            {
                var rows = SelectFrom<AMBomTool>.InnerJoin<AMToolMst>
                    .On<AMBomTool.toolID.IsEqual<AMToolMst.toolID>>
                    .Where<AMBomTool.noteID.IsEqual<@P.AsGuid>
                    .Or<AMBomTool.noteID.IsEqual<@P.AsGuid>>>
                    .View.Select(this, new Guid(note1), new Guid(note2));
                foreach (PXResult<AMBomTool, AMToolMst> row in rows)
                {
                    ((AMBomTool)row).Selected = ((AMBomTool)row).NoteID.ToString() == note2;
                    list.Add(row);
                }
            }
            else if (type1 == OperCategories.TOOL.ToString())
            {
                PXResult<AMBomTool, AMToolMst> row1 = (PXResult<AMBomTool, AMToolMst>)SelectFrom<AMBomTool>.InnerJoin<AMToolMst>
                    .On<AMBomTool.toolID.IsEqual<AMToolMst.toolID>>
                    .Where<AMBomTool.noteID.IsEqual<@P.AsGuid>>.View.Select(this, new Guid(note1)).First();
                list.Add(row1);
            }
            else if (type2 == OperCategories.TOOL.ToString())
            {
                PXResult<AMBomTool, AMToolMst> row2 = (PXResult<AMBomTool, AMToolMst>)SelectFrom<AMBomTool>.InnerJoin<AMToolMst>
                    .On<AMBomTool.toolID.IsEqual<AMToolMst.toolID>>
                    .Where<AMBomTool.noteID.IsEqual<@P.AsGuid>>.View.Select(this, new Guid(note2)).First();
                ((AMBomTool)row2).Selected = true;
                list.Add(row2);
            }

            return list;
        }

        public SelectFrom<AMBomOvhd>
                    .InnerJoin<AMOverhead>
                    .On<AMBomOvhd.ovhdID.IsEqual<AMOverhead.ovhdID>>
                    .Where<AMBomOvhd.ovhdID.IsEqual<Argument.AsString>
                        .Or<AMBomOvhd.ovhdID.IsEqual<Argument.AsString>>>.View BomOvhdRecords;

        protected virtual IEnumerable bomOvhdRecords([PXString]string detailLineNbr1, [PXString]string detailLineNbr2)
        {
            var type1 = string.Empty;
            var note1 = string.Empty;
            var type2 = string.Empty;
            var note2 = string.Empty;


            if (string.IsNullOrEmpty(detailLineNbr1) && string.IsNullOrEmpty(detailLineNbr2))
                return new List<PXResult<AMBomOvhd, AMOverhead>>();
            if (!string.IsNullOrEmpty(detailLineNbr1))
            {
                var parms1 = detailLineNbr1.Split(';');
                type1 = parms1[0];
                note1 = parms1[1];
            }
            if (!string.IsNullOrEmpty(detailLineNbr2))
            {
                var parms2 = detailLineNbr2.Split(';');
                type2 = parms2[0];
                note2 = parms2[1];
            }

            var list = new List<PXResult<AMBomOvhd, AMOverhead>>();
            if (type1 == OperCategories.OVHD.ToString() && type2 == OperCategories.OVHD.ToString())
            {
                var rows = SelectFrom<AMBomOvhd>
                    .InnerJoin<AMOverhead>
                    .On<AMBomOvhd.ovhdID.IsEqual<AMOverhead.ovhdID>>
                    .Where<AMBomOvhd.noteID.IsEqual<@P.AsGuid>
                    .Or<AMBomOvhd.noteID.IsEqual<@P.AsGuid>>>.View.Select(this, new Guid(note1), new Guid(note2));
                foreach (PXResult<AMBomOvhd, AMOverhead> row in rows)
                {
                    ((AMBomOvhd)row).Selected = ((AMBomOvhd)row).NoteID.ToString() == note2;
                    list.Add(row);
                }
            }
            else if (type1 == OperCategories.OVHD.ToString())
            {
                PXResult<AMBomOvhd, AMOverhead> row1 = (PXResult<AMBomOvhd, AMOverhead>)SelectFrom<AMBomOvhd>
                    .InnerJoin<AMOverhead>
                    .On<AMBomOvhd.ovhdID.IsEqual<AMOverhead.ovhdID>>
                    .Where<AMBomOvhd.noteID.IsEqual<@P.AsGuid>>.View.Select(this, new Guid(note1)).First();
                list.Add(row1);
            }
            else if (type2 == OperCategories.OVHD.ToString())
            {
                PXResult<AMBomOvhd, AMOverhead> row2 = (PXResult<AMBomOvhd, AMOverhead>)SelectFrom<AMBomOvhd>
                    .InnerJoin<AMOverhead>
                    .On<AMBomOvhd.ovhdID.IsEqual<AMOverhead.ovhdID>>
                    .Where<AMBomOvhd.noteID.IsEqual<@P.AsGuid>>.View.Select(this, new Guid(note2)).First();
                ((AMBomOvhd)row2).Selected = true;
                list.Add(row2);
            }

            return list;
        }

        public SelectFrom<AMBOMCompareTreeNode>
            .Where<AMBOMCompareTreeNode.parentID.IsEqual<Argument.AsString>
                .And<AMBOMCompareTreeNode.lineNbr.IsEqual<Argument.AsInt>>
                .And<AMBOMCompareTreeNode.categoryNbr.IsEqual<Argument.AsInt>>
                .And<AMBOMCompareTreeNode.detailLineNbr.IsEqual<Argument.AsString>>>
            .OrderBy<Asc<AMBOMCompareTreeNode.sortOrder>>.View Tree1;
            

        protected virtual IEnumerable tree1([PXString]string parentID, [PXInt]int? lineNbr, [PXInt]int? categoryNbr, [PXString]string detailLineNbr)
        {
            if(parentID == null)
            {
                switch(Filter.Current.IDType1)
                {
                    case IDTypes.BOM:
                        var parentbom = SelectFrom<AMBomItem>
                            .InnerJoin<InventoryItem>
                            .On<AMBomItem.inventoryID.IsEqual<InventoryItem.inventoryID>>
                            .Where<AMBomItem.bOMID.IsEqual<BOMCompareFilter.bOMID1.FromCurrent>
                            .And<AMBomItem.revisionID.IsEqual<BOMCompareFilter.revisionID1.FromCurrent>>>.View.Select(this);
                        foreach(PXResult<AMBomItem, InventoryItem> parent in parentbom)
                        {
                            var item = new AMBOMCompareTreeNode
                            {
                                ParentID = ((AMBomItem)parent).BOMID,
                                Label = ParentDescrDisplay((AMBomItem)parent, (InventoryItem)parent),
                                // item.SortOrder = oper.SortOrder;
                                ToolTip = string.Empty,
                                Icon = Sprite.Main.GetFullUrl(Sprite.Tree.Folder)
                            };

                            yield return item;
                        }
                        break;
                    case IDTypes.ECR:
                        var parentECR = SelectFrom<AMECRItem>
                            .InnerJoin<InventoryItem>
                            .On<AMECRItem.inventoryID.IsEqual<InventoryItem.inventoryID>>
                            .Where<AMECRItem.eCRID.IsEqual<BOMCompareFilter.bOMID1.FromCurrent>>.View.Select(this);
                        foreach (PXResult<AMECRItem, InventoryItem> parent in parentECR)
                        {
                            var item = new AMBOMCompareTreeNode
                            {
                                ParentID = ((AMECRItem)parent).ECRID,
                                Label = ParentDescrDisplay((AMECRItem)parent, (InventoryItem)parent),
                                // item.SortOrder = oper.SortOrder;
                                ToolTip = string.Empty,
                                Icon = Sprite.Main.GetFullUrl(Sprite.Tree.Folder)
                            };

                            yield return item;
                        }
                        break;
                    case IDTypes.ECO:
                        var parentECO = SelectFrom<AMECOItem>
                            .InnerJoin<InventoryItem>
                            .On<AMECOItem.inventoryID.IsEqual<InventoryItem.inventoryID>>
                            .Where<AMECOItem.eCOID.IsEqual<BOMCompareFilter.bOMID1.FromCurrent>>.View.Select(this);
                        foreach (PXResult<AMECOItem, InventoryItem> parent in parentECO)
                        {
                            var item = new AMBOMCompareTreeNode
                            {
                                ParentID = ((AMECOItem)parent).ECOID,
                                Label = ParentDescrDisplay((AMECOItem)parent, (InventoryItem)parent),
                                // item.SortOrder = oper.SortOrder;
                                ToolTip = string.Empty,
                                Icon = Sprite.Main.GetFullUrl(Sprite.Tree.Folder)
                            };

                            yield return item;
                        }
                        break;
                }
            }
            else if (lineNbr == null)
            {
                // Get operations related to current BOM
                var results = PXSelect<
                    AMBomOper, 
                    Where<AMBomOper.bOMID, Equal<Current<BOMCompareFilter.bOMID1>>,
                        And<AMBomOper.revisionID, Equal<Current<BOMCompareFilter.revisionID1>>>>,
                        OrderBy<Asc<AMBomOper.operationCD>>>
                    .Select(this);

                // Set the icon based on completed status.
                for(var i = 0; i<results.Count;i++)
                {
                    AMBomOper oper = results[i];
                    var item = new AMBOMCompareTreeNode
                    {
                        ParentID = parentID,
                        LineNbr = oper.OperationID,
                        Label = OperDescrDisplay(oper.OperationCD, oper.WcID, oper.Descr),
                        SortOrder = i,
                        ToolTip = string.Empty,
                        Icon = GetIcon(1, oper)
                    };

                    yield return item;
                }
            }
            else if (categoryNbr == null)
            {
                foreach (var category in OperCategories.List())
                {
                    AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                    {
                        ParentID = parentID,
                        LineNbr = lineNbr,
                        CategoryNbr = category.Key,
                        Label = category.Value,
                        ToolTip = string.Empty,
                        Icon = Sprite.Main.GetFullUrl(Sprite.Tree.Folder)
                    };
                    yield return item;
                }
            }
            else if (detailLineNbr == null)
            {
                switch (categoryNbr)
                {
                    case OperCategories.MATL:
                        var matls = MatlTree1.Select(lineNbr);
                        foreach (PXResult<AMBomMatl, InventoryItem> matl in matls)
                        {
                            var bommatl = (AMBomMatl)matl;
                            var initem = (InventoryItem)matl;
                            AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                            {
                                ParentID = parentID,
                                LineNbr = lineNbr,
                                CategoryNbr = categoryNbr,
                                DetailLineNbr = string.Join(";", categoryNbr, bommatl.NoteID),
                                Label = INDescrDisplay(initem.InventoryCD, bommatl.Descr),
                                ToolTip = string.Empty,
                                Icon = GetIcon(1, matl),
                                SortOrder = bommatl.SortOrder
                            };
                            yield return item;
                        }
                        break;
                    case OperCategories.STEP:
                        var steps = StepTree1.Select(lineNbr);
                        foreach (AMBomStep step in steps)
                        {
                            AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                            {
                                ParentID = parentID,
                                LineNbr = lineNbr,
                                CategoryNbr = categoryNbr,
                                DetailLineNbr = string.Join(";", categoryNbr, step.NoteID),
                                Label = step.Descr,
                                ToolTip = string.Empty,
                                Icon = GetIcon(1,step),
                                SortOrder = step.SortOrder
                            };
                            yield return item;
                        }
                        break;
                    case OperCategories.TOOL:
                        var tools = ToolTree1.Select(lineNbr);
                        foreach (PXResult<AMBomTool,AMToolMst> result in tools)
                        {
                            var tool = (AMBomTool)result;
                            var toolmst = (AMToolMst)result;
                            AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                            {
                                ParentID = parentID,
                                LineNbr = lineNbr,
                                CategoryNbr = categoryNbr,
                                DetailLineNbr = string.Join(";", categoryNbr, tool.NoteID),
                                Label = tool.ToolID + " - " + tool.Descr,
                                ToolTip = string.Empty,
                                Icon = GetIcon(1, tool)
                            };
                            yield return item;
                        }
                        break;
                    case OperCategories.OVHD:
                        var overheads = OvhdTree1.Select(lineNbr);
                        foreach (PXResult<AMBomOvhd,AMOverhead> result in overheads)
                        {
                            var ovhd = (AMBomOvhd)result;
                            var ovhdmst = (AMOverhead)result;
                            AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                            {
                                ParentID = parentID,
                                LineNbr = lineNbr,
                                CategoryNbr = categoryNbr,
                                DetailLineNbr = string.Join(";", categoryNbr, ovhd.NoteID),
                                Label = ovhd.OvhdID + " - " + ovhdmst.Descr,
                                ToolTip = string.Empty,
                                Icon = GetIcon(1, ovhd)
                            };
                            yield return item;
                        }
                        break;
                }
            }
        }

        public SelectFrom<AMBOMCompareTreeNode>
            .Where<AMBOMCompareTreeNode.parentID.IsEqual<Argument.AsString>
                .And<AMBOMCompareTreeNode.lineNbr.IsEqual<Argument.AsInt>>
                .And<AMBOMCompareTreeNode.categoryNbr.IsEqual<Argument.AsInt>>
                .And<AMBOMCompareTreeNode.detailLineNbr.IsEqual<Argument.AsString>>>
            .OrderBy<Asc<AMBOMCompareTreeNode.sortOrder>>.View Tree2;

        protected virtual IEnumerable tree2([PXString]string parentID, [PXInt]int? lineNbr, [PXInt]int? categoryNbr, [PXString]string detailLineNbr)
        {
            if (parentID == null)
            {
                switch (Filter.Current.IDType2)
                {
                    case IDTypes.BOM:
                        var parentbom = SelectFrom<AMBomItem>
                            .InnerJoin<InventoryItem>
                            .On<AMBomItem.inventoryID.IsEqual<InventoryItem.inventoryID>>
                            .Where<AMBomItem.bOMID.IsEqual<BOMCompareFilter.bOMID2.FromCurrent>
                            .And<AMBomItem.revisionID.IsEqual<BOMCompareFilter.revisionID2.FromCurrent>>>.View.Select(this);
                        foreach (PXResult<AMBomItem, InventoryItem> parent in parentbom)
                        {
                            var item = new AMBOMCompareTreeNode
                            {
                                ParentID = ((AMBomItem)parent).BOMID,
                                Label = ParentDescrDisplay((AMBomItem)parent, (InventoryItem)parent),
                                // item.SortOrder = oper.SortOrder;
                                ToolTip = string.Empty,
                                Icon = Sprite.Main.GetFullUrl(Sprite.Tree.Folder)
                            };

                            yield return item;
                        }
                        break;
                    case IDTypes.ECR:
                        var parentECR = SelectFrom<AMECRItem>
                            .InnerJoin<InventoryItem>
                            .On<AMECRItem.inventoryID.IsEqual<InventoryItem.inventoryID>>
                            .Where<AMECRItem.eCRID.IsEqual<BOMCompareFilter.bOMID2.FromCurrent>>.View.Select(this);
                        foreach (PXResult<AMECRItem, InventoryItem> parent in parentECR)
                        {
                            var item = new AMBOMCompareTreeNode
                            {
                                ParentID = ((AMECRItem)parent).ECRID,
                                Label = ParentDescrDisplay((AMECRItem)parent, (InventoryItem)parent),
                                // item.SortOrder = oper.SortOrder;
                                ToolTip = string.Empty,
                                Icon = Sprite.Main.GetFullUrl(Sprite.Tree.Folder)
                            };

                            yield return item;
                        }
                        break;
                    case IDTypes.ECO:
                        var parentECO = SelectFrom<AMECOItem>
                            .InnerJoin<InventoryItem>
                            .On<AMECOItem.inventoryID.IsEqual<InventoryItem.inventoryID>>
                            .Where<AMECOItem.eCOID.IsEqual<BOMCompareFilter.bOMID2.FromCurrent>>.View.Select(this);
                        foreach (PXResult<AMECOItem, InventoryItem> parent in parentECO)
                        {
                            var item = new AMBOMCompareTreeNode
                            {
                                ParentID = ((AMECOItem)parent).ECOID,
                                Label = ParentDescrDisplay((AMECOItem)parent, (InventoryItem)parent),
                                // item.SortOrder = oper.SortOrder;
                                ToolTip = string.Empty,
                                Icon = Sprite.Main.GetFullUrl(Sprite.Tree.Folder)
                            };

                            yield return item;
                        }
                        break;
                }
            }
            else if (lineNbr == null)
            {
                // Get operations related to current BOM
                var results = PXSelect<
                    AMBomOper,
                    Where<AMBomOper.bOMID, Equal<Current<BOMCompareFilter.bOMID2>>,
                        And<AMBomOper.revisionID, Equal<Current<BOMCompareFilter.revisionID2>>>>,
                        OrderBy<Asc<AMBomOper.operationCD>>>
                    .Select(this);

                // Set the icon based on completed status.
                for (var i = 0; i < results.Count; i++)
                {
                    AMBomOper oper = results[i];
                    var item = new AMBOMCompareTreeNode
                    {
                        ParentID = parentID,
                        LineNbr = oper.OperationID,
                        Label = OperDescrDisplay(oper.OperationCD, oper.WcID, oper.Descr),
                        SortOrder = i,
                        ToolTip = string.Empty,
                        Icon = GetIcon(2, oper)
                    };

                    yield return item;
                }
            }
            else if (categoryNbr == null)
            {
                foreach (var category in OperCategories.List())
                {
                    AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                    {
                        ParentID = parentID,
                        LineNbr = lineNbr,
                        CategoryNbr = category.Key,
                        Label = category.Value,
                        ToolTip = string.Empty,
                        Icon = Sprite.Main.GetFullUrl(Sprite.Main.Folder)
                    };
                    yield return item;
                }
            }
            else if (detailLineNbr == null)
            {
                switch (categoryNbr)
                {
                    case OperCategories.MATL:
                        var matls = MatlTree2.Select(lineNbr);
                        foreach (PXResult<AMBomMatl, InventoryItem> matl in matls)
                        {
                            var bommatl = (AMBomMatl)matl;
                            var initem = (InventoryItem)matl;
                            AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                            {
                                ParentID = parentID,
                                LineNbr = lineNbr,
                                CategoryNbr = categoryNbr,
                                DetailLineNbr = string.Join(";", categoryNbr, bommatl.NoteID),
                                Label = INDescrDisplay(initem.InventoryCD, bommatl.Descr),
                                ToolTip = string.Empty,
                                Icon = GetIcon(2, matl),
                                SortOrder = bommatl.SortOrder
                            };
                            yield return item;
                        }
                        break;
                    case OperCategories.STEP:
                        var steps = StepTree2.Select(lineNbr);
                        foreach (AMBomStep step in steps)
                        {
                            AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                            {
                                ParentID = parentID,
                                LineNbr = lineNbr,
                                CategoryNbr = categoryNbr,
                                DetailLineNbr = string.Join(";", categoryNbr, step.NoteID),
                                Label = step.Descr,
                                ToolTip = string.Empty,
                                Icon = GetIcon(2, step),
                                SortOrder = step.SortOrder
                            };
                            yield return item;
                        }
                        break;
                    case OperCategories.TOOL:
                        var tools = ToolTree2.Select(lineNbr);
                        foreach (PXResult<AMBomTool, AMToolMst> result in tools)
                        {
                            var tool = (AMBomTool)result;
                            var toolmst = (AMToolMst)result;
                            AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                            {
                                ParentID = parentID,
                                LineNbr = lineNbr,
                                CategoryNbr = categoryNbr,
                                DetailLineNbr = string.Join(";", categoryNbr, tool.NoteID),
                                Label = tool.ToolID + " - " + tool.Descr,
                                ToolTip = string.Empty,
                                Icon = GetIcon(2, tool)
                            };
                            yield return item;
                        }
                        break;
                    case OperCategories.OVHD:
                        var overheads = OvhdTree2.Select(lineNbr);
                        foreach (PXResult<AMBomOvhd, AMOverhead> result in overheads)
                        {
                            var ovhd = (AMBomOvhd)result;
                            var ovhdmst = (AMOverhead)result;
                            AMBOMCompareTreeNode item = new AMBOMCompareTreeNode
                            {
                                ParentID = parentID,
                                LineNbr = lineNbr,
                                CategoryNbr = categoryNbr,
                                DetailLineNbr = string.Join(";", categoryNbr, ovhd.NoteID),
                                Label = ovhd.OvhdID + " - " + ovhdmst.Descr,
                                ToolTip = string.Empty,
                                Icon = GetIcon(2, ovhd)
                            };
                            yield return item;
                        }
                        break;
                }
            }
        }

        private string GetIcon(int treeNum, AMBomOvhd ovhd)
        {
            AMBomOvhd matchRec;
            if (treeNum == 1)
            {
                if (Tree2Blank())
                    return IconTypes.DEFAULT;
                matchRec = OvhdTree2.Search<AMBomOvhd.ovhdID>(ovhd.OvhdID, ovhd.OperationID);
            }
            else
            {
                if (Tree1Blank())
                    return IconTypes.DEFAULT;
                matchRec = OvhdTree1.Search<AMBomOvhd.ovhdID>(ovhd.OvhdID, ovhd.OperationID);
            }
            if (matchRec == null)
                return IconTypes.NOMATCH;
            else
            {
                if (ObjCompareHelper.ChangesExist(BomOvhdRecords.Cache, matchRec, ovhd, "BOMID", "RevisionID"))
                    return IconTypes.PARTIAL;
                else
                    return IconTypes.MATCH;
            }
        }

        private string GetIcon(int treeNum, AMBomTool tool)
        {
            AMBomTool matchRec;
            if (treeNum == 1)
            {
                if (Tree2Blank())
                    return IconTypes.DEFAULT;
                matchRec = ToolTree2.Search<AMBomTool.toolID>(tool.ToolID, tool.OperationID);
            }
            else
            {
                if (Tree1Blank())
                    return IconTypes.DEFAULT;
                matchRec = ToolTree1.Search<AMBomTool.toolID>(tool.ToolID, tool.OperationID);
            }
            if (matchRec == null)
                return IconTypes.NOMATCH;
            else
            {
                if (ObjCompareHelper.ChangesExist(BomToolRecords.Cache, matchRec, tool, "BOMID", "RevisionID"))
                    return IconTypes.PARTIAL;
                else
                    return IconTypes.MATCH;
            }
        }

        private string GetIcon(int treeNum, AMBomStep step)
        {
            AMBomStep matchRec;
            if (treeNum == 1)
            {
                if (Tree2Blank())
                    return IconTypes.DEFAULT;
                matchRec = StepTree2.Search<AMBomStep.lineID>(step.LineID, step.OperationID);
            }
            else
            {
                if (Tree1Blank())
                    return IconTypes.DEFAULT;
                matchRec = StepTree1.Search<AMBomStep.lineID>(step.LineID, step.OperationID);
            }
            if (matchRec == null)
                return IconTypes.NOMATCH;
            else
            {
                if (ObjCompareHelper.ChangesExist(BomStepRecords.Cache, matchRec, step, "BOMID", "RevisionID"))
                    return IconTypes.PARTIAL;
                else
                    return IconTypes.MATCH;
            }
        }

        private string GetIcon(int treeNum, PXResult<AMBomMatl, InventoryItem> matlItem)
        {
            var matl = (AMBomMatl)matlItem;
            AMBomMatl matchRec;
            if (treeNum == 1)
            {
                if (Tree2Blank())
                    return IconTypes.DEFAULT;
                matchRec = MatlTree2.Search<AMBomMatl.inventoryID>(matl.InventoryID, matl.OperationID);
            }
            else
            {
                if (Tree1Blank())
                    return IconTypes.DEFAULT;
                matchRec = MatlTree1.Search<AMBomMatl.inventoryID>(matl.InventoryID, matl.OperationID);
            }
            if (matchRec == null)
                return IconTypes.NOMATCH;
            else
            {
                if (ObjCompareHelper.ChangesExist(BomMatlRecords.Cache, matchRec, matl, "BOMID","RevisionID"))
                    return IconTypes.PARTIAL;
                else
                    return IconTypes.MATCH;
            }
        }

        private string GetIcon(int treeNum, AMBomOper oper)
        {
            AMBomOper matchRec;
            if (treeNum == 1)
            {
                if (Tree2Blank())
                    return IconTypes.DFLTFOLDER;
                matchRec = OperTree2.Search<AMBomOper.operationCD>(oper.OperationCD);
            }
            else
            {
                if (Tree1Blank())
                    return IconTypes.DFLTFOLDER;
                matchRec = OperTree1.Search<AMBomOper.operationCD>(oper.OperationCD);
            }
            if (matchRec == null)
                return IconTypes.NOMATCH;
            else
            {
                if (ObjCompareHelper.ChangesExist(BomOperRecords.Cache, matchRec, oper, "BOMID", "RevisionID"))
                    return IconTypes.PARTIAL;
                else
                    return IconTypes.MATCH;
            }
        }

        private bool Tree1Blank()
        {
            return Filter.Current.BOMID1 == null || Filter.Current.RevisionID1 == null;
        }

        private bool Tree2Blank()
        {
            return Filter.Current.BOMID2 == null || Filter.Current.RevisionID2 == null;
        }


        private string ParentDescrDisplay(AMECOItem eco, InventoryItem item)
        {
            return ParentDescrDisplay(eco.BOMID, eco.BOMRevisionID, item);
        }

        private string ParentDescrDisplay(AMECRItem ecr, InventoryItem item)
        {
            return ParentDescrDisplay(ecr.BOMID, ecr.BOMRevisionID, item);
        }

        private string ParentDescrDisplay(AMBomItem bom, InventoryItem item)
        {
            return ParentDescrDisplay(bom.BOMID, bom.RevisionID, item);
        }

        private string ParentDescrDisplay(string bOMID, string revisionID, InventoryItem item)
        {
            var descr = INDescrDisplay(item);
            return $"{bOMID} - {revisionID} ({descr})";
        }

        protected virtual string OperDescrDisplay(AMBomOper oper)
        {
            return OperDescrDisplay(oper?.OperationCD, oper?.WcID, oper?.Descr);
        }

        private string OperDescrDisplay(string operationCD, string wcID, string descr)
        {
            var display = operationCD?.TrimIfNotNullEmpty() + " - " + wcID?.TrimIfNotNullEmpty();
            if (!string.IsNullOrEmpty(descr))
                display += ", " + descr.Trim();
            return display;
        }

        protected virtual string INDescrDisplay(InventoryItem inventoryItem)
        {
            return INDescrDisplay(inventoryItem?.InventoryCD, inventoryItem?.Descr);
        }

        private string INDescrDisplay(string inventoryCD, string descr)
        {
            var display = inventoryCD.TrimIfNotNullEmpty();
            if (!string.IsNullOrEmpty(descr))
                display += " - " + descr.Trim();
            return display;
        }

        protected virtual void BOMCompareFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        { 
            var row = (BOMCompareFilter)e.Row;
            if (row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetVisible<BOMCompareFilter.bOMID1>(Filter.Cache, row, row.IDType1 == IDTypes.BOM);
            PXUIFieldAttribute.SetVisible<BOMCompareFilter.revisionID1>(Filter.Cache, row, row.IDType1 == IDTypes.BOM);
            PXUIFieldAttribute.SetVisible<BOMCompareFilter.eCRID1>(Filter.Cache, row, row.IDType1 == IDTypes.ECR);
            PXUIFieldAttribute.SetVisible<BOMCompareFilter.eCOID1>(Filter.Cache, row, row.IDType1 == IDTypes.ECO);

            PXUIFieldAttribute.SetVisible<BOMCompareFilter.bOMID2>(Filter.Cache, row, row.IDType2 == IDTypes.BOM);
            PXUIFieldAttribute.SetVisible<BOMCompareFilter.revisionID2>(Filter.Cache, row, row.IDType2 == IDTypes.BOM);
            PXUIFieldAttribute.SetVisible<BOMCompareFilter.eCRID2>(Filter.Cache, row, row.IDType2 == IDTypes.ECR);
            PXUIFieldAttribute.SetVisible<BOMCompareFilter.eCOID2>(Filter.Cache, row, row.IDType2 == IDTypes.ECO);
        }

        protected virtual void BOMCompareFilter_ECRID1_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (BOMCompareFilter)e.Row;
            if (row == null)
                return;

            row.BOMID1 = row.ECRID1;
            row.RevisionID1 = AMECRItem.ECRRev;
        }

        protected virtual void BOMCompareFilter_ECOID1_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (BOMCompareFilter)e.Row;
            if (row == null)
                return;

            row.BOMID1 = row.ECOID1;
            row.RevisionID1 = AMECOItem.ECORev;
        }

        protected virtual void BOMCompareFilter_IDType1_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (BOMCompareFilter)e.Row;
            if (row == null)
                return;

            row.BOMID1 = null;
            row.RevisionID1 = null;
            row.ECRID1 = null;
            row.ECOID1 = null;
        }

        protected virtual void BOMCompareFilter_ECRID2_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (BOMCompareFilter)e.Row;
            if (row == null)
                return;

            row.BOMID2 = row.ECRID2;
            row.RevisionID2 = AMECRItem.ECRRev;
        }

        protected virtual void BOMCompareFilter_ECOID2_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (BOMCompareFilter)e.Row;
            if (row == null)
                return;

            row.BOMID2 = row.ECOID2;
            row.RevisionID2 = AMECOItem.ECORev;
        }

        protected virtual void BOMCompareFilter_IDType2_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (BOMCompareFilter)e.Row;
            if (row == null)
                return;

            row.BOMID2 = null;
            row.RevisionID2 = null;
            row.ECRID2 = null;
            row.ECOID2 = null;
        }

        protected virtual void AMBomMatl_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            var row = (AMBomMatl)e.Row;
            if (row == null)
                return;

            if (row.RevisionID == AMECRItem.ECRRev || row.RevisionID == AMECOItem.ECORev)
                row.RevisionID = "";
        }

        protected virtual void AMBomStep_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            var row = (AMBomStep)e.Row;
            if (row == null)
                return;

            if (row.RevisionID == AMECRItem.ECRRev || row.RevisionID == AMECOItem.ECORev)
                row.RevisionID = "";
        }

        protected virtual void AMBomTool_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            var row = (AMBomTool)e.Row;
            if (row == null)
                return;

            if (row.RevisionID == AMECRItem.ECRRev || row.RevisionID == AMECOItem.ECORev)
                row.RevisionID = "";
        }

        protected virtual void AMBomOvhd_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            var row = (AMBomOvhd)e.Row;
            if (row == null)
                return;

            if (row.RevisionID == AMECRItem.ECRRev || row.RevisionID == AMECOItem.ECORev)
                row.RevisionID = "";
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXSelector(typeof(Search<AMBomOper.operationID,
                Where<AMBomOper.bOMID, Equal<Current<AMBomMatl.bOMID>>,
                    And<Where<AMBomOper.revisionID, Equal<Current<AMBomMatl.revisionID>>, 
                        Or<AMBomOper.revisionID, Equal<AMECRItem.eCRRev>,
                            Or<AMBomOper.revisionID, Equal<AMECOItem.eCORev>>>>>>>),
            SubstituteKey = typeof(AMBomOper.operationCD))]
        protected virtual void AMBomMatl_OperationID_CacheAttached(PXCache cache)
        { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXSelector(typeof(Search<AMBomOper.operationID,
                Where<AMBomOper.bOMID, Equal<Current<AMBomStep.bOMID>>,
                    And<Where<AMBomOper.revisionID, Equal<Current<AMBomStep.revisionID>>,
                        Or<AMBomOper.revisionID, Equal<AMECRItem.eCRRev>,
                            Or<AMBomOper.revisionID, Equal<AMECOItem.eCORev>>>>>>>),
            SubstituteKey = typeof(AMBomOper.operationCD))]
        protected virtual void AMBomStep_OperationID_CacheAttached(PXCache cache)
        { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXSelector(typeof(Search<AMBomOper.operationID,
                Where<AMBomOper.bOMID, Equal<Current<AMBomTool.bOMID>>,
                    And<Where<AMBomOper.revisionID, Equal<Current<AMBomTool.revisionID>>,
                        Or<AMBomOper.revisionID, Equal<AMECRItem.eCRRev>,
                            Or<AMBomOper.revisionID, Equal<AMECOItem.eCORev>>>>>>>),
            SubstituteKey = typeof(AMBomOper.operationCD))]
        protected virtual void AMBomTool_OperationID_CacheAttached(PXCache cache)
        { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXSelector(typeof(Search<AMBomOper.operationID,
                Where<AMBomOper.bOMID, Equal<Current<AMBomOvhd.bOMID>>,
                    And<Where<AMBomOper.revisionID, Equal<Current<AMBomOvhd.revisionID>>,
                        Or<AMBomOper.revisionID, Equal<AMECRItem.eCRRev>,
                            Or<AMBomOper.revisionID, Equal<AMECOItem.eCORev>>>>>>>),
            SubstituteKey = typeof(AMBomOper.operationCD))]
        protected virtual void AMBomOvhd_OperationID_CacheAttached(PXCache cache)
        { }

        [Serializable]
        [PXCacheName("BOM Compare Filter")]
        public class BOMCompareFilter : IBqlTable
        {
            #region IDType1
            public abstract class iDType1 : PX.Data.BQL.BqlString.Field<iDType1> { }

            protected int? _IDType1;
            [PXInt]
            [PXUIField(DisplayName = "BOM Type", Visible = false)]
            [PXDefault(IDTypes.BOM, PersistingCheck = PXPersistingCheck.Nothing)]
            [IDTypes.List]
            public virtual int? IDType1
            {
                get
                {
                    return this._IDType1;
                }
                set
                {
                    this._IDType1 = value;
                }
            }

            #endregion
            #region BOMID1
            public abstract class bOMID1 : PX.Data.BQL.BqlString.Field<bOMID1> { }

            protected String _BOMID1;
            [BomID]
            [PXSelector(typeof(Search2<
                AMBomItem.bOMID,
                InnerJoin<AMBomItemBomAggregate,
                    On<AMBomItem.bOMID, Equal<AMBomItemBomAggregate.bOMID>,
                    And<AMBomItem.revisionID, Equal<AMBomItemBomAggregate.revisionID>>>,
                InnerJoin<InventoryItem,
                    On<AMBomItem.inventoryID, Equal<InventoryItem.inventoryID>>>>>),
                typeof(AMBomItem.bOMID), typeof(AMBomItem.revisionID), typeof(AMBomItem.inventoryID),
                typeof(AMBomItem.subItemID), typeof(AMBomItem.siteID), typeof(AMBomItem.descr),
                typeof(InventoryItem.itemClassID), typeof(InventoryItem.descr))]
            public virtual String BOMID1
            {
                get
                {
                    return this._BOMID1;
                }
                set
                {
                    this._BOMID1 = value;
                }
            }
            #endregion
            #region RevisionID
            public abstract class revisionID1 : PX.Data.BQL.BqlString.Field<revisionID1> { }

            protected String _RevisionID1;
            [RevisionIDField]
            [PXSelector(typeof(Search<
                AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<BOMCompareFilter.bOMID1>>>>)
                , typeof(AMBomItem.revisionID)
                , typeof(AMBomItem.status)
                , typeof(AMBomItem.descr)
                , typeof(AMBomItem.effStartDate)
                , typeof(AMBomItem.effEndDate)
                , DescriptionField = typeof(AMBomItem.descr))]
            [PXFormula(typeof(Default<BOMCompareFilter.bOMID1>))] //to clear change of BOM
            public virtual String RevisionID1
            {
                get
                {
                    return this._RevisionID1;
                }
                set
                {
                    this._RevisionID1 = value;
                }
            }
            #endregion
            #region ECRID1
            public abstract class eCRID1 : PX.Data.BQL.BqlString.Field<eCRID1> { }

            protected String _ECRID1;
            [PXString]
            [PXUIField(DisplayName = "ECR ID")]
            [PXSelector(typeof(Search<AMECRItem.eCRID>))]
            public virtual String ECRID1
            {
                get
                {
                    return this._ECRID1;
                }
                set
                {
                    this._ECRID1 = value;
                }
            }
            #endregion
            #region ECOID1
            public abstract class eCOID1 : PX.Data.BQL.BqlString.Field<eCOID1> { }

            protected String _ECOID1;
            [PXString]
            [PXUIField(DisplayName = "ECO ID")]
            [PXSelector(typeof(Search<AMECOItem.eCOID>))]
            public virtual String ECOID1
            {
                get
                {
                    return this._ECOID1;
                }
                set
                {
                    this._ECOID1 = value;
                }
            }
            #endregion
            #region IDType1
            public abstract class iDType2 : PX.Data.BQL.BqlString.Field<iDType2> { }

            protected int? _IDType2;
            [PXInt]
            [PXUIField(DisplayName = "BOM Type", Visible = false)]
            [PXDefault(IDTypes.BOM, PersistingCheck = PXPersistingCheck.Nothing)]
            [IDTypes.List]
            public virtual int? IDType2
            {
                get
                {
                    return this._IDType2;
                }
                set
                {
                    this._IDType2 = value;
                }
            }

            #endregion
            #region BOMID2
            public abstract class bOMID2 : PX.Data.BQL.BqlString.Field<bOMID2> { }

            protected String _BOMID2;
            [BomID]
            [PXSelector(typeof(Search2<
                AMBomItem.bOMID,
                InnerJoin<AMBomItemBomAggregate,
                    On<AMBomItem.bOMID, Equal<AMBomItemBomAggregate.bOMID>,
                    And<AMBomItem.revisionID, Equal<AMBomItemBomAggregate.revisionID>>>,
                InnerJoin<InventoryItem,
                    On<AMBomItem.inventoryID, Equal<InventoryItem.inventoryID>>>>>),
                typeof(AMBomItem.bOMID), typeof(AMBomItem.revisionID), typeof(AMBomItem.inventoryID),
                typeof(AMBomItem.subItemID), typeof(AMBomItem.siteID), typeof(AMBomItem.descr),
                typeof(InventoryItem.itemClassID), typeof(InventoryItem.descr))]
            public virtual String BOMID2
            {
                get
                {
                    return this._BOMID2;
                }
                set
                {
                    this._BOMID2 = value;
                }
            }
            #endregion
            #region RevisionID
            public abstract class revisionID2 : PX.Data.BQL.BqlString.Field<revisionID2> { }

            protected String _RevisionID2;
            [RevisionIDField]
            [PXSelector(typeof(Search<
                AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<BOMCompareFilter.bOMID2>>>>)
                , typeof(AMBomItem.revisionID)
                , typeof(AMBomItem.status)
                , typeof(AMBomItem.descr)
                , typeof(AMBomItem.effStartDate)
                , typeof(AMBomItem.effEndDate)
                , DescriptionField = typeof(AMBomItem.descr))]
            [PXFormula(typeof(Default<BOMCompareFilter.bOMID2>))] //to clear change of BOM
            public virtual String RevisionID2
            {
                get
                {
                    return this._RevisionID2;
                }
                set
                {
                    this._RevisionID2 = value;
                }
            }
            #endregion
            #region ECRID2
            public abstract class eCRID2 : PX.Data.BQL.BqlString.Field<eCRID2> { }

            protected String _ECRID2;
            [PXString]
            [PXUIField(DisplayName = "ECR ID")]
            [PXSelector(typeof(Search<AMECRItem.eCRID>))]
            public virtual String ECRID2
            {
                get
                {
                    return this._ECRID2;
                }
                set
                {
                    this._ECRID2 = value;
                }
            }
            #endregion
            #region ECOID2
            public abstract class eCOID2 : PX.Data.BQL.BqlString.Field<eCOID2> { }

            protected String _ECOID2;
            [PXString]
            [PXUIField(DisplayName = "ECO ID")]
            [PXSelector(typeof(Search<AMECOItem.eCOID>))]
            public virtual String ECOID2
            {
                get
                {
                    return this._ECOID2;
                }
                set
                {
                    this._ECOID2 = value;
                }
            }
            #endregion
        }

        public class IDTypes
        {
            public const int BOM = 0;
            public const int ECR = 1;
            public const int ECO = 2;
            public class Desc
            {
                public static string BOM => Messages.GetLocal(Messages.BOM);
                public static string ECR => Messages.GetLocal(Messages.ECR);
                public static string ECO => Messages.GetLocal(Messages.ECO);
            }

            public class bom : PX.Data.BQL.BqlInt.Constant<bom>
            {
                public bom() : base(BOM) { }
            }
            public class ecr : PX.Data.BQL.BqlInt.Constant<ecr>
            {
                public ecr() : base(ECR) { }
            }
            public class eco : PX.Data.BQL.BqlInt.Constant<eco>
            {
                public eco() : base(ECO) { }
            }
            public class ListAttribute : PXIntListAttribute
            {
                public ListAttribute()
                    : base(
                    new int[] { BOM, ECR, ECO },
                    new string[] { Messages.BOM, Messages.ECR, Messages.ECO })
                { }

            }
        }

        public class OperCategories
        {
            public const int MATL = 0;
            public const int STEP = 1;
            public const int TOOL = 2;
            public const int OVHD = 3;
            public class Desc
            {
                public static string MATL => "Materials";
                public static string STEP => "Steps";
                public static string TOOL => "Tools";
                public static string OVHD => "Overheads";
            }

            public class matl : PX.Data.BQL.BqlInt.Constant<matl>
            {
                public matl() : base(MATL) { }
            }
            public class step : PX.Data.BQL.BqlInt.Constant<step>
            {
                public step() : base(STEP) { }
            }
            public class tool : PX.Data.BQL.BqlInt.Constant<tool>
            {
                public tool() : base(TOOL) { }
            }
            public class ovhd : PX.Data.BQL.BqlInt.Constant<ovhd>
            {
                public ovhd() : base(OVHD) { }
            }

            public static Dictionary<int, string> List()
            {
                var categoryList = new Dictionary<int, string>
                {
                    {MATL, Desc.MATL}, {STEP, Desc.STEP}, {TOOL, Desc.TOOL}, {OVHD, Desc.OVHD}
                };
                return categoryList;
            }
        }

        public class IconTypes
        {
            public static string NOMATCH = Sprite.Control.GetFullUrl(Sprite.Control.Error);
            public static string PARTIAL = Sprite.Control.GetFullUrl(Sprite.Control.Warning);
            public static string MATCH = Sprite.Control.GetFullUrl(Sprite.Control.Info);
            public static string DEFAULT = Sprite.Tree.GetFullUrl(Sprite.Tree.Leaf);
            public static string DFLTFOLDER = Sprite.Main.GetFullUrl(Sprite.Tree.Folder);
        }
    }
}