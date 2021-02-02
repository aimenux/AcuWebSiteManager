using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.IN.Matrix.GraphExtensions;
using PX.Objects.IN.Matrix.DAC.Unbound;

namespace PX.Objects.IN.Matrix.Graphs
{
	public class CreateMatrixItems : PXGraph<CreateMatrixItems, EntryHeader>
	{
		public class CreateMatrixItemsImpl : CreateMatrixItemsExt<CreateMatrixItems, EntryHeader>
		{
		}

		public CreateMatrixItems()
		{
			Save.SetVisible(false);
			Insert.SetVisible(false);
			Delete.SetVisible(false);
			CopyPaste.SetVisible(false);
			Next.SetVisible(false);
			Previous.SetVisible(false);
			First.SetVisible(false);
			Last.SetVisible(false);
		}

		public override bool CanClipboardCopyPaste() => false;
	}
}
