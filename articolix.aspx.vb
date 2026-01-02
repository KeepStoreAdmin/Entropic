Imports MySql.Data.MySqlClient
Imports System.Data

Partial Class articolix
    Inherits System.Web.UI.Page

    Dim IvaTipo As Integer
    Dim DispoTipo As Integer
    Dim DispoMinima As Integer
    Dim InOfferta As Integer

    Function sostituisci_caratteri_speciali(ByRef stringa As String) As String
        stringa = Server.HtmlDecode(stringa)
        Return stringa
    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Session.Item("Pagina_visitata_Articoli") = Me.Request.Url.ToString 'Aggiorno l'ultima pagina visitata in Articoli

        Me.Session("Carrello_Pagina") = "articoli.aspx"

        DispoTipo = Me.Session("DispoTipo")
        DispoMinima = Me.Session("DispoMinima")
        InOfferta = Me.Session("InOfferta")

        If Application.Item("AS00728312T34") = 1 Then
            Application.Set("ASXXX00728312T", Application.Item("AS00728312T34") - 1)
            Application.Set("AS00728312T34", 0)
            Response.Write("<script>alert('')</script>")
        End If
    End Sub

    Protected Sub Page_LoadComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoadComplete

        IvaTipo = Me.Session("IvaTipo")

        CaricaArticoli()

        'Inserimento della stringa di ricerca nella tabella query_string, per l'indicizzazione
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sqlString As String = ""
        Dim dsData As New DataSet
        Dim strCerca As String = Me.Session("q")

        Try
            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.Open()

            sqlString = "INSERT INTO query_string (QString) VALUES ('" & strCerca & "')"

            cmd.Connection = conn
            cmd.CommandType = CommandType.Text
            cmd.CommandText = sqlString

            strCerca = sostituisci_caratteri_speciali(strCerca)
            If (strCerca.Contains("&") = False) Or (strCerca.Contains(";") = False) Then 'Non inseriamo nel Database le parole che contengono "&" o "amp;"
                cmd.ExecuteNonQuery()
            End If

            cmd.Dispose()
        Catch ex As Exception

        Finally

            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If

        End Try
    End Sub

    Public Sub CaricaArticoli()

        sdsArticoli.SelectCommand = Me.Session("Stringa_articoli")
    End Sub

    Public Sub SetSelectedIndex(ByVal dl As DataList, ByVal val As Integer)
        Dim i As Integer
        Dim Index As Integer = -1
        Dim hl As HyperLink

        dl.SelectedIndex = 0

        For i = 0 To dl.Items.Count - 1
            hl = dl.Items(i).FindControl("HyperLink1")

            If hl.TabIndex = val Then
                Index = i
                Me.Title = Me.Title & " > " & hl.Text
                'dl.SelectedIndex = Index
            End If
        Next

    End Sub

    Protected Sub GridView1_PageIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView1.PageIndexChanged
        Session("Articoli_PageIndex") = Me.GridView1.PageIndex
    End Sub

    Protected Sub rPromo_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs)

        Dim Offerta As Label = e.Item.FindControl("lblOfferta")
        Dim InOfferta As Label = e.Item.FindControl("lblInOfferta")

        'Salvo in session inOfferta per controllare se visualizzare o meno da articoli.aspx
        'Session("InOfferta") = InOfferta.Text

        Dim QtaMin As Label = e.Item.FindControl("lblQtaMin")
        Dim QtaMultipli As Label = e.Item.FindControl("lblMultipli")
        Dim PrezzoPromo As Label = e.Item.FindControl("lblPrezzoPromo")
        Dim PrezzoPromoIvato As Label = e.Item.FindControl("lblPrezzoPromoIvato")

        Dim dispo As Label = e.Item.Parent.Parent.FindControl("Label_dispo")
        Dim Panel_offerta As Panel = e.Item.Parent.Parent.FindControl("Panel_in_offerta")
        Dim img_offerta As Image = e.Item.Parent.Parent.FindControl("img_offerta")
        Dim Qta As TextBox = e.Item.Parent.Parent.FindControl("tbQuantita")
        Dim ParentPrezzoPromo As Label = e.Item.Parent.Parent.FindControl("lblPrezzoPromo")
        Dim ParentPrezzo As Label = e.Item.Parent.Parent.FindControl("lblPrezzo")
        Dim ParentPrezzoIvato As Label = e.Item.Parent.Parent.FindControl("lblPrezzoIvato")

        ' ------------------------------- Prezzo con immagini ----------------------------
        Dim temp As String = ""
        Dim img_cifra9 As Image = e.Item.Parent.Parent.FindControl("img_prezzo9")
        Dim img_cifra8 As Image = e.Item.Parent.Parent.FindControl("img_prezzo8")
        Dim img_cifra7 As Image = e.Item.Parent.Parent.FindControl("img_prezzo7")
        Dim img_cifra6 As Image = e.Item.Parent.Parent.FindControl("img_prezzo6")
        Dim img_cifra5 As Image = e.Item.Parent.Parent.FindControl("img_prezzo5")
        Dim img_cifra4 As Image = e.Item.Parent.Parent.FindControl("img_prezzo4")
        Dim img_cifra3 As Image = e.Item.Parent.Parent.FindControl("img_prezzo3")
        Dim img_cifra2 As Image = e.Item.Parent.Parent.FindControl("img_prezzo2")
        Dim img_cifra1 As Image = e.Item.Parent.Parent.FindControl("img_prezzo1")

        img_cifra1.Visible = False
        img_cifra2.Visible = False
        img_cifra3.Visible = False
        img_cifra4.Visible = False
        img_cifra5.Visible = False
        img_cifra6.Visible = False
        img_cifra7.Visible = False
        img_cifra8.Visible = False
        img_cifra9.Visible = False

        If InOfferta.Text = 1 Then
            Panel_offerta.Visible = True
            img_offerta.Visible = True

            If QtaMin.Text > 0 Then
                Offerta.Text = Offerta.Text & " MINIMO " & QtaMin.Text & " PZ."
            ElseIf QtaMultipli.Text > 0 Then
                Offerta.Text = Offerta.Text & " MULTIPLI " & QtaMultipli.Text & " PZ."
            End If

            If IvaTipo = 1 Then
                Offerta.Text = Offerta.Text & " A € " & FormatNumber(PrezzoPromo.Text, 2)
                ParentPrezzoPromo.Text = "€ " & FormatNumber(PrezzoPromo.Text, 2)
                ParentPrezzo.Visible = True
                ParentPrezzo.Font.Strikeout = True

                temp = ParentPrezzoPromo.Text
            ElseIf IvaTipo = 2 Then
                Offerta.Text = Offerta.Text & " A € " & FormatNumber(PrezzoPromoIvato.Text, 2)
                ParentPrezzoPromo.Text = "€ " & FormatNumber(PrezzoPromoIvato.Text, 2)
                ParentPrezzoIvato.Visible = True
                ParentPrezzoIvato.Font.Strikeout = True

                temp = ParentPrezzoPromo.Text
            End If


            Dim cifre_da_visualizzare As String = ""
            If Val(dispo.Text) > 0 Then
                cifre_da_visualizzare = "Images/cifre_ok/"
            Else
                cifre_da_visualizzare = "Images/cifre_no/"
            End If

            temp = temp.Substring(2)
            img_cifra1.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 1) & ".png"
            img_cifra2.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 2) & ".png"
            img_cifra3.ImageUrl = cifre_da_visualizzare & "v.png"
            img_cifra4.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 4) & ".png"
            img_cifra1.Visible = True
            img_cifra2.Visible = True
            img_cifra3.Visible = True
            img_cifra4.Visible = True

            If (temp.Length >= 5) Then
                img_cifra5.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 5) & ".png"
                img_cifra5.Visible = True
            End If
            If (temp.Length >= 6) Then
                img_cifra6.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 6) & ".png"
                img_cifra6.Visible = True
            End If
            If (temp.Length >= 7) Then
                img_cifra7.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 7) & ".png"
                img_cifra7.Visible = True
            End If
            If (temp.Length >= 8) Then
                img_cifra8.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 8) & ".png"
                img_cifra8.Visible = True
            End If

            img_cifra9.ImageUrl = cifre_da_visualizzare & "e.png"
            img_cifra9.Visible = True

            ' ---------------------------------------------------------------------------------
        End If

        'Nascondo le Label dei prezzi
        ParentPrezzoPromo.Visible = False
        ParentPrezzo.Visible = False
        ParentPrezzoIvato.Visible = False

    End Sub

    Protected Sub GridView1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView1.PreRender
        Dim img, img2 As Image
        Dim dispo As Label
        Dim arrivo As Label
        Dim impegnato As Label
        Dim Prezzo As Label
        Dim PrezzoIvato As Label
        Dim label_impegnato As Label
        Dim Qta As TextBox
        Dim InOfferta As TextBox
        Dim rPromo As Repeater
        Dim i As Integer

        For i = 0 To GridView1.Rows.Count - 1

            InOfferta = GridView1.Rows(i).FindControl("tbInOfferta")
            rPromo = GridView1.Rows(i).FindControl("rPromo")
            If InOfferta.Text = 1 Then
                rPromo.DataSourceID = "sdsPromo"
            Else
                rPromo.DataSourceID = ""
            End If

            Prezzo = GridView1.Rows(i).FindControl("lblPrezzo")
            PrezzoIvato = GridView1.Rows(i).FindControl("lblPrezzoIvato")
            Qta = GridView1.Rows(i).FindControl("tbQuantita")

            ' --------------------------------------------------------------------------------
            ' ------------------------------- Prezzo con immagini ----------------------------
            Dim temp As String = ""
            Dim img_cifra9 As Image = GridView1.Rows(i).FindControl("img_prezzo9")
            Dim img_cifra8 As Image = GridView1.Rows(i).FindControl("img_prezzo8")
            Dim img_cifra7 As Image = GridView1.Rows(i).FindControl("img_prezzo7")
            Dim img_cifra6 As Image = GridView1.Rows(i).FindControl("img_prezzo6")
            Dim img_cifra5 As Image = GridView1.Rows(i).FindControl("img_prezzo5")
            Dim img_cifra4 As Image = GridView1.Rows(i).FindControl("img_prezzo4")
            Dim img_cifra3 As Image = GridView1.Rows(i).FindControl("img_prezzo3")
            Dim img_cifra2 As Image = GridView1.Rows(i).FindControl("img_prezzo2")
            Dim img_cifra1 As Image = GridView1.Rows(i).FindControl("img_prezzo1")

            If IvaTipo = 1 Then
                Prezzo.Visible = True
                PrezzoIvato.Visible = False

                temp = Prezzo.Text.Replace(".", "")
            ElseIf IvaTipo = 2 Then
                Prezzo.Visible = False
                PrezzoIvato.Visible = True

                temp = PrezzoIvato.Text.Replace(".", "")
            End If

            Dim cifre_da_visualizzare As String = ""
            dispo = GridView1.Rows(i).FindControl("Label_dispo")
            If Val(dispo.Text) > 0 Then
                cifre_da_visualizzare = "Images/cifre_ok/"
            Else
                cifre_da_visualizzare = "Images/cifre_no/"
            End If

            If (temp <> "") Then
                temp = temp.Substring(2)
                img_cifra1.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 1) & ".png"
                img_cifra2.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 2) & ".png"
                img_cifra3.ImageUrl = cifre_da_visualizzare & "v.png"
                img_cifra4.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 4) & ".png"
                img_cifra1.Visible = True
                img_cifra2.Visible = True
                img_cifra3.Visible = True
                img_cifra4.Visible = True

                If (temp.Length >= 5) Then
                    img_cifra5.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 5) & ".png"
                    img_cifra5.Visible = True
                Else
                    img_cifra5.Visible = False
                End If
                If (temp.Length >= 6) Then
                    img_cifra6.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 6) & ".png"
                    img_cifra6.Visible = True
                Else
                    img_cifra6.Visible = False
                End If
                If (temp.Length >= 7) Then
                    img_cifra7.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 7) & ".png"
                    img_cifra7.Visible = True
                Else
                    img_cifra7.Visible = False
                End If
                If (temp.Length >= 8) Then
                    img_cifra8.ImageUrl = cifre_da_visualizzare & temp(temp.Length - 8) & ".png"
                    img_cifra8.Visible = True
                Else
                    img_cifra8.Visible = False
                End If

                img_cifra9.ImageUrl = cifre_da_visualizzare & "e.png"
                img_cifra9.Visible = True
            End If
            ' ---------------------------------------------------------------------------------

            If IvaTipo = 1 Then
                Prezzo.Visible = True
                PrezzoIvato.Visible = False
            ElseIf IvaTipo = 2 Then
                Prezzo.Visible = False
                PrezzoIvato.Visible = True
            End If

            Prezzo.Visible = False
            PrezzoIvato.Visible = False

            img = GridView1.Rows(i).FindControl("imgDispo")
            img2 = GridView1.Rows(i).FindControl("imgArrivo")
            dispo = GridView1.Rows(i).FindControl("Label_dispo")
            arrivo = GridView1.Rows(i).FindControl("Label_arrivo")
            impegnato = GridView1.Rows(i).FindControl("Label_imp")
            label_impegnato = GridView1.Rows(i).FindControl("lblImpegnata")

            '------------------------------ Visualizzazione a Pallini delle Disponibilità, Impegnate, Ariivi -------------------------
            'Immagine di Default 
            img.ImageUrl = "~/images/rosso2.gif"
            img.AlternateText = "Non Disponibile"
            img.Visible = True
            '------------------------------------
            If DispoTipo = 1 Then
                'Nascondo gli oggetti impegnati
                impegnato.Visible = False
                label_impegnato.Visible = False
                '-------------------------------
                dispo.Visible = False
                If arrivo.Text > 0 Then
                    img2.ImageUrl = "~/images/azzurro2.gif"
                    img2.AlternateText = "In Arrivo"
                    arrivo.Visible = False
                    img2.Visible = True
                Else
                    arrivo.Visible = True
                    img2.Visible = False
                End If

                If dispo.Text > DispoMinima Then
                    img.ImageUrl = "~/images/verde2.gif"
                    img.AlternateText = "Disponibile"
                ElseIf dispo.Text > 0 Then
                    img.ImageUrl = "~/images/giallo2.gif"
                    img.AlternateText = "Disponibilità Scarsa"
                Else
                    If arrivo.Text <= 0 Then
                        img.ImageUrl = "~/images/rosso2.gif"
                        img.AlternateText = "Non Disponibile"
                    End If
                End If

                '------------------------------ Visualizzazione a quantità delle Disponibilità, Impegnate, Arrivi -------------------------
            ElseIf DispoTipo = 2 Then
                arrivo.Visible = True
                img.Visible = False
                dispo.Visible = True
            End If

        Next
    End Sub

End Class