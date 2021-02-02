using PX.Objects.CM;

namespace PX.Objects.DR.Descriptor
{
	/// <summary>
	/// Represents a line of an AR / AP document, 
	/// in parts relevant to Deferred Revenue.
	/// </summary>
	public interface IDocumentLine : IDocumentTran
	{
		/// <summary>
		/// The module of the source document
		/// should either be <see cref="GL.BatchModule.AR"/> 
		/// or <see cref="GL.BatchModule.AP"/>.
		/// </summary>
		string Module { get; }
		string DeferredCode { get; }
	}
}