using PX.Data;

namespace PX.Objects.FS
{
    public class ServiceContractAutoNumberAttribute : AlternateAutoNumberAttribute
    {
        protected override string GetInitialRefNbr(string baseRefNbr)
        {
            return "000001";
        }

        /// <summary>
        /// Allows to calculate the <c>RefNbr</c> sequence when trying to insert a new register
        /// It's called from the Persisting event of FSServiceContract.
        /// </summary>
        protected override bool SetRefNbr(PXCache cache, object row)
        {
            FSServiceContract fsServiceContractRow = (FSServiceContract)row;

            FSServiceContract fsServiceContractRowTmp = PXSelectGroupBy<FSServiceContract,
                                                        Where<
                                                            FSServiceContract.customerID, Equal<Current<FSServiceContract.customerID>>>,
                                                        Aggregate<
                                                            Max<FSServiceContract.customerContractNbr,
                                                            GroupBy<FSServiceContract.customerID>>>>
                                                        .Select(cache.Graph);

            string lastRefNbr = fsServiceContractRowTmp?.CustomerContractNbr;

            fsServiceContractRow.CustomerContractNbr = GetNextRefNbr(null, lastRefNbr);

            return true;
        }
    }
}