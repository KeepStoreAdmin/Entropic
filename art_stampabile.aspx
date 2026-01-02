<%@ Page Language="VB" AutoEventWireup="false" CodeFile="art_stampabile.aspx.vb" Inherits="art_stampabile" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Pagina senza titolo</title>
    <meta name="robots" content="index,follow" />
    <meta http-equiv="imagetoolbar" content="no" />
    <meta http-equiv="Content-Language" content="it" />
    <meta name="Distribution" content="Global" />
    <meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
    <meta name="revisit-after" content="1 days"/>   
    <head><!-- TradeDoubler site verification 1649696 --></head>
    <head><!-- TradeDoubler site verification 1653756 --></head>
    
     <!-- TradeDoubler site verification 1649696 -->
     <link href="public/style/popup.css" rel="stylesheet" type="text/css" />
         
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.4.1/jquery.min.js" type="text/javascript"></script>
    <script type="text/javascript">
    JQ = jQuery.noConflict();
    </script>
    
    <script type="text/javascript" language="JavaScript" src="Public/script/prototype.js"></script>
    <script type="text/javascript" language="Javascript" src="Public/script/scriptaculous.js"> </script>
    <script type="text/javascript" language="Javascript" src="Public/script/effects.js"> </script>
    <script type="text/javascript" language="Javascript" src="Public/script/controls.js"> </script> 
</head>
<body>
    <form id="form1" runat="server">
 <script type="text/javascript" src="https://code.jquery.com/jquery-latest.js"></script> 
<script type="text/javascript" src="Public/script/ddpowerzoomer.js" > </script>
<script type="text/javascript"> 
jQuery(document).ready(function($){
//$(document).ready(function(){
 
//Larger thumbnail preview 
 
$("ul.thumb li").hover(function() {
	$(this).css({'z-index' : '10'});
	$(this).find('img').addClass("hover").stop()
		.animate({
			marginTop: '-110px', 
			marginLeft: '-110px', 
			top: '50%', 
			left: '50%', 
			width: '174px', 
			height: '174px',
			padding: '20px' 
		}, 200);
	
	} , function() {
	$(this).css({'z-index' : '0'});
	$(this).find('img').removeClass("hover").stop()
		.animate({
			marginTop: '0', 
			marginLeft: '0',
			top: '0', 
			left: '0', 
			width: '100px', 
			height: '100px', 
			padding: '5px'
		}, 400);
		
	$('#main_img').addpowerzoom({
		defaultpower: 2,
		powerrange: [2,5],
		largeimage: null,
		magnifiersize: [100,100]
	});
});
 
//Swap Image on Click
	$("ul.thumb li a").click(function() {
		
		var mainImage = $(this).attr("href"); //Find Image Name
		$("#main_view img").attr({ src: mainImage });
		return false;		
	});
	 
	 $('#main_img').addpowerzoom({
		defaultpower: 2,
		powerrange: [2,5],
		largeimage: null,
		magnifiersize: [100,100]
	});
});
</script> 

 <h1>Scheda Prodotto</h1>
 
    <asp:SqlDataSource ID="sdsArticolo" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"  ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
        SelectCommand="SELECT * FROM vsuperarticoli  WHERE (id = ?id) and (NListino = ?NListino)">
        <SelectParameters>
            <asp:QueryStringParameter Name="id" QueryStringField="id" Type="Int32" />
            <asp:SessionParameter Name="NListino" SessionField="listino" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:FormView ID="fvPage" runat="server" DataKeyNames="id" DataSourceID="sdsArticolo" Width="100%">
          <ItemTemplate>

            <br />
            <asp:HyperLink ID="HyperLink1" runat="server" ToolTip="Visualizza tutta la categoria"  NavigateUrl='<%# "articoli.aspx?st="& Eval("SettoriId") &"&ct="& Eval("CategorieID") &"&mr="& Eval("MarcheId") &"&tp="& Eval("TipologieID") &"&gr="& Eval("GruppiID") &"&sg="& Eval("SottogruppiID") %>' Text='<%# Eval("CategorieDescrizione") &" <font color=#E12825><b>»</b></font> "& Eval("MarcheDescrizione") &" <font color=#E12825><b>»</b></font> "& Eval("TipologieDescrizione") &" <font color=#E12825><b>»</b></font> "& Eval("GruppiDescrizione") &" <font color=#E12825><b>»</b></font> "& Eval("SottogruppiDescrizione") %>'></asp:HyperLink>
            <br /><br />

            <span class="link">
            <asp:HyperLink ID="HyperLink5" runat="server" NavigateUrl='<%# "articolo.aspx?id="& Eval("id")%>'>
            <asp:Label ID="lblDescrizione" runat="server" Text='<%# Bind("Descrizione1") %>' Font-Bold="True" Font-Size="12pt" ></asp:Label>
            </asp:HyperLink>
            </span>
            
            <asp:TextBox ID="tbID" runat="server" Text='<%# Eval("ID") %>' Width="30" EnableViewState="false" Visible="false"></asp:TextBox>
            <asp:SqlDataSource ID="sdsPromo" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>" ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
             SelectCommand="SELECT * FROM vsuperarticoli WHERE ID=?ID AND NListino=?NListino ORDER BY PrezzoPromo DESC" EnableViewState="False">
             <SelectParameters>
                  <asp:ControlParameter Name="ID" ControlID="tbID" PropertyName="Text" Type="Int32" />
                  <asp:SessionParameter Name="NListino" SessionField="listino" Type="Int32" />
             </SelectParameters>
             </asp:SqlDataSource>
                        
             <asp:Repeater ID="rPromo" runat="server" DataSourceID="sdsPromo" EnableViewState="false" OnItemDataBound="rPromo_ItemDataBound">
             <ItemTemplate>
             <asp:Label ID="lblQtaMin" runat="server" Text='<%# Eval("OfferteQntMinima") %>' Visible="false"></asp:Label>
             <asp:Label ID="lblMultipli" runat="server" Text='<%# Eval("OfferteMultipli") %>' Visible="false"></asp:Label>
             <asp:Label ID="lblPrezzoPromo" runat="server" Text='<%# Eval("PrezzoPromo") %>' Visible="false"></asp:Label>
             <asp:Label ID="lblPrezzoPromoIvato" runat="server" Text='<%# Eval("PrezzoPromoIvato") %>' Visible="false"></asp:Label>
             <asp:Label ID="lblInOfferta" runat="server" Text='<%# Eval("InOfferta") %>' Visible="false"></asp:Label>
             <asp:Label ID="lblOfferta" runat="server" Visible='<%# Eval("InOfferta")%>' Text='<%# "<br>PROMO DAL "& Eval("OfferteDataInizio") &" AL "& Eval("OfferteDataFine") %>' BackColor="#e12825" ForeColor="white" style="line-height:150%"></asp:Label>
             </ItemTemplate>
             </asp:Repeater>           
            
            <br /><div style="float:left">
            <asp:Label ID="Label9" runat="server" Text="Marca: " Font-Bold="true" Width="50"></asp:Label><asp:Label ID="lblMarca" runat="server" Text='<%# Bind("MarcheDescrizione") %>' Width="120"></asp:Label>
            <br /><asp:Label ID="Label10" runat="server" Text="Codice: " Font-Bold="true" Width="50"></asp:Label><asp:Label ID="lblCodice" runat="server" Text='<%# Bind("Codice") %>' Width="120"></asp:Label>
            <br /><asp:Label ID="Label11" runat="server" Text="EAN: " Font-Bold="true" Width="50"></asp:Label><asp:Label ID="lblEan" runat="server" Text='<%# Bind("ean") %>' Width="120"></asp:Label>
            </div>
            <asp:Label ID="Label8" runat="server" Text='<%# Eval("Descrizione2") %>' Font-Size="8pt" Font-Italic="true"></asp:Label>
            
            
            <table width="100%">
                <tr>
                    <td width="70%">
                        <!-- <br /><asp:Image ID="Image1" runat="server" ImageAlign="left" ImageUrl='<%# "public/foto/"& Eval("Img1") %>' AlternateText='<%# Eval("Descrizione1") %>' OnPreRender="Image1_PreRender"/> -->
                        <div class="container"> 
                        <div id="main_view"> 
	                        <a href="#" target="_self"><img id="main_img" height="200px" src="<%# "public/foto/"& Eval("Img1") %>" alt="<%# Eval("Descrizione1") %>" /></a>
                        </div> 
                        <div class="thumb"> 
                            <ul class="thumb"> 
                            <script runat="server">
                                Function checkImg(ByVal imgname As String) As String
                                    If imgname <> "" Then
                                        Return "public/foto/" & imgname
                                    Else
                                        Return "Public/Foto/img_non_disponibile.png"
                                    End If
                                End Function
                            </script>
                                <li><a href="<%# checkImg(Eval("Img1")) %>"><img src="<%# checkImg(Eval("Img1")) %>" width="100" alt="" /></a></li> 
                                <li><a href="<%# checkImg(Eval("Img2")) %>"><img src="<%# checkImg(Eval("Img2")) %>" width="100" alt="" /></a></li> 
                                <li><a href="<%# checkImg(Eval("Img3")) %>"><img src="<%# checkImg(Eval("Img3")) %>" width="100" alt="" /></a></li> 
                                <li><a href="<%# checkImg(Eval("Img4")) %>"><img src="<%# checkImg(Eval("Img4")) %>" width="100" alt="" /></a></li> 
                            </ul> 
                        </div>
                        </div> 
                    </td>
                    <td>
      
                <asp:Label ID="Label7" runat="server" Text="Disponibilità" Width="90"></asp:Label><asp:Label ID="lblPunti1" runat="server" Font-Bold="false" Visible="false" Text=":"></asp:Label><asp:Label ID="lblDispo" runat="server" visible="false" Text='<%# Eval("Giacenza") %>' ForeColor="#E12825" style="text-align:right" Width="25" Font-Bold="true"></asp:Label><br />
                <asp:Image ID="imgDispo" runat="server" visible="true"/>
                <asp:Label ID="lblImp" runat="server" Text="Impegnata" Width="90" Visible="false"></asp:Label><asp:Label ID="lblPunti2" Font-Bold="false" runat="server" Visible="false" Text=":"></asp:Label><asp:Label ID="lblImpegnata" runat="server" visible="false" Text='<%# Eval("Impegnata") %>' ForeColor="#E12825" style="text-align:right" Width="25" Font-Bold="true"></asp:Label>
                <br><asp:Label ID="lblArr" runat="server" Text="In Arrivo" Width="90" Visible="false"></asp:Label><asp:Label ID="lblPunti3" Font-Bold="false" runat="server" Visible="false" Text=":"></asp:Label><asp:Label ID="lblArrivo" runat="server" visible="false" Text='<%# Eval("InOrdine") %>' ForeColor="#E12825" style="text-align:right" Width="25" Font-Bold="true"></asp:Label>
                <br />
                <br />
                <asp:Label ID="lblPrezzoDes" runat="server" Text="Prezzo" Font-Bold="true"></asp:Label>
                <br />
                <asp:Label ID="lblPrezzoIvato" runat="server" Text='<%# Bind("PrezzoIvato","{0:C}") %>' ForeColor="#E12825" Font-Bold="True" Font-Size="12pt" Width="125" style="text-align:right"></asp:Label>
                <asp:Label ID="lblPrezzo" runat="server" Font-Bold="True" Font-Size="12pt" ForeColor="#E12825" Text='<%# Bind("Prezzo","{0:C}") %>' Visible="False" Width="125" style="text-align:right"></asp:Label>
                <br />
                <asp:Label ID="lblPrezzoPromo" runat="server" Font-Bold="True" Font-Size="12pt" ForeColor="#E12825" Text='<%# Bind("Prezzo","{0:C}") %>' Visible='<%# Eval("InOfferta") %>' Width="125" style="text-align:right"></asp:Label>
                <br />
                <br />
                <asp:Label ID="Label1" runat="server" Text="Aggiungi al Carrello" Font-Bold="true" ForeColor="#E12825"></asp:Label>
                <br />
                <br />
                <asp:TextBox ID="tbQuantita" runat="server" Width="30px" style="text-align:right;font-size:10pt">1</asp:TextBox>
                <asp:ImageButton ID="ImageButton1" runat="server" ImageUrl="Images/cart.gif" ToolTip="Aggiungi al Carrello" OnClick="ImageButton1_Click"/>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="tbQuantita" Display="Dynamic" ErrorMessage="Inserire una Quantità Valida" SetFocusOnError="True"></asp:RequiredFieldValidator>
                <asp:CompareValidator ID="CompareValidator1" runat="server" ControlToValidate="tbQuantita" Display="Dynamic" ErrorMessage="Inserire una Quantità Valida" Operator="GreaterThan" SetFocusOnError="True" Type="Integer" ValueToCompare="0"></asp:CompareValidator>
                
                    </td>
                </tr>
            </table>

<br />
<h1>Dettagli</h1>
<br />
            <asp:Label ID="lblDescrizioneArt" runat="server" Text='<%# Eval("DescrizioneLunga") %>' Font-Size="8pt" style="line-height:130%"></asp:Label>
 <br /> <br />           
        </ItemTemplate>
    </asp:FormView>

    </form>
</body>
</html>
