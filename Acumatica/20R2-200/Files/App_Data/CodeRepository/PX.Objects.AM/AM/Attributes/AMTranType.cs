using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    public class AMTranType 
    {
        /// <summary>
        /// WIP Entry from production close process [XXX]
        /// </summary>
        public const string WIPadjustment = "XXX";
        /// <summary>
        /// WIP Entry from production close process [XVV]
        /// </summary>
        public const string WIPvariance = "XVV";
        /// <summary>
        /// Manufacturing Variable Overhead Transaction [XVO]
        /// </summary>
        public const string VarOvhd = "XVO";
        /// <summary>
        /// Manufacturing Fixed Overhead Transaction [XFO]
        /// </summary>
        public const string FixOvhd = "XFO";
        /// <summary>
        /// Manufacturing Tool transaction [XTL]
        /// </summary>
        public const string Tool = "XTL";
        /// <summary>
        /// Manufacturing Backflush Labor transaction [XBL]
        /// </summary>
        public const string BFLabor = "XBL";
        /// <summary>
        /// Manufacturing Labor transaction [XLA]
        /// </summary>
        public const string Labor = "XLA";
        /// <summary>
        /// Manufacturing Indirect Labor transaction
        /// </summary>
        public const string IndirectLabor = "XIL";
        /// <summary>
        /// Manufacturing Machine transaction [XMC]
        /// </summary>
        public const string Machine = "XMC";
        /// <summary>
        /// Material Issue transaction ('III')
        /// </summary>
        public const string Issue = "III";
        /// <summary>
        /// Finished Good (Bi-Product) Receipt transaction ('RCP')
        /// </summary>
        public const string Receipt = "RCP";
        /// <summary>
        /// Return Issue transaction ('RET')
        /// </summary>
        public const string Return = "RET";
        /// <summary>
        /// Production adjustment ('ADJ')
        /// Typical use is a negative move transaction
        /// </summary>
        public const string Adjustment = "ADJ";
        /// <summary>
        /// Scrap Write Off ('XSW')
        /// Scrap that is removed from the order to a journal entry
        /// </summary>
        public const string ScrapWriteOff = "XSW";
        /// <summary>
        /// Scrap Quarantine ('XSQ')
        /// Scrap that is removed from the order to inventory quarantine location
        /// </summary>
        public const string ScrapQuarantine = "XSQ";
        /// <summary>
        /// Operation WIP Complete Amount (OWC) [aka 'MFG to Inv.' by operation]
        /// Value of WIP Complete (MFG to Inv.) at an operation
        /// </summary>
        public const string OperWIPComplete = "OWC";
        /// <summary>
        /// MFG Disassembly
        /// </summary>
        public const string Disassembly = INTranType.Disassembly;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class ListDesc
        {
            public static string WIPadjustment => Messages.GetLocal(Messages.TranTypeWIPadjustment);
            public static string WIPvariance => Messages.GetLocal(Messages.TranTypeWIPvariance);
            public static string VarOvhd => Messages.GetLocal(Messages.TranTypeVarOvhd);
            public static string FixOvhd => Messages.GetLocal(Messages.TranTypeFixOvhd);
            public static string Tool => Messages.GetLocal(Messages.TranTypeTool);
            public static string BFLabor => Messages.GetLocal(Messages.TranTypeBFLabor);
            public static string Labor => Messages.GetLocal(Messages.TranTypeLabor);
            public static string IndirectLabor => Messages.GetLocal(Messages.TranTypeIndirectLabor);
            public static string Machine => Messages.GetLocal(Messages.TranTypeMachine);
            public static string Issue => Messages.GetLocal(Messages.TranTypeIssue);
            public static string Receipt => Messages.GetLocal(Messages.TranTypeReceipt);
            public static string Return => Messages.GetLocal(Messages.TranTypeReturn);
            public static string Adjustment => Messages.GetLocal(PX.Objects.IN.Messages.Adjustment);
            public static string ScrapWriteOff => Messages.GetLocal(Messages.ScrapWriteOff);
            public static string ScrapQuarantine => Messages.GetLocal(Messages.ScrapQuarantine);
            public static string OperWIPComplete => Messages.GetLocal(Messages.OperationWipComplete);
            public static string Disassembly => Messages.GetLocal(PX.Objects.IN.Messages.Disassembly);
            public static string Correction => Messages.GetLocal(Messages.Correction);
        }

        /// <summary>
        /// Constants for transaction descriptions
        /// </summary>
        public class TranDesc
        {
            public static string WIPadjustment => Messages.GetLocal(Messages.ProdGLEntry_WIPAdjustment);
            public static string WIPvariance => Messages.GetLocal(Messages.ProdGLEntry_WIPVariance);
            public static string VarOvhd => Messages.GetLocal(Messages.ProdGLEntry_VarOverheadCosts);
            public static string FixOvhd => Messages.GetLocal(Messages.ProdGLEntry_FixOverheadCosts);
            public static string Tool => Messages.GetLocal(Messages.ProdGLEntry_ToolCosts);
            public static string BFLabor => Messages.GetLocal(Messages.ProdGLEntry_LaborBackflush);
            public static string Labor => Messages.GetLocal(Messages.ProdGLEntry_LaborEntry);
            public static string IndirectLabor => Messages.GetLocal(Messages.ProdGLEntry_IndirectLaborEntry);
            public static string Machine => Messages.GetLocal(Messages.ProdGLEntry_MachineCosts);
            public static string Issue => Messages.GetLocal(Messages.ProdEntry_Issue);
            public static string Receipt => Messages.GetLocal(Messages.ProdEntry_Receipt);
            public static string Return => Messages.GetLocal(Messages.ProdEntry_Return);
            public static string Adjustment => Messages.GetLocal(Messages.ProdEntry_Adjustment);
            public static string ScrapWriteOff => Messages.GetLocal(Messages.ProdEntry_ScrapWriteOff);
            public static string ScrapQuarantine => Messages.GetLocal(Messages.ProdEntry_ScrapQuarantine);
            public static string OperWIPComplete => Messages.GetLocal(Messages.ProdEntry_OperationWipComplete);
            public static string Disassembly => Messages.GetLocal(Messages.ProdEntry_Disassembly);
        }

        /// <summary>
        /// WIP Entry from WIP Adjustment entry page
        /// </summary>
        public class wIPadjustment : PX.Data.BQL.BqlString.Constant<wIPadjustment>
        {
            public wIPadjustment() : base(WIPadjustment) { }
        }
        /// <summary>
        /// WIP Entry from production close process
        /// </summary>
        public class wIPvariance : PX.Data.BQL.BqlString.Constant<wIPvariance>
        {
            public wIPvariance() : base(WIPvariance) { }
        }
        /// <summary>
        /// Manufacturing Variable Overhead transaction
        /// </summary>
        public class varOvhd : PX.Data.BQL.BqlString.Constant<varOvhd>
        {
            public varOvhd() : base(VarOvhd) { }
        }
        /// <summary>
        /// Manufacturing Fixed Overhead transaction
        /// </summary>
        public class fixOvhd : PX.Data.BQL.BqlString.Constant<fixOvhd>
        {
            public fixOvhd() : base(FixOvhd) { }
        }
        /// <summary>
        /// Manufacturing Tool transaction
        /// </summary>
        public class tool : PX.Data.BQL.BqlString.Constant<tool>
        {
            public tool() : base(Tool) { }
        }
        /// <summary>
        /// Manufacturing Backflush Labor transaction
        /// </summary>
        public class bFLabor : PX.Data.BQL.BqlString.Constant<bFLabor>
        {
            public bFLabor() : base(BFLabor) { }
        }
        /// <summary>
        /// Manufacturing  Labor transaction
        /// </summary>
        public class labor : PX.Data.BQL.BqlString.Constant<labor>
        {
            public labor() : base(Labor) { }
        }
        /// <summary>
        /// Manufacturing Indirect Labor transaction
        /// </summary>
        public class indirectLabor : PX.Data.BQL.BqlString.Constant<indirectLabor>
        {
            public indirectLabor() : base(IndirectLabor) { }
        }
        /// <summary>
        /// Manufacturing Machine transaction
        /// </summary>
        public class machine : PX.Data.BQL.BqlString.Constant<machine>
        {
            public machine() : base(Machine) { }
        }
        /// <summary>
        /// Material Issue transaction
        /// </summary>
        public class issue : PX.Data.BQL.BqlString.Constant<issue> { public issue() : base(Issue) { } }
        /// <summary>
        /// Finished Good Receipt transaction
        /// </summary>
        public class receipt : PX.Data.BQL.BqlString.Constant<receipt> { public receipt() : base(Receipt) { } }
        /// <summary>
        /// Material Return Issue transaction
        /// </summary>
        public class _return : PX.Data.BQL.BqlString.Constant<_return> { public _return() : base(Return) { } }
        /// <summary>
        /// Production adjustment
        /// Typical use is a negative move transaction
        /// </summary>
        public class adjustment : PX.Data.BQL.BqlString.Constant<adjustment>
        {
            public adjustment() : base(Adjustment) { }
        }
        /// <summary>
        /// Production Scrap Write-Off
        /// Typical use is a move or labor transaction with QtyScrapped != 0
        /// </summary>
        public class scrapWriteOff : PX.Data.BQL.BqlString.Constant<scrapWriteOff>
        {
            public scrapWriteOff() : base(ScrapWriteOff) { }
        }
        /// <summary>
        /// Production Scrap Quarantine
        /// Typical use is a move or labor transaction with QtyScrapped != 0
        /// </summary>
        public class scrapQuarantine : PX.Data.BQL.BqlString.Constant<scrapQuarantine>
        {
            public scrapQuarantine() : base(ScrapQuarantine) { }
        }
        /// <summary>
        /// Operation WIP Complete Amount (OWC) [aka 'MFG to Inv.' by operation]
        /// Value of WIP Complete (MFG to Inv.) at an operation
        /// </summary>
        public class operWIPComplete : PX.Data.BQL.BqlString.Constant<operWIPComplete>
        {
            public operWIPComplete() : base(OperWIPComplete) { }
        }

        public class disassembly : PX.Data.BQL.BqlString.Constant<disassembly>
        {
            public disassembly() : base(Disassembly) { }
        }

        /// <summary>
        /// Get the Manufacturing Transaction description based on the tran type
        /// </summary>
        /// <param name="tranType">AMTranType string</param>
        /// <returns>Prod Entry message</returns>
        public static string GetTranDescription(string tranType)
        {
            if (string.IsNullOrWhiteSpace(tranType))
            {
                return string.Empty;
            }

            try
            {
                var x = new ListAttribute();
                return x.ValueLabelDic[tranType];
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool IsScrapTranType(string tranType)
        {
            return tranType != null && (tranType == ScrapQuarantine || tranType == ScrapWriteOff);
        }

        public static short? InvtMult(string tranType)
        {
            return InvtMult(tranType, (bool?) null);
        }

        public static short? InvtMult(string tranType, bool? isScrap)
        {           
            switch (tranType)
            {
                case Issue:
                case Disassembly:
                    return new short?((short)-1);
                case Adjustment:
                    return isScrap.GetValueOrDefault() ? new short?((short)1) : new short?((short)-1);
                case Receipt:
                case Return:
                    return new short?((short)1);
                default:
                    return new short?((short)0);
            }
        }

        public static short? InvtMult(string tranType, decimal? qty)
        {
            var isNegativeQty = qty != null && qty < 0;

            switch (tranType)
            {
                case Issue:
                case Disassembly:
                    return isNegativeQty ? new short?((short)1) : new short?((short)-1);
                case Adjustment:
                case Receipt:
                case Return:
                    return isNegativeQty ? new short?((short)-1) : new short?((short)1);
                default:
                    return new short?((short)0);
            }
        }

        public static string ConvertToINTranType(string tranType)
        {
            switch (tranType)
            {
                case Issue:
                    return INTranType.Issue;
                case Receipt:
                case Disassembly:
                    return INTranType.Receipt;
                case Return:
                    return INTranType.Return;
                case Adjustment:
                    return INTranType.Adjustment;
                default:
                    return INTranType.NoUpdate;
            }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
            : base(
                new[] { 
                    AMTranType.WIPadjustment, 
                    AMTranType.WIPvariance, 
                    AMTranType.VarOvhd, 
                    AMTranType.FixOvhd, 
                    AMTranType.Tool, 
                    AMTranType.BFLabor, 
                    AMTranType.Labor,
                    AMTranType.IndirectLabor,
                    AMTranType.Machine, 
                    AMTranType.Issue, 
                    AMTranType.Receipt, 
                    AMTranType.Return,
                    AMTranType.Adjustment,
                    AMTranType.ScrapWriteOff,
                    AMTranType.ScrapQuarantine,
                    AMTranType.OperWIPComplete,
                    AMTranType.Disassembly
                },
                new[] {
                    Messages.TranTypeWIPadjustment, // WIPadjustment
                    Messages.TranTypeWIPvariance,   // WIPvariance
                    Messages.TranTypeVarOvhd,       // VarOvhd
                    Messages.TranTypeFixOvhd,       // FixOvhd
                    Messages.TranTypeTool,          // Tool
                    Messages.TranTypeBFLabor,       // BFLabor
                    Messages.TranTypeLabor,         // Labor
                    Messages.TranTypeIndirectLabor, // IndirectLabor
                    Messages.TranTypeMachine,       // Machine
                    Messages.TranTypeIssue,         // Issue
                    Messages.TranTypeReceipt,       // Receipt
                    Messages.TranTypeReturn,        // Return
                    IN.Messages.Adjustment,         // Adjustment
                    Messages.ScrapWriteOff,         // ScrapWriteOff
                    Messages.ScrapQuarantine,       // ScrapQuarantine
                    Messages.OperationWipComplete,  // OperWIPComplete
                    IN.Messages.Disassembly,        // Disassembly
                })
            {}
        }

        public class DisassembleBatchListAttribute : PXStringListAttribute
        {
            public DisassembleBatchListAttribute()
                : base(
                    new[] {
                        AMTranType.Disassembly,
                        AMTranType.Adjustment
                    },
                    new[] {
                        IN.Messages.Disassembly,
                        Messages.Correction //Adjustment type displayed as Correction
                    })
            {}
        }

        public class DisassembleTranListAttribute : PXStringListAttribute
        {
            public DisassembleTranListAttribute()
                : base(
                    new[] {
                        AMTranType.Receipt,
                        AMTranType.Adjustment
                    },
                    new[] {
                        Messages.TranTypeReceipt,
                        IN.Messages.Adjustment
                    })
            { }
        }
    }
}