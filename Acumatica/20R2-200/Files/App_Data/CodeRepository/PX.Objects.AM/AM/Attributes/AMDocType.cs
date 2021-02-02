using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Document/Transaction Types
    /// </summary>
    public class AMDocType
    {
        /// <summary>
        /// Manufacturing Move Transaction
        /// </summary>
        public const string Move = "O";
        /// <summary>
        /// Manufacturing Labor Transaction
        /// </summary>
        public const string Labor = "L";
        /// <summary>
        /// Manufacturing Material Transaction
        /// </summary>
        public const string Material = "M";
        /// <summary>
        /// Manufacturing Wip Adjustment
        /// </summary>
        public const string WipAdjust = "W";
        /// <summary>
        /// Manufacturing Cost Transaction
        /// </summary>
        public const string ProdCost = "C";
        /// <summary>
        /// Manufacturing Disassembly Transaction
        /// </summary>
        public const string Disassembly = "D";
        /// <summary>
        /// Vendor Shipment (for references only - data is not stored in the same table as the other types)
        /// </summary>
        public const string VendorShipment = "V";
        /// <summary>
        /// Clock Entry
        /// </summary>
        public const string Clock = "T";

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            /// <summary>
            /// Manufacturing Move Transaction
            /// </summary>
            public static string Move => Messages.GetLocal(Messages.DocTypeMove);

            /// <summary>
            /// Manufacturing Labor Transaction
            /// </summary>
            public static string Labor => Messages.GetLocal(Messages.DocTypeLabor);

            /// <summary>
            /// Manufacturing Material Transaction
            /// </summary>
            public static string Material => Messages.GetLocal(Messages.DocTypeMaterial);

            /// <summary>
            /// Manufacturing Wip Adjustment
            /// </summary>
            public static string WipAdjust => Messages.GetLocal(Messages.DocTypeWipAdjust);

            /// <summary>
            /// Manufacturing Cost Transaction
            /// </summary>
            public static string ProdCost => Messages.GetLocal(Messages.DocTypeProdCost);

            /// <summary>
            /// Manufacturing Disassembly Transaction
            /// </summary>
            public static string Disassembly => Messages.GetLocal(PX.Objects.IN.Messages.Disassembly);

            /// <summary>
            /// Vendor Shipment
            /// </summary>
            public static string VendorShipment => Messages.GetLocal(Messages.VendorShipment);

            /// <summary>
            /// Clock
            /// </summary>
            public static string Clock => Messages.GetLocal(Messages.DocTypeClock);
        }

        public class NumberingAttribute : AutoNumberAttribute
        {
            public NumberingAttribute() : this(typeof(AMBatch.docType), typeof(AMBatch.tranDate))
            {
            }

            public NumberingAttribute(System.Type doctypeField, System.Type dateField)
                : base(doctypeField, dateField, new string[]
            {
              Move,
              Labor,
              Material,
              WipAdjust,
              ProdCost,
              Disassembly
            }, new System.Type[]
            {
              typeof (AMPSetup.moveNumberingID),
              typeof (AMPSetup.laborNumberingID),
              typeof (AMPSetup.materialNumberingID),
              typeof (AMPSetup.wipAdjustNumberingID),
              typeof (AMPSetup.prodCostNumberingID),
              typeof (AMPSetup.disassemblyNumberingID)
            })
                {
                }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[]
            {
                Move,
                Labor,
                Material,
                WipAdjust,
                ProdCost,
                Disassembly,
                VendorShipment,
                Clock
            }, new string[]
            {
                Messages.DocTypeMove,
                Messages.DocTypeLabor,
                Messages.DocTypeMaterial,
                Messages.DocTypeWipAdjust,
                Messages.DocTypeProdCost,
                IN.Messages.Disassembly,
                Messages.VendorShipment,
                Messages.DocTypeClock
            })
                {
                }
        }

        public class move : PX.Data.BQL.BqlString.Constant<move>
        {
            public move() : base(Move) { }
        }

        public class labor : PX.Data.BQL.BqlString.Constant<labor>
        {
            public labor() : base(Labor) { }
        }

        public class material : PX.Data.BQL.BqlString.Constant<material>
        {
            public material() : base(Material) { }
        }

        public class wipAdjust : PX.Data.BQL.BqlString.Constant<wipAdjust>
        {
            public wipAdjust() : base(WipAdjust) { }
        }

        public class prodCost : PX.Data.BQL.BqlString.Constant<prodCost>
        {
            public prodCost() : base(ProdCost) { }
        }

        public class disassembly : PX.Data.BQL.BqlString.Constant<disassembly>
        {
            public disassembly() : base(Disassembly) { }
        }

        public class vendorShipment : PX.Data.BQL.BqlString.Constant<vendorShipment>
        {
            public vendorShipment() : base(VendorShipment) { }
        }

        public class clock : PX.Data.BQL.BqlString.Constant<clock>
        {
            public clock() : base(Clock) { }
        }

        public static string GetDocTypeDesc(string docType)
        {
            if (string.IsNullOrWhiteSpace(docType))
            {
                return string.Empty;
            }

            switch (docType)
            {
                case Move:
                    return Desc.Move;
                case Labor:
                    return Desc.Labor;
                case Material:
                    return Desc.Material;
                case WipAdjust:
                    return Desc.WipAdjust;
                case ProdCost:
                    return Desc.ProdCost;
                case Disassembly:
                    return Desc.Disassembly;
                case VendorShipment:
                    return Desc.VendorShipment;
                case Clock:
                    return Desc.Clock;
                default:
                    return string.Empty;
            }
        }

        public static void DocTypeRedirectRequiredException(string docType, string batNbr, PXGraph graph)
        {
            if (string.IsNullOrWhiteSpace(batNbr) || string.IsNullOrWhiteSpace(docType))
            {
                return;
            }

            if (graph == null)
            {
                graph = PXGraph.CreateInstance<PXGraph>();
            }
            
            AMBatch doc = PXSelect<AMBatch, 
                Where<AMBatch.batNbr, Equal<Required<AMBatch.batNbr>>, 
                And<AMBatch.docType, Equal<Required<AMBatch.docType>>>>
                >.Select(graph, batNbr, docType.Trim());

            if (doc != null)
            {
                DocTypeRedirectRequiredException(doc);
            }
        }

        /// <summary>
        /// Is the given AM Document Type a type that allows material
        /// </summary>
        /// <param name="docType"></param>
        public static bool IsDocTypeMaterial(string docType)
        {
            return !string.IsNullOrWhiteSpace(docType) && (docType == Material || docType == Disassembly);
        }

        /// <summary>
        /// If the doc type is a move transaction related doc type
        /// </summary>
        public static bool IsDocTypeMove(string docType)
        {
            return !string.IsNullOrWhiteSpace(docType) && (docType == Move || docType == Labor);
        }

        public static void DocTypeRedirectRequiredException(AMBatch doc)
        {
            if (doc == null || string.IsNullOrWhiteSpace(doc.DocType))
            {
                return;
            }

            switch (doc.DocType)
            {
                case Labor:
                    var laborGraph = PXGraph.CreateInstance<LaborEntry>();
                    laborGraph.batch.Current = doc;
                    throw RedirectRequiredException(laborGraph, typeof(LaborEntry));

                case Move:
                    var moveGraph = PXGraph.CreateInstance<MoveEntry>();
                    moveGraph.batch.Current = doc;
                    throw RedirectRequiredException(moveGraph, typeof(MoveEntry));

                case Material:
                    var matlGraph = PXGraph.CreateInstance<MaterialEntry>();
                    matlGraph.batch.Current = doc;
                    throw RedirectRequiredException(matlGraph, typeof(MaterialEntry));

                case WipAdjust:
                    var wipGraph = PXGraph.CreateInstance<WIPAdjustmentEntry>();
                    wipGraph.batch.Current = doc;
                    throw RedirectRequiredException(wipGraph, typeof(WIPAdjustmentEntry));

                case ProdCost:
                    var costGraph = PXGraph.CreateInstance<ProductionCostEntry>();
                    costGraph.batch.Current = doc;
                    throw RedirectRequiredException(costGraph, typeof(ProductionCostEntry));

                case Disassembly:
                    var disassemblyGraph = PXGraph.CreateInstance<DisassemblyEntry>();
                    disassemblyGraph.Document.Current = disassemblyGraph.Document.Search<AMDisassembleBatch.batchNbr>(doc.BatNbr, doc.DocType);
                    throw RedirectRequiredException(disassemblyGraph, typeof(DisassemblyEntry));

                default:
                    throw new PXException(Messages.GetLocal(Messages.UnknownDocType));
            }
        }

        protected static PXRedirectRequiredException RedirectRequiredException(PXGraph graph, System.Type graphType)
        {
            return new PXRedirectRequiredException(graph, true, string.Empty) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
    }
}