<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="GL506000.aspx.cs" Inherits="Page_GL506000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.Reclassification.UI.ReclassifyTransactionsProcess" PrimaryView="GLTranForReclass">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="ViewDocument" />
            <px:PXDSCallbackCommand Name="Delete" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="Replace" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="Split" CommitChanges="True" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ValidateAndProcess" CommitChanges="True"/>
            <px:PXDSCallbackCommand Name="ViewReclassBatch" DependOnGrid="grid" CommitChanges="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="PrimaryInquire" SyncPosition="True" OnRowDataBound="grid_OnRowDataBound">
		<Levels>
            <px:PXGridLevel DataMember="GLTranForReclass">
				<RowTemplate>
					<px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID"  />
					<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
					<px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" Size="S" />
			        <px:PXTextEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
					<px:PXNumberEdit ID="edCuryDebitAmt" runat="server" DataField="CuryDebitAmt" Size="S" />
					<px:PXNumberEdit ID="edCuryCreditAmt" runat="server" DataField="CuryCreditAmt" Size="S" />
                    
                    <px:PXSegmentMask ID="edNewBranch" runat="server" DataField="NewBranchID" OnValueChange="Commit" />
					<px:PXSegmentMask ID="edNewAccountID" runat="server" DataField="NewAccountID" AutoRefresh="True" OnValueChange="Commit" />
					<px:PXSegmentMask ID="edNewSubID" runat="server" DataField="NewSubID" AutoRefresh="True" OnValueChange="Commit" />
					<px:PXDateTimeEdit ID="edNewTranDate" runat="server" DataField="NewTranDate" Size="S" />
					<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />                
				</RowTemplate>
				<Columns>
				    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" CommitChanges="True" ForceExport="true"/>
                    <px:PXGridColumn DataField="SplittedIcon" AllowShowHide="Server" AllowResize="false" AllowFilter="false" AllowSort="false" ForceExport="true"/>
                    <px:PXGridColumn DataField="ReclassBatchNbr" TextAlign="Right" AllowShowHide="Server" LinkCommand="ViewReclassBatch"/>
                    <px:PXGridColumn DataField="NewBranchID" CommitChanges="True" />
					<px:PXGridColumn DataField="NewAccountID" CommitChanges="True" />
					<px:PXGridColumn DataField="NewAccountID_Account_description" />
					<px:PXGridColumn DataField="NewSubID"  CommitChanges="True"/>
                    <px:PXGridColumn DataField="NewTranDate" DisplayFormat="d"  CommitChanges="True"/>
                    <px:PXGridColumn DataField="NewTranDesc" />
                    <px:PXGridColumn DataField="CuryNewAmt" CommitChanges="True" />
                    <px:PXGridColumn DataField="CuryDebitAmt" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryID" CommitChanges="True"/>
				    <px:PXGridColumn DataField="Module" TextAlign="Right" />
				    <px:PXGridColumn DataField="BatchNbr" TextAlign="Right" />
					<px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
					<px:PXGridColumn DataField="BranchID" AutoCallBack="True" />
					<px:PXGridColumn DataField="AccountID" AutoCallBack="True" />
					<px:PXGridColumn DataField="AccountID_Account_description" />
					<px:PXGridColumn DataField="SubID" AutoCallBack="True"/>
					<px:PXGridColumn DataField="TranDate" DisplayFormat="d" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="TranDesc" />
                    <px:PXGridColumn DataField="ReferenceID"/>
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument"/>
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
    <px:PXSmartPanel ID="pnlLoadOpts" runat="server" Style="z-index: 108;" Caption="Load Transactions" CaptionVisible="True" Key="LoadOptionsView" AutoReload="true" LoadOnDemand="true">
		<px:PXFormView ID="loform" runat="server" Style="z-index: 100;" DataMember="LoadOptionsView" CaptionVisible="False" DefaultControlID="edBranch" SkinID="Transparent"  DataSourceID="ds">
			<ContentStyle BorderWidth="0px">
			</ContentStyle>
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXSelector  ID="edBranch" runat="server" DataField="BranchID" CommitChanges="True" />
                <px:PXSegmentMask ID="edFromAccountID" runat="server" DataField="FromAccountID" CommitChanges="True" />
                <px:PXSegmentMask ID="edToAccountID" runat="server" DataField="ToAccountID" CommitChanges="True" />
                <px:PXSegmentMask ID="edFromSubID" runat="server" DataField="FromSubID" CommitChanges="True"/>
                <px:PXSegmentMask ID="edToSubID" runat="server" DataField="ToSubID" CommitChanges="True"/>
				<px:PXSelector ID="edFromFinPeriodID" runat="server" DataField="FromFinPeriodID" CommitChanges="True"/>
                <px:PXSelector ID="edToFinPeriodID" runat="server" DataField="ToFinPeriodID" CommitChanges="True"/>
                <px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate" CommitChanges="True"/>
                <px:PXDateTimeEdit ID="edToDate" runat="server" DataField="ToDate" CommitChanges="True"/>
				
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXDropDown ID="edModule" runat="server" DataField="Module" CommitChanges="True"/>
			    <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" CommitChanges="True" AutoRefresh="True"/>
                <px:PXTextEdit ID="edRefNbr" runat="server" DataField="RefNbr"  CommitChanges="True"  />
                <px:PXSelector ID="edReferenceID" runat="server" DataField="ReferenceID" CommitChanges="True" />
                <px:PXNumberEdit ID="edMaxTrans" runat="server" DataField="MaxTrans" CommitChanges="True"/>

                <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="M" LabelsWidth="S" StartRow="True" />
                <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton5" runat="server" Text="Reload" CommandName="ReloadTrans" CommandSourceID="ds" SyncVisible="false" DialogResult="Cancel"/>
					<px:PXButton ID="PXButton3" runat="server" Text="Load" CommandName="LoadTrans" CommandSourceID="ds"  SyncVisible="false" DialogResult="Cancel" />
					<px:PXButton ID="PXButton4" runat="server" Text="Cancel" DialogResult="Cancel" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
     <px:PXSmartPanel ID="pnlRepalceOpt" runat="server" Style="z-index: 108;" Caption="Find and Replace" CaptionVisible="True" Key="ReplaceOptionsView" AutoReload="true" LoadOnDemand="true">
		<px:PXFormView ID="roform" runat="server" Style="z-index: 100;" DataMember="ReplaceOptionsView" CaptionVisible="False" DefaultControlID="edOldBranchID" SkinID="Transparent">
			<ContentStyle BorderWidth="0px">
			</ContentStyle>
			<Template>
                <px:PXTextEdit ID="edWarning" runat="server" DataField="Warning" ControlSize="M" SuppressLabel="true" TextMode="MultiLine" Height="40px" Style="font-weight:bold;color:red;border-style:none;" />
                <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" GroupCaption="FIND" StartGroup="True" />
                <px:PXSegmentMask ID="edWithBranchID" runat="server" DataField="WithBranchID" />
                <px:PXSegmentMask ID="edWithAccountID" runat="server" DataField="WithAccountID" CommitChanges="True" />
				<px:PXSegmentMask ID="edWithSubID" runat="server" DataField="WithSubID" CommitChanges="True"/>
                <px:PXDateTimeEdit ID="edWithDate" runat="server" DataField="WithDate" CommitChanges="True"/>
				<px:PXSelector ID="edWithFinPeriodID" runat="server" DataField="WithFinPeriodID" CommitChanges="True"/>
                <px:PXTextEdit ID="edWithTranDescFilteringValue" runat="server" DataField="WithTranDescFilteringValue" CommitChanges="True"/>  

                <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" GroupCaption="REPLACE WITH" StartGroup="True" />
				<px:PXSegmentMask ID="edNewBranchID" runat="server" DataField="NewBranchID" CommitChanges="True" />
                <px:PXSegmentMask ID="edNewAccountID" runat="server" DataField="NewAccountID" CommitChanges="True" />
				<px:PXSegmentMask ID="edNewSubID" runat="server" DataField="NewSubID" CommitChanges="True"/>
                <px:PXDateTimeEdit ID="edNewDate" runat="server" DataField="NewDate" CommitChanges="True"/>
                <px:PXTextEdit ID="edNewTranDesc" runat="server" DataField="NewTranDesc" CommitChanges="True"/>  

                <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" ControlSize="M" LabelsWidth="S" StartRow="True" />
                <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
					<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Replace" />
					<px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
</asp:Content>
