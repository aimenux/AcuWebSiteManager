<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM205530.aspx.cs" Inherits="Pages_SM205530" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.SM.AUAuditInquire" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM"
                StartColumn="True" />
            <px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID" CommitChanges="true">
                <AutoCallBack Enabled="true" ActiveBehavior="true">
                    <Behavior CommitChanges="true" RepaintControlsIDs="gridKeys,gridChanges" />
                </AutoCallBack>
                <GridProperties>
                    <Columns>
                        <px:PXGridColumn DataField="ScreenID" Visible="False" SyncVisible="False"/>
                        <px:PXGridColumn DataField="VirtualScreenID" />
                        <px:PXGridColumn DataField="IsActive" Type="CheckBox" />
                        <px:PXGridColumn DataField="Description" />
                        <px:PXGridColumn DataField="ScreenName" />
                    </Columns>
                </GridProperties>
            </px:PXSelector>
			<px:PXSelector ID="edUserID" runat="server" DataField="UserID" CommitChanges="true" DisplayMode="Text">
				<AutoCallBack Enabled="true" ActiveBehavior="true" >
					<Behavior CommitChanges="true" RepaintControlsIds="gridKeys,gridChanges"  />
				</AutoCallBack>
			</px:PXSelector>
			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="true">
				<AutoCallBack Enabled="true" ActiveBehavior="true" >
					<Behavior CommitChanges="true" RepaintControlsIds="gridKeys,gridChanges"  />
				</AutoCallBack>
			</px:PXDateTimeEdit>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="M" LabelsWidth="SM"	StartColumn="True" />
					<px:PXSelector ID="edTableName" runat="server" DataField="TableName" CommitChanges="true"
						AutoRefresh="True" DisplayMode="Text">
						<AutoCallBack Enabled="true" ActiveBehavior="true">
							<Behavior CommitChanges="true" RepaintControlsIDs="gridKeys,gridChanges" />
						</AutoCallBack>
					</px:PXSelector>
			<px:PXLabel ID="PXHole" runat="server" />
			<px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="true">
				<AutoCallBack Enabled="true" ActiveBehavior="true" >
					<Behavior CommitChanges="true" RepaintControlsIds="gridKeys,gridChanges"  />
				</AutoCallBack>
			</px:PXDateTimeEdit>
        </Template>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="300px">
        <AutoSize Enabled="true" Container="Window" />
        <Template1>
            <px:PXGrid ID="gridKeys" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" AllowPaging="True" 
                SkinID="Inquire" Caption="Records" CaptionVisible="true" OnLayoutLoad="OnGridLayoutLoad" FastFilterFields="All" AdjustPageSize="Auto">
                <Levels>
                    <px:PXGridLevel DataMember="Keys">
                        <Columns>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
				<AutoCallBack Enabled="true" Target="gridChanges" Command="Refresh" />
                <ActionBar PagerVisible="False">
                </ActionBar>
                <AutoSize Container="Parent" Enabled="True" MinHeight="150" />
            </px:PXGrid>	
        </Template1>
		<Template2>
			 <px:PXGrid ID="gridChanges" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
                SkinID="Inquire" Caption="Events" CaptionVisible="true" AllowPaging="True"  AdjustPageSize="Auto">
                <Levels>
                    <px:PXGridLevel DataMember="Changes">
                        <Columns>
							<px:PXGridColumn AllowUpdate="False" DataField="BatchID" >
								<Style Font-Bold="true" />
							</px:PXGridColumn>
							<px:PXGridColumn AllowUpdate="False" DataField="ChangeID" >
								<Style Font-Bold="true" />
							</px:PXGridColumn>
							<px:PXGridColumn AllowUpdate="False" DataField="Operation" Width="100px" >
								<Style Font-Bold="true" />
							</px:PXGridColumn>
							<px:PXGridColumn AllowUpdate="False" DataField="ChangeDate" Width="120px"  DisplayFormat="d" >
								<Style Font-Bold="true" />
							</px:PXGridColumn>
                            <px:PXGridColumn AllowUpdate="false" DataField="UserName" Width="100px">
                                <Style Font-Bold="true" />
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <ActionBar PagerVisible="False">
                </ActionBar>
                <AutoSize Container="Parent" Enabled="True" MinHeight="150" />
				<Parameters>
					<px:PXSyncGridParam Name="SyncChanges" ControlID="gridKeys" />
				</Parameters>
            </px:PXGrid>
		</Template2>
    </px:PXSplitContainer>
</asp:Content>
