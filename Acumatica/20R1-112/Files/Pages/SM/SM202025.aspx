<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM202025.aspx.cs" Inherits="Page_SM204026"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" PrimaryView="fltStatusRecords"
		TypeName="PX.SM.WikiStatusMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="view" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="Process" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ProcessAll" Visible="False" />
			<px:PXDSCallbackCommand Name="Schedule" StartNewGroup="True" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Folders" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlUpdatePages" runat="server" Height="350px" Style="z-index: 108;
		left: 351px; position: absolute; top: 99px" Width="400px" CaptionVisible="true"
		Caption="Update Article Properties" LoadOnDemand="true" Key="fltArticlesProps" 
		AutoReload="true">
		<div>
			<px:PXFormView ID="frmUpdatePages" runat="server" DataSourceID="ds" Style="z-index: 100"
				Width="100%" DataMember="fltArticlesProps" SkinID="Transparent" TemplateContainer="">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
						ControlSize="M" />
					<px:PXTreeSelector CommitChanges="True" SuppressLabel="False" ID="edParent"
						runat="server" DataField="ParentUID" PopulateOnDemand="True" ShowRootNode="False"
						TreeDataSourceID="ds" TreeDataMember="Folders" AutoRefresh="True">
						<Images>
							<LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
						</Images>
						<DataBindings>
							<px:PXTreeItemBinding TextField="Title" ValueField="PageID" />
						</DataBindings>
					</px:PXTreeSelector>
					<px:PXSelector CommitChanges="True" ID="edTagID" runat="server" DataField="TagID"
						AutoRefresh="True" DataSourceID="ds" />
					<px:PXTextEdit CommitChanges="True" ID="edKeywords" runat="server" DataField="Keywords"
						Height="62px" TextMode="MultiLine" />
					<px:PXLabel ID="PXHole1" runat="server"></px:PXLabel>
					<px:PXLabel ID="PXHolel2" runat="server"></px:PXLabel>
					<px:PXDropDown CommitChanges="True" ID="edHold" runat="server" AllowNull="False"
						DataField="Hold" Size="M" />
					<px:PXDropDown CommitChanges="True" ID="edVersioned" runat="server" 
						DataField="Versioned" Size="M" />
				</Template>
			</px:PXFormView>
		</div>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="Apply" />
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="fltStatusRecords" Caption="Selection" CheckChanges="False" 
		TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" 
				StartColumn="True" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector CommitChanges="True" Size="M" ID="edOwnerID" runat="server" DataField="OwnerID"
				TextField="AcctName" DataSourceID="ds" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyOwner" runat="server"
				Checked="True" DataField="MyOwner" AlignLeft="True" Size="XS" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector CommitChanges="True" Size="M" ID="edWorkGroupID" runat="server" 
				DataField="WorkGroupID" DataSourceID="ds" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server"
				DataField="MyWorkGroup" AlignLeft="True" Size="XS" />
			<px:PXLayoutRule runat="server" />
			<px:PXCheckBox CommitChanges="True" ID="chkMyEscalated" runat="server" DataField="MyEscalated" />
			<px:PXSelector CommitChanges="True" ID="edUserID" runat="server" DataField="UserID"
				TextField="Username" DataSourceID="ds" />
			<px:PXDropDown CommitChanges="True" ID="edStatusID" runat="server" DataField="StatusID" />
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" 
				StartColumn="True" />
			<px:PXSelector CommitChanges="True" ID="edWikiID" runat="server" DataField="WikiID"
				TextField="Name" DataSourceID="ds" />
			<px:PXTreeSelector CommitChanges="True" ID="edFolder" runat="server" AutoRefresh="True"
				DataField="FolderID" PopulateOnDemand="True" ShowRootNode="False" TreeDataSourceID="ds"
				TreeDataMember="Folders">
				<Images>
					<LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
				</Images>
				<DataBindings>
					<px:PXTreeItemBinding TextField="Title" ValueField="PageID" />
				</DataBindings>
			</px:PXTreeSelector>
			<px:PXDateTimeEdit CommitChanges="True" ID="edCreatedFrom" runat="server" DataField="CreatedFrom"
				DisplayFormat="g" Size="M" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edCreatedTill" runat="server" DataField="CreatedTill"
				DisplayFormat="g" Size="M" />
			
		</Template>
		<Parameters>
			<px:PXQueryStringParam Name="WikiID" OnLoadOnly="True" QueryStringField="wikiID"
				Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" AutoAdjustColumns="True"
		CheckChanges="False" Caption="Articles" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="ArticlesByStatusRecords">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit ID="edName" runat="server" DataField="Name" />
					<px:PXTextEdit ID="edTitle" runat="server" DataField="Title" />
					<px:PXTextEdit ID="edSummary" runat="server" DataField="Summary" />
					<px:PXTextEdit ID="edKeywords" runat="server" DataField="Keywords" />
					<px:PXTextEdit ID="edPath" runat="server" DataField="Path" />
                </RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox"
						Width="25px" AllowCheckAll="True" AllowShowHide="False" />
					<px:PXGridColumn DataField="Name" Width="108px" AllowUpdate="False">
					    <Style ForeColor="Black" />
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Title" Width="108px" AllowUpdate="False">
						<Style ForeColor="Black" />
					</px:PXGridColumn>
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_Username"
						Width="80px" />
					<px:PXGridColumn AllowUpdate="False" DataField="StatusID" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Versioned" TextAlign="Center"
						Type="CheckBox" Width="50px" />
					<px:PXGridColumn DataField="Path" Width="200px" AllowUpdate="False">
					    <Style ForeColor="Black" />
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton AutoPostBack="False" Key="view" Text="View" CommandName="view" CommandSourceID="ds" />
			    <px:PXToolBarButton Text="Update" CommandSourceID="ds" CommandName="Process" />
			    <px:PXToolBarButton CommandName="ProcessAll" CommandSourceID="ds" Text="Update All" />
			</CustomItems>
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
				<NoteShow Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
