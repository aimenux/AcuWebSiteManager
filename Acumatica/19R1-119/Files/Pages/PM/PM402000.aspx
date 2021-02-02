<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM402000.aspx.cs"
    Inherits="Page_PM402000l" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.PM.TaskInquiry" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" Caption="Selection">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L"/>
		    <px:PXSelector runat="server" ID="edApproverID" DataField="ApproverID" CommitChanges="True"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" SyncPosition="True" RestrictFields="True"
        FastFilterFields="ProjectID, PMProject__Description,TaskCD,Description">
        <Levels>
            <px:PXGridLevel DataMember="FilteredItems">
                <Columns>
                    <px:PXGridColumn DataField="ProjectID" Label="Project ID" Visible="False" LinkCommand="ViewProject" />
                    <px:PXGridColumn DataField="PMProject__Description" Label="Project-Description" Width="120px" />
                    <px:PXGridColumn DataField="TaskCD" Label="Task ID" LinkCommand="ViewTask" />
                    <px:PXGridColumn DataField="Description" Label="Description" Width="120px" />
                    <px:PXGridColumn DataField="LocationID" Label="Location"/>
                    <px:PXGridColumn DataField="RateTableID" Label="Rate Table" />
                    <px:PXGridColumn DataField="Status" Label="Status" RenderEditorText="True" />
                    <px:PXGridColumn DataField="CompletedPercent" Label="Completed (%)" TextAlign="Right" Width="90px"/>
                    <px:PXGridColumn DataField="PlannedStartDate" Label="Planned Start" Width="90px" />
                    <px:PXGridColumn DataField="PlannedEndDate" Label="Planned End" Width="90px" />
                    <px:PXGridColumn DataField="StartDate" Label="Start Date" Width="90px" />
                    <px:PXGridColumn DataField="EndDate" Label="End Date" Width="90px" />
                    <px:PXGridColumn DataField="ApproverID" Width="90px"/>
                    <px:PXGridColumn DataField="DefaultSubID" Label="Default Sub." />
                    <px:PXGridColumn DataField="VisibleInGL" Label="GL" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="VisibleInAP" Label="AP" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="VisibleInAR" Label="AR" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="VisibleInSO" Label="SO" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="VisibleInPO" Label="PO" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="VisibleInTA" Label="TA" TextAlign="Center" Type="CheckBox" Width="60px"/>
                    <px:PXGridColumn DataField="VisibleInEA" Label="EA" TextAlign="Center" Type="CheckBox" Width="60px"/>
                    <px:PXGridColumn DataField="VisibleInIN" Label="IN" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="PMTaskTotal__CuryAsset" Label="PMTaskTotal-Asset" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="PMTaskTotal__CuryLiability" Label="PMTaskTotal-Liability" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="PMTaskTotal__CuryIncome" Label="PMTaskTotal-Income" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="PMTaskTotal__CuryExpense" Label="PMTaskTotal-Expense" TextAlign="Right" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar DefaultAction="ViewTask"/>
    </px:PXGrid>
</asp:Content>
