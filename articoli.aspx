<%@ Page Language="VB" MasterPageFile="~/Page.master" AutoEventWireup="false" CodeFile="articoli.aspx.vb" Inherits="Articoli" MaintainScrollPositionOnPostback="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cph" Runat="Server">

    <h1> 
    <asp:FormView ID="FormView1" runat="server" DataSourceID="sdsCategorie" EnableViewState="False">
        <ItemTemplate>
    <asp:Label ID="lblSettore" runat="server" Text='<%# ucase(Eval("SettoriDescrizone")) %>' EnableViewState="False"></asp:Label>
            »
    <asp:Label ID="lblCategoria" runat="server" Text='<%# Eval("Descrizione") %>' EnableViewState="False"></asp:Label>
        </ItemTemplate>
    </asp:FormView>
            <asp:Label ID="lblRicerca" runat="server" Text="Risultato ricerca per:" Font-Bold="False" Visible="False"></asp:Label>
            <asp:Label ID="lblRisultati" runat="server" Font-Bold="True" ></asp:Label></h1>

<asp:SqlDataSource ID="sdsCategorie" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
        ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
        SelectCommand="SELECT id, Codice, Descrizione, SettoriCodice, SettoriDescrizone FROM vCategorieSettori WHERE ((Abilitato = ?Abilitato) AND (ID = ?ID)) ORDER BY Ordinamento, Descrizione" EnableViewState="False">
        <SelectParameters>
            <asp:Parameter DefaultValue="1" Name="Abilitato" Type="Int32" />
            <asp:SessionParameter Name="ID" SessionField="ct" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>


    <asp:SqlDataSource ID="sdsTipologie" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
        ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
        SelectCommand="SELECT * FROM vcategorietipologie WHERE ((Abilitato = ?Abilitato) AND (SettoriId = ?SettoriId) AND (CategorieId = ?CategorieId)) ORDER BY Ordinamento, Descrizione" EnableViewState="False">
        <SelectParameters>
            <asp:Parameter DefaultValue="1" Name="Abilitato" Type="Int32" />
             <asp:SessionParameter Name="SettoriId" SessionField="st" Type="Int32" />
            <asp:SessionParameter Name="CategorieId" SessionField="ct" Type="String" />
            <asp:SessionParameter Name="NListino" SessionField="listino" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    
    <asp:SqlDataSource ID="sdsGruppo" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
        ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
        SelectCommand="SELECT * FROM vcategoriegruppi WHERE ((Abilitato = ?Abilitato) AND (SettoriId = ?SettoriId) AND (CategorieId = ?CategorieId)) ORDER BY Ordinamento, Descrizione" EnableViewState="False">
        <SelectParameters>
            <asp:Parameter DefaultValue="1" Name="Abilitato" Type="Int32" />
             <asp:SessionParameter Name="SettoriId" SessionField="st" Type="Int32" />
            <asp:SessionParameter Name="CategorieId" SessionField="ct" Type="String" />
            <asp:SessionParameter Name="NListino" SessionField="listino" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="sdsSottogruppo" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
        ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
        SelectCommand="SELECT * FROM vcategoriesottogruppi WHERE ((Abilitato = ?Abilitato) AND (SettoriId = ?SettoriId) AND (CategorieId = ?CategorieId)) ORDER BY Ordinamento, Descrizione" EnableViewState="False">
        <SelectParameters>
            <asp:Parameter DefaultValue="1" Name="Abilitato" Type="Int32" />
             <asp:SessionParameter Name="SettoriId" SessionField="st" Type="Int32" />
            <asp:SessionParameter Name="CategorieId" SessionField="ct" Type="String" />
            <asp:SessionParameter Name="NListino" SessionField="listino" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource><asp:SqlDataSource ID="sdsMarche" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
        ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
        SelectCommand="SELECT * FROM vcategoriemarche WHERE Abilitato=@Abilitato AND SettoriId=?SettoriId AND CategorieId=?CategorieId ORDER BY Ordinamento, Descrizione" EnableViewState="False">
        <SelectParameters>
            <asp:Parameter DefaultValue="1" Name="Abilitato" Type="Int32" />
            <asp:SessionParameter Name="SettoriId" SessionField="st" Type="Int32" />
            <asp:SessionParameter Name="CategorieId" SessionField="ct" Type="String" />
            <asp:SessionParameter Name="NListino" SessionField="listino" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
<div class="container">
	 <div class="row mt-3" runat="server" id="tNavig">

		<div id="filtersMr" class="col-12 col-md-6" style="padding-right: 2px; padding-left: 2px;">
			<asp:DataList ID="DataList4" runat="server" DataSourceID="sdsMarche" RepeatLayout="Flow" Font-Size="8pt" >
				<SelectedItemStyle Font-Bold="True" />
				<HeaderTemplate>
					<div style=" font-weight:bold; font-size:10pt; background-position: left top; color:White; background-image:url('Public/Images/back.jpg'); background-repeat:repeat-x; text-align:center;">
						MARCHE
					</div>
					<asp:Label ID="Label2" runat="server" Text=": :" Font-Bold="true" ForeColor="#E12825"></asp:Label>&nbsp;&nbsp;&nbsp;<asp:HyperLink CssClass='filterRemoveAll' ID="hlTutti" runat="server" NavigateUrl='<%# Me.Request.Url.toString & "&rimuovi=mr" %>' Text="Rimuovi tutti" ></asp:HyperLink>
				</HeaderTemplate>
				<ItemTemplate>
					<%# if(filterIdsContains("mr",Eval("marcheid").toString),"<b>","") %>
					<asp:CheckBox ID='CheckBoxMr' checked='<%# if(filterIdsContains("mr",Eval("marcheid").toString),true,false) %>' runat='server' AutoPostBack='True' OnCheckedChanged ='CheckBoxMr_CheckedChanged' filterId='<%# Eval("marcheid") %>' CssClass='filterCheckbox' Text='<%# getCorrectLengthDescription(Eval("Descrizione")) & " " & "<font color=#E12825>("& Eval("Numero") &")</font>"  %>' Width='150px' ToolTip='Applica/Rimuovi Filtro'/></asp:CheckBox> 
					<%# if(filterIdsContains("mr",Eval("marcheid").toString),"</b>","") %>
				</ItemTemplate>
			</asp:DataList>
		</div>
		
		<div id="filtersTp" class="col-12 col-md-6" style="padding-right: 2px; padding-left: 2px;">
		
			<asp:DataList ID="DataList1" runat="server" DataSourceID="sdsTipologie" RepeatLayout="Flow" Font-Size="8pt" >
				<ItemTemplate>
				<%# if(filterIdsContains("tp",Eval("TipologieId").toString),"<b>","") %>
				<asp:CheckBox ID='CheckBoxTp' checked='<%# if(filterIdsContains("tp",Eval("TipologieId").toString),true,false) %>' runat='server' AutoPostBack='True' OnCheckedChanged ='CheckBoxTp_CheckedChanged' filterId='<%# Eval("TipologieId") %>' CssClass='filterCheckbox' Text='<%# getCorrectLengthDescription(Eval("Descrizione")) & " " & "<font color=#E12825>("& Eval("Numero") &")</font>"  %>' Width='150px' ToolTip='Applica/Rimuovi Filtro'/></asp:CheckBox> 
				<%# if(filterIdsContains("tp",Eval("TipologieId").toString),"</b>","") %>
				</ItemTemplate>
				<HeaderTemplate>
				<div style=" font-weight:bold; font-size:10pt; background-position: left top; color:White; background-image:url('Public/Images/back.jpg'); background-repeat:repeat-x; text-align:center;">
					TIPOLOGIE
				</div>
				<asp:Label ID="Label2" runat="server" Text=": :" Font-Bold="true" ForeColor="#E12825"></asp:Label>&nbsp;&nbsp;&nbsp;<asp:HyperLink CssClass='filterRemoveAll' ID="hlTutti" runat="server" NavigateUrl='<%# Me.Request.Url.toString & "&rimuovi=tp" %>' Text="Rimuovi tutti" ></asp:HyperLink>
				</HeaderTemplate>
				<SelectedItemStyle Font-Bold="True" />
			</asp:DataList>
		</div>
		
		<div id="filtersGr" class="col-12 col-md-6" style="padding-right: 2px; padding-left: 2px;">
		
			<asp:DataList ID="DataList2" runat="server" DataSourceID="sdsGruppo" RepeatLayout="Flow" Font-Size="8pt">
				<ItemTemplate>
				<%# if(filterIdsContains("gr",Eval("GruppiId").toString),"<b>","") %>
				<asp:CheckBox ID='CheckBoxGr' checked='<%# if(filterIdsContains("gr",Eval("GruppiId").toString),true,false) %>' runat='server' AutoPostBack='True' OnCheckedChanged ='CheckBoxGr_CheckedChanged' filterId='<%# Eval("GruppiId") %>' CssClass='filterCheckbox' Text='<%# getCorrectLengthDescription(Eval("Descrizione")) & " " & "<font color=#E12825>("& Eval("Numero") &")</font>"  %>' Width='150px' ToolTip='Applica/Rimuovi Filtro'/></asp:CheckBox> 
				<%# if(filterIdsContains("gr",Eval("GruppiId").toString),"</b>","") %>
				</ItemTemplate>
				 <HeaderTemplate>
				 <div style=" font-weight:bold; font-size:10pt; background-position: left top; color:White; background-image:url('Public/Images/back.jpg'); background-repeat:repeat-x; text-align:center;">
					 GRUPPO
				 </div>
				<asp:Label CssClass='filterRemoveAll' ID="Label2" runat="server" Text=": :" Font-Bold="true" ForeColor="#E12825"></asp:Label>&nbsp;&nbsp;<asp:HyperLink ID="hlTutti" runat="server" NavigateUrl='<%# Me.Request.Url.toString & "&rimuovi=gr" %>' Text="Rimuovi tutti" ></asp:HyperLink>
				</HeaderTemplate>
				<SelectedItemStyle Font-Bold="True" />
			</asp:DataList>
		</div>
		
		<div id="filtersSg" class="col-12 col-md-6" style="padding-right: 2px; padding-left: 2px;">
		
			<asp:DataList ID="DataList3" runat="server" DataSourceID="sdsSottogruppo" RepeatLayout="Flow" Font-Size="8pt">
				<ItemTemplate>
				<%# if(filterIdsContains("sg",Eval("SottogruppiId").toString),"<b>","") %>
				<asp:CheckBox ID='CheckBoxSg' checked='<%# if(filterIdsContains("sg",Eval("SottogruppiId").toString),true,false) %>' runat='server' AutoPostBack='True' OnCheckedChanged ='CheckBoxSg_CheckedChanged' filterId='<%# Eval("SottogruppiId") %>' CssClass='filterCheckbox' Text='<%# getCorrectLengthDescription(Eval("Descrizione")) & " " & "<font color=#E12825>("& Eval("Numero") &")</font>"  %>' Width='150px' ToolTip='Applica/Rimuovi Filtro'/></asp:CheckBox> 
				<%# if(filterIdsContains("sg",Eval("SottogruppiId").toString),"</b>","") %>
				
				</ItemTemplate>
				  <HeaderTemplate>
				  <div style=" font-weight:bold; font-size:10pt; background-position: left top; color:White; background-image:url('Public/Images/back.jpg'); text-align:center;">
					  SOTTOGRUPPI
				  </div>
				<asp:Label CssClass='filterRemoveAll' ID="Label2" runat="server" Text=": :" Font-Bold="true" ForeColor="#E12825"></asp:Label>&nbsp;&nbsp;<asp:HyperLink ID="hlTutti" runat="server" NavigateUrl='<%# Me.Request.Url.toString & "&rimuovi=sg" %>' Text="Rimuovi tutti" ></asp:HyperLink>
				</HeaderTemplate>
				<SelectedItemStyle Font-Bold="True" />
			</asp:DataList>
		</div>
	</div>
</div>

    <!--<hr size="2" id="HR1"/>-->
   
   
    <asp:SqlDataSource ID="sdsArticoli" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
        ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
        SelectCommand="SELECT id, Codice, Descrizione1, PrezzoAcquisto, Img1, DescrizioneLunga FROM varticolibase ORDER BY Codice, Descrizione1" EnableViewState="False">
    </asp:SqlDataSource>
    
    <center>
        <br />
    <asp:Label ID="lblTrovati" runat="server" Font-Bold="True" ForeColor="#E12825" ></asp:Label>
        articoli trovati <i>(<asp:Label ID="lblLinee" runat="server" Text="0" Font-Size="8pt">&gt;</asp:Label> 
        per pagina)<br />
    </i>
    </center>
    
    <!-- Oridinamento Articoli -->
    <br />
    <br />
    <div class="container">
		<div class="row">
			<div class="form-group col-md-6">
				<asp:CheckBox ID="CheckBox_Disponibile" runat="server" Text="Solo Disponibili" Width="150px" AutoPostBack="True" style="float:left;font-size:10pt;margin-top:2pt;margin-left: 15px;" CssClass='filterCheckbox availablesOnly' />
			</div>
			
			<div class="form-group col-md-6">
				<span style="width:240px;font-size:10pt;display:inline-block"/>
				<span style="width:90px;display:inline-block;text-align:right"/>Ordina per</span>
				<asp:DropDownList ID="Drop_Ordinamento" style="vertical-align:middle" runat="server" Width="140px" AutoPostBack="True" BackColor="#FFFF80" Font-Bold="False" Font-Size="10pt" ForeColor="Black">
					<asp:ListItem Value="P_offerta">offerta</asp:ListItem>
					<asp:ListItem Value="P_basso">prezzo più basso</asp:ListItem>
					<asp:ListItem Value="P_alto">prezzo più alto</asp:ListItem>
					<asp:ListItem Value="P_recenti">più recenti</asp:ListItem>
					<asp:ListItem Value="P_popolarit&#224;">popolarità</asp:ListItem>
				</asp:DropDownList>
				</span>
			</div>
		</div>
		
		<div id="filtritagliaecolore" runat="server">			
			<div class="row">
				<div class="form-group col-md-6">
					<span style="width:240px;font-size:10pt;display:inline-block"/>
					<span style="width:90px;display:inline-block;text-align:right"/>Filtra taglia</span>
					<asp:DropDownList ID="Drop_Filtra_Taglia" style="text-align:left;vertical-align:middle" runat="server" Width="140px" AutoPostBack="True" BackColor="#FFFF80" Font-Bold="False" Font-Size="10pt" ForeColor="Black">
						<asp:ListItem Value="P_tutte_taglie">Tutte</asp:ListItem>
					</asp:DropDownList>
					</span>
				</div>
				<div class="form-group col-md-6">
					<span style="width:240px;font-size:10pt;display:inline-block"/>
					<span style="width:90px;display:inline-block;text-align:right"/>Filtra colore</span>
					<asp:DropDownList ID="Drop_Filtra_Colore" style="text-align:left;vertical-align:middle" runat="server" Width="140px" AutoPostBack="True" BackColor="#FFFF80" Font-Bold="False" Font-Size="10pt" ForeColor="Black">
						<asp:ListItem Value="P_tutti_colori">Tutti</asp:ListItem>
					</asp:DropDownList>	
					</span>					
				</div>
			</div>
		</div>
	</div>
	
	
    <div class="bg-white">
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataKeyNames="id"
        DataSourceID="sdsArticoli" AllowPaging="True" Font-Size="8pt" GridLines="None" CellPadding="3" Width="100%" style=" z-index:-1;" ShowFooter="True" ShowHeader="False">
        <Columns>      
            <asp:TemplateField HeaderText="Ordina &gt;&gt;" >
                <ItemTemplate>
                    <!-- Nuovo Articolo -->
                    <div class="container-fluid bg-white">
                        <div class="row mt-4">
                            <div class="col-12 colore_sito" style="background-color:rgb(224, 224, 224); border-style:none; padding:5px;">
                                <!-- Titolo -->
                                <asp:Label ID="Label12" runat="server" Text=' <%# Eval("MarcheDescrizione") %>' Font-Size="11pt" Font-Bold="true" Height="10px" style="display:inline;"></asp:Label>
                                <asp:HyperLink ID="HyperLink5"  ToolTip='<%# Eval("SettoriDescrizione") &" > "&  Eval("CategorieDescrizione") &" > "& Eval("MarcheDescrizione") &" > "& Eval("TipologieDescrizione") &" > "&  Eval("GruppiDescrizione") &" > "&  Eval("SottogruppiDescrizione") &" > "&  Eval("Codice") %>' runat="server" NavigateUrl='<%# "~/articolo.aspx?id="& Eval("id") &"&TCid="& Eval("TCid") %>'>
                                <asp:Label ID="Label8" runat="server" Text='<%# " - " & Eval("Descrizione1") %>' Font-Size="9pt" Font-Bold="true" ForeColor="Black"></asp:Label>
                                </asp:HyperLink>
                            </div>
                        </div>
                        <div class="row mt-2">
                            <div class="col-12">
                                <a href='<%# "articoli.aspx?tp=" & Eval("TipologieId") & "&mr=" & Eval("MarcheId") %>'>
									<span style="font-size:7pt;"> in <%# Eval("SettoriDescrizione") &" > "&  Eval("CategorieDescrizione") &" > "& Eval("MarcheDescrizione")%></span>
								</a>
                                <div style="float:right; width:auto; font-size:7pt; color:rgb(24, 40, 156);">
                                    <asp:Label ID="Label5" runat="server" Text="Codice: " ForeColor="black"></asp:Label><asp:Label ID="Label3" runat="server" Text='<%# Eval("Codice") %>' Font-Bold="true"></asp:Label>&nbsp;&nbsp;<asp:Label ID="Label6" runat="server" Text="EAN: " ForeColor="black"></asp:Label><asp:Label ID="Label7" runat="server"  Text='<%# Eval("Ean") %>' Font-Bold="true"></asp:Label>
                                </div>
                            </div>
                        </div>
                        <div class="row mt-2">
							<div class="col-12 col-md-3">
								<div style="text-align: center;">
									<div style="padding:5px">
										<!-- Immagine Prodotto -->
										<asp:HyperLink ID="HyperLink3" runat="server" NavigateUrl='<%# "~/articolo.aspx?id="& Eval("id") &"&TCid="& Eval("TCid") %>' >
											<asp:Image ID="Image1" runat="server" style=" max-height:150px; max-width:100%;" AlternateText='<%# Eval("Descrizione1") %>' ImageUrl='<%# checkImg(Eval("img1"))%>' />
										</asp:HyperLink>
										<div style="position:absolute; right:0px; top:0px;">
											<asp:Image ID="img_offerta" runat="server" ImageUrl="~/Public/Images/bollinoPromoVetrina.png" Visible="False"/>
										</div>
									</div>
									<div class="mt-3">
										<!-- Marca -->
										<asp:Image ID="Image2" runat="server" style="margin:auto auto 0 auto; max-width:100px; max-height:45px;" AlternateText='<%# Eval("MarcheDescrizione") %>' ImageUrl='<%# Eval("Marche_img", "~/Public/Marche/{0}") %>' visible='<%# controllo_img(Eval("Marche_img")) %>'/>
									</div>
									<!-- Controllo se esiste l'immagine -->
									<script runat="server">
										Function controllo_img(ByVal temp) As String
											If IsDBNull(temp) Then
												Return "false"
											Else
												Return "true"
											End If
										End Function
										
										Function checkImg(ByVal imgname As String) As String
											If imgname <> "" Then
												Return "public/foto/_" & imgname
											Else
												Return "Public/Foto/img_non_disponibile.png"
											End If
										End Function
									</script>
								</div>
							</div>
							<div class="col-12 col-md-9">

                                    <div class="row">
                                        <div class="col-12" style="border-style:none; padding:5px; text-align:left;">
                                            <!-- Descrizione Breve -->
                                            <script runat="server">
                                                Function sotto_stringa(ByVal temp As String) As String
                                                    temp = Server.HtmlEncode(temp)
                                                    Return Left(temp.Replace("&#160;", " "), 200) & " ..."
                                                End Function
                                            </script>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("Descrizione2")%>' Font-Size="11px" style="text-align:justify;" wrap="true" EnableTheming="False" EnableViewState="False"></asp:Label>
                                            </br>
                                            <!-- <asp:Label ID="tagliecolori" runat="server" Text='<%# Eval("taglia") & " " & Eval("colore") %>'></asp:Label> -->
                                        </div>
                                    </div>
                                    <div class="row">
																				
                                        <!-- Info Articolo + Prezzo -->
                                        <div class="col-12 col-md-5 col-1-articolo">
                                            <asp:Label ID="Label13" runat="server" Text="ID Art.: " Font-Bold="True" Visible="false"></asp:Label><asp:Label ID="Label_idArticolo" runat="server"  Text='<%# Eval("id") %>' ForeColor="white" Visible="false"></asp:Label>
                                            
                                            <table style="vertical-align:middle; font-size:6pt; text-align:left; margin:auto;">
                                                <tr>
                                                    <td>
                                                        <!-- PROMO -->
                                                        <asp:SqlDataSource ID="sdsPromo" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>" ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"  SelectCommand="SELECT * FROM vsuperarticoli WHERE ID=?ID AND NListino=?NListino ORDER BY PrezzoPromo DESC" EnableViewState="False">
                                                            <SelectParameters>
                                                                <asp:ControlParameter Name="ID" ControlID="tbID" PropertyName="Text" Type="Int32" />
                                                                <asp:SessionParameter Name="NListino" SessionField="listino" Type="Int32" />
                                                            </SelectParameters>
                                                        </asp:SqlDataSource>

                                                        <table visible='<%# Eval("InOfferta")%>' style="width:100%; border-style:none; padding:3px; vertical-align:middle; border-style:dotted; border-width:1px; border-color:Gray;" cellspacing="0">
                                                            <tr>
                                                                <td colspan="5" style="text-align:center; font-weight:bold; background-color:Gray; color:White; padding:2px;">
                                                                    <%#IIf(Eval("InOfferta") = 1, "PROMO", "NESSUNA PROMO")%>
                                                                </td>
                                                            </tr>
                                                            <asp:Repeater ID="rPromo" runat="server" DataSourceID="sdsPromo" EnableViewState="false" OnItemDataBound="rPromo_ItemDataBound">
                                                            <ItemTemplate>
                                                            <%Session("InOfferta") = 1%>
                                                                <tr>
                                                                    <td>
                                                                       <asp:Label ID="lblOfferta" runat="server" Visible='<%# Eval("InOfferta")%>' Font-Size="6pt" Text="" ForeColor="Black"></asp:Label> 
                                                                    </td>
                                                                    <td style="font-size:7pt; font-weight:bold; color:Red; padding-left:5px;">
                                                                        <asp:Label ID="lblQtaMin" runat="server" Text='<%# Eval("OfferteQntMinima") %>' Visible="false"></asp:Label>
                                                                        <asp:Label ID="lblMultipli" runat="server" Text='<%# Eval("OfferteMultipli") %>' Visible="false"></asp:Label>
                                                                    </td>
                                                                    <td style="padding-left:5px;">
                                                                        PZ.
                                                                    </td>
                                                                    <td style="padding-left:5px;">
                                                                        A
                                                                    </td>
                                                                    <td style="font-size:7pt; font-weight:bold; color:Red; padding-left:5px;">
                                                                        <asp:Label ID="lblPrezzoPromo" runat="server" Text='<%# "&#8364;" & FormatNumber(Eval("PrezzoPromo"), 2) %>' Visible="false"></asp:Label>
                                                                        <asp:Label ID="lblPrezzoPromoIvato" runat="server" Text='<%# "&#8364;" & FormatNumber(Eval("PrezzoPromoIvato"), 2) %>' Visible="false"></asp:Label>     
                                                                    </td>
                                                                    <asp:Label ID="lblInOfferta" runat="server" Text='<%# Eval("InOfferta") %>' Visible="false"></asp:Label>
                                                                </tr>
                                                            </ItemTemplate>
                                                            </asp:Repeater>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>   

											<div class="my-3">
												<div class="" style="min-inline-size: fit-content; width: 175px; margin: 0 auto;">
													<!-- Spedizione Gratis -->
													<asp:Label ID="lblID" runat="server" Text='<%# Bind("ID") %>' style="z-index:-1;" Visible="false"></asp:Label>
													<asp:Label ID="lblTCID" runat="server" Text='<%# Bind("TCID") %>' style="z-index:-1;" Visible="false"></asp:Label>
													<asp:SqlDataSource ID="sdsSpedizioneGratis" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
														ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
														SelectCommand="SELECT SpedizioneGratis_Listini, SpedizioneGratis_Data_Inizio, SpedizioneGratis_Data_Fine, articoli.id FROM articoli LEFT OUTER JOIN articoli_tagliecolori on articoli.id = articoli_tagliecolori.ArticoliId WHERE (SpedizioneGratis_Listini LIKE CONCAT('%', @Param1, ';%')) AND (articoli.id = @Param2) AND (articoli_tagliecolori.id = @Param3  OR articoli_tagliecolori.id IS NULL) AND (SpedizioneGratis_Data_Inizio <= CURDATE()) AND (SpedizioneGratis_Data_Fine >= CURDATE())">
														<SelectParameters>
															<asp:SessionParameter Name="Param1" SessionField="Listino" />
															<asp:ControlParameter ControlID="lblID" Name="Param2" PropertyName="Text" />
															<asp:ControlParameter ControlID="lblTCID" Name="Param3" PropertyName="Text" />
														</SelectParameters>
													</asp:SqlDataSource>
													<asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" DataSourceID="sdsSpedizioneGratis" BorderWidth="0px" ShowHeader="False" style="float:left; vertical-align:middle;">
														<Columns>
															<asp:TemplateField>
																<ItemTemplate>
																	<img style="border-width:0px; height:30px; margin-top:5px;" src="Images/spedizione_gratis.png" title='Questo articolo verrà spedito GRATIS !!! fino al <%# Eval("SpedizioneGratis_Data_Fine","{0:d}") %>' alt="" />
																</ItemTemplate>
															</asp:TemplateField>
														</Columns>
														<RowStyle BorderColor="White" BorderWidth="0px" />
														<PagerStyle BorderColor="White" BorderWidth="0px" />
													</asp:GridView>
													<!-- --------------------------------------------------------------------------------------------------- -->
													<!-- Icone Proprietà -->
													<img src="Images/refurbished.png" title="Articolo ricondizionato" alt="" style="height:30px; float:left; padding:5px 0px 0px 0px;visibility:<%# Eval("refurbished")%>" /> 
													<asp:LinkButton ID="LB_wishlist" runat="server" OnClick="BT_Aggiungi_wishlist_Click" style="float:left;  padding:5px;"><img src="Images/wishlist.png" title="Aggiungi a Wishlist" alt="" style="height:30px" /></asp:LinkButton>
													<a href='<%# "articolo.aspx?id="& Eval("id") &"&TCid="& Eval("TCid") %>' style="float:left; padding:5px;"><img src="Images/scheda_tecnica.png" title="Scheda Tecnica" alt="" style="height:30px;" /></a>
													<a href="https://wa.me/?text=<%# Eval("Descrizione1") %> - https://<%# Session("AziendaUrl") %>/articolo.aspx?id=<%# Eval("id") %>%26TCid=<%# Eval("TCid") %>" style="float:left; padding:5px;">
													<img src="https://<%# Session("AziendaUrl") %>/Public/Images/WhatsApp-Symbolo.png" alt="" height="30px" border="0">
													</a>
													<!-- AddThis Button BEGIN -->
													<!--
													<div class="addthis_toolbox addthis_default_style addthis_32x32_style" style="padding-top:4px;">
													<a class="addthis_button_preferred_1" addthis:url="<%# "http://" & Session("AziendaUrl") & "/articolo.aspx?id=" & Eval("id") &"&TCid="& Eval("TCid")%>"></a>
													<a class="addthis_button_compact"></a>
													</div>
													<script type="text/javascript">var addthis_config = {"data_track_addressbar":true};</script>
													<script type="text/javascript" src="//s7.addthis.com/js/300/addthis_widget.js#pubid=ra-52a5f0943d53948f"></script>
													-->
													<!-- AddThis Button END -->
												</div>
											</div>											
                                        </div>
                                        <div class="col-12 col-md-7 text-right" style="border-style:none; padding-left:5px;">
                                            <div class="col-md-12" style="border-bottom-style: dotted; border-top-style: dotted; border-width: 1px; padding: 0.3rem 0;">
												<asp:Label ID="lblDispo" runat="server" CssClass="ml-2" Font-Bold="True" Font-Size="6pt" Text="Disponibilità:"></asp:Label>
												<asp:Label ID="Label_dispo" runat="server" ForeColor="red" Font-Bold="true" Text='<%# iif(Eval("Giacenza")>1000,">1000",iif(Eval("Giacenza").toString.contains("-"),Eval("Giacenza").toString.Replace("-","&minus;"),Eval("Giacenza"))) %>'></asp:Label>
												<asp:Image ID="imgDispo" runat="server"/>
										   
												<asp:Label ID="lblImpegnata" runat="server" CssClass="ml-2" Font-Bold="True" Font-Size="6pt" Text="Impegnati:"></asp:Label>
												<asp:Label ID="Label_imp" runat="server" ForeColor="red" Font-Bold="true" Text='<%# iif(Val(Eval("Impegnata").toString)>1000,">1000", Val(Eval("Impegnata").toString)) %>'></asp:Label>
											
											    <!--sezione arrivi -->
												
												<span id="lblArrivo" class="ml-2" style="font-size:6pt;font-weight:bold;">
												
												In Arrivo:
												</span>
												<asp:Label ID="Label_arrivo" runat="server" ForeColor="red" Font-Bold="true" Text='<%# iif(Val(Eval("InOrdine").toString)>1000,">1000", Val(Eval("InOrdine").toString)) %>' style="padding-right"></asp:Label>
												<asp:Image ID="imgArrivo" runat="server" Visible="false"/>
											</div>
                                            <div>
                                            <!-- Sconto -->
                                            <asp:Panel ID="Panel_Visualizza_Percentuale_Sconto" runat="server" Visible="false" style="float:left;">
                                                <div style="padding:5px; height:61px; background-image:url('Images/sfondoOfferta.png'); background-position:center; background-repeat:no-repeat; color:White;">
                                                    <table style="height:61px; width:61px; vertical-align:middle; text-align:center;">
                                                        <tr>
                                                            <td>
                                                                <span style="font-size:9px;">SCONTO</span><br />
                                                                <asp:Label ID="sconto_applicato" runat="server" Text="" ForeColor="White" Font-Size="12pt" Font-Bold="true"></asp:Label>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </div>
                                            </asp:Panel>
                                            
                                            <span class="colore_sito" style="float:right;padding-top:3px; text-align:right;">
                                                <asp:Label ID="lblPrezzoPromo" runat="server" Width="100%" Text='<%# Eval("Prezzo", "{0:C}") %>' Visible='<%# Eval("InOfferta") %>' style="font-size:19pt; font-weight:bold; width:100%; text-align:right;"></asp:Label><br />
                                                <%  
                                                    If Session("InOfferta") = 1 Then
                                                        Session("InOfferta") = 0
                                                %>
                                                    <div>
                                                    <asp:Panel ID="Panel_in_offerta" runat="server" Height="15px" Width="150px" Visible="False" style="margin:0 0 0 auto;">
                                                        invece di
                                                    <asp:Label ID="Label4" runat="server" Text='<%# Bind("PrezzoIvato", "{0:C}") %>' ForeColor="Red" style="text-decoration:line-through;"></asp:Label>
                                                    <asp:Label ID="Label10" runat="server" Text='<%# Bind("Prezzo", "{0:C}") %>' Visible="False" ForeColor="Red" style="text-decoration:line-through;"></asp:Label></asp:Panel>
                                                    </div>
                                                <%Else%>
                                                    <asp:Label ID="lblPrezzoIvato" runat="server" Text='<%# Bind("PrezzoIvato", "{0:C}") %>' style="font-size:19pt; font-weight:bold;"></asp:Label>
                                                    <asp:Label ID="lblPrezzo" runat="server" Text='<%# Bind("Prezzo", "{0:C}") %>' Visible="false" style="font-size:19pt; font-weight:bold;"></asp:Label>
                                                <%  End If%>
                                                
                                                <br /><%#IIf(Eval("Giacenza") > 0, "<span style=""color:green; font-weight:bold; font-size:11pt;"">DISPONIBILE</span>", "<span style=""color:Red; font-weight:bold; font-size:12pt;"">NON DISPONIBILE</span>")%>
                                                
                                            </span>
											</div>
                                        </div>
                                    </div>
									<div class="row mt-3">
										<div class="col-12">
										<table style="vertical-align:middle; height:100%; float:right; display:block;">
											<tr>
											   <td>
													<asp:CheckBox ID="CheckBox_SelezioneMultipla" runat="server" BorderWidth="2" BorderColor="#CCCCCC" BorderStyle="Solid" />
											   </td>
											   <td>
													<div class="d-flex" style="height:37px;background-color: lightgray;">
													<!--<span style="font-size: 13px; font-weight: bold;">Quantità</span>-->
														<i data-qty-action="decrementQty" style="color: #383838;font-size:16px;" class="fa fa-minus-circle fa-2x align-self-center mx-1"></i>
														<asp:TextBox ID="tbQuantita" runat="server" Width="50px" style="text-align:center;font-size: 13px; font-weight: bold;" MaxLength="4" Pattern="\d*">1</asp:TextBox>
														<i data-qty-action="incrementQty" style="color: #383838;font-size:16px;" class="fa fa-plus-circle fa-2x align-self-center mx-1"></i>									   
										</div>
											   </td>
											   <td>
													<div class="d-flex" style="position: relative;">
													  <div style="background-image: url(../../Images/back_menu.png);<%# IIf (Eval("Giacenza") > 0,"background-color: #70db10;" ,"background-color: #e02020;") %>; color: white; position: absolute;height:37px; width:180px; text-align: center; vertical-align: middle;line-height: 37px; font-size: 13px; font-weight: bold;"><i class="fas fa-cart-plus" style="font-size: 16px;"></i>&nbsp;&nbsp;Aggiungi al carrello</div>
													  <div style="z-index: 10;height:37px; width:180px;">
														<asp:ImageButton ID="ImageButton2" style="border: none;height:37px; width:180px;" runat="server" ImageUrl="Public/Images/spazio_vuoto.gif" ToolTip="Aggiungi al Carrello" OnClick="ImageButton1_Click" />
													  </div>
													</div>
													
											   </td>
											</tr>
										</table>
										<asp:HyperLink ID="HyperLink2" Visible="false" runat="server" ImageUrl='<%# "images/cart.gif" %>'  Text="Scheda Prodotto"></asp:HyperLink>
										<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="tbQuantita" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True"></asp:RequiredFieldValidator>
										<asp:CompareValidator ID="CompareValidator2" runat="server" ControlToValidate="tbQuantita" Display="Dynamic"  ErrorMessage="!" Operator="GreaterThan" SetFocusOnError="True"  Type="Integer" ValueToCompare="0"></asp:CompareValidator>
										</div>
									</div>
                                    <div class="row" style="display:none;">
                                        
										<asp:TextBox ID="tbID" runat="server" Text='<%# Eval("ID") %>' Width="30" EnableViewState="false" Visible="false" ></asp:TextBox>
										<asp:TextBox ID="tbInOfferta" runat="server" Text='<%# Eval("InOfferta") %>' Width="30" EnableViewState="false" Visible="false" ></asp:TextBox>
									
                                    </div>

                           </div>
                        </div>
                    </div>
         </ItemTemplate>
         <FooterTemplate>
             <img src="Public/Images/selection.gif" style="max-width:100%" alt=""/>
             &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                    <asp:ImageButton ID="Selezione_Multipla" runat="server" title="Aggiungi gli articoli selezionati al carrello" OnClick="Selezione_Multipla_Click" ImageUrl="~/Public/Images/aggiungiMultiplo.png" />
                </FooterTemplate>
                <FooterStyle BackColor="#CCCCCC" HorizontalAlign="Center" VerticalAlign="Middle" />
      </asp:TemplateField>
     </Columns>
        <PagerStyle CssClass="pagination-ys" />
		<PagerSettings Mode="NumericFirstLast" FirstPageText="Inizio" LastPageText="Fine" />
    </asp:GridView>
    </div>
	
    <asp:Label ID="lblPrezzi" runat="server" Text="*Prezzi" Font-Size="7pt" Font-Names="arial"></asp:Label><br /><br />
    
	<script type="text/javascript">

		$(function() {
		  $("[id*=CheckBoxMr]").click(disable_checkbox);
		  $("[id*=CheckBoxTp]").click(disable_checkbox);
		  $("[id*=CheckBoxGr]").click(disable_checkbox);
		  $("[id*=CheckBoxSg]").click(disable_checkbox);
		});
		
		function disable_checkbox() {
			$('#filtersMr').fadeTo('fast',.6);
			$('#filtersMr').append('<div style="position: absolute;top:0;left:0;width: 100%;height:100%;z-index:2;opacity:0.4;filter: alpha(opacity = 50)"></div>');
			$('#filtersTp').fadeTo('fast',.6);
			$('#filtersTp').append('<div style="position: absolute;top:0;left:0;width: 100%;height:100%;z-index:2;opacity:0.4;filter: alpha(opacity = 50)"></div>');
			$('#filtersGr').fadeTo('fast',.6);
			$('#filtersGr').append('<div style="position: absolute;top:0;left:0;width: 100%;height:100%;z-index:2;opacity:0.4;filter: alpha(opacity = 50)"></div>');
			$('#filtersSg').fadeTo('fast',.6);
			$('#filtersSg').append('<div style="position: absolute;top:0;left:0;width: 100%;height:100%;z-index:2;opacity:0.4;filter: alpha(opacity = 50)"></div>');
		}

	</script>
	
</asp:Content>

