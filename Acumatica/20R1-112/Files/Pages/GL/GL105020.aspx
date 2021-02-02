<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL105020.aspx.cs" Inherits="Page_GL105020"
	Title="Budget Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLAccessByBudgetNode"
		PrimaryView="BudgetTree" PageLoadBehavior="GoFirstRecord" >
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveTree" />
			<px:PXDSCallbackCommand Name="FirstTree" StartNewGroup="True" HideText="True" />
			<px:PXDSCallbackCommand Name="PrevTree" HideText="True" />
			<px:PXDSCallbackCommand Name="NextTree" HideText="True"  />
			<px:PXDSCallbackCommand Name="LastTree" HideText="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" Caption="Budget Tree Node" DataMember="BudgetTree"
		DefaultControlID="edGroupID" TabIndex="100">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="M" />
			<px:PXSelector ID="edGroupID" runat="server" DataField="GroupID" DisplayMode="Text">
							<AutoCallBack Command="CancelTree" Target="ds">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" Enabled="False">
			</px:PXSegmentMask>
			<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" Enabled="False">
			</px:PXSegmentMask>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="M" />
			<px:PXCheckBox SuppressLabel="False" ID="chkIsGroup" runat="server" DataField="IsGroup"
				Enabled="False" />
			<px:PXMaskEdit ID="edAccountMask" runat="server" DataField="AccountMask" Enabled="False">
			</px:PXMaskEdit>
			<px:PXMaskEdit ID="edSubMask" runat="server" DataField="SubMask" Enabled="False">
			</px:PXMaskEdit>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" AllowPaging="True"
		AdjustPageSize="Auto" Caption="Restriction Groups" AllowSearch="True" TabIndex="200"
		SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="Groups">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
					<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
					<px:PXGridColumn DataField="GroupName" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="GroupType" Type="DropDownList" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM"
						ControlSize="M" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included" />
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" DataField="Active" />
					<px:PXDropDown ID="edGroupType" runat="server" DataField="GroupType" />
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" />
		<ActionBar CustomItemsGroup="1">
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
