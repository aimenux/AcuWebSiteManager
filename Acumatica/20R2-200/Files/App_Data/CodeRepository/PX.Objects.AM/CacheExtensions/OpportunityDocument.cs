using System;
using PX.Data;

namespace PX.Objects.AM.CacheExtensions
{
    public class OpportunityDocument : PXMappedCacheExtension
    {
        public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }
        public virtual string OpportunityID { get; set; }

        /// <summary>
        /// Opportunity quotes started using a GUID (CROpportunityRevision.NoteID) in 2018R2 (replacing OpportunityID & RevisionID keys)
        /// </summary>
        public abstract class quoteID : PX.Data.BQL.BqlString.Field<quoteID> { }
        /// <summary>
        /// Opportunity quotes started using a GUID (CROpportunityRevision.NoteID) in 2018R2 (replacing OpportunityID & RevisionID keys)
        /// </summary>
        public virtual Guid? QuoteID { get; set; }

        public abstract class quoteNbr : PX.Data.BQL.BqlString.Field<quoteNbr> { }
        public virtual string QuoteNbr { get; set; }

        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        public virtual int? BAccountID { get; set; }

        public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
        public virtual bool? Approved { get; set; }

        public abstract class isDisabled : PX.Data.BQL.BqlBool.Field<isDisabled> { }
        public virtual bool? IsDisabled { get; set; }
    }
}