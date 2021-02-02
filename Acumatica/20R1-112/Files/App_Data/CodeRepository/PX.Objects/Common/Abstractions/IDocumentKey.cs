using PX.Objects.AP;
using PX.Objects.AR;

namespace PX.Objects.Common.Abstractions
{
	/// <summary>
	/// Abstracts an entity identified by the document type and reference number,
	/// e.g. an <see cref="ARRegister"/> or <see cref="APRegister"/>.
	/// </summary>
	public interface IDocumentKey
	{
		string DocType
		{
			get;
			set;
		}

		string RefNbr
		{
			get;
			set;
		}
	}
}
