using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public enum ConfigCuryType
    {
        Base,
        Document
    }

    public class AMConfigurationPriceAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
    {
        public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if(e.Row != null && e.Row is AMConfigurationResults)
            {
                var row = (AMConfigurationResults)e.Row;
                e.ReturnValue = GetExtPrice(sender, row, sender.Graph.Accessinfo.CuryViewState ? ConfigCuryType.Base : ConfigCuryType.Document);
            }
        }

        public decimal GetExtPrice(PXCache sender, AMConfigurationResults row, ConfigCuryType curyType)
        {
            if (row == null || sender == null)
            {
                return 0m;
            }

            AMConfiguration configuration = PXSelect<AMConfiguration,
                                                Where<AMConfiguration.configurationID,
                                                    Equal<Required<AMConfiguration.configurationID>>,
                                                And<AMConfiguration.revision,
                                                    Equal<Required<AMConfiguration.revision>>>>>.Select(sender.Graph, row.ConfigurationID, row.Revision);
            if (configuration != null)
            {
                switch (configuration.PriceCalc)
                {
                    case CalcOptions.OnCompletion:
                        if (row.Completed.GetValueOrDefault())
                            return GetExtPrice(row, configuration.PriceRollup, curyType);
                        else
                            return 0m;
                    default:
                        return GetExtPrice(row, configuration.PriceRollup, curyType);

                }
            }

            return 0m;
        }

        private decimal GetExtPrice(AMConfigurationResults row, string rollup, ConfigCuryType curyType)
        {
            if (curyType == ConfigCuryType.Base)
            {
                return GetExtPrice(row.FixedPriceTotal.GetValueOrDefault(),
                                   row.OptionPriceTotal.GetValueOrDefault(),
                                   row.BOMPriceTotal.GetValueOrDefault(),
                                   rollup);
            }

            return GetExtPrice(row.CuryFixedPriceTotal.GetValueOrDefault(),
                row.CuryOptionPriceTotal.GetValueOrDefault(),
                row.CuryBOMPriceTotal.GetValueOrDefault(),
                rollup);
        }

        private decimal GetExtPrice(decimal fixedPriceTotal, decimal optionPriceTotal, decimal bOMPriceTotal, string rollup)
        {
            switch (rollup)
            {
                case RollupOptions.Parent:
                    return fixedPriceTotal;
                case RollupOptions.ChildrenAll:
                    return optionPriceTotal + bOMPriceTotal;
                case RollupOptions.ChildrenCFG:
                    return optionPriceTotal;
                case RollupOptions.ParentChildren:
                default:
                    return fixedPriceTotal + optionPriceTotal + bOMPriceTotal;
            }
        }

        public static decimal? GetPriceExt<Field>(PXCache sender, AMConfigurationResults row, ConfigCuryType curyType)
            where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly<Field>())
            {
                if (attr is AMConfigurationPriceAttribute)
                {
                    var configPriceAttr = (AMConfigurationPriceAttribute)attr;
                    return configPriceAttr.GetExtPrice(sender, row, curyType);
                }
            }
            return null;
        }
    }
}
