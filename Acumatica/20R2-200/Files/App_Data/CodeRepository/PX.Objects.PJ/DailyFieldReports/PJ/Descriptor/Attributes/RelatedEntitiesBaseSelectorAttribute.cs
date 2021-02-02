using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Descriptor;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class RelatedEntitiesBaseSelectorAttribute : PXCustomSelectorAttribute
    {
        protected IEnumerable<string> RelatedEntitiesIds;

        protected RelatedEntitiesBaseSelectorAttribute(Type type, params Type[] fieldList)
            : base(type, fieldList)
        {
        }

        public override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            if (args.NewValue == null || !ValidateValue || _BypassFieldVerifying.Value)
            {
                return;
            }
            if (RelatedEntitiesIds.All(id => id != args.NewValue.ToString()))
            {
                throw new PXSetPropertyException(SharedMessages.CannotBeFound, _FieldName);
            }
        }

        protected IEnumerable<TTable> GetRelatedEntities<TTable, TProjectField>()
            where TTable : class, IBqlTable, new()
            where TProjectField : BqlInt.Field<TProjectField>
        {
            return SelectFrom<TTable>
                .Where<BqlInt.Field<TProjectField>.IsEqual<DailyFieldReport.projectId.FromCurrent>>.View
                .Select(_Graph).FirstTableItems;
        }

        protected IEnumerable<TTable> GetRelatedEntities<TTable>()
            where TTable : class, IBqlTable, new()
        {
            return SelectFrom<TTable>.View.Select(_Graph).FirstTableItems;
        }
    }
}
