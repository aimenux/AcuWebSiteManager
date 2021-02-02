<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="WZ201500.aspx.cs" Inherits="Page_WZ201500" 
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Scenario" TypeName="PX.Objects.WZ.WizardScenarioMaint" Visible="True" >
	    <CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="Cancel" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="StartTask"  CommitChanges="True" PopupCommand="Refresh" PopupCommandTarget="grid" RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="ViewTask" Visible="False" CommitChanges="True" PopupCommand="Refresh"  RepaintControls="All"/>
            <px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Reopen" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Assign" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="CompleteScenario" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Skip" Visible="False" CommitChanges="True" PopupCommand="Refresh" PopupCommandTarget="mapForm" RepaintControls="All" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXFormView ID="scenarioForm" runat="server" DataMember="Scenario" DataSourceID="ds" RenderStyle="Simple"></px:PXFormView>
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%"
		AllowPaging="True" AllowSearch="false" AdjustPageSize="Auto" AutoAdjustColumns="True" DataSourceID="ds" SkinID="PrimaryInquire" SyncPosition="True"
        OnRowDataBound="grid_RowDataBound">
		<Levels>
			<px:PXGridLevel DataMember="Tasks">
                <Columns>
                    <px:PXGridColumn DataField="Name" Width="450px" CommitChanges="True" AllowUpdate="False"  AllowSort="False" LinkCommand="ViewTask"/>
                    <px:PXGridColumn DataField="Status" Width="100px" AllowSort="False"/>
                    <px:PXGridColumn DataField="AssignedTo" Width="200px" AllowSort="False"/>
                    <px:PXGridColumn DataField="CompletedBy" Width="200px" AllowSort="False" AllowShowHide="true" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="StartedDate" Width="120px" DisplayFormat="g" AllowSort="False" AllowShowHide="true" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="CompletedDate" Width="120px" DisplayFormat="g" AllowSort="False" AllowShowHide="true" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="IsOptional" Width="200px" Type="CheckBox" TextAlign="Center" AllowSort="False"/>
                </Columns>
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXMaskEdit ID="edName" runat="server" DataField="Name" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="false"/>
                    <px:PXSelector ID="edAssignedTo" runat="server" DataField="AssignedTo" Enabled="false"/>
                    <px:PXSelector ID="edCompletedBy" runat="server" DataField="CompletedBy" Enabled="false"/>
                </RowTemplate>
			</px:PXGridLevel> 
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
	</px:PXGrid>
    <px:PXSmartPanel ID="spAssignDlg" runat="server" Key="CurrentTask" LoadOnDemand="True" ShowAfterLoad="true"
        AutoCallBack-Enabled="true" AutoCallBack-Target="assignForm" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" Width="400"
        AcceptButtonID="btnOk" CancelButtonID="btnCancel" Caption="Assign Task To" CaptionVisible="True">
        <px:PXFormView ID="assignForm" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" 
            SkinID="Transparent" DataMember="CurrentTask">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="XL" SuppressLabel="True" />
                    <px:PXSelector ID="edAssignedToTask" runat="server" DataField="AssignedTo" CommitChanges="True" SuppressLabel="True"/>
                    <px:PXCheckBox ID="chkOverrideAssignee" runat="server" DataField="OverrideAssignee" CommitChanges="True"/>
                </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOk" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="btnCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
