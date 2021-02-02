<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR102000.aspx.cs" Inherits="Page_AR102000"
	Title="Customer Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARAccess"
		PrimaryView="Group">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Delete" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formGroup" runat="server" Width="100%" DataMember="Group" Caption="Restriction Group"
		DefaultControlID="edGroupName" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName">
			</px:PXSelector>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description">
			</px:PXTextEdit>
			<px:PXDropDown ID="edGroupType" runat="server" DataField="GroupType">
			</px:PXDropDown>
			<px:PXCheckBox ID="chkActive" runat="server" DataField="Active">
			</px:PXCheckBox>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="168%" Width="100%" SelectedIndex="1">
		<Items>
			<px:PXTabItem Text="Users">
				<Template>
					<px:PXGrid ID="gridUsers" BorderWidth="0px" runat="server" Height="150px" Width="100%"
						AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab" DataSourceID="ds" 
						FastFilterFields="FullName,Username">
						<Levels>
							<px:PXGridLevel DataMember="Users">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"  />
									<px:PXCheckBox ID="chkIncluded" runat="server" DataField="Included">
									</px:PXCheckBox>
									<px:PXSelector ID="edUsername" runat="server" DataField="Username" TextField="Username">
									</px:PXSelector>
									<px:PXTextEdit ID="FullName" runat="server" DataField="FullName">
									</px:PXTextEdit>
									<px:PXTextEdit ID="edComment" runat="server" DataField="Comment">
									</px:PXTextEdit>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="30px"
										AllowCheckAll="True" >
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Username" Width="300px">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="FullName" Width="200px">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Comment" Width="300px">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridUsers" Name="Username" PropertyName="DataValues[&quot;Username&quot;]"
								Type="String" />
						</EditPageParams>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Customers">
				<Template>
					<px:PXGrid ID="gridCustomers" BorderWidth="0px" runat="server" Height="150px" Width="100%"
						AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab" TabIndex="400" 
						DataSourceID="ds" FastFilterFields="AcctCD,AcctName">
						<Levels>
							<px:PXGridLevel DataMember="Customer">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="30px"
										AllowCheckAll="True" >
									</px:PXGridColumn>
									<px:PXGridColumn DataField="AcctCD" Width="100px" >
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Status" Width="70px" >
									</px:PXGridColumn>
									<px:PXGridColumn DataField="AcctName" Width="351px">
									</px:PXGridColumn>
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXCheckBox ID="chkCustomerIncluded" runat="server" DataField="Included">
									</px:PXCheckBox>
									<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" AllowEdit="True">
									</px:PXSegmentMask>
									<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False">
									</px:PXDropDown>
									<px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName" Enabled="False">
									</px:PXTextEdit>
								</RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridCustomers" Name="AcctCD" PropertyName="DataValues[&quot;AcctCD&quot;]"
								Type="String" />
						</EditPageParams>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
