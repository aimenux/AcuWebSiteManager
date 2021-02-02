<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP405000.aspx.cs" Inherits="Page_EP405000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server" Visible="false">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Width="100%" Height="0px"
		PrimaryView="Filter" TypeName="PX.Objects.EP.ActivitiesMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True"
				PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="ViewActivity" Visible="False" CommitChanges="True"
				DependOnGrid="gridActivities" />
		</CallbackCommands>
	</px:PXDataSource>
	<px:PXFormView ID="edFilter" runat="server" DataSourceID="ds" DataMember="Filter" CaptionVisible="false" Height="0px" />
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="server">
	<pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" Height="420px" 
		AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
		FilesField="NoteFiles" BorderWidth="1px" BorderColor="Gray" GridSkinID="Details" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray"
		PreviewPanelStyle="z-index: 100; background-color: Window" PreviewPanelSkinID="Preview"
		BlankFilterHeader="All Activities" MatrixMode="true" GridStyle="border: 0px" PrimaryViewControlID="edFilter" >
		<ActionBar DefaultAction="cmdViewActivity">
			<Actions>
				<AddNew Enabled="False" />
			</Actions>
			<CustomItems>
				<px:PXToolBarButton Key="cmdAddTask">
				    <AutoCallBack Command="NewTask" Enabled="True" Target="ds" />
				    <PopupCommand Command="Cancel" Enabled="true" Target="ds" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdAddEvent">
				    <AutoCallBack Command="NewEvent" Enabled="True" Target="ds" />
				    <PopupCommand Command="Cancel" Enabled="true" Target="ds" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdAddEmail">
				    <AutoCallBack Command="NewMailActivity" Enabled="True" Target="ds" />
				    <PopupCommand Command="Cancel" Enabled="true" Target="ds" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdAddActivity">
				    <AutoCallBack Command="NewActivity" Enabled="True" Target="ds" />
				    <PopupCommand Command="Cancel" Enabled="true" Target="ds" />
					<ActionBar MenuVisible="true" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdViewActivity" Visible="false">
					<ActionBar MenuVisible="false" />
					<AutoCallBack Command="ViewActivity" Enabled="True" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Levels>
			<px:PXGridLevel  DataMember="Activities">
				<Columns>
					<px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" AllowResize="False" />
					<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" />
					<px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" AllowResize="False" />
					<px:PXGridColumn DataField="Type" >
						<Header Text="Type">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
					<px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
					<px:PXGridColumn AllowNull="False" DataField="Status" DataType="Int32" DefValueText="1">
						<Header Text="Status">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="StartDate" DataType="DateTime" DisplayFormat="g">
						<Header Text="Start Time">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="TimeSpent" DataType="DateTime">
						<Header Text="Time Spent">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID" Visible="false" AllowShowHide="False" />
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username"
						Visible="false" SyncVisible="False" AllowShowHide="True" SyncVisibility="False"
						Width="108px">
						<Header Text="Created By">
						</Header>
						<NavigateParams>
							<px:PXControlParam Name="PKID" ControlID="gridActivities" PropertyName="DataValues[&quot;CreatedByID&quot;]" />
						</NavigateParams>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="WorkgroupID">
						<Header Text="Workgroup">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" />
                    <px:PXGridColumn DataField="Released" />
				</Columns>
				<RowTemplate>
					<px:PXSelector ID="edActivityAssignedTo" runat="server" DataField="ActivityOwner__fullname"
						  AllowEdit="true">
					</px:PXSelector>
					<px:PXSelector ID="edActivityOwnerID" runat="server" DataField="CreatedByID" 
						 AllowEdit="true">
					</px:PXSelector>
					<px:PXSelector ID="edActivitySubject" runat="server"  DataField="Subject"
						 AllowEdit="true" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Enabled="True" Container="Window" />
		<GridMode AllowAddNew="False" AllowUpdate="False" />
		<PreviewPanelTemplate>
			<px:PXHtmlView ID="edBody" runat="server" DataField="previewHtml" TextMode="MultiLine" MaxLength="50"
				Width="100%" Height="100px" SkinID="Label">
                                      <AutoSize Container="Parent" Enabled="true" />
                                </px:PXHtmlView>
			</px:PXHtmlView> 
		</PreviewPanelTemplate>
	</pxa:PXGridWithPreview>
</asp:Content>
