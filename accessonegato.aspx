<%@ Page Language="VB" MasterPageFile="~/Page.master" AutoEventWireup="false" CodeFile="accessonegato.aspx.vb" Inherits="accessonegato" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cph" Runat="Server">

<h1>Accesso Negato</h1>
<br />
<b>Per eseguire l'operazione richiesta è necessario registrarsi al sito <asp:Label ID="lblUrl" runat="server" Text="Url" Font-Bold="True"></asp:Label></b>
<br /><br />
Se sei già un utente registrato, allora digita la tua <i>Username</i> e la tua <i>Password</i> nel box "<b>My Account"</b> presente sulla destra di ogni pagina, ed effettua il <b>Login</b> in tutta sicurezza.
<br /><br />
<a href="remind.aspx"><span style="text-decoration: underline">Non ricordi i tuoi dati d'accesso al sito <asp:Label ID="lblSito" runat="server" Text="sito"  Font-Underline="True"></asp:Label>?</span></a>
<br /><br /><br />
<b><asp:Label ID="Label1" ForeColor="#e12825" runat="server" Text="Se non sei registrato, registrati subito! E' gratis e potrai usufruire di tanti vantaggi:"></asp:Label>
<br />
    <ul>
    <li>Assegnazione di scontistiche dei prodotti</li>
    <li>Richiedere quotazioni per quantità</li>
    <li>Inviare ordini e visualizzare il loro stato in tempo reale</li>
    <li>Visualizzare in "My Account" tutte le tue movimentazioni</li>
    <li>Ricevere le promozioni ed offerte personalizzate</li>
    <li>Effettuare richiesta di resi merce direttamente online</li>
    <li>Accumulare punti fedeltà per i premi</li>
    </ul>
    </b>
    <p align="center"><asp:Button ID="Button1" runat="server" Text="REGISTRATI ADESSO" PostBackUrl="registrazione.aspx"/></p>
 
<hr />

</asp:Content>

