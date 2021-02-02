using PX.Data;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System.Collections;
using System.Linq;
using PX.Objects.GL.DAC;
using PX.Objects.CS;
using System;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.SQLTree;

namespace PX.Objects.FA
{
	public class FABookPeriodsMaint : PXGraph<FABookPeriodsMaint>
	{
		public PXFilter<FABookYear> BookYear;

		[PXFilterable]
		public SelectFrom<FABookPeriod>
			.Where<FABookPeriod.bookID.IsEqual<FABookYear.bookID.FromCurrent>
				.And<FABookPeriod.organizationID.IsEqual<FABookYear.organizationID.FromCurrent>>
				.And<FABookPeriod.finYear.IsEqual<FABookYear.year.FromCurrent>>>
			.OrderBy<
				Asc<FABookPeriod.periodNbr>>
			.View.ReadOnly BookPeriod;

		public enum LastFirstYear { Last, First };

		public enum PrevNextYear { Previous, Next, Equal };

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(PXDBIntAttribute), nameof(PXDBIntAttribute.IsKey), false)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Required), true)]
		protected void FABookYear_BookID_CacheAttached(PXCache sender)
		{ }

		[Organization(IsKey = false, ValidateValue = false)]
		protected void FABookYear_OrganizationID_CacheAttached(PXCache sender)
		{ }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(PXDBStringAttribute), nameof(PXDBStringAttribute.IsKey), false)]
		[PXSelector(typeof(Search<
			FABookYear.year,
			Where<FABookYear.organizationID, Equal<Current<FABookYear.organizationID>>,
				And<FABookYear.bookID, Equal<Current<FABookYear.bookID>>>>,
			OrderBy<
				Desc<FABookYear.year>>>))]
		protected void FABookYear_Year_CacheAttached(PXCache sender)
		{ }

		public PXAction<FABookYear> First;
		[PXUIField]
		[PXFirstButton]
		protected virtual IEnumerable first(PXAdapter adapter)
		{
			if (!IsValidCurrent()) yield break;

			FABookYear bookYear = SelectSingleBookYear(LastFirstYear.First);

			BookYear.Cache.Clear();

			yield return bookYear;
		}

		public PXAction<FABookYear> Previous;
		[PXUIField]
		[PXPreviousButton]
		protected virtual IEnumerable previous(PXAdapter adapter)
		{
			if (!IsValidCurrent()) yield break;

			FABookYear current = BookYear.Cache.Current as FABookYear;

			FABookYear bookYear = SelectSingleBookYear(PrevNextYear.Previous, current.Year);

			if (bookYear == null)
			{
				bookYear = SelectSingleBookYear(LastFirstYear.Last);
			}

			BookYear.Cache.Clear();

			yield return bookYear;
		}

		public PXAction<FABookYear> Next;
		[PXUIField]
		[PXNextButton]
		protected virtual IEnumerable next(PXAdapter adapter)
		{
			if (!IsValidCurrent()) yield break;

			FABookYear current = BookYear.Cache.Current as FABookYear;

			FABookYear bookYear = SelectSingleBookYear(PrevNextYear.Next, current.Year);

			if (bookYear == null)
			{
				bookYear = SelectSingleBookYear(LastFirstYear.First);
			}

			BookYear.Cache.Clear();

			yield return bookYear;
		}


		public PXAction<FABookYear> Last;
		[PXUIField]
		[PXLastButton]
		protected virtual IEnumerable last(PXAdapter adapter)
		{
			if (!IsValidCurrent()) yield break;

			FABookYear bookYear = SelectSingleBookYear(LastFirstYear.Last);

			BookYear.Cache.Clear();

			yield return bookYear;
		}

		protected bool IsValidCurrent()
		{
			if (BookYear.Cache.InternalCurrent == null) return false;

			FABookYear current = BookYear.Cache.Current as FABookYear;

			if (current.BookID == null || current.OrganizationID == null) return false;

			FABookYear existYear = SelectFrom<FABookYear>
				.Where<FABookYear.bookID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectSingleBound(this, null, current.BookID);

			if (existYear == null) return false;

			return true;
		}

		protected FABookYear SelectSingleBookYear(LastFirstYear lastFirstYear)
		{
			this.Caches[typeof(FABookYear)].ClearQueryCache();

			PXResultset<FABookYear> query = SelectFrom<FABookYear>
				.Where<
					FABookYear.bookID.IsEqual<FABookYear.bookID.FromCurrent>
					.And<FABookYear.organizationID.IsEqual<FABookYear.organizationID.FromCurrent>>>
				.View.ReadOnly.SelectSingleBound(this, null);

			if (lastFirstYear == LastFirstYear.First)
			{
				return query
					.OrderBy(row => ((FABookYear)row).Year)
					.First();
			}
			else
			{
				return query
					.OrderByDescending(row => ((FABookYear)row).Year)
					.First();
			}
		}

		protected FABookYear SelectSingleBookYear(PrevNextYear direction, string year)
		{
			this.Caches[typeof(FABookYear)].ClearQueryCache();

			PXResultset<FABookYear> query = SelectFrom<FABookYear>
				.Where<
					FABookYear.bookID.IsEqual<FABookYear.bookID.FromCurrent>
					.And<FABookYear.organizationID.IsEqual<FABookYear.organizationID.FromCurrent>>>
				.View.ReadOnly.Select(this);

			if (direction == PrevNextYear.Next)
			{
				return query
					.OrderBy(row => ((FABookYear)row).Year)
					.Where(row => String.Compare(((FABookYear)row).Year, year) > 0)
					.ReadOnly()
					.FirstOrDefault();
			}
			else if (direction == PrevNextYear.Previous)
			{
				return query
					.OrderByDescending(row => ((FABookYear)row).Year)
					.Where(row => String.Compare(((FABookYear)row).Year, year) < 0)
					.ReadOnly()
					.FirstOrDefault();
			}
			else
			{
				return query
					.Where(row => String.Compare(((FABookYear)row).Year, year) == 0)
					.ReadOnly()
					.FirstOrDefault();
			}
		}


		protected virtual void _(Events.RowUpdated<FABookYear> e)
		{
			e.Cache.IsDirty = false;
		}

		protected virtual void _(Events.RowSelected<FABookYear> e)
		{
			FABook book =
				SelectFrom<FABook>
					.Where<FABook.bookID.IsEqual<@P.AsInt>>
				.View.SelectSingleBound(this, null, e.Row.BookID);

			PXUIFieldAttribute.SetVisible<FABookYear.organizationID>(BookYear.Cache, null, (book != null && book.UpdateGL == true && PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>()));
		}

		protected IEnumerable bookYear()
		{
			if (BookYear.Cache.InternalCurrent == null)
			{
				FABookYear defaultYear =
					SelectFrom<FABookYear>
						.InnerJoin<FABook>.On<FABookYear.bookID.IsEqual<FABook.bookID>>
					.Where<FABookYear.organizationID.IsEqual<@P.AsInt>
						.And<FABookYear.endDate.IsGreater<@P.AsDateTime>>>
					.OrderBy<
						Desc<FABook.updateGL>,
						Asc<FABookYear.year>>
					.View.SelectSingleBound(this, null, PXAccess.GetParentOrganizationID(Accessinfo.BranchID), Accessinfo.BusinessDate);

				if (defaultYear == null)
				{
					defaultYear = SelectFrom<FABookYear>
						.InnerJoin<FABook>.On<FABookYear.bookID.IsEqual<FABook.bookID>>
					.Where<FABookYear.organizationID.IsEqual<@P.AsInt>
						.And<FABookYear.endDate.IsLess<@P.AsDateTime>>>
					.OrderBy<
						Desc<FABook.updateGL>,
						Desc<FABookYear.year>>
					.View.SelectSingleBound(this, null, PXAccess.GetParentOrganizationID(Accessinfo.BranchID), Accessinfo.BusinessDate);
				}

				return new object[] { defaultYear ?? throw new PXSetupNotEnteredException<FABookYear>(Messages.NoCalendarDefined) };
			}

			FABookYear currentFABookYear = BookYear.Cache.Current as FABookYear;

			FABook book =
				SelectFrom<FABook>
					.Where<FABook.bookID.IsEqual<@P.AsInt>>
				.View.SelectSingleBound(this, null, currentFABookYear.BookID);

			if (book == null) return null;

			if (book.UpdateGL == true && PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>())
			{
				if (currentFABookYear.OrganizationID == FinPeriod.organizationID.MasterValue)
				{
					currentFABookYear.OrganizationID = PXAccess.GetParentOrganizationID(Accessinfo.BranchID);
				}
			}
			else
			{
				currentFABookYear.OrganizationID = FinPeriod.organizationID.MasterValue;
			}

			FABookYear existingYear =
				SelectFrom<FABookYear>
					.Where<FABookYear.bookID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectSingleBound(this, null, book.BookID);

			if (existingYear == null)
			{
				currentFABookYear.Year = null;
			}
			else if (String.IsNullOrWhiteSpace(currentFABookYear.Year))
			{
				DateTime businessDate = Accessinfo.BusinessDate ?? DateTime.Now;
				currentFABookYear.Year = businessDate.Year.ToString();
			}

			FABookYear bookYear = SelectSingleBookYear(PrevNextYear.Equal, currentFABookYear.Year);

			return new object[] { bookYear };
		}
	}
}