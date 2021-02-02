namespace PX.Objects.Common
{
	/// <summary>
	/// An abstraction that represents an application
	/// of one document to another, exposing the adjusting /
	/// adjusted documents' primary keys.
	/// </summary>
	public interface IDocumentAdjustment
	{
		string AdjgDocType
		{
			get;
			set;
		}

		string AdjgRefNbr
		{
			get;
			set;
		}

		string AdjdDocType
		{
			get;
			set;
		}

		string AdjdRefNbr
		{
			get;
			set;
		}

		string Module
		{
			get;
		}
	}
}
