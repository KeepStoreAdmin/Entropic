Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Net.Mail

Partial Class cambiapassword
    Inherits System.Web.UI.Page

    Dim strSql As String = ""

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Me.Session("LoginId") Is Nothing Then
            Me.Session("Page") = Me.Request.Url.ToString
            Me.Response.Redirect("accessonegato.aspx")
        End If

        Me.MaintainScrollPositionOnPostBack = True

        Me.lblSito.Text = Session("AziendaNome")
        Me.lblMesi.Text = Session("ScadenzaPassword")

    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        Me.Title = Me.Title & " - Registrazione al sito"
        CaricaDati()
    End Sub


    Public Sub CaricaDati()
        Dim params As New Dictionary(Of String, String)
        params.add("@LoginID", Session("LoginID"))
        Dim dr = ExecuteQueryGetDataReader("*", "vlogin", "WHERE (id=@LoginID)", params)

        If dr.Count > 0 Then
            Dim row = dr(0)
            Me.tbUsername.Text = row("username").ToString
            Me.tbPasswordOK.Text = row("password").ToString
            Me.tbEmail.Text = row("email").ToString
        End If
    End Sub


    Protected Sub btRegistrati_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btRegistrati.Click
        Dim params As New Dictionary(Of String, String)
        params.add("@passwordConferma", Me.tbPasswordConferma.Text)
        params.add("@dataPassword", System.DateTime.Today.Year & "-" & System.DateTime.Today.Month & "-" & System.DateTime.Today.Day)
        params.add("@LoginId", Session("LoginID"))
        ExecuteUpdate("login", "password=@passwordConferma, DataPassword=@dataPassword", "WHERE id = @LoginId", params)

        Me.tRegistrazione.Visible = False
        Me.tAggiorna.Visible = True

        Me.Session("DataPassword") = System.DateTime.Today

        Email("Password aggiornata sul sito ")

    End Sub

    Public Sub Email(ByVal oggetto As String)

        Dim oMsg As MailMessage = New MailMessage()
        oMsg.From = New MailAddress(Session("AziendaEmail"), Session("AziendaNome"))
        oMsg.To.Add(Me.Session("LoginEmail"))
        'oMsg.Bcc.Add(New MailAddress(Session("AziendaEmail"), Session("AziendaNome")))
        oMsg.Subject = oggetto & Session("AziendaNome")
        oMsg.Body = "<font face=arial size=2 color=black>Gentile " & Me.Session("LoginNomeCognome") & "," &
                    "<br>Le comunichiamo i suoi nuovi dati di accesso al sito web <u>" & Session("AziendaUrl") & "</u>" &
                    "<br><br><b>Username:</b> " & Me.tbUsername.Text & "<br><b>Password:</b> " & Me.tbPasswordConferma.Text & "<br><b>Email:</b> " & Me.tbEmail.Text & " </b>" &
                    "<br><br><br><font face=arial size=2 color=black><b>" & Session("AziendaNome") & "</b><br>" & Session("AziendaDescrizione") & "<br>Sito Web: <a href=http://" & Session("AziendaUrl") & ">http://" & Session("AziendaUrl") & "</a> - Email: <a href=mailto:" & Session("AziendaEmail") & ">" & Session("AziendaEmail") & "</a></font>" &
                    "<br><br><font face=arial size=1 color=silver>D.Lgs 196/2003 tutela delle persone di altri soggetti rispetto al trattamento di dati personali.<br>La presente comunicazione è destinata esclusivamente al soggetto indicato più sopra quale destinatario o ad eventuali altri soggetti autorizzati a riceverla. Essa contiene informazioni strettamente confidenziali e riservate, la cui comunicazione o diffusione a terzi è proibita, salvo che non sia espressamente autorizzata. Se avete ricevuto questa comunicazione per errore, o se desiderate non ricevere più comunicazioni su novità e offerte, Vi preghiamo di darne immediata comunicazione al mittente scrivendo a " & Me.Session("AziendaEmail") & ".<br>Si informa che i dati forniti saranno tenuti rigorosamente riservati, saranno utilizzati unicamente da " & Me.Session("AziendaNome") & " per comunicare offerte promozionali o novità sui prodotti/servizi e resteranno a disposizione per eventuali variazioni o per la cancellazione ai sensi dell'art. 7 del citato decreto legislativo.</font>"

        oMsg.IsBodyHtml = True

        Dim oSmtp As SmtpClient = New SmtpClient("smtp.entropic.it")
        oSmtp.DeliveryMethod = SmtpDeliveryMethod.Network

        Dim oCredential As System.Net.NetworkCredential = New System.Net.NetworkCredential(CType(Session.Item("User_smtp"), String), CType(Session.Item("Password_smtp"), String))
        oSmtp.UseDefaultCredentials = True
        oSmtp.Credentials = oCredential

        oSmtp.Send(oMsg)

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
                Dim cmd As New MySqlCommand
                cmd.Connection = conn
                cmd.CommandText = sqlString
                For Each paramName In params.Keys
                    If paramName = "?parPrezzo" Or paramName = "?parPrezzoIvato" Then
                        cmd.Parameters.Add(paramName, MySqlDbType.Double).Value = Convert.ToDecimal(params(paramName), System.Globalization.CultureInfo.InvariantCulture)
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
