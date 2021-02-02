<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR503220.aspx.cs" Inherits="Page_CR503220"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" AutoCallBack="True" Visible="True"
		IsDefaultDatasourceWidth="100%" TypeName="PX.Objects.CR.UpdateCaseMassProcess" PrimaryView="Filter"
		PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccount_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccountParent_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Contact_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Location_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Contract_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Contract_CustomerID_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" Name="wizardNext" CommitChanges="True" CommitChangesIDs="grdFields,grdAttrs" RepaintControls="None" RepaintControlsIDs="grdFields,grdAttrs,wizard"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Selection" DataMember="Filter">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXDropDown ID="edOperation" runat="server" DataField="Operation" CommitChanges="True" />
			<px:PXDropDown ID="edAction" runat="server" DataField="Action" CommitChanges="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Width="100%" Height="150px"
		SkinID="PrimaryInquire" AdjustPageSize="Auto" AllowPaging="True" Caption="Matching Records" AutoGenerateColumns="AppendDynamic" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="CaseCD" DisplayFormat="&gt;aaaaaaaaaa" LinkCommand="Items_ViewDetails"/>
					<px:PXGridColumn DataField="Subject" />
					<px:PXGridColumn AllowNull="False" DataField="Status" />
					<px:PXGridColumn AllowNull="False" DataField="Resolution" />
					<px:PXGridColumn AllowNull="False" DataField="Severity" />
					<px:PXGridColumn AllowNull="False" DataField="Priority" />
					<px:PXGridColumn DataField="ETA" />
					<px:PXGridColumn DataField="TimeEstimated" />
					<px:PXGridColumn DataField="RemaininingDate" />
                    <px:PXGridColumn DataField="Age" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="LastActivity" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="LastActivityAge" />
                    <px:PXGridColumn DataField="LastModified" DisplayFormat="g" TimeMode="True" />
					<px:PXGridColumn DataField="CaseClassID" DisplayFormat="&gt;aaaaaaaaaa" />
					<px:PXGridColumn DataField="BAccount__AcctCD" LinkCommand="Items_BAccount_ViewDetails"/>
					<px:PXGridColumn DataField="BAccount__AcctName" />  
					<px:PXGridColumn DataField="BAccountParent__AcctCD" RenderEditorText="True" LinkCommand="Items_BAccountParent_ViewDetails"/>
					<px:PXGridColumn DataField="BAccountParent__AcctName" />
					<px:PXGridColumn DataField="Contact__DisplayName" LinkCommand="Items_Contact_ViewDetails"/>                    
					<px:PXGridColumn DataField="Location__LocationCD" LinkCommand="Items_Location_ViewDetails"/>	
					<px:PXGridColumn DataField="Contract__ContractCD" LinkCommand="Items_Contract_ViewDetails"/> 
					<px:PXGridColumn DataField="Contract__Description" /> 
                    <px:PXGridColumn DataField="Contract__CustomerID" LinkCommand="Items_Contract_CustomerID_ViewDetails"/>
                    <px:PXGridColumn DataField="BAccountContract__AcctName" />
					<px:PXGridColumn DataField="InitResponse" />
					<px:PXGridColumn DataField="TimeResolution" />
					<px:PXGridColumn DataField="TimeSpent" />
					<px:PXGridColumn DataField="OvertimeSpent" />
					<px:PXGridColumn DataField="TimeBillable" />
					<px:PXGridColumn DataField="OvertimeBillable" />
					<px:PXGridColumn DataField="WorkgroupID" />
					<px:PXGridColumn DataField="OwnerID" DisplayMode="Text"/>
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" />
					<px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" TimeMode="True"/> 
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" SyncVisible="False" SyncVisibility="False" Visible="False"/>	  
					<px:PXGridColumn DataField="LastModifiedDateTime" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
	<px:PXSmartPanel ID="spUpdateParamsDlg" runat="server" Width="500px" Height="350px" Caption="Mass Update"
		CaptionVisible="True" Key="Fields" LoadOnDemand="True" ShowAfterLoad="true" AutoReload="True" AllowResize="False">
		<px:PXWizard ID="wizard" runat="server" Width="100%" Height="300px" DataMember="wizardSummary" 
			SkinID="Flat" OnSelectNextPage="wizard_OnSelectNextPage" AutoSize="True" CaptionVisible="True">
			<NextCommand Target="ds" Command="wizardNext" />
			<AutoSize Enabled="true" />
			<Pages>
				<px:PXWizardPage Caption="Please select the fields you want to update">
					<Template>
						<px:PXGrid ID="grdFields" runat="server" Height="150px" Width="100%" DataSourceID="ds" AutoAdjustColumns="true" MatrixMode="True">
							<CallbackCommands>
								<Save CommitChanges="true" CommitChangesIDs="grdFields" RepaintControls="None" RepaintControlsIDs="grdFields" />
								<FetchRow RepaintControls="None" />
							</CallbackCommands>
							<AutoSize Enabled="true" />
							<Levels>
								<px:PXGridLevel DataMember="Fields">
									<Columns>
										<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowSort="false" AllowCheckAll="true" TextAlign="Center" />
										<px:PXGridColumn DataField="DisplayName" />
										<px:PXGridColumn DataField="Value" RenderEditorText="True" AutoCallBack="True" />
									</Columns>
									<Layout ColumnsMenu="False" />
									<Mode AllowAddNew="false" AllowDelete="false" />
								</px:PXGridLevel>
							</Levels>
							<ActionBar ActionsHidden="true" />
						</px:PXGrid>
					</Template>
				</px:PXWizardPage>
				<px:PXWizardPage Caption="Please select the attributes you want to update">
					<Template>
						<px:PXGrid ID="grdAttrs" runat="server" Height="150px" Width="100%" DataSourceID="ds"
							AutoAdjustColumns="true" MatrixMode="True">
							<CallbackCommands>
								<Save CommitChanges="true" CommitChangesIDs="grdAttrs" RepaintControls="None" RepaintControlsIDs="grdAttrs" />
								<FetchRow RepaintControls="None" />
							</CallbackCommands>
							<AutoSize Enabled="true"/>
							<Levels>
								<px:PXGridLevel DataMember="Attributes">
									<Columns>
										<px:PXGridColumn DataField="Selected" Width="30px" Type="CheckBox" AllowSort="false"
											AllowCheckAll="true" TextAlign="Center" />
										<px:PXGridColumn DataField="DisplayName" />
										<px:PXGridColumn DataField="Value" RenderEditorText="False" AutoCallBack="True" />
										<px:PXGridColumn DataField="Required" Type="CheckBox" AllowSort="false" />
									</Columns>
									<Layout ColumnsMenu="False" />
									<Mode AllowAddNew="false" AllowDelete="false" />
								</px:PXGridLevel>
							</Levels>
							<ActionBar ActionsHidden="true" />
						</px:PXGrid>
					</Template>
				</px:PXWizardPage>
				<px:PXWizardPage Caption="Please review your action and press finish button to apply the changes to selected records:">
					<Template>
						<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True"/>
						<px:PXTextEdit runat="server" ID="edSpace" Height="1px" Width="1px" SkinID="Label" DataField="Summary" SuppressLabel="true" />
					    <px:PXTextEdit runat="server" ID="edSummary" DataField="Summary" Width="440px" Height="225px" TextMode="MultiLine" SuppressLabel="true" SkinID="Label" Style="padding-left: 10px; padding-right: 10px;" />
					</Template>
				</px:PXWizardPage>
			</Pages>
		</px:PXWizard>
	</px:PXSmartPanel>
</asp:Content>
