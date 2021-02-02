<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL201100.aspx.cs" Inherits="Page_GL201100"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		TypeName="PX.Objects.GL.OrganizationFinPeriodMaint" PrimaryView="OrgFinYear" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" Visible="false" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
			<px:PXDSCallbackCommand Name="Insert" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%"
		DataMember="OrgFinYear" NoteIndicator="True" FilesIndicator="True"
		ActivityIndicator="True" ActivityField="NoteActivity">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXSegmentMask ID="edOrganizationID" runat="server" DataField="OrganizationID"/>
			<px:PXSelector ID="edYear" runat="server" DataField="Year" AutoRefresh = "true"/>
			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" Enabled="False" />
			<px:PXNumberEdit ID="edFinPeriods" runat="server" DataField="FinPeriods" Enabled="False" />
		</Template>
	</px:PXFormView>

</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px"
		Width="100%" Caption="Financial Year Periods" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="OrgFinPeriods">
				<Columns>
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="StartDateUI" />
					<px:PXGridColumn DataField="EndDateUI" AutoCallBack="true" />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn DataField="Status" />
					<px:PXGridColumn DataField="APClosed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="ARClosed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="INClosed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="CAClosed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="FAClosed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="MasterFinPeriodID" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
	<px:PXSmartPanel ID="spNewCalendar" runat="server" Key="NewCalendarParams" AutoCallBack-Command="Refresh"
		AutoCallBack-Target="NewCalendarParams" AutoCallBack-Enabled="true" LoadOnDemand="True"
		Caption="Create First Year of Company Calendar"
		CaptionVisible="True"
		AcceptButtonID="cbOk" CancelButtonID="cbCancel">
		<px:PXFormView ID="NewCalendarParams" runat="server" DataSourceID="ds" Style="z-index: 108"
			Width="100%" DataMember="NewCalendarParams" CaptionVisible="false" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule ID="spLayout" runat="server" StartRow="True" LabelsWidth="SM" ControlSize="L" SuppressLabel="false" />
				<px:PXMaskEdit  ID="edOrganizationID" runat="server" DataField="OrganizationID"/>
				<px:PXSelector ID="edStartYear" runat="server" DataField="StartYear" CommitChanges="true" AutoRefresh="true"/>
				<px:PXSelector ID="edStartMasterFinPeriodID" runat="server" DataField="StartMasterFinPeriodID" CommitChanges="true" AutoRefresh="true"/>
			    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate"/>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons" Style="float: right">
			<px:PXButton ID="cbOk" runat="server" DialogResult="Ok" Text="OK" CommandSourceID="ds"/>
			<px:PXButton ID="cbCancel" runat="server" DialogResult="No" Text="Cancel"/>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="spGenParams" runat="server" Key="GenerateParams" AutoCallBack-Command="Refresh"
		AutoCallBack-Target="GenParams" AutoCallBack-Enabled="true" LoadOnDemand="True"
		Caption="Generate GL Calendar"
		CaptionVisible="True" DesignView="Content" Width="500px"
		AcceptButtonID="cbOk2" CancelButtonID="cbCancel2">
		<px:PXFormView ID="GenParams" runat="server" DataSourceID="ds" Style="z-index: 108"
			Width="100%" DataMember="GenerateParams" CaptionVisible="false" SkinID="Transparent">
			<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
			<Template>
				<px:PXLayoutRule ID="spLayout" runat="server" StartRow="True" LabelsWidth="S" ControlSize="L" />
				<px:PXMaskEdit  ID="edOrganizationID" runat="server" DataField="OrganizationID"/>
				<px:PXLayoutRule ID="spLayout3" runat="server" StartRow="True" LabelsWidth="S" ControlSize="S" SuppressLabel="false" />
				<px:PXTextEdit ID="edFirstYear" runat="server" DataField="FirstFinYear"/>
				<px:PXTextEdit ID="edLastYear" runat="server" DataField="LastFinYear" />
				<px:PXLayoutRule ID="spLayout4" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
				<px:PXTextEdit ID="edFromYear" runat="server" DataField="FromYear" CommitChanges="true" />
				<px:PXTextEdit ID="edToYear" runat="server" DataField="ToYear" CommitChanges="true" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons" Style="float: right">
			<px:PXButton ID="cbOk2" runat="server" DialogResult="Ok" Text="OK" CommitChanges="True"/>
			<px:PXButton ID="cbCancel2" runat="server" DialogResult="Cancel" Text="Cancel"/>
		</px:PXPanel>
	</px:PXSmartPanel>

</asp:Content>
