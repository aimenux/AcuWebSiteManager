<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL101010.aspx.cs" Inherits="Page_GL101010"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLBranchAcctMapMaint"
		PrimaryView="Branches">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="Branches" Caption="Branch Summary"
		DefaultControlID="edBranchCD" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='SM' ControlSize="M" />
			<px:PXSegmentMask runat="server" DataField="BranchCD" ID="edBranchCD" OnValueChange="Refresh">
			</px:PXSegmentMask>
			<px:PXSelector runat="server" DataField="LedgerID" ID="edLedgerID" OnValueChange="Commit">
			</px:PXSelector>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Width="100%" TabIndex="200">
		<Items>
			<px:PXTabItem Text="Transaction in Originating Branch">
				<Template>
					<px:PXGrid ID="gridFrom" runat="server" Height="300px" Width="100%" SkinID="DetailsInTab"
						TabIndex="300">
						<Mode AllowFormEdit="True" />
						<Levels>
							<px:PXGridLevel DataMember="MapFrom">
								<RowTemplate>
									<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='SM' ControlSize="M" />
									<px:PXSelector ID="edToBranchID" runat="server" DataField="ToBranchID" AutoRefresh="True">
									</px:PXSelector>
									<px:PXSegmentMask ID="edFromAccountCD" runat="server" DataField="FromAccountCD">
									</px:PXSegmentMask>
									<px:PXSegmentMask ID="edToAccountCD" runat="server" DataField="ToAccountCD">
									</px:PXSegmentMask>
									<px:PXSegmentMask ID="edMapAccountID" runat="server" DataField="MapAccountID" OnValueChange="Commit">
									</px:PXSegmentMask>
									<px:PXSegmentMask ID="edMapSubID" runat="server" DataField="MapSubID" AutoRefresh="True">
									</px:PXSegmentMask>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ToBranchID">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="FromAccountCD">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="ToAccountCD">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="MapAccountID" CommitChanges="true">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="MapSubID">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Transaction in Destination Branch">
				<Template>
					<px:PXGrid ID="gridTo" runat="server" Height="300px" Width="100%" SkinID="DetailsInTab"
						TabIndex="400">
						<Mode AllowFormEdit="True" />
						<Levels>
							<px:PXGridLevel DataMember="MapTo">
								<RowTemplate>
									<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth='SM' ControlSize="M" />
									<px:PXSelector ID="edFromBranchID1" runat="server" DataField="FromBranchID" AutoRefresh="True">
									</px:PXSelector>
									<px:PXSegmentMask ID="edFromAccountCD1" runat="server" DataField="FromAccountCD">
									</px:PXSegmentMask>
									<px:PXSegmentMask ID="edToAccountCD1" runat="server" DataField="ToAccountCD">
									</px:PXSegmentMask>
									<px:PXSegmentMask ID="edMapAccountID1" runat="server" DataField="MapAccountID" OnValueChange="Commit">
									</px:PXSegmentMask>
									<px:PXSegmentMask ID="edMapSubID1" runat="server" DataField="MapSubID" AutoRefresh="True">
									</px:PXSegmentMask>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="FromBranchID">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="FromAccountCD">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="ToAccountCD">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="MapAccountID" CommitChanges="true">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="MapSubID">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="300" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" MinHeight="300" Container="Window" />
	</px:PXTab>
</asp:Content>
