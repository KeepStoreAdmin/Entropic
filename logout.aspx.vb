
Partial Class logout
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim Pagina_visitata As Uri = Session.Item("Pagina_visitata")

        'Possibile che la pagina di rimando sia non in Sessione ma nel GET
        If (Request.QueryString("redirect") <> "") Then
            Session.Item("Pagina_visitata") = New Uri("http://" & Session("AziendaUrl") & "/" & Request.QueryString("redirect"))
        End If

        If Session.Item("Pagina_visitata") IsNot Nothing Then
            Pagina_visitata = Session.Item("Pagina_visitata")
        Else
            If (Not Session("AziendaUrl") Is Nothing) AndAlso (Session("AziendaUrl") <> "") Then
                Pagina_visitata = New Uri("http://" & Session("AziendaUrl") & "/Default.aspx")
            End If
        End If
        Me.Session.Clear()

        Session("Iva_Utente") = -1
        Session.Item("Prezzo_MIN") = ""
        Session.Item("Prezzo_MAX") = ""

        Session.Item("Pagina_visitata") = Pagina_visitata

        If Not Pagina_visitata Is Nothing Then
            Me.Response.Redirect(Pagina_visitata.AbsoluteUri)
        Else
            Me.Response.Redirect("Default.aspx")
        End If
    End Sub
End Class
