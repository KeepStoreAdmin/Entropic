<%@ Page Language="VB" MasterPageFile="~/Page.master" AutoEventWireup="false" CodeFile="remind.aspx.vb" Inherits="Remind" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cph" Runat="Server">

<style>
#tabellaReminder
{
	margin-top: 30px;
}
</style>

<h1 align="center">Non ricordi i tuoi dati d'accesso al sito?</h1>
<div class="ks-table" id="tabellaReminder">
	<div class="ks-sector" style="margin-top:10px">
		<div class="ks-col">
			<div class="ks-row login-content login-label">
				<asp:Label ID="lblUsername" runat="server" Text="Inserisci il tuo indirizzo Email e te li spediremo!" Visible="True"></asp:Label>
			</div>
		</div>
	</div>
	<div class="ks-sector-no-flex">
		<div class="ks-col">
			<div class="ks-row login-content" style="text-align:right">
				<i id="login-email-icon" class="fa fa-envelope fa-3x"></i>
			</div>
			<div class="ks-row login-content">
			</div>
		</div>
		<div class="ks-col">
			<div class="ks-row login-content" style="width:350px">
				<asp:TextBox ID="tbEmail" CssClass="form-control" AutoPostBack="false" runat="server" Visible="True"></asp:TextBox>
			</div>
			<div class="ks-row login-content validator">
				<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="tbEmail" ErrorMessage="Indirizzo Email non valido!" Font-Bold="True" SetFocusOnError="True" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="Dynamic"></asp:RegularExpressionValidator>
				<asp:Label ID="lblError" runat="server" Font-Bold="True" ForeColor="Red" Text="Indirizzo Email non presente in archivio!" Visible="False"></asp:Label>
				<asp:RequiredFieldValidator ID="RequiredFieldValidatorUser" runat="server" ControlToValidate="tbEmail" ErrorMessage="Inserire Email"></asp:RequiredFieldValidator>
			</div>
		</div>
	</div>
	<div class="ks-sector" style="margin-top:20px">
		<div class="ks-col">
			<div class="ks-row login-content" style="text-align:center">
				<asp:Button ID="btInvia" CssClass="btnStandardColor btn" CausesValidation="True" Visible="true" runat="server" Text="Invia dati d'accesso" PostBackUrl="remind.aspx"/>
			</div>
			<div class="ks-row login-content" style="text-align:center">
				<asp:Label ID="lblOk" runat="server" Font-Size="8pt" Visible=false Text="I tuoi dati d'accesso al sito sono stati inviati correttamente.<br><br>Attendi qualche istante e controlla la tua email." Font-Bold="True" EnableViewState="False"></asp:Label>
			</div>
		</div>
	</div>
</div>
</asp:Content>

