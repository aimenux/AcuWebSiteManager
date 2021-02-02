<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP301030.aspx.cs" Inherits="Page_EP301030" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%"  Visible="True" runat="server" TypeName="PX.Objects.EP.ExpenseClaimMaint"  PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
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
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="RefNbr,DocDesc" AutoSize="True">
		<Levels>
			<px:PXGridLevel DataMember="Claim">
                <Columns>
                    <px:PXGridColumn DataField="DocDate" />
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="EditDetail"/>
                    <px:PXGridColumn DataField="Status" />
                    <px:PXGridColumn DataField="DocDesc"  />
                    <px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryID" />
                    <px:PXGridColumn DataField="EmployeeID" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="CreatedByID" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="DepartmentID" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="ApproveDate"/>
                    <px:PXGridColumn DataField="BranchID" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	    <ActionBar DefaultAction="EditDetail" PagerVisible="False"/>
	</px:PXGrid>
</asp:Content>
