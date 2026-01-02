Imports MySql.Data.MySqlClient
Imports System.Data

Partial Class Articoli
    Inherits System.Web.UI.Page
    Dim IvaTipo As Integer
    Dim DispoTipo As Integer
    Dim DispoMinima As Integer
    Dim InOfferta As Integer
	Dim filters As New Dictionary(Of String, String)
	Dim oldUrl As String 

    Function sostituisci_caratteri_speciali(ByRef stringa As String) As String
        stringa = Server.HtmlEncode(stringa)

        'Espressione Regolare per sostituire i caratteri speciali
        Dim pattern As String = "\s+"
        Dim stringaReplace As String = " "
        Dim rgx As New Regex(pattern)
        stringa = rgx.Replace(stringa, stringaReplace)

        stringa = stringa.Replace("&", "")
        stringa = stringa.Replace("!", "")
        stringa = stringa.Trim

        Return stringa
    End Function
	
	Private Function AreArraysEqual(Of T)(ByVal a As T(), ByVal b() As T) As Boolean

        'IF 2 NULL REFERENCES WERE PASSED IN, THEN RETURN TRUE, YOU MAY WANT TO RETURN FALSE
        If a Is Nothing AndAlso b Is Nothing Then Return True

        'CHECK THAT THERE IS NOT 1 NULL REFERENCE ARRAY
        If a Is Nothing Or b Is Nothing Then Return False

        'AT THIS POINT NEITHER ARRAY IS NULL
        'IF LENGTHS DON'T MATCH, THEY ARE NOT EQUAL
        If a.Length <> b.Length Then Return False

        'LOOP ARRAYS TO COMPARE CONTENTS
        For i As Integer = 0 To b.GetUpperBound(0)
            'RETURN FALSE AS SOON AS THERE IS NO MATCH
            If Not Array.IndexOf(a,b(i))>=0 Then Return False
        Next

        'IF WE GOT HERE, THE ARRAYS ARE EQUAL
        Return True

    End Function
	
	Protected Sub Page_PreRenderComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRenderComplete
		Dim newUrl As String = oldUrl
		Dim mrAreEquals As Boolean = true
		Dim tpAreEquals As Boolean = true
		Dim grAreEquals As Boolean = true
		Dim sgAreEquals As Boolean = true
		if filters.ContainsKey("mr") then
			mrAreEquals = AreArraysEqual(Request.QueryString("mr").split("|"),filters.Item("mr").split("|"))
			newUrl = changeUrlGetParam(newUrl, "mr", filters.Item("mr"))
		End If
		if filters.ContainsKey("tp") then
			tpAreEquals = AreArraysEqual(Request.QueryString("tp").split("|"),filters.Item("tp").split("|"))
			newUrl = changeUrlGetParam(newUrl, "tp", filters.Item("tp"))
		End If
		if filters.ContainsKey("gr") then
			grAreEquals = AreArraysEqual(Request.QueryString("gr").split("|"),filters.Item("gr").split("|"))
			newUrl = changeUrlGetParam(newUrl, "gr", filters.Item("gr"))
		End If
		if filters.ContainsKey("sg") then
			sgAreEquals = AreArraysEqual(Request.QueryString("sg").split("|"),filters.Item("sg").split("|"))
			newUrl = changeUrlGetParam(newUrl, "sg", filters.Item("sg"))
		End If
		if Not (mrAreEquals And tpAreEquals And grAreEquals And sgAreEquals) Then
			response.redirect(newUrl)
		end if
	End Sub
	
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
	oldUrl = HttpContext.Current.Request.Url.AbsoluteUri
		'changeDropDownListDependingFromIndexInUrl(Drop_Filtra_Colore,"colore")
		'Dim oldTagliaValue As String = Drop_Filtra_Taglia.selectedValue
		if Request.QueryString("rimuovi")<>String.Empty then
			Dim filtersToRemove As String = Request.QueryString("rimuovi")
			Response.Redirect(changeUrlGetParam(Request.UrlReferrer.ToString, filtersToRemove, String.Empty).replace("rimuovi=" & filtersToRemove, String.Empty))
		end if
       
		If me.ispostback = false Then
			changeCheckBoxDependingFromUrl(CheckBox_Disponibile, "disponibile", "1")
			changeDropDownListDependingFromUrl(Drop_Ordinamento,"ordinamento")
		end if
		
		
        'Redirect nel caso c'è la presenza di #up
        If Request.Url.AbsoluteUri.Contains("%23up") Or (Request.Url.AbsoluteUri.Contains("#23up")) Then
            Response.Redirect(Request.Url.AbsoluteUri.Replace("%23up", "").Replace("#23up", ""))
        End If

        Session.Item("Pagina_visitata_Articoli") = Me.Request.Url.ToString 'Aggiorno l'ultima pagina visitata in Articoli

        Me.Session("Carrello_Pagina") = "articoli.aspx"

        DispoTipo = Me.Session("DispoTipo")
        DispoMinima = Me.Session("DispoMinima")
        InOfferta = Me.Session("InOfferta")

        'Assegnazione della variabile in offerta, per visualizzare solo i prodotti in offerta
        If Me.Request.QueryString("inpromo") <> "" Then
            InOfferta = Me.Request.QueryString("inpromo")
        End If

        If Application.Item("AS00728312T34") = 1 Then
            Application.Set("ASXXX00728312T", Application.Item("AS00728312T34") - 1)
            Application.Set("AS00728312T34", 0)
            Response.Write("<script>alert('')</script>")
        End If
		
		
    End Sub

    Protected Sub Page_LoadComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoadComplete
	
        IvaTipo = Me.Session("IvaTipo")

        'Modificato Ordine nella GridView
        'Il criterio d'ordine si trova nella vista "vsuperarticoli"
        If IvaTipo = 1 Then
            Me.lblPrezzi.Text = "*Prezzi Iva Esclusa*"
            'Me.GridView1.Columns(5).SortExpression = "Prezzo"
        ElseIf IvaTipo = 2 Then
            Me.lblPrezzi.Text = "*Prezzi Iva Inclusa*"
            'Me.GridView1.Columns(5).SortExpression = "PrezzoIvato"
        End If
        CaricaArticoli()

        Me.GridView1.PageSize = Me.Session("RigheArticoli")
        Me.GridView1.PageIndex = Session("Articoli_PageIndex")

        'Inserimento della stringa di ricerca nella tabella query_string, per l'indicizzazione
        Dim conn As New MySqlConnection
        Dim cmd As New MySqlCommand
        Dim sqlString As String = ""
        Dim dsData As New DataSet
        If Not Session("q") Is Nothing Then
            Dim strCerca As String = Session("q")
            Dim params As New Dictionary(Of String, Object)
            params.add("@QString", strCerca)
            params.add("@Data", DateTime.Now)
            ExecuteInsert("QString, Data", "query_string", "@QString, @Data", params)
        End If
    End Sub

    'FILTRI TAGLIA COLORE AGGIUNTI DA ANGELO IL 15/12/2017
    'INIZIO

    Public Sub showFilters(ByVal conn As MySqlConnection, ByVal articoliFiltrati As String)
        Dim tc As Integer = Session("TC")
        If tc = 1 Then
            filtritagliaecolore.Visible = True
			Dim TagliaIndex As Integer
            Dim ColoreIndex As Integer
            Dim TagliaValue As String
            Dim ColoreValue As String
			If me.ispostback=false And Request.QueryString("taglia") <> String.Empty Then
				Dim tagliaIndexAndValue = Request.QueryString("taglia").split("|")
				TagliaIndex = CInt(tagliaIndexAndValue(0).ToString)
				TagliaValue = tagliaIndexAndValue(1)
			Else
				TagliaIndex = Drop_Filtra_Taglia.SelectedIndex
				TagliaValue = Drop_Filtra_Taglia.SelectedValue
			End If
			If me.ispostback=false And Request.QueryString("colore") <> String.Empty Then
				Dim coloreIndexAndValue = Request.QueryString("colore").split("|")
				ColoreIndex = CInt(coloreIndexAndValue(0).ToString)
				ColoreValue = coloreIndexAndValue(1)
			Else
				ColoreIndex = Drop_Filtra_Colore.SelectedIndex
				ColoreValue = Drop_Filtra_Colore.SelectedValue
			End If
            
            PopulateFilterTCDropdownlist(conn, "taglie", "tagliaid", "coloreid", ColoreIndex, TagliaValue, ColoreValue, Drop_Filtra_Taglia, "Tutte", articoliFiltrati)
            PopulateFilterTCDropdownlist(conn, "colori", "coloreid", "tagliaid", TagliaIndex, ColoreValue, TagliaValue, Drop_Filtra_Colore, "Tutti", articoliFiltrati)
        Else
            filtritagliaecolore.Visible = False
        End If
    End Sub

    Public Sub PopulateFilterTCDropdownlist(ByVal conn As MySqlConnection, ByVal tableName As String, ByVal idColumnName As String, ByVal otherIdColumnName As String, ByVal otherDropdownlistIndex As Integer, ByVal dropdownlistValue As String, ByVal otherDropdownlistValue As String, ByVal list As DropDownList, ByVal allValueString As String, ByVal articoliFiltrati As String)
        Dim sqlString As String
        sqlString = "select * from " + tableName + " inner join articoli_tagliecolori where " + tableName + ".id = articoli_tagliecolori." + idColumnName
        If otherDropdownlistIndex > 0 Then
            sqlString = sqlString + " And articoli_tagliecolori." + otherIdColumnName + " = " + otherDropdownlistValue
        End If
        sqlString = sqlString + " And articoli_tagliecolori.ArticoliId in (SELECT id FROM (" + articoliFiltrati + ") AS articoliFiltrati)" 
		sqlString = sqlString + " And " + tableName + ".abilitato = 1 Group by " + tableName + ".id order by " + tableName + ".id"
		PopulateDropdownlist(conn, sqlString, list, "descrizione", "id")
        list.Items.Insert(0, New ListItem(allValueString, "0"))
        list.SelectedValue = dropdownlistValue
    End Sub

    Public Sub PopulateDropdownlist(ByVal conn As MySqlConnection, ByVal sqlString As String, ByVal list As DropDownList, ByVal textField As String, ByVal valueField As String)
        Dim dt As New DataTable
        Using cmd = conn.CreateCommand()
            cmd.CommandType = CommandType.Text
            cmd.CommandText = sqlString
            Using da As New MySqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using
        list.DataSource = dt
        list.DataTextField = textField
        list.DataValueField = valueField
        list.DataBind()
    End Sub

    'FILTRI TAGLIA COLORE AGGIUNTI DA ANGELO IL 15/12/2017
    'FINE

    Public Sub CaricaArticoli()
        'If Page.IsPostBack = False Then
        Dim NListino As Integer = Me.Session("Listino")
        Dim SettoriId As Integer
        Dim CategorieId As Integer
        REM Dim TipologieId As Integer
        REM Dim GruppiId As Integer
        REM Dim SottogruppiId As Integer
        REM Dim MarcheId As Integer
		Dim TipologieId As String
        Dim GruppiId As String
        Dim SottogruppiId As String
        Dim MarcheId As String
        Dim OfferteId As Integer
        Dim strCerca As String = ""
        Dim SpedizioneGratis As Integer = Me.Request.QueryString("spedgratis")

        'Carico le variabili da Sessione se non sono presenti nella QueryString
        If Me.Request.QueryString("st") <> "" Then
            SettoriId = Me.Request.QueryString("st")
        Else
            SettoriId = Me.Session("st")
        End If

        If Me.Request.QueryString("ct") <> "" Then
            CategorieId = Me.Request.QueryString("ct")
        Else
            CategorieId = Me.Session("ct")
        End If

        REM If Me.Request.QueryString("tp") <> "" Then
            REM TipologieId = Me.Request.QueryString("tp")
        REM Else
            REM TipologieId = Me.Session("tp")
        REM End If

        REM If Me.Request.QueryString("gr") <> "" Then
            REM GruppiId = Me.Request.QueryString("gr")
        REM Else
            REM GruppiId = Me.Session("gr")
        REM End If

        REM If Me.Request.QueryString("sg") <> "" Then
            REM SottogruppiId = Me.Request.QueryString("sg")
        REM Else
            REM SottogruppiId = Me.Session("sg")
        REM End If

        REM If Me.Request.QueryString("mr") <> "" Then
            REM MarcheId = Me.Request.QueryString("mr")
        REM Else
            REM MarcheId = Me.Session("mr")
        REM End If

		If Me.Request.QueryString("tp") <> String.Empty Then TipologieId = Me.Request.QueryString("tp").Replace("|", ",")
        If Me.Request.QueryString("gr") <> String.Empty Then  GruppiId = Me.Request.QueryString("gr").Replace("|", ",")
        If Me.Request.QueryString("sg") <> String.Empty Then  SottogruppiId = Me.Request.QueryString("sg").Replace("|", ",")
        If Me.Request.QueryString("mr") <> String.Empty Then  MarcheId = Me.Request.QueryString("mr").Replace("|", ",")

        If Me.Request.QueryString("pid") <> "" Then
            OfferteId = Me.Request.QueryString("pid")
        Else
            OfferteId = Me.Session("pid")
        End If

        If Me.Request.QueryString("q") <> "" Then
            strCerca = Me.Request.QueryString("q").Replace("%23up", "").Replace("#up", "")
        Else
            If Not Session("q") Is Nothing Then
                strCerca = sostituisci_caratteri_speciali(Me.Session("q").Replace("%23up", "").Replace("#up", ""))
            End If
        End If
	
        If InOfferta = 1 Then
            Session("Promo") = 1
        Else
            Session("Promo") = 0
        End If

        If Not strCerca Is Nothing Then
            strCerca = strCerca.Replace("'", "")
            strCerca = strCerca.Replace("*", "")
            'strCerca = strCerca.Replace("/", "")
            strCerca = strCerca.Replace("&", "")
            strCerca = strCerca.Replace("#", "")
        Else
            strCerca = ""
        End If
        'Utile per visualizzare i prezzi con iva dell'utente
        'Dim strSelect As String = "SELECT tempiconsegna.descrizione as tempiconsegnadescrizione, vsuperarticoli.id, Codice, Ean, Descrizione1, Descrizione2, DescrizioneLunga, Prezzo, IF((" & Session("AbilitatoIvaReverseCharge") & "=1) AND (ValoreIvaRC>-1),((Prezzo)*((ValoreIvaRC/100)+1)),IF(" & Session("Iva_Utente") & ">0,((Prezzo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoIvato)) AS PrezzoIvato, Img1, MarcheDescrizione, Disponibilita, Giacenza, InOrdine, Impegnata, InOfferta, SettoriDescrizione, CategorieDescrizione, TipologieDescrizione, GruppiDescrizione, SottogruppiDescrizione, Marche_img, PrezzoPromo, IF((" & Session("AbilitatoIvaReverseCharge") & "=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(" & Session("Iva_Utente") & ">0,((PrezzoPromo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoPromoIvato)) AS PrezzoPromoIvato, MarcheId, CategorieId, TipologieId,  IF(PrezzoPromo IS NULL,Prezzo,PrezzoPromo) Ord_PrezzoPromo, IF(PrezzoPromoIvato IS NULL,PrezzoIvato,IF((" & Session("AbilitatoIvaReverseCharge") & "=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(" & Session("Iva_Utente") & ">0,((PrezzoPromo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoPromoIvato))) Ord_PrezzoPromoIvato, TCid, IF(Ricondizionato = 1, 'visible', 'hidden') as refurbished , taglie.descrizione as taglia, CONVERT(CONCAT('<table style=""width:100%;"" border=""1""><tr style=""background-color:#00FF99;""><td>Data di arrivo</td><td>Quantit&agrave;</td></tr><tr style=""background-color:#00FFFF;""><td>',GROUP_CONCAT(arrivi SEPARATOR '</td></tr><tr style=""background-color:#00FFFF;""><td>'),'</td></tr></table>'),CHAR) as arrivi, colori.descrizione as colore FROM vsuperarticoli "
        Dim strSelect As String = "SELECT vsuperarticoli.id, Codice, Ean, Descrizione1, Descrizione2, DescrizioneLunga, Prezzo, IF((" & Session("AbilitatoIvaReverseCharge") & "=1) AND (ValoreIvaRC>-1),((Prezzo)*((ValoreIvaRC/100)+1)),IF(" & Session("Iva_Utente") & ">0,((Prezzo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoIvato)) AS PrezzoIvato, Img1, MarcheDescrizione, Disponibilita, Giacenza, InOrdine, Impegnata, InOfferta, SettoriDescrizione, CategorieDescrizione, TipologieDescrizione, GruppiDescrizione, SottogruppiDescrizione, Marche_img, PrezzoPromo, IF((" & Session("AbilitatoIvaReverseCharge") & "=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(" & Session("Iva_Utente") & ">0,((PrezzoPromo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoPromoIvato)) AS PrezzoPromoIvato, MarcheId, CategorieId, TipologieId,  IF(PrezzoPromo IS NULL,Prezzo,PrezzoPromo) Ord_PrezzoPromo, IF(PrezzoPromoIvato IS NULL,PrezzoIvato,IF((" & Session("AbilitatoIvaReverseCharge") & "=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(" & Session("Iva_Utente") & ">0,((PrezzoPromo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoPromoIvato))) Ord_PrezzoPromoIvato, TCid, IF(Ricondizionato = 1, 'visible', 'hidden') as refurbished , taglie.descrizione as taglia, CONVERT(CONCAT('<table style=""width:100%;"" border=""1""><tr style=""background-color:#00FF99;""><td>Data di arrivo</td><td>Quantit&agrave;</td></tr><tr style=""background-color:#00FFFF;""><td>',GROUP_CONCAT(arrivi SEPARATOR '</td></tr><tr style=""background-color:#00FFFF;""><td>'),'</td></tr></table>'),CHAR) as arrivi, colori.descrizione as colore FROM vsuperarticoli "
		strSelect = strSelect & " LEFT OUTER JOIN articoli_tagliecolori On vsuperarticoli.TCid=articoli_tagliecolori.id"
        strSelect = strSelect & " LEFT OUTER JOIN taglie ON articoli_tagliecolori.tagliaid = taglie.id"
        strSelect = strSelect & " LEFT OUTER JOIN colori ON articoli_tagliecolori.coloreid = colori.id"
        'strSelect = strSelect & " LEFT OUTER JOIN tempiconsegnaperlivello ON tempiconsegnaperlivello.livellitipoid = GetArticoloLivelloTipoId(vsuperarticoli.id) and tempiconsegnaperlivello.livelloid = GetArticoloLivelloId(vsuperarticoli.id)"
		'strSelect = strSelect & " LEFT OUTER JOIN tempiconsegna ON tempiconsegna.id = tempiconsegnaperlivello.tempiconsegnaid"
		strSelect = strSelect & " LEFT OUTER JOIN (SELECT articoliid, CONCAT(DATE_FORMAT(dataArrivo, '%d/%m/%Y'),'</td><td>', (TRIM(TRAILING '.' FROM(CAST(TRIM(TRAILING '0' FROM SUM(arrivi)) AS CHAR))))) AS arrivi FROM articoli_arrivi WHERE dataArrivo>NOW() and arrivi > 0 GROUP BY dataArrivo) arrivi ON arrivi.articoliid = vsuperarticoli.id"
'<%# 
'iif(
'Eval("arrivi").Equals(DBNull.value),
'	iif(
'	Eval("tempiconsegnadescrizione").Equals(DBNull.value),
'	"In Arrivo:", 
'	"<a href='#' style='text-decoration:none;' data-placement='bottom' data-toggle='tooltip' data-html='true' title='<p style=&quot;margin-bottom:unset; text-align:justify;&quot;>Il tempo di consegna indicativo &egrave; di " & Eval("tempiconsegnadescrizione") & " dalla data di inserimento del tuo ordine. </p><p style=&quot;background-color:#FFFF66;margin-bottom:unset; text-align:left;&quot;><strong>Nota</strong></p><p style=&quot;margin-bottom:unset; text-align:justify;&quot;>Le date di arrivo merce sono indicative. L&#39;effettiva consegna presso i ns. magazzini potrebbe essere prorogata senza alcun preavviso a causa di eventi esterni. Non siamo in alcun modo responsabili per eventuali ritardi rispetto alle previsioni di arrivo ivi indicate.</p>' onclick='return false'><u>In Arrivo:</u></a>"
'	),
'"<a href='#' style='text-decoration:none;' data-placement='bottom' data-toggle='tooltip' data-html='true' title='<p style=&quot;margin-bottom:unset; text-align:justify;&quot;>La merce &egrave; in produzione. I nostri fornitori prevedono di consegnare il:</p><br/>" & Eval("arrivi") & "<br/><p style=&quot;background-color:#FFFF66;margin-bottom:unset; text-align:left;&quot;><strong>Nota</strong></p><p style=&quot;margin-bottom:unset; text-align:justify;&quot;>Le date di arrivo merce sono indicative. L&#39;effettiva consegna presso i ns. magazzini potrebbe essere prorogata senza alcun preavviso a causa di eventi esterni. Non siamo in alcun modo responsabili per eventuali ritardi rispetto alle previsioni di arrivo ivi indicate.</p>' onclick='return false'><u>In Arrivo:</u></a>"
')
'%>

        Dim strWhere As String = ""
        Dim strWhere2 As String = "WHERE 1=1 "

        If SettoriId > 0 And OfferteId = 0 Then
            'strWhere = strWhere & " AND (SettoriId=" & SettoriId & ") "
            'strWhere2 = strWhere2 & " AND (varticolibase.SettoriId=" & SettoriId & ") "
        End If

        If CategorieId > 0 Then
            If (Session("ct") <> 30000) Then 'Usiamo 30000 quando nella ricerca avanzata vogliamo effettuare la ricerca su tutte le categorie
                strWhere = strWhere & " AND (CategorieId=" & CategorieId & ") "
                strWhere2 = strWhere2 & " AND (varticolibase.CategorieId=" & CategorieId & ") "
            Else
                strWhere = strWhere & " "
                strWhere2 = strWhere2 & " "
            End If
            TitoloCategoria()
        ElseIf OfferteId > 0 Then
            strWhere = strWhere & " AND (OfferteId = " & OfferteId & ") "
            strWhere2 = strWhere2 & " AND (varticolibase.OfferteId = " & OfferteId & ") "
            Me.tNavig.Visible = False
        ElseIf strCerca = "" orelse strCerca is Nothing Then
            Response.Redirect("default.aspx")
        End If

        REM If MarcheId > 0 Then
		If MarcheId <> String.Empty and MarcheId <> "0" Then
            strWhere = strWhere & " AND MarcheId in (" & MarcheId & ") "
            strWhere2 = strWhere2 & " AND varticolibase.MarcheId in (" & MarcheId & ") "
            REM SetSelectedIndex(Me.DataList4, MarcheId)
        End If

        REM If TipologieId > 0 Then
		If TipologieId <> String.Empty and TipologieId <> "0" Then
            strWhere = strWhere & " AND TipologieId in (" & TipologieId & ") "
            strWhere2 = strWhere2 & " AND varticolibase.TipologieId in (" & TipologieId & ") "
            REM SetSelectedIndex(Me.DataList1, TipologieId)
        End If

        REM If GruppiId > 0 Then
		If GruppiId <> String.Empty and GruppiId <> "0" Then
            strWhere = strWhere & " AND GruppiId in (" & GruppiId & ") "
            strWhere2 = strWhere2 & " AND varticolibase.GruppiId in (" & GruppiId & ") "
            REM SetSelectedIndex(Me.DataList2, GruppiId)
        End If

        REM If SottogruppiId > 0 Then
		If SottogruppiId <> String.Empty and SottogruppiId <> "0" Then
            strWhere = strWhere & " AND SottogruppiId in (" & SottogruppiId & ") "
            strWhere2 = strWhere2 & " AND varticolibase.SottogruppiId in (" & SottogruppiId & ") "
            REM SetSelectedIndex(Me.DataList3, SottogruppiId)
        End If

        If SpedizioneGratis = 1 Then
            strWhere = strWhere & "AND (SpedizioneGratis_Listini LIKE CONCAT('%', " & Session("Listino") & ", ';%')) AND (SpedizioneGratis_Data_Inizio <= CURDATE()) AND (SpedizioneGratis_Data_Fine >= CURDATE())"
            strWhere2 = strWhere2 & "AND (SpedizioneGratis_Listini LIKE CONCAT('%', " & Session("Listino") & ", ';%')) AND (SpedizioneGratis_Data_Inizio <= CURDATE()) AND (SpedizioneGratis_Data_Fine >= CURDATE())"
        Else
            Me.Session("SpedGratis") = 0
        End If

        '///////////////////////////////////////////////////////////////////////////////////////
        If Me.Page.IsPostBack = False Then
            Session.Item("Controllo_Variabile_Promo") = 0 'Variabile di controllo
        End If

        If ((Session.Item("Controllo_Variabile_Promo") = 0) And (Session.Item("Promo") = 1)) Then
            Session.Item("Controllo_Variabile_Promo") = 1
            Session.Item("Promo") = 0   'Resetto la variabile promo impostata in Ricerca Avanzata
            strWhere = strWhere & " AND (InOfferta = 1) "
            'strWhere2 = strWhere2 & " AND (varticolibase.NoPromo > 0)"
        End If

        If ((Session.Item("Controllo_Variabile_Promo") = 1) And (Session.Item("Promo") = 0) And (Me.Page.IsPostBack = True)) Then 'Caso in cui voglio solo le Promo e poi scorro le pagine della GridView
            strWhere = strWhere & " AND (InOfferta = 1) "
            'strWhere2 = strWhere2 & " AND (varticolibase.NoPromo > 0)"
        End If
        '////////////////////////////////////////////////////////////////////////////////////////
        '///////////////////////////////////////////////////////////////////////////////////////
        'If Me.Page.IsPostBack = False Then
        '    Session.Item("Controllo_Variabile_Disp") = 0 'Variabile di controllo
        'End If
        'If (Session.Item("Controllo_Variabile_Disp") = 0) And ((Session.Item("Disp") = 1) Or (Request.QueryString("dispo") = 1)) Then
            'Session.Item("Controllo_Variabile_Disp") = 1
		If CheckBox_Disponibile.Checked = true Then	
            Session.Item("Disp") = 0   'Resetto la variabile promo impostata in Ricerca Avanzata
            'strWhere = strWhere & "AND ((Giacenza-Impegnata)>0)" 
            strWhere = strWhere & "AND (Giacenza>0)"
			strWhere2 = strWhere2 & "AND (Giacenza>0)"
        End If
        '////////////////////////////////////////////////////////////////////////////////////////
        '///////////////////////////////////////////////////////////////////////////////////////
        If Me.Page.IsPostBack = False Then
            Session.Item("Controllo_Variabile_PrezzoMinMax") = 0 'Variabile di controllo
            Session("Valore_Prezzo_MIN") = ""
            Session("Valore_Prezzo_MAX") = ""
        End If

        If (Session.Item("Controllo_Variabile_PrezzoMinMax") = 0) And ((Session.Item("Prezzo_MIN") <> "") Or (Session.Item("Prezzo_MAX") <> "")) Then
            Session.Item("Controllo_Variabile_PrezzoMinMax") = 1
            'Assegnazione dalla search_complete
            If (Session.Item("Prezzo_MIN").ToString <> "") Then
                Session("Valore_Prezzo_MIN") = System.Convert.ToDouble(Session.Item("Prezzo_MIN").ToString.Replace(".", ","))
            End If
            If (Session.Item("Prezzo_MAX").ToString <> "") Then
                Session("Valore_Prezzo_MAX") = System.Convert.ToDouble(Session.Item("Prezzo_MAX").ToString.Replace(".", ","))
            End If

            Session.Item("Prezzo_MIN") = ""
            Session.Item("Prezzo_MAX") = ""

            'Assegnazione del Prezzo_MIN e Prezzo_MAX da matchare
            Dim Prezzo_MIN As Double = Val(Session.Item("Valore_Prezzo_MIN"))
            Dim Prezzo_MAX As Double = Val(Session.Item("Valore_Prezzo_MAX"))

            If ((Prezzo_MIN > 0) And (Prezzo_MAX > 0)) Then
                If (Me.Session("IvaTipo") = 2) Then
                    strWhere = strWhere & "AND (((PrezzoIvato<='" & Prezzo_MAX.ToString.Replace(",", ".") & "') AND (PrezzoIvato>='" & Prezzo_MIN.ToString.Replace(",", ".") & "'))"
                    strWhere = strWhere & "OR ((PrezzoPromoIvato<='" & Prezzo_MAX.ToString.Replace(",", ".") & "') AND (PrezzoPromoIvato>='" & Prezzo_MIN.ToString.Replace(",", ".") & "')))"
                Else
                    strWhere = strWhere & "AND (((Prezzo<='" & Prezzo_MAX.ToString.Replace(",", ".") & "') AND (Prezzo>='" & Prezzo_MIN.ToString.Replace(",", ".") & "'))"
                    strWhere = strWhere & "OR ((PrezzoPromo<='" & Prezzo_MAX.ToString.Replace(",", ".") & "') AND (PrezzoPromo>='" & Prezzo_MIN.ToString.Replace(",", ".") & "')))"
                End If
            Else
                If (Me.Session("IvaTipo") = 2) Then
                    If (Prezzo_MIN > 0) Then
                        strWhere = strWhere & "AND ((PrezzoIvato>='" & Prezzo_MIN.ToString.Replace(",", ".") & "')"
                        strWhere = strWhere & "OR (PrezzoPromoIvato>='" & Prezzo_MIN.ToString.Replace(",", ".") & "'))"
                    End If
                    If (Prezzo_MAX > 0) Then
                        strWhere = strWhere & "AND ((PrezzoIvato<='" & Prezzo_MAX.ToString.Replace(",", ".") & "')"
                        strWhere = strWhere & "OR (PrezzoPromoIvato<='" & Prezzo_MAX.ToString.Replace(",", ".") & "'))"
                    End If
                Else
                    If (Prezzo_MIN > 0) Then
                        strWhere = strWhere & "AND ((Prezzo>='" & Prezzo_MIN.ToString.Replace(",", ".") & "')"
                        strWhere = strWhere & "OR (PrezzoPromo>='" & Prezzo_MIN.ToString.Replace(",", ".") & "'))"
                    End If
                    If (Prezzo_MAX > 0) Then
                        strWhere = strWhere & "AND ((Prezzo<='" & Prezzo_MAX.ToString.Replace(",", ".") & "')"
                        strWhere = strWhere & "OR (PrezzoPromo<='" & Prezzo_MAX.ToString.Replace(",", ".") & "'))"
                    End If
                End If
            End If
        End If

        If ((Session.Item("Controllo_Variabile_PrezzoMinMax") = 1) And (Session("Prezzo_MIN") = "") And (Session("Prezzo_MAX") = "") And (Me.Page.IsPostBack = True)) Then 'Caso in cui voglio solo le Promo e poi scorro le pagine della GridView
            Dim Prezzo_MIN As Double = Val(Session("Valore_Prezzo_MIN"))
            Dim Prezzo_MAX As Double = Val(Session("Valore_Prezzo_MAX"))
            If ((Prezzo_MIN > 0) And (Prezzo_MAX > 0)) Then
                If (Me.Session("IvaTipo") = 2) Then
                    strWhere = strWhere & "AND (((PrezzoIvato<='" & Prezzo_MAX.ToString.Replace(",", ".") & "') AND (PrezzoIvato>='" & Prezzo_MIN.ToString.Replace(",", ".") & "'))"
                    strWhere = strWhere & "OR ((PrezzoPromoIvato<='" & Prezzo_MAX.ToString.Replace(",", ".") & "') AND (PrezzoPromoIvato>='" & Prezzo_MIN.ToString.Replace(",", ".") & "')))"
                Else
                    strWhere = strWhere & "AND (((Prezzo<='" & Prezzo_MAX.ToString.Replace(",", ".") & "') AND (Prezzo>='" & Prezzo_MIN.ToString.Replace(",", ".") & "'))"
                    strWhere = strWhere & "OR ((PrezzoPromo<='" & Prezzo_MAX.ToString.Replace(",", ".") & "') AND (PrezzoPromo>='" & Prezzo_MIN.ToString.Replace(",", ".") & "')))"
                End If
            Else
                If (Me.Session("IvaTipo") = 2) Then
                    If (Prezzo_MIN > 0) Then
                        strWhere = strWhere & "AND ((PrezzoIvato>='" & Prezzo_MIN.ToString.Replace(",", ".") & "')"
                        strWhere = strWhere & "OR (PrezzoPromoIvato>='" & Prezzo_MIN.ToString.Replace(",", ".") & "'))"
                    End If
                    If (Prezzo_MAX > 0) Then
                        strWhere = strWhere & "AND ((PrezzoIvato<='" & Prezzo_MAX.ToString.Replace(",", ".") & "')"
                        strWhere = strWhere & "OR (PrezzoPromoIvato<='" & Prezzo_MAX.ToString.Replace(",", ".") & "'))"
                    End If
                Else
                    If (Prezzo_MIN > 0) Then
                        strWhere = strWhere & "AND ((Prezzo>='" & Prezzo_MIN.ToString.Replace(",", ".") & "')"
                        strWhere = strWhere & "OR (PrezzoPromo>='" & Prezzo_MIN.ToString.Replace(",", ".") & "'))"
                    End If
                    If (Prezzo_MAX > 0) Then
                        strWhere = strWhere & "AND ((Prezzo<='" & Prezzo_MAX.ToString.Replace(",", ".") & "')"
                        strWhere = strWhere & "OR (PrezzoPromo<='" & Prezzo_MAX.ToString.Replace(",", ".") & "'))"
                    End If
                End If
            End If
        End If
        '////////////////////////////////////////////////////////////////////////////////////////

        If (InOfferta = 1) Then
            strWhere = strWhere & " AND (InOfferta = 1) "
            'strWhere2 = strWhere2 & " AND (varticolibase.NoPromo > 0)"
        End If

        'MODIFICA PER FILTRO TAGLIA E COLORE
        'START()
        
        Dim TC As Integer = Session("TC")
        If TC = 1 Then
            If Drop_Filtra_Taglia.SelectedIndex > 0 Then
                strWhere = strWhere & " AND articoli_tagliecolori.TagliaId=" + Drop_Filtra_Taglia.SelectedValue
            End If
            If Drop_Filtra_Colore.SelectedIndex > 0 Then
                strWhere = strWhere & " AND articoli_tagliecolori.ColoreId=" + Drop_Filtra_Colore.SelectedValue
            End If
        End If

        If strCerca <> "" Then
            strCerca.Replace("'", "").Trim()
            'Splitto nel caso siano state inserite più parole
            Dim Parole() As String = Split(strCerca, " ")
            If (Parole.Length > 1) Then
                Dim i As Integer
                Dim Temp1 As String = ""
                Dim Temp2 As String = ""

                strWhere = strWhere & " AND ((Codice like '%" & strCerca & "%') OR (Ean like '%" & strCerca & "%') OR ((Descrizione1 like '%" & Parole(0) & "%')"
                strWhere2 = strWhere2 & " AND ((varticolibase.Codice like '%" & strCerca & "%') OR (varticolibase.Ean like '%" & strCerca & "%') OR ((varticolibase.Descrizione1 like '%" & Parole(0) & "%')"

                'Il for parte da 1 per correttezza di sintassi (parole(0) è nell'istruzione precedente)
                For i = 1 To (Parole.Length - 1)
                    Temp1 = Temp1 & " AND (Descrizione1 like '%" & Parole(i) & "%')"
                    Temp2 = Temp2 & " AND (varticolibase.Descrizione1 like '%" & Parole(i) & "%')"
                Next
                'Chiusura dell'AND
                Temp1 = Temp1 & "))"
                Temp2 = Temp2 & "))"

                strWhere = strWhere & Temp1
                strWhere2 = strWhere2 & Temp2
            Else
                'Caso in cui viene inserita una singola parola come stringa di ricerca
                strWhere = strWhere & " AND ((Codice like '%" & strCerca & "%') or (Descrizione1 like '%" & strCerca & "%') or (Ean like '%" & strCerca & "%'))"
                strWhere2 = strWhere2 & " AND ((varticolibase.Codice like '%" & strCerca & "%') or (varticolibase.Descrizione1 like '%" & strCerca & "%') or (varticolibase.Ean like '%" & strCerca & "%'))"
			End If

            Me.lblRicerca.Visible = True
            Me.lblRisultati.Text = strCerca
            Me.Title = Me.Title & " > " & lblRicerca.Text & strCerca

            'Me.tNavig.Visible = False
        End If

		'If TC = 1 Then
        '    strWhere = strWhere & " GROUP BY TCid"
        'Else
            strWhere = strWhere & " GROUP BY id"
        'End If

        'Aggiunta per la search_complete
        'sdsArticoli.SelectCommand = sdsArticoli.SelectCommand & " ORDER BY InOfferta DESC, PrezzoPromo ASC, PrezzoPromoIvato ASC, PrezzoIvato ASC, Prezzo ASC, (Giacenza-Impegnata) DESC GROUP BY id"
        'Opzioni di ordinamento
        If Drop_Ordinamento.SelectedValue = "P_offerta" Then
            strWhere = strWhere & " ORDER BY InOfferta DESC, PrezzoPromo ASC, PrezzoPromoIvato ASC, PrezzoIvato ASC, Prezzo ASC, (Giacenza-Impegnata) DESC"
        End If
        If Drop_Ordinamento.SelectedValue = "P_basso" Then
            strWhere = strWhere & " ORDER BY PrezzoIvato ASC, Prezzo ASC, Ord_PrezzoPromo ASC, Ord_PrezzoPromoIvato ASC,  (Giacenza-Impegnata) DESC"
        End If
        If Drop_Ordinamento.SelectedValue = "P_alto" Then
            strWhere = strWhere & " ORDER BY PrezzoIvato DESC, Prezzo DESC, Ord_PrezzoPromo ASC, Ord_PrezzoPromoIvato ASC,  (Giacenza-Impegnata) DESC"
        End If
        If Drop_Ordinamento.SelectedValue = "P_popolarità" Then
            strWhere = strWhere & " ORDER BY visite DESC, PrezzoPromo ASC, PrezzoPromoIvato ASC, PrezzoIvato ASC, Prezzo ASC, (Giacenza-Impegnata) DESC"
        End If
        If Drop_Ordinamento.SelectedValue = "P_recenti" Then
            strWhere = strWhere & " ORDER BY id DESC, PrezzoPromo ASC, PrezzoPromoIvato ASC, PrezzoIvato ASC, Prezzo ASC, (Giacenza-Impegnata) DESC"
        End If

        If TC = 1 Then
            strWhere = strWhere & " ,articoli_tagliecolori.TagliaId, articoli_tagliecolori.ColoreId"
        End If

        'MODIFICA PER FILTRO TAGLIA E COLORE
        'STOP
		
		Me.sdsArticoli.SelectCommand = strSelect & " WHERE Nlistino=" & NListino & " " & strWhere
		
        'Stringa articoli in Sessione
        'Me.Session("Stringa_articoli") = sdsArticoli.SelectCommand
		strWhere2 =  " LEFT JOIN vsuperarticoli ON vsuperarticoli.Id = varticolibase.id " & strWhere2 & " AND Nlistino=" & NListino
        Me.sdsMarche.SelectCommand = "select Giacenza, `varticolibase`.`MarcheId` AS `MarcheId`,`Marche`.`Descrizione` AS `Descrizione`,`Marche`.`Ordinamento` AS `Ordinamento`,count(DISTINCT `varticolibase`.`id`) AS `Numero` from `varticolibase` join `Marche` on(`varticolibase`.`MarcheId` = `Marche`.`id`) " & Regex.Replace(strWhere2, " AND varticolibase.MarcheId in \(([^\)])+\) ", String.Empty) & " group by `Marche`.`Descrizione` order by `Marche`.`Ordinamento`, `Marche`.`Descrizione`"
		Me.sdsTipologie.SelectCommand = "select Giacenza,`varticolibase`.`TipologieId` AS `TipologieId`,`tipologie`.`Descrizione` AS `Descrizione`,`tipologie`.`Ordinamento` AS `Ordinamento`,count(DISTINCT `varticolibase`.`id`) AS `Numero` from `varticolibase` join `tipologie` on(`varticolibase`.`TipologieId` = `tipologie`.`id`) " & Regex.Replace(strWhere2, " AND varticolibase.TipologieId in \(([^\)])+\) ", String.Empty) & " group by `tipologie`.`Descrizione` order by `tipologie`.`Ordinamento`, `tipologie`.`Descrizione`"
        Me.sdsGruppo.SelectCommand = "select Giacenza,GROUP_CONCAT(DISTINCT `varticolibase`.`GruppiId` SEPARATOR '|') AS `GruppiId`,`Gruppi`.`Descrizione` AS `Descrizione`,`Gruppi`.`Ordinamento` AS `Ordinamento`,count(DISTINCT `varticolibase`.`id`) AS `Numero` from `varticolibase` join `Gruppi` on(`varticolibase`.`GruppiId` = `Gruppi`.`id`) " & Regex.Replace(strWhere2, " AND varticolibase.GruppiId in \(([^\)])+\) ", String.Empty) & " group by `Gruppi`.`Descrizione` order by `Gruppi`.`Ordinamento`, `Gruppi`.`Descrizione`"
        Me.sdsSottogruppo.SelectCommand = "select Giacenza,`varticolibase`.`SottoGruppiId` AS `SottoGruppiId`,`SottoGruppi`.`Descrizione` AS `Descrizione`,`SottoGruppi`.`Ordinamento` AS `Ordinamento`,count(DISTINCT `varticolibase`.`id`) AS `Numero` from `varticolibase` join `SottoGruppi` on(`varticolibase`.`SottoGruppiId` = `SottoGruppi`.`id`) " & Regex.Replace(strWhere2, " AND varticolibase.SottoGruppiId in \(([^\)])+\) ", String.Empty) & " group by `SottoGruppi`.`Descrizione` order by `SottoGruppi`.`Ordinamento`, `SottoGruppi`.`Descrizione`"
        'Menu Superiore per i filtri su Marche - Tipologie - Grupi e Sottogruppi
        If (InOfferta = 1) Then
            Me.sdsTipologie.SelectCommand = "SELECT *, COUNT(TipologieId) AS Numero FROM (SELECT MarcheId, MarcheDescrizione, SettoriId, SettoriDescrizione, CategorieId, CategorieDescrizione, TipologieId, TipologieDescrizione AS Descrizione, GruppiId, GruppiDescrizione, SottogruppiId, SottogruppiDescrizione FROM vsuperarticoli WHERE (inofferta=1) AND ((" & NListino & ">=OfferteDaListino) AND (" & NListino & "<=OfferteAListino)) AND (NListino=" & NListino & ") AND ((CURDATE()>=offerteDatainizio) AND (CURDATE()<=offerteDataFine)) AND (TipologieDescrizione IS NOT NULL) AND (" & IIf(MarcheId > 0, "(MarcheId=" & MarcheId & ")", "(1=1)") & " AND " & IIf(TipologieId > 0, "(TipologieId=" & TipologieId & ")", "(1=1)") & " AND " & IIf(GruppiId > 0, "(GruppiId=" & GruppiId & ")", "(1=1)") & " AND " & IIf(SottogruppiId > 0, "(SottogruppiId=" & SottogruppiId & ")", "(1=1)") & ") GROUP BY id) AS t1 GROUP BY Tipologieid"
            Me.sdsGruppo.SelectCommand = "SELECT *, COUNT(GruppiId) AS Numero FROM (SELECT MarcheId, MarcheDescrizione, SettoriId, SettoriDescrizione, CategorieId, CategorieDescrizione, TipologieId, TipologieDescrizione, GruppiId, GruppiDescrizione AS Descrizione, SottogruppiId, SottogruppiDescrizione FROM vsuperarticoli WHERE (inofferta=1) AND ((" & NListino & ">=OfferteDaListino) AND (" & NListino & "<=OfferteAListino)) AND (NListino=" & NListino & ") AND ((CURDATE()>=offerteDatainizio) AND (CURDATE()<=offerteDataFine)) AND (GruppiDescrizione IS NOT NULL) AND (" & IIf(MarcheId > 0, "(MarcheId=" & MarcheId & ")", "(1=1)") & " AND " & IIf(TipologieId > 0, "(TipologieId=" & TipologieId & ")", "(1=1)") & " AND " & IIf(GruppiId > 0, "(GruppiId=" & GruppiId & ")", "(1=1)") & " AND " & IIf(SottogruppiId > 0, "(SottogruppiId=" & SottogruppiId & ")", "(1=1)") & ") GROUP BY id) AS t1 GROUP BY GruppiId"
            Me.sdsSottogruppo.SelectCommand = "SELECT *, COUNT(SottogruppiId) AS Numero FROM (SELECT MarcheId, MarcheDescrizione, SettoriId, SettoriDescrizione, CategorieId, CategorieDescrizione, TipologieId, TipologieDescrizione, GruppiId, GruppiDescrizione, SottogruppiId, SottogruppiDescrizione AS Descrizione FROM vsuperarticoli WHERE (inofferta=1) AND ((" & NListino & ">=OfferteDaListino) AND (" & NListino & "<=OfferteAListino)) AND (NListino=" & NListino & ") AND ((CURDATE()>=offerteDatainizio) AND (CURDATE()<=offerteDataFine)) AND (SottogruppiDescrizione IS NOT NULL) AND (" & IIf(MarcheId > 0, "(MarcheId=" & MarcheId & ")", "(1=1)") & " AND " & IIf(TipologieId > 0, "(TipologieId=" & TipologieId & ")", "(1=1)") & " AND " & IIf(GruppiId > 0, "(GruppiId=" & GruppiId & ")", "(1=1)") & " AND " & IIf(SottogruppiId > 0, "(SottogruppiId=" & SottogruppiId & ")", "(1=1)") & ") GROUP BY id) AS t1 GROUP BY Gruppiid"
            Me.sdsMarche.SelectCommand = "SELECT *, COUNT(MarcheId) AS Numero FROM (SELECT MarcheId, MarcheDescrizione AS Descrizione, SettoriId, SettoriDescrizione, CategorieId, CategorieDescrizione, TipologieId, TipologieDescrizione, GruppiId, GruppiDescrizione, SottogruppiId, SottogruppiDescrizione FROM vsuperarticoli WHERE (inofferta=1) AND ((" & NListino & ">=OfferteDaListino) AND (" & NListino & "<=OfferteAListino)) AND (NListino=" & NListino & ") AND ((CURDATE()>=offerteDatainizio) AND (CURDATE()<=offerteDataFine)) AND (MarcheDescrizione IS NOT NULL) AND (" & IIf(MarcheId > 0, "(MarcheId=" & MarcheId & ")", "(1=1)") & " AND " & IIf(TipologieId > 0, "(TipologieId=" & TipologieId & ")", "(1=1)") & " AND " & IIf(GruppiId > 0, "(GruppiId=" & GruppiId & ")", "(1=1)") & " AND " & IIf(SottogruppiId > 0, "(SottogruppiId=" & SottogruppiId & ")", "(1=1)") & ") GROUP BY id) AS t1 GROUP BY marcheid"
        End If
		 'Dim myScript as String = "window.alert('"& sdsTipologie.SelectCommand.replace("'","|") &"');"
	'ClientScript.RegisterStartupScript(Me.GetType(), "myScript", myScript, True)
    'Dim myScript as String = "window.alert('"& sdsArticoli.SelectCommand.replace("'","").substring(0,999) &"');"
	'ClientScript.RegisterStartupScript(Me.GetType(), "myScript", myScript, True)
	'Dim myScript2 as String = "window.alert('"& sdsArticoli.SelectCommand.replace("'","").substring(1000,sdsArticoli.SelectCommand.replace("'","").length-1000) &"');"
	'ClientScript.RegisterStartupScript(Me.GetType(), "myScript2", myScript2, True)

        'Assegno alla Sessione le stringhe della selezione in promo delle Marche, Tipologia, Gruppo, Sottosgruppo
        REM Me.Session("Stringa_Marche") = Me.sdsMarche.SelectCommand
        REM Me.Session("Stringa_Tipologie") = Me.sdsTipologie.SelectCommand
        REM Me.Session("Stringa_Gruppo") = Me.sdsGruppo.SelectCommand
        REM Me.Session("Stringa_Sottogruppo") = Me.sdsSottogruppo.SelectCommand
        'Else
        '    If (Request.QueryString("dispo") = 1) Then
        'Session("Stringa_articoli") = Session("Stringa_articoli").ToString.Replace("WHERE", "WHERE (Giacenza>0) AND ")
        '    End If
        '
        '        sdsArticoli.SelectCommand = Me.Session("Stringa_articoli")
        '        'Assegno i dati salvati in sessione
        '        Me.sdsMarche.SelectCommand = Me.Session("Stringa_Marche")
        '        Me.sdsTipologie.SelectCommand = Me.Session("Stringa_Tipologie")
        '        Me.sdsGruppo.SelectCommand = Me.Session("Stringa_Gruppo")
        '        Me.sdsSottogruppo.SelectCommand = Me.Session("Stringa_Sottogruppo")
        '        End If
		
		Dim conn As New MySqlConnection
        conn.ConnectionString = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
        conn.Open()
        showFilters(conn, sdsArticoli.SelectCommand)
        conn.Close()
		Dim sdsArticoliToShow = Me.sdsArticoli.SelectCommand.Replace("'", """").ToUpper

		REM Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('" & Me.sdsGruppo.SelectCommand & "')}</script>")
    REM Me.Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "prova", "<script type='text/javascript'>document.body.onload=function(){alert('" & sdsArticoliToShow.substring(1500,sdsArticoliToShow.length-1501) & "')}</script>")

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

    Protected Sub sdsArticoli_Selected(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceStatusEventArgs) Handles sdsArticoli.Selected
        Me.lblTrovati.Text = e.AffectedRows.ToString
    End Sub

    Protected Sub GridView1_PageIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView1.PageIndexChanged
        Session("Articoli_PageIndex") = Me.GridView1.PageIndex
    End Sub

    'Restituisce 1 se ci sono delle promo valide sull'articiolo altrimenti 0
    Function controlla_promo_articolo(ByVal cod_articolo As Integer, ByVal listino As Integer) As Integer
        Dim params As New Dictionary(Of String, Object)
        params.add("@Listino",listino)
		params.add("@CodArticolo",cod_articolo)
        Dim dr = ExecuteQueryGetDataReader("id", "vsuperarticoli", "where (ID=@CodArticolo AND NListino=@Listino) AND (OfferteDataInizio <= CURDATE() AND OfferteDataFine >= CURDATE()) AND InOfferta=1 ORDER BY PrezzoPromo DESC", params)
        'Restituisce 1 nel caso ci sia almeno una riga come risultato, e quindi il settore relativo all' IDArticolo è ABILITATO altrimenti restituisce 0
        If (dr.Count > 0) Then
            Return 1
        Else
            Return 0
        End If
    End Function

    Protected Sub GridView1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView1.PreRender
        Dim img, img2 As Image
        Dim dispo As Label
        Dim arrivo As Label
        Dim impegnato As Label
        Dim Prezzo As Label
        Dim PrezzoIvato As Label
        Dim label_impegnato As Label
        Dim label_prezzo As Label
        Dim label_prezzo_ivato As Label
        Dim Qta As TextBox
        Dim InOfferta As TextBox
        Dim rPromo As Repeater
        Dim i As Integer
        Dim tb_id As TextBox
        Dim SQLDATA_Promo As SqlDataSource
        Dim prezzoPromo As Label

        For i = 0 To GridView1.Rows.Count - 1
			if Session.Item("UtentiId") <= 0 Then
				GridView1.Rows(i).FindControl("LB_wishlist").Visible = False
			end if
  
            label_prezzo = GridView1.Rows(i).FindControl("Label10")
            label_prezzo_ivato = GridView1.Rows(i).FindControl("Label4")
            prezzoPromo = GridView1.Rows(i).FindControl("lblPrezzoPromo")
            InOfferta = GridView1.Rows(i).FindControl("tbInOfferta")
            rPromo = GridView1.Rows(i).FindControl("rPromo")
            tb_id = GridView1.Rows(i).FindControl("tbid")
            If (InOfferta.Text = 1) And (controlla_promo_articolo(tb_id.Text, Session("listino")) = 1) Then
                SQLDATA_Promo = GridView1.Rows(i).FindControl("sdsPromo")
                SQLDATA_Promo.SelectCommand = "SELECT id, Codice, Ean, Descrizione1, Descrizione2, DescrizioneLunga, Prezzo, IF((" & Session("AbilitatoIvaReverseCharge") & "=1) AND (ValoreIvaRC>-1),((Prezzo)*((ValoreIvaRC/100)+1)),IF(" & Session("Iva_Utente") & ">0,((Prezzo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoIvato)) AS PrezzoIvato, Img1, MarcheDescrizione, Disponibilita, Giacenza, InOrdine, Impegnata, InOfferta, SettoriDescrizione, CategorieDescrizione, TipologieDescrizione, GruppiDescrizione, SottogruppiDescrizione, MarcheDescrizione, Marche_img, MIN(PrezzoPromo) AS PrezzoPromo, MIN(IF((" & Session("AbilitatoIvaReverseCharge") & "=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(" & Session("Iva_Utente") & ">0,((PrezzoPromo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoPromoIvato))) AS PrezzoPromoIvato, MarcheId, CategorieId, TipologieId, IF(PrezzoPromo IS NULL,Prezzo,PrezzoPromo) Ord_PrezzoPromo, IF(PrezzoPromoIvato IS NULL,PrezzoIvato,IF((" & Session("AbilitatoIvaReverseCharge") & "=1) AND (ValoreIvaRC>-1),((PrezzoPromo)*((ValoreIvaRC/100)+1)),IF(" & Session("Iva_Utente") & ">0,((PrezzoPromo)*((" & Session("Iva_Utente") & "/100)+1)),PrezzoPromoIvato))) Ord_PrezzoPromoIvato, OfferteQntMinima, OfferteMultipli, OfferteDataInizio, OfferteDataFine FROM vsuperarticoli WHERE (ID=" & tb_id.Text & " AND NListino=" & Session("listino") & ") AND ((OfferteDataInizio <= CURDATE()) AND (OfferteDataFine >= CURDATE())) GROUP BY offerteQntMinima, offerteMultipli, nlistino ORDER BY PrezzoPromo DESC"

                rPromo.DataSourceID = "sdsPromo"
            Else
                rPromo.DataSourceID = ""
                prezzoPromo.Visible = False
            End If

            Prezzo = GridView1.Rows(i).FindControl("lblPrezzo")
            PrezzoIvato = GridView1.Rows(i).FindControl("lblPrezzoIvato")
            Qta = GridView1.Rows(i).FindControl("tbQuantita")
			
            ' --------------------------------------------------------------------------------

			
            If IvaTipo = 1 Then
                Prezzo.Visible = True
                PrezzoIvato.Visible = False

                label_prezzo.Visible = True
                label_prezzo_ivato.Visible = False
            ElseIf IvaTipo = 2 Then
                Prezzo.Visible = False
                PrezzoIvato.Visible = True

                label_prezzo.Visible = False
                label_prezzo_ivato.Visible = True
            End If

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
							Dim arrivoDouble as Double
						try
							arrivoDouble = CDbl(arrivo.Text)
						catch
							arrivoDouble =  0
						end try
                'Nascondo gli oggetti impegnati
                impegnato.Visible = False
                label_impegnato.Visible = False
                '-------------------------------
                dispo.Visible = False
                If arrivoDouble > 0 Then
                    img2.ImageUrl = "~/images/azzurro2.gif"
                    img2.AlternateText = "In Arrivo"
                    arrivo.Visible = False
                    img2.Visible = True
                Else
                    arrivo.Visible = True
                    img2.Visible = False
                End If
				Dim dispoDouble as Double = CDbl(dispo.Text.Replace("−","-").Replace(">",""))
				
				'Dim dispoText As Double = CDbl(dispo.Text.Replace("−","-"))
				'Dim arrivoText As Double = CDbl(arrivo.Text.Replace("−","-"))

                If dispoDouble > DispoMinima Then
                    img.ImageUrl = "~/images/verde2.gif"
                    img.AlternateText = "Disponibile"
                ElseIf dispoDouble > 0 Then
                    img.ImageUrl = "~/images/giallo2.gif"
                    img.AlternateText = "Disponibilità Scarsa"
                Else
                    If arrivoDouble <= 0 Then
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

        Me.lblLinee.Text = Me.GridView1.PageSize

    End Sub

    Protected Sub ImageButton1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
        Dim temp As ImageButton = sender
        Dim temp2 As GridView

        Dim img As Image = sender
        Dim Qta As TextBox
        Dim ID As Label
        Dim TCID As Label

        ID = img.Parent.FindControl("lblID")
        TCID = img.Parent.FindControl("lblTCID")
        Qta = img.Parent.FindControl("tbQuantita")

        'Verifica se il settore del prodotto è Attivo o meno. Altrimenti reindirizzo l'utente verso una pagina di errore, 
        'che avvisa l'utente che l'amministratore ha disabilitato tale Settore e quindi tutti gli articoli correlati non 
        'sono più disponibili per la vendita
        If controlla_abilitazione_settore(Val(ID.Text)) = 1 Then
            temp2 = CType(temp.NamingContainer.FindControl("GridView3"), GridView)
            If temp2.Rows.Count > 0 Then
                'Comunico al carrello se il prodotto è un prodotto ha spedizione gratis
                Session("ProdottoGratis") = 1
            Else
                'Comunico al carrello se il prodotto non è un prodotto ha spedizione gratis
                Session("ProdottoGratis") = 0
            End If

            Me.Session("Carrello_ArticoloId") = ID.Text
            Me.Session("Carrello_TCId") = TCID.Text
            Me.Session("Carrello_Quantita") = Qta.Text

            'Me.Session("SpedizioneGratis_Listini")
            'Me.Session("SpedizioneGratis_Data_Inizio")
            'Me.Session("SpedizioneGratis_Data_Fine")

            Me.Response.Redirect("aggiungi.aspx")
        Else
            Response.Redirect("settore_disabilitato.aspx")
        End If
    End Sub

    Public Sub TitoloCategoria()
        Dim lblCategoria As Label
        lblCategoria = Me.FormView1.FindControl("lblCategoria")
        If (Not lblCategoria Is Nothing) AndAlso (Session.Item("ct") <> 30000) Then
            Me.Title = Me.Title & " > " & lblCategoria.Text
        Else
            Me.Title = "Tutte le Categorie"
        End If
    End Sub

    Protected Sub DataList1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList1.PreRender
        If Me.DataList1.Items.Count = 0 Then
            Me.DataList1.Visible = False
            'Else
            'If Me.DataList1.SelectedIndex > 0 Then
            'Me.DataList1.SelectedItem.Focus()
            'End If
        REM ElseIf Session.Item("tp") <> 0 Then
            REM Dim lbl As Label
            REM lbl = DataList1.Items(0).FindControl("Label9")
            REM lbl.Text = "<font color=#E12825>(X)</font>"
        End If
    End Sub

    Protected Sub DataList2_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList2.PreRender
        If Me.DataList2.Items.Count = 0 Then
            Me.DataList2.Visible = False
            'Else
            'If Me.DataList2.SelectedIndex > 0 Then
            'Me.DataList2.SelectedItem.Focus()
            'End If
        REM ElseIf Session.Item("gr") <> 0 Then
            REM Dim lbl As Label
            REM lbl = DataList2.Items(0).FindControl("Label9")
            REM lbl.Text = "<font color=#E12825>(X)</font>"
        End If
    End Sub

    Protected Sub DataList3_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList3.PreRender
        If Me.DataList3.Items.Count = 0 Then
            Me.DataList3.Visible = False
            'Else
            'If Me.DataList3.SelectedIndex > 0 Then
            'Me.DataList3.SelectedItem.Focus()
            'End If
        REM ElseIf Session.Item("sg") <> 0 Then
            REM Dim lbl As Label
            REM lbl = DataList3.Items(0).FindControl("Label9")
            REM lbl.Text = "<font color=#E12825>(X)</font>"
        End If
    End Sub

    Protected Sub DataList4_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataList4.PreRender
        If Me.DataList4.Items.Count = 0 Then
            Me.DataList4.Visible = False
            'Else
            'If Me.DataList4.SelectedIndex > 0 Then
            'Me.DataList4.SelectedItem.Focus()
            'End If
        REM ElseIf Session.Item("mr") <> 0 Then
            REM Dim lbl As Label
            REM lbl = DataList4.Items(0).FindControl("Label9")
            REM lbl.Text = "<font color=#E12825>(X)</font>"
        End If
    End Sub

    Protected Sub rPromo_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs)
        Dim label_sconto As Label
        Dim panel_sconto As Panel

        panel_sconto = e.Item.Parent.Parent.FindControl("Panel_Visualizza_Percentuale_Sconto")
        label_sconto = e.Item.Parent.Parent.FindControl("sconto_applicato")

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

        Dim temp As String

        If InOfferta.Text = 1 Then
            Panel_offerta.Visible = True

            img_offerta.Visible = True

            If QtaMin.Text > 0 Then
                Offerta.Text = Offerta.Text & "MINIMO"
                QtaMin.Visible = True
                Qta.Text = QtaMin.Text
            ElseIf QtaMultipli.Text > 0 Then
                Offerta.Text = Offerta.Text & "MULTIPLI"
                QtaMultipli.Visible = True
                Qta.Text = QtaMultipli.Text
            End If

            If IvaTipo = 1 Then
                'Offerta.Text = Offerta.Text & " A € " & FormatNumber(PrezzoPromo.Text, 2)
                ParentPrezzoPromo.Text = "€ " & FormatNumber(PrezzoPromo.Text, 2)
                PrezzoPromo.Visible = True
                ParentPrezzo.Visible = True
                ParentPrezzo.Font.Strikeout = True

                temp = ParentPrezzoPromo.Text
            ElseIf IvaTipo = 2 Then
                'Offerta.Text = Offerta.Text & " A € " & FormatNumber(PrezzoPromoIvato.Text, 2)
                ParentPrezzoPromo.Text = "€ " & FormatNumber(PrezzoPromoIvato.Text, 2)
                PrezzoPromoIvato.Visible = True
                ParentPrezzoIvato.Visible = True
                ParentPrezzoIvato.Font.Strikeout = True

                temp = ParentPrezzoPromo.Text
            End If

            'Stampo a video lo sconto applcato all'offerta
            panel_sconto.Visible = True
            If IvaTipo = 1 Then
                'label_sconto.Text = "- " & String.Format("{0:0}", ((100 * (ParentPrezzo.Text - temp)) / ParentPrezzo.Text)) & "%"
                label_sconto.Text = String.Format("{0:0}", (((ParentPrezzo.Text - temp) * 100) / ParentPrezzo.Text)) & "%"
            Else
                'label_sconto.Text = "- " & String.Format("{0:0}", ((100 * (ParentPrezzoIvato.Text - temp)) / ParentPrezzoIvato.Text)) & "%"
                label_sconto.Text = String.Format("{0:0}", (((ParentPrezzoIvato.Text - temp) * 100) / ParentPrezzoIvato.Text)) & "%"
            End If

            'Controllo che lo sconto non sia inferiore a 0
            Try
                If Val(label_sconto.Text) <= 0 Then
                    label_sconto.Text = "0%"
                Else
                    label_sconto.Text = "-" & label_sconto.Text
                End If
            Catch
            End Try


            Dim cifre_da_visualizzare As String = ""
            If Val(dispo.Text) > 0 Then
                cifre_da_visualizzare = "Images/cifre_ok/"
            Else
                cifre_da_visualizzare = "Images/cifre_no/"
            End If
        End If

    End Sub

    Protected Sub Selezione_Multipla_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
        Dim i As Integer = 0
        Dim temp_check As CheckBox
        Dim ListaArticoli As New ArrayList

        Dim Qta As TextBox
        Dim ID As Label
        Dim TCID As Label

        ID = Me.GridView1.Rows(i).FindControl("lblID")
        TCID = Me.GridView1.Rows(i).FindControl("lblTCID")
        Qta = Me.GridView1.Rows(i).FindControl("tbQuantita")

        'Verifica se il settore del prodotto è Attivo o meno. Altrimenti reindirizzo l'utente verso una pagina di errore, 
        'che avvisa l'utente che l'amministratore ha disabilitato tale Settore e quindi tutti gli articoli correlati non 
        'sono più disponibili per la vendita
        If controlla_abilitazione_settore(Val(ID.Text)) = 1 Then
            For i = 0 To Me.GridView1.Rows.Count - 1
                temp_check = CType(Me.GridView1.Rows(i).FindControl("CheckBox_SelezioneMultipla"), CheckBox)
                If temp_check.Checked = True Then
                    Dim temp2 As GridView

                    temp2 = CType(Me.GridView1.Rows(i).FindControl("GridView3"), GridView)
                    If temp2.Rows.Count > 0 Then
                        'Comunico al carrello se il prodotto è un prodotto che ha spedizione gratis
                        Session("ProdottoGratis") = 1
                    Else
                        'Comunico al carrello se il prodotto non è un prodotto che ha spedizione gratis
                        Session("ProdottoGratis") = 0
                    End If


                    ID = Me.GridView1.Rows(i).FindControl("lblID")
                    TCID = Me.GridView1.Rows(i).FindControl("lblTCID")
                    Qta = Me.GridView1.Rows(i).FindControl("tbQuantita")

                    Me.Session("Carrello_ArticoloId") = ID.Text
                    Me.Session("Carrello_TCId") = TCId.Text
                    Me.Session("Carrello_Quantita") = Qta.Text

                    ListaArticoli.Add(ID.Text & "," & TCID.Text & "," & Qta.Text & "," & Session("ProdottoGratis"))
                End If
            Next

            Session("Carrello_SelezioneMultipla") = ListaArticoli
            Me.Response.Redirect("aggiungi.aspx")
        Else
            Response.Redirect("settore_disabilitato.aspx")
        End If
    End Sub

    Public Function spedito_gratis(ByVal idArticolo As Integer, ByVal listino As Integer) As Integer
        Dim params As New Dictionary(Of String, Object)
        params.add("@Listino",listino)
		params.add("@IdArticolo",idArticolo)
        Dim dr = ExecuteQueryGetDataReader("SpedizioneGratis_Listini, SpedizioneGratis_Data_Inizio, SpedizioneGratis_Data_Fine, id", "articoli", "where (SpedizioneGratis_Listini LIKE CONCAT('%',@Listino, ';%')) AND (id = @IdArticolo) AND (SpedizioneGratis_Data_Inizio <= CURDATE()) AND (SpedizioneGratis_Data_Fine >= CURDATE())", params)
        'Restituisce 1 nel caso ci sia almeno una riga come risultato, e quindi il settore relativo all' IDArticolo è ABILITATO altrimenti restituisce 0
        If (dr.Count > 0) Then
            Return 1
        Else
            Return 0
        End If
    End Function

    Protected Sub BT_Aggiungi_wishlist_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        'Dim ArticoloId As Label = sender.NamingContainer.FindControl("label_idArticolo")
		Dim ID As Label= sender.NamingContainer.FindControl("lblID")
        Dim TCID As Label= sender.NamingContainer.FindControl("lblTCID")
        If (Session.Item("UtentiId") > 0) Then
            Dim paramsSelect As New Dictionary(Of String, Object)
            paramsSelect.add("@IdArticolo",ID.Text)
			paramsSelect.add("@TcId",TCID.Text)
			paramsSelect.add("@IdUtente",Session.Item("UtentiID"))
            Dim dr = ExecuteQueryGetDataReader("id", "wishlist", "where (id_articolo=@IdArticolo) AND (TCid=@TcId) AND (id_utente=@IdUtente)", paramsSelect)
            'Restituisce 1 nel caso ci sia almeno una riga come risultato, e quindi il settore relativo all' IDArticolo è ABILITATO altrimenti restituisce 0
            If (dr.Count <= 0) Then
                Dim paramsProcedure As New Dictionary(Of String, String)
                paramsProcedure.add("?pIdUtente", Session.Item("UtentiID"))
                paramsProcedure.add("?pIdArticolo", ID.Text)
                paramsProcedure.add("?pTCid", TCID.Text)
                ExecuteStoredProcedure(paramsProcedure, "NewElement_Wishlist")
            End If
        End If
    End Sub

    Function controlla_abilitazione_settore(ByVal idArticolo As Integer) As Integer
        Dim params As New Dictionary(Of String, Object)
        params.add("@VsuperarticoliId",idArticolo)
        Dim dr = ExecuteQueryGetDataReader("*", "vsuperarticoli", "INNER JOIN settori ON settori.id=vsuperarticoli.SettoriId WHERE (vsuperarticoli.id=@VsuperarticoliId) AND (settori.Abilitato=1)", params)
        'Restituisce 1 nel caso ci sia almeno una riga come risultato, e quindi il settore relativo all' IDArticolo è ABILITATO altrimenti restituisce 0
        If (dr.Count > 0) Then
            Return 1
        Else
            Return 0
        End If
    End Function
	
	Protected Sub CheckBoxMr_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
		CheckBoxFilter_CheckedChanged(sender, e, "mr")
    End Sub

	Protected Sub CheckBoxSg_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
		CheckBoxFilter_CheckedChanged(sender, e, "sg")
    End Sub

	Protected Sub CheckBoxTp_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
		CheckBoxFilter_CheckedChanged(sender, e, "tp")
    End Sub

	Protected Sub CheckBoxGr_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
		CheckBoxFilter_CheckedChanged(sender, e, "gr")
    End Sub

	Protected Sub CheckBoxFilter_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs, ByVal parName As String)
		Dim checkBox As CheckBox = sender
		Dim filterId as String = checkBox.Attributes("filterId")
		Dim queryString As String = Request.QueryString(parName)
		Dim parValue As String = String.Empty
		if checkBox.Checked = True Then
			if queryString <> String.Empty Then				
				parValue = queryString & ("|") & filterId
			else
				parValue = filterId
			End If
		else 
			Dim ids As String() = queryString.split("|")
			if ids.length > 1 then
				Dim filterIdArray As String() = filterId.split("|")
				For Each id As String In ids
					if Not Array.IndexOf(filterIdArray,id)>=0 Then
						parValue &= id & "|"
					end if
				Next
				if parValue.length > 0 then
					parValue = parValue.Substring(0, parValue.Length - 1)
				end if
			end if
		End If
        Dim newUrl As String = changeUrlGetParam(Request.UrlReferrer.ToString, parName, parValue)
		Response.Redirect(newUrl)
    End Sub

	Protected Sub Drop_Ordinamento_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Drop_Ordinamento.SelectedIndexChanged
		Dim newUrl As String = changeUrlDependingFromDropDownList(Request.UrlReferrer.ToString, Drop_Ordinamento, "ordinamento")
		Response.Redirect(newUrl)
    End Sub

    Protected Sub Drop_Filtra_Taglia_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Drop_Filtra_Taglia.SelectedIndexChanged
        Dim newUrl As String = changeUrlDependingFromDropDownList(Request.UrlReferrer.ToString, Drop_Filtra_Taglia, "taglia")
		Response.Redirect(newUrl)
    End Sub

    Protected Sub Drop_Filtra_Colore_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Drop_Filtra_Colore.SelectedIndexChanged
        Dim newUrl As String = changeUrlDependingFromDropDownList(Request.UrlReferrer.ToString, Drop_Filtra_Colore, "colore")
		Response.Redirect(newUrl)
    End Sub

    Protected Sub CheckBox_Disponibile_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CheckBox_Disponibile.CheckedChanged
        Dim newUrl As String = changeUrlDependingFromCheckBox(Request.UrlReferrer.ToString, CheckBox_Disponibile, "disponibile", "1", "0")
		Response.Redirect(newUrl)
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        
    End Sub
	
	Sub changeCheckBoxDependingFromUrl(ByVal checkBox As CheckBox, ByVal parName As String, ByVal parValueIfChecked As String)
		If Request.QueryString(parName) = parValueIfChecked Then
			checkBox.Checked = True
		Else
			checkBox.Checked = False
		End If
	End Sub
	
	Function changeUrlDependingFromCheckBox(ByVal url As String, ByVal checkBox As CheckBox, ByVal parName As String, ByVal parValueIfChecked As String, ByVal parValueIfNotChecked As String) As String
		Dim newUrl As String
		If checkBox.Checked = True Then
			newUrl = changeUrlGetParam(url, parName, parValueIfChecked)
        Else
			newUrl = changeUrlGetParam(url, parName, parValueIfNotChecked)
        End If
		return newUrl
	End Function
	
	Sub changeDropDownListDependingFromUrl(ByVal dropDownList As DropDownList, ByVal parName As String)
		' 0 = index , 1 = value
		if Request.QueryString(parName) <> String.Empty Then
			dropDownList.selectedValue = Request.QueryString(parName).split("|")(1)
		End If
	End Sub
	
	Function changeUrlDependingFromDropDownList(ByVal url As String, ByVal dropDownList As DropDownList, ByVal parName As String) As String
		return changeUrlGetParam(url, parName, dropDownList.SelectedIndex & "|" & dropDownList.selectedValue)
	End Function
	
	Function changeUrlGetParam(ByVal url As String, ByVal parName As String, ByVal parValue As String) As String
		Dim newUrl As String = Regex.Replace(url, "&" & parName & "=([^&])+", String.Empty)
		newUrl = Regex.Replace(newUrl, "\?" & parName & "=([^&])+", "?")
		if parValue <> String.Empty Then
			newUrl = newUrl & "&" & parName & "=" & parValue
		End if
		return newUrl.Replace("?&", "?")
	End Function

	Sub alert(ByVal message As String)
	   Dim myScript as String = "window.alert('" & message.replace("'","|") & "');"
	   ClientScript.RegisterStartupScript(Me.GetType(), "myScript", myScript, True)
	End Sub
	
	function getCorrectLengthDescription(ByVal description As String) as String
		if description.length>28 Then
			return description.substring(0,26) & "..."
		end if
		return description
	end function
	
	function getFilterIds(ByVal parName As String) as String()
		if Not String.IsNullOrEmpty(Request.QueryString(parName)) Then
			return Request.QueryString(parName).split("|")
		else
			Dim result(0) As String
			return result
		end if
	end function
	
	function addIds(ByVal filterIds As String, ByVal idsToAdd As String) As String
		Dim result As String = filterIds
		Dim filterIdsArray As String() = filterIds.split("|")
		Dim idsToAddArray As String() = idsToAdd.split("|")
		for each id As String in idsToAddArray
			if Not Array.IndexOf(filterIdsArray,id)>=0 Then
				result = result + "|" + id
			end if
		next
		return result
	End function
	
	function filterIdsContains(ByVal parName As String, ByVal ids As String) as Boolean
	if Not String.IsNullOrEmpty(Request.QueryString(parName)) Then
			Dim queryStringIds = Request.QueryString(parName).split("|")
			Dim idsArray = ids.split("|")
			For Each id As String In idsArray
				if Array.IndexOf(queryStringIds,id)>=0 Then
					If filters.ContainsKey(parName) then
						filters.Item(parName) = addIds(filters.Item(parName),ids)
					else
						filters.add(parName,ids)
					end if
					return True
				End if
			Next
			return False
		else
			return False
		end if
	end function

    Protected Function ExecuteQueryGetDataSet(ByVal fields As String, ByVal table As String, Optional ByVal wherePart As String = "", Optional ByVal params As Dictionary(Of String, String) = Nothing) As DataSet
        Dim sqlString As String = "SELECT " & fields & " FROM " & table & " " & wherePart
        Dim ds As DataSet = New DataSet()
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
				If Not params Is Nothing then
					Dim paramName As String
					For Each paramName In params.Keys
						cmd.Parameters.AddWithValue(paramName, params(paramName))
					Next
				End If
				Dim sqlAdp As New MySqlDataAdapter(cmd)
				sqlAdp.Fill(ds, table)
				sqlAdp.Dispose()
                cmd.Dispose()
            End If
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If
        End Try
        Return ds
    End Function

    Protected Function ExecuteQueryGetDataReader(ByVal fields As String, ByVal table As String, Optional ByVal wherePart As String = "", Optional ByVal params As Dictionary(Of String, Object) = Nothing) As List(Of Dictionary(Of String, Object))
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
                        If Not row.ContainsKey(columnName) Then
                            row.Add(columnName, value)
                        End If
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

    Protected Function ExecuteInsert(ByVal fields As String, ByVal table As String, Optional ByVal values As String = "", Optional ByVal params As Dictionary(Of String, Object) = Nothing)
        Dim sqlString As String = "INSERT INTO " & table & " (" & fields & ") VALUES (" & values & ")"
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

    Protected Sub ExecuteStoredProcedure(ByVal params As Dictionary(Of String, String), ByVal storedProcedure As String)
        Dim conn As New MySqlConnection
        Try
            Dim connectionString As String = ConfigurationManager.ConnectionStrings("EntropicConnectionString").ConnectionString
            conn.ConnectionString = connectionString
            conn.Open()
            Dim cmd = New MySqlCommand With {
                .Connection = conn,
                .CommandType = CommandType.StoredProcedure,
                .CommandText = storedProcedure
            }
            For Each paramName In params.Keys
                cmd.Parameters.AddWithValue(paramName, params(paramName))
            Next
            cmd.Parameters.AddWithValue("?parRetVal", "0")
            cmd.Parameters("?parRetVal").Direction = ParameterDirection.Output
            cmd.ExecuteNonQuery()
            cmd.Parameters.Clear()
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
                conn.Dispose()
            End If
        End Try
    End Sub
End Class
