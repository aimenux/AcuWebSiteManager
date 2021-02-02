using PX.Data;

namespace PX.Objects.FS
{
    public class ServiceContractAutoNumberAttribute : AlternateAutoNumberAttribute
    {
        #region Private Members

        private string initialNbr = "000001";

        #endregion

        /// <summary>
        /// Allows to calculate the <c>RefNbr</c> sequence when trying to insert a new register
        /// It's called from the Persisting event of FSServiceContract.
        /// </summary>
        protected override bool SetRefNbr(PXCache cache, object row)
        {
            FSServiceContract fsServiceContractRow = (FSServiceContract)row;

            FSServiceContract fsServiceContractRow_tmp = PXSelectGroupBy<FSServiceContract,
                                            Where<
                                                FSServiceContract.customerID, Equal<Current<FSServiceContract.customerID>>>,
                                            Aggregate<
                                                Max<FSServiceContract.customerContractNbr,
                                            GroupBy<
                                                FSServiceContract.customerID>>>>
                                    .Select(cache.Graph);

            string customerContractNbr = fsServiceContractRow_tmp == null ? null : fsServiceContractRow_tmp.CustomerContractNbr;

            if (string.IsNullOrEmpty(customerContractNbr))
            {
                customerContractNbr = initialNbr;
            }
            else
            {
                customerContractNbr = (int.Parse(customerContractNbr) + 1).ToString().PadLeft(initialNbr.Length, '0');
            }

            fsServiceContractRow.CustomerContractNbr = customerContractNbr;

            return true;
        }
    }
}