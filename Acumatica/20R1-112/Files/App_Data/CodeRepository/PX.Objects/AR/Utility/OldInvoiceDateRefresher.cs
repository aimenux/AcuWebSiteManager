using System;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.AR
{
    /// <summary>
    /// Internal class used to update <see cref="ARBalances.OldInvoiceDate"/> for the AR documents affected by the release process after it is finished.
    /// </summary>
    class OldInvoiceDateRefresher
    {
        private struct Key
        {
            public int? BranchID;
            public int? CustomerID;
            public int? CustomerLocationID;

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (BranchID ?? 0);
                    hash = hash * 23 + (CustomerID ?? 0);
                    hash = hash * 23 + (CustomerLocationID ?? 0);
                    return hash;
                }
            }
        }
        private HashSet<Key> _trace = new HashSet<Key>();
        private ARInvoiceEarliestDueDateGraph dueDateGraph = PXGraph.CreateInstance<ARInvoiceEarliestDueDateGraph>();

        public void RecordDocument(int? branchID, int? customerID, int? customerLocationID)
        {
            _trace.Add(new Key { BranchID = branchID, CustomerID = customerID, CustomerLocationID = customerLocationID });
        }

        public void CommitRefresh(PXGraph graph)
        {
            foreach (var k in _trace)
            {
                dueDateGraph.EarliestDueDate.View.Clear();
                ARInvoiceEarliestDueDate due = dueDateGraph.EarliestDueDate.Select(k.CustomerID, k.CustomerLocationID, k.BranchID);

                PXUpdate<
                    Set<ARBalances.oldInvoiceDate, Required<ARInvoiceEarliestDueDate.dueDate>>,
                    ARBalances,
                    Where<ARBalances.customerID, Equal<Required<AR.ARInvoice.customerID>>,
                        And<ARBalances.customerLocationID, Equal<Required<AR.ARInvoice.customerLocationID>>,
                        And<ARBalances.branchID, Equal<Required<AR.ARInvoice.branchID>>>>>>
                    .Update(graph, due?.DueDate, k.CustomerID, k.CustomerLocationID, k.BranchID);
            }
        }
    }

    class ARInvoiceEarliestDueDateGraph : PXGraph<ARInvoiceEarliestDueDateGraph>
    {
        public PXSelect<ARInvoiceEarliestDueDate,
                    Where<ARInvoiceEarliestDueDate.customerID, Equal<Required<ARInvoiceEarliestDueDate.customerID>>,
                        And<ARInvoiceEarliestDueDate.customerLocationID, Equal<Required<ARInvoiceEarliestDueDate.customerLocationID>>,
                        And<ARInvoiceEarliestDueDate.branchID, Equal<Required<ARInvoiceEarliestDueDate.branchID>>>>>> EarliestDueDate;
    }

    /// <summary>
    /// Internal DAC used to execute the <see cref="ARBalances.OldInvoiceDate"/> update statement in the <see cref="OldInvoiceDateRefresher"/>.
    /// </summary>
    [PXHidden]
    [PXProjection(typeof(Select4<ARInvoice,
            Where<ARInvoice.released, Equal<True>,
                And<ARInvoice.openDoc, Equal<True>,
                And<ARInvoice.dueDate, IsNotNull,
                And<ARInvoice.origDocAmt, Greater<decimal0>>>>>,
            Aggregate<Min<ARInvoice.dueDate,
                GroupBy<ARInvoice.customerID,
                GroupBy<ARInvoice.customerLocationID,
                GroupBy<ARInvoice.branchID>>>>>>))]
    public class ARInvoiceEarliestDueDate : IBqlTable
    {
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        /// <summary>
        /// The identifier of the <see cref="Branch"/>.
        /// </summary>
        [PXDBInt(IsKey = true, BqlField = typeof(ARInvoice.branchID))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        /// <summary>
        /// The identifier of the <see cref="Customer"/>.
        /// </summary>
        [PXDBInt(IsKey = true, BqlField = typeof(ARInvoice.customerID))]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
        /// <summary>
        /// The identifier of the <see cref="Location">Customer Location</see>.
        /// </summary>
        [PXDBInt(IsKey = true, BqlField = typeof(ARInvoice.customerLocationID))]
        public virtual int? CustomerLocationID { get; set; }
        #endregion
        #region DueDate
        public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
        /// <summary>
        /// The earliest due date among all documents of the corresponding <see cref="BranchID"/>, <see cref="CustomerID"/>, <see cref="CustomerLocationID"/>.
        /// </summary>
        [PXDBDate(BqlField = typeof(ARInvoice.dueDate))]
        public virtual DateTime? DueDate { get; set; }
        #endregion
    }
}
