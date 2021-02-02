<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR209800.aspx.cs"
    Inherits="Page_PR209800" Title="Work Class Compensation Code" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PR.PRWorkCodeMaint" PrimaryView="WorkCompensationCodes" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSplitContainer ID="splitContainerWorkCodes" runat="server" PositionInPercent="true" SplitterPosition="40" Orientation="Vertical" Height="100%">
		<Template1>
			<px:PXGrid ID="grid1" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
				AdjustPageSize="Auto" DataSourceID="ds" SkinID="Details" SyncPosition="true" FastFilterFields="Description" CaptionVisible="true" Caption="WCC Code" KeepPosition="true">
				<Levels>
					<px:PXGridLevel DataMember="WorkCompensationCodes">
						<Columns>
							<px:PXGridColumn DataField="IsActive" Type="CheckBox" />
							<px:PXGridColumn DataField="WorkCodeID" />
							<px:PXGridColumn DataField="Description" />
							<px:PXGridColumn DataField="CostCodeFrom" />
							<px:PXGridColumn DataField="CostCodeTo" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
				<AutoCallBack Enabled="true" Command="Refresh" Target="rateGrid" />
			</px:PXGrid>
		</Template1>
		<Template2>
			<px:PXGrid ID="rateGrid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
				AdjustPageSize="Auto" DataSourceID="ds" SkinID="Details" SyncPosition="true" CaptionVisible="true" Caption="Rate">
				<Levels>
					<px:PXGridLevel DataMember="WorkCompensationRates">
						<RowTemplate>
							<px:PXNumberEdit ID="edRate" runat="server" DataField="Rate" />
						</RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="IsActive" Type="CheckBox" TextAlign="Center" Width="60px" />
							<px:PXGridColumn DataField="DeductCodeID" Width="120px" CommitChanges="true" />
							<px:PXGridColumn DataField="DeductionCalcType" Width="150px" />
							<px:PXGridColumn DataField="PRDeductCode__CntCalcType" Width="150px" />
							<px:PXGridColumn DataField="PRDeductCode__State" Width="80px" />
							<px:PXGridColumn DataField="DeductionRate" Width="80px" />
							<px:PXGridColumn DataField="Rate" Width="80px" />
							<px:PXGridColumn DataField="EffectiveDate" CommitChanges="true" Width="200px" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
				<ActionBar>
					<Actions>
						<Delete Enabled="false" />
					</Actions>
				</ActionBar>
			</px:PXGrid>
		</Template2>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXSplitContainer>
</asp:Content>
