Imports System.Collections.Generic

Partial Class search
    Inherits System.Web.UI.Page

    Public Sub Carica_Articoli()
        Dim valore_ricercato As String
        Dim i As Integer

        valore_ricercato = Request.Form("search")

        If (valore_ricercato <> "") Then
            'Replace dei caratteri speciali, per evitare SQL injection
            valore_ricercato = valore_ricercato.Replace("'", "")
            valore_ricercato = valore_ricercato.Replace("*", "")
            'valore_ricercato = valore_ricercato.Replace("/", "")
            valore_ricercato = valore_ricercato.Replace("&", "")
            valore_ricercato = valore_ricercato.Replace("#", "")

            Dim Parole() As String = Split(valore_ricercato, " ")

            If (Parole.Length) > 1 Then
                Dim Temp As String
                Dim Temp2 As String

                'Costruzione comando query per la ricerca del prodotto
                Temp = "SELECT id, TCid, Descrizione1, Img1, Giacenza, Visite, Codice FROM varticoligiacenze WHERE (((Descrizione1 LIKE '%" & Parole(0) & "%') OR (Codice LIKE '%" & Parole(0) & "%'))"
                'Costruzione comando query per la ricerca del suggerimento tra quelli indicizzati
                Temp2 = "SELECT QString, Conteggio FROM conteggia_querystring WHERE ((QString LIKE '%" & Parole(0) & "%') "

                'concatenazione nel caso di stringa di ricerca multiparola
                For i = 1 To (Parole.Length - 1)
                    Temp = Temp & "AND ((Descrizione1 LIKE '%" & Parole(i) & "%') OR (Codice LIKE '%" & Parole(i) & "%')) "
                    Temp2 = Temp2 & "AND (QString LIKE '%" & Parole(i) & "%') "
                Next
                Temp = Temp & ") ORDER BY Giacenza DESC LIMIT 6" 'Numero massimo link a prodotti
                Temp2 = Temp2 & ") ORDER BY Conteggio LIMIT 5"   'Numero massimo link a suggerimenti testuali per la ricerca
                Me.Search_Items.SelectCommand = Temp
                Me.Sql_Occorrenze.SelectCommand = Temp2
            Else
                Me.Sql_Occorrenze.SelectCommand = "SELECT QString, Conteggio FROM conteggia_querystring WHERE (QString LIKE '%@valoreRicercato%') ORDER BY Conteggio LIMIT 4"
                Me.Sql_Occorrenze.SelectParameters.Add("@valoreRicercato", valore_ricercato)
                Me.Search_Items.SelectCommand = "SELECT id, TCid, Descrizione1, Img1, Giacenza, Visite FROM varticoligiacenze WHERE ((Descrizione1 LIKE '%@valoreRicercato%') OR (Codice LIKE '%@valoreRicercato%')) ORDER BY Giacenza DESC LIMIT 6"
                Me.Search_Items.SelectParameters.Add("@valoreRicercato", valore_ricercato)
            End If


            If Me.Grid_ConteggioRecord.Rows.Count > 0 Then
                'Ogni Mod 50 riucerche viene compressa la tabella 
                If ((Integer.Parse(Me.Grid_ConteggioRecord.Rows(0).Cells(0).Text) Mod 50) = 0) Then
                    Dim Diz_VistaCompleta As New Dictionary(Of String, String)

                    For i = 0 To Me.Grid_VistaCompleta.Rows.Count() - 1
                        Diz_VistaCompleta.Add(Me.Grid_VistaCompleta.Rows(i).Cells(0).Text, Me.Grid_VistaCompleta.Rows(i).Cells(1).Text)
                    Next

                    Me.Sql_Occorrenze.DeleteCommand = "DELETE FROM query_string"
                    Me.Sql_Occorrenze.Delete()

                    Dim s As String
                    For Each s In Diz_VistaCompleta.Keys
                        Me.Sql_TabellaQueryString.InsertCommand = "INSERT INTO query_string VALUES( @s, @DizVistaCompleta)"
                        Me.Sql_TabellaQueryString.InsertParameters.Add("@s", s)
                        Me.Sql_TabellaQueryString.InsertParameters.Add("@DizVistaCompleta", Diz_VistaCompleta(s))
                        Me.Sql_TabellaQueryString.Insert()
                    Next
                End If
            End If
        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim i As Integer
        Dim ID As String
        Dim TCID As String
        Dim Descrizione As String
        Dim Nome_Foto As String
        Dim valore_ricercato As String

        Dim Colore As String = "Black"
        If (Session.Item("AziendaID")) = 1 Then
            Colore = "#E12825"
        End If
        If (Session.Item("AziendaID")) = 2 Then
            Colore = "#FF8C00"
        End If

        valore_ricercato = Request.Form("search") 'Solo per la stampa a video


        Response.Write("<ul style=""margin-left:0px;"">" + Chr(13))
        Carica_Articoli()
        If (Me.Grid_Occorrenze.Rows.Count > 0) Then
            'Visualizza o meno l'Header dei suggerimenti
            Response.Write("<li style=""height:25px; background-color:#d3d3d3;""><div style=""position:relative;width:500px;top:0px;left:0px;""><a style=""font-size:13px;font-weight:bold;color:#666;background: transparent none repeat scroll;text-decoration: none"">Forse cercavi</a></div></li>" & Chr(13))
            For i = 0 To Me.Grid_Occorrenze.Rows.Count - 1
                Response.Write("<li style=""height:20px;""><div style=""margin-top:-4px;position:relative;width:500px;top:0px;left:0px;vertical-align:middle;""><a href=""articoli.aspx?q=" & Me.Grid_Occorrenze.Rows(i).Cells(0).Text & """ style=""font-weight:bold; text-align:left; color:" & Colore & ";background: transparent none repeat scroll;text-decoration: none;"" >" & Me.Grid_Occorrenze.Rows(i).Cells(0).Text & "</a></div></li>" & Chr(13))
            Next
        End If

        If (Me.Grid_Items.Rows.Count > 0) Then
            'Visualizza o meno l'Header dei suggerimenti
            Response.Write("<li style=""height:25px; background-color:#d3d3d3;""><div style=""position:relative;width:500px;top:0px;left:0px;""><a style=""font-size:13px;font-weight:bold;color:#666;background: transparent none repeat scroll;text-decoration: none;  vertical-align:middle;"">Prodotti</a></div></li>" & Chr(13))

            For i = 0 To Me.Grid_Items.Rows.Count - 1
                ID = Me.Grid_Items.Rows(i).Cells(0).Text
                TCID = Me.Grid_Items.Rows(i).Cells(1).Text
                Descrizione = Me.Grid_Items.Rows(i).Cells(2).Text
                If Descrizione.Length > 200 Then
                    Descrizione = Mid(Descrizione, 1, 200) & "..."
                End If
                Nome_Foto = Me.Grid_Items.Rows(i).Cells(3).Text
                Response.Write("<li><div style=""position:relative;width:500px;top:0px;left:0px;""><div style=""position: absolute;width:50px;top:0px;left:0px;""><img style=""height:40px;width:40px border-width: 0px;"" width=""40px"" src=""Public/foto/" & Nome_Foto & """></div> <div style=""width:440px;position: absolute; top: 0px; left:60px; right: 0px; text-align:left;""><a href=""articolo.aspx?id=" & ID & "&TCid=" & TCID & """ style=""font-weight:bold;color:" & Colore & ";background: transparent none repeat scroll;text-decoration: none"" >" & Descrizione & "</a></div></div></li>" & Chr(13))
            Next
        End If

        If Me.Grid_Items.Rows.Count > 0 Then
            Response.Write("<li style=""height:25px;background-color:#d3d3d3;""><div style=""position:relative;width:500px;top:0px;left:0px;""><a href=""articoli.aspx?q=" & valore_ricercato & """ style=""font-size:15px;font-weight:bold;color:#666;background: transparent none repeat scroll;text-decoration: none"">Visualizza tutti i rislultati per """ & valore_ricercato & """</a></div></li>" & Chr(13))
        Else
            Response.Write("<li style=""height:25px;background-color:#d3d3d3;""><div style=""position:relative;width:500px;top:0px;left:0px;""><a style=""font-size:12px;font-weight:bold;color:#666;background: transparent none repeat scroll;text-decoration: none"">Nessun risultato per """ & valore_ricercato & """</a></div></li>" & Chr(13))
        End If
        Response.Write("</ul>")
    End Sub
End Class
