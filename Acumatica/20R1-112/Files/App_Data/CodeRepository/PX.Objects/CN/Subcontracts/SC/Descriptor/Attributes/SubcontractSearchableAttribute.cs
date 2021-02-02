using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Subcontracts.SC.DAC;
using PX.Objects.PO;
using PX.Objects.SM;

namespace PX.Objects.CN.Subcontracts.SC.Descriptor.Attributes
{
    public class SubcontractSearchableAttribute : PXSearchableAttribute
    {
        private const string TitlePrefix = "{0} - {2}";
        private const string FirstLineFormat = "{0}{1:d}{2}{3}{4}";
        private const string SecondLineFormat = "{0}";

        private static readonly Type[] FieldsForTheFirstLine =
        {
            typeof(POOrder.orderType),
            typeof(POOrder.orderDate),
            typeof(POOrder.status),
            typeof(POOrder.vendorRefNbr),
            typeof(POOrder.expectedDate)
        };

        private static readonly Type[] Fields =
        {
            typeof(POOrder.vendorRefNbr),
            typeof(POOrder.orderDesc)
        };

        private static readonly Type[] TitleFields =
        {
            typeof(POOrder.orderNbr),
            typeof(POOrder.vendorID),
            typeof(Vendor.acctName)
        };

        public SubcontractSearchableAttribute()
            : base(SearchCategory.PO, TitlePrefix, TitleFields, Fields)
        {
            NumberFields = typeof(POOrder.orderNbr).CreateArray();
            Line1Format = FirstLineFormat;
            Line1Fields = FieldsForTheFirstLine;
            Line2Format = SecondLineFormat;
            Line2Fields = typeof(POOrder.orderDesc).CreateArray();
            MatchWithJoin = typeof(InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POOrder.vendorID>>>);
            SelectForFastIndexing =
                typeof(Select2<POOrder, InnerJoin<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>>>>);
        }

        public override SearchIndex BuildSearchIndex(PXCache cache, object commitment, PXResult result, string noteText)
        {
            var content = BuildContent(cache, commitment, result);
            var entityType = ((POOrder) commitment).OrderType == POOrderType.RegularSubcontract
                ? typeof(Subcontract).FullName
                : typeof(POOrder).FullName;
            return new SearchIndex
            {
                IndexID = Guid.NewGuid(),
                NoteID = (Guid?) cache.GetValue(commitment, typeof(Note.noteID).Name),
                Category = category,
                Content = $"{content} {noteText}",
                EntityType = entityType
            };
        }
    }
}
