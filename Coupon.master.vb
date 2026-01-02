Imports MySql.Data.MySqlClient
Imports System.Data

Partial Class Coupon
    Inherits System.Web.UI.MasterPage

    Public cont_settori As Integer = 0

    Protected Sub TextBox_Campo_Ricerca_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox_Campo_Ricerca.TextChanged
        If TextBox_Campo_Ricerca.Text <> "" Then
            Response.Redirect("lista_coupon.aspx?search=" & TextBox_Campo_Ricerca.Text)
        End If
    End Sub

    Protected Sub Repeater_Settori_Categorie_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Settori_Categorie.ItemDataBound
        'Creo le Categorie rispettive all'ultimo Settore Creato dal Repeater
        'Lo faccio nel Prerender per lavorare l'ultimo elemento generato dal Repeater
        Dim temp_repeater As Repeater = sender
        Dim temp_sql As SqlDataSource = New SqlDataSource
        Dim temp_label As Label = New Label

        temp_label.Text = ""

        If (temp_repeater.Items.Count > 0) Then
            temp_label = temp_repeater.Items(temp_repeater.Items.Count - 1).FindControl("LB_Settore")
            temp_sql = temp_repeater.Items(temp_repeater.Items.Count - 1).FindControl("SqlDataCategorie")
            temp_sql.SelectCommand = "SELECT idCategoria, NomeCategoria, OrdinamentoCategoria, Attiva_Disattiva_Categoria, imgCategoria, linkCategoria FROM coupon_categorie WHERE idSettore=@idSettore ORDER BY OrdinamentoCategoria, Nomecategoria"
            temp_sql.SelectParameters.Clear()
            temp_sql.SelectParameters.Add("@idSettore", IIf(Val(temp_label.Text) > 0, Val(temp_label.Text), 0))
        End If
    End Sub

    'Creo le varie categorie per l'ultimo pulsante del repeater (perchè ItemDataBound arriva fino count-1, quindi per l'ultimo devo farlo nel prerender)
    Protected Sub Repeater_Settori_Categorie_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Repeater_Settori_Categorie.PreRender
        'Creo le Categorie rispettive all'ultimo Settore Creato dal Repeater
        'Lo faccio nel Prerender per lavorare l'ultimo elemento generato dal Repeater
        Dim temp_repeater As Repeater = sender
        Dim temp_sql As SqlDataSource = New SqlDataSource
        Dim temp_label As Label = New Label

        temp_label.Text = ""

        If (temp_repeater.Items.Count > 0) Then
            temp_label = temp_repeater.Items(temp_repeater.Items.Count - 1).FindControl("LB_Settore")
            temp_sql = temp_repeater.Items(temp_repeater.Items.Count - 1).FindControl("SqlDataCategorie")
            temp_sql.SelectCommand = "SELECT idCategoria, NomeCategoria, OrdinamentoCategoria, Attiva_Disattiva_Categoria, imgCategoria, linkCategoria FROM coupon_categorie WHERE idSettore=@idSettore ORDER BY OrdinamentoCategoria, Nomecategoria"
            temp_sql.SelectParameters.Clear()
            temp_sql.SelectParameters.Add("@idSettore", IIf(Val(temp_label.Text) > 0, Val(temp_label.Text), 0))

        End If
    End Sub

    Protected Sub button_login_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles button_login.Click
        Response.Redirect("login.aspx?username=" & textbox_username.Text & "&passw=" & textbox_password.Text & "&redirect=" & Request.Url.ToString)
    End Sub

    Public Sub LeggiAzienda()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand

        If IsNothing(Me.Session("AziendaID")) Then
            Dim sDominio As String = Me.Request.Url.Host
            If sDominio.Contains(".") Then
                Dim temp() As String = sDominio.Split(".")
                sDominio = temp(1)
            End If

            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            cmd.Connection = conn

            conn.Open()

            cmd.CommandType = CommandType.Text
            cmd.CommandText = "SELECT * FROM aziende LEFT JOIN pagine ON aziende.Id=Aziendeid WHERE url1 like '%@sDominio%' or url2 like '%@sDominio%' limit 0, 1"
            'cmd.CommandText = "Select * from aziende where url1='192.168.0.2' or url2='192.168.0.2' limit 0, 1"
            cmd.Parameters.AddWithValue("@sDominio", sDominio)
            Dim dr As MySqlDataReader = cmd.ExecuteReader()
            dr.Read()

            If Not dr.HasRows Then
                Response.Write("Nessun sito web configurato per questa applicazione.")
                Response.End()
            Else
                Me.Session("AziendaID") = dr.Item("Id")
                Me.Session("AziendaEmail") = dr.Item("Email")
                Me.Session("AziendaNome") = dr.Item("Nome")
                Me.Session("AziendaDescrizione") = dr.Item("Descrizione")
                Me.Session("AziendaLogo") = "public/images/" & dr.Item("logoWeb")
                Me.Session("AziendaUrl") = dr.Item("url1")
                Me.Session("Credits") = " <b>© " & DateTime.Now.Year.ToString & " " & dr.Item("RagioneSociale") & "</b> - " & dr.Item("Indirizzo") & " - " & dr.Item("Cap") & " " & dr.Item("Citta") & " (" & dr.Item("provincia") & ") - P.I. " & dr.Item("Piva") & " - Tel. " & dr.Item("Telefono") & " - Fax " & dr.Item("Fax")
                Me.Session("Credits2") = "<br>" & dr.Item("RagioneSociale") & "<br>" & dr.Item("Indirizzo") & "-" & dr.Item("Cap") & "<br>" & dr.Item("Citta") & " (" & dr.Item("provincia") & ")<br>P.Iva " & dr.Item("Piva") & "<br>Tel. " & dr.Item("Telefono") & "<br>Fax " & dr.Item("Fax")
                Me.Session("Listino") = dr.Item("ListinoDefault")
                Me.Session("ListinoUser") = dr.Item("ListinoUser")
                Me.Session("IvaTipo") = dr.Item("IvaTipo")
                Me.Session("CanOrder") = dr.Item("CanOrder")
                Me.Session("MagazzinoDefault") = dr.Item("MagazzinoDefault")
                Me.Session("DispoTipo") = dr.Item("DispoTipo")
                Me.Session("DispoMinima") = dr.Item("DispoMinima")
                Me.Session("RigheArticoli") = dr.Item("RigheArticoli")
                Me.Session("Abilita_Groupon") = dr.Item("Groupon")
                Me.Session("UtentiId") = -1
                Me.Session("ScadenzaPassword") = dr.Item("ScadenzaPassword")
                Me.Session("VetrinaArticoliNovita") = dr.Item("VetrinaArticoliNovita")
                Me.Session("VetrinaArticoliUltimiArriviPuntoVendita") = dr.Item("VetrinaArticoliUltimiArriviPuntoVendita")
                Me.Session("VetrinaArticoliImpatto") = dr.Item("VetrinaArticoliImpatto")
                Me.Session("VetrinaArticoliPiuVenduti") = dr.Item("VetrinaArticoliPiuVenduti")
                Me.Session("VetrinaPromoFissi") = dr.Item("VetrinaPromoFissi")
                Me.Session("VetrinaPromoRandom") = dr.Item("VetrinaPromoRandom")
                Me.Session("VetrinaPromoScadenza") = dr.Item("VetrinaPromoScadenza")
                Me.Session("VetrinaPromoInizio") = dr.Item("VetrinaPromoInizio")
                Me.Session("VetrinaDispoMinima") = dr.Item("VetrinaDispoMinima")
                Me.Session("css") = dr.Item("css")
                Me.Session("smtp") = dr.Item("smtp")
                Me.Session("User_smtp") = dr.Item("User_smtp")
                Me.Session("Password_smtp") = dr.Item("Password_smtp")
                Me.Session("AziendaCopyright") = dr.Item("copyright")
                Me.Session("AziendaDescrizioneServizioCoupon") = dr.Item("descrizione_servizio_coupon")
                Me.Session("AziendaLogoVerificSite1") = dr.Item("logo_verific_site1")
                Me.Session("AziendaLogoVerificSite2") = dr.Item("logo_verific_site2")
                Me.Session("AziendaLogoVerificSite3") = dr.Item("logo_verific_site3")
                Me.Session("AziendaLogoVerificSite4") = dr.Item("logo_verific_site4")
                Me.Session("LinkAziendaLogoVerificSite1") = dr.Item("link_logo_verific_site1")
                Me.Session("LinkAziendaLogoVerificSite2") = dr.Item("link_logo_verific_site2")
                Me.Session("LinkAziendaLogoVerificSite3") = dr.Item("link_logo_verific_site3")
                Me.Session("LinkAziendaLogoVerificSite4") = dr.Item("link_logo_verific_site4")
                Me.Session("AziendaLogoFooter") = "Images/Coupon/loghi/" & dr.Item("logo_footer")
                Me.Session("Script_Visite_Azienda") = dr.Item("statistiche_visite")
                Me.Session("facebookLink") = dr.Item("facebookLink")
                Me.Session("IconaWeb") = dr.Item("Icona_web")

                'Setto l'id del documento che mi indica il Coupon
                Session("IdDocumentoCoupon") = 18
                'Session("IdPagamentoCoupon") = 10

                'Iva da applicare al vettore (da settare nella tabella Aziende)
                If Session("Iva_Utente") > -1 Then
                    Session("Iva_Vettori") = Session("Iva_Utente")
                Else
                    Session("Iva_Vettori") = IvaVettoreDefault(Session("AziendaID"))
                End If

                'Setto l'abilitazione dell'utente all'IVA Reverce Charge o meno
                If Session("AbilitatoIvaReverseCharge") = 1 Then
                    Session("AbilitatoIvaReverseCharge") = 1
                Else
                    Session("AbilitatoIvaReverseCharge") = 0
                End If

                'icona_sito_web
                'icona_sito.Attributes("href") = "public/ico/" & dr.Item("Icona_web")

                Try
                    Me.Session("AccountPaypal") = dr.Item("AccountPaypal")
                Catch ex As Exception
                    Me.Session("AccountPaypal") = "000000"
                End Try

                Try
                    Me.Session("AccountIwBank") = dr.Item("AccountIwBank")
                Catch ex As Exception
                    Me.Session("AccountIwBank") = "000000"

                End Try
            End If

            dr.Close()
            dr.Dispose()

            conn.Close()
            conn.Dispose()

            cmd.Dispose()
        End If

        ImpostaTemplate()
    End Sub

    'Mi permette di leggere dal vettore l'IVA impostata per il Vettori 
    Function IvaVettoreDefault(ByVal AziendaId As Integer) As Double
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim dr As MySqlDataReader
        Dim temp_iva As Double = 0

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn.Open()

        cmd.Connection = conn
        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT vettori.*, iva.Valore FROM vettori LEFT JOIN iva ON vettori.iva=iva.id WHERE vettori.Predefinito=1 AND vettori.AziendeID=@AziendaId"
        cmd.Parameters.AddWithValue("@AziendaId", AziendaId)
        dr = cmd.ExecuteReader()
        dr.Read()

        If dr.HasRows = True Then
            temp_iva = dr.Item("Valore")
        End If

        dr.Close()
        conn.Close()

        Return temp_iva
    End Function

    Public Sub ImpostaTemplate()
        Me.Page.Title = Me.Session("AziendaNome")

        Dim objcss As New HtmlLink()
        Dim obj3 As New HtmlLink()
        objcss.Href = "~/public/style/" & Session("css")
        objcss.Attributes.Add("rel", "stylesheet")
        objcss.Attributes.Add("type", "text/css")

        'Credits relativo all'Azienda
        'Me.lblCredits.Text = Me.Session("Credits")

        obj3.Attributes.Add("rel", "shortcut icon")
        If (Session.Item("AziendaID") = 1) Then
            obj3.Href = "entropic.ico"
        Else
            obj3.Href = "webaffare.ico"
        End If

        Me.Page.Header.Controls.Add(objcss)
        Me.Page.Header.Controls.Add(obj3)
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        LeggiAzienda()

        'Setto i Partners dell'Azienda
        Sql_Partners.SelectCommand = "SELECT partners.*, Ordinamento AS Expr1 FROM partners WHERE (AziendaId=@AziendeId) ORDER BY Expr1"
        Sql_Partners.SelectParameters.Clear()
        Sql_Partners.SelectParameters.Add("@AziendeId", Session("AziendaID"))
    End Sub

    Protected Sub DataList_DIV1_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList_DIV1.Init
        If IsNothing(Me.Session("AziendaID")) Then
            LeggiAzienda()
        End If

        SqlDataFooterDIV1.SelectCommand = "SELECT id, AziendeId, nome, descrizione, img, link_esterno, target, div_position, ordinamento, abilitato FROM pagine WHERE (Abilitato = 1) AND (AziendeId=@AziendeId) AND (div_position=1) AND ((Tipo=1) OR (Tipo=4) OR (Tipo=6) OR (Tipo=7)) ORDER BY ordinamento_coupon ASC, Nome ASC"
        SqlDataFooterDIV1.SelectParameters.Clear()
        SqlDataFooterDIV1.SelectParameters.Add("@AziendeId", Session("AziendaID"))
    End Sub

    Protected Sub DataList_DIV2_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList_DIV2.Init
        If IsNothing(Me.Session("AziendaID")) Then
            LeggiAzienda()
        End If

        SqlDataFooterDIV2.SelectCommand = "SELECT id, AziendeId, nome, descrizione, img, link_esterno, target, div_position, ordinamento, abilitato FROM pagine WHERE (Abilitato = 1) AND (AziendeId=@AziendeId) AND (div_position=2) AND ((Tipo=1) OR (Tipo=4) OR (Tipo=6) OR (Tipo=7)) ORDER BY ordinamento_coupon ASC, Nome ASC"
        SqlDataFooterDIV2.SelectParameters.Clear()
        SqlDataFooterDIV2.SelectParameters.Add("@AziendeId", Session("AziendaID"))
    End Sub

    Protected Sub DataList_DIV3_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList_DIV3.Init
        If IsNothing(Me.Session("AziendaID")) Then
            LeggiAzienda()
        End If

        SqlDataFooterDIV3.SelectCommand = "SELECT id, AziendeId, nome, descrizione, img, link_esterno, target, div_position, ordinamento, abilitato FROM pagine WHERE (Abilitato = 1) AND (AziendeId=@AziendeId) AND (div_position=3) AND ((Tipo=1) OR (Tipo=4) OR (Tipo=6) OR (Tipo=7)) ORDER BY ordinamento_coupon ASC, Nome ASC"
        SqlDataFooterDIV3.SelectParameters.Clear()
        SqlDataFooterDIV3.SelectParameters.Add("@AziendeId", Session("AziendaID"))
    End Sub

    Protected Sub DataList_DIV4_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList_DIV4.Init
        If IsNothing(Me.Session("AziendaID")) Then
            LeggiAzienda()
        End If

        SqlDataFooterDIV4.SelectCommand = "SELECT id, AziendeId, nome, descrizione, img, link_esterno, target, div_position, ordinamento, abilitato FROM pagine WHERE (Abilitato = 1) AND (AziendeId=@AziendeId) AND (div_position=4) AND ((Tipo=1) OR (Tipo=4) OR (Tipo=6) OR (Tipo=7)) ORDER BY ordinamento_coupon ASC, Nome ASC"
        SqlDataFooterDIV4.SelectParameters.Clear()
        SqlDataFooterDIV4.SelectParameters.Add("@AziendeId", Session("AziendaID"))
    End Sub
End Class

