<%@ Page Language="VB" MasterPageFile="~/Page.master" AutoEventWireup="false" CodeFile="login.aspx.vb" Inherits="Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph" Runat="Server">
<script language="vbscript" type="text/vbscript">

sub ctl00_cph_tRegistrazione_onkeydown()
    if window.event.keyCode=13 then
        window.event.returnValue=false
    end if
end sub

</script>

<style>
#tabellaLogin
{
	margin-top: 30px;
}
</style>

<h1 align="center">Accedi</h1>
<div class="ks-table" id="tabellaLogin">
	<div class="ks-sector" style="margin-top:10px">
		<div class="ks-col">
			<div class="ks-row login-content login-label">
				<asp:Label ID="lblUsername" runat="server" Text="USERNAME:" Visible="True"></asp:Label>
			</div>
		</div>
	</div>
	<div class="ks-sector-no-flex">
		<div class="ks-col">
			<div class="ks-row login-content" style="text-align:right">
				<i id="login-user-icon" class="fa fa-user-circle fa-3x"></i>
			</div>
			<div class="ks-row login-content">
			</div>
		</div>
		<div class="ks-col">
			<div class="ks-row login-content">
				<asp:TextBox ID="tbUsername" CssClass="form-control" AutoPostBack="false" runat="server" Visible="True"></asp:TextBox>
			</div>
			<div class="ks-row login-content validator">
				<asp:RequiredFieldValidator ID="RequiredFieldValidatorUser" runat="server" ControlToValidate="tbUsername" ErrorMessage="Inserire Username"></asp:RequiredFieldValidator>
			</div>
		</div>
	</div>
	<div class="ks-sector" style="margin-top:10px">
		<div class="ks-col">
			<div class="ks-row login-content login-label">
				<asp:Label ID="lblPassword" runat="server" Text="PASSWORD:" Visible="True"></asp:Label>
			</div>
		</div>
	</div>
	<div class="ks-sector-no-flex">
		<div class="ks-col">
			<div class="ks-row login-content" style="text-align:right">
				<i id="login-pass-icon" class="fa fa-key fa-3x"></i>
			</div>
			<div class="ks-row login-content">
			</div>
		</div>
		<div class="ks-col">
			<div class="ks-row login-content">
				<asp:TextBox ID="tbPassword" CssClass="form-control" AutoPostBack="false" TextMode="Password" runat="server" Visible="True"></asp:TextBox>
			</div>
			<div class="ks-row login-content validator">
				<asp:RequiredFieldValidator ID="RequiredFieldValidatorPass" runat="server" ControlToValidate="tbPassword" ErrorMessage="Inserire Password"></asp:RequiredFieldValidator>
			</div>
		</div>
	</div>
	<div class="ks-sector" style="margin-top:20px">
		<div class="ks-col">
			<div class="ks-row login-content" style="text-align:center">
				<asp:Button ID="btLogin" CssClass="btnStandardColor btn" CausesValidation="True" Visible="true" runat="server" Text="Login" PostBackUrl="login.aspx"/>
			</div>
			<div class="ks-row login-content" style="text-align:center">
				<asp:Label ID="lblLogin" runat="server" Font-Size="8pt" ForeColor="Red" Font-Bold="True" EnableViewState="False"></asp:Label>
			</div>
			<div class="ks-row login-content" style="text-align:center">
				<a id="hlRegistrati" href="registrazione.aspx" style="font-weight:bold;">REGISTRATI!</a>
			</div>
			<div class="ks-row login-content" style="text-align:center">
				<a id="hlRemind" href="remind.aspx">PASSWORD PERSA?</a>
			</div>
		</div>
	</div>
</div>
  

</asp:Content>