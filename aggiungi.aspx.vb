Imports MySql.Data.MySqlClient
Imports System.Data

Partial Class aggiungi
    Inherits System.Web.UI.Page

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
        Dim articoliIdGlobali As String
        'Controllo se è un prodotto venduto su Coupon
        If Request.QueryString("id") = "Coupon" Then
            Dim params As New Dictionary(Of String, String)
            params.add("@LoginId", IIf(Session("LoginId") > 0, Session("LoginId"), 0))
            params.add("@SessionId", Session.SessionID)
            ExecuteDelete("carrello", "WHERE LoginId=@LoginId OR SessionId=@SessionId", params)
            'Inserisco articolo
            Dim paramsProcedure As New Dictionary(Of String, String)
            paramsProcedure.add("?parLoginId", Session("LoginId"))
            paramsProcedure.add("?parSessionId", Session.SessionID)
            paramsProcedure.add("?parArticoliId", Session("Coupon_idArticolo"))
            paramsProcedure.add("?parCodice", Session("Coupon_codArticolo"))
            paramsProcedure.add("?parDescrizione1", Session("Coupon_DescrizioneArticolo"))
            paramsProcedure.add("?parQnt", Session("Coupon_Qnt_Pezzi"))
            paramsProcedure.add("?parNListino", 1)
            paramsProcedure.add("?parTCid", -1)
            paramsProcedure.add("?parPrezzo", Session("Coupon_Prezzo"))
            paramsProcedure.add("?parPrezzoIvato", Session("Coupon_PrezzoIvato"))
            paramsProcedure.add("?parOfferteDettaglioID", 0)
            paramsProcedure.add("?parProdottoGratis", 0)
            ExecuteStoredProcedure("Newcarrello", paramsProcedure)

            Me.Session("Ordine_TipoDoc") = Session("IdDocumentoCoupon") '18 è l'id del TipoDocumento Coupon
            Me.Session("Ordine_Documento") = "Coupon"
            Me.Session("Ordine_Pagamento") = Session("IdPagamentoCoupon") 'Impostiamo il pagamento visualizzato nell'ordine del Coupon, nel nostro caso Carta di Credito
            Me.Session("Ordine_Vettore") = Session("Ordine_Vettore")
            Me.Session("Ordine_SpeseSped") = Session("Coupon_SpeseSpedizione")
            Me.Session("Ordine_SpeseAss") = 0
            Me.Session("Ordine_SpesePag") = 0
            Me.Session("NoteDocumento") = "Acquisto " & Session("Coupon_Qnt_Coupon") & "x Coupon - " & Session("Coupon_DescrizioneCoupon") & " - codice controllo ** " & Session("Coupon_Codice_Controllo") & " **"

            'Da Aggiornare il numero dei coupon acquistati, solo ad ordine arrivato DA FARE

            'Resetto le variabili di Sessione relative al Coupon
            Session("Coupon_idArticolo") = 0
            Session("Coupon_DescrizioneCoupon") = ""
            Session("Coupon_codArticolo") = 0
            Session("Coupon_DescrizioneArticolo") = ""
            ' La Qnt Coupon mi serve ancora, per incrementare il numero dei Coupon Acquistati, dopo aver effettuato l'ordine
            'Session("Coupon_Qnt_Coupon") = 0
            Session("Coupon_Qnt_Pezzi") = 0
            Session("Coupon_Prezzo") = 0
            Session("Coupon_PrezzoIvato") = 0
            Session("Coupon_StatoPagamento") = 0
            Session("Spese_Spedizione") = 0

            Session("Ordine_DescrizioneBuonoSconto") = 0
            Session("Ordine_TotaleBuonoScontoImponibile") = 0
            Session("Ordine_CodiceBuonoSconto") = 0
            Session("Ordine_BuonoScontoIdIva") = 0
            Session("Ordine_BuonoScontoValoreIva") = 0

            Response.Redirect("ordine.aspx")
        End If

        'Controllo se si sta aggiungendo un Buono sconto al documento
        'Controllo se è un prodotto venduto su Coupon
        If Request.QueryString("id") = "BuonoSconto" Then
            'Codice per il Buono Sconto
        End If


        Dim Pagina As String = Me.Session("Carrello_Pagina")
        If Pagina Is Nothing Then
            Pagina = Request.UrlReferrer.ToString
        End If

        If Not Me.Session("Carrello_ArticoloId") Is Nothing Then

            Dim IdRiga As Integer = 0
            Dim LoginId As Integer = Me.Session("LoginId")
            Dim SessionID As String = Me.Session.SessionID
            Dim Quantita As Double = Me.Session("Carrello_Quantita")
            Dim NListino As Integer = Me.Session("Listino")
            Dim Codice As String = ""
            Dim Descrizione As String = ""
            Dim Prezzo As Double = 0
            Dim PrezzoIvato As Double = 0
            Dim OfferteDettagliID As Integer

            Dim ArticoliId As String = Me.Session("Carrello_ArticoloId")
            Dim TCId As String = Me.Session("Carrello_TCId")
            Dim i As Integer
            Dim ListaArticoli As New ArrayList
            Dim ListaTCs As New ArrayList

            If ArticoliId = 0 Then
                ListaArticoli = Me.Session("Carrello_ListaArticoloId")
            Else
                ListaArticoli.AddRange(ArticoliId.Split(","))
                ListaTCs.AddRange(TCId.Split(","))
            End If

            Dim SelezioneMultipla As New ArrayList
            If Session("Carrello_SelezioneMultipla") IsNot Nothing Then
                SelezioneMultipla = Session("Carrello_SelezioneMultipla")
            End If


            If SelezioneMultipla.Count > 0 Then

                For i = 0 To SelezioneMultipla.Count - 1
                    Dim selezionamultipla_ID As String = SelezioneMultipla(i).ToString.Split(",")(0)
                    Dim selezionamultipla_TCID As String = SelezioneMultipla(i).ToString.Split(",")(1)
                    Dim selezionamultipla_Qta As String = SelezioneMultipla(i).ToString.Split(",")(2)
                    Dim selezionamultipla_SpedGRATIS As String = SelezioneMultipla(i).ToString.Split(",")(3)
                    Dim wherePart As String = ""
                    If LoginId = 0 Then
                        wherePart = "where SessionID=@SessionID"
                    Else
                        wherePart = "where LoginID=@LoginId"
                        SessionID = ""
                    End If
                    wherePart &= " and ArticoliId=@ArticoliId and TCId=@TCId"
                    Dim params As New Dictionary(Of String, String)
                    params.add("@ArticoliId", selezionamultipla_ID)
                    params.add("@TCId", selezionamultipla_TCID)
                    params.add("@SessionID", SessionID)
                    params.add("@LoginId", LoginId)
                    Dim dr = ExecuteQueryGetDataReader("id, qnt", "carrello", wherePart, params)

                    'Se l'articolo è già presente nel carrello sommo la quantità
                    If dr.Count > 0 Then
                        Dim row = dr(0)
                        Quantita = Val(selezionamultipla_Qta) + row("qnt")
                        IdRiga = row("id")
                        params.add("@idRiga", IdRiga)
                        ExecuteDelete("carrello", "where id=@idRiga", params)
                    Else
                        Quantita = Val(selezionamultipla_Qta)
                    End If

                    'Leggo prezzi e promozioni
                    params.add("@NListino", NListino)
                    dr = ExecuteQueryGetDataReader("*", "vsuperarticoli", "where id=@ArticoliId and TCId=@TCId AND NListino=@NListino ORDER BY PrezzoPromo DESC", params)

                    OfferteDettagliID = 0
                    Prezzo = 0
                    PrezzoIvato = 0

                    For Each row As Dictionary(Of String, Object) In dr

                        Codice = row("Codice")
                        Descrizione = row("Descrizione1")
                        OfferteDettagliID = 0
                        If Prezzo = 0 Then
                            Prezzo = row("Prezzo")
                        End If
                        If PrezzoIvato = 0 Then
                            PrezzoIvato = row("PrezzoIvato")
                        End If

                        If row("InOfferta") = 1 Then
                            If Quantita >= row("OfferteQntMinima") And row("OfferteQntMinima") > 0 Then
                                OfferteDettagliID = row("OfferteDettagliId")
                                Prezzo = row("PrezzoPromo")
                                PrezzoIvato = row("PrezzoPromoIvato")
                            ElseIf Quantita Mod row("OfferteMultipli") = 0 And row("OfferteMultipli") > 0 Then
                                OfferteDettagliID = row("OfferteDettagliId")
                                Prezzo = row("PrezzoPromo")
                                PrezzoIvato = row("PrezzoPromoIvato")
                            End If
                        End If

                    Next

                    'Inserisco articolo
                    Dim paramsProcedure As New Dictionary(Of String, String)
                    paramsProcedure.add("?parLoginId", LoginId)
                    paramsProcedure.add("?parSessionId", SessionID)
                    paramsProcedure.add("?parArticoliId", Val(selezionamultipla_ID))
                    paramsProcedure.add("?parCodice", Codice)
                    paramsProcedure.add("?parDescrizione1", Descrizione)
                    paramsProcedure.add("?parQnt", Quantita)
                    paramsProcedure.add("?parNListino", NListino)
                    paramsProcedure.add("?parTCId", Val(selezionamultipla_TCID))
                    paramsProcedure.add("?parPrezzo", Prezzo)
                    paramsProcedure.add("?parPrezzoIvato", PrezzoIvato)
                    paramsProcedure.add("?parOfferteDettaglioID", OfferteDettagliID)
                    paramsProcedure.add("?parProdottoGratis", selezionamultipla_SpedGRATIS)
                    ExecuteStoredProcedure("Newcarrello", paramsProcedure)

                    AggiornaVisite(selezionamultipla_ID)
                    If articoliIdGlobali <> String.Empty Then
                        articoliIdGlobali &= ","
                    End If
                    articoliIdGlobali &= selezionamultipla_ID
                Next

            Else
                For i = 0 To ListaArticoli.Count - 1
                    Dim wherePart As String = ""
                    If LoginId = 0 Then
                        wherePart = "where SessionID=@SessionID"
                    Else
                        wherePart = "where LoginID=@LoginId"
                        SessionID = ""
                    End If

                    wherePart &= " and ArticoliId=@ArticoliId and TCId=@TCId"
                    Dim params As New Dictionary(Of String, String)
                    params.add("@ArticoliId", ListaArticoli(i))
                    params.add("@TCId", ListaTCs(i))
                    params.add("@SessionID", SessionID)
                    params.add("@LoginId", LoginId)
                    Dim dr = ExecuteQueryGetDataReader("id, qnt", "carrello", wherePart, params)

                    'Se l'articolo è già presente nel carrello sommo la quantità
                    If dr.Count > 0 Then
                        Dim row = dr(0)
                        Quantita = Quantita + row("qnt")
                        IdRiga = row("id")
                        params.add("@idRiga", IdRiga)
                        ExecuteDelete("carrello", "where id=@idRiga", params)
                    End If

                    'Leggo prezzi e promozioni
                    params.add("@NListino", NListino)
                    dr = ExecuteQueryGetDataReader("*", "vsuperarticoli", "where id=@ArticoliId and TCId=@TCId AND NListino=@NListino ORDER BY PrezzoPromo DESC", params)

                    OfferteDettagliID = 0
                    Prezzo = 0
                    PrezzoIvato = 0

                    For Each row As Dictionary(Of String, Object) In dr
                        Codice = row("Codice")
                        Descrizione = row("Descrizione1")
                        OfferteDettagliID = 0
                        If Prezzo = 0 Then
                            Prezzo = row("Prezzo")
                        End If
                        If PrezzoIvato = 0 Then
                            PrezzoIvato = row("PrezzoIvato")
                        End If

                        If row("InOfferta") = 1 Then
                            If Quantita >= row("OfferteQntMinima") And row("OfferteQntMinima") > 0 Then
                                OfferteDettagliID = row("OfferteDettagliId")
                                Prezzo = row("PrezzoPromo")
                                PrezzoIvato = row("PrezzoPromoIvato")
                            ElseIf Quantita Mod row("OfferteMultipli") = 0 And row("OfferteMultipli") > 0 Then
                                OfferteDettagliID = row("OfferteDettagliId")
                                Prezzo = row("PrezzoPromo")
                                PrezzoIvato = row("PrezzoPromoIvato")
                            End If
                        End If
                    Next

                    'Inserisco articolo
                    Dim paramsProcedure As New Dictionary(Of String, String)
                    paramsProcedure.add("?parLoginId", LoginId)
                    paramsProcedure.add("?parSessionId", SessionID)
                    paramsProcedure.add("?parArticoliId", ListaArticoli(i))
                    paramsProcedure.add("?parTCId", ListaTCs(i))
                    paramsProcedure.add("?parCodice", Codice)
                    paramsProcedure.add("?parDescrizione1", Descrizione)
                    paramsProcedure.add("?parQnt", Quantita)
                    paramsProcedure.add("?parNListino", NListino)
                    paramsProcedure.add("?parPrezzo", Prezzo)
                    paramsProcedure.add("?parPrezzoIvato", PrezzoIvato)
                    paramsProcedure.add("?parOfferteDettaglioID", OfferteDettagliID)
                    paramsProcedure.add("?parProdottoGratis", Session("ProdottoGratis").ToString)
                    ExecuteStoredProcedure("Newcarrello", paramsProcedure)
                    AggiornaVisite(ListaArticoli(i))
                    If articoliIdGlobali <> String.Empty Then
                        articoliIdGlobali &= ","
                    End If
                    articoliIdGlobali &= ListaArticoli(i)
                Next

            End If
        End If

        Me.Session("Carrello_ArticoloId") = Nothing
        Me.Session("Carrello_ListaArticoloId") = Nothing
        Me.Session("Carrello_Quantita") = Nothing
        Me.Session("Carrello_Pagina") = Nothing

        'Me.Response.Redirect(Pagina)
        Session("Carrello_SelezioneMultipla") = Nothing

        facebook_pixel(articoliIdGlobali)

        Me.Response.Redirect("carrello.aspx")

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


    Public Sub aggiungiInCarrello()

    End Sub

    Public Sub AggiornaVisite(ByVal ArticoliId As Integer)
        If ArticoliId <> CLng(Me.Session("visite_articoloid")) Then
            Me.Session("visite_articoloid") = ArticoliId
            Dim params As New Dictionary(Of String, String)
            params.add("@id", ArticoliId)
            ExecuteUpdate("articoli", "visite=visite+1", "where id=@id", params)
        End If
    End Sub

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

    Protected Sub ExecuteStoredProcedure(ByVal storedProcedure As String, Optional ByVal params As Dictionary(Of String, String) = Nothing)
        ExecuteNonQuery(True, storedProcedure, params)
    End Sub

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
End Class
