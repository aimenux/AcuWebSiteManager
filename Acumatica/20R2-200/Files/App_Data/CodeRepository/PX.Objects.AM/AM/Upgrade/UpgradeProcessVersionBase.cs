using System;
using System.Text;
using Customization;
using PX.Data;

namespace PX.Objects.AM.Upgrade
{
    internal abstract class UpgradeProcessVersionBase
    {
        public abstract int Version { get; }
        protected UpgradeProcess _upgradeGraph;
        protected CustomizationPlugin _plugin;

        protected UpgradeProcessVersionBase(UpgradeProcess upgradeGraph, CustomizationPlugin plugin) : this(upgradeGraph)
        {
            _plugin = plugin;
        }

        protected UpgradeProcessVersionBase(UpgradeProcess upgradeGraph)
        {
            _upgradeGraph = upgradeGraph ?? throw new PXArgumentException(nameof(upgradeGraph));
        }

        public abstract void ProcessTables();

        protected virtual string FormatedVersion()
        {
            try
            {
                var sb = new StringBuilder();
                var pre2017R2 = Version < UpgradeVersions.Version2017R2Ver00;
                var dot1 = pre2017R2 ? 1 : 2;
                var dot2 = pre2017R2 ? 4 : 6;

                foreach (var c in Version.ToString().ToCharArray())
                {
                    sb.Append(c);

                    if (sb.Length == dot1)
                    {
                        sb.Append(".");
                        continue;
                    }
                    if (sb.Length == dot2)
                    {
                        sb.Append(".X.");
                    }
                }
                return sb.ToString();
            }
            catch
            {
                return Version.ToString();
            }
        }

        // Here in case it needs to be overridden for some scenarios
        protected virtual bool ProcessCompletedInPreviousVersion(int upgradeFrom)
        {
            //Example usage:
            //  return upgradeFrom.BetweenInclusive(UpgradeVersions.Version2017R2Ver38, UpgradeVersions.MaxVersionNumbers.Version2017R2);

            // Always false unless overriding
            return upgradeFrom < 0;
        }

        public static bool Process<TUpgrade>(TUpgrade upgrade, int upgradeFrom)
            where TUpgrade : UpgradeProcessVersionBase
        {
            if (upgrade == null)
            {
                throw new ArgumentNullException(nameof(upgrade));
            }

            if (upgrade.ProcessCompletedInPreviousVersion(upgradeFrom))
            {
                upgrade.ProcessSkip();
                return false;
            }

            if (upgradeFrom >= upgrade.Version)
            {
                return false;
            }

            upgrade.Process();
            return true;
        }

        /// <summary>
        /// Need to mark the version number but we are not running any upgrade scripts
        /// </summary>
        public virtual void ProcessSkip()
        {
            AMPSetup ampSetup = PXSelect<AMPSetup>.Select(_upgradeGraph);
            if (ampSetup == null)
            {
                throw new PXException(Messages.GetLocal(Messages.SetupNotEntered, Messages.GetLocal(Messages.ProductionSetup)));
            }
            ampSetup.UpgradeStatus = Version;
            _upgradeGraph.ProductionSetup.Update(ampSetup);
            _upgradeGraph.Actions.PressSave();
        }

        /// <summary>
        /// Process/Execute upgrade for listed version
        /// </summary>
        public virtual void Process()
        {
            var formatedVersion = FormatedVersion();
            WriteCstInfoOnly(0, Messages.GetLocal(Messages.UpgradeToVersion), formatedVersion, Messages.GetLocal(Messages.Starting), string.Empty);

            try
            {
                AMPSetup ampSetup = PXSelect<AMPSetup>.Select(_upgradeGraph);
                if (ampSetup == null)
                {
                    throw new PXException(Messages.GetLocal(Messages.SetupNotEntered, Messages.GetLocal(Messages.ProductionSetup)));
                }

                ProcessTables();

                ampSetup.UpgradeStatus = Version;
                _upgradeGraph.ProductionSetup.Update(ampSetup);
                _upgradeGraph.Actions.PressSave();
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                WriteInfo(0, Messages.GetLocal(Messages.UpgradeError, e.Message));
                throw new PXException(Messages.GetLocal(Messages.UpgradeToVersion, formatedVersion, Messages.GetLocal(Messages.Failed), Messages.GetLocal(Messages.SeeTraceWindow)));
            }

            WriteInfo(0, Messages.GetLocal(Messages.UpgradeToVersion, formatedVersion, Messages.GetLocal(Messages.Completed), string.Empty));
        }

        public void WriteInfo(string infoMsg, params object[] args)
        {
            WriteInfo(1, infoMsg, args);
        }

        public void WriteInfo(int indent, string infoMsg, params object[] args)
        {
            UpgradeHelper.WriteInfo(_plugin,
                _plugin == null ? infoMsg : infoMsg.Indent(indent), 
                args);
        }

        public void WriteCstInfoOnly(string infoMsg, params object[] args)
        {
           WriteCstInfoOnly(1, infoMsg, args);
        }

        public void WriteCstInfoOnly(int indent, string infoMsg, params object[] args)
        {
            UpgradeHelper.WriteCstInfoOnly(_plugin,
                _plugin == null ? infoMsg : infoMsg.Indent(indent), 
                args);
        }

        protected virtual void WriteUnableToUpdateCacheName<Dac>() where Dac : IBqlTable
        {
            WriteInfo($"Unable to update {Common.Cache.GetCacheName(typeof(Dac))}.");
        }

        protected void TraceTableUpgradeException(Exception e, string tableName)
        {
            if (_plugin != null)
            {
                WriteInfo("Unable to upgrade table '{0}' to version '{1}'", tableName, Version);
                return;
            }

            PXTraceHelper.PxTraceException(e);
            PXTrace.WriteWarning("Unable to upgrade table '{0}' to version '{1}'", tableName, Version);
        }

        /// <summary>
        /// Forms the string value to use in a DirectSqlExpression PXDatabase.Update in the form of "LEFT(field, length)".
        /// Both MSSQL and MySQL support the same LEFT function syntax.
        /// </summary>
        protected string LeftCharOfFieldDirectSqlExpression(Type field, int length)
        {
            return LeftCharOfFieldDirectSqlExpression(field.Name.ToCapitalized(), length);
        }

        protected string LeftCharOfFieldDirectSqlExpression(string fieldName, int length)
        {
            return $"LEFT({QuoteDbIdentifier(fieldName)}, {length})";
        }

        protected void CopyFieldValueWhereDecimalZero<TDac, TSetField, TSourceField>()
            where TDac : IBqlTable
            where TSetField : IBqlField
            where TSourceField : IBqlField
        {
            CopyFieldValue<TDac, TSetField, TSourceField>(PXDbType.Decimal, 8, 0m, PXComp.EQ);
        }

        protected void CopyFieldValue<TDac, TSetField, TSourceField>(PXDbType valueType, int? valueLength, object value, PXComp comp)
            where TDac : IBqlTable
            where TSetField : IBqlField
            where TSourceField : IBqlField
        {
            TryUpdate<TDac>(
                DirectExpressionFieldAssign<TSetField, TSourceField>(),
                new PXDataFieldRestrict<TSetField>(valueType, valueLength, value, comp)
            );
        }

        protected PXDataFieldAssign DirectExpressionFieldAssign<TSetField, TSourceField>()
            where TSetField : IBqlField
            where TSourceField : IBqlField
        {
            return new PXDataFieldAssign<TSetField>(PXDbType.DirectExpression, QuoteDbIdentifier<TSourceField>());
        }

        protected string QuoteDbIdentifier<TField>() where TField : IBqlField
        {
            return QuoteDbIdentifier(typeof(TField));
        }

        protected string QuoteDbIdentifier(Type field)
        {
            return QuoteDbIdentifier(field.Name.ToCapitalized());
        }

        protected string QuoteDbIdentifier(string fieldName)
        {
            return _upgradeGraph.SqlDialect.quoteDbIdentifier(fieldName);
        }

        /// <summary>
        /// Update the given table with a try/catch and reporting of data field information
        /// </summary>
        /// <returns>True when the table updated records</returns>
        protected virtual bool TryUpdate<Table>(params PXDataFieldParam[] pars) where Table : IBqlTable
        {
            if (pars == null)
            {
                throw new ArgumentNullException(nameof(pars));
            }

            if (pars.Length == 0)
            {
                throw new ArgumentException(nameof(pars));
            }

            try
            {
#if DEBUG
                PXTraceHelper.WriteInformation(PXDataFieldParamAsString<Table>($"Updating {typeof(Table).Name}", pars));
#endif
                return PXDatabase.Update<Table>(pars);
            }
            catch
            {
#if !DEBUG
                PXTraceHelper.WriteInformation(PXDataFieldParamAsString<Table>($"Updating {typeof(Table).Name}", pars));
#endif
                WriteUnableToUpdateCacheName<Table>();
                throw;
            }
        }

        protected virtual bool TryDelete<Table>(params PXDataFieldRestrict[] pars) 
            where Table : IBqlTable
        {
            if (pars == null)
            {
                throw new ArgumentNullException(nameof(pars));
            }

            if (pars.Length == 0)
            {
                throw new ArgumentException(nameof(pars));
            }

            return _TryDelete<Table>(pars);
        }

        protected virtual bool TryDeleteAll<Table>()
            where Table : IBqlTable
        {
            return _TryDelete<Table>();
        }

        private bool _TryDelete<Table>(params PXDataFieldRestrict[] pars)
            where Table : IBqlTable
        {
            try
            {
#if DEBUG
                PXTraceHelper.WriteInformation(PXDataFieldParamAsString<Table>($"Deleting {typeof(Table).Name}", pars));
#endif
                return PXDatabase.Delete<Table>(pars);
            }
            catch
            {
                WriteInfo($"Unable to delete {Common.Cache.GetCacheName(typeof(Table))}. {System.Environment.NewLine} {PXDataFieldParamAsString<Table>(pars)}");
                throw;
            }
        }

        protected virtual string PXDataFieldParamAsString<Table>(params PXDataFieldParam[] pars) 
            where Table : IBqlTable
        {
            return PXDataFieldParamAsString<Table>(null, pars);
        }

        protected virtual string PXDataFieldParamAsString<Table>(string msg, params PXDataFieldParam[] pars)
            where Table : IBqlTable
        {
            return PXDataFieldParamAsString<Table>(msg, 1, pars);
        }

        /// <summary>
        /// Format the data field params as user displayable string values
        /// </summary>
        protected virtual string PXDataFieldParamAsString<Table>(string msg, int indentPars, params PXDataFieldParam[] pars) where Table : IBqlTable
        {
            if (pars == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(msg))
            {
                sb.AppendLine(msg);
            }
            foreach (var dataFieldParam in pars)
            {
                var dfp = PXDataFieldParamAsString(dataFieldParam);
                if (string.IsNullOrWhiteSpace(dfp))
                {
                    continue;
                }

                sb.AppendLine(dfp.Indent(indentPars));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Format the data field param as user displayable string value
        /// </summary>
        protected string PXDataFieldParamAsString(PXDataFieldParam dataFieldParam)
        {
            if (dataFieldParam == null)
            {
                return null;
            }

            if (dataFieldParam is PXDataFieldAssign)
            {
                return $"SET {dataFieldParam.Column.Name} = '{Convert.ToString(dataFieldParam.Value ?? "NULL")}'";
            }

            if (dataFieldParam is PXDataFieldRestrict)
            {
                var fieldRestrict = (PXDataFieldRestrict) dataFieldParam;
                var fieldRestrictOperator = fieldRestrict.OrOperator ? " (OR)" : null;
                var fieldRestrictValue = fieldRestrict.Value == null ? null : $" '{fieldRestrict.Value}'";
                return $"WHERE{fieldRestrictOperator} {fieldRestrict.Column.Name} {PXCompNameValue(fieldRestrict.Comp)}{fieldRestrictValue}";
            }

            return $"{dataFieldParam.Column.Name}; {dataFieldParam.Value}";
        }

        /// <summary>
        /// return the PXComp enum label value
        /// </summary>
        protected string PXCompNameValue(PXComp pxComp)
        {
            return Enum.Parse(typeof(PXComp), pxComp.ToString()).ToString();
        }

        /// <summary>
        /// Perform an RTrim update to the given DAC field
        /// </summary>
        /// <typeparam name="TDac">Table receiving the update</typeparam>
        /// <typeparam name="TField">Field of the given table receiving the RTrim update</typeparam>
        protected void ProcessRTrimUpdate<TDac, TField>()
            where TDac : IBqlTable
            where TField : IBqlField
        {
            // RTrim is null safe in MSSQL and MySQL. If field is null it just returns null.
            TryUpdate<TDac>(new PXDataFieldAssign<TField>(PXDbType.DirectExpression, MakeRTrimDirectSqlExpression(typeof(TField))));
        }

        protected string MakeRTrimDirectSqlExpression(Type fieldType)
        {
            return $"RTrim({ QuoteDbIdentifier(fieldType)})";
        }

        protected static bool TryDeleteDocument<TDoc>(PXGraph graph, TDoc doc, out Exception ex) where TDoc : class, IBqlTable, new()
        {
            ex = null;

            if (graph == null || doc == null)
            {
                return false;
            }

            graph.Clear();

            try
            {
                graph.Caches<TDoc>().AllowDelete = true;
                graph.Caches<TDoc>().AllowUpdate = true;
                graph.Caches<TDoc>().AllowInsert = true;
                graph.Caches<TDoc>().Current = doc;
                graph.Caches<TDoc>().Delete(doc);
                graph.Persist();
                return true;
            }
            catch (Exception e)
            {
                PXTrace.WriteError(e);
                ex = e;
            }

            return false;
        }
    }
}