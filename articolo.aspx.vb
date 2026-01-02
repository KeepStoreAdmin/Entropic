Imports MySql.Data.MySqlClient
Imports System.Data

Partial Class Articolo
    Inherits System.Web.UI.Page

    Public IvaTipo As Integer
    Dim DispoTipo As Integer
    Dim DispoMinima As Integer
    Dim requestId As Integer
    Dim requestTCId As Integer
	Public firstName As String
	Public lastName As String
	Public email As String
	Public phone As String
	Public country As String
	Public province As String
	Public city As String
	Public cap As String
	Public facebook_pixel_id As String
	Public utenteId As String
	Public idsFbPixelsSku as New Dictionary(of String, String)
	
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

		Dim articoloId As String = Request.QueryString("id")
		
        If Not (Integer.TryParse(articoloId, requestId) And Integer.TryParse(Request.QueryString("TCId"), requestTCId)) Then
            Response.Redirect("default.aspx")
        End If

        IvaTipo = Me.Session("IvaTipo")
        DispoTipo = Me.Session("DispoTipo")
        DispoMinima = Me.Session("DispoMinima")

        'Redirect nel caso c'è la presenza di #up
        If Request.Url.AbsoluteUri.Contains("%23up") Or Request.Url.AbsoluteUri.Contains("#23up") Then
            Response.Redirect(Request.Url.AbsoluteUri.Replace("%23up", "").Replace("#23up", ""))
        End If

        'Nella query devo controllare che i valori di Descrizione Lunga e Descrizione Html non siano NULL altrimenti si genera un'errore sui valori del campo
        'sdsArticolo.SelectCommand = "SELECT id, Codice, Ean, Descrizione1, Descrizione2, IF(DescrizioneLunga IS NULL,'',DescrizioneLunga) AS DescrizioneLunga, IF(DescrizioneHTML IS NULL,'',DescrizioneHTML) AS DescrizioneHTML, ArticoliIva, Prezzo, IF(" & Session("Iva_Utente") & ">-1,((Prezzo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoIvato) AS PrezzoIvato, Img1, MarcheDescrizione, Disponibilita, Giacenza, InOrdine, Impegnata, InOfferta, SettoriDescrizione, CategorieDescrizione, TipologieDescrizione, GruppiDescrizione, SottogruppiDescrizione, MarcheDescrizione, Marche_img, PrezzoPromo, IF(" & Session("Iva_Utente") & ">-1,((PrezzoPromo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoPromoIvato) AS PrezzoPromoIvato, MarcheId, CategorieId, TipologieId, IF(PrezzoPromo IS NULL,Prezzo,PrezzoPromo) Ord_PrezzoPromo, IF(PrezzoPromoIvato IS NULL,PrezzoIvato,IF(" & Session("Iva_Utente") & ">-1,((PrezzoPromo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoPromoIvato)) Ord_PrezzoPromoIvato, OfferteQntMinima, OfferteMultipli, OfferteDataInizio, OfferteDataFine, MarcheId, SettoriId, CategorieId, TipologieId, GruppiId, SottogruppiId, Img1, Img2, Img3, Img4, Peso, ListinoUfficiale, Brochure, LinkProduttore FROM vsuperarticoli WHERE (id = " & requestId & ") and (NListino = " & Session("listino") & ")"
        sdsArticolo.SelectCommand = "SELECT vsuperarticoli.id, vsuperarticoli.NoteRicondizionato, Mesi, Codice, Ean, Descrizione1, Descrizione2, SpeditoGratis, SpedizioneGratis_Data_Fine, DescrizioneIvaRC, IF(DescrizioneLunga IS NULL,'',DescrizioneLunga) AS DescrizioneLunga, IF(DescrizioneHTML IS NULL,'',DescrizioneHTML) AS DescrizioneHTML, ArticoliIva, Prezzo, IF((@AbilitatoIvaReverseCharge=1) AND (ValoreIvaRC>-1),((Prezzo)*((ValoreIvaRC/100)+1)),IF(@Iva_Utente>-1,((Prezzo)*((@Iva_Utente/100)+1)),PrezzoIvato)) AS PrezzoIvato, MarcheDescrizione, Disponibilita, Giacenza, InOrdine, Impegnata, InOfferta, SettoriDescrizione, CategorieDescrizione, TipologieDescrizione, ValoreIva, GruppiDescrizione, SottogruppiDescrizione, MarcheDescrizione, Marche_img, PrezzoPromo, IF((@AbilitatoIvaReverseCharge=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(@Iva_Utente>-1,((PrezzoPromo)*((@Iva_Utente/100)+1)),PrezzoPromoIvato)) AS PrezzoPromoIvato, MarcheId, CategorieId, TipologieId, IF(PrezzoPromo IS NULL,Prezzo,PrezzoPromo) Ord_PrezzoPromo, IF(PrezzoPromoIvato IS NULL,PrezzoIvato,IF((@AbilitatoIvaReverseCharge=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(@Iva_Utente>-1,((PrezzoPromo)*((@Iva_Utente/100)+1)),PrezzoPromoIvato))) Ord_PrezzoPromoIvato, OfferteQntMinima, OfferteMultipli, OfferteDataInizio, OfferteDataFine, MarcheId, SettoriId, CategorieId, TipologieId, GruppiId, SottogruppiId, Peso, ListinoUfficiale, Brochure, LinkProduttore, IdIvaRC, Visite, vsuperarticoli.TCid, IF(Ricondizionato = 1, 'visible', 'hidden') as refurbished,  CONVERT(CONCAT('<table style=""width:100%;"" border=""1""><tr style=""background-color:#00FF99;""><td>Data di arrivo</td><td>Quantit&agrave;</td></tr><tr style=""background-color:#00FFFF;""><td>',GROUP_CONCAT(arrivi SEPARATOR '</td></tr><tr style=""background-color:#00FFFF;""><td>'),'</td></tr></table>'),CHAR) as arrivi, tempiconsegna.descrizione as tempiconsegnadescrizione"
        Dim tc As Integer = Session("TC")
        If tc = 1 Then
            sdsArticolo.SelectCommand &= ", IFNULL(immagine1, '') as Img1, IFNULL(immagine2, '') as Img2, IFNULL(immagine3, '') as Img3, IFNULL(immagine4, '') as Img4, IFNULL(immagine5, '') as Img5, IFNULL(immagine6, '') as Img6 FROM vsuperarticoli"
            sdsArticolo.SelectCommand &= " Left Join articoli_tagliecolori on vsuperarticoli.TCId = articoli_tagliecolori.id"
            sdsArticolo.SelectCommand &= " Left Join immagini on articoli_tagliecolori.immaginiId = immagini.id"
			sdsArticolo.SelectCommand &= " LEFT OUTER JOIN tempiconsegnaperlivello ON tempiconsegnaperlivello.livellitipoid = GetArticoloLivelloTipoId(vsuperarticoli.id) and tempiconsegnaperlivello.livelloid = GetArticoloLivelloId(vsuperarticoli.id)"
			sdsArticolo.SelectCommand &= " LEFT OUTER JOIN tempiconsegna ON tempiconsegna.id = tempiconsegnaperlivello.tempiconsegnaid"
			sdsArticolo.SelectCommand &= " LEFT OUTER JOIN (SELECT articoliid, TCId, CONCAT(DATE_FORMAT(dataArrivo, '%d/%m/%Y'),'</td><td>', (TRIM(TRAILING '.' FROM(CAST(TRIM(TRAILING '0' FROM SUM(arrivi)) AS CHAR))))) AS arrivi FROM articoli_arrivi WHERE dataArrivo>NOW() and arrivi > 0 GROUP BY dataArrivo) arrivi ON arrivi.articoliid = vsuperarticoli.id and vsuperarticoli.TCId = arrivi.TCId"
            sdsArticolo.SelectCommand &= " WHERE vsuperarticoli.id = @requestId And vsuperarticoli.TCid = @requestTCId"
        Else
            sdsArticolo.SelectCommand &= ",Img1, Img2, Img3, Img4 as Img4, Img4 as Img5, Img4 as Img6 FROM vsuperarticoli"
			sdsArticolo.SelectCommand &= " LEFT OUTER JOIN tempiconsegnaperlivello ON tempiconsegnaperlivello.livellitipoid = GetArticoloLivelloTipoId(vsuperarticoli.id) and tempiconsegnaperlivello.livelloid = GetArticoloLivelloId(vsuperarticoli.id)"
			sdsArticolo.SelectCommand &= " LEFT OUTER JOIN tempiconsegna ON tempiconsegna.id = tempiconsegnaperlivello.tempiconsegnaid"
			sdsArticolo.SelectCommand &= " LEFT OUTER JOIN (SELECT articoliid, CONCAT(DATE_FORMAT(dataArrivo, '%d/%m/%Y'),'</td><td>', (TRIM(TRAILING '.' FROM(CAST(TRIM(TRAILING '0' FROM SUM(arrivi)) AS CHAR))))) AS arrivi FROM articoli_arrivi WHERE dataArrivo>NOW() and arrivi > 0 GROUP BY dataArrivo) arrivi ON arrivi.articoliid = vsuperarticoli.id"
            sdsArticolo.SelectCommand &= " WHERE vsuperarticoli.id = @requestId"
        End If
        sdsArticolo.SelectCommand &= " And NListino = @NListino"
        sdsArticolo.SelectParameters.Clear()
        sdsArticolo.SelectParameters.Add("@AbilitatoIvaReverseCharge", Session("AbilitatoIvaReverseCharge"))
        sdsArticolo.SelectParameters.Add("@Iva_Utente", Session("Iva_Utente"))
        sdsArticolo.SelectParameters.Add("@requestId", requestId)
        sdsArticolo.SelectParameters.Add("@requestTCId", requestTCId)
        sdsArticolo.SelectParameters.Add("@NListino", Session("listino"))
        'Aggiunta dei TAG per facebook (Condivisione dell'articolo)
        aggiungi_tag_Facebook()

        'Se l'articolo è stato visualizzato tramite BestShopping, mi salvo in Sessione la variabile che avvierà la procedura indicatomi da BestShopping nella pagina documentidettaglio.aspx
        If Request.QueryString("comparatore") = "BESTSHOPPING" Then
            Session("Tracking_BestShopping") = 1
        End If
		
		facebook_pixel(articoloId)
		
    End Sub

	Public Sub facebook_pixel(ByVal articoliId As String)

        If Session("utentiid") > -1 Then
            Dim params As New Dictionary(Of String, String)
            params.add("@id", Session("utentiid"))
            Dim dr = ExecuteQueryGetDataReader("ifnull(CognomeNome,'') as CognomeNome, RagioneSociale, ifnull(email,'') as email, coalesce(case when ifnull(cellulare,'') = '' then null else cellulare end,case when ifnull(telefono,'') = '' then null else telefono end,'') as telefono, ifnull(nazione,'') as nazione, ifnull(provincia,'') as provincia, ifnull(citta,'') as citta, ifnull(cap,'') as cap", "utenti", "WHERE id = @id", params)
            If dr.Count > 0 Then
                Dim row = dr(0)
                firstName = row("CognomeNome")
                lastName = row("RagioneSociale")
                email = row("email")
                phone = row("telefono")
                country = row("nazione")
                province = row("provincia")
                city = row("citta")
                cap = row("cap")
                Dim oldIdFbPixel As String = String.Empty
                Dim sku As String
                params.add("@aziendaId", Session("AziendaID"))
                Dim wherePart As String = "Left Join ks_fb_pixel_products on ks_fb_pixel_products.id_product = articoli.id "
                wherePart &= "Left Join ks_fb_pixel on ks_fb_pixel_products.id_fb_pixel = ks_fb_pixel.id "
                wherePart &= "WHERE articoli.id in (" & articoliId & ") And ks_fb_pixel.start_date<=CURDATE() And ks_fb_pixel.stop_date>CURDATE() And ks_fb_pixel.id_company = @aziendaId "
                wherePart &= "Order by ks_fb_pixel_products.id_fb_pixel"
                dr = ExecuteQueryGetDataReader("articoli.codice as sku, ks_fb_pixel.id_pixel", "articoli", wherePart, params)
                For Each subRow As Dictionary(Of String, Object) In dr
                    Dim newIdFbPixel As String = subRow("id_pixel")
                    If newIdFbPixel <> oldIdFbPixel Then
                        If oldIdFbPixel <> String.Empty Then
                            idsFbPixelsSku.Add(oldIdFbPixel, sku)
                        End If
                        oldIdFbPixel = newIdFbPixel
                        sku = String.Empty
                    Else
                        sku &= ","
                    End If
                    sku &= subRow("sku")
                Next
                If oldIdFbPixel <> String.Empty Then
                    idsFbPixelsSku.Add(oldIdFbPixel, sku)
                End If
            End If
        End If

    End Sub

    Public Sub aggiungi_tag_Facebook()
        Dim params As New Dictionary(Of String, String)
        params.add("@requestId", requestId)
        params.add("@NListino", Session("listino"))
        Dim dr = ExecuteQueryGetDataReader("*", "vsuperarticoli", "where ID=@requestId AND NListino=@NListino", params)
        If dr.Count > 0 Then
            Dim row = dr(0)
            'Aggiungo nella Header i metaTag relativi a Facebook
            Dim keywords As HtmlMeta = New HtmlMeta()
                keywords.Attributes("property") = "og:type"
                keywords.Content = "website"

                keywords = New HtmlMeta()
                keywords.Attributes("property") = "og:title"
                keywords.Content = row("Descrizione1")
                Page.Header.Controls.AddAt(0, keywords)

                keywords = New HtmlMeta()
                keywords.Attributes("property") = "og:image"
                keywords.Content = "http://" & Session("AziendaUrl") & "/Public/Foto/Facebook/facebook_" & row("Img1")
                Page.Header.Controls.AddAt(0, keywords)

                keywords = New HtmlMeta()
                keywords.Attributes("property") = "og:url"
                keywords.Content = Request.Url.AbsoluteUri
                Page.Header.Controls.AddAt(0, keywords)

                keywords = New HtmlMeta()
                keywords.Attributes("property") = "og:site_name"
                keywords.Content = "http://" & Session("AziendaUrl")
                Page.Header.Controls.AddAt(0, keywords)

                keywords = New HtmlMeta()
                keywords.Attributes("property") = "og:description"
                keywords.Content = row("Descrizione2")
                Page.Header.Controls.AddAt(0, keywords)

            'ApplicationID - Opzionale
            'keywords = New HtmlMeta()
            'keywords.Attributes("property") = "og:appID"
            'keywords.Content = "1405753389699057"
            'Page.Header.Controls.AddAt(0, keywords)


        End If
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        SettaTitolo()
        AggiornaVisite()
    End Sub

    Public Sub SettaTitolo()
        Try
            Dim lblDescrizione As Label
            Dim Codice As Label = Me.fvPage.FindControl("Label13")
            Dim EAN As Label = Me.fvPage.FindControl("Label15")
            lblDescrizione = Me.fvPage.FindControl("lblDescrizione")
            Me.Title = Me.Title & " > " & lblDescrizione.Text & " > Codice: " & Codice.Text & " > EAN: " & EAN.Text
        Catch ex As Exception
        End Try
    End Sub

    Public Sub SettaDisponibilita()
        Dim img As Image
        Dim dispo As Label
        Dim impegnato As Label
        Dim arrivo As Label
        Dim PrezzoDes As Label
        Dim Prezzo As Label
        Dim PrezzoIvato As Label
        Dim PrezzoPromo As Label
        Dim Qta As TextBox
        Try

            PrezzoDes = Me.fvPage.FindControl("lblPrezzoDes")
            Prezzo = Me.fvPage.FindControl("lblPrezzo")
            PrezzoIvato = Me.fvPage.FindControl("lblPrezzoIvato")
            PrezzoPromo = Me.fvPage.FindControl("lblPrezzoPromo")
            Qta = Me.fvPage.FindControl("tbQuantita")

            If IvaTipo = 1 Then
                PrezzoDes.Text = "Prezzo Iva Esclusa"
                'Prezzo.Visible = True
                'PrezzoIvato.Visible = False
            ElseIf IvaTipo = 2 Then
                PrezzoDes.Text = "Prezzo Iva Inclusa"
                'Prezzo.Visible = False
                'PrezzoIvato.Visible = True
            End If

            img = Me.fvPage.FindControl("imgDispo")
            dispo = Me.fvPage.FindControl("lblDispo")
            impegnato = Me.fvPage.FindControl("lblImpegnata")
            arrivo = Me.fvPage.FindControl("lblArrivo")

            If DispoTipo = 1 Then
                dispo.Visible = False
                If dispo.Text > DispoMinima Then
                    img.ImageUrl = "~/images/verde2.gif"
                    img.AlternateText = "Disponibile"
                ElseIf dispo.Text > 0 Then
                    img.ImageUrl = "~/images/giallo2.gif"
                    img.AlternateText = "Disponibilità Scarsa"
                Else
                    If arrivo.Text > 0 Then
                        img.ImageUrl = "~/images/azzurro2.gif"
                        img.AlternateText = "In Arrivo"
                    Else
                        img.ImageUrl = "~/images/rosso2.gif"
                        img.AlternateText = "Non Disponibile"
                    End If
                End If

            ElseIf DispoTipo = 2 Then
                img.Visible = False
                impegnato.Visible = True
                dispo.Visible = True
                arrivo.Visible = True
                Me.fvPage.FindControl("lblArr").Visible = True
                Me.fvPage.FindControl("lblImp").Visible = True
                Me.fvPage.FindControl("lblPunti1").Visible = True
                Me.fvPage.FindControl("lblPunti2").Visible = True
                Me.fvPage.FindControl("lblPunti3").Visible = True
            End If
        Catch
        End Try
    End Sub

    'Restituisce 1 se ci sono delle promo valide sull'articiolo altrimenti 0
    Function controlla_promo_articolo(ByVal cod_articolo As Integer, ByVal listino As Integer) As Integer
        Dim params As New Dictionary(Of String, String)
        params.add("@cod_articolo", cod_articolo)
        params.add("@NListino", listino)
        Dim dr = ExecuteQueryGetDataReader("id", "vsuperarticoli", "WHERE (ID=@cod_articolo AND NListino=@NListino) AND ((OfferteDataInizio <= CURDATE()) AND (OfferteDataFine >= CURDATE())) AND (InOfferta=1) ORDER BY PrezzoPromo DESC", params)

        'Restituisce 1 nel caso ci sia almeno una riga come risultato, e quindi il settore relativo all' IDArticolo è ABILITATO altrimenti restituisce 0
        If (dr.Count > 0) Then
            Return 1
        Else
            Return 0
        End If
    End Function

    Protected Sub fvPage_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles fvPage.PreRender
        Dim Prezzo As Label
        Dim PrezzoIvato As Label
        Dim dispo As Label

        Dim form As FormView
        Dim tb_id As TextBox
        Dim SQLDATA_Promo As SqlDataSource
        Dim prezzoPromo As Label

        form = CType(sender, FormView)

        prezzoPromo = form.FindControl("lblPrezzoPromo")
        tb_id = form.FindControl("tbid")
        SQLDATA_Promo = form.FindControl("sdsPromo")
        If Not tb_id Is Nothing Then
            If (controlla_promo_articolo(Val(tb_id.Text), Session("listino")) = 1) Then
                'Nella query devo controllare che i valori di Descrizione Lunga e Descrizione Html non siano NULL altrimenti si genera un'errore sui valori del campo
                SQLDATA_Promo.SelectCommand = "SELECT id, Codice, Ean, Descrizione1, Descrizione2, NoteRicondizionato, IF(DescrizioneLunga IS NULL,'',DescrizioneLunga) AS DescrizioneLunga, IF(DescrizioneHTML IS NULL,'',DescrizioneHTML) AS DescrizioneHTML, Prezzo, IF((@AbilitatoIvaReverseCharge=1) AND (ValoreIvaRC>-1),((Prezzo)*((ValoreIvaRC/100)+1)),IF((@AbilitatoIvaReverseCharge=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(@Iva_Utente>-1,((Prezzo)*((@Iva_Utente/100)+1)),PrezzoIvato))) AS PrezzoIvato, Img1, MarcheDescrizione, Disponibilita, Giacenza, InOrdine, Impegnata, InOfferta, SettoriDescrizione, CategorieDescrizione, TipologieDescrizione, GruppiDescrizione, SottogruppiDescrizione, MarcheDescrizione, Marche_img, MIN(PrezzoPromo) AS PrezzoPromo, MIN(IF((@AbilitatoIvaReverseCharge=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(@Iva_Utente>-1,((PrezzoPromo)*((@Iva_Utente/100)+1)),PrezzoPromoIvato))) AS PrezzoPromoIvato, MarcheId, CategorieId, TipologieId, IF(PrezzoPromo IS NULL,Prezzo,PrezzoPromo) Ord_PrezzoPromo, IF(PrezzoPromoIvato IS NULL,PrezzoIvato,IF((@AbilitatoIvaReverseCharge=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(@Iva_Utente>-1,((PrezzoPromo)*((@Iva_Utente/100)+1)),PrezzoPromoIvato))) Ord_PrezzoPromoIvato, OfferteQntMinima, OfferteMultipli, OfferteDataInizio, OfferteDataFine FROM vsuperarticoli WHERE ID=@TB_ID AND NListino=@NListino GROUP BY offerteQntMinima, offerteMultipli, nlistino ORDER BY PrezzoPromo DESC"
                SQLDATA_Promo.SelectParameters.Clear()
                SQLDATA_Promo.SelectParameters.Add("@AbilitatoIvaReverseCharge", Session("AbilitatoIvaReverseCharge"))
                SQLDATA_Promo.SelectParameters.Add("@Iva_Utente", Session("Iva_Utente"))
                SQLDATA_Promo.SelectParameters.Add("@TB_ID", tb_id.Text)
                SQLDATA_Promo.SelectParameters.Add("@NListino", Session("listino"))
            Else
                SQLDATA_Promo.SelectCommand = ""
                prezzoPromo.Visible = False
            End If


            Try
                SettaDisponibilita()
                Dim lblDes As Label
                Dim lblDesHTML As Label
                lblDes = Me.fvPage.FindControl("lblDescrizioneArt")
                lblDesHTML = Me.fvPage.FindControl("lblDescrizioneHTMLArt")
                If lblDes.Text <> "" Then
                    lblDes.Text = lblDes.Text.Replace(vbNewLine, "<br>")
                End If

                Dim IvaTipo As Integer = Me.Session("IvaTipo")
                dispo = Me.fvPage.FindControl("lblDispo")
                Prezzo = Me.fvPage.FindControl("lblPrezzo")
                PrezzoIvato = Me.fvPage.FindControl("lblPrezzoIvato")

                ' --------------------------------------------------------------------------------
                If IvaTipo = 1 Then
                    Prezzo.Visible = True
                ElseIf IvaTipo = 2 Then
                    PrezzoIvato.Visible = True
                End If
                ' ---------------------------------------------------------------------------------

                showTagliecolori()

            Catch ex As Exception
            End Try
        Else
            Response.Redirect("default.aspx")
        End If
    End Sub

    Protected Sub ImageButton1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
        Dim temp As ImageButton = sender
        Dim temp2 As HtmlInputControl

        'Verifica se il settore del prodotto è Attivo o meno. Altrimenti reindirizzo l'utente verso una pagina di errore, 
        'che avvisa l'utente che l'amministratore ha disabilitato tale Settore e quindi tutti gli articoli correlati non 
        'sono più disponibili per la vendita
        If controlla_abilitazione_settore(Me.requestId) = 1 Then
            temp2 = CType(temp.NamingContainer.FindControl("SpeditoGratis"), HtmlInputControl)
            If temp2.Value = 1 Then
                'Comunico al carrello se il prodotto è un prodotto ha spedizione gratis
                Session("ProdottoGratis") = 1
            Else
                'Comunico al carrello se il prodotto non è un prodotto ha spedizione gratis
                Session("ProdottoGratis") = 0
            End If

            Dim Qta As TextBox
            Qta = Me.fvPage.FindControl("tbQuantita")

            Me.Session("Carrello_ArticoloId") = Me.requestId
            Me.Session("Carrello_TCId") = Me.requestTCId
            Me.Session("Carrello_Quantita") = Qta.Text
            Me.Response.Redirect("aggiungi.aspx")
        Else
            Response.Redirect("settore_disabilitato.aspx")
        End If

    End Sub

    Public Sub AggiornaVisite()
        Dim id As Integer = Me.requestId
        If id <> CLng(Me.Session("visite_articoloid")) Then
            Me.Session("visite_articoloid") = id
            Dim params As New Dictionary(Of String, String)
            params.add("@id", id)
            ExecuteUpdate("articoli", "visite=visite+1", "where id=@id", params)
        End If
    End Sub

    Protected Function ExecuteUpdate(ByVal table As String, ByVal fieldAndValues As String, Optional ByVal wherePart As String = "", Optional ByVal params As Dictionary(Of String, String) = Nothing)
        Dim sqlString As String = "UPDATE " & table & " set " & fieldAndValues & " " & wherePart
        ExecuteNonQuery(False, sqlString, params)
    End Function

    Protected Function ExecuteNonQuery(ByVal isStoredProcedure As Boolean, ByVal sqlString As String, Optional ByVal params As Dictionary(Of String, String) = Nothing)
        Dim conn As New MySqlConnection
        Try
            Dim connectionString As String = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            If Not connectionString Is Nothing Then
                conn.ConnectionString = connectionString
                conn.Open()
                Dim cmd = New MySqlCommand With {
                    .Connection = conn,
                    .CommandType = CommandType.Text,
                    .CommandText = sqlString
                }
                Dim paramName As String
                For Each paramName In params.Keys
                    cmd.Parameters.AddWithValue(paramName, params(paramName))
                Next
                If isStoredProcedure Then
                    cmd.Parameters.AddWithValue("?parRetVal", "0")
                    cmd.Parameters("?parRetVal").Direction = ParameterDirection.Output
                End If
                cmd.ExecuteNonQuery()
                cmd.Parameters.Clear()
                cmd.Dispose()
            End If
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If
        End Try
    End Function

    Protected Sub rPromo_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs)

        Dim label_sconto As Label
        Dim panel_sconto As Panel
        Dim prezzo_canc As Label
        Dim prezzo_ivato_canc As Label

        panel_sconto = e.Item.Parent.Parent.FindControl("Panel_Visualizza_Percentuale_Sconto")
        prezzo_canc = e.Item.Parent.Parent.FindControl("Label_Canc_Prezzo")
        prezzo_ivato_canc = e.Item.Parent.Parent.FindControl("Label_Canc_PrezzoIvato")

        label_sconto = e.Item.Parent.Parent.FindControl("sconto_applicato")

        Dim Offerta As Label = e.Item.FindControl("lblOfferta")
        Dim InOfferta As Label = e.Item.FindControl("lblInOfferta")
        Dim QtaMin As Label = e.Item.FindControl("lblQtaMin")
        Dim QtaMultipli As Label = e.Item.FindControl("lblMultipli")
        Dim PrezzoPromo As Label = e.Item.FindControl("lblPrezzoPromo")
        Dim PrezzoPromoIvato As Label = e.Item.FindControl("lblPrezzoPromoIvato")

        Dim Qta As TextBox = e.Item.Parent.Parent.FindControl("tbQuantita")
        Dim ParentPrezzoPromo As Label = e.Item.Parent.Parent.FindControl("lblPrezzoPromo")
        Dim ParentPrezzo As Label = e.Item.Parent.Parent.FindControl("lblPrezzo")
        Dim ParentPrezzoIvato As Label = e.Item.Parent.Parent.FindControl("lblPrezzoIvato")
        Dim ParentIconPromo As Image = e.Item.Parent.Parent.FindControl("img_offerta")
        Dim ParentIconPromoImg As Image = e.Item.Parent.Parent.FindControl("img_promo")
		Dim prezzoStandard As Label = e.Item.Parent.Parent.FindControl("Label_Canc_PrezzoIvato")
		
		Dim lblListinoUfficiale As Label = e.Item.Parent.Parent.FindControl("Label_LU")

        If InOfferta.Text = 1 Then
            'Visualizzo o meno l'icona in offerta
            ParentIconPromo.Visible = True
            ParentIconPromoImg.Visible = True

            If QtaMin.Text > 0 Then
                Offerta.Text = Offerta.Text & " MINIMO " & QtaMin.Text & " PZ."
                Qta.Text = QtaMin.Text
            ElseIf QtaMultipli.Text > 0 Then
                Offerta.Text = Offerta.Text & " MULTIPLI " & QtaMultipli.Text & " PZ."
                Qta.Text = QtaMultipli.Text
            End If

            If IvaTipo = 1 Then
                Offerta.Text = Offerta.Text & " A € " & FormatNumber(PrezzoPromo.Text, 2)
            ElseIf IvaTipo = 2 Then
                Offerta.Text = Offerta.Text & " A € " & FormatNumber(PrezzoPromoIvato.Text, 2)
            End If

            ' --------------------------------------------------------------------------------
            'Stampo a video lo sconto applcato all'offerta
            panel_sconto.Visible = True
            If IvaTipo = 1 Then
				Dim prezzoPromoDouble As Double = FormatNumber(PrezzoPromo.Text.Replace(".", ""), 2)
				Dim prezzoParentDouble As Double = FormatNumber(ParentPrezzo.Text.Replace(".", ""), 2)
				label_sconto.Text = "- " & String.Format("{0:0}", (prezzoParentDouble - prezzoPromoDouble) * 100 / prezzoParentDouble) & "%"
				ParentPrezzo.Text = "€ " & FormatNumber(PrezzoPromo.Text, 2)
            Else
				Dim prezzoPromoIvatoDouble As Double = FormatNumber(PrezzoPromoIvato.Text.Replace(".", ""), 2)
				Dim prezzoStandardDouble As Double = FormatNumber(prezzoStandard.Text.Replace(".", ""), 2)
				label_sconto.Text = "- " & String.Format("{0:0}", (prezzoStandardDouble - prezzoPromoIvatoDouble) * 100 / prezzoStandardDouble) & "%"
                ParentPrezzoIvato.Text = "€ " & FormatNumber(PrezzoPromoIvato.Text, 2)
            End If

            If Val(label_sconto.Text) = 0 Then
                label_sconto.Text = "0%"
            End If
            ' ---------------------------------------------------------------------------------

            e.Item.Parent.Parent.FindControl("Panel_in_offerta").Visible = True

            If Session("IvaTipo") = 1 Then
                prezzo_canc.Visible = True
                prezzo_ivato_canc.Visible = False
            Else
                prezzo_canc.Visible = False
                prezzo_ivato_canc.Visible = True
            End If
        End If

    End Sub

    Protected Sub Image1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs)

        Dim img As Image = sender
        Dim imageurl As String = Server.MapPath(img.ImageUrl)

        Dim temp_obj As HtmlLink
        temp_obj = Me.Page.Master.FindControl("Immagine_Facebook")
        temp_obj.Href = img.ImageUrl.ToString

        Try
            Dim bmp As System.Drawing.Image = System.Drawing.Image.FromFile(imageurl)

            If bmp.Width > 400 Then
                img.Width = 400
            End If
        Catch ex As Exception

        End Try

    End Sub

    Function controlla_abilitazione_settore(ByVal idArticolo As Integer) As Integer
        Dim params As New Dictionary(Of String, String)
        params.add("@idArticolo", idArticolo)
        Dim dr = ExecuteQueryGetDataReader("vsuperarticoli.*, settori.Descrizione, settori.Abilitato, settori.Ordinamento, settori.Predefinito, settori.Img", "vsuperarticoli", "INNER JOIN settori ON settori.id=vsuperarticoli.SettoriId WHERE (vsuperarticoli.id=@idArticolo) AND (settori.Abilitato=1)", params)

        'Restituisce 1 nel caso ci sia almeno una riga come risultato, e quindi il settore relativo all' IDArticolo è ABILITATO altrimenti restituisce 0
        If (dr.Count > 0) Then
            Return 1
        Else
            Return 0
        End If
    End Function

    Protected Function ExecuteQueryGetDataReader(ByVal fields As String, ByVal table As String, Optional ByVal wherePart As String = "", Optional ByVal params As Dictionary(Of String, String) = Nothing) As List(Of Dictionary(Of String, Object))
        Dim sqlString As String = "SELECT " & fields & " FROM " & table & " " & wherePart
        Dim dr As MySqlDataReader
        Dim result As New List(Of Dictionary(Of String, Object))
        Dim conn As New MySqlConnection
        Try
            Dim connectionString As String = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            If Not connectionString Is Nothing Then
                conn.ConnectionString = connectionString
                conn.Open()
                Dim cmd = New MySqlCommand With {
                    .Connection = conn,
                    .CommandType = CommandType.Text,
                    .CommandText = sqlString
                }
                If Not params Is Nothing Then
                    Dim paramName As String
                    For Each paramName In params.Keys
                        cmd.Parameters.AddWithValue(paramName, params(paramName))
                    Next
                End If
                dr = cmd.ExecuteReader()

                While dr.Read()
                    Dim row As New Dictionary(Of String, Object)()

                    ' Per ogni colonna nella riga, aggiungi la colonna al dizionario
                    For i As Integer = 0 To dr.FieldCount - 1
                        ' Prendi il nome della colonna e il valore
                        Dim columnName As String = dr.GetName(i)
                        Dim value As Object = dr.GetValue(i)

                        ' Aggiungi la colonna e il valore al dizionario
                        row.Add(columnName, value)
                    Next

                    ' Aggiungi la riga al risultato
                    result.Add(row)
                End While

                dr.Close()
                dr.Dispose()
            End If
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If
        End Try
        Return result
    End Function

    Protected Sub LB_Dettagli_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim pulsante As LinkButton = sender

        Dim MV As MultiView = pulsante.Parent.FindControl("Multi_Vista")

        MV.ActiveViewIndex = 0
    End Sub

    Protected Sub LB_ArtCollegati_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim pulsante As LinkButton = sender

        Dim MV As MultiView = pulsante.Parent.FindControl("Multi_Vista")

        MV.ActiveViewIndex = 1
    End Sub

    Protected Sub LB_Recensioni_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim pulsante As LinkButton = sender

        Dim MV As MultiView = pulsante.Parent.FindControl("Multi_Vista")

        MV.ActiveViewIndex = 2
    End Sub
	
	Protected Sub LB_NormeGaranzia_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim pulsante As LinkButton = sender

        Dim MV As MultiView = pulsante.Parent.FindControl("Multi_Vista")

        MV.ActiveViewIndex = 3
    End Sub


    'FILTRI TAGLIA COLORE AGGIUNTI DA ANGELO IL 15/12/2017
    'INIZIO

    Public Sub showTagliecolori()
        Dim list As DropDownList = Me.fvPage.FindControl("Drop_Tagliecolori")
        Dim tc As Integer = Session("TC")
        If tc = 1 Then
            list.Visible = True
            Dim sqlString As String
            sqlString = "select CONCAT_WS(' , ',taglie.descrizione, colori.descrizione , (TRIM(TRAILING '.' FROM(CAST(TRIM(TRAILING '0' FROM vsuperarticoli.Giacenza)AS char))))) AS details, TCid, ArticoliId from vsuperarticoli "
            sqlString = sqlString & "inner join articoli_tagliecolori on vsuperarticoli.TCid = articoli_tagliecolori.id "
            sqlString = sqlString & "inner join taglie on taglie.id = articoli_tagliecolori.tagliaid "
            sqlString = sqlString & "inner join colori on colori.id = articoli_tagliecolori.coloreid "
            sqlString = sqlString & "where NListino=@NListino AND ArticoliId = @ArticoliId Group by TCid Order by tagliaid, coloreid"
            Dim params As New Dictionary(Of String, String)
            params.add("@NListino", Session("listino"))
            params.add("@ArticoliId", Me.requestId)
            PopulateDropdownlist(sqlString, list, "details", "TCid", params)
            list.SelectedValue = requestTCId

        Else
            list.Visible = False
        End If
    End Sub

    Public Sub PopulateDropdownlist(ByVal sqlString As String, ByVal list As DropDownList, ByVal textField As String, ByVal valueField As String, Optional params As Dictionary(Of String, String) = Nothing)
        Dim dt As New DataTable
        Dim conn As New MySqlConnection
        Try
            Dim connectionString As String = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            If Not connectionString Is Nothing Then
                Using cmd = conn.CreateCommand()
                    cmd.CommandType = CommandType.Text
                    cmd.CommandText = sqlString
                    Dim paramName As String
                    For Each paramName In params.Keys
                        cmd.Parameters.AddWithValue(paramName, params(paramName))
                    Next
                    Using da As New MySqlDataAdapter(cmd)
                        da.Fill(dt)
                    End Using
                End Using
                list.DataSource = dt
                list.DataTextField = textField
                list.DataValueField = valueField
                list.DataBind()
            End If
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If
        End Try
    End Sub

    Protected Sub Drop_Tagliecolori_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim list As DropDownList = Me.fvPage.FindControl("Drop_Tagliecolori")
        Response.Redirect(Request.Url.AbsolutePath & "?id=" & requestId & "&TCid=" & list.SelectedValue)
    End Sub

    'FILTRI TAGLIA COLORE AGGIUNTI DA ANGELO IL 15/12/2017
    'FINE
End Class