using System;
using PX.Web.UI;

public partial class Page_GL102000 : PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		PXGroupBox gb = (PXGroupBox)form.FindControl("gbCOAOrder");
		((PXRadioButton)gb.FindControl("gbCOAOrder_op0")).Text = PX.Objects.GL.Messages.COAOrderOp0;
		((PXRadioButton)gb.FindControl("gbCOAOrder_op1")).Text = PX.Objects.GL.Messages.COAOrderOp1;
		((PXRadioButton)gb.FindControl("gbCOAOrder_op2")).Text = PX.Objects.GL.Messages.COAOrderOp2;
		((PXRadioButton)gb.FindControl("gbCOAOrder_op3")).Text = PX.Objects.GL.Messages.COAOrderOp3;
		((PXRadioButton)gb.FindControl("gb_COAOrder_op128")).Text = PX.Objects.GL.Messages.COAOrderOp128;
	}
}
