<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL103040.aspx.cs" Inherits="Page_GL103040"
	Title="GL Branch Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLBranchAccess"
		PrimaryView="Group">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Delete" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formGroup" runat="server" Width="100%" DataMember="Group" Caption="Restriction Group"
		DefaultControlID="edGroupName" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='SM' ControlSize="M" />
			<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" OnValueChange="Refresh">
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
	<px:PXTab ID="tab" runat="server" Height="300px" Width="100%" DataMember="SegmentFilter">
		<Items>
			<px:PXTabItem Text="Branches">
				<Template>
					<px:PXGrid ID="gridBranchs" runat="server" Height="300px" Width="100%" AdjustPageSize="Auto"
						AllowSearch="True" SkinID="DetailsInTab" FastFilterFields="BranchCD,AcctName"
						TabIndex="300">
						<Levels>
							<px:PXGridLevel DataMember="Branch">
								<RowTemplate>
									<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='SM' ControlSize="M" />
									<px:PXSegmentMask ID="edBranchCD" runat="server" DataField="BranchCD">
									</px:PXSegmentMask>
									<px:PXSelector ID="edRoleName" runat="server" DataField="RoleName">
									</px:PXSelector>
									<px:PXSelector ID="edLedgerID" runat="server" DataField="LedgerID">
									</px:PXSelector>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="BranchCD">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="AcctName">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="LedgerID">
									</px:PXGridColumn>
								</Columns>
								<Mode AllowAddNew="True" AllowDelete="False" />
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
						</ActionBar>
						<Mode AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Accounts">
				<Template>
					<px:PXGrid ID="gridAccounts" runat="server" Height="300px" Width="100%" AllowSearch="True"
						EditPageUrl="~/Pages/GL/GL104020.aspx" SkinID="DetailsInTab" FastFilterFields="AccountCD,Description"
						TabIndex="400" AdjustPageSize="Auto">
						<Levels>
							<px:PXGridLevel DataMember="Account">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='SM' ControlSize="M" />
									<px:PXSegmentMask ID="edAccountCD" runat="server" DataField="AccountCD">
									</px:PXSegmentMask>
									<px:PXDropDown ID="edType" runat="server" DataField="Type">
									</px:PXDropDown>
									<px:PXSelector ID="edAccountClassID" runat="server" DataField="AccountClassID">
									</px:PXSelector>
									<px:PXCheckBox ID="chkAccountIncluded" runat="server" DataField="Included">
									</px:PXCheckBox>
									<px:PXCheckBox ID="chkActive" runat="server" DataField="Active">
									</px:PXCheckBox>
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description">
									</px:PXTextEdit>
									<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID">
									</px:PXSelector>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="AccountCD">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Type" Type="DropDownList">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="AccountClassID">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Active" Type="CheckBox" TextAlign="Center">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Description">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="CuryID">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridAccounts" Name="AccountCD" PropertyName="DataValues[&quot;AccountCD&quot;]"
								Type="String" />
						</EditPageParams>
						<ActionBar>
							<Actions>
								<Delete Enabled="False" />
								<EditRecord Enabled="False" Text="Membership" ImageUrl="main@Settings" GroupIndex="2" Order="0" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
