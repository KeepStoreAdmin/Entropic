<%@ Page Language="VB" MasterPageFile="~/Page.master" AutoEventWireup="false" CodeFile="ordine.aspx.vb" Inherits="ordine"  %>
<asp:Content ID="Content1" ContentPlaceHolderID="cph" Runat="Server">

<%For Each pairInidsFbPixelsSku In idsFbPixelsSku
Dim facebook_pixel_id As String = pairInidsFbPixelsSku.key
Dim sku As String = pairInidsFbPixelsSku.value%>
<!-- Facebook Pixel Code -->
<script>
  !function(f,b,e,v,n,t,s)
  {if(f.fbq)return;n=f.fbq=function(){n.callMethod?
  n.callMethod.apply(n,arguments):n.queue.push(arguments)};
  if(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';
  n.queue=[];t=b.createElement(e);t.async=!0;
  t.src=v;s=b.getElementsByTagName(e)[0];
  s.parentNode.insertBefore(t,s)}(window, document,'script',
  'https://connect.facebook.net/en_US/fbevents.js');
  fbq('init', '<%=facebook_pixel_id%>'<%if utenteId=-1 then%>);<%else%>, {
	fn: '<%=firstName%>',
    ln: '<%=lastName%>',
	em: '<%=email%>',
    ph: '<%=phone%>',
	country: '<%=country%>',
	st: '<%=province%>',
	ct: '<%=city%>',
	zp: '<%=cap%>'
  });<%end if%>
  fbq('track', 'Purchase', {
    content_ids: '<%=sku%>',
    content_type: 'product',
  });
</script>
<noscript><img height="1" width="1" style="display:none"
  src="https://www.facebook.com/tr?id=<%=facebook_pixel_id%>&ev=PageView&noscript=1"
/></noscript>
<!-- End Facebook Pixel Code -->
<%next%>

<script>
window.location.replace("<%=redirect%>");
</script>
<h1>Carrello</h1>

   
<br /><br /><br />
<center>

<asp:label id="img_bs_label" runat="server" />
    
<asp:Panel ID="Panel1" runat="server">
<asp:Label ID="Label2" runat="server" Text="" Font-Bold="true"></asp:Label> n° <asp:Label ID="Label1" runat="server" Text="" Font-Bold="true"></asp:Label> del <asp:Label ID="Label3" runat="server" Text="" Font-Bold="true"></asp:Label> correttamente inviato.
<br /><br /><br />
<asp:HyperLink ID="HyperLink1" runat="server" Font-Underline="true"></asp:HyperLink>
</asp:Panel>
<asp:Panel ID="Panel2" runat="server" Visible="false">
<b>Si è verificato un problema durante l'elaborazione.</b><br /><br />La preghiamo di contattare l'amministratore.
</asp:Panel>

</center>
<br /><br /><br />
<hr />
<div runat="server" id="DivImg">
    
</div>
<asp:Literal runat="server" ID="litScript"></asp:Literal>
 
</asp:Content>

