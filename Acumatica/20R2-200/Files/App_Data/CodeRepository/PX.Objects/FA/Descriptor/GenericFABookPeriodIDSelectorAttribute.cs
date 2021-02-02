using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common.DAC;
using PX.Objects.GL.Attributes;

namespace PX.Objects.FA.Descriptor
{
	public class FAQueryParameters : QueryParameters
	{
		public new abstract class organizationID : IBqlField { }
		public new abstract class branchID : IBqlField { }
		#region OrgBAccountID
		public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }

		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
		public int? OrgBAccountID { get; set; }
		#endregion
		#region AssetID
		public abstract class assetID : IBqlField {}
		[PXSelector(typeof(Search<FixedAsset.assetID>),
			SubstituteKey = typeof(FixedAsset.assetCD), DescriptionField = typeof(FixedAsset.description))]
		[PXDBInt]
		public virtual int? AssetID
		{
			get;
			set;
		}
		#endregion
		#region BookID
		public abstract class bookID : IBqlField {}
		[PXSelector(typeof(Search<FABook.bookID>),
			SubstituteKey = typeof(FABook.bookCode), DescriptionField = typeof(FABook.description))]
		[PXDBInt]
		public virtual int? BookID
		{
			get;
			set;
		}
		#endregion
	}

	public class GenericFABookPeriodSelectorAttribute: PXCustomSelectorAttribute
    {
        public Type OrigSearchType { get; set; }

		protected ReportParametersFlag ReportParametersMask;

        public FABookPeriodKeyProvider BookPeriodKeyProvider { get; set; }

		public GenericFABookPeriodSelectorAttribute(
			Type searchType,
			FABookPeriodKeyProvider bookPeriodKeyProvider,
			ReportParametersFlag reportParametersMask = ReportParametersFlag.None,
            Type[] fieldList = null)
        :base(GetSearchType(searchType, reportParametersMask), fieldList)
        {
            OrigSearchType = searchType;

            BookPeriodKeyProvider = bookPeriodKeyProvider;
			SelectorMode |= PXSelectorMode.NoAutocomplete;

			ReportParametersMask = reportParametersMask;
		}

        protected virtual IEnumerable GetRecords()
        {
            PXCache cache = _Graph.Caches[_CacheType];

            object extCurrentRow = PXView.Currents.FirstOrDefault(c => _CacheType.IsAssignableFrom(c.GetType()));

            FABookPeriod.Key periodKey =
                ReportParametersMask != ReportParametersFlag.None
                ? BookPeriodKeyProvider.GetKeyFromReportParameters(_Graph, PXView.Parameters, ReportParametersMask)
                : BookPeriodKeyProvider.GetKey(_Graph, cache, extCurrentRow);

            int startRow = PXView.StartRow;
            int totalRows = 0;

            List<object> parameters = new List<object>();

            BqlCommand cmd = GetCommand(cache, extCurrentRow, parameters, periodKey);

            PXView view = new PXView(_Graph, PXView.View?.IsReadOnly ?? true, cmd);

            try
            {
                return view.Select(PXView.Currents, parameters.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
            }
            finally
            {
                PXView.StartRow = 0;
            }
        }

		public static Type GetSearchType(Type origSearchType, ReportParametersFlag reportParametersMask)
		{
			//params will be passed into GetRecords context if they will be parsed from the query
			if (reportParametersMask != ReportParametersFlag.None)
			{
				BqlCommand cmd = BqlCommand.CreateInstance(origSearchType);

				if ((reportParametersMask & ReportParametersFlag.Organization) == ReportParametersFlag.Organization)
				{
					cmd = cmd.WhereAnd<Where<FABookPeriod.organizationID, Equal<Optional2<FAQueryParameters.organizationID>>>>();
				}
				if ((reportParametersMask & ReportParametersFlag.Branch) == ReportParametersFlag.Branch)
				{
					cmd = cmd.WhereAnd<Where<FABookPeriod.organizationID, Equal<Optional2<FAQueryParameters.branchID>>>>();
				}
				if ((reportParametersMask & ReportParametersFlag.BAccount) == ReportParametersFlag.BAccount)
				{
					cmd = cmd.WhereAnd<Where<FABookPeriod.organizationID, Equal<Optional2<FAQueryParameters.orgBAccountID>>>>();
				}
				if ((reportParametersMask & ReportParametersFlag.FixedAsset) == ReportParametersFlag.FixedAsset)
				{
					cmd = cmd.WhereAnd<Where<FABookPeriod.organizationID, Equal<Optional2<FAQueryParameters.assetID>>>>();
				}
				if ((reportParametersMask & ReportParametersFlag.Book) == ReportParametersFlag.Book)
				{
					cmd = cmd.WhereAnd<Where<FABookPeriod.bookID, Equal<Optional2<FAQueryParameters.bookID>>>>();
				}

				return cmd.GetType();
			}

			return origSearchType;
		}


		protected virtual BqlCommand GetCommand(PXCache cache, object extRow, List<object> parameters, FABookPeriod.Key periodKey)
        {
            BqlCommand cmd = BqlCommand.CreateInstance(OrigSearchType);

            cmd = cmd.WhereAnd<Where<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
                And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>();

            parameters.Add(periodKey.OrganizationID);
            parameters.Add(periodKey.BookID);

            return cmd;
        }
    }
}
