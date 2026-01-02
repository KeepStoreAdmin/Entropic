Imports System.Net.Mail
Imports MySql.Data.MySqlClient
Imports System.Data

Partial Class Contattaci
    Inherits System.Web.UI.Page



    Protected Sub Button_Invia_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button_Invia.Click
        Try
            Me.Label_esito.Visible = True

            Dim oMsg As MailMessage = New MailMessage()
            oMsg.From = New MailAddress(Me.TextBox_email.Text, Me.TextBox_nome.Text)
            oMsg.To.Add(New MailAddress(Session("AziendaEmail")))
            oMsg.Subject = Me.DropDownList_subject.SelectedValue
            oMsg.Body = Me.TextBox_testo.Text

            oMsg.IsBodyHtml = True

            Dim oSmtp As SmtpClient = New SmtpClient(Me.Session.Item("smtp"))
            oSmtp.DeliveryMethod = SmtpDeliveryMethod.Network

            Dim oCredential As System.Net.NetworkCredential = New System.Net.NetworkCredential(CType(Session.Item("User_smtp"), String), CType(Session.Item("Password_smtp"), String))
            oSmtp.UseDefaultCredentials = True
            oSmtp.Credentials = oCredential

            'ATTENZIONE
            oSmtp.Send(oMsg)

            Me.Label_esito.Text = "Richiesta inoltrata"
        Catch ex As Exception
            Me.Label_esito.Text = "Errore - " & ex.Message.ToString
        End Try

    End Sub

    Protected Sub Button_Invia_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button_Invia.Load
        Me.Label_esito.Visible = False
    End Sub
End Class
