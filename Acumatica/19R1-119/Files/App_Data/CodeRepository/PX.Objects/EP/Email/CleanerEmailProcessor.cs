using PX.Data;

namespace PX.Objects.EP
{
	public class CleanerEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true || account.DeleteUnProcessed != true) return false;
			if (account.TypeDelete == TypeDeleteAttribute._Failed && package.IsProcessed) return false;
			if (account.TypeDelete == TypeDeleteAttribute._Successful && !package.IsProcessed) return false;

			var message = package.Message;
			package.Graph.Caches[message.GetType()].Delete(message);
			return true;
		}
	}
}
