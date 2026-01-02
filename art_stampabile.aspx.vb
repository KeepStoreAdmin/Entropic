Imports MySql.Data.MySqlClient
Imports System.Data

Partial Class art_stampabile
    Inherits System.Web.UI.Page
    Dim IvaTipo As Integer
    Dim DispoTipo As Integer
    Dim DispoMinima As Integer

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        IvaTipo = Me.Session("IvaTipo")
        DispoTipo = Me.Session("DispoTipo")
        DispoMinima = Me.Session("DispoMinima")
        ImpostaTemplate()
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        SettaTitolo()
        AggiornaVisite()
    End Sub

    Public Sub SettaTitolo()
        Try
            Dim lblDescrizione As Label
            lblDescrizione = Me.fvPage.FindControl("lblDescrizione")
            Me.Title = Me.Title & " > " & lblDescrizione.Text
        Catch ex As Exception
        End Try
    End Sub

    Public Sub ImpostaTemplate()

        Dim objcss As New HtmlLink()
        Dim obj2 As New HtmlLink()
        Dim obj3 As New HtmlLink()
        objcss.Href = "~/public/style/" & Session("css")
        objcss.Attributes.Add("rel", "stylesheet")
        objcss.Attributes.Add("type", "text/css")

        obj3.Attributes.Add("rel", "shortcut icon")
        If (Session.Item("AziendaID") = 1) Then
            obj3.Href = "entropic.ico"
        Else
            obj3.Href = "webaffare.ico"
        End If

        obj2.Attributes.Add("rel", "image_src")
        obj2.ID = "Immagine_Facebook"
        Me.Page.Header.Controls.Add(objcss)
        Me.Page.Header.Controls.Add(obj2)
        Me.Page.Header.Controls.Add(obj3)
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
                Prezzo.Visible = True
                PrezzoIvato.Visible = False
            ElseIf IvaTipo = 2 Then
                PrezzoDes.Text = "Prezzo Iva Inclusa"
                Prezzo.Visible = False
                PrezzoIvato.Visible = True
            End If

            img = Me.fvPage.FindControl("imgDispo")
            dispo = Me.fvPage.FindControl("lblDispo")
            impegnato = Me.fvPage.FindControl("lblImpegnata")
            arrivo = Me.fvPage.FindControl("lblArrivo")

            If DispoTipo = 1 Then

                If dispo.Text > DispoMinima Then
                    img.ImageUrl = "~/images/verde.gif"
                    img.AlternateText = "Disponibile"
                ElseIf dispo.Text > 0 Then
                    img.ImageUrl = "~/images/giallo.gif"
                    img.AlternateText = "Disponibilità Scarsa"
                Else
                    If arrivo.Text > 0 Then
                        img.ImageUrl = "~/images/azzurro.gif"
                        img.AlternateText = "In Arrivo"
                    Else
                        img.ImageUrl = "~/images/rosso.gif"
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

    Protected Sub fvPage_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles fvPage.PreRender
        Try
            SettaDisponibilita()
            Dim lblDes As Label
            lblDes = Me.fvPage.FindControl("lblDescrizioneArt")
            If lblDes.Text <> "" Then
                lblDes.Text = lblDes.Text.Replace(vbNewLine, "<br>")
            End If
        Catch
        End Try
    End Sub

    Protected Sub ImageButton1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)

        Dim Qta As TextBox
        Qta = Me.fvPage.FindControl("tbQuantita")

        Me.Session("Carrello_ArticoloId") = Me.Request.QueryString("id")
        Me.Session("Carrello_Quantita") = Qta.Text
        Me.Response.Redirect("aggiungi.aspx")

    End Sub

    Public Sub AggiornaVisite()
        Dim id As Integer = Me.Request.QueryString("id")
        If id <> CLng(Me.Session("visite_articoloid")) Then
            Me.Session("visite_articoloid") = id
            Dim params As New Dictionary(Of String, String)
            params.add("@id", ArticoliId)
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

        If InOfferta.Text = 1 Then

            If QtaMin.Text > 0 Then
                Offerta.Text = Offerta.Text & " MINIMO " & QtaMin.Text & " PZ."
                Qta.Text = QtaMin.Text
            ElseIf QtaMultipli.Text > 0 Then
                Offerta.Text = Offerta.Text & " MULTIPLI " & QtaMultipli.Text & " PZ."
                Qta.Text = QtaMultipli.Text
            End If

            If IvaTipo = 1 Then
                Offerta.Text = Offerta.Text & " A € " & FormatNumber(PrezzoPromo.Text, 2)
                ParentPrezzoPromo.Text = "€ " & FormatNumber(PrezzoPromo.Text, 2)
                ParentPrezzo.Font.Strikeout = True
            ElseIf IvaTipo = 2 Then
                Offerta.Text = Offerta.Text & " A € " & FormatNumber(PrezzoPromoIvato.Text, 2)
                ParentPrezzoPromo.Text = "€ " & FormatNumber(PrezzoPromoIvato.Text, 2)
                ParentPrezzoIvato.Font.Strikeout = True
            End If

        End If

    End Sub

    Protected Sub Image1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs)

        Dim img As Image = sender
        Dim imageurl As String = Server.MapPath(img.ImageUrl)

        'Dim temp_obj As HtmlLink
        'temp_obj = Me.Page.Master.FindControl("Immagine_Facebook")
        'temp_obj.Href = img.ImageUrl.ToString

        Try
            Dim bmp As System.Drawing.Image = System.Drawing.Image.FromFile(imageurl)

            If bmp.Width > 400 Then
                img.Width = 400
            End If
        Catch ex As Exception

        End Try

    End Sub

End Class
