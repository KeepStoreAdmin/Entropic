
Partial Class search_complete
    Inherits System.Web.UI.Page

    Protected Sub Button_Abilita_Marche_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button_Abilita_Marche.Click
        If (Me.DropDownList_Marche.Enabled = False) Then
            Me.DropDownList_Marche.Enabled = True
            Me.Button_Abilita_Marche.Text = "Disabilita"
        Else
            Me.DropDownList_Marche.Enabled = False
            Me.Button_Abilita_Marche.Text = "Abilita"
        End If
    End Sub


    Protected Sub Button_Abilita_Tipologie_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button_Abilita_Tipologie.Click
        If (Me.DropDownList_Tipologie.Enabled = False) Then
            Me.DropDownList_Tipologie.Enabled = True
            Me.Button_Abilita_Tipologie.Text = "Disabilita"
        Else
            Me.DropDownList_Tipologie.Enabled = False
            Me.Button_Abilita_Tipologie.Text = "Abilita"
        End If
    End Sub

    Protected Sub Button_Abilita_Gruppi_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button_Abilita_Gruppi.Click
        If (Me.DropDownList_Gruppi.Enabled = False) Then
            Me.DropDownList_Gruppi.Enabled = True
            Me.Button_Abilita_Gruppi.Text = "Disabilita"
        Else
            Me.DropDownList_Gruppi.Enabled = False
            Me.Button_Abilita_Gruppi.Text = "Abilita"
        End If
    End Sub

    Protected Sub Button_Abilita_Sottogruppi_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button_Abilita_Sottogruppi.Click
        If (Me.DropDownList_Sottogruppi.Enabled = False) Then
            Me.DropDownList_Sottogruppi.Enabled = True
            Me.Button_Abilita_Sottogruppi.Text = "Disabilita"
        Else
            Me.DropDownList_Sottogruppi.Enabled = False
            Me.Button_Abilita_Sottogruppi.Text = "Abilita"
        End If
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoadComplete
        If Me.Page.IsPostBack = False Then
            Me.DropDownList_Gruppi.Enabled = False
            Me.DropDownList_Marche.Enabled = False
            Me.DropDownList_Sottogruppi.Enabled = False
            Me.DropDownList_Tipologie.Enabled = False
        End If

        Session.Item("Sto_usando_search_complete") = 1
    End Sub

    Protected Sub Button_Effettua_Ricerca_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button_Effettua_Ricerca.Click
        Dim Link As String

        Link = "articoli.aspx?st=0"

        If Me.DropDownList_Categorie.Enabled = False Then
            Link = Link & "&ct=30000"
        Else
            Link = Link & "&ct=" & Me.DropDownList_Categorie.SelectedValue
        End If

        If Me.DropDownList_Tipologie.Enabled = False Then
            Link = Link & "&tp=0"
        Else
            Link = Link & "&tp=" & Me.DropDownList_Tipologie.SelectedValue
        End If

        If Me.DropDownList_Gruppi.Enabled = False Then
            Link = Link & "&gr=0"
        Else
            Link = Link & "&gr=" & Me.DropDownList_Gruppi.SelectedValue
        End If

        If Me.DropDownList_Sottogruppi.Enabled = False Then
            Link = Link & "&sg=0"
        Else
            Link = Link & "&sg=" & Me.DropDownList_Sottogruppi.SelectedValue
        End If

        If Me.DropDownList_Marche.Enabled = False Then
            Link = Link & "&mr=0"
        Else
            Link = Link & "&mr=" & Me.DropDownList_Marche.SelectedValue
        End If

        If Me.Text_Descrizione.Text <> "" Then
            Link = Link & "&q=" & Me.Text_Descrizione.Text
        End If

        If (Me.CheckBox_InPromo.Checked = True) Then
            Link = Link & "&inpromo=1"
        Else
            Link = Link & "&inpromo=0"
        End If

        If Val(Me.TextBox_PrezzoMin.Text) > 0 Then
            Session.Item("Prezzo_MIN") = Me.TextBox_PrezzoMin.Text
        Else
            Session.Item("Prezzo_MIN") = ""
        End If

        If Val(Me.TextBox_PrezzoMax.Text) > 0 Then
            Session.Item("Prezzo_MAX") = Me.TextBox_PrezzoMax.Text
        Else
            Session.Item("Prezzo_MAX") = ""
        End If

        'Nel Caso in cui l'utente ha inserito un prezzo min maggiore di un massimo
        If (Val(Me.TextBox_PrezzoMin.Text) > Val(Me.TextBox_PrezzoMax.Text)) Then
            Session.Item("Prezzo_MIN") = Me.TextBox_PrezzoMin.Text
            Session.Item("Prezzo_MAX") = ""
        End If


        If Me.CheckBox_Disponibile.Checked = True Then
            Session.Item("Disp") = 1
            Link = Link & "&dispo=1"
        Else
            Session.Item("Disp") = 0
        End If

        'Salvo il valore del checkbox relativo ai prodotti con Spedizione GRATIS
        If Me.CheckBox_SpedizioneGratis.Checked = True Then
            Session.Item("SpedGratis") = 1
            Link = Link & "&spedgratis=1"
        Else
            Session.Item("SpedGratis") = 0
        End If

        Response.Redirect(Link)
    End Sub

    Protected Sub Button_Abilita_Categorie_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button_Abilita_Categorie.Click
        If (Me.DropDownList_Categorie.Enabled = True) Then
            Me.DropDownList_Categorie.Enabled = False

            'Abilito tutte le marche
            Me.Sql_Marche.SelectCommand = "SELECT DISTINCT marche.id, marche.Descrizione FROM marche INNER JOIN vsuperarticoli ON marche.id = vsuperarticoli.MarcheId ORDER BY marche.Descrizione"
            'Disabilita DropDownList
            Me.DropDownList_Gruppi.Enabled = False
            Me.DropDownList_Sottogruppi.Enabled = False
            Me.DropDownList_Tipologie.Enabled = False
            'Disabilita Bottoni
            Me.Button_Abilita_Sottogruppi.Enabled = False
            Me.Button_Abilita_Sottogruppi.Text = "Abilita"
            Me.Button_Abilita_Tipologie.Enabled = False
            Me.Button_Abilita_Tipologie.Text = "Abilita"
            Me.Button_Abilita_Gruppi.Enabled = False
            Me.Button_Abilita_Gruppi.Text = "Abilita"

            Me.Button_Abilita_Categorie.Text = "Filtra Categorie"
        Else
            'Abilito tutte le marche
            Me.Sql_Marche.SelectCommand = "SELECT DISTINCT marche.id, marche.Descrizione, vsuperarticoli.CategorieId FROM marche INNER JOIN vsuperarticoli ON marche.id = vsuperarticoli.MarcheId WHERE (vsuperarticoli.CategorieId = " & Me.DropDownList_Categorie.SelectedValue & ") ORDER BY marche.Descrizione"

            Me.DropDownList_Categorie.Enabled = True
            Me.Button_Abilita_Categorie.Text = "Tutte le Categorie"

            'Abilita Bottoni
            Me.Button_Abilita_Sottogruppi.Enabled = True
            Me.Button_Abilita_Sottogruppi.Text = "Disabilita"
            Me.Button_Abilita_Tipologie.Enabled = True
            Me.Button_Abilita_Tipologie.Text = "Disabilita"
            Me.Button_Abilita_Gruppi.Enabled = True
            Me.Button_Abilita_Gruppi.Text = "Disabilita"
        End If
    End Sub

End Class
