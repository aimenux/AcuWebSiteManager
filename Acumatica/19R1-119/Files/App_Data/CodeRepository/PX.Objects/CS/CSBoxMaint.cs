using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.CS
{
	public class CSBoxMaint : PXGraph<CSBoxMaint>
	{
		public PXSetup<CommonSetup> Setup;
        public PXSelectJoin<CSBox, CrossJoin<CommonSetup>> Records;
        public PXSavePerRow<CSBox> Save;
        public PXCancel<CSBox> Cancel;


		public CSBoxMaint()
		{
			CommonSetup record = Setup.Current;
		}

		protected virtual void CSBox_BoxWeight_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CSBox row = (CSBox) e.Row;
			if (row == null) return;

			if ( (decimal?)e.NewValue >= row.MaxWeight )
				throw new PXSetPropertyException(Messages.WeightOfEmptyBoxMustBeLessThenMaxWeight);
		}
    }
}
