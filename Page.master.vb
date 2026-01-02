Imports MySql.Data.MySqlClient
Imports System.Data

Partial Class Page
    Inherits System.Web.UI.MasterPage

    Dim IvaTipo As Integer
    Dim conn As New MySqlConnection
    Dim conn2 As New MySqlConnection
    Dim cmd As New MySqlCommand
	public Dim social_buttons As New Dictionary(of String, String)
	public Dim social_buttons_rules As New Dictionary(of String, String)


    Function sostituisci_caratteri_speciali(ByRef stringa As String) As String
        stringa = Server.HtmlDecode(stringa)
        stringa = stringa.Replace("'", "''")
        Return stringa
    End Function

	Sub Load_social_buttons()
		Dim dr As MySqlDataReader
        Dim aziendaId As Integer = Me.Session("AziendaID")
        conn.Open()
		cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT * FROM social_buttons WHERE company_id = " & aziendaId & " and enabled = 1 order by button_order"
        dr = cmd.ExecuteReader()

        While dr.Read()
            social_buttons.add(dr.Item("key"),dr.Item("value"))
		End While
		dr.Close()

        cmd.CommandText = "SELECT * FROM social_buttons_rules where company_id = " & aziendaId

        dr = cmd.ExecuteReader()
		dr.Read()
		social_buttons_rules.add("enabled",dr.Item("enabled"))
		social_buttons_rules.add("callToAction",dr.Item("callToAction"))
		social_buttons_rules.add("buttonColor",dr.Item("buttonColor"))
		social_buttons_rules.add("position",dr.Item("position"))
		dr.Close()
		conn.Close()
		
	End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn2.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn
        LeggiAzienda()
        SettaCatalogo()
        If Me.Session.Item("ListaSettori") Is Nothing Then
            Me.Session.Item("ListaSettori") = Settore.creaListaSettori()
			'Page.ClientScript.RegisterStartupScript(Me.GetType(), "AlertScript", "alert('"& Me.Session.Item("ListaSettori")(2).categorie(0).sottoLivelli.Count &"');", True)
        End If

        Dim aziendaId As Integer = Me.Session("AziendaID")
        Dim dr As MySqlDataReader

        'MiPiace di Facebook
        If (Request.Cookies("FacebookLike") Is Nothing) And (Session("facebookLink").ToString.Trim.Length > 0) Then
            PopUpfacebook.Visible = True
        Else
            PopUpfacebook.Visible = False
        End If


        'Popup 
        Dim DataOdierna As String = Date.Today.Year.ToString & "-" & Date.Today.Month.ToString & "-" & Date.Today.Day.ToString
        If (Me.Session.Item("Popup") = 1) Then
            conn = New MySqlConnection()
            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            cmd.Connection = conn
            conn.Open()

            cmd.CommandType = CommandType.Text
            cmd.CommandText = "SELECT Azienda, Data_Inizio, Data_Fine, Messaggio, Abilitato FROM popup WHERE (Azienda=" & aziendaId & ") AND ((Data_Inizio<='" & DataOdierna & "') AND (Data_Fine>='" & DataOdierna & "')) AND (Abilitato=1)"

            'Assegno il comnado SELECT al DataSource
            Me.SqlData_Popup.SelectCommand = cmd.CommandText

            dr = cmd.ExecuteReader()
            dr.Read()

            If Not dr.HasRows Then
                Me.Session.Item("Popup") = 0
            Else
                Me.Session.Item("Popup") = 1
            End If

            dr.Close()
            conn.Close()
        End If

        'Backgroumd
        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT * FROM sfondi WHERE (aziendaid=" & aziendaId & ") AND ((data_inizio<='" & DataOdierna & "') AND (data_fine>='" & DataOdierna & "')) AND (abilitato=1)"
        dr = cmd.ExecuteReader()
        dr.Read()

        'Default
        Dim background As String
        If dr.HasRows Then
			background = dr.Item("path")
		Else
			background = "Default" & Session("AziendaID") & ".png"
        End If

		PageBody.Style.Value = PageBody.Style.Value & "; background-image:url('public/Sfondi/" & background & "')"

        dr.Close()
        conn.Close()
		
		Load_social_buttons()
		
		cmd.Dispose()
    End Sub



    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Me.Request.Url.ToString.Contains("accessonegato.aspx") Then
            Session.Item("Pagina_visitata") = Me.Request.Url
        End If

        If ((Session.Item("Inserimento_User") <> "") And (Session.Item("Inserimento_Password") <> "")) Then
            Me.tbUsername.Text = Session.Item("Inserimento_User")
            Me.tbPassword.Text = Session.Item("Inserimento_Password")
            Session.Item("Inserimento_User") = ""
            Session.Item("Inserimento_Password") = ""
        End If

        If Me.tbUsername.Text <> "" And Me.tbUsername.Text <> "Username" And Me.tbPassword.Text <> "" Then
            Login(Me.tbUsername.Text.Replace("'", ""), Me.tbPassword.Text.Replace("'", ""))
        End If

        Me.tbPassword.readonly = True

        If Not Me.IsPostBack Then
            If Me.Session("LoginId") Is Nothing Then
                If Not IsNothing(Me.Request.Cookies(Me.Session("AziendaNome"))) Then
                    Me.tbUsername.Text = Me.Request.Cookies(Me.Session("AziendaNome"))("Username")
                    Try : Me.tbPassword.Text = Me.Request.Cookies(Me.Session("Password"))("Password") : Catch : Me.tbPassword.Text = "" : End Try
                End If
            End If
        End If

        'COMMENTATO
        If Not Me.Session("LoginId") Is Nothing And Not Me.Request.ServerVariables("script_name").Contains("cambiapassword.aspx") Then
            If System.DateTime.Compare(System.DateTime.Today, CDate(Me.Session("DataPassword")).AddMonths(Me.Session("ScadenzaPassword"))) = 1 Then
                Me.Response.Redirect("cambiapassword.aspx")
            End If
        End If

        'aggiorno la query relativa agli ordini effettuati sul sito
        SqlData_TotOrdini.SelectCommand = "SELECT COUNT(*) AS Conteggio FROM documenti WHERE (TipoDocumentiId = 4) AND (DataDocumento <= { d '" & Date.Now.Year & "-12-31' }) AND (DataDocumento >= { d '" & Date.Now.Year & "-01-01' })"

        'Setto i Partners dell'Azienda
        Dim aziendaId As Integer = Session("AziendaID")
        Sql_Partners.SelectCommand = "SELECT partners.*, Ordinamento AS Expr1 FROM partners WHERE (AziendaId=" & aziendaId & ") ORDER BY Expr1"
    End Sub

    'Prelevo l'ID iva default
    Function preleva_idiva_default() As Integer
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim risultato As Integer = 0

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn

        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT * FROM ivadefault WHERE NOW() BETWEEN Dal AND Al"

        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If dr.HasRows Then
            risultato = dr.Item("IdIva")
        Else
            risultato = -1
        End If

        dr.Close()
        conn.Close()

        Return risultato
        '----------------------------------------------------------
    End Function

    'Prelevo il valore dell'iva default
    Function preleva_valoreiva_default() As Integer
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim risultato As Integer = 0

        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn

        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT * FROM ivadefault INNER JOIN iva ON iva.`id`=ivadefault.`IvaId` WHERE NOW() BETWEEN Dal AND Al"

        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If dr.HasRows Then
            risultato = dr.Item("Valore")
        Else
            risultato = -1
        End If

        dr.Close()
        conn.Close()

        Return risultato
        '----------------------------------------------------------
    End Function

    'Banner Top accanto al logo azienda nella Page-Master (Posizione 1 nella tabella pubblicità)
    Sub pubblicita_top()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim aziendaId As Integer = Me.Session("AziendaID")
        Dim lastBanner As Integer = Application.Item("Last_Banner")
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn

        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>=" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND (id_posizione_banner=1) ORDER BY id ASC LIMIT 1"
        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If Not dr.HasRows Then
            Application.Set("Last_Banner", "0")
            Me.SqlDataSource_Pubblicita_Top.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>=" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND (id_posizione_banner=1) ORDER BY id ASC LIMIT 1"

            dr.Close()
            cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>=" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND (id_posizione_banner=1) ORDER BY id ASC LIMIT 1"
            dr = cmd.ExecuteReader()
            dr.Read()
            If dr.HasRows Then
                Application.Set("Last_Banner", dr.Item("id"))
            End If
            dr.Close()
        Else
            Me.SqlDataSource_Pubblicita_Top.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>=" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND (id_posizione_banner=1) ORDER BY id ASC LIMIT 1"
            Dim id_pubblicita As Integer
            id_pubblicita = dr.Item("id")
            dr.Close()

            cmd.CommandText = "UPDATE pubblicitaV2 SET numero_impressioni_attuale = numero_impressioni_attuale + 1 WHERE (id = ?idPubblicita)"
            cmd.Parameters.AddWithValue("?idPubblicita", id_pubblicita)
            cmd.ExecuteNonQuery()

            'Tengo traccia dell'ultima pubblicità visualizzata
            Application.Set("Last_Banner", id_pubblicita)
        End If

        conn.Close()
        '----------------------------------------------------------
    End Sub

    'Banner Sinistro nella Page-Master (Posizione 2 nella tabella pubblicità, Ordinamento 1)
    Sub pubblicita_sinistra_1()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim aziendaId As Integer = Me.Session("AziendaID")
        Dim lastBanner As Integer = Application.Item("Last_Banner_Posizione_Sinistra_1")
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn

        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=1)) ORDER BY id ASC LIMIT 1"
        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If Not dr.HasRows Then
            Application.Set("Last_Banner_Posizione_Sinistra_1", "0")
            Me.SqlDataSource_Pubblicita_Sinistra_1.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=1)) ORDER BY id ASC LIMIT 1"
            dr.Close()
            cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=1)) ORDER BY id ASC LIMIT 1"
            dr = cmd.ExecuteReader()
            dr.Read()
            If dr.HasRows Then
                Application.Set("Last_Banner_Posizione_Sinistra_1", dr.Item("id"))
            End If
            dr.Close()
        Else
            Me.SqlDataSource_Pubblicita_Sinistra_1.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=1)) ORDER BY id ASC LIMIT 1"
            Dim id_pubblicita As Integer
            id_pubblicita = dr.Item("id")
            dr.Close()

            cmd.CommandText = "UPDATE pubblicitaV2 SET numero_impressioni_attuale = numero_impressioni_attuale + 1 WHERE (id = ?idPubblicita)"
            cmd.Parameters.AddWithValue("?idPubblicita", id_pubblicita)
            cmd.ExecuteNonQuery()

            'Tengo traccia dell'ultima pubblicità visualizzata
            Application.Set("Last_Banner_Posizione_Sinistra_1", id_pubblicita)
        End If

        conn.Close()
        '----------------------------------------------------------
    End Sub

    'Banner Sinistro nella Page-Master (Posizione 2 nella tabella pubblicità, Ordinamento 2)
    Sub pubblicita_sinistra_2()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim aziendaId As Integer = Me.Session("AziendaID")
        Dim lastBanner As Integer = Application.Item("Last_Banner_Posizione_Sinistra_2")
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn

        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=2)) ORDER BY id ASC LIMIT 1"
        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If Not dr.HasRows Then
            Application.Set("Last_Banner_Posizione_Sinistra_2", "0")
            Me.SqlDataSource_Pubblicita_Sinistra_2.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=2)) ORDER BY id ASC LIMIT 1"
            dr.Close()
            cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=2)) ORDER BY id ASC LIMIT 1"
            dr = cmd.ExecuteReader()
            dr.Read()
            If dr.HasRows Then
                Application.Set("Last_Banner_Posizione_Sinistra_2", dr.Item("id"))
            End If
            dr.Close()
        Else
            Me.SqlDataSource_Pubblicita_Sinistra_2.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=2)) ORDER BY id ASC LIMIT 1"
            Dim id_pubblicita As Integer
            id_pubblicita = dr.Item("id")
            dr.Close()

            cmd.CommandText = "UPDATE pubblicitaV2 SET numero_impressioni_attuale = numero_impressioni_attuale + 1 WHERE (id = ?idPubblicita)"
            cmd.Parameters.AddWithValue("?idPubblicita", id_pubblicita)
            cmd.ExecuteNonQuery()

            'Tengo traccia dell'ultima pubblicità visualizzata
            Application.Set("Last_Banner_Posizione_Sinistra_2", id_pubblicita)
        End If

        conn.Close()
        '----------------------------------------------------------
    End Sub

    'Banner Sinistro nella Page-Master (Posizione 2 nella tabella pubblicità, Ordinamento 3)
    Sub pubblicita_sinistra_3()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim aziendaId As Integer = Me.Session("AziendaID")
        Dim lastBanner As Integer = Application.Item("Last_Banner_Posizione_Sinistra_3")
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn

        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=3)) ORDER BY id ASC LIMIT 1"
        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If Not dr.HasRows Then
            Application.Set("Last_Banner_Posizione_Sinistra_3", "0")
            Me.SqlDataSource_Pubblicita_Sinistra_3.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=3)) ORDER BY id ASC LIMIT 1"
            dr.Close()
            cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=3)) ORDER BY id ASC LIMIT 1"
            dr = cmd.ExecuteReader()
            dr.Read()
            If dr.HasRows Then
                Application.Set("Last_Banner_Posizione_Sinistra_3", dr.Item("id"))
            End If
            dr.Close()
        Else
            Me.SqlDataSource_Pubblicita_Sinistra_3.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=2) AND (ordinamento=3)) ORDER BY id ASC LIMIT 1"
            Dim id_pubblicita As Integer
            id_pubblicita = dr.Item("id")
            dr.Close()

            cmd.CommandText = "UPDATE pubblicitaV2 SET numero_impressioni_attuale = numero_impressioni_attuale + 1 WHERE (id = ?idPubblicita)"
            cmd.Parameters.AddWithValue("?idPubblicita", id_pubblicita)
            cmd.ExecuteNonQuery()

            'Tengo traccia dell'ultima pubblicità visualizzata
            Application.Set("Last_Banner_Posizione_Sinistra_3", id_pubblicita)
        End If

        conn.Close()
        '----------------------------------------------------------
    End Sub

    'Banner Destro nella Page-Master (Posizione 3 nella tabella pubblicità, Ordinamento 1)
    Sub pubblicita_destra_1()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim aziendaId As Integer = Me.Session("AziendaID")
        Dim lastBanner As Integer = Application.Item("Last_Banner_Posizione_Destra_1")
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn

        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=1)) ORDER BY id ASC LIMIT 1"
        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If Not dr.HasRows Then
            Application.Set("Last_Banner_Posizione_Destra_1", "0")
            Me.SqlDataSource_Pubblicita_Destra_1.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=1)) ORDER BY id ASC LIMIT 1"
            dr.Close()
            cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=1)) ORDER BY id ASC LIMIT 1"
            dr = cmd.ExecuteReader()
            dr.Read()
            If dr.HasRows Then
                Application.Set("Last_Banner_Posizione_Destra_1", dr.Item("id"))
            End If
            dr.Close()
        Else
            Me.SqlDataSource_Pubblicita_Destra_1.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=1)) ORDER BY id ASC LIMIT 1"
            Dim id_pubblicita As Integer
            id_pubblicita = dr.Item("id")
            dr.Close()

            cmd.CommandText = "UPDATE pubblicitaV2 SET numero_impressioni_attuale = numero_impressioni_attuale + 1 WHERE (id = ?idPubblicita)"
            cmd.Parameters.AddWithValue("?idPubblicita", id_pubblicita)
            cmd.ExecuteNonQuery()

            'Tengo traccia dell'ultima pubblicità visualizzata
            Application.Set("Last_Banner_Posizione_Destra_1", id_pubblicita)
        End If

        conn.Close()
        '----------------------------------------------------------
    End Sub

    'Banner Destro nella Page-Master (Posizione 3 nella tabella pubblicità, Ordinamento 2)
    Sub pubblicita_destra_2()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim aziendaId As Integer = Me.Session("AziendaID")
        Dim lastBanner As Integer = Application.Item("Last_Banner_Posizione_Destra_2")
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn

        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=2)) ORDER BY id ASC LIMIT 1"
        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If Not dr.HasRows Then
            Application.Set("Last_Banner_Posizione_Destra_2", "0")
            Me.SqlDataSource_Pubblicita_Destra_2.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=2)) ORDER BY id ASC LIMIT 1"
            dr.Close()
            cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=2)) ORDER BY id ASC LIMIT 1"
            dr = cmd.ExecuteReader()
            dr.Read()
            If dr.HasRows Then
                Application.Set("Last_Banner_Posizione_Destra_2", dr.Item("id"))
            End If
            dr.Close()
        Else
            Me.SqlDataSource_Pubblicita_Destra_2.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=2)) ORDER BY id ASC LIMIT 1"
            Dim id_pubblicita As Integer
            id_pubblicita = dr.Item("id")
            dr.Close()

            cmd.CommandText = "UPDATE pubblicitaV2 SET numero_impressioni_attuale = numero_impressioni_attuale + 1 WHERE (id = ?idPubblicita)"
            cmd.Parameters.AddWithValue("?idPubblicita", id_pubblicita)
            cmd.ExecuteNonQuery()

            'Tengo traccia dell'ultima pubblicità visualizzata
            Application.Set("Last_Banner_Posizione_Destra_2", id_pubblicita)
        End If

        conn.Close()
        '----------------------------------------------------------
    End Sub

    'Banner Destro nella Page-Master (Posizione 3 nella tabella pubblicità, Ordinamento 3)
    Sub pubblicita_destra_3()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim aziendaId As Integer = Me.Session("AziendaID")
        Dim lastBanner As Integer = Application.Item("Last_Banner_Posizione_Destra_3")
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        cmd.Connection = conn

        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=3)) ORDER BY id ASC LIMIT 1"
        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If Not dr.HasRows Then
            Application.Set("Last_Banner_Posizione_Destra_3", "0")
            Me.SqlDataSource_Pubblicita_Destra_3.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=3)) ORDER BY id ASC LIMIT 1"
            dr.Close()
            cmd.CommandText = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=3)) ORDER BY id ASC LIMIT 1"
            dr = cmd.ExecuteReader()
            dr.Read()
            If dr.HasRows Then
                Application.Set("Last_Banner_Posizione_Destra_3", dr.Item("id"))
            End If
            dr.Close()
        Else
            Me.SqlDataSource_Pubblicita_Destra_3.SelectCommand = "SELECT id, id_Azienda, data_inizio_pubblicazione, data_fine_pubblicazione, limite_click, limite_impressioni, id_posizione_banner, numero_click_attuale, numero_impressioni_attuale, link, img_path, titolo, descrizione, abilitato from pubblicitaV2 WHERE (id>" & lastBanner & ") AND ((DATEDIFF(CURDATE(),data_inizio_pubblicazione)>=0) AND (DATEDIFF(CURDATE(),data_fine_pubblicazione)<=0)) AND ((numero_click_attuale<=limite_click) OR (limite_click=-1)) AND ((numero_impressioni_attuale<=limite_impressioni) OR (limite_impressioni=-1)) AND (abilitato=1) AND (id_Azienda=" & aziendaId & ") AND ((id_posizione_banner=3) AND (ordinamento=3)) ORDER BY id ASC LIMIT 1"
            Dim id_pubblicita As Integer
            id_pubblicita = dr.Item("id")
            dr.Close()

            cmd.CommandText = "UPDATE pubblicitaV2 SET numero_impressioni_attuale = numero_impressioni_attuale + 1 WHERE (id = ?idPubblicita)"
            cmd.Parameters.AddWithValue("?idPubblicita", id_pubblicita)
            cmd.ExecuteNonQuery()

            'Tengo traccia dell'ultima pubblicità visualizzata
            Application.Set("Last_Banner_Posizione_Destra_3", id_pubblicita)
        End If

        conn.Close()
        '----------------------------------------------------------
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        'Richiamo tutte le pubblicità
        pubblicita_top()
        pubblicita_sinistra_1()
        pubblicita_sinistra_2()
        pubblicita_sinistra_3()
        pubblicita_destra_1()
        pubblicita_destra_2()
        pubblicita_destra_3()
        '----------------------------

        'Visualizzo o meno il link per i listin8i personalizzati
        If Session.Item("AbilitaListino") > 0 Then
            Me.Label4.Visible = True
            Me.HyperLink14.Visible = True
        Else
            Me.Label4.Visible = False
            Me.HyperLink14.Visible = False
        End If

        Dim InOfferta As Integer = Me.Session("InOfferta")

        If Not Me.Session("LoginId") Is Nothing Then
            Dim lbl As Label
            Me.mvLogin.ActiveViewIndex = 1
			
            lbl = Me.mvLogin.FindControl("lblUtente")
            lbl.Text = Session("LoginNomeCognome")
            lbl = Me.mvLogin.FindControl("lblAccesso")
			
            Try
                lbl.Text = Session("LoginUltimoAccesso")
            Catch
                lbl.Text = "Oggi"
            End Try
			Dim toPayString As String = get_documents_to_pay()
			if toPayString <> "0" then
				pay_your_orders.visible = true
				to_pay.InnerHtml = toPayString
			End if
			
        End If
        'Me.imgCerca.Focus()

        IvaTipo = Me.Session("IvaTipo")

        If InOfferta = 1 Then
            'Me.sdsCategorie.SelectCommand = "select * from vcategoriepromozioni WHERE Abilitato=?Abilitato AND SettoriId=?SettoriId AND Nlistino=?NListino ORDER BY Ordinamento, Descrizione"
            'Me.sdsCategorie.SelectCommand = "select * from vcategoriepromozioni WHERE Abilitato=?Abilitato AND Nlistino=?NListino ORDER BY Ordinamento, Descrizione"
        End If

        LeggiCarrello()

        ' Creo la vista PROMO una volta al giorno
        'Dim DataOdierna As Date = Date.Now
        'Dim DataULtimoAggiornamento As Date = CDate(Application.Item("DataAggiornamentoOfferte"))
        'If DataULtimoAggiornamento.Date < DataOdierna.Date Then
        'CreaVistaPromo()
        'Application.Set("DataAggiornamentoOfferte", Date.Now)
        'End If

        Meta()

    End Sub

    Public Sub SettaCatalogo()

        SettoreDefault()

        'Settore
        If IsNumeric(Me.Request.QueryString("st")) Then
            Me.Session("st") = Me.Request.QueryString("st")
            Me.Session("ct") = 30000
            Me.Session("tp") = 0
            Me.Session("gr") = 0
            Me.Session("sg") = 0
            Me.Session("mr") = 0
            Me.Session("pid") = 0
            Me.Session("q") = Nothing
        End If

        'Categoria
        If IsNumeric(Me.Request.QueryString("ct")) Then
            Me.Session("ct") = Me.Request.QueryString("ct")
            Me.Session("tp") = 0
            Me.Session("gr") = 0
            Me.Session("sg") = 0
            'Me.Session("mr") = 0
            Me.Session("pid") = 0
            Me.Session("q") = Nothing
            If Not Me.Page.IsPostBack Then
                Session("Articoli_PageIndex") = Nothing
            End If
        End If

        'Tipologia
        If IsNumeric(Me.Request.QueryString("tp")) Then
            'If Me.Session("tp") = Me.Request.QueryString("tp") And Not Me.IsPostBack Then
            'Me.Session("tp") = 0
            'Else
            Me.Session("tp") = Me.Request.QueryString("tp")
            'End If
            'Me.Session("mr") = 0
            'Me.Session("gr") = 0
            'Me.Session("sg") = 0
            Me.Session("pid") = 0
            If Not Me.Page.IsPostBack Then
                Session("Articoli_PageIndex") = Nothing
            End If
        End If

        'Gruppo
        If IsNumeric(Me.Request.QueryString("gr")) Then
            'If Me.Session("gr") = Me.Request.QueryString("gr") And Not Me.IsPostBack Then
            'Me.Session("gr") = 0
            'Else
            Me.Session("gr") = Me.Request.QueryString("gr")
            'End If
            'Me.Session("mr") = 0
            'Me.Session("tp") = 0
            'Me.Session("sg") = 0
            Me.Session("pid") = 0
            If Not Me.Page.IsPostBack Then
                Session("Articoli_PageIndex") = Nothing
            End If
        End If

        'Sottogruppo
        If IsNumeric(Me.Request.QueryString("sg")) Then
            'If Me.Session("sg") = Me.Request.QueryString("sg") And Not Me.IsPostBack Then
            'Me.Session("sg") = 0
            'Else
            Me.Session("sg") = Me.Request.QueryString("sg")
            'End If
            'Me.Session("mr") = 0
            'Me.Session("tp") = 0
            'Me.Session("gr") = 0
            Me.Session("pid") = 0
            If Not Me.Page.IsPostBack Then
                Session("Articoli_PageIndex") = Nothing
            End If
        End If

        'Marca
        If IsNumeric(Me.Request.QueryString("mr")) Then
            'If Me.Session("mr") = Me.Request.QueryString("mr") And Not Me.IsPostBack Then
            'Me.Session("mr") = 0
            'Else
            Me.Session("mr") = Me.Request.QueryString("mr")
            'End If
            'Me.Session("tp") = 0
            'Me.Session("gr") = 0
            'Me.Session("sg") = 0
            Me.Session("pid") = 0
            If Not Me.Page.IsPostBack Then
                Session("Articoli_PageIndex") = Nothing
            End If
        End If

        'Promo
        If IsNumeric(Me.Request.QueryString("pid")) Then
            Me.Session("pid") = Me.Request.QueryString("pid")
            Me.Session("mr") = 0
            'Me.Session("st") = 0
            Me.Session("ct") = 30000
            Me.Session("tp") = 0
            Me.Session("gr") = 0
            Me.Session("sg") = 0
            If Not Me.Page.IsPostBack Then
                Session("Articoli_PageIndex") = Nothing
            End If
        End If

        'Ricerca
		Me.Session("q") = Nothing
        If Me.Request.QueryString("q") <> "" Then

            Me.Session("q") = Me.Request.QueryString("q").Trim
            'Me.Session("q") = Server.UrlEncode(Me.Request.QueryString("q"))

            'I parametri non devono essere commentati nel caso della ricerca avanzata
            If Not IsNothing(Me.Request.UrlReferrer) Then
                If (Me.Request.UrlReferrer.AbsolutePath.ToString.IndexOf("search_complete") < 0) Then

                    'Me.Session("mr") = 0
                    'Me.Session("st") = 0
                    Me.Session("ct") = 30000
                    'Me.Session("tp") = 0
                    'Me.Session("gr") = 0
                    'Me.Session("sg") = 0
                    'Me.Session("pid") = 0
                    'Me.Session("InOfferta") = 0
                End If
            End If
            If Not Me.Page.IsPostBack Then
                Session("Articoli_PageIndex") = Nothing
            End If
        End If

    End Sub

    Public Sub LeggiAzienda()
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand


        If IsNothing(Me.Session("AziendaID")) Then
            Dim sDominio As String = Me.Request.Url.Host
            'If sDominio.Contains(".") Then
            '    Dim temp() As String = sDominio.Split(".")
            '    sDominio = temp(1)
            'End If
			
            conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            cmd.Connection = conn

            conn.Open()

            cmd.CommandType = CommandType.Text
            cmd.CommandText = "SELECT * FROM aziende LEFT JOIN pagine ON aziende.Id=Aziendeid WHERE url1 like '%" & sDominio & "%' or url2 like '%" & sDominio & "%' limit 0, 1"
            'cmd.Parameters.AddWithValue("?sDominio", sDominio)
            'cmd.CommandText = "Select * from aziende where url1='192.168.0.2' or url2='192.168.0.2' limit 0, 1"

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
                Me.Session("Abilita_Coupon") = dr.Item("Coupon")
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
                Me.Session("AbilitaBuoniScontiCarrello") = dr.Item("AbilitaBuoniScontiCarrello")
                Me.Session("TC") = dr.Item("TC")
				
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

    Public Sub CreaVistaPromo()
        Dim dr As MySqlDataReader
        Dim Data As Date = System.DateTime.Today
        Dim strSelect As String = "SELECT id "
        Dim strSelect2 As String
        Dim strFrom As String = "FROM articoli WHERE Abilitato=1 AND NoPromo=0 "
        Dim strWhere As String
        Dim strArticoli As String = ""
        Dim MarcheID As String
        Dim SettoriId As String
        Dim CategorieId As String
        Dim TipologieId As String
        Dim GruppiId As String
        Dim SottoGruppiId As String
        Dim ArticoliId As String

        conn.Open()

        cmd.CommandType = CommandType.Text

        cmd.CommandText = "Delete from voffertearticoli"
        cmd.ExecuteNonQuery()

        'cmd.CommandText = "SELECT * FROM vOfferteDettagli WHERE (Abilitato=1) AND (DataInizio<=?Data) AND (DataFine>=?Data) ORDER BY OfferteId, id"
        'cmd.Parameters.AddWithValue("?Data", Data)

        cmd.CommandText = "SELECT DISTINCT Id, MarcheID, SettoriId, CategorieId, TipologieId, GruppiId, SottoGruppiId, ArticoliId, OfferteId"
        cmd.CommandText &= ", Descrizione, Immagine, DataInizio, DataFine, DaListino, AListino, QntMinima, Multipli, Prezzo, Sconto "
        cmd.CommandText &= "FROM voffertedettagli "
        cmd.CommandText &= "WHERE ('" & Format(Date.Today, "yyyyMMdd") & "' between datainizio and datafine) and Abilitato=1 ORDER BY OfferteId, id"

        dr = cmd.ExecuteReader()

        Dim cmd2 As MySqlCommand = New MySqlCommand()
        Dim conn3 As MySqlConnection = New MySqlConnection
        conn3.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn3.Open()
        cmd2.Connection = conn3
        cmd2.CommandType = CommandType.Text

        While dr.Read()
            MarcheID = dr.Item("MarcheId").ToString
            SettoriId = dr.Item("SettoriId").ToString
            CategorieId = dr.Item("CategorieId").ToString
            TipologieId = dr.Item("TipologieId").ToString
            GruppiId = dr.Item("GruppiId").ToString
            SottoGruppiId = dr.Item("SottoGruppiId").ToString
            ArticoliId = dr.Item("ArticoliId").ToString

            strSelect2 = ""
            strWhere = ""

            strSelect2 = strSelect2 & ", " & dr.Item("OfferteId") & " as OfferteID "
            strSelect2 = strSelect2 & ", " & dr.Item("Id") & " as OfferteDettagliId "
            strSelect2 = strSelect2 & ", '" & sostituisci_caratteri_speciali(dr.Item("Descrizione")) & "' as OfferteDescrizione "
            strSelect2 = strSelect2 & ", '" & dr.Item("Immagine") & "' as OfferteImmagine "
            strSelect2 = strSelect2 & ", '" & Format(dr.Item("DataInizio"), "yyyyMMdd") & "' as OfferteDataInizio "
            strSelect2 = strSelect2 & ", '" & Format(dr.Item("DataFine"), "yyyyMMdd") & "' as OfferteDataFine "
            strSelect2 = strSelect2 & ", " & dr.Item("DaListino") & " as OfferteDaListino "
            strSelect2 = strSelect2 & ", " & dr.Item("AListino") & " as OfferteAListino "
            strSelect2 = strSelect2 & ", " & dr.Item("QntMinima") & " as OfferteQntMinima "
            strSelect2 = strSelect2 & ", " & dr.Item("Multipli") & " as OfferteMultipli "
            strSelect2 = strSelect2 & ", " & dr.Item("Prezzo").ToString.Replace(",", ".") & " as OffertePrezzo "
            strSelect2 = strSelect2 & ", " & dr.Item("Sconto").ToString.Replace(",", ".") & " as OfferteSconto "

            If MarcheID <> "" And MarcheID <> "0" Then
                strWhere = strWhere & " AND MarcheID=" & MarcheID
            End If
            If SettoriId <> "" And SettoriId <> "0" Then
                strWhere = strWhere & " AND SettoriId=" & SettoriId
            End If
            If CategorieId <> "" And CategorieId <> "0" Then
                strWhere = strWhere & " AND CategorieId=" & CategorieId
            End If
            If TipologieId <> "" And TipologieId <> "0" Then
                strWhere = strWhere & " AND TipologieId=" & TipologieId
            End If
            If GruppiId <> "" And GruppiId <> "0" Then
                strWhere = strWhere & " AND GruppiId=" & GruppiId
            End If
            If SottoGruppiId <> "" And SottoGruppiId <> "0" Then
                strWhere = strWhere & " AND SottoGruppiId=" & SottoGruppiId
            End If
            If ArticoliId <> "" And ArticoliId <> "0" Then
                strWhere = strWhere & " AND Id=" & ArticoliId
            End If

            cmd2.CommandText = "Insert into voffertearticoli (" & strSelect & strSelect2 & strFrom & strWhere & ")"
            cmd2.ExecuteNonQuery()

        End While

        conn3.Close()
        conn3.Dispose()

        dr.Close()
        dr.Dispose()

        cmd.Dispose()
        cmd2.Dispose()

        conn.Close()
        conn.Dispose()
    End Sub

    Public Sub ImpostaTemplate()
        Me.Page.Title = Me.Session("AziendaNome")
        Me.imgLogo.ImageUrl = Me.Session("AziendaLogo")
        Me.imgLogo.AlternateText = Me.Session("AziendaNome") & " - " & Me.Session("AziendaDescrizione")
		Me.imgLogoMobile.ImageUrl = Me.Session("AziendaLogo")
        Me.imgLogoMobile.AlternateText = Me.Session("AziendaNome") & " - " & Me.Session("AziendaDescrizione")
        Me.lblCredits.Text = Me.Session("Credits")

        Dim objcss As New HtmlLink()
        Dim obj2 As New HtmlLink()
        Dim obj3 As New HtmlLink()
        objcss.Href = "~/public/style/" & Session("css")
        objcss.Attributes.Add("rel", "stylesheet")
        objcss.Attributes.Add("type", "text/css")
        'If (Session.Item("AziendaID") = 1) Then
        'obj2.Href = "http://www.entropic.it/Public/Images/entropic.jpg"
        'Else
        'obj2.Href = "http://www.entropic.it/Public/Images/webaffare3.jpg"
        'End If

        'FavIcon per il sito visualizzato
        obj3.Attributes.Add("rel", "shortcut icon")
        obj3.Href = Session("IconaWeb")
        
        'obj2.Attributes.Add("rel", "image_src")
        'obj2.ID = "Immagine_Facebook"
        Me.Page.Header.Controls.Add(objcss)
        'Me.Page.Header.Controls.Add(obj2)
        Me.Page.Header.Controls.Add(obj3)
    End Sub

    Public Sub SettoreDefault()

        If IsNothing(Me.Session("st")) Then

            conn.Open()

            cmd.CommandType = CommandType.Text
            cmd.CommandText = "Select id from settori where predefinito = 1"

            Dim dr As MySqlDataReader = cmd.ExecuteReader()
            dr.Read()

            If dr.HasRows Then
                Me.Session("st") = dr.Item("Id")
            End If

            dr.Close()
            dr.Dispose()

            conn.Close()
            conn.Dispose()

            cmd.Dispose()

        End If

    End Sub

    Public Sub Login(ByVal user As String, ByVal pass As String)
        conn.Open()

        cmd.CommandType = CommandType.Text
        cmd.CommandText = "Select * from vlogin where AziendeID=?id and UPPER(Username)=?Username limit 0, 1"
        cmd.Parameters.AddWithValue("?id", Session("AziendaID"))
        cmd.Parameters.AddWithValue("?Username", user.ToUpper)
        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If dr.HasRows Then
            If dr.Item("Abilitato") <> 1 Then
                Me.lblLogin.Text = "Login non attivo!"
                Me.lblLogin.Focus()
                Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Login non attivo!')}</script>")
            ElseIf dr.Item("UtentiAbilitato") <> 1 Then
                Me.lblLogin.Text = "Utente non attivo!"
                Me.lblLogin.Focus()
                Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Utente non attivo!')}</script>")
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
                Me.lblLogin.Text = "Password Errata!"
                Me.tbPassword.Focus()
                Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Password Errata!')}</script>")
            End If
        Else
            Me.lblLogin.Text = "Username Errato!"
            Me.tbUsername.Focus()
            Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('Username Errato!')}</script>")
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

        conn.Open()
        conn2.Open()

        Dim cmd As New MySqlCommand
        cmd.Connection = conn2
        cmd.CommandType = CommandType.Text

        Dim comm As New MySqlCommand
        comm.Connection = conn
        comm.CommandType = CommandType.Text

        'Aggiorno ultimo accesso su Login
        comm.CommandText = "update login set ultimoaccesso= now() , UltimoIp=?UltimoIp, NumeroAccessi=NumeroAccessi+1 where id=?LoginID"
        comm.Parameters.AddWithValue("?UltimoIp", Me.Request.UserHostAddress)
        comm.Parameters.AddWithValue("?LoginID", Session("LoginID"))
        comm.ExecuteNonQuery()

        'Aggiorno LoginID sugli articoli del carrello
        comm.CommandText = "update carrello set LoginID=?LoginID, SessionId='' where SessionId=?sessionId"
        comm.Parameters.AddWithValue("?sessionId", Session.SessionID)
        comm.ExecuteNonQuery()

        'Pulisco SessionID e NListino sugli articoli del carrello
        'comm.CommandText = "update carrello set SessionId='', NListino=" & Session("Listino") & " where LoginID=" & Session("LoginID")
        comm.CommandText = "update carrello set SessionId='', NListino=1"
        comm.ExecuteNonQuery()

        'Controllo se ci sono già altri articoli, e aggiorno solo le quantità
        Dim ArticoloID As Integer

        Dim conn_controllo As New MySqlConnection
        conn_controllo.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn_controllo.Open()

        Dim cmd_controllo As New MySqlCommand
        cmd_controllo.Connection = conn_controllo
        cmd_controllo.CommandText = "SELECT * FROM carrello WHERE (LoginID=?LoginID)"
        cmd_controllo.Parameters.AddWithValue("?LoginID", Session("LoginID"))

        Dim controllo_articolo As Integer
        Dim dsdata As New DataSet

        Dim sqlAdp As New MySqlDataAdapter(cmd_controllo)
        sqlAdp.Fill(dsdata, "carrello")

        Dim ROW_temp As DataRow
        Dim i As Integer = 1
        Dim j As Integer
        For Each ROW As DataRow In dsdata.Tables(0).Rows
            controllo_articolo = 0
            ArticoloID = ROW("ArticoliId").ToString

            'Aggiorno il prezzo dell'articolo, con quello attuale
            cmd_controllo.CommandText = "UPDATE carrello SET Prezzo=(SELECT Prezzo FROM articoli_listini WHERE NListino=?listino AND ArticoliId=" & ROW("ArticoliId") & "), PrezzoIvato=(SELECT PrezzoIvato FROM articoli_listini WHERE NListino=?listino AND ArticoliId=" & ROW("ArticoliId") & ") WHERE id=" & ROW("id")
            cmd_controllo.Parameters.AddWithValue("?listino", Session("Listino"))
            cmd_controllo.ExecuteNonQuery()

            For j = i To dsdata.Tables(0).Rows.Count - 1
                ROW_temp = dsdata.Tables(0).Rows(j)
                If (ROW_temp("ArticoliId").ToString = ROW("ArticoliId").ToString) Then
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

	Public Function get_documents_to_pay() As String
        Dim LoginId As Integer = Me.Session("LoginId")
        Dim SessionID As String = Me.Session.SessionID

        conn.Open()

        cmd.CommandType = CommandType.Text

        If LoginId > 0 Then
			cmd.CommandText = "SELECT COUNT(*) FROM documenti INNER JOIN login ON documenti.utentiid = login.utentiid Left Join pagamentitipo on documenti.pagamentiTipoId = pagamentiTipo.id Where login.id = " & LoginId & " And documenti.TipoDocumentiId = 4 AND documenti.StatiId <> 0 AND documenti.StatiId <> 3 AND PAGATO = 0 AND ONLINE <> 0"
        End If

        Dim toPay As String = cmd.ExecuteScalar()
		
		cmd.Dispose()
        conn.Close()
        conn.Dispose()

		Return toPay
    End Function
	
    Public Sub LeggiCarrello()
        Dim LoginId As Integer = Me.Session("LoginId")
        Dim SessionID As String = Me.Session.SessionID

        conn.Open()

        cmd.CommandType = CommandType.Text

        If LoginId = 0 Then
            If IvaTipo = 1 Then
                cmd.CommandText = "SELECT Sum(Qnt) AS Quantita, Sum(qnt*prezzo) AS TotRiga  FROM carrello where SessionID=?sessionId"
                cmd.Parameters.AddWithValue("?sessionId", SessionID)
            ElseIf IvaTipo = 2 Then
                cmd.CommandText = "SELECT Sum(Qnt) AS Quantita, Sum(qnt*prezzoivato) AS TotRiga  FROM carrello where SessionID=?sessionId"
                cmd.Parameters.AddWithValue("?sessionId", SessionID)
            End If
        Else
            If IvaTipo = 1 Then
                cmd.CommandText = "SELECT Sum(Qnt) AS Quantita, Sum(qnt*prezzo) AS TotRiga  FROM carrello where LoginId=" & LoginId
            ElseIf IvaTipo = 2 Then
                cmd.CommandText = "SELECT Sum(Qnt) AS Quantita, Sum(qnt*prezzoivato) AS TotRiga  FROM carrello where LoginId=" & LoginId
            End If
        End If

        Dim dr As MySqlDataReader = cmd.ExecuteReader()
        dr.Read()

        If dr.HasRows Then
            If Not dr.Item("quantita").ToString = "" Then
                Me.lblCarrelloCount.Text = dr.Item("quantita").ToString
                Me.lblCarrelloTotale.Text = FormatNumber(dr.Item("totriga").ToString, 2)
            End If
        End If

        dr.Close()
        dr.Dispose()

        conn.Close()
        conn.Dispose()

        cmd.Dispose()
    End Sub

    Sub Meta()

        Dim description As String
        description = Me.Page.Title
        description = Regex.Replace(description, "<[^>]*>", "")

        If description.Length > 255 Then
            description = description.Substring(0, 255)
        End If

        Dim MetaDescription As HtmlMeta = New HtmlMeta
        MetaDescription.Name = "description"
        MetaDescription.Content = description
        Me.Page.Header.Controls.Add(MetaDescription)

        Dim keywords As String
        keywords = Me.Page.Title
        keywords = keywords.Replace(" ", ",")

        Dim MetaKeywords As HtmlMeta = New HtmlMeta
        MetaKeywords.Name = "keywords"
        MetaKeywords.Content = keywords
        Me.Page.Header.Controls.Add(MetaKeywords)

    End Sub

    Public Sub Cerca()
        If (Me.tbCerca.Text.Trim <> "") And (Me.tbCerca.Text <> Application.Item("Campo_Ricerca")) Then
            Me.Session("ct") = 30000
            Me.Session("tp") = 0
            Me.Session("gr") = 0
            Me.Session("sg") = 0
            Me.Session("mr") = 0
            Me.Response.Redirect("articoli.aspx?q=" & Me.tbCerca.Text)
        End If
    End Sub
	
    Protected Sub btEntra_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btEntra.Click
		
    End Sub
	
    Protected Sub tbCerca_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbCerca.TextChanged
		Cerca()
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
        cmd.CommandText = "SELECT vettori.*, iva.Valore FROM vettori LEFT JOIN iva ON vettori.iva=iva.id WHERE vettori.Predefinito=1 AND vettori.AziendeID=" & AziendaId

        dr = cmd.ExecuteReader()
        dr.Read()

        If dr.HasRows = True Then
            temp_iva = dr.Item("Valore")
        End If

        dr.Close()
        conn.Close()

        Return temp_iva
    End Function

    Protected Sub DataList_DIV1_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList_DIV1.Init
        If IsNothing(Me.Session("AziendaID")) Then
            LeggiAzienda()
        End If

        SqlDataFooterDIV1.SelectCommand = "SELECT id, AziendeId, nome, descrizione, img, link_esterno, target, div_position, ordinamento, abilitato FROM pagine WHERE (Abilitato = 1) AND (AziendeId=?id) AND (div_position=1) AND ((Tipo=1) OR (Tipo=3) OR (Tipo=5) OR (Tipo=7)) ORDER BY ordinamento_footer ASC, Nome ASC"
        SqlDataFooterDIV1.SelectParameters.Add("?id", Session("AziendaID"))
    End Sub

    Protected Sub DataList_DIV2_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList_DIV2.Init
        If IsNothing(Me.Session("AziendaID")) Then
            LeggiAzienda()
        End If

        SqlDataFooterDIV2.SelectCommand = "SELECT id, AziendeId, nome, descrizione, img, link_esterno, target, div_position, ordinamento, abilitato FROM pagine WHERE (Abilitato = 1) AND (AziendeId=?id) AND (div_position=2) AND ((Tipo=1) OR (Tipo=3) OR (Tipo=5) OR (Tipo=7)) ORDER BY ordinamento_footer ASC, Nome ASC"
        SqlDataFooterDIV2.SelectParameters.Add("?id", Session("AziendaID"))
    End Sub

    Protected Sub DataList_DIV3_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList_DIV3.Init
        If IsNothing(Me.Session("AziendaID")) Then
            LeggiAzienda()
        End If

        SqlDataFooterDIV3.SelectCommand = "SELECT id, AziendeId, nome, descrizione, img, link_esterno, target, div_position, ordinamento, abilitato FROM pagine WHERE (Abilitato = 1) AND (AziendeId=?id) AND (div_position=3) AND ((Tipo=1) OR (Tipo=3) OR (Tipo=5) OR (Tipo=7)) ORDER BY ordinamento_footer ASC, Nome ASC"
        SqlDataFooterDIV3.SelectParameters.Add("?id", Session("AziendaID"))
    End Sub

    Protected Sub DataList_DIV4_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList_DIV4.Init
        If IsNothing(Me.Session("AziendaID")) Then
            LeggiAzienda()
        End If

        SqlDataFooterDIV4.SelectCommand = "SELECT id, AziendeId, nome, descrizione, img, link_esterno, target, div_position, ordinamento, abilitato FROM pagine WHERE (Abilitato = 1) AND (AziendeId=?id) AND (div_position=4) AND ((Tipo=1) OR (Tipo=3) OR (Tipo=5) OR (Tipo=7)) ORDER BY ordinamento_footer ASC, Nome ASC"
        SqlDataFooterDIV4.SelectParameters.Add("?id", Session("AziendaID"))
    End Sub

    Protected Sub LB_CancelFacebook_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LB_CancelFacebook.Click
        Session("MiPiaceFacebook") = 1

        'Creo un Coockie per nascondere il mi piace per una settimana
        CreateCookiesLikeFacebook()

        Response.Redirect(Request.UrlReferrer.AbsoluteUri)
    End Sub

    Private Sub CreateCookiesLikeFacebook()
        If Request.Cookies("FacebookLike") Is Nothing Then
            Dim aCookie As New HttpCookie("FacebookLike")
            aCookie.Values("Nascondi") = 1
            aCookie.Expires = DateTime.Now.AddDays(7)
            Response.Cookies.Add(aCookie)
        Else
            Dim cookie As HttpCookie = HttpContext.Current.Request.Cookies("FacebookLike")
            cookie.Values("Nascondi") = 1
            cookie.Expires = DateTime.Now.AddDays(-30)
            Response.Cookies.Add(cookie)
        End If
    End Sub
End Class

