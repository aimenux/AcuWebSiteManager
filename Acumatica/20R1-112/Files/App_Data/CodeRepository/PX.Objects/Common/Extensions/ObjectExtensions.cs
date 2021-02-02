using System;
using System.Collections.Generic;

namespace PX.Objects.Common.Extensions
{
	public static class ObjectExtensions
	{
		public static bool IsComplex(this object obj)
		{
			return !obj.GetType().IsPrimitive
				   && obj.GetType() != typeof(string)
				   && obj.GetType().IsAssignableFrom(typeof(decimal))
			       && obj.GetType().IsAssignableFrom(typeof(DateTime))
			       && obj.GetType().IsAssignableFrom(typeof(Guid));
		}

		public static TObject[] SingleToArray<TObject>(this TObject obj)
		{
			return new[] { obj };
		}

		public static TObject[] SingleToArrayOrNull<TObject>(this TObject obj)
		{
			if (obj == null)
				return null;

			return new[] { obj };
		}

	    public static List<TObject> SingleToListOrNull<TObject>(this TObject obj)
	    {
	        if (obj == null)
	            return null;

	        return new List<TObject>() {obj};
	    }

        public static object[] SingleToObjectArray<TObject>(this TObject obj, bool dontCreateForNull = true)
	    {
	        if (obj == null && dontCreateForNull)
	            return null; 

	        return new[] { (object)obj };
	    }

        public static List<TObject> SingleToList<TObject>(this TObject obj)
		{
			return new List<TObject>() { obj };
		}
	}
}
