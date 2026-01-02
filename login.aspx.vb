Imports MySql.Data.MySqlClient
Imports System.Data

Partial Class Login
    Inherits System.Web.UI.Page
    

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Me.Session("LoginId") is Nothing Then
			Response.Redirect("default.aspx")
		else if session("loginResult") <> ""
			Me.lblLogin.Text = session("loginResult")
			session("loginResult") =  ""
		end if
		If Me.tbUsername.Text <> "" AndAlso Me.tbPassword.Text <> "" Then
            Login()
        End If
    End Sub
	
	Private Sub Login() 
	
		Dim user As String = tbUsername.text
		Dim pass As String = tbPassword.text
		Dim conn As New MySqlConnection
		conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        Dim cmd As New MySqlCommand
		cmd.Connection = conn
        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "Select * from vlogin where AziendeID=?AziendaID and UPPER(Username)=?username limit 0, 1"
        cmd.Parameters.AddWithValue("?AziendaID", Session("AziendaID"))
        cmd.Parameters.AddWithValue("?username", user.ToUpper)

        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()
	
        If dr.HasRows Then
            If dr.Item("Abilitato") <> 1 Then
                session("loginResult") = "Login non attivo!"
                'Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Login non attivo!')}</script>")
            ElseIf dr.Item("UtentiAbilitato") <> 1 Then
                session("loginResult") = "Utente non attivo!"
                'Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Utente non attivo!')}</script>")
            ElseIf dr.Item("Password").ToString.ToLower = pass.ToLower Then
                'Login OK
                Try
                    Me.Session("AbilitaListino") = CType(dr.Item("AbilitaListino"), Integer)
                Catch
                    Me.Session("AbilitaListino") = 0
                End Try
				
                Me.Session("LoginId") = dr.Item("id")
                Me.Session("LoginEmail") = dr.Item("email")
                Me.Session("LoginNomeCognome") = dr.Item("cognomenome")
                ' Session("Iva_Utente") Da impostare, iva relativa all'utente
                If (dr.Item("ultimoaccesso") Is Nothing) = False Then
                    Me.Session("LoginUltimoAccesso") = dr.Item("ultimoaccesso")
                End If
                Me.Session("UtentiId") = dr.Item("utentiid")
                Me.Session("UtentiTipoId") = dr.Item("utentitipoid")

                'Indica se l'utente può o meno creare l'html per le promo mailing
                Me.Session("genera_html_mail") = dr.Item("genera_html_mail")

                'Iva applicata all'utente Utente - Esenzioni
                If dr.Item("idEsenzioneIva") <> -1 Then
                    Me.Session("Iva_Utente") = dr.Item("ValoreEsenzioneIva")
                    Session("DescrizioneEsenzioneIva") = dr.Item("DescrizioneEsenzioneIva")
                    Session("IdEsenzioneIva") = dr.Item("IdEsenzioneIva")
                    'Iva da applicare al vettore (da settare nella tabella Aziende)
                    Session("Iva_Vettori") = Session("Iva_Utente")
                Else
                    Session("IdEsenzioneIva") = -1
                    Session("DescrizioneEsenzioneIva") = ""
                    Me.Session("Iva_Utente") = -1
                End If

                'Reverse Charge Utente
                Session("AbilitatoIvaReverseCharge") = dr.Item("AbilitatoIvaReverseCharge")

                Me.Session("Listino") = dr.Item("listino")
                Me.Session("IvaTipo") = dr.Item("IvaTipo")
                Me.Session("DataPassword") = dr.Item("DataPassword")
                Me.Response.Cookies(Me.Session("AziendaNome"))("Username") = user
                Me.Response.Cookies("Password")("Password") = pass
                Me.Response.Cookies(Me.Session("AziendaNome")).Expires = DateTime.Now.AddYears(1)
            Else
                session("loginResult") = "Password Errata!"
                'Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Password Errata!')}</script>")
            End If
        Else
            session("loginResult") = "Username Errato!"
            'Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Username Errato!')}</script>")
        End If

        dr.Close()
        dr.Dispose()

        conn.Close()
        conn.Dispose()

        cmd.Dispose()

        If Not Me.Session("LoginId") Is Nothing Then
            AggiornaDati()
        End If

        If Not Session.Item("Pagina_visitata") Is Nothing Then
            Response.Redirect(Session.Item("Pagina_visitata").AbsoluteUri)
        End If

    End Sub

    Public Sub AggiornaDati()
		Dim conn As New MySqlConnection
		conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn.Open()
        
		Dim conn2 As New MySqlConnection
		conn2.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn2.Open()

        Dim cmd As New MySqlCommand
        cmd.Connection = conn2
        cmd.CommandType = CommandType.Text

        Dim comm As New MySqlCommand
        comm.Connection = conn
        comm.CommandType = CommandType.Text

        'Aggiorno ultimo accesso su Login
        comm.CommandText = "update login set ultimoaccesso= now() , UltimoIp=?UltimoIp, NumeroAccessi=NumeroAccessi+1 where id=?loginId"
        comm.Parameters.AddWithValue("?loginId", Session("LoginID"))
        comm.Parameters.AddWithValue("?UltimoIp", Me.Request.UserHostAddress)
        comm.ExecuteNonQuery()

        'Aggiorno LoginID sugli articoli del carrello
        comm.CommandText = "update carrello set LoginID=?loginId, SessionId='' where SessionId=?SessionId"
        comm.Parameters.AddWithValue("?SessionId", Session.SessionID)
        comm.ExecuteNonQuery()

        'Pulisco SessionID e NListino sugli articoli del carrello
        'comm.CommandText = "update carrello set SessionId='', NListino=" & Session("Listino") & " where LoginID=" & Session("LoginID")
        comm.CommandText = "update carrello set SessionId='', NListino=1"
        comm.ExecuteNonQuery()

        'Controllo se ci sono già altri articoli, e aggiorno solo le quantità

        Dim conn_controllo As New MySqlConnection
        conn_controllo.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn_controllo.Open()

        Dim cmd_controllo As New MySqlCommand
        cmd_controllo.Connection = conn_controllo
        cmd_controllo.CommandText = "SELECT * FROM carrello WHERE (LoginID=?loginId)"
        cmd_controllo.Parameters.AddWithValue("?loginId", Session("LoginID"))
        Dim controllo_articolo As Integer
        Dim dsdata As New DataSet

        Dim sqlAdp As New MySqlDataAdapter(cmd_controllo)
        sqlAdp.Fill(dsdata, "carrello")

        Dim ROW_temp As DataRow
        Dim i As Integer = 1
        Dim j As Integer
        cmd_controllo.Parameters.AddWithValue("?listino", Session("Listino"))
        Dim ArticoloID As Integer
        For Each ROW As DataRow In dsdata.Tables(0).Rows
            controllo_articolo = 0
            ArticoloID = ROW("ArticoliId")

            'Aggiorno il prezzo dell'articolo, con quello attuale
            cmd_controllo.CommandText = "UPDATE carrello SET Prezzo=(SELECT Prezzo FROM articoli_listini WHERE NListino=?listino AND ArticoliId=" & ArticoloID & "), PrezzoIvato=(SELECT PrezzoIvato FROM articoli_listini WHERE NListino=?listino AND ArticoliId=" & ArticoloID & ") WHERE id=" & ROW("id")
            cmd_controllo.ExecuteNonQuery()

            For j = i To dsdata.Tables(0).Rows.Count - 1
                ROW_temp = dsdata.Tables(0).Rows(j)
                If (ROW_temp("ArticoliId").ToString = ArticoloID.ToString) Then
                    cmd_controllo.CommandText = "UPDATE carrello SET QNT=QNT+" & ROW_temp("QNT") & " WHERE id=" & ROW("id")
                    cmd_controllo.ExecuteNonQuery()
                    cmd_controllo.CommandText = "DELETE FROM carrello WHERE id=" & ROW_temp("id")
                    cmd_controllo.ExecuteNonQuery()
                    Exit For
                End If
            Next
            i = i + 1
        Next
        dsdata.Dispose()
        conn_controllo.Close()

        'Aggiorno i prezzi del carrello con promo etc
        Dim LoginId As Integer = Me.Session("LoginId")
        Dim Qta As Integer
        Dim ID As Integer
        Dim ArtID As Integer
        Dim Listino As Integer = Me.Session("listino")
        Dim Prezzo As Double
        Dim PrezzoIvato As Double
        Dim OfferteDettagliID As Long
        Dim sb As StringBuilder = New StringBuilder()

        comm.CommandText = "SELECT * FROM vcarrello WHERE (LoginId=" & LoginId & ") ORDER BY id"

        Dim dr As MySqlDataReader = comm.ExecuteReader()
        While dr.Read()

            ID = dr.Item("ID")
            ArtID = dr.Item("ArticoliId")
            Qta = dr.Item("Qnt")
            Prezzo = 0
            PrezzoIvato = 0
            OfferteDettagliID = 0

            cmd.CommandText = "select * from vsuperarticoli where id=" & ArtID & " AND NListino=" & Listino & " ORDER BY PrezzoPromo DESC"

            Dim dr2 As MySqlDataReader = cmd.ExecuteReader()

            While dr2.Read()

                OfferteDettagliID = 0
                If Prezzo = 0 Then
                    Prezzo = dr2.Item("prezzo")
                End If
                If PrezzoIvato = 0 Then
                    PrezzoIvato = dr2.Item("prezzoivato")
                End If

                If dr2.Item("InOfferta") = 1 Then
                    If Qta >= dr2.Item("OfferteQntMinima") And dr2.Item("OfferteQntMinima") > 0 Then
                        OfferteDettagliID = dr2.Item("OfferteDettagliId")
                        Prezzo = dr2.Item("prezzopromo")
                        PrezzoIvato = dr2.Item("prezzopromoivato")
                    ElseIf dr2.Item("OfferteMultipli") > 0 Then
                        If Qta Mod dr2.Item("OfferteMultipli") = 0 Then
                            OfferteDettagliID = dr2.Item("OfferteDettagliId")
                            Prezzo = dr2.Item("prezzopromo")
                            PrezzoIvato = dr2.Item("prezzopromoivato")
                        End If
                    End If
                End If

            End While

            dr2.Close()
            dr2.Dispose()
            cmd.Dispose()

            sb.Append("UPDATE carrello SET ")
            sb.Append(" OfferteDettaglioId = " & CLng(OfferteDettagliID))
            sb.Append(", Prezzo = '" & Prezzo.ToString.Replace(",", ".") & "' ")
            sb.Append(", PrezzoIvato = '" & PrezzoIvato.ToString.Replace(",", ".") & "' ")
            sb.Append(" WHERE ID = ")
            sb.Append(ID)
            sb.Append(" ; ")

        End While

        dr.Close()
        dr.Dispose()

        If sb.ToString <> "" Then
            comm.CommandText = sb.ToString
            comm.ExecuteNonQuery()
        End If

        comm.Dispose()

        conn.Close()
        conn.Dispose()

        conn2.Close()
        conn2.Dispose()
    End Sub
	
End Class
