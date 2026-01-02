Imports MySql.Data.MySqlClient
Imports System.Net
Imports System.Security.Authentication
Imports System.Data

Partial Class documentidettaglio
    Inherits System.Web.UI.Page
     
	Const _Tls12 As SslProtocols = DirectCast(&HC00, SslProtocols)
	Const Tls12 As SecurityProtocolType = DirectCast(_Tls12, SecurityProtocolType)
    
	Dim IvaTipo As Integer

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        Me.Title = Me.Title & " - Dettaglio documento"
    End Sub
	
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Dim lblPrezzi As Label = Me.FormView2.FindControl("lblPrezzi")
        Dim tbTipo As TextBox = Me.FormView1.FindControl("tbTipo")
        Dim tbOnline As TextBox = Me.FormView1.FindControl("tbOnline")
        Dim tbPagato As TextBox = Me.FormView1.FindControl("tbPagato")
        Dim btBancaSella As ImageButton = Me.FormView1.FindControl("btBancaSella")
        Dim btIwBank As ImageButton = Me.FormView1.FindControl("btIwBank")
        Dim btPayPal As Button = Me.FormView1.FindControl("btPayPal")
        If Me.Session("LoginId") Is Nothing Then
            Me.Session("Page") = Me.Request.Url.ToString
            Me.Response.Redirect("accessonegato.aspx")
        Else
			Try
				IvaTipo = Me.Session("IvaTipo")
				If IvaTipo = 1 Then
					lblPrezzi.Text = "*Prezzi Iva Esclusa"
				ElseIf IvaTipo = 2 Then
					lblPrezzi.Text = "*Prezzi Iva Inclusa"
				End If
			Catch 
				Me.Response.Redirect("default.aspx")
			End Try
        End If
		
        'AggiornaTotali()

        If (btBancaSella.CommandArgument <> "") AndAlso (btBancaSella.Attributes("CodiceAutorizzazione") = "") Then
            btBancaSella.Visible = True
        End If

        If tbTipo.Text = 4 And tbOnline.Text = 1 Then
            'btIwBank.Visible = True
            If tbPagato.Text <> "" Then
                If tbPagato.Text = 1 Then
                    btIwBank.Enabled = False
                    btIwBank.ImageUrl = "public/images/pagato.gif"
                End If
            End If
        ElseIf tbTipo.Text = 4 And tbOnline.Text = 2 Then
            btPayPal.Visible = True
            If tbPagato.Text <> "" Then
                If tbPagato.Text = 1 Then
                    btPayPal.Enabled = False
                    btPayPal.Text = "PAGATO"
                End If
                'If tbPagato.Text = 2 Then
                'btPayPal.Enabled = False
                'btPayPal.Text = "IN CONFERMA"
                'End If
            End If
        End If

        'Quando provengo da un ordine effettuato con Session("Tracking_BestShopping")=1, vuol dire che il prodotto è stato visualizzato tramite BestShopping
        'Questo controllo viene effettuato nella pagina articolo.aspx, controllando che il campo GET comparatore sia uguale a BESTSHOPPING
        If Session("Tracking_BestShopping") = 1 Then
            Session("Tracking_BestShopping") = 0
            Track_BestShopping(Request.QueryString("ndoc"))
        End If
    End Sub

    Protected Sub GridView1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView1.PreRender

        Dim i As Integer

        For i = 0 To GridView1.Rows.Count - 1

            Dim importo As Label
            Dim importoIvato As Label
            Dim temp_hyperlink As HyperLink
            Dim temp_label_seriale As Label
            Dim p_seriali As HtmlGenericControl

            importo = GridView1.Rows(i).FindControl("lblImporto")
            importoIvato = GridView1.Rows(i).FindControl("lblImportoIvato")
            temp_hyperlink = GridView1.Rows(i).FindControl("HyperLink3")
            temp_label_seriale = GridView1.Rows(i).FindControl("lbl_seriale")
            p_seriali = GridView1.Rows(i).FindControl("Panel_Seriali")

            If IvaTipo = 1 Then
                importo.Visible = True
                importoIvato.Visible = False
                GridView1.Rows(i).FindControl("lblprezzo").Visible = True
                GridView1.Rows(i).FindControl("lblprezzoivato").Visible = False
            ElseIf IvaTipo = 2 Then
                importo.Visible = False
                importoIvato.Visible = True
                GridView1.Rows(i).FindControl("lblprezzo").Visible = False
                GridView1.Rows(i).FindControl("lblprezzoivato").Visible = True
            End If

            'Sostituisco alle immagini vuote una immagine stardard
            Dim img_temp As Image = GridView1.Rows(i).FindControl("Image_Articolo")
            If img_temp.ImageUrl.Replace("~/Public/foto/", "").Trim = "" Then
                img_temp.ImageUrl = "images/no_img.jpg"
            End If

            temp_label_seriale.Text = preleva_seriali(temp_hyperlink.Attributes("idarticolo"))

            If temp_label_seriale.Text <> "" Then
                p_seriali.Visible = True
            End If
        Next

    End Sub

    Private Sub Track_BestShopping(ByVal numero_documento As Long)

        Dim tr, sc
            Dim cont = 0

            Dim pa(cont, 3)

            Dim params As New Dictionary(Of String, String)
            params.add("@id", Request.QueryString("id"))
            'Usare la riga sottostante quando verrà inserito il campo idriga nella tabella seriali
            Dim drReport = ExecuteQueryGetDataReader("*", "documentirighe", "where (DocumentiId =@id)", params)

            For Each row As Dictionary(Of String, Object) In drReport
                cont += 1
                ReDim pa(cont, 3)      ' NOTA: DIMENSIONATO A NUMERO PRODOTTI+1 ,3

                ' PRODOTTO 1
                pa(cont - 1, 0) = row("Codice")  'DA MODIFICARE - id articolo
                pa(cont - 1, 1) = row("importo").ToString 'DA MODIFICARE - prezzo articolo iva inclusa, il divisore per
                'il separatore dei decimali deve essere un punto, non la virgola
                pa(cont - 1, 2) = row("Qnt").ToString     'DA MODIFICARE - quantità  acquistata dell'articolo
            Next

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

    End Sub

    Public Sub AggiornaTotali()
        Dim lblImponibile As Label = Me.FormView2.FindControl("lblImponibile")
        Dim lblSpeseSped As Label = Me.FormView2.FindControl("lblSpeseSped")
        Dim lblSpeseAss As Label = Me.FormView2.FindControl("lblSpeseAss")
        Dim lblSpesePag As Label = Me.FormView2.FindControl("lblSpesePag")
        Dim lblIva As Label = Me.FormView2.FindControl("lblIva")
        Dim lblTotale As Label = Me.FormView2.FindControl("lblTotale")
        lblIva.Text = String.Format("{0:c}", (CDbl(lblImponibile.Text) + (CDbl(lblSpeseSped.Text) + CDbl(lblSpeseAss.Text))) * 20 / 100)
        lblTotale.Text = String.Format("{0:c}", CDbl(lblImponibile.Text) + CDbl(lblSpeseSped.Text) + CDbl(lblSpeseAss.Text) + CDbl(lblSpesePag.Text) + CDbl(lblIva.Text))
    End Sub
    'Usare la riga sottostante quando verrà inserito il campo idriga nella tabella seriali
    'Protected Function preleva_seriali(ByVal idriga As Integer, ByVal idArticolo As Integer) As String
    Protected Function preleva_seriali(ByVal idArticolo As Integer) As String

        Dim params As New Dictionary(Of String, String)
        params.add("@id", Request.QueryString("id"))
        params.add("@idArticolo", idArticolo)
        'Usare la riga sottostante quando verrà inserito il campo idriga nella tabella seriali
        Dim drReport = ExecuteQueryGetDataReader("*", "seriali", "where (DocumentiId =@id) AND (ArticoliId=@idArticolo)", params)

        Dim risultato As String = ""
        For Each row As Dictionary(Of String, Object) In drReport
            risultato = risultato & row("Seriale") & "; "
        Next

        Return risultato
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

    Protected Sub GridView2_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView2.PreRender
        Dim i As Integer

        For i = 0 To GridView2.Rows.Count - 1

            Dim importo As Label
            Dim importoIvato As Label
            Dim temp_hyperlink As HyperLink
            Dim temp_label_seriale As Label
            Dim p_seriali As HtmlGenericControl

            importo = GridView2.Rows(i).FindControl("lblImporto")
            importoIvato = GridView2.Rows(i).FindControl("lblImportoIvato")
            temp_hyperlink = GridView2.Rows(i).FindControl("HyperLink3")
            temp_label_seriale = GridView2.Rows(i).FindControl("lbl_seriale")
            p_seriali = GridView2.Rows(i).FindControl("Panel_Seriali")

            If IvaTipo = 1 Then
                importo.Visible = True
                importoIvato.Visible = False
                GridView2.Rows(i).FindControl("lblprezzo").Visible = True
                GridView2.Rows(i).FindControl("lblprezzoivato").Visible = False
            ElseIf IvaTipo = 2 Then
                importo.Visible = False
                importoIvato.Visible = True
                GridView2.Rows(i).FindControl("lblprezzo").Visible = False
                GridView2.Rows(i).FindControl("lblprezzoivato").Visible = True
            End If

            'Sostituisco alle immagini vuote una immagine stardard
            Dim img_temp As Image = GridView2.Rows(i).FindControl("Image_Articolo")
            If img_temp.ImageUrl.Replace("~/Public/foto/", "").Trim = "" Then
                img_temp.ImageUrl = "images/no_img.jpg"
            End If

            temp_label_seriale.Text = preleva_seriali(temp_hyperlink.Attributes("idarticolo"))

            If temp_label_seriale.Text <> "" Then
                p_seriali.Visible = True
            End If
        Next
    End Sub

    Private Sub FormView1_ItemCommand(sender As Object, e As FormViewCommandEventArgs) Handles FormView1.ItemCommand
		ServicePointManager.SecurityProtocol = Tls12
        If e.CommandName = "PagamentoBancaSella" Then
            Dim btBancaSella As ImageButton = Me.FormView1.FindControl("btBancaSella")
            Dim currency As String = "242"
            Dim amount As String = HttpUtility.UrlEncode(Replace(btBancaSella.Attributes("totaleDocumento"), ",", "."))
            Dim shopTransactionId As String = HttpUtility.UrlEncode(btBancaSella.Attributes("nDocumento") & "/" & Date.Now.Year)
            Dim idDocumento As String = HttpUtility.UrlEncode(btBancaSella.Attributes("idDocumento"))
            Dim sitoWeb As String = HttpUtility.UrlEncode(Me.Session("AziendaUrl")) 'HttpContext.Current.Request.Url.AbsoluteUri 
            Dim buyerName As String = HttpUtility.UrlEncode(Me.Session("LoginNomeCognome"))
            Dim buyerEmail As String = HttpUtility.UrlEncode(Me.Session("LoginEmail"))
            Response.Redirect("/bancasella.aspx?currency=" & currency & "&amount=" & amount & "&shopTransactionId=" & shopTransactionId & "&iddocumento=" & idDocumento & "&sitoweb=" & sitoWeb & "&buyername=" & buyerName & "&buyeremail=" & buyerEmail)
        ElseIf e.CommandName = "PagamentoPayPal" Then
            Dim btPayPal As Button = Me.FormView1.FindControl("btPayPal")
            Dim accountPaypal As String = HttpUtility.UrlEncode(Me.Session("AccountPaypal"))
            Dim nDocumento As String = HttpUtility.UrlEncode(btPayPal.Attributes("NDocumento"))
            Dim dataDocumento As String = HttpUtility.UrlEncode(Replace(btPayPal.Attributes("DataDocumento"), " 00.00.00", ""))
            Dim itemName As String = "Ordine+n.+" & nDocumento & "+del+" & dataDocumento
            Dim totaleDocumento As String = HttpUtility.UrlEncode(btPayPal.Attributes("TotaleDocumento"))
            Dim idDocumento As String = btPayPal.Attributes("idDocumento")
            Dim returnLink As String = HttpUtility.UrlEncode("http://" & Request.Url.Host & "/pagamento.aspx?id=" & idDocumento)
            Dim cancelReturnLink As String = HttpUtility.UrlEncode("http://" & Request.Url.Host & "/documentidettaglio.aspx?id=" & idDocumento)
            Dim notifyUrl As String = HttpUtility.UrlEncode("http://" & Request.Url.Host & "/ipn.aspx?id=" & idDocumento)
            Response.Redirect("https://www.paypal.com/it/cgi-bin/webscr/?cmd=_xclick&business=" & accountPaypal & "&item_name=" & itemName & "&currency_code=EUR&amount=" & totaleDocumento & "&item_number=" & itemName & "&quantity=1&return=" & returnLink & "&cancel_return=" & cancelReturnLink & "&notify_url=" & notifyUrl)
        End If
    End Sub
	
End Class

