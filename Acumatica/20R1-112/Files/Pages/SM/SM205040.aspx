<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM205040.aspx.cs" Inherits="Page_SM205040"
	Title="Automation Notification Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUNotificationMaint"
		PrimaryView="Notification">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Refresh" Visible="false" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="true"/>
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Prev" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Next" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="ViewScreen" StartNewGroup="true" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="Graphs" TreeKeys="GraphName,IsNamespace" />
            <px:PXTreeDataMember TreeKeys="Key" TreeView="EntityItems" />
		</DataTrees>
	</pxa:AUDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<script language="javascript">
		function tab_focusDefaultControl(sender, args)
		{
			var focusBody = px_all["ctl00_phF_formNotification_edSubject"].getValue() != null;
			args.target.defaultFocusCtrl = focusBody ? "ctl00_phG_tab_t0_wikiEdit" : "ctl00_phF_formNotification_edSubject";
		}
	</script>
	<px:PXFormView ID="formNotification" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Caption="Change Notification" DataMember="Notification" NoteIndicator="True"
		FilesIndicator="True" EmailingGraph="" LinkPage="" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="SM" />
			<px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" />
			<px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" AutoRefresh="True"
				TextField="Description" NullText="<NEW>" DataSourceID="ds">
				<AutoCallBack Command="Refresh" Target="ds">
				</AutoCallBack>
				<Parameters>
					<px:PXControlParam Name="AUNotification.screenID" ControlID="formNotification" PropertyName="DataControls[&quot;edScreenID&quot;].Value"
						Type="String" Size="8" />
				</Parameters>
			</px:PXSelector>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
			<px:PXCheckBox ID="chkIsPublic" runat="server" DataField="IsPublic" />			
			<px:PXTreeSelector ID="edSubject" runat="server" DataField="Subject" TreeDataSourceID="ds"
				TreeDataMember="EntityItems" PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="false"
				MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true" AppendSelectedValue="true"	AutoRefresh="true">
				<DataBindings>
					<px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
				</DataBindings>
			</px:PXTreeSelector>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="SM" />
			<px:PXDropDown CommitChanges="True" ID="edCreationMethod" runat="server" AllowNull="False"
				DataField="CreationMethod" Size="M" />
            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID"  DisplayMode="Text" FilterByAllFields="true" />
			<px:PXDropDown CommitChanges="True" ID="edReportFormat" runat="server" AllowNull="False"
				DataField="ReportFormat" Size="M" />
			<px:PXCheckBox CommitChanges="True" ID="chkIsEmbeded" runat="server" DataField="IsEmbeded" />
			<px:PXDropDown CommitChanges="True" ID="edActionName" runat="server" 
				DataField="ActionName" Size="M" />
			<px:PXDropDown ID="edMenuText" runat="server" DataField="MenuText" Size="M" />
		</Template>
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Parameters>
			<px:PXControlParam ControlID="formNotification" Name="AUNotification.screenID" PropertyName="NewDataKey[&quot;ScreenID&quot;]"
				Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100" Width="100%"
		DataSourceID="ds" DataMember="CurrentNotification">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Message" BindingContext="formNotification" VisibleExp="DataControls[&quot;chkIsEmbeded&quot;].Value == False">
				<Template>
					<px:PXRichTextEdit ID="wikiEdit" runat="server" Style="z-index: 113; border-width: 0px;"
						DataField="Body" Width="100%" FilesContainer="formNotification" AllowImageEditor="true"
						AllowLinkEditor="true" OnBeforePreview="wikiEdit_BeforePreview" AllowSourceMode="true"
						AllowAttached="true" AllowSearch="true" AllowLoadTemplate="false" AllowInsertParameter="true">
						<ContentStyle BorderStyle="None" />
						<AutoSize Enabled="True" />
                        <InsertDatafield DataSourceID="ds" DataMember="EntityItems" TextField="Name" ValueField="Path"
							ImageField="Icon" />
                        <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
					</px:PXRichTextEdit>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Conditions">
				<Template>
					<px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" MatrixMode="true">
						<ActionBar>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Filters">
								<Mode InitNewRow="True" />
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px"
										AutoCallBack="true" />
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" />
									<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="Value" Width="200px"/>
									<px:PXGridColumn DataField="Value2" Width="200px"/>
									<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="60px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Addresses" BindingContext="tab">
				<Template>
					<px:PXGrid ID="gridAddresses" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" SyncPosition="true"
						AutoAdjustColumns="true">
						<AutoSize Enabled="True" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataKeyNames="ScreenID,NotificationID,RowNbr" DataMember="Addresses">
								<Mode InitNewRow="true" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXLayoutRule runat="server" Merge="True" />
									<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
									<px:PXDropDown Size="s" ID="edAddressSource" runat="server" AllowNull="False" DataField="AddressSource"
										SelectedIndex="-1" />
									<px:PXDropDown Size="s" ID="edAddressType" runat="server" AllowNull="False" DataField="AddressType"
										SelectedIndex="-1" />
									<px:PXLayoutRule runat="server" Merge="False" />
									<px:PXSelector ID="edEmail" runat="server" DataField="Email"
										TextField="Value" AutoRefresh="true" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" Label="Active" TextAlign="Center"
										Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="AddressSource" Label="Source" RenderEditorText="True"
										AutoCallBack="true" />
									<px:PXGridColumn DataField="Email" Label="Email" Width="308px" TextField="Email_description" />
									<px:PXGridColumn AllowNull="False" DataField="AddressType" Label="Type" RenderEditorText="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Parameters" BindingContext="formNotification" VisibleExp="DataControls[&quot;edReportID&quot;].Value IsNotNull">
				<Template>
					<px:PXGrid ID="gridParameters" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" MatrixMode="true">
						<AutoSize Enabled="True" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataMember="Parameters">
								<Mode InitNewRow="true" />
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="ParameterName" Width="200px" Type="DropDownList" Label="Parameter Name" />
									<px:PXGridColumn DataField="Value" Width="200px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Fields" BindingContext="tab">
				<Template>
					<px:PXGrid ID="gridFills" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" MatrixMode="true">
						<AutoSize Enabled="True" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataMember="Fields">
								<Mode InitNewRow="true" />
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
