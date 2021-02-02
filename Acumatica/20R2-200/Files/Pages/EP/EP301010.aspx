<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP301010.aspx.cs" Inherits="Page_EP301010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.ExpenseClaimDetailMaint" PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="viewClaim" Visible="False" RepaintControls="All" DependOnGrid="grid" PopupCommand="Cancel" PopupCommandTarget="ds" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
   <px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edEmployee" NoteField="">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edEmployee" runat="server" DataField="EmployeeID"  />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" 
		Caption="Expense Receipts" AdjustPageSize="Auto" SkinID="PrimaryInquire" SyncPosition="True" KeepPosition="true" FastFilterFields="ExpenseRefNbr,TranDesc">
		<Levels>
			<px:PXGridLevel DataMember="ClaimDetails" DataKeyNames="ClaimDetailID">
				<RowTemplate>
					<px:PXSelector CommitChanges="True" runat="server" DataField="EmployeeID" ID="edEmployeeID1" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="ClaimDetailCD" TextAlign="Right" />
					<px:PXGridColumn DataField="ContractID" />
					<px:PXGridColumn DataField="TaskID" />
					<px:PXGridColumn DataField="CostCodeID" />
					<px:PXGridColumn DataField="ExpenseDate" />
					<%--<px:PXGridColumn DataField="ByCorporateCard" TextAlign="Center" Type="CheckBox" />--%>
                    <px:PXGridColumn DataField="Status" ></px:PXGridColumn>
					<px:PXGridColumn DataField="TranDesc" LinkCommand="editDetail" ></px:PXGridColumn>
					<px:PXGridColumn DataField="ExpenseRefNbr" ></px:PXGridColumn>
					<px:PXGridColumn DataField="CuryTranAmtWithTaxes" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand="viewClaim" />
					<px:PXGridColumn DataField="EmployeeID" DisplayMode="Text" />
					<px:PXGridColumn DataField="CreatedByID" DisplayMode="Text" />
					<px:PXGridColumn DataField="BranchID" />
				</Columns>
				<Mode AllowUpdate="False" />
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="editDetail" PagerVisible="False" />
	</px:PXGrid>

</asp:Content>
