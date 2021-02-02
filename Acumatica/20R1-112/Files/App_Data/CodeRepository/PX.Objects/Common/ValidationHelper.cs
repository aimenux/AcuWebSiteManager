using PX.Data;

namespace PX.Objects.Common
{
	public sealed class ValidationHelper : ValidationHelper<ValidationHelper>
	{
		public ValidationHelper(PXCache cache, object row) : base(cache, row)
		{
		}
	}

	public class ValidationHelper<T> 
		where T : ValidationHelper<T>
	{
		protected readonly object Row;
		protected readonly PXCache Cache;
		public bool IsValid { get; protected set; }

		public static bool SetErrorEmptyIfNull<TField>(PXCache cache, object row, object value)
			where TField : IBqlField
		{
			if (value == null)
			{
				cache.RaiseExceptionHandling<TField>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<TField>(cache)));
				return false;
			}

			return true;
		}

		public ValidationHelper(PXCache cache, object row)
		{
			Row = row;
			Cache = cache;
			IsValid = true;
		}

		public T SetErrorEmptyIfNull<TField>(object value)
			where TField : IBqlField
		{
			IsValid &= SetErrorEmptyIfNull<TField>(Cache, Row, value);
			return (T)this;
		}
	}
}
