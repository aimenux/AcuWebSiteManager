using PX.Data;

namespace PX.Objects.CR.MassProcess
{	
	public interface IPXTransactionCache : IPXExtensableCache
	{
		void Backup();
		void Restore();
		PXGraph Graph { get; }
		void Commit();
	}
}