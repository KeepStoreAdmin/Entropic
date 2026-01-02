<%@ Page Language="VB" AutoEventWireup="false" CodeFile="articolix.aspx.vb" Inherits="articolix" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Offerte</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:SqlDataSource ID="sdsArticoli" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
        ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
        SelectCommand="SELECT * FROM varticolibase ORDER BY Codice, Descrizione1" EnableViewState="False">
    </asp:SqlDataSource>
    
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataKeyNames="id"
        DataSourceID="sdsArticoli" Font-Size="8pt" GridLines="None" CellPadding="3" 
            Width="100%" style=" z-index:-1;" ShowFooter="True">
        <Columns>      
            <asp:TemplateField>
                <ItemTemplate>
                    <table style="width:100%; border-bottom: lightgrey solid; padding-bottom: 2px; margin-bottom: 2px;">
                        <tr>
                            <td rowspan="4" style=" width:150px; height:170px; border-style:solid; border-width:1px; border-color:lightgrey; text-align:center; vertical-align:middle;">
                                <!-- Immagine Prodotto -->
                                <asp:HyperLink ID="HyperLink3" runat="server" NavigateUrl='<%# "~/articolo.aspx?id="& Eval("id") %>' >
                                <asp:Image ID="Image1" runat="server" style=" max-height:170px; max-width:150px;" AlternateText='<%# Eval("Descrizione1") %>' ImageUrl='<%# Eval("img1", "~/Public/foto/_{0}") %>' />
                                </asp:HyperLink>
                                <!-- Controllo se esiste l'immagine -->
                                <script runat="server">
                                    Function controllo_img(ByVal temp) As String
                                        If IsDBNull(temp) Then
                                            Return "false"
                                        Else
                                            Return "true"
                                        End If
                                    End Function
                                </script>
                                <!-- Immagine e Descrizone Marca -->
                                <div style="width:150px;  text-align:center; vertical-align:bottom;">
                                    <asp:Image ID="Image2" runat="server" style="margin:auto auto 0 auto; max-width:150px; max-height:100px;" AlternateText='<%# Eval("MarcheDescrizione") %>' ImageUrl='<%# Eval("Marche_img", "~/Public/Marche/{0}") %>' visible='<%# controllo_img(Eval("Marche_img")) %>'/>
                                </div>
                                
                                <br />
                            </td>
                            <td colspan="2" style="padding-left:10px; padding-top:10px; padding-bottom:10px; padding-right:0px;">
                                <!-- Titolo -->
                                <span style="font-size:10pt; vertical-align:middle; color:Black;">
                                <div style="padding:5px;">
                                    <asp:Label ID="Label12" runat="server" Text=' <%# Eval("MarcheDescrizione") %>' Font-Size="11pt" Font-Bold="true" Height="10px" ForeColor="red" style="display:inline;"></asp:Label>
                                    <asp:HyperLink ID="HyperLink5"  ToolTip='<%# Eval("SettoriDescrizione") &" > "&  Eval("CategorieDescrizione") &" > "& Eval("MarcheDescrizione") &" > "& Eval("TipologieDescrizione") &" > "&  Eval("GruppiDescrizione") &" > "&  Eval("SottogruppiDescrizione") &" > "&  Eval("Codice") %>' runat="server" NavigateUrl='<%# "~/articolo.aspx?id="& Eval("id") %>'>
                                    <asp:Label ID="Label8" runat="server" Text='<%# " - " & Eval("Descrizione1") %>' Font-Size="9pt" Font-Bold="true" ForeColor="black"></asp:Label>
                                    </asp:HyperLink>
                                    <br />
                                    <a href='<%# "articoli.aspx?tp=" & Eval("TipologieId") & "&mr=" & Eval("MarcheId") %>''><span style="color:Red; font-size:7pt;">
                                    in <%# Eval("SettoriDescrizione") &" > "&  Eval("CategorieDescrizione") &" > "& Eval("MarcheDescrizione")%></span></a>
                                </div>
                                <!-- PROMO -->
                                <asp:SqlDataSource ID="sdsPromo" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>" ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"  SelectCommand="SELECT * FROM vsuperarticoli WHERE ID=?ID AND NListino=?NListino ORDER BY PrezzoPromo DESC" EnableViewState="False">
                                    <SelectParameters>
                                        <asp:ControlParameter Name="ID" ControlID="tbID" PropertyName="Text" Type="Int32" />
                                        <asp:SessionParameter Name="NListino" SessionField="listino" Type="Int32" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                
                                <asp:Repeater ID="rPromo" runat="server" DataSourceID="sdsPromo" EnableViewState="false" OnItemDataBound="rPromo_ItemDataBound">
                                <ItemTemplate>
                                <%Session("InOfferta") = 1%>
                                <table visible='<%# Eval("InOfferta")%>' style="height:27px; border-style:none;" cellspacing="0">
                                    <tr style="border-style:solid; border-color:Red; border-width:2px;">
                                        <td style="width:91px;">
                                        <img alt="" src="Images/Promo/Promo_fisso.gif" />
                                        </td>
                                        <td style="vertical-align:middle; padding-left:10px;">
                                            <asp:Label ID="lblQtaMin" runat="server" Text='<%# Eval("OfferteQntMinima") %>' Visible="false"></asp:Label>
                                            <asp:Label ID="lblMultipli" runat="server" Text='<%# Eval("OfferteMultipli") %>' Visible="false"></asp:Label>
                                            <asp:Label ID="lblPrezzoPromo" runat="server" Text='<%# Eval("PrezzoPromo") %>' Visible="false"></asp:Label>
                                            <asp:Label ID="lblPrezzoPromoIvato" runat="server" Text='<%# Eval("PrezzoPromoIvato") %>' Visible="false"></asp:Label>
                                            <asp:Label ID="lblInOfferta" runat="server" Text='<%# Eval("InOfferta") %>' Visible="false"></asp:Label>
                                            <div style="margin-top:-10px;">
                                                <asp:Label ID="lblOfferta" runat="server" Visible='<%# Eval("InOfferta")%>' Font-Size="7pt" Text='<%# "<br>DAL "& Eval("OfferteDataInizio") &" AL "& Eval("OfferteDataFine") %>' ForeColor="Black"></asp:Label>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                                </ItemTemplate>
                                </asp:Repeater>
                                </span>     
                            </td>
                        </tr>
                        <tr>
                            <td style="padding:10px;">
                                <asp:TextBox ID="tbID" runat="server" Text='<%# Eval("ID") %>' Width="30" EnableViewState="false" Visible="false" ></asp:TextBox>
                                <asp:TextBox ID="tbInOfferta" runat="server" Text='<%# Eval("InOfferta") %>' Width="30" EnableViewState="false" Visible="false" ></asp:TextBox>

                                <div>
                                    <script runat="server">
                                        Function sotto_stringa(ByVal temp As String) As String
                                            temp = Server.HtmlEncode(temp)
                                            Return Left(temp.Replace("&#160;", " "), 200) & " ..."
                                        End Function
                                    </script>
                                    <asp:Label ID="Label11" runat="server" Text='<%# sotto_stringa(Eval("DescrizioneLunga")) %>' Font-Size="8pt" wrap="true"  EnableViewState="False" EnableTheming="False"></asp:Label>
                                    <asp:HyperLink ID="HyperLink_Continua" runat="server" NavigateUrl='<%# "~/articolo.aspx?id="& Eval("id") %>' ForeColor="Blue"> 
                                    continua</asp:HyperLink>
                                    <br /><br /><br />
                                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("Descrizione2") %>' Font-Size="7pt" Font-Italic="true" wrap="true" EnableTheming="False" EnableViewState="False"></asp:Label>
                                </div>
                                
                                <div style=" bottom:0px;">
                                <br /><asp:Label ID="Label5" runat="server" Text="Codice: " Font-Bold="True" Width="50"></asp:Label><asp:Label ID="Label3" runat="server"  Text='<%# Eval("Codice") %>' Width="80%" ForeColor="blue"></asp:Label>
                                <br /><asp:Label ID="Label6" runat="server" Text="EAN: " Font-Bold="True" Width="50"></asp:Label><asp:Label ID="Label7" runat="server"  Text='<%# Eval("Ean") %>' Width="80%" ForeColor="blue"></asp:Label>
                                </div>
                            </td>
                            <td>
                                <!-- Disponibilità -->
                                &nbsp;
                                
                                <!-- -->
                                <div style=" text-align:right; padding-bottom:3px;">
                                    <asp:Label ID="lblID" runat="server" Text='<%# Bind("ID") %>' style="z-index:-1;" Visible="false"></asp:Label>
                                    &nbsp;
                                <asp:Label ID="lblArrivo" runat="server" Text='<%# Eval("InOrdine")%>' Visible="false"></asp:Label><br />
                                    <div style="padding-right: 10px; background-position: right 50%; background-image: url(Public/Images/info_disp.png);
                                        width: 180px; color: black; padding-top: 10px; background-repeat: no-repeat; height: 50px; margin:0 0 0 auto;">
                                        <!-- Informazioni Articolo -->
                                        <div>
                                            <table style="width:150px; height:30px; float:right;" cellspacing="0" cellpadding="0">
                                                <tr>
                                                    <td>
                                                        <asp:Label ID="lblDispo" runat="server" Font-Bold="True" Text="Disponibilità: "></asp:Label>
                                                    </td>
                                                    <td>
                                                        <asp:Label ID="Label_dispo" runat="server" ForeColor="Red" Text='<%# Eval("Giacenza") %>'></asp:Label>
                                                        <asp:Image ID="imgDispo" runat="server"/>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Label ID="lblImpegnata" runat="server" Font-Bold="True" Text="Impegnati: "></asp:Label>
                                                    </td>
                                                    <td>
                                                        <asp:Label ID="Label_imp" runat="server" ForeColor="Red" Text='<%# Eval("Impegnata") %>'></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:Label ID="lblArrivo2" runat="server" Font-Bold="True" Text="In Arrivo: "></asp:Label>
                                                    </td>
                                                    <td>
                                                       <asp:Label ID="Label_arrivo" runat="server" ForeColor="Red" Text='<%# Eval("InOrdine") %>' style="padding-right"></asp:Label>
                                                        <asp:Image ID="imgArrivo" runat="server" Visible="false"/>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </div>
                                        &nbsp;
                                    <br />

                                    <!-- Cifre con immagini -->
                                    <div style="padding:0px; float:right; color: black; width: 200px;">
                                    <asp:Image ID="img_prezzo9" runat="server" Height="30px" Visible="False" />
                                    <asp:Image ID="img_prezzo8" runat="server" Height="30px" Visible="False" />
                                    <asp:Image ID="img_prezzo7" runat="server" Height="30px" Visible="False" />
                                    <asp:Image ID="img_prezzo6" runat="server" Height="30px" Visible="False" />
                                    <asp:Image ID="img_prezzo5" runat="server" Height="30px" Visible="False" />
                                    <asp:Image ID="img_prezzo4" runat="server" Height="30px" Visible="False" />
                                    <asp:Image ID="img_prezzo3" runat="server" Height="20px" Visible="False" />
                                    <asp:Image ID="img_prezzo2" runat="server" Height="20px" Visible="False" />
                                    <asp:Image ID="img_prezzo1" runat="server" Height="20px" Visible="False" />&nbsp;
                                    <%  
                                        If Session("InOfferta") = 1 Then
                                            Session("InOfferta") = 0
                                    %>
                                    <div style="height:20px;">
                                    <asp:Panel ID="Panel_in_offerta" runat="server" Height="15px" Width="150px" Visible="False" style="margin:0 0 0 auto;">
                                        invece di
                                    <asp:Label ID="Label4" runat="server" Text='<%# Bind("PrezzoIvato", "{0:C}") %>' ForeColor="Red" style="text-decoration:line-through;"></asp:Label><asp:Label ID="Label10" runat="server" Text='<%# Bind("Prezzo", "{0:C}") %>' Visible="False" ForeColor="Red"></asp:Label></asp:Panel>
                                    </div>
                                    <%  End If%>
                                    </div>

                                    <asp:Label ID="lblPrezzoIvato" runat="server" Text='<%# Bind("PrezzoIvato", "{0:C}") %>'></asp:Label>
                                    <asp:Label ID="lblPrezzo" runat="server" Text='<%# Bind("Prezzo", "{0:C}") %>' Visible="false"></asp:Label>
                                    <br />
                                    <asp:Label ID="lblPrezzoPromo" runat="server" Text='<%# Eval("Prezzo", "{0:C}") %>' Visible='<%# Eval("InOfferta") %>'></asp:Label>                         
                                    <br />
                                   <asp:HyperLink ID="HyperLink2" Visible="false" runat="server" ImageUrl='<%# "images/cart.gif" %>'  Text="Scheda Prodotto"></asp:HyperLink>
                                    &nbsp;&nbsp;&nbsp;
                               </div>
                            </td>
                        </tr>
                        <tr>
                            <td nowrap="nowrap" style="text-align: right; padding-right:10px; background-position: bottom right; background-repeat:no-repeat;" colspan="2">
                                <div style="float:left; vertical-align:bottom;  width:40%; padding-top:10px">
                                    <asp:Image ID="img_offerta" runat="server" ImageUrl="~/Images/icon_ecommerce/golden_offer.png"
                                        Visible="False" style="float:left;"/>
                                    <asp:Image ID="img_regalo" runat="server" ImageUrl="~/Images/icon_ecommerce/present.png"
                                        Visible="False" style="float:left;"/>
                                    <asp:Image ID="img_nodisp" runat="server" ImageUrl="~/Images/icon_ecommerce/cancel.png"
                                        Visible="False" style="float:left;"/>
                                    <asp:Image ID="img_trasportogratis" runat="server" ImageUrl="~/Images/icon_ecommerce/bag_green.png"
                                        Visible="False" style="float:left;"/>

                                    <!-- Spedizione Gratis -->
                                    <asp:SqlDataSource ID="sdsSpedizioneGratis" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
                                        ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
                                        SelectCommand="SELECT SpedizioneGratis_Listini, SpedizioneGratis_Data_Inizio, SpedizioneGratis_Data_Fine, id FROM articoli WHERE (SpedizioneGratis_Listini LIKE CONCAT('%', @Param1, ';%')) AND (id = @Param2) AND (SpedizioneGratis_Data_Inizio <= CURDATE()) AND (SpedizioneGratis_Data_Fine >= CURDATE())">
                                        <SelectParameters>
                                            <asp:SessionParameter Name="Param1" SessionField="Listino" />
                                            <asp:ControlParameter ControlID="lblID" Name="Param2" PropertyName="Text" />
                                        </SelectParameters>
                                    </asp:SqlDataSource>
                                    <asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" DataSourceID="sdsSpedizioneGratis" BorderWidth="0px" ShowHeader="False" style="float:left; vertical-align:middle;">
                                        <Columns>
                                            <asp:TemplateField>
                                                <ItemTemplate>
                                                    <img style="border-width:0px; background-color:white; margin-top:5px;" src="Images/freeshipping_fisso.jpg" title='Questo articolo verrà spedito GRATIS !!! fino al <%# Eval("SpedizioneGratis_Data_Fine","{0:d}") %>' alt="" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <RowStyle BorderColor="White" BorderWidth="0px" />
                                        <PagerStyle BorderColor="White" BorderWidth="0px" />
                                    </asp:GridView>
                                    <!-- --------------------------------------------------------------------------------------------------- -->
                                    </div> 
                            </td>
                        </tr>
                    </table>
         </ItemTemplate>
    </asp:TemplateField>
     </Columns>
        <PagerStyle CssClass="nav" />
        <SelectedRowStyle BackColor="Red" />
        <AlternatingRowStyle BorderStyle="None" />
    </asp:GridView>
    </div>
    </form>
</body>
</html>
