using PX.Data;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA
{
	public class FABookCollection : IPrefetchable
	{
		public Dictionary<int, FABook> Books = new Dictionary<int, FABook>();

		public void Prefetch()
		{
			Books = PXDatabase
				.SelectMulti<FABook>(
					new PXDataField<FABook.bookID>(),
					new PXDataField<FABook.bookCode>(),
					new PXDataField<FABook.updateGL>(),
					new PXDataField<FABook.description>())
				.Select(row => new FABook
				{
					BookID = row.GetInt32(0),
					BookCode = row.GetString(1).Trim(),
					UpdateGL = row.GetBoolean(2),
					Description = row.GetString(3)?.Trim()
				})
				.ToDictionary(book => (int)book.BookID);
		}
	}
}
