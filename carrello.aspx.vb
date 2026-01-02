Imports MySql.Data.MySqlClient
Imports System.Data
Imports CityRegistry.CityRegistrySoapClient

Partial Class carrello
    Inherits System.Web.UI.Page

    Dim IvaTipo As Integer
    Dim DispoTipo As Integer
    Dim DispoMinima As Integer
    Dim RitiroSede As Boolean = False
    Dim Cookie As String = "N"
	Dim cityRegistry As New CityRegistry.CityRegistrySoapClient
    'Variabili per il carrello
    Dim i As Integer
    Public imponibile As Double = 0
    Public imponibile_gratis As Double = 0
    Dim calcolo_iva As Double = 0
    Dim totale As Double
    Dim pesoTotale As Double = 0
    Dim qta As Integer = 0
    Dim Selezionato_Vettore_Promo As Integer

    'Conteggio totali per calcolo Buono Sconto
    Dim TotaleMerce As Double = 0

    'Variabili per selezionare il vettore PROMO
    Dim cont_indice_riga As Integer = 0
    Dim indice_riga_da_selezionare As Integer = -1
    Dim costo_promo_minimo As Double = 1000000.0

    Public differenzaTrasportoGratis As Double = 0

	Enum Lst
        indirizzoSpedizione
        destinazioneAlternativa
    End Enum
	
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
	'Page.ClientScript.RegisterStartupScript(Me.GetType(), "AlertScript", "alert('"& Me.Session.Item("Listino") & "-" & Me.Session.Item("AziendaId") & "');", True)
        'Setto il Timeout di Sessione
        Session.Timeout = 10
		Session("DESTINAZIONEALTERNATIVA") = 0
		REM btnElimDest.enabled = false
		REM btnModDest.enabled = false
        Me.MaintainScrollPositionOnPostBack = True

        IvaTipo = Me.Session("IvaTipo")
        DispoTipo = Me.Session("DispoTipo")
        DispoMinima = Me.Session("DispoMinima")

        'If DispoTipo = 1 Then
        '    Me.Repeater1.Columns(2).HeaderText = "Disp."
        '    Me.Repeater1.Columns(3).Visible = False
        '    Me.Repeater1.Columns(4).Visible = False
        'ElseIf DispoTipo = 2 Then
        '    Me.Repeater1.Columns(2).HeaderText = "D"
        '    Me.Repeater1.Columns(3).Visible = True
        '    Me.Repeater1.Columns(4).Visible = True
        'End If

        'Nascondo i pannelli dei dati anagrafici quando non sono loggato
        If Me.Session("LoginId") > 0 Then
            Me.pnlFatturazione.Visible = True
			Me.PnlSpedizione.Visible = True
            Me.PnlDestinazione.Visible = True
            Me.Panel_Note.Visible = True
        Else
            Me.pnlFatturazione.Visible = False
			Me.PnlSpedizione.Visible = False
            Me.PnlDestinazione.Visible = False
            Me.Panel_Note.Visible = False
        End If

        'Quando clicco su Aggiorna Carrello riaggiorno la pagina carrello e rifaccio i calcoli
        'If Session("Click_AggiornaCarrello") = 1 Then
        '    Session("Click_AggiornaCarrello") = 0

        '    Me.tOrdine.Visible = True
        '    Me.btAggiorna.Enabled = True
        '    Me.btContinua.Enabled = True
        '    Me.btSvuota.Enabled = True
        '    'Me.Repeater1.DataBind()
        '    Me.lblPagamento.Text = String.Format("{0:c}", CDbl("0"))
        '    Me.lblSpeseSped.Text = String.Format("{0:c}", CDbl("0"))
        '    Me.lblSpeseAss.Text = String.Format("{0:c}", CDbl("0"))
        'End If

        FillTableInfo()
    End Sub

    ' forgotten code?

    Sub preleva_prezzi_articoli()

        Dim LoginId As Integer = Me.Session("LoginId")

        Dim params As New Dictionary(Of String, String)
        params.add("@IvaUtente", Session("Iva_Utente"))
        params.add("@IvaRCUtente", Session("IvaReverseCharge_Utente"))
        params.add("@listino", Session("Listino"))
        Dim loginOrSessionId = ""
        If LoginId = 0 Then
            loginOrSessionId = "SessionID=@SessionId"
            params.add("@SessionId", Me.Session.SessionID)
        Else
            loginOrSessionId = "LoginId=@LoginId"
            params.add("@LoginId", LoginId)
        End If
        Dim innerJoin = "INNER JOIN (SELECT carrello.id AS idCarrello, carrello.ArticoliId, vsuperarticoli.id, vsuperarticoli.Nlistino, vsuperarticoli.InOfferta, vsuperarticoli.DescrizioneIvaRC, IF((InOfferta=1) AND ((CDate(OfferteDataInizio)<=CURDATE()) AND (CDate(OfferteDataFine)>=CURDATE())),vsuperarticoli.PrezzoPromo,vsuperarticoli.Prezzo) AS new_Prezzo, IF((InOfferta=1) AND ((CDate(OfferteDataInizio)<=CURDATE()) AND (CDate(OfferteDataFine)>=CURDATE())),IF(@IvaUtente>-1,((vsuperarticoli.PrezzoPromo)*((@IvaUtente/100)+1)),vsuperarticoli.PrezzoPromoIvato),IF(@IvaUtente>-1,((vsuperarticoli.Prezzo)*((@IvaUtente/100)+1)),vsuperarticoli.PrezzoIvato)) AS new_PrezzoIvato, IF((InOfferta=1) AND ((CDate(OfferteDataInizio)<=CURDATE()) AND (CDate(OfferteDataFine)>=CURDATE())),IF(@IvaRCUtente>-1,((vsuperarticoli.PrezzoPromo)*((@IvaRCUtente/100)+1)),vsuperarticoli.PrezzoPromoIvato),IF(@IvaRCUtente)>-1,((vsuperarticoli.Prezzo)*((@IvaRCUtente/100)+1)),-1)) AS new_PrezzoRC FROM carrello  INNER JOIN vsuperarticoli  ON (carrello.ArticoliId=vsuperarticoli.id)  WHERE (vsuperarticoli.Nlistino=@listino) AND " & loginOrSessionId & ") AS t1  ON idCarrello=carrello.id"

        ExecuteUpdate(innerJoin, "carrello.Prezzo=new_Prezzo, carrello.PrezzoIvato=new_PrezzoIvato, carrello.ValoreIvaRC=new_PrezzoRC, carrello.DescrizioneIvaRC=DescrizioneIvaRC", "", params)
    End Sub

    Sub cancella_campi_destinazione_alternativa_o_indirizzo_spedizione()
		tbRagioneSocialeA.Text = ""
		tbNomeA.Text = ""
		tbIndirizzo2.Text = ""
		riempi_ddl_citta(tbCap2.Text, ddlCitta2, tbProvincia2, "")
		tbCap2.Text = ""
		tbProvincia2.Text = ""
		tbZona.Text = ""
		tbTelefono2.Text = ""
		tbNote.Text = ""
		CHKPREDEFINITO.Checked = False
		lblTab_RagioneSocialeSpedizione.Text = ""
		lblTab_NomeSpedizione.Text = ""
		lblTab_IndirizzoSpedizione.Text = ""
		lblTab_CittaSpedizione.Text = ""
		lblTab_CapSpedizione.Text = ""
		lblTab_ProvinciaSpedizione.Text = ""
		lblTab_ZonaSpedizione.Text = ""
		lblTab_TelSpedizione.Text = ""
		lblTab_NotaDestinazione.Text = ""
	End Sub
	
    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        Me.Title = Me.Title & " - Il tuo Carrello"
		
        Dim LoginId As Integer = Me.Session("LoginId")
        Dim SessionID As String = Me.Session.SessionID
        Dim WhereUserId As String

		'cancella_campi_destinazione_alternativa_o_indirizzo_spedizione()
		
        Dim Sqlstring As String = "SELECT vcarrello.*, articoli.SpedizioneGratis_Listini, articoli.SpedizioneGratis_Data_Inizio, articoli.SpedizioneGratis_Data_Fine, taglie.descrizione as taglia, colori.descrizione as colore FROM vcarrello"
        Sqlstring = Sqlstring + " LEFT OUTER JOIN articoli ON vcarrello.ArticoliId = articoli.id"
        Sqlstring = Sqlstring + " LEFT OUTER JOIN articoli_tagliecolori ON vcarrello.TCid = articoli_tagliecolori.id"
        Sqlstring = Sqlstring + " LEFT OUTER JOIN taglie ON articoli_tagliecolori.tagliaid = taglie.id"
        Sqlstring = Sqlstring + " LEFT OUTER JOIN colori ON articoli_tagliecolori.coloreid = colori.id"
        If LoginId = 0 Then
            WhereUserId = "(SessionId=@SessionId)"
        Else
            WhereUserId = "(LoginId=@LoginId)"
        End If
        Me.sdsArticoli.SelectCommand = Sqlstring & " WHERE (" & WhereUserId & " ) ORDER BY id"
        sdsArticoli.SelectParameters.Clear()
        sdsArticoli.SelectParameters.Add("@SessionId", SessionID)
        sdsArticoli.SelectParameters.Add("@LoginId", LoginId)

        Me.sdsArticoli_Spedizione_Gratis.SelectCommand = Sqlstring & " WHERE " & WhereUserId & " AND (articoli.SpedizioneGratis_Listini != '') AND (SpedizioneGratis_Listini LIKE CONCAT('%', @listino, ';%')) AND ((SpedizioneGratis_Data_Inizio <= CURDATE()) AND (SpedizioneGratis_Data_Fine >= CURDATE() OR SpedizioneGratis_Data_Fine Is NULL)) ORDER BY id"
        sdsArticoli_Spedizione_Gratis.SelectParameters.Clear()
        sdsArticoli_Spedizione_Gratis.SelectParameters.Add("@SessionId", SessionID)
        sdsArticoli_Spedizione_Gratis.SelectParameters.Add("@LoginId", LoginId)
        sdsArticoli_Spedizione_Gratis.SelectParameters.Add("@listino", Session("Listino"))

        IvaTipo = Me.Session("IvaTipo")
        If IvaTipo = 1 Then
            Me.lblPrezzi.Text = "*Prezzi Iva Esclusa"
        ElseIf IvaTipo = 2 Then
            If Session("Iva_Utente") > -1 Then
                Me.lblPrezzi.Text = "*Prezzi Iva Inclusa - (IVA Utente al " & Session("Iva_Utente") & "%)"
            Else
                Me.lblPrezzi.Text = "*Prezzi Iva Inclusa"
            End If
        End If

        'Nascondo i pannelli dei dati anagrafici quando non sono loggato
        If Me.Session("LoginId") > 0 Then
            Me.pnlFatturazione.Visible = True
			Me.PnlSpedizione.Visible = True
			Me.PnlDestinazione.Visible = True
            Me.Panel_Note.Visible = True
        Else
            Me.pnlFatturazione.Visible = False
			Me.PnlSpedizione.Visible = False
            Me.PnlDestinazione.Visible = False
            Me.Panel_Note.Visible = False
        End If
		
		
		REM Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('" & Me.sdsArticoli.SelectCommand.Replace("'", """").ToUpper & "')}</script>")
    End Sub

    Protected Sub Repeater1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Repeater1.PreRender
        Dim i As Integer

        'Carrello Normale
        For i = 0 To Repeater1.items.Count - 1
            Dim img As Image
            Dim dispo As Label
            Dim arrivo As Label
            Dim importo As Label
            Dim importoIvato As Label
            Dim peso As Label
            Dim tbQta As TextBox

            tbQta = Repeater1.items(i).FindControl("tbQta")
            img = Repeater1.items(i).FindControl("imgDispo")
            dispo = Repeater1.items(i).FindControl("lblDispo")
            arrivo = Repeater1.items(i).FindControl("lblArrivo")
            importo = Repeater1.items(i).FindControl("lblImporto")
            importoIvato = Repeater1.items(i).FindControl("lblImportoIvato")
            peso = Repeater1.items(i).FindControl("lblPeso")

            qta = qta + tbQta.Text

            If qta > 0 Then

                If IvaTipo = 1 Then
                    importo.Visible = True
                    importoIvato.Visible = False
                    Repeater1.items(i).FindControl("lblprezzo").Visible = True
                    Repeater1.items(i).FindControl("lblprezzoivato").Visible = False
                    'Conteggio Totale per Buono Sconto
                    TotaleMerce += CDbl(importo.Text)
                ElseIf IvaTipo = 2 Then
                    importo.Visible = False
                    importoIvato.Visible = True
                    Repeater1.items(i).FindControl("lblprezzo").Visible = False
                    Repeater1.items(i).FindControl("lblprezzoivato").Visible = True
                    'Conteggio Totale per Buono Sconto
                    TotaleMerce += CDbl(importoIvato.Text)
                End If

                Session("TotaleMerce") = TotaleMerce

                imponibile = imponibile + importo.Text
                calcolo_iva = calcolo_iva + (CDbl(importoIvato.Text) - CDbl(importo.Text))
                totale = totale + importoIvato.Text

                If peso.Text <> "" Then
                    pesoTotale = pesoTotale + peso.Text
                End If

                If DispoTipo = 1 Then
					Dim dispoDouble as Double = 0'CDbl(dispo.Text.Replace("−","-").Replace(">",""))
                    If dispoDouble > DispoMinima Then
                        img.ImageUrl = "~/images/verde.gif"
                        img.AlternateText = "Disponibile"
                    ElseIf dispoDouble > 0 Then
                        img.ImageUrl = "~/images/giallo.gif"
                        img.AlternateText = "Disponibilità Scarsa"
                    Else
						Dim arrivoDouble as Double
						try
							arrivoDouble = CDbl(arrivo.Text)
						catch
							arrivoDouble =  0
						end try
                        If arrivoDouble > 0 Then
                            img.ImageUrl = "~/images/azzurro.gif"
                            img.AlternateText = "In Arrivo"
                        Else
                            img.ImageUrl = "~/images/rosso.gif"
                            img.AlternateText = "Non Disponibile"
                        End If
                    End If

                ElseIf DispoTipo = 2 Then
                    img.Visible = False
                    dispo.Visible = True
                End If
            End If
        Next

        ' ------------------------ CONTEGGIO DEI TOTALI DA PAGARE -----------------------
        'Salvataggio per l'SQLData relativo ai vettori in PROMO
        Session.Item("Imponibile") = imponibile - imponibile_gratis

        Me.lblImponibile.Text = "€ " & FormatNumber(imponibile, 2)
        'Session("Calcolo_Iva") = calcolo_iva
        Me.tbPeso.Text = pesoTotale

        Me.tbTotale.Text = totale
        ' --------------------------------------------------------------------------------

        'ABILITA E DISABILITA I PULSANTI
        ArticoliCarrello(qta)

        'Me.gvVettori.DataBind()
    End Sub

    Public Sub ArticoliCarrello(ByVal numero As Integer)
        Me.lblArticoli.Text = numero
        If numero = 0 Then
            Me.lblPresenti.Text = "articoli nel carrello"
            Me.btSvuota.Visible = False
            Me.btCompleta.Visible = False
            Me.btAggiorna.Visible = True
        ElseIf numero = 1 Then
            Me.lblPresenti.Text = "articolo nel carrello"
            Me.btSvuota.Visible = True
            If (Me.gvVettoriPromo.Visible = True) Then
                Me.btCompleta.Visible = False
            Else
                Me.btCompleta.Visible = True
            End If
            Me.btAggiorna.Visible = True
        Else
            Me.lblPresenti.Text = "articoli nel carrello"
            Me.btSvuota.Visible = True
            If (Me.gvVettoriPromo.Visible = True) Then
                Me.btCompleta.Visible = False
            Else
                Me.btCompleta.Visible = True
            End If
            Me.btAggiorna.Visible = True
        End If
		if Me.Session("CanOrder") = 0 Then
			Me.btCompleta.Visible = False
			Me.canorder.Visible = True
		else
			Me.canorder.Visible = False
		End If
    End Sub

    Private Sub SendOrder()

        Try

            Me.Session("Ordine_TipoDoc") = 4
            Me.Session("Ordine_Documento") = "Ordine"
            Me.Session("Ordine_Pagamento") = Me.tbPagamenti.Text
            Me.Session("Ordine_BancaSellaGestPay_ShopId") = Me.tbShopIdGestPay.Text
            Me.Session("Ordine_Vettore") = Me.tbVettoriId.Text
            Me.Session("Ordine_SpeseSped") = CDbl(Me.lblSpeseSped.Text)
            Me.Session("Ordine_SpeseAss") = CDbl(Me.lblSpeseAss.Text)
            Me.Session("Ordine_SpesePag") = CDbl(Me.lblPagamento.Text)
            Me.Session("Ordine_Totale_Documento") = CDbl(Me.lblTotale.Text)

            If (GV_BuoniSconti.Rows.Count > 0) Then
                Dim BuonoSconto_Descrizione1 As Label = GV_BuoniSconti.Rows(0).FindControl("lbl_Descrizione1_BuonoSconto")
                Dim BuonoSconto_Descrizione2 As Label = GV_BuoniSconti.Rows(0).FindControl("lbl_Descrizione2_BuonoSconto")
                Dim BuonoSconto_Fisso As Label = GV_BuoniSconti.Rows(0).FindControl("lbl_scontoFisso_BuonoSconto")
                Dim BuonoSconto_Percentuale As Label = GV_BuoniSconti.Rows(0).FindControl("lbl_Percentuale_BuonoSconto")
                Dim BuonoSconto_Vettore As Label = GV_BuoniSconti.Rows(0).Cells(0).FindControl("lbl_scontoVettore")
                Dim BuonoSconto_Codice As TextBox = TB_BuonoSconto
                Dim BuonoSconto_Totale As Label = lblBuonoSconto
                Dim BuonoSconto_IVA As Label = lblBuonoScontoIVA

                Me.Session("Ordine_DescrizioneBuonoSconto") = BuonoSconto_Descrizione1.Text & " " & BuonoSconto_Descrizione2.Text & " per un valore di € " & CDbl(BuonoSconto_Totale.Text.Replace("€ ", "")) + CDbl(BuonoSconto_IVA.Text.Replace("€ ", "")) & " Codice Applicato: " & BuonoSconto_Codice.Text
                Me.Session("Ordine_TotaleBuonoSconto") = CDbl(BuonoSconto_Totale.Text.Replace("€ ", "")) + CDbl(BuonoSconto_IVA.Text.Replace("€ ", ""))
                Me.Session("Ordine_TotaleBuonoScontoImponibile") = CDbl(BuonoSconto_Totale.Text.Replace("€ ", ""))
                Me.Session("Ordine_BuonoScontoIdIva") = preleva_IdIva(Session("Iva_Utente"))
                Me.Session("Ordine_BuonoScontoValoreIva") = preleva_ValoreIva(Session("Iva_Utente"))
                Me.Session("Ordine_CodiceBuonoSconto") = BuonoSconto_Codice.Text
            Else
                Me.Session("Ordine_DescrizioneBuonoSconto") = ""
                Me.Session("Ordine_TotaleBuonoSconto") = 0
                Me.Session("Ordine_TotaleBuonoScontoImponibile") = 0
                Me.Session("Ordine_BuonoScontoIdIva") = -1
                Me.Session("Ordine_BuonoScontoValoreIva") = 0
                Me.Session("Ordine_CodiceBuonoSconto") = ""
            End If
            Me.Session("NoteDocumento") = Me.txtNoteSpedizione.Text

            Response.Redirect("ordine.aspx?C=" & Cookie.ToUpper)

            'Test di controllo, relativo al buono sconto del carrello
            'Dim test As Integer = 0
            'Dim test2 As Integer = 0

            'test = test2 + Session("Ordine_DescrizioneBuonoSconto")

        Catch ex As Exception

        End Try

    End Sub

    Protected Sub gvVettori_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles gvVettori.PreRender
        LeggiVettori()
    End Sub

    Public Sub LeggiVettori()
        Dim i As Integer
        Dim rb As ConwayControls.Web.RadioButton
        Dim AsssicurazionePercentuale As Double
        Dim AssicurazioneMinimo As Double
        Dim TotAssicurazione As Double
        Dim lbl As Label
        Dim lblContrPerc As Label
        Dim lblContrFisso As Label
        Dim lblContrMinimo As Label
        Dim lblCosto As Label
        Dim sel As Boolean = False

        'Resetto il prezzo relativo al metodo di pagamento
        lblPagamento.Text = String.Format("{0:c}", CDbl("0"))

        'Controllo se Esiste ed è abilitato un Vettore PROMO
        Dim Vettore_Promo_Abilitato As Integer = 0
        For i = 0 To (Me.gvVettoriPromo.Rows.Count - 1)
            rb = gvVettoriPromo.Rows(i).FindControl("rbSpedizione")
            If rb.Enabled = True Then
                Vettore_Promo_Abilitato = 1
                Exit For
            End If
        Next

        'Controllo se è selezionato un vettore NORMALE
        Dim Vettore_NoNPromo_Selezionato As Integer = 0
        For i = 0 To (Me.gvVettori.Rows.Count - 1)
            rb = gvVettori.Rows(i).FindControl("rbSpedizione")
            If rb.Checked = True Then
                Vettore_NoNPromo_Selezionato = 1
                Exit For
            End If
        Next


        If (Vettore_Promo_Abilitato = 0) Or ((Vettore_Promo_Abilitato = 1) And (Vettore_NoNPromo_Selezionato = 1)) Then
            For i = 0 To gvVettori.Rows.Count - 1
                rb = gvVettori.Rows(i).FindControl("rbSpedizione")

                If rb.Checked Then

                    sel = True

                    'Spedizione
                    lblCosto = gvVettori.Rows(i).FindControl("lblCosto")
                    Me.lblSpeseSped.Text = String.Format("{0:c}", lblCosto.Text)
                    lbl = gvVettori.Rows(i).FindControl("lblId")
                    Me.tbVettoriId.Text = lbl.Text

                    'Assicurazione
                    lbl = gvVettori.Rows(i).FindControl("lblAssPerc")
                    AsssicurazionePercentuale = lbl.Text
                    lbl = gvVettori.Rows(i).FindControl("lblAssicurazioneMinimo")
                    AssicurazioneMinimo = lbl.Text

                    TotAssicurazione = (AsssicurazionePercentuale * Me.lblImponibile.Text) / 100
                    If TotAssicurazione < AssicurazioneMinimo Then
                        TotAssicurazione = AssicurazioneMinimo
                    End If

                    Me.lblAssicurazione.Text = String.Format("{0:c}", TotAssicurazione)

                    'Contrassegno
                    lblContrPerc = gvVettori.Rows(i).FindControl("lblContrPerc")
                    lblContrFisso = gvVettori.Rows(i).FindControl("lblContrFisso")
                    lblContrMinimo = gvVettori.Rows(i).FindControl("lblContrMinimo")
                    Me.tbContrFisso.Text = lblContrFisso.Text
                    Me.tbContrPerc.Text = lblContrPerc.Text
                    Me.tbContrMinimo.Text = lblContrMinimo.Text

                    AggiornaSpeseAssicurazione()

                    If AsssicurazionePercentuale = 0 Then
                        Me.cbAssicurazione.Checked = False
                        Me.cbAssicurazione.Enabled = False
                    Else
                        Me.cbAssicurazione.Enabled = True
                    End If

                    If lblContrPerc.Text = 0 Then
                        RitiroSede = True
                    Else
                        RitiroSede = False
                    End If

                End If

            Next

            If sel = False Then
                If (gvVettori.Rows.Count > 0) And (Selezionato_Vettore_Promo = 0) Then
                    rb = gvVettori.Rows(0).FindControl("rbSpedizione")
                    rb.Checked = True
                    LeggiVettori()
                End If
            End If
        Else
            For i = 0 To Me.gvVettoriPromo.Rows.Count - 1
                rb = gvVettoriPromo.Rows(i).FindControl("rbSpedizione")

                If rb.Enabled = True Then
                    rb.Checked = True
                End If

                If rb.Checked Then

                    sel = True

                    'Spedizione
                    lblCosto = gvVettoriPromo.Rows(i).FindControl("lblCosto")
                    Me.lblSpeseSped.Text = String.Format("{0:c}", lblCosto.Text)
                    lbl = gvVettoriPromo.Rows(i).FindControl("lblId")
                    Me.tbVettoriId.Text = lbl.Text

                    'Assicurazione
                    lbl = gvVettoriPromo.Rows(i).FindControl("lblAssPerc")
                    AsssicurazionePercentuale = lbl.Text
                    lbl = gvVettoriPromo.Rows(i).FindControl("lblAssicurazioneMinimo")
                    AssicurazioneMinimo = lbl.Text

                    TotAssicurazione = (AsssicurazionePercentuale * Me.lblImponibile.Text) / 100
                    If TotAssicurazione < AssicurazioneMinimo Then
                        TotAssicurazione = AssicurazioneMinimo
                    End If

                    Me.lblAssicurazione.Text = String.Format("{0:c}", TotAssicurazione)

                    'Contrassegno
                    lblContrPerc = gvVettoriPromo.Rows(i).FindControl("lblContrPerc")
                    lblContrFisso = gvVettoriPromo.Rows(i).FindControl("lblContrFisso")
                    lblContrMinimo = gvVettoriPromo.Rows(i).FindControl("lblContrMinimo")
                    Me.tbContrFisso.Text = lblContrFisso.Text
                    Me.tbContrPerc.Text = lblContrPerc.Text
                    Me.tbContrMinimo.Text = lblContrMinimo.Text

                    AggiornaSpeseAssicurazione()

                    If AsssicurazionePercentuale = 0 Then
                        Me.cbAssicurazione.Checked = False
                        Me.cbAssicurazione.Enabled = False
                    Else
                        Me.cbAssicurazione.Enabled = True
                    End If

                    If lblContrPerc.Text = 0 Then
                        RitiroSede = True
                    Else
                        RitiroSede = False
                    End If

                End If

            Next
        End If

        'Setto l'iva relativa al vettore selezionato
        If tbVettoriId.Text <> "" Then
            Session("Iva_Vettori") = IvaVettore(tbVettoriId.Text)
        End If
    End Sub

    Protected Sub cbAssicurazione_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbAssicurazione.Load
        AggiornaSpeseAssicurazione()
    End Sub

    Public Sub AggiornaSpeseAssicurazione()
        If Me.cbAssicurazione.Checked Then
            Me.lblSpeseAss.Text = Me.lblAssicurazione.Text
        Else
            Me.lblSpeseAss.Text = "€ 0,00"
        End If
    End Sub

    Protected Sub gvPagamento_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles gvPagamento.PreRender
        LeggiPagamenti()
    End Sub

    Public Sub LeggiPagamenti()

        Dim i As Integer
        Dim rb As ConwayControls.Web.RadioButton
        Dim Percentuale As Double
        Dim Fisso As Double
        Dim Minimo As Double
        Dim tot As Double
        Dim lbl As Label
        Dim lblContrassegno As Label
        Dim totPagamento As Double
        Dim sel As Boolean = False

        'Prima di effettuare i calcoli sulla percentuale di spesa di pagamento, devo aggiornare i Totali
        lblIva.Text = "€ " & FormatNumber(calcola_iva(CDbl(lblSpeseSped.Text), Session("Iva_Vettori")) + CDbl(lblSpeseAss.Text) * (preleva_ValoreIva(Session("Iva_Utente")) / 100), 2)
        lblTotale.Text = "€ " & FormatNumber(CDbl(lblImponibile.Text) + CDbl(lblIva.Text) + CDbl(lblSpeseAss.Text) + CDbl(lblSpeseSped.Text) + CDbl(lblPagamento.Text) + CDbl(lblBuonoSconto.Text), 2)

        'Setto in Sessione il Totale Documento, che verrà utilizzato dalla pagina ordine.aspx per i calcoli sul pagamento. Aggiornato in data 25/10/2017
        'Me.Session("Ordine_Totale_Documento") = CDbl(Me.lblTotale.Text)

        tot = (CDbl(Me.lblImponibile.Text) + CDbl(Me.lblSpeseSped.Text) + CDbl(Me.lblSpeseAss.Text) + CDbl(Me.lblIva.Text))

        For i = 0 To gvPagamento.Rows.Count - 1
            rb = gvPagamento.Rows(i).FindControl("rbPagamento")

            lblContrassegno = gvPagamento.Rows(i).FindControl("lblContrassegno")
            If lblContrassegno.Text = 1 Then
                Percentuale = CType(IIf(Me.tbContrPerc.Text <> "", Me.tbContrPerc.Text, 0), Double)
                Fisso = CType(IIf(Me.tbContrFisso.Text <> "", Me.tbContrFisso.Text, 0), Double)
                Minimo = CType(IIf(Me.tbContrMinimo.Text <> "", Me.tbContrMinimo.Text, 0), Double)
                If RitiroSede = True Then
                    rb.Checked = False
                    rb.Enabled = False
                Else
                    rb.Enabled = True
                End If
            Else
                lbl = gvPagamento.Rows(i).FindControl("lblCostoP")
                Percentuale = lbl.Text
                lbl = gvPagamento.Rows(i).FindControl("lblCostoF")
                Fisso = lbl.Text
                Minimo = 0
            End If

            Percentuale = tot * (Percentuale / 100)

            totPagamento = Percentuale + Fisso

            If totPagamento < Minimo Then
                totPagamento = Minimo
            End If

            lbl = gvPagamento.Rows(i).FindControl("lblCosto")
            Try : lbl.Text = String.Format("{0:c}", totPagamento) : Catch : lbl.Text = "0,00" : End Try

            If rb.Checked = True Then
                sel = True
                lbl = gvPagamento.Rows(i).FindControl("lblId")
                Me.tbPagamenti.Text = lbl.Text
                lbl = gvPagamento.Rows(i).FindControl("lblShopLogin")
                Me.tbShopIdGestPay.Text = lbl.Text
                Me.lblPagamento.Text = String.Format("{0:c}", totPagamento)
            End If

        Next
        If sel = False Then
            If gvPagamento.Rows.Count > 0 Then
                rb = gvPagamento.Rows(0).FindControl("rbPagamento")
                rb.Checked = True
                'LeggiPagamenti()
            End If
        End If
    End Sub

    Protected Sub rPromo_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs)
        Dim Offerta As Label = e.Item.FindControl("lblOfferta")
        Dim InOfferta As Label = e.Item.FindControl("lblInOfferta")
        Dim DataInizio As Label = e.Item.FindControl("lblDataInizio")
        Dim DataFine As Label = e.Item.FindControl("lblDataFine")
        Dim QtaMin As Label = e.Item.FindControl("lblQtaMin")
        Dim QtaMultipli As Label = e.Item.FindControl("lblMultipli")
        Dim PrezzoPromo As Label = e.Item.FindControl("lblPrezzoPromo")
        Dim PrezzoPromoIvato As Label = e.Item.FindControl("lblPrezzoPromoIvato")
        Dim idIvaRC As Label = e.Item.FindControl("lblidIvaRC")
        Dim ValoreIvaRC As Label = e.Item.FindControl("lblValoreIvaRC")

        If (InOfferta.Text = 1) Then
            'Eseguo la differnza tra le date, invecce del confronto con "<" o ">" per evitare problemi di confronto tra dati differenti
            If ((DateDiff(DateInterval.Day, CDate(DataInizio.Text), Date.Now) >= 0) And (DateDiff(DateInterval.Day, CDate(DataFine.Text), Date.Now) <= 0)) Then
                If QtaMin.Text > 0 Then
                    Offerta.Text = Offerta.Text & " MINIMO " & QtaMin.Text & " PZ."
                ElseIf QtaMultipli.Text > 0 Then
                    Offerta.Text = Offerta.Text & " MULTIPLI " & QtaMultipli.Text & " PZ."
                End If

                If IvaTipo = 1 Then
                    Offerta.Text = Offerta.Text & " A € " & FormatNumber(PrezzoPromo.Text, 2)
                ElseIf IvaTipo = 2 Then
                    If ((Session("AbilitatoIvaReverseCharge") = 1) And (idIvaRC.Text <> -1)) Then
                        Offerta.Text = Offerta.Text & " A € " & FormatNumber((CType(PrezzoPromo.Text, Double) * ((CType(ValoreIvaRC.Text, Double) / 100) + 1)), 2)
                    Else
                        If (Session("Iva_Utente") > -1) Then
                            Offerta.Text = Offerta.Text & " A € " & FormatNumber(CType(PrezzoPromo.Text, Double) * (((Session("Iva_Utente") / 100)) + 1), 2)
                        Else
                            Offerta.Text = Offerta.Text & " A € " & FormatNumber(CType(PrezzoPromoIvato.Text, Double), 2)
                        End If
                    End If
                End If

                Offerta.Visible = True
            End If
        End If
    End Sub


    Public Sub BindLstDestinazioneLstScegliIndirizzo

        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sqlString As String = ""
        Dim dsData As New DataSet

        Try

            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()

            cmd.Connection = conn
            cmd.CommandType = CommandType.Text
			cmd.Parameters.AddWithValue("@id",Session("UTENTIID"))
            cmd.CommandText = "SELECT ID, CONCAT(RAGIONESOCIALEA, ' - ', NOMEA, ' - ',INDIRIZZOA, ', CAP: ', CAPA, ' - ',CITTAA,' (', PROVINCIAA, ')') AS CAMPO FROM utentiindirizzi where UTENTEID = @id Order by Predefinito Desc"


            Dim sqlAdp As New MySqlDataAdapter(cmd)
            sqlAdp.Fill(dsData, "utentiindirizzi")

            cmd.Dispose()

            LstDestinazione.Items.Clear()
            LstDestinazione.DataSource = dsData
            LstDestinazione.DataValueField = "ID"
            LstDestinazione.DataTextField = "CAMPO"
            LstDestinazione.DataBind()
			LstDestinazione.Items.Insert(0, New ListItem("(Seleziona)", "0"))
			
			LstScegliIndirizzo.Items.Clear()
            LstScegliIndirizzo.DataSource = dsData
            LstScegliIndirizzo.DataValueField = "ID"
            LstScegliIndirizzo.DataTextField = "CAMPO"
            LstScegliIndirizzo.DataBind()

        Catch ex As Exception

        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

        End Try

    End Sub

    Public Function getIndirizzoPrincipale() As String

        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sqlString As String = ""
        Dim dsData As New DataSet

        Try
            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()

            cmd.Connection = conn
            cmd.CommandType = CommandType.Text
			cmd.Parameters.AddWithValue("@id",Session("UTENTIID"))
            cmd.CommandText = "SELECT CONCAT(RAGIONESOCIALE, ' - ', COGNOMENOME, ' - ',INDIRIZZO, ', CAP: ', CAP, ' - ',CITTA,' (', PROVINCIA, ')')AS CAMPO FROM utenti where ID = @id"

            If Not (IsNothing(cmd.ExecuteScalar)) AndAlso Not (IsDBNull(cmd.ExecuteScalar)) Then
                Return cmd.ExecuteScalar
            Else
                Return ""
            End If

            cmd.Dispose()

        Catch ex As Exception

            Return "ERRORE"

        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

        End Try

    End Function

    Public Sub FillTableInfo()

        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sqlString As String = ""
        Dim dr As MySqlDataReader

        Try

            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()

            cmd.Connection = conn
            cmd.CommandType = CommandType.Text
            cmd.Parameters.AddWithValue("@id", Session("UTENTIID"))
            cmd.CommandText = "SELECT * FROM utenti WHERE ID=@id"
            dr = cmd.ExecuteReader

            If dr.Read Then
                Me.lblTab_Cap.Text = dr.Item("CAP")
                If Not IsDBNull(dr.Item("CELLULARE")) Then Me.lblTab_Cell.Text = dr.Item("CELLULARE")
                Me.lblTab_CF.Text = dr.Item("CODICEFISCALE")
                Me.lblTab_Citta.Text = dr.Item("CITTA")
                If Not IsDBNull(dr.Item("FAX")) Then Me.lblTab_Fax.Text = dr.Item("FAX")
                Me.lblTab_Indirizzo.Text = dr.Item("INDIRIZZO")
                Me.lblTab_mail.Text = dr.Item("EMAIL")
                Me.lblTab_Nome.Text = dr.Item("COGNOMENOME")
                Me.lblTab_pIva.Text = dr.Item("PIVA")
                Me.lblTab_Provincia.Text = dr.Item("PROVINCIA")
                Me.lblTab_RagioneSociale.Text = dr.Item("RAGIONESOCIALE")
                Me.lblTab_Tel.Text = dr.Item("TELEFONO")
            End If

            dr.Close()
            cmd.Dispose()

        Catch ex As Exception

        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

        End Try

    End Sub

    Protected Sub ImgBtnDestinazioneSi_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImgBtnDestinazioneSi.Click
        AggiornaDestinazionePredefinita(True)
    End Sub

    Protected Sub ImgBtnDestinazioneNo_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImgBtnDestinazioneNo.Click
        AggiornaDestinazionePredefinita(False)
    End Sub

    Private Sub AggiornaDestinazionePredefinita(ByVal Aggiorna As Boolean)
        Dim predefinito = 0
        Dim params As New Dictionary(Of String, String)
        If Aggiorna = True Then
            params.add("@UtenteId", Session("UtentiID"))
            ExecuteUpdate("utentiindirizzi", "PREDEFINITO = 0", "UTENTEID=@UtenteId", params)
            predefinito = 1
        End If
        params.add("@UtenteId", Session("UtentiID"))
        params.add("@RAGIONESOCIALEA", Me.tbRagioneSocialeA.Text.Replace("'", "''").ToUpper)
        params.add("@NOMEA", Me.tbNomeA.Text.Replace("'", "''").ToUpper)
        params.add("@INDIRIZZOA", Me.tbIndirizzo2.Text.Replace("'", "''").ToUpper)
        params.add("@CAPA", Me.tbCap2.Text.Replace("'", "''").ToUpper)
        params.add("@CITTAA", getDdlCittaValue(Me.ddlCitta2).Replace("'", "''").ToUpper)
        params.add("@PROVINCIAA", Me.tbProvincia2.Text.Replace("'", "''").ToUpper)
        params.add("@NOTE", Me.tbNote.Text.Replace("'", "''").ToUpper)
        params.add("@TELEFONOA", Me.tbTelefono2.Text.Replace("'", "''").ToUpper)
        params.add("@ZONA", Me.tbZona.Text.Replace("'", "''").ToUpper)
        params.add("@PREDEFINITO", predefinito)
        ExecuteInsert("utentiindirizzi", "UTENTEID, RAGIONESOCIALEA, NOMEA, INDIRIZZOA, CAPA, CITTAA, PROVINCIAA, NOTE, TELEFONOA, ZONA, PREDEFINITO", "@UtenteId, @RAGIONESOCIALEA, @NOMEA, @INDIRIZZOA, @CAPA, @CITTAA, @PROVINCIAA, @NOTE, @TELEFONOA, @ZONA, @PREDEFINITO", params)

        BindLstDestinazioneLstScegliIndirizzo()
        Me.tblDestAlter.Visible = False

        Me.tbRagioneSocialeA.Text = ""
        Me.tbNomeA.Text = ""
        Me.tbIndirizzo2.Text = ""
        Me.tbCap2.Text = ""
        riempi_ddl_citta(tbCap2.Text, ddlCitta2, tbProvincia2, "")
        Me.tbProvincia2.Text = ""
        Me.tbNote.Text = ""
        Me.tbZona.Text = ""
        Me.tbTelefono2.Text = ""

        Me.RFRagioneSocialeA.Enabled = False
        Me.RFIndirizzo2.Enabled = False
        Me.RFCitta2.Enabled = False
        Me.RFProvincia2.Enabled = False
        Me.RFCap2.Enabled = False
        Me.RFTelefono2.Enabled = False

    End Sub

	Protected Sub clear_destinazione_alternativa()
		BindLstDestinazioneLstScegliIndirizzo
		Me.tbRagioneSocialeA.Text = ""
        Me.tbNomeA.Text = ""
        Me.tbIndirizzo2.Text = ""
        Me.tbCap2.Text = ""
        riempi_ddl_citta(tbCap2.Text, ddlCitta2, tbProvincia2, "")
        Me.tbProvincia2.Text = ""
        Me.tbNote.Text = ""
        Me.tbZona.Text = ""
		Me.tbTelefono2.Text = ""
	End Sub
	
    Protected Sub btnAnnullaDest_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAnnullaDest.Click
        'btInviaOrdine.Enabled = True
		clear_destinazione_alternativa
		Session("cityBinding") = 0
    End Sub

	Protected Sub LstScegliIndirizzo_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstScegliIndirizzo.PreRender
		If LstScegliIndirizzo.items.count > 0 Then
			If LstScegliIndirizzo.SelectedValue <= 0 Then
				LstScegliIndirizzo.SelectedValue = calcola_indirizzo_spedizione_predefinito()
			End If
			compila_campi_destinazione_alternativa_o_indirizzo_spedizione(LstScegliIndirizzo.SelectedValue,Lst.indirizzoSpedizione)
			Me.CHKPREDEFINITO.visible = true
		Else
			Me.CHKPREDEFINITO.visible = false
		End If
        Session("SCEGLIINDIRIZZO") = LstScegliIndirizzo.SelectedItem.Value
		if Session("cityBinding")<>1
			open1.Style.Item("display") = ""
			open2.Style.Item("display") = ""
			panel.Style.Item("display") = "none"
			compila_campi_destinazione_alternativa_o_indirizzo_spedizione(LstScegliIndirizzo.SelectedValue,Lst.destinazioneAlternativa)
		else 
			open1.Style.Item("display") = "none"
			open2.Style.Item("display") = "none"
			panel.Style.Item("display") = ""
			if insOmod.value = "mod" then
				btnModDest.Style.Item("display") = ""
				btnElimDest.Style.Item("display") = "none"
				btnSalvaDest.Style.Item("display") = "none"
			else if insOmod.value = "ins" then
				btnModDest.Style.Item("display") = "none"
				btnElimDest.Style.Item("display") = "none"
				btnSalvaDest.Style.Item("display") = ""
			end if
			Session("cityBinding") = 0
		end if
    End Sub
	
    Protected Sub LstDestinazione_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstDestinazione.PreRender
        'If LstDestinazione.SelectedValue <= 0 Then
        '    LstDestinazione.SelectedValue = calcola_predefinito_destinazione_alternativa()
        'End If
		
        REM Session("DESTINAZIONEALTERNATIVA") = LstDestinazione.SelectedItem.Value

		REM btnElimDest.enabled = false
		REM btnModDest.enabled = false
		REM if LstDestinazione.Items(0).value = 0 then
			REM if Session("DESTINAZIONEALTERNATIVA") > 0 then
				REM LstDestinazione.Items.RemoveAt(0)
				REM btnModDest.enabled = true
				REM if LstDestinazione.items.count > 1 Then
					REM btnElimDest.enabled = true
				REM End If
			REM End If
		REM Else
			REM if LstDestinazione.items.count > 1 Then
				REM btnElimDest.enabled = true
				REM btnModDest.enabled = true
			REM End If
		REM End If

        REM 'Aggiorno i campi Text sottostanti per dar modo all'utente di modificare o inserire una nuova destinazione in modo facile
        REM if Session("VECCHIADESTINAZIONEALTERNATIVA") <> Session("DESTINAZIONEALTERNATIVA") Then
			REM compila_campi_destinazione_alternativa_o_indirizzo_spedizione(LstDestinazione.SelectedValue,Lst.destinazioneAlternativa)
			REM Session("VECCHIADESTINAZIONEALTERNATIVA") = Session("DESTINAZIONEALTERNATIVA")
		REM End if
    End Sub

    Protected Sub LstDestinazione_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstDestinazione.SelectedIndexChanged
		Session("VECCHIADESTINAZIONEALTERNATIVA") = Session("DESTINAZIONEALTERNATIVA")
        'If LstDestinazione.SelectedItem.Value <> "0" Then
        '    Session("DESTINAZIONEALTERNATIVA") = LstDestinazione.SelectedItem.Value
        'Else
        '    Session("DESTINAZIONEALTERNATIVA") = 0
        'End If
		
    End Sub

    Function calcola_indirizzo_spedizione_predefinito() As Integer
        Dim predefinito As Integer = 0
        Dim params As New Dictionary(Of String, String)
        params.add("@UtenteId", Session("UtentiID"))
        Dim dr = ExecuteQueryGetDataReader("id", "utentiindirizzi", "(UtenteId=@UtenteId) AND (Predefinito=1)", params)
        dr.Read()

        If dr.HasRows = True Then
            predefinito = dr.Item("id")
        End If

        dr.Close()

        Return predefinito
    End Function

    Function compila_campi_destinazione_alternativa_o_indirizzo_spedizione(ByVal idDestinazione As Integer, ByVal tipolst As Lst) As Integer
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sqlString As String = ""
        Dim dr As MySqlDataReader
        Dim predefinito As Integer = 0

        Try

            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()
			
            cmd.Connection = conn
            cmd.CommandType = CommandType.Text
			cmd.Parameters.AddWithValue("@id",idDestinazione)
            cmd.CommandText = "SELECT * FROM utentiindirizzi WHERE ID=@id"

            dr = cmd.ExecuteReader

            If dr.Read Then
				If tipolst = Lst.destinazioneAlternativa Then
					tbRagioneSocialeA.Text = dr.Item("RagioneSocialeA")
					tbNomeA.Text = dr.Item("NomeA")
					tbIndirizzo2.Text = dr.Item("IndirizzoA")
					riempi_ddl_citta(dr.Item("CapA"), ddlCitta2, tbProvincia2, dr.Item("CittaA").ToString)
					tbCap2.Text = dr.Item("CapA")
					tbProvincia2.Text = dr.Item("ProvinciaA")
					tbZona.Text = dr.Item("Zona")
					tbTelefono2.Text = dr.Item("TelefonoA")
					tbNote.Text = dr.Item("Note")
					If dr.Item("Predefinito") = 1 Then
						CHKPREDEFINITO.Checked = True
					Else
						CHKPREDEFINITO.Checked = False
					End If
				Else
					lblTab_RagioneSocialeSpedizione.Text = dr.Item("RagioneSocialeA")
					lblTab_NomeSpedizione.Text = dr.Item("NomeA")
					lblTab_IndirizzoSpedizione.Text = dr.Item("IndirizzoA")
					lblTab_CittaSpedizione.Text = dr.Item("CittaA")
					lblTab_CapSpedizione.Text = dr.Item("CapA")
					lblTab_ProvinciaSpedizione.Text = dr.Item("ProvinciaA")
					lblTab_ZonaSpedizione.Text = dr.Item("Zona")
					lblTab_TelSpedizione.Text = dr.Item("TelefonoA")
					lblTab_NotaDestinazione.Text = dr.Item("Note")
				End If
            End If

            dr.Close()
            conn.Close()

            Return predefinito
        Catch ex As Exception

        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

        End Try
    End Function

    Protected Sub gvArticoliGratis_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles gvArticoliGratis.PreRender
        Dim i As Integer

        'Carrello Gratis
        For i = 0 To gvArticoliGratis.Items.Count - 1
            Dim img As Image
            Dim dispo As Label
            Dim arrivo As Label
            Dim importo As Label
            Dim importoIvato As Label
            Dim peso As Label
            Dim tbQta As TextBox

            tbQta = gvArticoliGratis.Items(i).FindControl("tbQta")
            img = gvArticoliGratis.Items(i).FindControl("imgDispo")
            dispo = gvArticoliGratis.Items(i).FindControl("lblDispo")
            arrivo = gvArticoliGratis.Items(i).FindControl("lblArrivo")
            importo = gvArticoliGratis.Items(i).FindControl("lblImporto")
            importoIvato = gvArticoliGratis.Items(i).FindControl("lblImportoIvato")
            peso = gvArticoliGratis.Items(i).FindControl("lblPeso")

            qta = qta + tbQta.Text

            If qta > 0 Then

                If IvaTipo = 1 Then
                    importo.Visible = True
                    importoIvato.Visible = False
                    gvArticoliGratis.Items(i).FindControl("lblprezzo").Visible = True
                    gvArticoliGratis.Items(i).FindControl("lblprezzoivato").Visible = False
                    'Conteggio Totale per Buono Sconto
                    TotaleMerce += CDbl(importo.Text)
                ElseIf IvaTipo = 2 Then
                    importo.Visible = False
                    importoIvato.Visible = True
                    gvArticoliGratis.Items(i).FindControl("lblprezzo").Visible = False
                    gvArticoliGratis.Items(i).FindControl("lblprezzoivato").Visible = True
                    'Conteggio Totale per Buono Sconto
                    TotaleMerce += CDbl(importoIvato.Text)
                End If

                Session("TotaleMerce") = TotaleMerce

                imponibile = imponibile + importo.Text

                calcolo_iva = calcolo_iva + (CDbl(importoIvato.Text) - CDbl(importo.Text))

                imponibile_gratis = imponibile_gratis + importo.Text
                totale = totale + importoIvato.Text

                If peso.Text <> "" Then
                    pesoTotale = pesoTotale + peso.Text
                End If

                If DispoTipo = 1 Then

                    If dispo.Text > DispoMinima Then
                        img.ImageUrl = "~/images/verde.gif"
                        img.AlternateText = "Disponibile"
                    ElseIf dispo.Text > 0 Then
                        img.ImageUrl = "~/images/giallo.gif"
                        img.AlternateText = "Disponibilità Scarsa"
                    Else
                        If Not arrivo is Nothing andAlso arrivo.Text > 0 Then
                            img.ImageUrl = "~/images/azzurro.gif"
                            img.AlternateText = "In Arrivo"
                        Else
                            img.ImageUrl = "~/images/rosso.gif"
                            img.AlternateText = "Non Disponibile"
                        End If
                    End If

                ElseIf DispoTipo = 2 Then
                    img.Visible = False
                    dispo.Visible = True
                End If
            End If
        Next
    End Sub

    Protected Sub gvVettoriPromo_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles gvVettoriPromo.PreRender
        Dim i As Integer = 0
        Dim Selezione_Vettore As ConwayControls.Web.RadioButton
        'Dim Selezione_Vettore_Temp As ConwayControls.Web.RadioButton

        If indice_riga_da_selezionare > -1 Then
            '(indice_riga_da_selezionare - 2) e non (indice_riga_da_selezionare - 1) perchè il DataRowBound viene fatto una volta in più
            Selezione_Vettore = Me.gvVettoriPromo.Rows(indice_riga_da_selezionare - 2).FindControl("rbSpedizione")
            Selezione_Vettore.Enabled = True
            Selezione_Vettore.Checked = True

            Selezionato_Vettore_Promo = 1
        End If

        'For i = 0 To Me.gvVettoriPromo.Rows.Count - 1
        'Selezione_Vettore = Me.gvVettoriPromo.Rows(i).FindControl("rbSpedizione")

        'If Selezione_Vettore.Enabled = True Then
        'Selezione_Vettore.Checked = True
        'Selezionato_Vettore_Promo = 1
        'End If

        'If ((i = 1) And (Selezione_Vettore.Enabled = True)) Then
        'Selezione_Vettore_Temp = Me.gvVettoriPromo.Rows(i - 1).FindControl("rbSpedizione")
        'Selezione_Vettore_Temp.Enabled = False

        'If Selezione_Vettore.Enabled = True Then
        'Selezione_Vettore.Checked = True
        'Selezionato_Vettore_Promo = 1
        'End If
        'End If
        'Next

        'Nel caso ci sia nel carrello SOLO prodotti GRATIS
        If (imponibile - imponibile_gratis = 0) Then
            Me.Panel_SpedizioneGratis.Visible = True
        Else
            Me.Panel_SpedizioneGratis.Visible = False
        End If
    End Sub

    Protected Sub gvVettoriPromo_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles gvVettoriPromo.RowDataBound
        Dim Soglia As Label
        Dim Peso As Label
        Dim Costo As Label
        Dim Percentuale As Label
        Dim Selezione As ConwayControls.Web.RadioButton

        cont_indice_riga += 1

        If e.Row.RowType = DataControlRowType.DataRow Then
            Selezione = e.Row.FindControl("rbSpedizione")
            Soglia = e.Row.FindControl("lblSogliaMinima")
            Peso = e.Row.FindControl("lblPeso")
            Costo = e.Row.FindControl("lblCosto")
            Percentuale = e.Row.FindControl("lblPercentuale")

            If (Double.Parse(Soglia.Text) <= (imponibile - imponibile_gratis)) And (Double.Parse(Peso.Text) >= pesoTotale) Then
                'Selezione.Enabled = True
                'Selezione.Checked = True
                Selezione.Enabled = False
                Selezione.Checked = False

                Try
                    If (Percentuale.Text > 0) Then
                        Costo.Text = String.Format("{0:c}", ((imponibile - imponibile_gratis) / 100) * Double.Parse(Percentuale.Text))
                    End If
                Catch
                    Percentuale.Text = 0
                End Try

                'Salvo il costo minore
                If Double.Parse(Costo.Text.Replace("€ ", "")) < costo_promo_minimo Then
                    'Setto la riga da selezionare, quella con costo minore
                    costo_promo_minimo = Double.Parse(Costo.Text.Replace("€ ", ""))
                    indice_riga_da_selezionare = cont_indice_riga
                End If
            Else
                Selezione.Enabled = False
            End If

        End If
    End Sub

    Protected Sub rbSpedizioneGratis_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles rbSpedizioneGratis.PreRender
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand

        Dim AsssicurazionePercentuale As Double
        Dim AssicurazioneMinimo As Double
        Dim TotAssicurazione As Double

        If Me.rbSpedizioneGratis.Checked = True Then
            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            cmd.Connection = conn

            conn.Open()

            cmd.CommandType = CommandType.Text
            If Session("AziendaID") = 1 Then
                cmd.CommandText = "SELECT * FROM vettori WHERE id=-1"
            Else
                cmd.CommandText = "SELECT * FROM vettori WHERE id=-2"
            End If

            Dim dr As MySqlDataReader = cmd.ExecuteReader()
            dr.Read()

            If dr.HasRows Then
                'Spedizione
                Me.lblSpeseSped.Text = String.Format("{0:c}", 0)

                If Session("AziendaID") = 1 Then
                    Me.tbVettoriId.Text = "-1"
                Else
                    Me.tbVettoriId.Text = "-2"
                End If

                'Assicurazione
                AsssicurazionePercentuale = dr.Item("AssicurazionePercentuale")
                AssicurazioneMinimo = dr.Item("AssicurazioneMinimo")

                TotAssicurazione = (AsssicurazionePercentuale * Me.lblImponibile.Text) / 100
                If TotAssicurazione < AssicurazioneMinimo Then
                    TotAssicurazione = AssicurazioneMinimo
                End If

                Me.lblAssicurazione.Text = String.Format("{0:c}", TotAssicurazione)

                'Contrassegno
                Me.tbContrFisso.Text = dr.Item("ContrassegnoFisso")
                Me.tbContrPerc.Text = dr.Item("ContrassegnoPercentuale")
                Me.tbContrMinimo.Text = dr.Item("ContrassegnoMinimo")

                AggiornaSpeseAssicurazione()

                If AsssicurazionePercentuale = 0 Then
                    Me.cbAssicurazione.Checked = False
                    Me.cbAssicurazione.Enabled = False
                Else
                    Me.cbAssicurazione.Enabled = True
                End If

                If dr.Item("ContrassegnoPercentuale") = 0 Then
                    RitiroSede = True
                Else
                    RitiroSede = False
                End If
            End If
        End If
    End Sub

    Protected Sub Page_PreRenderComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRenderComplete
        'Nascondo i Pannelli quando non ci sono articoli nel carrello
        If (Me.gvArticoliGratis.Items.Count = 0) And (Me.Repeater1.items.Count = 0) Then
            Me.Panel_Unico.Visible = False
            Me.btContinua.Enabled = True
        Else
            Me.Panel_Unico.Visible = True
        End If

        If (controlla_articoli_quantita_zero() = 0) Then
            Qnt_Errata.Visible = True
        End If

        'Aggiorno una sola volta i prezzi degli articoli nel carrello
        'If (Request.QueryString("update") = Nothing) And (controlla_articoli_quantita_zero() = 1) Then
        Aggiorna_Prezzi_Carrello()
        'Response.Redirect("carrello.aspx?update=1")
        'End If

        'Buono Sconto
        If (Val(Session("BuonoSconto_id")) > 0) Then
            TB_BuonoSconto.Text = getBuonoScontoCodice(Val(Session("BuonoSconto_id")))
            TB_BuonoSconto.Enabled = False
        Else
            TB_BuonoSconto.Enabled = True

            checkOKBuonoSconto.Visible = False
            lblBuonoSconto.Text = String.Format("{0:c}", 0)
            lblBuonoScontoIVA.Text = String.Format("{0:c}", 0)
        End If

        If (gvArticoliGratis.Items.Count > 0) Or (Repeater1.items.Count > 0) Then
            TB_BuonoSconto_TextChanged(TB_BuonoSconto, New System.EventArgs)
            GV_BuoniSconti.DataBind()
        Else
            GV_BuoniSconti.Visible = False
            Session("BuonoSconto_id") = 0
        End If

        'Aggiorno i Pagamenti ed i relativi costi
        LeggiPagamenti()

        'Conteggi dell'iva
        lblIva.Text = "€ " & FormatNumber(calcola_iva(CDbl(lblSpeseSped.Text), Session("Iva_Vettori")) + CDbl(lblSpeseAss.Text) * (preleva_ValoreIva(Session("Iva_Utente")) / 100), 2)
        lblTotale.Text = "€ " & FormatNumber(CDbl(lblImponibile.Text) + CDbl(lblIva.Text) + CDbl(lblSpeseAss.Text) + CDbl(lblSpeseSped.Text) + CDbl(lblPagamento.Text), 2)

        'Aggiorno il valore del Buono Sconto
        If GV_BuoniSconti.Rows.Count > 0 Then
            Dim scontoPercentuale As Label = GV_BuoniSconti.Rows(0).Cells(0).FindControl("lbl_Percentuale_BuonoSconto")
            Dim scontoFisso As Label = GV_BuoniSconti.Rows(0).Cells(0).FindControl("lbl_scontoFisso_BuonoSconto")
            Dim scontoVettore As Label = GV_BuoniSconti.Rows(0).Cells(0).FindControl("lbl_scontoVettore")
            Dim valoreBuonoSconto As Label = GV_BuoniSconti.Rows(0).Cells(0).FindControl("lbl_valore_BuonoSconto")
            Dim totSconto As Label = GV_BuoniSconti.Rows(0).Cells(0).FindControl("lbl_TotSconto")

            'Controllo che lo sconto da applicare non sia uno sconto vettore
            If Val(scontoVettore.Text) = 1 Then
                lblBuonoSconto.Text = "€ " & FormatNumber(-CDbl(lblSpeseSped.Text.Replace("€ ", "")) - ((CDbl(lblSpeseSped.Text) / 100) * Session("Iva_Vettori")), 2)
            Else
                lblBuonoSconto.Text = "€ " & FormatNumber(-CDbl(IIf(CDbl(scontoPercentuale.Text) > 0, (CDbl(TotaleMerce) / 100) * CDbl(valoreBuonoSconto.Text), CDbl(valoreBuonoSconto.Text))), 2)
            End If

            lblBuonoScontoIVA.Text = "€ " & FormatNumber(CDbl(lblBuonoSconto.Text) - (FormatNumber(CDbl(lblBuonoSconto.Text) / (1 + preleva_ValoreIva(Session("Iva_Utente")) / 100), 2)))
            lblBuonoSconto.Text = "€ " & FormatNumber(CDbl(lblBuonoSconto.Text) + (-1 * CDbl(lblBuonoScontoIVA.Text)), 2)
            lblIva.Text = "€ " & FormatNumber(CDbl(lblIva.Text) + CDbl(lblBuonoScontoIVA.Text), 2)
            totSconto.Text = IIf(CDbl(scontoPercentuale.Text) > 0, "Sconto in percentuale " & CDbl(valoreBuonoSconto.Text) & "%", IIf(Val(scontoVettore.Text) > 0, "SPEDIZIONE OMAGGIO", "Sconto fisso euro " & CDbl(valoreBuonoSconto.Text))) & "<br/>" & String.Format("{0:c}", CDbl(lblBuonoSconto.Text) + CDbl(lblBuonoScontoIVA.Text))
        End If

		Dim totaleTemp As Double = CDbl(lblImponibile.Text) + CDbl(lblIva.Text) + CDbl(lblSpeseAss.Text) + CDbl(lblSpeseSped.Text) + CDbl(lblPagamento.Text) + CDbl(lblBuonoSconto.Text)
		totaleTemp = Math.Round(totaleTemp, 2, MidpointRounding.AwayFromZero)
        lblTotale.Text =  "€ " & FormatNumber(totaleTemp, 2) 

        Session("Calcolo_Iva") = lblIva.Text

        'Simulo il Click del tasto btCompleta
        'If Page.IsPostBack = False Then
        '    btCompleta_Click(sender, e)
        '    LeggiPagamenti()
        '    LeggiVettori()
        'End If

        'Visualizzo o meno il pannello relativo ai Buoni Sconti, in base alle impostazioni nell'azienda
        If (Session("AbilitaBuoniScontiCarrello") = 1) AndAlso (TableConteggi.Visible = True) Then
            Panel_BuoniSconto.Visible = True
        Else
            Panel_BuoniSconto.Visible = False
        End If
    End Sub

    'Restituisce 1, se il controllo è andato a buon fine, altrimenti 0
    Function controlla_articoli_quantita_zero() As Integer
        Dim row As RepeaterItem

        'Controllo che non ci siano articoli con quantità zero
        If Repeater1.items.Count > 0 Then
            For Each row In Repeater1.items
                Dim Qta As TextBox = row.FindControl("tbQta")
                If (CLng(Qta.Text) <= 0) Then
                    Return 0
                End If
            Next
        End If

        'Controllo che non ci siano articoli con quantità zero
        If Me.gvArticoliGratis.items.Count > 0 Then
            For Each row In gvArticoliGratis.items
                Dim Qta As TextBox = row.FindControl("tbQta")
                If (CLng(Qta.Text) <= 0) Then
                    Return 0
                End If
            Next
        End If

        Return 1
    End Function

    Sub Aggiorna_Prezzi_Carrello()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sb As StringBuilder = New StringBuilder()
        Dim row As RepeaterItem

        If (controlla_articoli_quantita_zero() = 0) Then
            Qnt_Errata.Visible = True
        End If

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn.Open()

        cmd.Connection = conn
        cmd.CommandType = CommandType.Text

        If Repeater1.items.Count > 0 Then
            cmd.Parameters.Add("@artId", MySqlDbType.Int32)
            cmd.Parameters.Add("@listino", MySqlDbType.Int32)
            For Each row In Repeater1.items

                Dim Qta As TextBox = row.FindControl("tbQta")
                Dim ID As TextBox = row.FindControl("tbID")
                Dim ArtID As TextBox = row.FindControl("tbArtID")
                Dim Listino As Integer = Session("listino")
                Dim Prezzo As Double = 0
                Dim PrezzoIvato As Double = 0
                Dim OfferteDettagliID As Long = 0
                Dim IdIvaRC As Integer = -1
                Dim ValoreIvaRC As Double = -1
                Dim DescrizioneIvaRC As String = ""
                Dim IdEsenzioneIva As Integer = Session("IdEsenzioneIva")
                Dim ValoreEsenzioneIva As Double = Session("Iva_Utente")
                Dim DescrizioneEsenzioneIva As String = Session("DescrizioneEsenzioneIva")
                cmd.Parameters("@artId").value = ArtID.Text
                cmd.Parameters("@listino").value = Listino
                cmd.CommandText = "SELECT * FROM vsuperarticoli WHERE ID=@artId AND NListino=@listino GROUP BY offerteQntMinima, offerteMultipli, nlistino ORDER BY PrezzoPromo DESC"

                Dim dr As MySqlDataReader = cmd.ExecuteReader()

                While dr.Read()
                    'Setto l'iva dell'Utente momentaneamente se l'articolo preso in corsiderazione è in reverse charge
                    'Dim temp_iva_utente As Integer = -1
                    'If (Session("ReverseCharge_Utente") = 1) And (dr.Item("IdIvaReserveCharge") = 1) Then
                    'temp_iva_utente = Session("Iva_Utente")
                    'Session("Iva_Utente") = dr.Item("IdIvaReserveCharge")
                    'End If
                    '--------------------------------------------------------------------------------------------------

                    OfferteDettagliID = 0
                    If Prezzo = 0 Then
                        Prezzo = dr.Item("prezzo")
                    End If
                    If PrezzoIvato = 0 Then
                        If ((Session("AbilitatoIvaReverseCharge") = 1) And (dr.Item("IdIvaRC") > -1)) Then
                            PrezzoIvato = (CType(dr.Item("prezzo"), Double) * ((dr.Item("ValoreIvaRC") / 100) + 1))
                        Else
                            If (Session("Iva_Utente") > -1) Then
                                PrezzoIvato = (CType(dr.Item("prezzo"), Double) * ((Session("Iva_Utente") / 100) + 1))
                            Else
                                PrezzoIvato = CType(dr.Item("prezzoIvato"), Double)
                            End If
                        End If
                    End If

                    If (dr.Item("InOfferta") = 1) Then
                        If ((DateDiff(DateInterval.Day, dr.Item("OfferteDataInizio"), Date.Now) >= 0) And (DateDiff(DateInterval.Day, dr.Item("OfferteDataFine"), Date.Now) <= 0)) Then
                            If Qta.Text >= dr.Item("OfferteQntMinima") And dr.Item("OfferteQntMinima") > 0 Then
                                OfferteDettagliID = dr.Item("OfferteDettagliId")
                                Prezzo = dr.Item("prezzopromo")

                                If ((Session("AbilitatoIvaReverseCharge") = 1) And (dr.Item("IdIvaRC") > -1)) Then
                                    PrezzoIvato = Prezzo * ((dr.Item("ValoreIvaRC") / 100) + 1)
                                Else
                                    If (Session("Iva_Utente") > -1) Then
                                        PrezzoIvato = (CType(dr.Item("prezzopromo"), Double) * ((Session("Iva_Utente") / 100) + 1))
                                    Else
                                        PrezzoIvato = dr.Item("prezzopromoIvato")
                                    End If
                                End If
                            ElseIf Qta.Text Mod dr.Item("OfferteMultipli") = 0 And dr.Item("OfferteMultipli") > 0 Then
                                OfferteDettagliID = dr.Item("OfferteDettagliId")
                                Prezzo = dr.Item("prezzopromo")

                                If ((Session("AbilitatoIvaReverseCharge") = 1) And (dr.Item("IdIvaRC") > -1)) Then
                                    PrezzoIvato = Prezzo * ((dr.Item("ValoreIvaRC") / 100) + 1)
                                Else
                                    If (Session("Iva_Utente") > -1) Then
                                        PrezzoIvato = (CType(dr.Item("prezzopromo"), Double) * ((Session("Iva_Utente") / 100) + 1))
                                    Else
                                        PrezzoIvato = dr.Item("prezzopromoIvato")
                                    End If
                                End If
                            End If
                        End If
                    End If

                    'Stampo a video l'applicazione del ReverseCharge sull'articolo
                    If ((Session("AbilitatoIvaReverseCharge") = 1) And (dr.Item("IdIvaRC") > -1)) Then
                        IdIvaRC = dr.Item("IdIvaRC")
                        ValoreIvaRC = dr.Item("ValoreIvaRC")
                        DescrizioneIvaRC = dr.Item("DescrizioneIvaRC")
                    Else
                        IdIvaRC = -1
                        ValoreIvaRC = -1
                        DescrizioneIvaRC = ""
                    End If

                    'Risetto l'iva Utente allo stato precedente
                    'Session("Iva_Utente") = temp_iva_utente
                    '--------------------------------------------------------------------------------------------------
                End While

                dr.Close()
                dr.Dispose()
                cmd.Dispose()
                Dim params As New Dictionary(Of String, String)
                params.add("@id", ID.Text)
                params.add("@Qnt", IIf(CLng(Qta.Text) <= 0, 0, CLng(Qta.Text)))
                params.add("@OfferteDettaglioId", CLng(OfferteDettagliID))
                params.add("@Prezzo", Prezzo.ToString.Replace(",", "."))
                params.add("@PrezzoIvato", PrezzoIvato.ToString.Replace(",", "."))
                params.add("@IdIvaRC", IdIvaRC)
                params.add("@ValoreIvaRC", ValoreIvaRC)
                params.add("@DescrizioneIvaRC", DescrizioneIvaRC)
                params.add("@IdEsenzioneIva", IdEsenzioneIva)
                params.add("@ValoreEsenzioneIva", ValoreEsenzioneIva)
                params.add("@DescrizioneEsenzioneIva", DescrizioneEsenzioneIva)
                ExecuteUpdate("carrello", "Qnt = @Qnt, OfferteDettaglioId = @OfferteDettaglioId, Prezzo = @Prezzo, PrezzoIvato = @PrezzoIvato, IdIvaRC = @IdIvaRC, ValoreIvaRC = @ValoreIvaRC, DescrizioneIvaRC = @DescrizioneIvaRC, IdEsenzioneIva = @IdEsenzioneIva, ValoreEsenzioneIva = @ValoreEsenzioneIva, DescrizioneEsenzioneIva = @DescrizioneEsenzioneIva", "WHERE ID = @id", params)
            Next
        End If


        If Me.gvArticoliGratis.Items.Count > 0 Then
            For Each row In gvArticoliGratis.Items

                Dim Qta As TextBox = row.FindControl("tbQta")
                Dim ID As TextBox = row.FindControl("tbID")
                Dim ArtID As TextBox = row.FindControl("tbArtID")
                Dim Listino As Integer = Session("listino")
                Dim Prezzo As Double = 0
                Dim PrezzoIvato As Double = 0
                Dim OfferteDettagliID As Long = 0
                Dim IdIvaRC As Integer = -1
                Dim ValoreIvaRC As Double = -1
                Dim DescrizioneIvaRC As String = ""
                Dim IdEsenzioneIva As Integer = Session("IdEsenzioneIva")
                Dim ValoreEsenzioneIva As Double = Session("Iva_Utente")
                Dim DescrizioneEsenzioneIva As String = Session("DescrizioneEsenzioneIva")
                cmd.Parameters.AddWithValue("@artId", ArtID.Text)
                cmd.Parameters.AddWithValue("@listino", Listino)
                cmd.CommandText = "SELECT * FROM vsuperarticoli WHERE ID=@artId AND NListino=@listino GROUP BY offerteQntMinima, offerteMultipli, nlistino ORDER BY PrezzoPromo DESC"

                Dim dr As MySqlDataReader = cmd.ExecuteReader()

                While dr.Read()

                    OfferteDettagliID = 0
                    If Prezzo = 0 Then
                        Prezzo = dr.Item("prezzo")
                    End If
                    If PrezzoIvato = 0 Then
                        If (Session("Iva_Utente") > -1) Then
                            PrezzoIvato = (CType(dr.Item("prezzo"), Double) * ((Session("Iva_Utente") / 100) + 1))
                        Else
                            PrezzoIvato = CType(dr.Item("prezzoIvato"), Double)
                        End If
                    End If

                    If dr.Item("InOfferta") = 1 Then
                        If ((CDate(dr.Item("OfferteDataInizio")).ToString("MM/dd/yyyy") <= Date.Now.ToString("MM/dd/yyyy")) And (CDate(dr.Item("OfferteDataFine")).ToString("MM/dd/yyyy") >= Date.Now.ToString("MM/dd/yyyy"))) Then
                            If Qta.Text >= dr.Item("OfferteQntMinima") And dr.Item("OfferteQntMinima") > 0 Then
                                OfferteDettagliID = dr.Item("OfferteDettagliId")
                                Prezzo = dr.Item("prezzopromo")

                                If ((Session("AbilitatoIvaReverseCharge") = 1) And (dr.Item("IdIvaRC") > -1)) Then
                                    PrezzoIvato = Prezzo * ((dr.Item("ValoreIvaRC") / 100) + 1)
                                Else
                                    If (Session("Iva_Utente") > -1) Then
                                        PrezzoIvato = (CType(dr.Item("prezzopromo"), Double) * ((Session("Iva_Utente") / 100) + 1))
                                    Else
                                        PrezzoIvato = dr.Item("prezzopromoIvato")
                                    End If
                                End If
                            ElseIf Qta.Text Mod dr.Item("OfferteMultipli") = 0 And dr.Item("OfferteMultipli") > 0 Then
                                OfferteDettagliID = dr.Item("OfferteDettagliId")
                                Prezzo = dr.Item("prezzopromo")

                                If ((Session("AbilitatoIvaReverseCharge") = 1) And (dr.Item("IdIvaRC") > -1)) Then
                                    PrezzoIvato = Prezzo * ((dr.Item("ValoreIvaRC") / 100) + 1)
                                Else
                                    If (Session("Iva_Utente") > -1) Then
                                        PrezzoIvato = (CType(dr.Item("prezzopromo"), Double) * ((Session("Iva_Utente") / 100) + 1))
                                    Else
                                        PrezzoIvato = dr.Item("prezzopromoIvato")
                                    End If
                                End If
                            End If
                        End If
                    End If

                    'Stampo a video l'applicazione del ReverseCharge sull'articolo
                    If ((Session("AbilitatoIvaReverseCharge") = 1) And (dr.Item("IdIvaRC") > -1)) Then
                        IdIvaRC = dr.Item("IdIvaRC")
                        ValoreIvaRC = dr.Item("ValoreIvaRC")
                        DescrizioneIvaRC = dr.Item("DescrizioneIvaRC")
                    End If
                End While

                dr.Close()
                dr.Dispose()
                cmd.Dispose()

                Dim params As New Dictionary(Of String, String)
                params.add("@id", ID.Text)
                params.add("@Qnt", IIf(CLng(Qta.Text) <= 0, 0, CLng(Qta.Text)))
                params.add("@OfferteDettaglioId", CLng(OfferteDettagliID))
                params.add("@Prezzo", Prezzo.ToString.Replace(",", "."))
                params.add("@PrezzoIvato", PrezzoIvato.ToString.Replace(",", "."))
                params.add("@IdIvaRC", IdIvaRC)
                params.add("@ValoreIvaRC", ValoreIvaRC)
                params.add("@DescrizioneIvaRC", DescrizioneIvaRC)
                Dim fieldAndValues As String = "Qnt = @Qnt, OfferteDettaglioId = @OfferteDettaglioId, Prezzo = @Prezzo, PrezzoIvato = @PrezzoIvato, IdIvaRC = @IdIvaRC, ValoreIvaRC = @ValoreIvaRC, DescrizioneIvaRC = @DescrizioneIvaRC"
                If IdEsenzioneIva <> -1 Then
                    params.add("@IdEsenzioneIva", IdEsenzioneIva)
                    params.add("@ValoreEsenzioneIva", ValoreEsenzioneIva)
                    params.add("@DescrizioneEsenzioneIva", DescrizioneEsenzioneIva)
                    fieldAndValues &= "IdEsenzioneIva = @IdEsenzioneIva, ValoreEsenzioneIva = @ValoreEsenzioneIva, DescrizioneEsenzioneIva = @DescrizioneEsenzioneIva"
                End If
                ExecuteUpdate("carrello", fieldAndValues, "WHERE ID = @id", params)
            Next
        End If

        conn.Close()
        conn.Dispose()
    End Sub

    Protected Sub btSvuota_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btSvuota.Click
        Dim LoginId As Integer = Me.Session("LoginId")
        Dim SessionID As String = Me.Session.SessionID
        Me.sdsArticoli.DeleteParameters.Clear()
        If LoginId = 0 Then
            Me.sdsArticoli.DeleteParameters.Add("@SessionID", SessionID)
            Me.sdsArticoli.DeleteCommand = "delete from carrello where (SessionID=@SessionID)"
        Else
            Me.sdsArticoli.DeleteParameters.Add("@LoginId", LoginId)
            Me.sdsArticoli.DeleteCommand = "delete from carrello where (LoginId=@LoginId)"
        End If

        Me.sdsArticoli.Delete()
    End Sub

    Protected Sub btCompleta_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btCompleta.Click
        'Aggiorno i prodotti e il prezzo
        Aggiorna_Prezzi_Carrello()

        'Disabilito il completa ordine, quando già cliccato
        Me.btCompleta.Visible = False

        If Me.tOrdine.Visible = True Then
            Me.tOrdine.Visible = False
            Me.TableConteggi.Visible = False
            Me.btAggiorna.Enabled = True
            Me.btContinua.Enabled = True
            Me.btSvuota.Enabled = True
            'Me.Repeater1.DataBind()
            Me.lblPagamento.Text = String.Format("{0:c}", CDbl("0"))
            Me.lblSpeseSped.Text = String.Format("{0:c}", CDbl("0"))
            Me.lblSpeseAss.Text = String.Format("{0:c}", CDbl("0"))
            Me.lblPagamento.Text = String.Format("{0:c}", CDbl("0"))
        Else
            Me.TableConteggi.Visible = True
            Me.tOrdine.Visible = True
            Me.btAggiorna.Enabled = True
            Me.btContinua.Enabled = True
            Me.btSvuota.Enabled = True
        End If


        FillTableInfo()

        BindLstDestinazioneLstScegliIndirizzo

        'Me.LblDescrDest.Text = "Destinazione predefinita: " & vbCrLf & Me.getIndirizzoPrincipale

    End Sub

    Protected Sub btnModDest_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnModDest.Click
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sqlString As String = ""
        Dim dsData As New DataSet
		Dim sqlStringPredefinito As String = ""
		
        Try
            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()

            cmd.Connection = conn
            cmd.CommandType = CommandType.Text

			If CHKPREDEFINITO.Checked Then
				sqlString = "UPDATE utentiindirizzi SET Predefinito=0 WHERE UtenteId=@UtentiId"
				cmd.Parameters.AddWithValue("@UtentiId",Session("UTENTIID"))
				cmd.CommandText = sqlString
                cmd.ExecuteNonQuery()
				sqlStringPredefinito = "    , PREDEFINITO= 1 " 
			Else
				sqlStringPredefinito = "    , PREDEFINITO= 0 " 
			End If
			
            sqlString = " UPDATE utentiindirizzi SET "
            sqlString &= "  @ragioneSocialeA, "
            sqlString &= "  @nomeA,"
            sqlString &= "  @indirizzo2, "
            sqlString &= "  @cap2, "
            sqlString &= "  @citta, "
            sqlString &= "  @provincia, "
            sqlString &= "  @note, "
            sqlString &= "  @zona, "
			sqlString &= "  @telefono2"
			sqlString &= sqlStringPredefinito
            sqlString &= "  WHERE Id=" & LstScegliIndirizzo.SelectedValue
			
			cmd.Parameters.AddWithValue("@UtentiId",Session("UTENTIID"))
            cmd.Parameters.AddWithValue("@ragioneSocialeA", Me.tbRagioneSocialeA.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@nomeA", Me.tbNomeA.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@indirizzo2", Me.tbIndirizzo2.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@cap2", Me.tbCap2.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@citta", getDdlCittaValue(Me.ddlCitta2).Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@provincia", Me.tbProvincia2.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@note", Me.tbNote.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@telefono2", Me.tbTelefono2.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@zona", Me.tbZona.Text.Replace("'", "''").ToUpper)
            cmd.CommandText = sqlString
            cmd.ExecuteNonQuery()

			If CHKPREDEFINITO.Checked = false Then
                sqlString = "UPDATE utentiindirizzi SET Predefinito=1 WHERE UtenteId=@UtentiId ORDER BY Id DESC LIMIT 1"
                cmd.CommandText = sqlString
				cmd.ExecuteNonQuery()
			End If
			
            cmd.Dispose()

            clear_destinazione_alternativa

            Me.RFRagioneSocialeA.Enabled = False
            Me.RFIndirizzo2.Enabled = False
            Me.RFCitta2.Enabled = False
            Me.RFProvincia2.Enabled = False
            Me.RFCap2.Enabled = False
			Me.RFTelefono2.Enabled = False

        Catch ex As Exception
'Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('" & ex.message().Replace("'", """").ToUpper & "')}</script>")

        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

        End Try
    End Sub
	
	Protected Sub btnSalvaDest_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSalvaDest.Click

        'btInviaOrdine.Enabled = True

        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sqlString As String = ""
        Dim dsData As New DataSet
		Dim sqlStringPredefinitoField As String = ""
		Dim sqlStringPredefinitoValue As String = ""
		
        Try

            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()

            cmd.Connection = conn
            cmd.CommandType = CommandType.Text

			If CHKPREDEFINITO.Checked Then
				sqlString = "UPDATE utentiindirizzi SET Predefinito=0 WHERE UtenteId=@UtentiId"
				cmd.Parameters.AddWithValue("@UtentiId",Session("UTENTIID"))
                cmd.CommandText = sqlString
                cmd.ExecuteNonQuery()
				
				sqlStringPredefinitoField = "    , PREDEFINITO "
				sqlStringPredefinitoValue = ", 1"
			Else If LstScegliIndirizzo.items.count = 0 Then
				sqlStringPredefinitoField = "    , PREDEFINITO "
				sqlStringPredefinitoValue = ", 1"
			End If

			sqlString = " INSERT INTO utentiindirizzi ("
            sqlString &= "      UTENTEID, "
            sqlString &= "      RAGIONESOCIALEA, "
            sqlString &= "      NOMEA, "
            sqlString &= "      INDIRIZZOA, "
            sqlString &= "      CAPA, "
            sqlString &= "      CITTAA, "
            sqlString &= "      PROVINCIAA, "
            sqlString &= "      NOTE, "
			sqlString &= "      TELEFONOA, "
            sqlString &= "      ZONA "
            sqlString &= sqlStringPredefinitoField
            sqlString &= " )"
            sqlString &= " VALUES "
            sqlString &= " ("
            sqlString &= "  @utentiId, "
            sqlString &= "  @ragioneSocialeA, "
            sqlString &= "  @nomeA,"
            sqlString &= "  @indirizzo2, "
            sqlString &= "  @cap2, "
            sqlString &= "  @citta, "
            sqlString &= "  @provincia, "
            sqlString &= "  @note, "
			sqlString &= "  @telefono2, "
            sqlString &= "  @zona"
            sqlString &= sqlStringPredefinitoValue
			sqlString &= " )"
			
			cmd.Parameters.AddWithValue("@UtentiId",Session("UTENTIID"))
            cmd.Parameters.AddWithValue("@ragioneSocialeA", Me.tbRagioneSocialeA.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@nomeA", Me.tbNomeA.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@indirizzo2", Me.tbIndirizzo2.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@cap2", Me.tbCap2.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@citta", getDdlCittaValue(Me.ddlCitta2).Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@provincia", Me.tbProvincia2.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@note", Me.tbNote.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@telefono2", Me.tbTelefono2.Text.Replace("'", "''").ToUpper)
            cmd.Parameters.AddWithValue("@zona", Me.tbZona.Text.Replace("'", "''").ToUpper)

            cmd.CommandText = sqlString
            cmd.ExecuteNonQuery()
			
            cmd.Dispose()

            clear_destinazione_alternativa

            Me.RFRagioneSocialeA.Enabled = False
            Me.RFIndirizzo2.Enabled = False
            Me.RFCitta2.Enabled = False
            Me.RFProvincia2.Enabled = False
            Me.RFCap2.Enabled = False
			Me.RFTelefono2.Enabled = False

        Catch ex As Exception
		
        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

        End Try

    End Sub
	
	
    Protected Sub btnElimDest_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnElimDest.Click
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sqlString As String = ""
        Dim dsData As New DataSet

		if LstScegliIndirizzo.Items.Count>1 Then
			Try
				conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
				conn.Open()

				cmd.Connection = conn
				cmd.CommandType = CommandType.Text
				
				sqlString = "SELECT * FROM utentiindirizzi WHERE Id=@id"
				cmd.Parameters.AddWithValue("@id",LstScegliIndirizzo.SelectedValue)
				cmd.CommandText = sqlString
				Dim dr As MySqlDataReader = cmd.ExecuteReader()
				dr.Read()
				Dim predefinito As Integer = dr.item("predefinito")
				dr.close()
				
				sqlString = "DELETE FROM utentiindirizzi WHERE Id=@id"
				cmd.CommandText = sqlString
				cmd.ExecuteNonQuery()
				
				if predefinito = 1 Then
					sqlString = "UPDATE utentiindirizzi SET Predefinito=1 WHERE UtenteId=@UtentiId ORDER BY Id DESC LIMIT 1"
					cmd.Parameters.AddWithValue("@UtentiId",Session("UTENTIID"))
					cmd.CommandText = sqlString
					cmd.ExecuteNonQuery()
				End If
				
				cmd.Dispose()

				clear_destinazione_alternativa

				Me.RFRagioneSocialeA.Enabled = False
				Me.RFIndirizzo2.Enabled = False
				Me.RFCitta2.Enabled = False
				Me.RFProvincia2.Enabled = False
				Me.RFCap2.Enabled = False
				Me.RFTelefono2.Enabled = False

			Catch ex As Exception

			Finally

				If conn.State = ConnectionState.Open Then
					conn.Close()
					conn.Dispose()
				End If

			End Try
		End If
    End Sub

    'Mi permette di leggere dal vettore l'IVA impostata per il Vettori
    Function IvaVettore(ByVal idVettore As Integer) As Double
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim dr As MySqlDataReader
        Dim temp_iva As Double = 0

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn.Open()

        cmd.Connection = conn
        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT vettori.*, iva.Valore FROM vettori LEFT JOIN iva ON vettori.iva=iva.id WHERE vettori.id= @IdVettore"
		cmd.Parameters.AddWithValue("@IdVettore",idVettore)
        dr = cmd.ExecuteReader()
        dr.Read()

        If dr.HasRows = True Then
            temp_iva = dr.Item("Valore")
        End If

        dr.Close()
        conn.Close()

        Return temp_iva
    End Function

    Function preleva_ValoreIva(ByVal idIva As Integer) As Double
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim dr As MySqlDataReader
        Dim temp_iva As Double = 0

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn.Open()

        cmd.Connection = conn
        cmd.CommandType = CommandType.Text

        If idIva = -1 Then
            cmd.CommandText = "SELECT iva.Valore FROM ivadefault INNER JOIN iva ON ivadefault.IvaVId=iva.id WHERE CURDATE() BETWEEN dal AND al"
        Else
			cmd.Parameters.AddWithValue("@idIva",idIva)
            cmd.CommandText = "SELECT iva.Valore FROM iva WHERE iva.id=@idIva"
        End If


        dr = cmd.ExecuteReader()
        dr.Read()

        If dr.HasRows = True Then
            temp_iva = dr.Item("Valore")
        End If

        dr.Close()
        conn.Close()

        Return temp_iva
    End Function

    Function preleva_IdIva(ByVal idIva As Integer) As Integer
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim dr As MySqlDataReader
        Dim risultato As Integer = 0

        If idIva = -1 Then
            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()

            cmd.Connection = conn
            cmd.CommandType = CommandType.Text

            cmd.CommandText = "SELECT IvaVid FROM ivadefault INNER JOIN iva ON ivadefault.IvaVId=iva.id WHERE CURDATE() BETWEEN dal AND al"

            dr = cmd.ExecuteReader()
            dr.Read()

            If dr.HasRows = True Then
                risultato = dr.Item("IvaVid")
            End If

            dr.Close()
            conn.Close()
        Else
            risultato = idIva
        End If

        Return risultato
    End Function

    Function calcola_iva(ByVal Spese_Spedizione As Double, ByVal ValoreIvaVettore As Integer) As Double

        Dim row As RepeaterItem

        Dim lblValoreIva As Label
        Dim lblIdIvaReverseCharge As Label
        Dim lblPrezzo As Label
        Dim Qnt As TextBox

        Dim ValoreIva As Double
        Dim ValoreIvaReverseCharge As Double
        Dim tot_iva As Double = 0

        'Riempio il data Table, relativo ai prodotti Normali, NO SPedizione GRATIS
        If Repeater1.items.Count > 0 Then
            For Each row In Repeater1.items
                ValoreIva = 0
                lblValoreIva = row.FindControl("lblValoreIva")
                lblIdIvaReverseCharge = row.FindControl("lblidIvaRC")
                lblPrezzo = row.FindControl("lblPrezzo")
                Qnt = row.FindControl("tbQta")

                'Caso in cui l'utente ha una propria iva (Esenzione)
                If Session("Iva_Utente") > -1 Then
                    ValoreIva = preleva_ValoreIva(Session("Iva_Utente"))
                Else
                    'Caso in cui applicare Reverse Charge
                    If ((Session("AbilitatoIvaReverseCharge") = 1) And (lblIdIvaReverseCharge.Text <> -1)) Then
                        ValoreIvaReverseCharge = preleva_ValoreIva(lblIdIvaReverseCharge.Text)
                    Else
                        ValoreIva = lblValoreIva.Text
                    End If
                End If

                tot_iva = tot_iva + (CDbl(lblPrezzo.Text * Qnt.Text) / 100 * ValoreIva)
            Next
        End If

        'Riempio il data Table, relativo ai prodotti con Spedizione Gratis
        If gvArticoliGratis.Items.Count > 0 Then
            For Each row In gvArticoliGratis.Items
                ValoreIva = 0
                lblValoreIva = row.FindControl("lblValoreIva")
                lblIdIvaReverseCharge = row.FindControl("lblidIvaRC")
                lblPrezzo = row.FindControl("lblPrezzo")
                Qnt = row.FindControl("tbQta")

                'Caso in cui l'utente ha una propria iva (Esenzione)
                If Session("Iva_Utente") > -1 Then
                    ValoreIva = preleva_ValoreIva(Session("Iva_Utente"))
                Else
                    'Caso in cui applicare Reverse Charge
                    If ((Session("AbilitatoIvaReverseCharge") = 1) And (lblIdIvaReverseCharge.Text <> -1)) Then
                        ValoreIvaReverseCharge = preleva_ValoreIva(lblIdIvaReverseCharge.Text)
                    Else
                        ValoreIva = lblValoreIva.Text
                    End If
                End If
                tot_iva = tot_iva + (CDbl(lblPrezzo.Text * Qnt.Text) / 100 * ValoreIva)
            Next
        End If

        'Aggiungo le altre ive al conteggio
        tot_iva = tot_iva + (Spese_Spedizione / 100 * ValoreIvaVettore)

        Return Math.Round(tot_iva, 2, MidpointRounding.AwayFromZero)
    End Function
	
    Protected Sub Repeater1_ItemCommand(ByVal sender As Object, ByVal e As RepeaterCommandEventArgs) Handles Repeater1.ItemCommand
		If e.CommandName = "Aggiorna" Then
            btAggiorna_Click(sender, e)
        End If

        If e.CommandName = "Elimina" Then
            eliminaRigaCarrello(e.CommandArgument)
        End If
    End Sub

    Protected Sub gvArticoliGratis_ItemCommand(ByVal sender As Object, ByVal e As RepeaterCommandEventArgs) Handles gvArticoliGratis.ItemCommand
        If e.CommandName = "Aggiorna" Then
            btAggiorna_Click(sender, e)
        End If

        If e.CommandName = "Elimina" Then
            eliminaRigaCarrello(e.CommandArgument)
        End If
    End Sub


    Public Sub eliminaRigaCarrello(ByVal id As Integer)
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand

        Try
            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            cmd.Connection = conn
            conn.Open()

            cmd.CommandText = "DELETE FROM carrello WHERE (Id = @Id)"
            cmd.Parameters.AddWithValue("Id", id)

            cmd.ExecuteNonQuery()
        Catch

        Finally
            conn.Close()
        End Try
    End Sub

    Protected Sub TB_BuonoSconto_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TB_BuonoSconto.TextChanged
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim dr As MySqlDataReader
        Dim idArticolo As TextBox
        Dim OperatoreLogico As New Label

        'Scorriamo tutti gli elementi presenti nel carrello per vedere se è possibile o meno applicare il Buono Sconto.
        If TB_BuonoSconto.Text.Length > 0 Then
            'For i As Integer = 0 To Repeater1.Rows.Count - 1
            'idArticolo = Repeater1.Rows(i).FindControl("tbArtID")

            'If controllaValiditaBuonoSconto(TB_BuonoSconto.Text, Session("AziendaID"), Val(idArticolo.Text), Session("UtentiId"), Session("UtentiTipoId"), Session("Listino")) = 1 Then
            'Sostituire con TotaleMerce -> Type(lblTotale.Text.Replace("€ ", ""), Double) se si vuole che la soglia del buono possa essere superata anche includendo spese di spedizione e altro ... e non solo la merce
            If VerificaBuonoSconto(listaArticoliInCarrello, TB_BuonoSconto.Text, Session("AziendaID"), Session("Listino"), Session("UtentiId"), Session("TotaleMerce")) Then
               
				conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
                cmd.Connection = conn
                conn.Open()

                cmd.CommandText = "SELECT * FROM buoni_sconti WHERE buonoSconto=@CodiceBuonoSconto"
                cmd.Parameters.AddWithValue("@CodiceBuonoSconto", TB_BuonoSconto.Text)

                dr = cmd.ExecuteReader

                If dr.HasRows Then
                    dr.Read()
                    Session("BuonoSconto_id") = dr.Item("id")

                    TB_BuonoSconto.Enabled = False

                    checkOKBuonoSconto.Visible = True
                    checkNOBuonoSconto.Visible = False
                    'Descrizione convalida Codice Sconto
                    lblBuonoScontoConvalida.Text = "Buono Sconto Applicato"
                    lblBuonoScontoConvalida.ForeColor = Drawing.Color.Green
                    lblBuonoScontoConvalida.Font.Size = 8
                    'Nascondo il pulsante Applica Codice Sconto
                    BT_ApplicaBuonoSconto.Enabled = False
                    'Nascondo pulsante di cancellazione BuonoSconto
                    LB_CancelBuonoSconto.Visible = True
                End If

                'Exit For
            Else
                Session("BuonoSconto_id") = Nothing

                TB_BuonoSconto.Enabled = True
                lblBuonoSconto.Text = String.Format("{0:c}", 0)
                lblBuonoScontoIVA.Text = String.Format("{0:c}", 0)

                checkOKBuonoSconto.Visible = False
                checkNOBuonoSconto.Visible = True
                'Descrizione convalida Buono Sconto
                lblBuonoScontoConvalida.Text = "Buono Sconto non valido"
                lblBuonoScontoConvalida.ForeColor = Drawing.Color.Red
                lblBuonoScontoConvalida.Font.Size = 8
                'Visualizzo il pulsante Applica Codice Sconto
                BT_ApplicaBuonoSconto.Enabled = True
                'Visualizzo pulsante di cancellazione BuonoSconto
                LB_CancelBuonoSconto.Visible = False
            End If
            'Next
        End If
    End Sub

    Public Function listaArticoliInCarrello() As String
        Dim stringa As String = ""
		Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
		Dim dr As MySqlDataReader
		Dim LoginId As Integer = Me.Session("LoginId")
        Dim SessionID As String = Me.Session.SessionID
		Dim WhereUserId as String = ""

		conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn
        conn.Open()
		Dim Sqlstring As String = "SELECT vcarrello.*, articoli.SpedizioneGratis_Listini, articoli.SpedizioneGratis_Data_Inizio, articoli.SpedizioneGratis_Data_Fine, taglie.descrizione as taglia, colori.descrizione as colore FROM vcarrello"
        Sqlstring = Sqlstring + " LEFT OUTER JOIN articoli ON vcarrello.ArticoliId = articoli.id"
        Sqlstring = Sqlstring + " LEFT OUTER JOIN articoli_tagliecolori ON vcarrello.TCid = articoli_tagliecolori.id"
        Sqlstring = Sqlstring + " LEFT OUTER JOIN taglie ON articoli_tagliecolori.tagliaid = taglie.id"
        Sqlstring = Sqlstring + " LEFT OUTER JOIN colori ON articoli_tagliecolori.coloreid = colori.id"
        If LoginId = 0 Then
			cmd.Parameters.AddWithValue("@SessionId", SessionID)
            WhereUserId = "(SessionId=@SessionId)"
        Else
			cmd.Parameters.AddWithValue("@LoginId", LoginId)
            WhereUserId = "(LoginId=@LoginId)"
        End If
		cmd.Parameters.AddWithValue("@Listino",Me.Session("Listino"))
		dim query as string = Sqlstring & " WHERE " & WhereUserId & " AND (articoli.SpedizioneGratis_Listini = '' OR (articoli.SpedizioneGratis_Listini != '' AND (SpedizioneGratis_Listini NOT LIKE CONCAT('%', @Listino, ';%') OR SpedizioneGratis_Data_Fine < CURDATE() OR (SpedizioneGratis_Listini LIKE CONCAT('%', @Listino, ';%') AND SpedizioneGratis_Data_Inizio <= CURDATE() AND (SpedizioneGratis_Data_Fine >= CURDATE() OR SpedizioneGratis_Data_Fine Is NULL))))) ORDER BY id"
        
        cmd.CommandText = query

		dr = cmd.ExecuteReader
		while dr.Read()
			If stringa.Trim.Length = 0 Then
                stringa = dr.Item("articoliid")
            Else
                stringa += "," & dr.Item("articoliid")
            End If
		end while

        Return stringa
    End Function

    Public Function VerificaBuonoSconto(ByVal articoli As String, ByVal buonosconto As String, ByVal azienda As Integer, ByVal listino As String, ByVal utenteid As Integer, ByVal totaleMerceCarrello As Double) As Integer
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim retval As Boolean = False
        Dim tQuery As String = ""
        Dim verificaUtilizzoBuonoSconto As Integer = 0

        Dim sQuery As String = String.Empty   

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn
        conn.Open()
		cmd.Parameters.AddWithValue("@buonoSconto", buonosconto)
		cmd.Parameters.AddWithValue("@azienda", azienda)
		cmd.Parameters.AddWithValue("@listino", listino)
		cmd.Parameters.AddWithValue("@utenteid", utenteid)
		cmd.Parameters.AddWithValue("@totaleMerceCarrello", CDbl(totaleMerceCarrello).ToString.Replace(",", "."))
        cmd.CommandText = "Select id from documenti where codicebuonosconto=@buonosconto and utentiid=@utenteid"

        verificaUtilizzoBuonoSconto = cmd.ExecuteScalar()

        If verificaUtilizzoBuonoSconto <> 0 AndAlso getUtilizzoBuonoSconto(buonosconto, azienda) = 1 Then Return 0
 
        cmd.CommandText = "Select sSql from Buoni_Sconti where buonosconto=@buonoSconto and idAzienda=@azienda and ListaListini LIKE '%,@listino,%' and (listautentiid=',' OR listautentiid LIKE '%,@utenteid,%') and sogliaprezzo<=@totaleMerceCarrello and curdate() between datainizio and datafine"

        tQuery = cmd.ExecuteScalar()

        If Not tQuery Is Nothing Then
			cmd.Parameters.AddWithValue("@tQuery", tQuery)
			cmd.Parameters.AddWithValue("@articoli", articoli)
            cmd.CommandText = "SELECT CASE WHEN COUNT(articoli.id)>0 THEN 1 ELSE 0 END as Trovato FROM articoli INNER JOIN (@tQuery) AS Test ON articoli.id=Test.id WHERE Test.id IN (@articoli)"
            retval = cmd.ExecuteScalar
			
        End If

        cmd.Dispose()
        conn.Close()

        Return retval
    End Function

    Public Function controllaValiditaBuonoSconto(ByVal codiceBuono As String, Optional ByVal idAzienda As Integer = 0, Optional ByVal idArticolo As Integer = -1, Optional ByVal idUtente As Integer = -1, Optional ByVal idTipoUtente As Integer = -1, Optional ByVal idListinoUtente As Integer = -1) As Integer
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim dr As MySqlDataReader
        Dim operatoreLogico As String = ""
        Dim tipoOperatoreLogico As Integer = 0

        Dim idMarca As Integer
        Dim idSettore As Integer
        Dim idCategoria As Integer
        Dim idTipologia As Integer
        Dim idGruppo As Integer
        Dim idSottoGruppo As Integer

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn
        conn.Open()

        If idArticolo > -1 Then
            'Prelevo dal DataBase le info relative all'articolo
            cmd.CommandText = "SELECT * FROM articoli WHERE id=@idArticolo"
            cmd.Parameters.AddWithValue("@idArticolo", idArticolo)

            dr = cmd.ExecuteReader

            If dr.HasRows Then
                dr.Read()
                idMarca = dr.Item("MarcheId")
                idSettore = dr.Item("SettoriId")
                idCategoria = dr.Item("CategorieId")
                idTipologia = dr.Item("TipologieId")
                idGruppo = dr.Item("GruppiId")
                idSottoGruppo = dr.Item("SottogruppiId")
            End If

            dr.Close()
            '--------------------------------------------------
        End If

        'Controllo il tipo di operatore logico scelto
        cmd.CommandText = "SELECT * FROM buoni_sconti WHERE (buonoSconto=@buonoSconto) AND (idAzienda=@idAzienda)"
        cmd.Parameters.AddWithValue("@buonoSconto", codiceBuono)
        cmd.Parameters.AddWithValue("@idAzienda", idAzienda)

        dr = cmd.ExecuteReader

        If dr.HasRows Then
            dr.Read()
            tipoOperatoreLogico = dr.Item("operatoreLogico")
        End If

        dr.Close()
        '--------------------------------------------------

        If tipoOperatoreLogico = 1 Then
            operatoreLogico = " OR "
        Else
            operatoreLogico = " AND "
        End If

        'Creo il filtro di selezione
        Dim where As String = " WHERE "
        cmd.Parameters.AddWithValue("@codiceBuono", "")
        cmd.Parameters.AddWithValue("@id", "")

        If idMarca > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidMarca(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idMarca
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        If idSettore > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidSettore(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idSettore
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        If idCategoria > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidCategoria(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idCategoria
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        If idTipologia > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidTipologia(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idTipologia
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        If idGruppo > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidGruppo(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idGruppo
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        If idSottoGruppo > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidSottogruppo(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idSottoGruppo
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        If idArticolo > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidArticolo(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idArticolo
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        If idUtente > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidUtente(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idUtente
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        If idTipoUtente > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidTipoUtente(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idTipoUtente
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        If idListinoUtente > -1 Then
            cmd.CommandText = "SELECT buoniscontiperidListinoUtente(@codiceBuono,@id)"
            cmd.Parameters("@codiceBuono").Value = codiceBuono
            cmd.Parameters("@id").Value = idListinoUtente
            'Caso di 1 o più condizioni
            If where.Contains("(") Then
                where = where & operatoreLogico & cmd.ExecuteScalar
            Else
                where = where & cmd.ExecuteScalar
            End If
        End If

        'Chiudo i filtri per le proprietà dell'articolo
        where = where & ")"

        cmd.CommandText = "SELECT BuoniScontiPerUtilizzo(@codiceBuono,@idUtente,@idAzienda)"
        cmd.Parameters("@codiceBuono").Value = codiceBuono
        cmd.Parameters.AddWithValue("@idUtente", Session("UtentiId"))
        cmd.Parameters("@idAzienda").Value = idAzienda
        'Caso di 1 o più condizioni
        If where.Contains("(") Then
            where = where & " AND " & cmd.ExecuteScalar
        Else
            where = where & cmd.ExecuteScalar
        End If

        'Applico la soglia prezzo se impostata nel buono sconto
        cmd.CommandText = "SELECT BuoniScontiPerSogliaPrezzo(@codiceBuono,@totaleCarrello)"
        cmd.Parameters("@codiceBuono").Value = codiceBuono
        cmd.Parameters.AddWithValue("@totaleCarrello", CType(lblTotale.Text.Replace("€ ", ""), Double))
        'Caso di 1 o più condizioni
        If where.Contains("(") Then
            where = where & " AND " & cmd.ExecuteScalar
        Else
            where = where & cmd.ExecuteScalar
        End If

        'Aggiungo la parentesi iniziale per poi definire la condizione di sogliaPrezzo
        cmd.CommandText = "SELECT count(*) FROM buoni_sconti" & where.Replace("WHERE ", "WHERE (") & " AND ((buonoSconto=@codiceBuono) AND (idAzienda=@idAzienda))"

        Dim risultato As Integer = cmd.ExecuteScalar

        cmd.Dispose()
        conn.Close()

        If risultato > 0 Then
            Return 1
        Else
            Return 0
        End If
    End Function

    Protected Sub GV_BuoniSconti_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GV_BuoniSconti.RowCommand
        If e.CommandName = "CancellaBuonoSconto" Then
            Session("BuonoSconto_id") = Nothing
            TB_BuonoSconto.Text = ""
            lblBuonoScontoConvalida.Text = ""
            'Visualizzo il pulsante Applica Codice Sconto
            BT_ApplicaBuonoSconto.Enabled = True
        End If
    End Sub

    'Funzione che restituisce 1 se il buono può essere utilizzato solo una volta, 0 nel caso il buono possa essere utilizzato più volte
    Function getUtilizzoBuonoSconto(ByVal codiceBuonoSconto As String, ByVal idAzienda As Integer) As Integer
        Dim UtilizzaSoloUnaVolta As Integer = 0
        Dim paramsSelect As New Dictionary(Of String, String)
        paramsSelect.add("codiceBuono", codiceBuonoSconto)
        paramsSelect.add("idAzienda", idAzienda)
        Dim dr As MySqlDataReader = ExecuteQueryGetDataReader("UtilizzaSoloUnaVolta", "buoni_sconti", "(buonoSconto=@codiceBuono) AND (idAzienda=@idAzienda)", paramsSelect)
        dr.Read()

        If dr.HasRows = True Then
            UtilizzaSoloUnaVolta = dr.Item("UtilizzaSoloUnaVolta")
        End If

        dr.Close()

        Return UtilizzaSoloUnaVolta
    End Function

    Function getBuonoScontoCodice(ByVal idBuonoSconto As Integer) As String
        Dim codiceBuonoSconto As String = ""
        Dim paramsSelect As New Dictionary(Of String, String)
        paramsSelect.add("@IdBuonoScorto", idBuonoSconto)
        Dim dr As MySqlDataReader = ExecuteQueryGetDataReader("buonoSconto", "buoni_sconti", "id=@IdBuonoScorto", paramsSelect)
        dr.Read()

        If dr.HasRows = True Then
            codiceBuonoSconto = dr.Item("buonoSconto")
        End If

        dr.Close()

        Return codiceBuonoSconto
    End Function

    Protected Sub btContinua_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btContinua.Click
        If Session.Item("Pagina_visitata_Articoli") Is Nothing Then
            Response.Redirect("default.aspx")
        Else
			If Session.Item("Pagina_visitata_Articoli").ToString = String.Empty Then
				Response.Redirect("default.aspx")
			Else
				Response.Redirect(Session.Item("Pagina_visitata_Articoli").ToString) 'Aggiorno l'ultima pagina visitata
			End If
        End If
    End Sub

    Protected Sub btAggiorna_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btAggiorna.Click
        Aggiorna_Prezzi_Carrello()

        'Salvo in sessione il click del tasto aggiorna
        Session("Click_AggiornaCarrello") = 1
        Response.Redirect("carrello.aspx")
    End Sub

    Protected Sub LB_CancelBuonoSconto_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LB_CancelBuonoSconto.Click
        Session("BuonoSconto_id") = Nothing
        TB_BuonoSconto.Text = ""
        lblBuonoScontoConvalida.Text = ""
        'Visualizzo il pulsante Applica Codice Sconto
        BT_ApplicaBuonoSconto.Enabled = True
        'Nascondo il pulsante di cancellazione Buono Sconto
        LB_CancelBuonoSconto.Visible = False
    End Sub

	Protected Sub btSalvaPreventivo_click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btSalvaPreventivo.Click
		Me.PnlDestinazione.Visible = False
	
        Me.Session("Ordine_TipoDoc") = 2
        Me.Session("Ordine_Documento") = "Preventivo"
        Me.Session("Ordine_Pagamento") = Me.tbPagamenti.Text
        Me.Session("Ordine_Vettore") = Me.tbVettoriId.Text

        Me.Session("Ordine_SpeseSped") = CDbl(Me.lblSpeseSped.Text)
        Me.Session("Ordine_SpeseAss") = CDbl(Me.lblSpeseAss.Text)
        Me.Session("Ordine_SpesePag") = CDbl(Me.lblPagamento.Text)

        Session("Ordine_DescrizioneBuonoSconto") = ""
        Session("Ordine_TotaleBuonoSconto") = 0
        Session("Ordine_CodiceBuonoSconto") = ""

        Me.Session("NoteDocumento") = Me.txtNoteSpedizione.Text

        Response.Redirect("ordine.aspx")
	End Sub
	
    Protected Sub btInviaOrdine_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btInviaOrdine.Click
		Me.PnlDestinazione.Visible = False

        Try
            LeggiVettori()

            'Aggiorno i prezzi del carrello e controllo che sia stata inserita una quantità articolo minore di 1. In questo caso elimino il prodotto dal carrello
            Aggiorna_Prezzi_Carrello()

            If (controlla_articoli_quantita_zero() = 1) Then

                LeggiPagamenti()

				Dim paramsSelect As New Dictionary(Of String, String)
				paramsSelect.add("@IdUtenti",Me.Session("UTENTIID"))
				Dim dr As MySqlDataReader = ExecuteQueryGetDataReader("UTENTI.AZIENDEID, AZIENDE.RAGIONESOCIALE", "UTENTI", "INNER JOIN AZIENDE ON UTENTI.AZIENDEID = AZIENDE.ID WHERE UTENTI.Id=@IdUtenti", paramsSelect)
				dr.Read()
				lblIntestDestinazione.Text = dr.Item("RAGIONESOCIALE")
                dr.Close()
            Else
                Qnt_Errata.Visible = True
            End If

        Catch ex As Exception

        Finally

            'Sondaggio, disabilitato
            If (controlla_articoli_quantita_zero() = 1) Then
                If (Me.Session("LoginId") > 0) Then
                    'Me.MPESondaggio.Show()
                    'Abbiamo disabilitato la schermata di votazione sito
                    Cookie = "N"
                    SendOrder()
                Else
                    Session.Item("StavonelCarrello") = 1
                    Response.Redirect("accessonegato.aspx")
                End If
            End If

        End Try

    End Sub
	
	Protected Sub City_Bind_Data2(ByVal sender As Object, ByVal e As System.EventArgs)
		riempi_ddl_citta(tbCap2.Text, ddlCitta2, tbProvincia2)
		Session("cityBinding")=1
    End Sub
	
	Protected Sub riempi_ddl_citta(ByVal cap As String, ByVal cittaddl As DropDownList, ByVal provincia As TextBox, Optional ByVal citta As String = "")
		
		Dim ds As DataSet = cityRegistry.GetCitiesFromPostcodeCode(cap)
		ConvertDataSetColumnToUpper(ds, "name_city")
		cittaddl.DataSource = ds.Tables(0)
		cittaddl.DataTextField = ds.Tables(0).Columns("name_city").ToString().ToUpper()
        cittaddl.DataValueField = ds.Tables(0).Columns("name_city").ToString().ToUpper()
		cittaddl.DataBind()
		If citta <> String.Empty Then
			cittaddl.Items(cittaddl.SelectedIndex).Selected=false
			cittaddl.Items.FindByValue(citta).Selected=true
		End If
		citta = String.Empty
		If cittaddl.Items.Count > 0 Then
			citta = cittaddl.Items(cittaddl.SelectedIndex).Text
		End If
		
		riempi_text_provincia(citta, provincia)
    End Sub
	
	Protected Function ConvertDataSetColumnToUpper(ByRef ds As DataSet, ByVal columnName As String)
		For Each row As DataRow In ds.Tables(0).Rows
			row(columnName) = row(columnName).ToString().ToUpper()
		Next
	End Function	
	
	Protected Sub riempi_text_provincia(ByVal citta As String, ByVal provincia As TextBox)
		If citta <> String.Empty Then
			Dim ds As DataSet = cityRegistry.GetProvinceFromCity(citta)
			provincia.text = ds.Tables(0).Rows(0)("abbreviation").ToString()
		Else
			provincia.text = String.Empty
		End If
    End Sub
	
	Protected Sub Province_Bind_Data2(ByVal sender As Object, ByVal e As System.EventArgs)
		riempi_text_provincia(getDdlCittaValue(ddlCitta2), tbProvincia2)
		Session("cityBinding")=1
    End Sub
	
	Protected Function getDdlCittaValue(ByVal ddlCitta As DropDownList) As String
		Dim value As String
		Try
			value = ddlCitta.Items(ddlCitta.SelectedIndex).Text
		Catch ex As Exception
			value = ""
        End Try
		return value
	End Function
	
	Protected Function ExecuteQueryGetDataReader(ByVal fields As String, ByVal table As String, Optional ByVal wherePart As String = "", Optional ByVal params As Dictionary(Of String, String) = Nothing) As MySqlDataReader
        Dim sqlString As String = "SELECT " & fields & " FROM " & table & " " & wherePart
        Dim dr As MySqlDataReader
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
				If Not params Is Nothing then
					Dim paramName As String
					For Each paramName In params.Keys
						cmd.Parameters.AddWithValue(paramName, params(paramName))
					Next
				End If
				dr = cmd.ExecuteReader()
            End If
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If
        End Try
        Return dr
    End Function

    Protected Function ExecuteInsert(ByVal table As String, ByVal fields As String, Optional ByVal values As String = "", Optional ByVal params As Dictionary(Of String, String) = Nothing)
        Dim sqlString As String = "INSERT INTO " & table & " (" & fields & ") VALUES (" & values & ")"
        ExecuteNonQuery(False, sqlString, params)
    End Function

    Protected Function ExecuteDelete(ByVal table As String, Optional ByVal wherePart As String = "", Optional ByVal params As Dictionary(Of String, String) = Nothing)
        Dim sqlString As String = "DELETE FROM " & table & " " & wherePart
        ExecuteNonQuery(False, sqlString, params)
    End Function

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
                Dim cmd As New MySqlCommand
                cmd.Connection = conn
                cmd.CommandText = sqlString
                For Each paramName In params.Keys
                    If paramName = "?parPrezzo" Or paramName = "?parPrezzoIvato" Then
                        cmd.Parameters.Add(paramName, MySqlDbType.Double).Value = Convert.ToDecimal(params(paramName), System.Globalization.CultureInfo.GetCultureInfo("it-IT"))
                    Else
                        cmd.Parameters.AddWithValue(paramName, params(paramName))
                    End If
                Next
                If isStoredProcedure Then
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("?parRetVal", "0")
                    cmd.Parameters("?parRetVal").Direction = ParameterDirection.Output
                Else
                    cmd.CommandType = CommandType.Text
                End If
                cmd.ExecuteNonQuery()
                cmd.Dispose()
            End If
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If
        End Try
    End Function

End Class