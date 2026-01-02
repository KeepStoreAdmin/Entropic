Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Net.Mail


Partial Class documenti
    Inherits System.Web.UI.Page

    Dim conn As New MySqlConnection
    Dim cmd As New MySqlCommand
    Dim strSql As String = ""

    Public nDocTrovati As String = "0"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Me.Session("LoginId") Is Nothing Then
            Me.Session("Page") = Me.Request.Url.ToString
            Me.Response.Redirect("accessonegato.aspx")
        End If

        lblInfo.Visible = False
        lblInfo.Text = ""

    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        Me.Title = Me.Title & " - Consultazione documenti"
    End Sub

    Sub preRenderClick(sender As Object, e As EventArgs)
        If Page.IsPostBack = False Then
            Dim t1 As String = Request.QueryString("t")

            Dim link As LinkButton = sender
            Dim t As String

            t = link.Attributes("tipoDocumento")
            link.CssClass = "nonSelezionato"
            If (t1 = t) Then
                link.CssClass = "selezionato"
            End If
        End If
    End Sub

    Sub tipoDocumentoClick(sender As Object, e As EventArgs)

        Dim link As LinkButton = sender

        Dim t As String
        t = link.attributes("tipoDocumento")

        Response.Redirect("documenti.aspx?t=" & t)
    End Sub

    Sub aggiungiStato(sender As Object, e As EventArgs)

        filtroStati.items.Insert(0, New ListItem("Qualsiasi stato", "-1"))

    End Sub


    Sub filtroDataRapido(sender As Object, e As EventArgs) Handles filtroTempo.SelectedIndexChanged, filtroStati.SelectedIndexChanged

        Dim v As Integer = filtroTempo.SelectedValue

        dataFine.Text = Format(Date.Now, "dd-MM-yyyy")

        If (v = -1) Then
            dataInizio.Text = ""
        End If

        If (v = 7) Then
            dataInizio.Text = Format(Date.Now.AddDays(-7), "dd-MM-yyyy")
        End If

        If (v = 30) Then
            dataInizio.Text = Format(Date.Now.AddDays(-30), "dd-MM-yyyy")
        End If

        If (v = 60) Then
            dataInizio.Text = Format(Date.Now.AddDays(-60), "dd-MM-yyyy")
        End If

        If (v = 90) Then
            dataInizio.Text = Format(Date.Now.AddDays(-90), "dd-MM-yyyy")
        End If

        Session("filtroDocumentoDataInizio") = dataInizio.Text
        Session("filtroDocumentoDataFine") = dataFine.Text

        applicaFiltri(Nothing, Nothing)

    End Sub

    Sub applicaFiltri(sender As Object, e As EventArgs)

        Dim idStato As Integer = filtroStati.SelectedValue
        Dim inizio As Date
        Dim fine As Date

        Try
            inizio = Date.Parse(dataInizio.Text)
        Catch
            dataInizio.Text = ""
        End Try

        Try
            fine = Date.Now
            fine = Date.Parse(dataFine.Text)
        Catch
            dataFine.Text = Format(fine, "dd-MM-yyyy")
        End Try

        Dim condizione As String = ""

        If idStato > -1 Then
            condizione = "AND (StatiId = @idStato)"
        End If

        sdsDocumenti.SelectCommand = "SELECT * FROM (`vdocumenti` LEFT JOIN `utenti` ON ((`vdocumenti`.`UtentiId` = `utenti`.`Id`)) LEFT JOIN ( SELECT id, Link_Tracking FROM `vettori`) AS vettori ON (`vdocumenti`.`VettoriId` = `vettori`.`id`) ) WHERE ( (UtentiId = ?UtentiId ) AND (TipoDocumentiId = ?TipoDocumentiId ) AND (DataDocumento >= '" & Format(inizio, "yyyy-MM-dd") & "') AND (DataDocumento <= '" & Format(fine, "yyyy-MM-dd") & "' ) " & condizione & " ) ORDER BY vdocumenti.ID DESC"
        sdsDocumenti.SelectParameters.Clear()
        sdsDocumenti.SelectParameters.Add("@idStato", idStato)
        'sdsDocumenti.SelectCommand = "SELECT * FROM (`vdocumenti` LEFT JOIN `utenti` ON ((`vdocumenti`.`UtentiId` = `utenti`.`Id`))) WHERE ( (UtentiId = ?UtentiId) AND (TipoDocumentiId = ?TipoDocumentiId) AND (DataDocumento >= '" & Format(inizio, "yyyy-MM-dd") & "') AND (DataDocumento <= '" & Format(fine, "yyyy-MM-dd") & "' ) " & condizione & " ) ORDER BY vdocumenti.ID DESC"

        Session("filtroDocumentoDataInizio") = dataInizio.Text
        Session("filtroDocumentoDataFine") = dataFine.Text

        rTipo.DataBind()

    End Sub

    Sub stampaClick(sender As Object, e As System.Web.UI.ImageClickEventArgs)

        Dim link As ImageButton = sender

        Dim id As String
        id = link.Attributes("idDoc")

        Dim esito As Integer = -1

        Try

            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            cmd.Connection = conn

            conn.Open()

            strSql = "INSERT INTO INVIADOCUMENTI "
            strSql &= " ( "
            strSql &= " UTENTIID, "
            strSql &= " AZIENDEID, "
            strSql &= " DOCUMENTIID, "
            strSql &= " DataRichiesta "
            strSql &= " ) VALUES ("
            strSql &= "@UTENTIID, "
            strSql &= "@AziendaID, "
            strSql &= id & ", "
            strSql &= "Now() )"


            cmd.CommandType = CommandType.Text
            cmd.CommandText = strSql
            cmd.Parameters.AddWithValue("@UTENTIID", Session("UTENTIID"))
            cmd.Parameters.AddWithValue("@AziendaID", Session("AziendaID"))
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            'Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Richiesta inoltrata. Riceverà il documento presso la sua casella email!')}</script>")
            'Comunico l'esecuzione positiva della richiesta 
            Session("esito_invio_mail") = 1

        Catch ex As Exception
            Session("esito_invio_mail") = 0
            Me.Response.Redirect("documenti.aspx?t=" & Request.QueryString("t") & "&esito=0&err=" & ex.Message)

            'Comunico l'esecuzione negativa della richiesta 
            'esito = 0
            'Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Impossibile inoltrare la richiesta')}</script>")
        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

            Session("esito_invio_mail") = 1
            Me.Response.Redirect("documenti.aspx?t=" & Request.QueryString("t"))
        End Try


    End Sub


    Protected Sub GridView1_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridView1.RowCommand
        'Test Prerender per testare se un documento è

        Dim esito As Integer = -1
        If Page.IsPostBack = False Then
            Try
                Dim c As Control = DirectCast(e.CommandSource, Control)
                Dim r As GridViewRow = DirectCast(c.NamingContainer, GridViewRow)

                'Dim DOC As String = DirectCast(GridView1.Rows(r.RowIndex).FindControl("HyperLink1"), HyperLink).Text
                Dim ID_DOC As String = DirectCast(GridView1.Rows(r.RowIndex).FindControl("iddoc"), HyperLink).Text

                If (e.CommandName = "Stampa") Then
                    conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
                    cmd.Connection = conn

                    conn.Open()

                    strSql = "INSERT INTO INVIADOCUMENTI "
                    strSql &= " ( "
                    strSql &= " UTENTIID, "
                    strSql &= " DOCUMENTIID, "
                    strSql &= " DataRichiesta "
                    strSql &= " ) VALUES ("
                    strSql &= "@UTENTIID, "
                    strSql &= ID_DOC & ", "
                    strSql &= "Now() )"

                    cmd.CommandType = CommandType.Text
                    cmd.CommandText = strSql
                    cmd.Parameters.AddWithValue("@UTENTIID", Session("UTENTIID"))

                    cmd.ExecuteNonQuery()
                    cmd.Dispose()

                    'Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Richiesta inoltrata. Riceverà il documento presso la sua casella email!')}</script>")
                    'Comunico l'esecuzione positiva della richiesta 
                    Session("esito_invio_mail") = 1
                End If

            Catch ex As Exception
                Session("esito_invio_mail") = 0
                Me.Response.Redirect("documenti.aspx?err=" & ex.Message)
            Finally

                If conn.State = ConnectionState.Open Then
                    conn.Close()
                    conn.Dispose()
                End If

                Session("esito_invio_mail") = 1
                Me.Response.Redirect("documenti.aspx")
            End Try
        End If
    End Sub

    Protected Sub dataInizio_PreRender(sender As Object, e As System.EventArgs) Handles dataInizio.PreRender
        If (dataInizio.Text = "") Then

            If Session("filtroDocumentoDataInizio") <> "" Then
                dataInizio.Text = Session("filtroDocumentoDataInizio")
            Else
                dataInizio.Text = ""
            End If

        End If
    End Sub

    Protected Sub dataFine_PreRender(sender As Object, e As System.EventArgs) Handles dataFine.PreRender
        If (dataFine.Text = "") Then

            If Session("filtroDocumentoDataInizio") <> "" Then
                dataFine.Text = Session("filtroDocumentoDataFine")
            Else
                dataFine.Text = Format(Date.Now, "dd-MM-yyyy")
            End If

        End If
    End Sub

    Protected Sub sdsDocumenti_Selected(sender As Object, e As System.Web.UI.WebControls.SqlDataSourceStatusEventArgs) Handles sdsDocumenti.Selected
        nDocTrovati = e.AffectedRows.ToString()
    End Sub

    Protected Sub Calendar1_SelectionChanged(sender As Object, e As System.EventArgs) Handles Calendar1.SelectionChanged
        dataInizio.Text = Format(Calendar1.SelectedDate, "dd-MM-yyyy")
        Calendar1.Visible = False
    End Sub

    Protected Sub Calendar2_SelectionChanged(sender As Object, e As System.EventArgs) Handles Calendar2.SelectionChanged
        dataFine.Text = Format(Calendar2.SelectedDate, "dd-MM-yyyy")
        Calendar2.Visible = False
    End Sub

    Protected Sub ib_calendarInizio_Click(sender As Object, e As System.Web.UI.ImageClickEventArgs) Handles ib_calendarInizio.Click
        Calendar1.Visible = True
    End Sub

    Protected Sub ImageButton1_Click(sender As Object, e As System.Web.UI.ImageClickEventArgs) Handles ImageButton1.Click
        Calendar2.Visible = True
    End Sub
End Class
