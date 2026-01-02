Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Net
Imports System.Net.Mail
Imports BancaSella
Imports System.Security.Authentication

Partial Class ordine
    Inherits System.Web.UI.Page
	Const _Tls12 As SslProtocols = DirectCast(&HC00, SslProtocols)
	Const Tls12 As SecurityProtocolType = DirectCast(_Tls12, SecurityProtocolType)
    Dim Contatore_BestShopping As Integer = 0

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
	Public redirect as String


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Me.Session("LoginId") Is Nothing Then
            Me.Session("Page") = Me.Request.Url.ToString
            Me.Response.Redirect("accessonegato.aspx")
        Else
            'Ciclo semaforico
            SyncLock Application.Item("Semaforo")

                Dim LoginId As Long = Me.Session("LoginId")
                Dim UtentiId As Long = Me.Session("UtentiId")
                Dim TipoDoc As Integer = Me.Session("Ordine_TipoDoc")
                Dim Documento As String = Me.Session("Ordine_Documento")
                Dim Pagamento As Integer = Me.Session("Ordine_Pagamento")
                Dim Vettore As Integer = Me.Session("Ordine_Vettore")
                Dim SpeseSped As Double = Me.Session("Ordine_SpeseSped")
                Dim SpeseAss As Double = Me.Session("Ordine_SpeseAss")
                Dim SpesePag As Double = Me.Session("Ordine_SpesePag")
                Dim Arrotondamento As Double = 0
                Dim documento_memorizzato As Integer = 0
                Dim id As Integer = 0
                Dim DataDoc As String = ""
                Dim Note As String = Me.Session("NoteDocumento")

                If TipoDoc = Nothing Then
                    Me.Response.Redirect("documenti.aspx")
                End If

                Dim dr As MySqlDataReader
                Dim conn As New MySqlConnection
                Dim cmd As New MySqlCommand
                Dim NumDoc As Long
                Dim trns As MySqlTransaction
                Dim noneseguito As Boolean = False
                Dim er As String = String.Empty

                conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
                conn.Open()

                cmd.Connection = conn
                cmd.CommandText = "	SELECT max(ndocumento) + 1 as nmax from documenti where year(datadocumento)=year(CURRENT_TIMESTAMP) and tipodocumentiid =" & TipoDoc

                Dim numDoc_tracking As String = "1"
                Try
                    numDoc_tracking = cmd.ExecuteScalar
                Catch
                End Try

                cmd.Dispose()

                cmd.CommandType = CommandType.Text
                cmd.CommandText = "SELECT * FROM carrello where LoginId=" & LoginId

                dr = cmd.ExecuteReader()
                
                If Not dr.HasRows Then
                    Me.Response.Redirect("carrello.aspx")
                End If

				Dim articoliIdGlobali As string = String.Empty
				while dr.read()
					if articoliIdGlobali <> String.Empty then 
						articoliIdGlobali & = ","
					End if
					articoliIdGlobali & = dr("articoliId")
				end while
				facebook_pixel(articoliIdGlobali)

                dr.Close()
                dr.Dispose()
                cmd.Dispose()

                trns = conn.BeginTransaction
					
                Try
                    cmd.Transaction = trns
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = "Carrello_Documento"

                    cmd.Parameters.AddWithValue("?pLoginId", LoginId)
                    cmd.Parameters.AddWithValue("?pTipoDoc", TipoDoc)
                    cmd.Parameters.AddWithValue("?pTipoPagamento", Pagamento)
                    cmd.Parameters.AddWithValue("?pVettore", Vettore)
					
                    'Parametri BuonoSconto
                    cmd.Parameters.AddWithValue("?pBuonoScontoDescrizione", Session("Ordine_DescrizioneBuonoSconto"))
                    cmd.Parameters.AddWithValue("?pBuonoScontoTotale", Session("Ordine_TotaleBuonoScontoImponibile"))
                    cmd.Parameters.AddWithValue("?pBuonoScontoCodice", Session("Ordine_CodiceBuonoSconto"))
                    cmd.Parameters.AddWithValue("?pBuonoScontoIdIVA", Session("Ordine_BuonoScontoIdIva"))
                    cmd.Parameters.AddWithValue("?pBuonoScontoValoreIVA", Session("Ordine_BuonoScontoValoreIva"))

                    If IsNothing(Session("SCEGLIINDIRIZZO")) Then
                        cmd.Parameters.AddWithValue("?pUtentiInirizzoId", "0")
                    Else
                        cmd.Parameters.AddWithValue("?pUtentiInirizzoId", Session("SCEGLIINDIRIZZO"))
                    End If

                    'Calcolare l'iva da passare all'ordine, tenere in considerazione ValoreEsenioneIva e ValoreIvaRC
                    'da capire se passare l'id dell'iva oppure il valore dell'iva
                    Dim IdEsenzioneIva As Integer = 0
                    Dim IdIvaRC As Integer = 0
                    Dim ValoreEsenzioneIva As Integer = 0
                    Dim ValoreIvaRC As Integer = 0
                    Dim IdEsenzioneIva_Utente As Integer = 0
                    Dim IdValoreReverseCharge As Integer = 0

                    'Se sto inserendo un documento relativo al Coupon allora devo settare i check relativi a Spese di Spedizione, Spese Assicurazione, Spese Pagamento
                    If Not (Session("Coupon_idArticolo") Is Nothing) AndAlso (Session("Coupon_idArticolo")) > 0 Then

                    End If

                    cmd.Parameters.AddWithValue("?pCostoAssicurazione", IIf(Not (Session("Coupon_idArticolo") Is Nothing) AndAlso (Session("Coupon_idArticolo")) > 0, 0, SpeseAss))
                    cmd.Parameters.AddWithValue("?pCostoSpedizione", IIf(Not (Session("Coupon_idArticolo") Is Nothing) AndAlso (Session("Coupon_idArticolo")) > 0, 0, SpeseSped))
                    cmd.Parameters.AddWithValue("?pArrotondamento", IIf(Session("Coupon_Arrotondamento") Is Nothing, 0, Session("Coupon_Arrotondamento")))
                    cmd.Parameters.AddWithValue("?pCostoPagamento", IIf(Not (Session("Coupon_idArticolo") Is Nothing) AndAlso (Session("Coupon_idArticolo")) > 0, 0, SpesePag))
                    cmd.Parameters.AddWithValue("?pNoteSpedizione", Note)
                    cmd.Parameters.AddWithValue("?pUtenteAbilitatoRC", Session("AbilitatoIvaReverseCharge"))
                    cmd.Parameters.AddWithValue("?pIvaVettore", Session("Iva_Vettori"))
                    cmd.Parameters.AddWithValue("?pStatiId", recupera_stato_default_Documento(TipoDoc))
                    cmd.Parameters.AddWithValue("?DocumentoMemorizzato", documento_memorizzato)
                    cmd.Parameters("?DocumentoMemorizzato").Direction = ParameterDirection.Output


                    Try
                        'If Request.QueryString("C") = "S" Then

                        Dim NAME As String = ""
                        Dim PRICES As String = ""
                        Dim UNITS As String = ""

                        Dim valore As Decimal = 0
                        Dim quantita As Decimal = 0

                        'Tracking per il sito di BestShopping
                        'Track_BestShopping(numDoc_tracking)
                        'Tracking per il sito di Kelkoo
                        'track_Kelkoo(numDoc_tracking, valore, quantita, NAME, PRICES, UNITS)
                        'Tracking per il sito di Pangora
                        'track_Pangora(numDoc_tracking, quantita, valore, NAME, PRICES, UNITS)

                        'End If

                    Catch ex2 As Exception

                    End Try

                    'Try : cmd.ExecuteNonQuery() : Catch ex As MySqlException : noneseguito = True : er = ex.Message : End Try
                    cmd.ExecuteNonQuery()

                    If noneseguito Then
                        trns.Rollback()
                        cmd.Dispose()
                        conn.Close()
                        conn.Dispose()
                        Me.Panel1.Visible = False
                        Me.Panel2.Visible = True
                        Response.Write("Errore di Automazione :" & er)
                        Exit Sub
                    End If

                    NumDoc = cmd.Parameters("?DocumentoMemorizzato").Value

                    cmd.Parameters.Clear()
                    cmd.Dispose()

                    cmd.CommandType = CommandType.Text
                    cmd.CommandText = "SELECT * FROM documenti where UtentiId=" & UtentiId & " AND TipoDocumentiID=" & TipoDoc & " ORDER BY ID Desc "

                    dr = cmd.ExecuteReader()
                    dr.Read()

                    If dr.HasRows Then
                        id = dr.Item("id")
                        DataDoc = dr.Item("DataDocumento")
                    End If

                    dr.Close()
                    dr.Dispose()

                    cmd.Dispose()

                    Me.Label1.Text = NumDoc
                    Me.Label2.Text = Documento
                    Me.Label3.Text = DataDoc

                    'Modificato perchè non abbiamo bisogno di visualizzare l'hyperlink del reindirizzamento al dettaglio dell'ordine
                    'visto che successivamente facciamo un reindirizzamento automatico al dettaglio tramite il Server
                    'Me.HyperLink1.NavigateUrl = "documentidettaglio.aspx?id=" & id
                    'Me.HyperLink1.Text = "<b>Controlla " & Documento & "</b><br> ed effettua il pagamento"

                    trns.Commit()

                    'Seleziono il tipo di mail da inviare, ordine normale oppure Coupon
                    If Session("Coupon_Codice_Controllo") = "" Then
                        SendEmail(NumDoc, Documento, id, "")
                    Else
                        SendEmail(NumDoc, Documento, id, Session("NoteDocumento"))
                    End If

                    Me.Session("Ordine_TipoDoc") = Nothing
                    Me.Session("Ordine_Documento") = Nothing
                    Me.Session("Ordine_Pagamento") = Nothing
                    Me.Session("Ordine_Vettore") = Nothing
                    Me.Session("Ordine_SpeseSped") = Nothing
                    Me.Session("Ordine_SpeseAss") = Nothing
                    Me.Session("Ordine_SpesePag") = Nothing

                    conn.Close()

                    'Imposto i check nel DocumentoPie in modo da forzare tutti i valori a 0
                    set_check_documento_pie(id) 'id Rappresenta l'id del documento appena inserito

                    'Imposto il porto del corriere sempre a Franco come indicatomi da Germano il 31/10/2013
                    set_porto_spedizione(id, 2)

                    'Resetto le variabili per l'acquisto del Coupon e reidirizzo il l'acquirente verso l'ordine normale oppure verso il Coupon
                    If Session("Coupon_Codice_Controllo") <> "" Then
                        Dim cmd_coupon As MySqlCommand = New MySqlCommand
                        Dim codice_controllo As String = Session("Coupon_Codice_Controllo")
                        Dim idCoupon As Integer = Me.Session("Coupon_idCoupon")
                        Dim NumeroOpzioneCoupon As Integer = Me.Session("Coupon_NumeroOpzione")
                        Dim idTransazione As String = Me.Session("Coupon_IdTransazione")

                        'Azzero le variabii di Sessione precedentemente impostate
                        Session("Coupon_Codice_Controllo") = ""
                        Session("Coupon_idTransazione") = ""
                        Session("Coupon_idCoupon") = 0
                        Session("Coupon_NumeroOpzione") = 0

                        conn.Open()
                        cmd_coupon.Connection = conn
                        cmd_coupon.CommandText = "UPDATE documenti SET Pagato=1, Coupon_idCoupon=?idCoupon, Coupon_NumeroOpzione=?NumeroOpzioneCoupon, Coupon_CodControllo=?codice_controllo, idTransazione=?idTransazione WHERE id=?id"
                        cmd_coupon.Parameters.AddWithValue("?idCoupon", idCoupon)
                        cmd_coupon.Parameters.AddWithValue("?NumeroOpzioneCoupon", NumeroOpzioneCoupon)
                        cmd_coupon.Parameters.AddWithValue("?codice_controllo", codice_controllo)
                        cmd_coupon.Parameters.AddWithValue("?idTransazione", idTransazione)
                        cmd_coupon.Parameters.AddWithValue("?id", id)
                        cmd_coupon.ExecuteNonQuery()

                        'Conteggio del numero dei Coupon Acquistati Realmente, tramite i documenti
                        cmd_coupon.CommandText = "UPDATE coupon_inserzione SET NumeroAcquisti=NumeroAcquisti+?qnt WHERE idCoupon=?idCoupon"
                        cmd_coupon.Parameters.AddWithValue("?idCoupon", idCoupon)
                        cmd_coupon.Parameters.AddWithValue("?qnt", Session("Coupon_Qnt_Coupon"))
                        cmd_coupon.ExecuteNonQuery()

                        conn.Close()

                        'Resetto la variabile di Sessione relativa alla Qnt di Coupon Acquistati
                        Session("Coupon_Qnt_Coupon") = 0
                        '------------------------------------------------------------------------------------------------------------------

                        'If (Me.Session("Ordine_BancaSellaGestPay_ShopId") <> "") Then
                        '    esguiPagamentoBancaSella(Me.Session("Ordine_BancaSellaGestPay_ShopId"), NumDoc, Me.Session("Ordine_Totale_Documento"))
                        'Else
						'Response.Redirect("coupon_esito_acquisto.aspx?id=" & idCoupon & "&cod=" & codice_controllo)
                        redirect = "coupon_esito_acquisto.aspx?id=" & idCoupon & "&cod=" & codice_controllo
                        'End If
                    Else
                        If (Me.Session("Ordine_BancaSellaGestPay_ShopId") <> "") Then
							ServicePointManager.SecurityProtocol = Tls12
                            Dim totaleDocumento As Double = Me.Session("Ordine_Totale_Documento")
                            'Resetto il totale dell'ordine
                            Me.Session("Ordine_Totale_Documento") = 0
                            'Aggiunto "- ivaBuonoSconto" in data 25/10/2017 perchè come da indicazioni di Germano non si trovava con il prezzo
							Dim currency As String = "242"
							Dim ivaBuonoSconto As Double = Session("Ordine_TotaleBuonoSconto") - Session("Ordine_TotaleBuonoScontoImponibile")
							Dim amount As String = HttpUtility.UrlEncode(Replace(totaleDocumento - ivaBuonoSconto, ",", "."))
							Dim shopTransactionId As String = HttpUtility.UrlEncode(NumDoc & "/" & Date.Now.Year)
							Dim idDocumento As String = HttpUtility.UrlEncode(id)
							Dim sitoWeb As String = HttpUtility.UrlEncode(Me.Session("AziendaUrl")) 
							Dim buyerName As String = HttpUtility.UrlEncode(Me.Session("LoginNomeCognome"))
							Dim buyerEmail As String = HttpUtility.UrlEncode(Me.Session("LoginEmail"))
							'Response.Redirect("/bancasella.aspx?currency=" & currency & "&amount=" & amount & "&shopTransactionId=" & shopTransactionId & "&iddocumento=" & idDocumento & "&sitoweb=" & sitoWeb & "&buyername=" & buyerName & "&buyeremail=" & buyerEmail)
							redirect = "/bancasella.aspx?currency=" & currency & "&amount=" & amount & "&shopTransactionId=" & shopTransactionId & "&iddocumento=" & idDocumento & "&sitoweb=" & sitoWeb & "&buyername=" & buyerName & "&buyeremail=" & buyerEmail
						Else
                            'Riindirizzo l'utente sull'ordine appena effettuato, affinche possa effettuare il pagamento, se il metodo di pagamento scelto è Carta di Credito
                            'Response.Redirect("documentidettaglio.aspx?id=" & id & "&ndoc=" & NumDoc)
							redirect = "documentidettaglio.aspx?id=" & id & "&ndoc=" & NumDoc
						End If
                    End If

                Catch ex As Exception
                    'trns.Rollback()
                    Me.Panel1.Visible = False
                    Me.Panel2.Visible = True
                    Response.Write("Errore :" & ex.Message)

                Finally

                    If conn.State = ConnectionState.Open Then
                        conn.Close()
                        conn.Dispose()
                    End If

                End Try

            End SyncLock

        End If

    End Sub

	Public Sub facebook_pixel(ByVal articoliId As String)

		if Session("utentiid")  > -1 then
		
			Dim conn As New MySqlConnection
			Try
				conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
				conn.Open()
				Dim cmd As New MySqlCommand
				cmd.Connection = conn
				cmd.CommandType = CommandType.Text
                cmd.CommandText = "SELECT ifnull(CognomeNome,'') as CognomeNome, RagioneSociale, ifnull(email,'') as email, coalesce(case when ifnull(cellulare,'') = '' then null else cellulare end,case when ifnull(telefono,'') = '' then null else telefono end,'') as telefono, ifnull(nazione,'') as nazione, ifnull(provincia,'') as provincia, ifnull(citta,'') as citta, ifnull(cap,'') as cap FROM utenti WHERE id = ?utentiid"
                cmd.Parameters.AddWithValue("?utentiid", Session("utentiid"))
                Dim dr As MySqlDataReader = cmd.ExecuteReader()
                If dr.HasRows Then
					dr.Read()
					firstName = dr.Item("CognomeNome")
					lastName = dr.Item("RagioneSociale")
					email = dr.Item("email")
					phone = dr.Item("telefono")
					country = dr.Item("nazione")
					province = dr.Item("provincia")
					city = dr.Item("citta")
					cap = dr.Item("cap")
					dr.Close()
					Dim oldIdFbPixel As String = String.Empty
					Dim sku As String
					Dim query As String = "SELECT articoli.codice as sku, ks_fb_pixel.id_pixel FROM articoli "
					query & = "Left Join ks_fb_pixel_products on ks_fb_pixel_products.id_product = articoli.id "
					query & = "Left Join ks_fb_pixel on ks_fb_pixel_products.id_fb_pixel = ks_fb_pixel.id "
                    query &= "WHERE articoli.id in (" & articoliId & ") And ks_fb_pixel.start_date<=CURDATE() And ks_fb_pixel.stop_date>CURDATE() And ks_fb_pixel.id_company = ?AziendaID"
                    query & = "Order by ks_fb_pixel_products.id_fb_pixel"
					cmd.CommandText = query
                    cmd.Parameters.AddWithValue("?AziendaID", Session("AziendaID"))
                    dr = cmd.ExecuteReader()
                    While dr.read()
						Dim newIdFbPixel As String = dr.Item("id_pixel")
						if newIdFbPixel<>oldIdFbPixel then
							if oldIdFbPixel<>String.Empty Then
								idsFbPixelsSku.Add(oldIdFbPixel, sku)
							End If
							oldIdFbPixel = newIdFbPixel
							sku = String.Empty
						else
							sku & = ","
						end if 
						sku & = dr.Item("sku")
					End while
					if oldIdFbPixel<>String.Empty Then
						idsFbPixelsSku.Add(oldIdFbPixel, sku)
					End If
					
				End If
				dr.Close()
			Catch
			Finally
				conn.Close()
			End Try
		End If
				
	End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        Me.Title = Me.Title & " - Carrello"
    End Sub

    Public Sub SendEmail(ByVal n As Long, ByVal documento As String, ByVal id As Integer, ByVal Descrizione_Coupon As String)
        Dim conn As New MySqlConnection
		Dim connDestAlt As New MySqlConnection
        Try
            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()
            Dim StrCarrello As String = ""
            Dim StrIva As String = ""
            Dim IvaTipo As Integer

            IvaTipo = Me.Session("IvaTipo")
            If IvaTipo = 1 Then
                StrIva = "*Prezzi Iva Esclusa"
            ElseIf IvaTipo = 2 Then
                StrIva = "*Prezzi Iva Inclusa"
            End If

            Dim cmdTestata As New MySqlCommand
            cmdTestata.Connection = conn
            cmdTestata.CommandType = CommandType.Text
            cmdTestata.CommandText = "SELECT * FROM vdocumenticompleta WHERE id=" & id

            Dim drTestata As MySqlDataReader = cmdTestata.ExecuteReader()
            drTestata.Read()

            Dim Imponibile As String = ""
            Dim SpeseSped As String = ""
            Dim SpeseAss As String = ""
            Dim SpesePag As String = ""
            Dim Iva As String = ""
            Dim Totale As String = ""

            If drTestata.HasRows Then
                StrCarrello = "<br><br><table cellpadding=3 cellspacing=3  border=0 bordercolor=silver style='font-family:arial;font-size:9pt;'>" & _
                        "<tr><td bgcolor=whitesmoke><b>" & documento & ":</td><td colspan=2><b>n° " & drTestata.Item("NDocumento") & " del " & drTestata.Item("DataDocumento") & "</td></tr>" & _
                        "<tr><td bgcolor=whitesmoke valign=top>Cliente:</td><td>" & drTestata.Item("RagioneSociale") & "<br>" & drTestata.Item("Indirizzo") & "<br>" & drTestata.Item("citta") & "<br>" & drTestata.Item("Cap") & " " & drTestata.Item("provincia") & "</td><td>" & drTestata.Item("cognomenome") & "<br>Codice: " & drTestata.Item("codice") & "<br>P.Iva: " & drTestata.Item("piva") & "<br>C.F: " & drTestata.Item("codicefiscale") & "</td></tr>" & _
                        "<tr><td bgcolor=whitesmoke valign=top>Recapiti:</td><td>Tel: " & drTestata.Item("Telefono") & "<br>Fax: " & drTestata.Item("Fax") & "</td><td>Cell: " & drTestata.Item("Cellulare") & "<br>Email: " & drTestata.Item("Email") & "</td></tr>"

                'Prelevo la destinazione alternativa
				
				connDestAlt.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
				connDestAlt.Open()
                Dim cmd As New MySqlCommand
				cmd.Connection = connDestAlt
				cmd.CommandType = CommandType.Text
				
                Dim RagioneSocialeA As String = ""
                Dim NomeA As String = ""
                Dim Indirizzo2 As String = ""
                Dim Citta2 As String = ""
                Dim Provincia2 As String = ""
                Dim Cap2 As String = ""
                Dim Zona As String = ""
                Dim Note As String = ""

                Dim SQL As String = ""
                Dim strIndSec As String = ""

                'Seleziono la destinazione alternativa se presente
                SQL = "SELECT * FROM utentiindirizzi where utenteid = ?UtentIId"

                If IsNothing(Session("SCEGLIINDIRIZZO")) Then
                    SQL &= " AND PREDEFINITO = 1"
                Else
                    SQL &= " AND ID =?id"
                    cmd.Parameters.AddWithValue("?id", Session("SCEGLIINDIRIZZO"))
                End If

                cmd.CommandText = SQL
                cmd.Parameters.AddWithValue("?UtentIId", Session("UtentIId"))

                Dim dsdata As New DataSet

                Dim sqlAdp As New MySqlDataAdapter(cmd)
                sqlAdp.Fill(dsdata, "utentiindirizzi")

                'For Each ROW As DataRow In dsdata.Tables(0).Select("PREDEFINITO = 1")
                For Each ROW As DataRow In dsdata.Tables(0).Rows

                    RagioneSocialeA = ROW("RagioneSocialeA").ToString
                    NomeA = ROW("NomeA").ToString
                    Indirizzo2 = ROW.Item("IndirizzoA").ToString
                    Citta2 = ROW.Item("CittaA").ToString
                    Provincia2 = ROW.Item("ProvinciaA").ToString
                    Cap2 = ROW.Item("CapA").ToString
                    'Zona = ROW.Item("Zona").ToString
                    'Note = ROW.Item("Note").ToString
                Next

                strIndSec = RagioneSocialeA
                strIndSec &= " - " & NomeA
                strIndSec &= " - " & Indirizzo2
                strIndSec &= ", CAP: " & Cap2
                strIndSec &= " - " & Citta2
                strIndSec &= " (" & Provincia2 & ") "

                'Stampo la destinazione alternativa, se presente
                If dsdata.Tables(0).Rows.Count > 0 Then
                    StrCarrello &= "<tr><td bgcolor=whitesmoke valign=top>Indirizzo<br/>Alternativo:</td><td style=""vertical-align:top;"">" & RagioneSocialeA & "<br>" & Indirizzo2 & "<br>" & Citta2 & "<br>" & Cap2 & " " & Provincia2 & "</td><td style=""vertical-align:top;"">" & NomeA & "</td></tr>"
                End If

				dsdata.Dispose()
				cmd.Dispose()
				
                StrCarrello &= "<tr><td bgcolor=whitesmoke>Stato:</td><td colspan=2>" & drTestata.Item("StatiDescrizione1") & " - " & drTestata.Item("StatiDescrizione2") & "</td></tr>" & _
                               "<tr><td bgcolor=whitesmoke>Spedizione:</td><td colspan=2>" & drTestata.Item("VettoriDescrizione") & " - " & drTestata.Item("VettoriInformazioni") & " </td></tr>"

                If Descrizione_Coupon <> "" Then
                    'Nel caso acquisto Coupon
                    StrCarrello = StrCarrello & "<tr><td bgcolor=whitesmoke>Pagamento:</td><td colspan=2>" & drTestata.Item("PagamentiTipoDescrizione") & " - PAGATO </td></tr>"
                    StrCarrello = StrCarrello & "</table><table cellpadding=3 cellspacing=3 border=0 bordercolor=silver width='500' style='font-family:arial;font-size:8pt;'>"
                Else
                    'Nel caso acquisto normale
                    StrCarrello = StrCarrello & "<tr><td bgcolor=whitesmoke>Pagamento:</td><td colspan=2>" & drTestata.Item("PagamentiTipoDescrizione") & " - " & drTestata.Item("PagamentiTipoInformazioni") & "</td></tr>"
                    StrCarrello = StrCarrello & "</table><table cellpadding=3 cellspacing=3 border=0 bordercolor=silver width='500' style='font-family:arial;font-size:8pt;'>"
                End If

                Imponibile = String.Format("{0:c}", drTestata.Item("totimponibile"))
                SpeseSped = String.Format("{0:c}", drTestata.Item("costospedizione"))
                SpeseAss = String.Format("{0:c}", drTestata.Item("costoassicurazione"))
                SpesePag = String.Format("{0:c}", drTestata.Item("costopagamento"))
                Iva = String.Format("{0:c}", drTestata.Item("totiva"))
                Totale = String.Format("{0:c}", drTestata.Item("totaledocumento"))
                'Totale = String.Format("{0:c}", CDbl(Imponibile) + CDbl(SpeseSped) + CDbl(SpeseAss) + CDbl(SpesePag) + CDbl(Iva) + CDbl(Session("Ordine_TotaleBuonoSconto")))

                'Modificato da Salvatore, da controllare
                'If Session("Iva_Utente") > -1 Then
                'Iva = String.Format("{0:c}", (((CDbl(Imponibile) / 100) * Session("Iva_Utente")) + (((CDbl(SpeseSped) + CDbl(SpeseAss)) / 100) * Session("Iva_Vettori"))))
                'Else
                'Iva = String.Format("{0:c}", (Iva))
                'Resetto la variabile di Sessione utilizzata per il calcolo dell'Iva
                'Session("Calcolo_Iva") = 0
                'End If
            End If

            drTestata.Close()
            drTestata.Dispose()
            cmdTestata.Dispose()

            'Caso o di Coupon oppure Normale
            If Descrizione_Coupon <> "" Then
                StrCarrello = StrCarrello & "<tr><td colspan=6 bgcolor=whitesmoke><b>Coupon</b></td></tr>"
                StrCarrello = StrCarrello & "<tr><td colspan=6>" & Descrizione_Coupon & "</td></tr>"
                StrCarrello = StrCarrello & "<tr><td colspan=6><a href=""http://" & Session("AziendaUrl") & "/coupon_stampa.aspx?id=" & Session("Coupon_idCoupon") & "&cod=" & Session("Coupon_Codice_Controllo") & """>Clicca qui</a> per visualizzare il Coupon Acquistato</td></tr>"
                StrCarrello = StrCarrello & "<tr><td colspan=6 bgcolor=whitesmoke height=1></td></tr>"
            Else
                Dim cmdRighe As New MySqlCommand
                cmdRighe.Connection = conn
                cmdRighe.CommandType = CommandType.Text
                cmdRighe.CommandText = "SELECT * FROM vdocumentirighe WHERE DocumentiId=" & id

                Dim drRighe As MySqlDataReader = cmdRighe.ExecuteReader()
                While drRighe.Read()
                    If IvaTipo = 1 Then
                        StrCarrello = StrCarrello & "<tr><td>" & drRighe.Item("marchedescrizione") & " " & drRighe.Item("codice") & "</td><td>" & drRighe.Item("descrizione1") & "</td><td align=right>" & drRighe.Item("qnt") & "</td><td nowrap align=right>" & String.Format("{0:c}", drRighe.Item("prezzo")) & "</td><td nowrap align=right><b>" & String.Format("{0:c}", drRighe.Item("importo")) & "</td></tr>"
                    ElseIf IvaTipo = 2 Then
                        StrCarrello = StrCarrello & "<tr><td>" & drRighe.Item("marchedescrizione") & " " & drRighe.Item("codice") & "</td><td>" & drRighe.Item("descrizione1") & "</td><td align=right>" & drRighe.Item("qnt") & "</td><td nowrap align=right>" & String.Format("{0:c}", drRighe.Item("prezzoivato")) & "</td><td nowrap align=right><b>" & String.Format("{0:c}", drRighe.Item("importoivato")) & "</td></tr>"
                        StrCarrello = StrCarrello & "<tr><td></td><td colspan=""2"" bgcolor=whitesmoke align=right style=""font-size:7pt;""><span style=""color:red;"">IVA " & drRighe.Item("ValoreIva") & "%</span> - <i>" & drRighe.Item("DescrizioneIva") & "</i></td><td nowrap align=right></td><td nowrap align=right></td></tr>"
                    End If
                    StrCarrello = StrCarrello & "<tr><td colspan=5 bgcolor=whitesmoke height=1></td></tr>"
                End While

                drRighe.Close()
                drRighe.Dispose()
                cmdRighe.Dispose()

            End If

            StrCarrello = StrCarrello & "<tr><td colspan=2>" & StrIva & "</td><td colspan=2 bgcolor=whitesmoke align=right>Imponibile:</td><td bgcolor=whitesmoke nowrap align=right><b>" & Imponibile & "</td></tr>" & _
                                        "<tr><td colspan=2></td><td colspan=2 bgcolor=whitesmoke align=right>Spedizione:</td><td bgcolor=whitesmoke nowrap align=right><b>" & SpeseSped & "</td></tr>" & _
                                        "<tr><td colspan=2></td><td colspan=2 bgcolor=whitesmoke align=right>Assicurazione:</td><td bgcolor=whitesmoke nowrap align=right><b>" & SpeseAss & "</td></tr>" & _
                                        "<tr><td colspan=2></td><td colspan=2 bgcolor=whitesmoke align=right>Pagamento:</td><td bgcolor=whitesmoke nowrap align=right><b>" & SpesePag & "</td></tr>" & _
                                        "<tr><td colspan=2></td><td colspan=2 bgcolor=whitesmoke align=right>Iva:</td><td bgcolor=whitesmoke nowrap align=right><b>" & Iva & "</td></tr>" & _
                                        "<tr><td colspan=2></td><td colspan=2 bgcolor=whitesmoke align=right><b>Totale:</td><td bgcolor=whitesmoke nowrap align=right><b>" & Totale & "</td></tr>" & _
                                        "</table>"

            Dim oMsg As MailMessage = New MailMessage()
            oMsg.From = New MailAddress(Session("AziendaEmail"), Session("AziendaNome"))
            oMsg.To.Add(New MailAddress(Session("LoginEmail"), Session("LoginNomeCognome")))
            oMsg.Bcc.Add(New MailAddress(Session("AziendaEmail"), Session("AziendaNome")))
            oMsg.Subject = "Conferma " & documento & " dal sito " & Session("AziendaNome")
            oMsg.Body = "<font face=arial size=2 color=black>Gentile " & Session("LoginNomeCognome") & "," & _
                        "<br>La ringraziamo per aver preferito " & Session("AziendaNome") & ", abbiamo ricevuto la sua richiesta di " & documento & ",<br>Le riportiamo di seguito l'elenco completo dei prodotti scelti e le condizioni commerciali.</font>" & _
                        StrCarrello

            If documento.Contains("Preventivo") = True Then
                oMsg.Body &= "<br/><span style=""font-size:9pt; color:red;"">Le ricordiamo che tale documento non ha nessuna validità di impegno poichè non è un ORDINE ma semplicemente un PREVENTIVO online.<br/>Se vuole può convertirlo in ordine contattandoci, ed indicando il tipo di pagamento che vuole effettuare.<br/>Oppure può rifare l’ordine on-line e alla fine del carrello deve cliccare sul tasto ""CONFERMA ORDINE"" e non ""SALVA PREVENTIVO"".<br/>Dopodichè seguendo le istruzioni, potrà procedere al pagamento.</span>"
            End If

            If (Session.Item("AziendaId") = 2) Then
                oMsg.Body &= "<br/><br/><font face=arial size=2 color=black><b>NOTE: </b><br>" & Me.Session("NoteDocumento") & "</font>" & _
                            "<br><font face=arial size=2 color=black><b>" & Session("AziendaNome") & "</b><br>" & Session("AziendaDescrizione") & "<br>Sito Web: <a href=http://" & Session("AziendaUrl") & ">http://" & Session("AziendaUrl") & "</a> - Email: <a href=mailto:" & Session("AziendaEmail") & ">" & Session("AziendaEmail") & "</a></font>" & _
                            "<br/><br/><a href=""http://www.facebook.com/pages/Webaffareit/199922450453""><img src=""http://www.webaffare.it/Public/Images/seguici_facebook.jpg""/></a><br>" & _
                            "<br/><br/><font face=arial size=1 color=silver>D.Lgs 196/2003 tutela delle persone di altri soggetti rispetto al trattamento di dati personali. La presente comunicazione è destinata esclusivamente al soggetto indicato più sopra quale destinatario o ad eventuali altri soggetti autorizzati a riceverla. Essa contiene informazioni strettamente confidenziali e riservate, la cui comunicazione o diffusione a terzi è proibita, salvo che non sia espressamente autorizzata. Se avete ricevuto questa comunicazione per errore, o se desiderate non ricevere più comunicazioni su novità e offerte, Vi preghiamo di darne immediata comunicazione al mittente scrivendo a " & Me.Session("AziendaEmail") & ". Si informa che i dati forniti saranno tenuti rigorosamente riservati, saranno utilizzati unicamente da " & Me.Session("AziendaNome") & " per comunicare offerte promozionali o novità sui prodotti/servizi e resteranno a disposizione per eventuali variazioni o per la cancellazione ai sensi dell'art. 7 del citato decreto legislativo.</font>"
            Else
                oMsg.Body &= "<br/><br/><font face=arial size=2 color=black><b>NOTE: </b><br>" & Me.Session("NoteDocumento") & "</font>" & _
                            "<br/><font face=arial size=2 color=black><b>" & Session("AziendaNome") & "</b><br>" & Session("AziendaDescrizione") & "<br>Sito Web: <a href=http://" & Session("AziendaUrl") & ">http://" & Session("AziendaUrl") & "</a> - Email: <a href=mailto:" & Session("AziendaEmail") & ">" & Session("AziendaEmail") & "</a></font>" & _
                            "<br/><br><font face=arial size=1 color=silver>D.Lgs 196/2003 tutela delle persone di altri soggetti rispetto al trattamento di dati personali. La presente comunicazione è destinata esclusivamente al soggetto indicato più sopra quale destinatario o ad eventuali altri soggetti autorizzati a riceverla. Essa contiene informazioni strettamente confidenziali e riservate, la cui comunicazione o diffusione a terzi è proibita, salvo che non sia espressamente autorizzata. Se avete ricevuto questa comunicazione per errore, o se desiderate non ricevere più comunicazioni su novità e offerte, Vi preghiamo di darne immediata comunicazione al mittente scrivendo a " & Me.Session("AziendaEmail") & ". Si informa che i dati forniti saranno tenuti rigorosamente riservati, saranno utilizzati unicamente da " & Me.Session("AziendaNome") & " per comunicare offerte promozionali o novità sui prodotti/servizi e resteranno a disposizione per eventuali variazioni o per la cancellazione ai sensi dell'art. 7 del citato decreto legislativo.</font>"
            End If

            oMsg.IsBodyHtml = True

            Dim oSmtp As SmtpClient = New SmtpClient(Me.Session.Item("smtp"))
            oSmtp.DeliveryMethod = SmtpDeliveryMethod.Network

            Dim oCredential As System.Net.NetworkCredential = New System.Net.NetworkCredential(CType(Session.Item("User_smtp"), String), CType(Session.Item("Password_smtp"), String))
            oSmtp.UseDefaultCredentials = True
            oSmtp.Credentials = oCredential

            'ATTENZIONE
            oSmtp.Send(oMsg)
			
        Catch ex As Exception
			
		
        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

			If connDestAlt.State = ConnectionState.Open Then
                connDestAlt.Close()
                connDestAlt.Dispose()
            End If
			
        End Try

    End Sub

    Private Sub track_Kelkoo(ByVal orderNumber As String, ByRef orderValue As Decimal, ByRef qta As Decimal, ByRef N As String, ByRef P As String, ByRef U As String)

        Try

            ' Imposta il numero dell'ordine. Questo valore deve essere diverso per ogni ordine e compilato dinamicamente.
            'Dim orderNumber As String = "123456"
            ' Imposta l'importo dell'ordine. Questo valore non deve contenere virgole o spazi.
            ' Usate il punto per separare i decimali.
            'Dim orderValue As String = "1"
            ' Imposta l'organization id fornito da Kelkoo. Questo codice identifica univocamente il vostro negozio su TradeDoubler.
            Dim organization As String = ""
            ' Imposta la valuta dell'importo dell'acquisto.
            Dim currency As String = "EUR"
            ' Imposta l'event id fornito da Kelkoo. Questo codice identifica univocamente l'evento vendita nel vostro programma di affiliazione a TradeDoubler.
            Dim eventId As String = ""

            ' --- Opzionale ---
            ' Report info. Questa informazione è usata per inviare a TradeDoubler
            ' dettagli specifici riguardo l'ordine.
            ' f1 è il numero di colli
            ' f2 è il nome del prodotto                                                        
            ' f3 è il prezzo del singolo prodotto. Usate il punto come separatore dei decimali.
            ' Usate l'and (&) come separatore di campi.                                        
            ' Se l'ordine è composto da più articoli diversi, usate il pipe (|)                
            ' per separare i prodotti.                                                         

            Dim reportInfo As String '= "f1=1&f2=nike shoes&f3=74.95|f1=2&f2=adidas hat&f3=19.95"

            Dim conn As MySqlConnection = New MySqlConnection

            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()

            Dim cmdD As New MySqlCommand
            cmdD.Connection = conn

            ' Importante! Il parametro reportInfo deve essere encodato nel formato UTF-8.
            cmdD.CommandText = "SELECT QNT, DESCRIZIONE1, PREZZO, PREZZOIVATO  FROM carrello where loginid =" & Session("LoginId")
            Dim drReport As MySqlDataReader = cmdD.ExecuteReader()

            While drReport.Read

                reportInfo &= "f1=" & drReport.Item("QNT") & "&f2=" & drReport.Item("DESCRIZIONE1") & "&f3=" & drReport.Item("PREZZOIVATO").ToString.Replace(",", ".") & "|"
                orderValue += drReport.Item("PREZZOIVATO")
                qta += drReport.Item("QNT")

                If N.Length > 0 Then N &= ","
                N &= drReport.Item("DESCRIZIONE1")

                If P.Length > 0 Then P &= ","
                P &= drReport.Item("PREZZOIVATO").ToString.Replace(",", ".")

                If U.Length > 0 Then U &= ","
                U &= drReport.Item("QNT")

            End While

            drReport.Close()

            reportInfo = reportInfo.Substring(0, reportInfo.Length - 1)
            reportInfo = Server.UrlEncode(reportInfo)

            ' Review info. Questa informazione deve essere aggiunta se l'utente partecipa alla valutazione negozio e deve essere compilata dinamicamente.
            ' name è il nome e cognome dell'utente                                                                 
            ' email è l'email dell'utente                                                                          
            ' expDeliveryDate è la data presunta di spedizione del prodotto                                        
            ' formattato così: yyyy-mm-dd                                                                          
            ' Il parametro review deve essere vuoto se l'utente non vuole ricevere l'email per la valutazione      

            Dim ReviewStr As String


            cmdD.CommandType = CommandType.Text
            'cmdD.CommandText = "SELECT RAGIONESOCIALE, COGNOMENOME, EMAIL, AZIENDEID FROM UTENTI where Id=" & Me.Session("UTENTIID")

            Dim SQL As String = ""

            SQL = " SELECT UTENTI.RAGIONESOCIALE, UTENTI.COGNOMENOME, "
            SQL &= "      UTENTI.EMAIL, UTENTI.AZIENDEID,AZIENDE.ORGANIZATION, AZIENDE.EVENT"
            SQL &= " FROM UTENTI INNER JOIN AZIENDE ON UTENTI.AZIENDEID = AZIENDE.ID"
            SQL &= " WHERE UTENTI.Id = ?utentiId"

            cmdD.CommandText = SQL
            cmdD.Parameters.AddWithValue("?utentiId", Me.Session("UTENTIID"))
            Dim drD As MySqlDataReader = cmdD.ExecuteReader()

            Dim Nome As String = ""
            Dim mail As String = ""

            If drD.Read Then

                If drD.Item("COGNOMENOME").ToString.Length = 0 Then
                    Nome = drD.Item("RAGIONESOCIALE").ToString
                Else
                    Nome = drD.Item("COGNOMENOME").ToString & " " & drD.Item("RAGIONESOCIALE").ToString
                End If

                mail = drD.Item("EMAIL").ToString

                organization = drD.Item("ORGANIZATION").ToString

                eventId = drD.Item("EVENT").ToString

            End If

            cmdD.Dispose()
            drD.Close()
            conn.Close()

            Dim dataSpedizione As Date = DateAdd(DateInterval.Day, 10, Date.Now)
            Dim DATA As String = CType(Format(dataSpedizione, "yyyy-MM-dd"), String)

            ReviewStr = "name=" & Nome & "&email=" & mail & "&expDeliveryDate=" & DATA
            ReviewStr = Server.UrlEncode(ReviewStr)

            DivImg.InnerHtml = "<img src='http://tbs.tradedoubler.com/report?organization=" & organization & "&event=" & eventId & "&orderNumber=" & orderNumber & "&orderValue=" & CType(orderValue, Decimal).ToString.Replace(",", ".") & "&currency=" & currency & "&reportInfo=" & reportInfo & "&review=" & ReviewStr & "'/>"

        Catch ex As Exception

        End Try

    End Sub

    Private Sub track_Pangora(ByVal orderId As String, ByVal qta As Decimal, ByVal orderValue As Decimal, ByRef N As String, ByRef P As String, ByRef U As String)

        Try

            Me.litScript.Text = "<!-- Pangora Sales Tracking Script V 1.0.0 - All rights reserved -->"
            Me.litScript.Text &= "<script language='JavaScript'>"
            Me.litScript.Text &= "  var pg_pangora_merchant_id='36904';"
            Me.litScript.Text &= "  var pg_order_id='" & orderId & "';"
            Me.litScript.Text &= "  var pg_cart_size='" & CType(qta, Decimal).ToString.Replace(",", ".") & "';"
            Me.litScript.Text &= "  var pg_cart_value='" & CType(orderValue, Decimal).ToString.Replace(",", ".") & "';"
            Me.litScript.Text &= "  var pg_currency='EUR';"
            Me.litScript.Text &= "  var pg_product_name='" & N & "';"
            Me.litScript.Text &= "  var pg_product_price='" & P & "';"
            Me.litScript.Text &= "  var pg_product_units='" & U & "';"
            Me.litScript.Text &= "</script>"
            Me.litScript.Text &= "<script language='JavaScript' src='https://clicks.pangora.com/sales-tracking/salesTracker.js'>"
            Me.litScript.Text &= "</script>"
            Me.litScript.Text &= "<noscript>"
            Me.litScript.Text &= "  <img src='https://clicks.pangora.com/sales-tracking/36904/salesPixel.do'/>"
            Me.litScript.Text &= "</noscript>"

        Catch ex As Exception

        End Try

    End Sub

    Private Sub set_check_documento_pie(ByVal idDocumento As Integer)
        Dim conn As MySqlConnection = New MySqlConnection

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn.Open()

        Dim cmd As New MySqlCommand

        cmd.Connection = conn
        cmd.CommandText = "UPDATE documentiplus SET Assicurazione=1, Spedizione=1, Pagamento=1 WHERE DocumentiId=" & idDocumento

        cmd.ExecuteNonQuery()
        conn.Close()
    End Sub

    Private Sub set_porto_spedizione(ByVal idDocumento As Integer, ByVal porto As Integer)
        Dim conn As MySqlConnection = New MySqlConnection

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn.Open()

        Dim cmd As New MySqlCommand

        cmd.Connection = conn
        cmd.CommandText = "UPDATE documentipie SET CausaliPortoId=" & porto & " WHERE DocumentiId=" & idDocumento

        cmd.ExecuteNonQuery()
        conn.Close()
    End Sub

    Private Sub Track_BestShopping(ByVal numero_documento As Long)
        Try

            Dim tr, sc
            Dim cont = 0

            Dim pa(cont, 3)

            Dim conn As MySqlConnection = New MySqlConnection

            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()

            Dim cmdD As New MySqlCommand
            cmdD.Connection = conn

            cmdD.CommandText = "SELECT ArticoliId, Codice, QNT, DESCRIZIONE1, PREZZO, PREZZOIVATO  FROM carrello where loginid =?loginid"
            cmdD.Parameters.AddWithValue("?loginid", Session("LoginId"))
            Dim drReport As MySqlDataReader = cmdD.ExecuteReader()

            While drReport.Read

                cont += 1
                ReDim pa(cont, 3)      ' NOTA: DIMENSIONATO A NUMERO PRODOTTI+1 ,3

                ' PRODOTTO 1
                pa(cont - 1, 0) = drReport.Item("Codice")  'DA MODIFICARE - id articolo
                pa(cont - 1, 1) = drReport.Item("PREZZOIVATO").ToString 'DA MODIFICARE - prezzo articolo iva inclusa, il divisore per
                'il separatore dei decimali deve essere un punto, non la virgola
                pa(cont - 1, 2) = drReport.Item("QNT").ToString     'DA MODIFICARE - quantità  acquistata dell'articolo
            End While

            drReport.Close()

            ' ALTRI DATI DELL'ORDINE NECESSARI
            tr = numero_documento  'DA MODIFICARE - id ordine

            sc = Me.Session("Ordine_SpeseSped") * ((Session("Iva_Vettori") / 100) + 1)     'DA MODIFICARE - spese di spedizione ordine
            'il separatore dei decimali deve essere un punto, non la virgola
            '*********************** FINE CODICE INTERAZIONE MERCHANT **********************

            Dim img_bs As Object
            img_bs = New OB_image_bestshopping()

            ' generazione e print dell'image url - WriteImage restituisce una stringa contentente l'img url
            ' pertanto è possibile salvare il risultato in una variabile e gestirselo come si vuole
            img_bs_label.Text = img_bs.WriteImage(pa, tr, sc)

            drReport.Close()
            conn.Close()

        Catch ex As Exception

        End Try
    End Sub

    Function recupera_stato_default_Documento(ByVal TipoDocumento As Integer) As Integer
        Dim conn As New MySqlConnection
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
		conn.Open()
        Dim cmd As MySqlCommand = New MySqlCommand
        Dim esito As Integer
        Try
            cmd.Connection = conn

            cmd.CommandText = "SELECT * FROM tipodocumenti WHERE id=" & TipoDocumento

            Dim dr As MySqlDataReader = cmd.ExecuteReader()

            If dr.Read() = True Then
                esito = dr.Item("StatiId")
            Else
                esito = 1
            End If
            dr.Close()
            cmd.Dispose()
            conn.Close()
            conn.Dispose()
            Return esito
        Catch e As Exception

        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

        End Try
    End Function

End Class