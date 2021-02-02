<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM106000.aspx.cs" Inherits="Page_SM106000"
	Title="GL Budget Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLAccessPrinter"
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
	<px:PXFormView ID="formGroup" runat="server"  
		Width="100%" DataMember="Group" Caption="Restriction Group" DefaultControlID="edGroupName"
		TabIndex="100">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" >
				<AutoCallBack Command="Cancel" Target="ds">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXDropDown ID="edGroupType" runat="server"  DataField="GroupType" />
			<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="168%" Width="100%" TabIndex="200">
		
		<Items>
			<px:PXTabItem Text="Users">
				<Template>
					<px:PXGrid ID="gridUsers" BorderWidth="0px" runat="server"  Height="150px"
						Width="100%" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab"
						TabIndex="200" FastFilterFields="Username,FullName">
						<Levels>
							<px:PXGridLevel DataMember="Users">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector ID="edUsername" runat="server" DataField="Username" TextField="Username" />
									<px:PXTextEdit ID="edComment" runat="server" DataField="Comment" />
									<px:PXCheckBox ID="chkIncluded" runat="server" DataField="Included" />
									<px:PXTextEdit ID="FullName" runat="server" DataField="FullName">
									</px:PXTextEdit>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="40px"
										AllowCheckAll="True" />
									<px:PXGridColumn DataField="Username" Width="300px" />
									<px:PXGridColumn DataField="FullName" Width="200px" />
									<px:PXGridColumn DataField="Comment" Width="300px">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Printers">
				<Template>
					<px:PXGrid ID="gridBudgets" BorderWidth="0px" runat="server"  Height="150px"
						 Width="100%" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True"
						 SkinID="DetailsInTab" TabIndex="300"
						FastFilterFields="PrinterName">
						<Levels>
							<px:PXGridLevel DataMember="Printers">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXCheckBox ID="chkArticleIncluded" runat="server" DataField="Included"/>
                                    <px:PXSelector ID="edPrinterName" runat="server" DataField="PrinterName" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn  DataField="Included" TextAlign="Center" AllowCheckAll="True"
										Type="CheckBox" Width="40px" />
									<px:PXGridColumn DataField="PrinterName" Width="200px" />
                                    <px:PXGridColumn DataField="Description" Width="300px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" AllowAddNew="True" AllowUpdate="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridBudgets" Name="GroupID" PropertyName="DataValues[&quot;GroupID&quot;]"
								Type="String" />
						</EditPageParams>
						<ActionBar>
							<Actions>
								<Delete Enabled="False" />
                                <EditRecord Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
