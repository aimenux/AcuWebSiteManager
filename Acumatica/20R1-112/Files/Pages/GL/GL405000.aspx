<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="GL405000.aspx.cs" Inherits="Page_GL405000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="true" Width="100%" TypeName="PX.Objects.GL.Reclassification.UI.ReclassificationHistoryInq" PrimaryView="TransView">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="Reclassify" DependOnGrid="grid" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="ReclassifyAll" DependOnGrid="grid" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="ReclassificationHistory" DependOnGrid="grid" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="ViewBatch" DependOnGrid="grid" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="ViewOrigBatch" DependOnGrid="grid" CommitChanges="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="PrimaryInquire" SyncPosition="true" OnRowDataBound="grid_OnRowDataBound">
        <AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="True"  /> 
		<Levels>
            <px:PXGridLevel DataMember="TransView">
				<Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" CommitChanges="True" ForceExport="true"/>
                    <px:PXGridColumn DataField="OrigBatchNbr" LinkCommand="ViewOrigBatch"/>
                    <px:PXGridColumn DataField="ActionDesc" />
				    <px:PXGridColumn DataField="SplitIcon" AllowResize="false" AllowFilter="false" AllowSort="false" ForceExport="true"/>
                    <px:PXGridColumn DataField="BatchNbr" TextAlign="Right" LinkCommand="ViewBatch" />
					<px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
					<px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="AccountID" />
					<px:PXGridColumn DataField="AccountID_Account_description" />
					<px:PXGridColumn DataField="SubID"/>
                    <px:PXGridColumn DataField="CuryDebitAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryReclassRemainingAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranDesc" />
					<px:PXGridColumn DataField="TranDate" DisplayFormat="d" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="ReclassSeqNbr" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
