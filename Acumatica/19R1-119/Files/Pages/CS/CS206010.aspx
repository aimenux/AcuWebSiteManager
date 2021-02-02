<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS206010.aspx.cs" Inherits="Page_CS206010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.CS.RMRowSetMaint" PrimaryView="RowSet">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="CopyRowSet" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="CopyStyle" CommitChanges="true" DependOnGrid="grid" Visible="false" />
			<px:PXDSCallbackCommand Name="PasteStyle" CommitChanges="true" DependOnGrid="grid" Visible="false" />
			<px:PXDSCallbackCommand Name="renumber" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSmartPanel ID="pnlCopyRowSet" runat="server" Style="z-index: 108;" Caption="New Row Set Code" CaptionVisible="True" Key="Parameter" CreateOnDemand="False" AutoCallBack-Target="formCopyRowSet" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="btnOK">
		<px:PXFormView ID="formCopyRowSet" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="Parameter" TabIndex="-32636">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
				<px:PXMaskEdit ID="edNewRowSetCode" runat="server" DataField="NewRowSetCode" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="Copy">
				<AutoCallBack Target="formCopyRowSet" Command="Save" />
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlRenumber" runat="server" Style="z-index: 108;" CaptionVisible="True" Caption="Set Numbering Step" Key="fltNumberingStep" AutoReload="True" 
        AutoCallBack-Enabled="true" AutoCallBack-Target="frmNumberStep" AutoCallBack-Command="Refresh">
		<px:PXFormView ID="frmNumberStep" runat="server" DataMember="fltNumberingStep" SkinID="Transparent"
            Style="z-index: 100" TemplateContainer="" Width="100%" DataSourceID="ds" TabIndex="32300">
			<Template>
				<px:PXLayoutRule runat="server" ControlSize="S" LabelsWidth="S" StartColumn="True" />
				<px:PXNumberEdit ID="edStep" runat="server" DataField="Step" />
				<px:PXNumberEdit ID="edMask" runat="server" DataField="MaskLength" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnSave" runat="server" CommandName="renumber" CommandSourceID="ds" DialogResult="OK" Text="OK" />
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="RowSet" Caption="Row Set" NoteIndicator="True" FilesIndicator="True" TemplateContainer="" 
		TabIndex="-32436">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector ID="edRowSetCode" runat="server" DataField="RowSetCode" DataSourceID="ds" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" ControlSize="S" LabelsWidth="S" StartColumn="True" />
			<px:PXDropDown CommitChanges="True" ID="edType" runat="server" DataField="Type" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="280px" Style="z-index: 100; top: 0px;" Width="100%" Caption="Rows" ActionsPosition="Top" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="Rows">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXMaskEdit ID="edRowCode" runat="server" DataField="RowCode" />
					<px:PXNumberEdit ID="edIndent" runat="server" DataField="Indent" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<pxa:RMFormulaEditor ID="edFormula" runat="server" DataField="Formula" DataSourceID="ds" Parameters="@AccountCode,@AccountDescr,@BaseRowCode,@BookCode,@BranchName,@ColumnCode,@ColumnIndex,@ColumnSetCode,@ColumnText,@EndAccount,@EndAccountGroup,@EndBranch,@EndPeriod,@EndProject,@EndProjectTask,@EndSub,@Organization,@OrganizationName,@StartAccount,@StartAccountGroup,@StartBranch,@StartPeriod,@StartProject,@StartProjectTask,@StartSub,@RowCode,@RowIndex,@RowSetCode,@RowText,@UnitCode,@UnitSetCode,@UnitText,@Today,@WeekStart,@WeekEnd,@MonthStart,@MonthEnd,@QuarterStart,@QuarterEnd,@PeriodStart,@PeriodEnd,@YearStart,@YearEnd"
					ViewNameForParameters="Rows" FieldNameForParameters="RowCode"/>
					<px:PXTextEdit ID="edFormat" runat="server" DataField="Format" />
                    <px:PXNumberEdit ID="edHeight" runat="server" DataField="Height" />
					<px:PXDropDown ID="edPrintControl" runat="server" DataField="PrintControl" />
					<px:PXMaskEdit ID="edColumnGroupID" runat="server" DataField="ColumnGroupID" />
					<px:PXCheckBox ID="chkSuppressEmpty" runat="server" DataField="SuppressEmpty" />
					<px:PXCheckBox ID="chkHideZero" runat="server" DataField="HideZero" />
					<pxa:RMStyleEditor ID="edStyleID" runat="server" DataField="StyleID" DataMember="Style" />
					<pxa:RMDataSetEditor ID="edDataSource" runat="server" DataMember="DataSource" DataSourceID="ds" DataField="DataSourceID" ShowExpandFlag="True" />
					<px:PXMaskEdit ID="edLinkedRowCode" runat="server" DataField="LinkedRowCode" />
                    <px:PXMaskEdit ID="edBaseRowCode" runat="server" DataField="BaseRowCode" />
					<px:PXMaskEdit ID="edUnitGroupID" runat="server" DataField="UnitGroupID" />
					<px:PXCheckBox SuppressLabel="True" ID="chkPageBreak" runat="server" DataField="PageBreak" AlignLeft="True" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="RowCode" Width="60px" />
					<px:PXGridColumn DataField="Description" Width="150px" />
					<px:PXGridColumn DataField="RowType" Type="DropDownList" Width="60px" />
					<px:PXGridColumn DataField="Formula" Width="150px" />
                    <px:PXGridColumn DataField="Format" Width="80px" />
					<px:PXGridColumn AllowUpdate="False" DataField="DataSourceID" TextField="DataSourceIDText" Width="100px" />
					<px:PXGridColumn AllowUpdate="False" DataField="StyleID" TextField="StyleIDText" Width="80px" />
					<px:PXGridColumn DataField="PrintControl" Type="DropDownList" Width="85px" RenderEditorText="True" />
					<px:PXGridColumn DataField="PageBreak" TextAlign="Center" Type="CheckBox" Width="60px" />
					<px:PXGridColumn DataField="Height" TextAlign="Right" Width="60px" />
					<px:PXGridColumn DataField="Indent" TextAlign="Right" Width="60px" />
					<px:PXGridColumn DataField="LineStyle" Type="DropDownList" Width="70px" />
					<px:PXGridColumn DataField="SuppressEmpty" TextAlign="Center" Type="CheckBox" Width="80px" />
					<px:PXGridColumn DataField="HideZero" TextAlign="Center" Type="CheckBox" Width="60px" />
					<px:PXGridColumn DataField="LinkedRowCode" Width="70px" />
                    <px:PXGridColumn DataField="BaseRowCode" Width="60px" />
					<px:PXGridColumn DataField="ColumnGroupID" Width="70px" />
					<px:PXGridColumn DataField="UnitGroupID" Width="60px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode InitNewRow="True" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="Copy Style" CommandSourceID="ds" CommandName="CopyStyle" />
			    <px:PXToolBarButton Text="Paste Style" CommandSourceID="ds" CommandName="PasteStyle" />
			    <px:PXToolBarButton Text="Renumber" PopupPanel="pnlRenumber" />
			</CustomItems>
		</ActionBar>
		<CallbackCommands>
			<Save RepaintControls="Unbound" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>
