Imports System.Data
Imports MySql.Data.MySqlClient
Imports it.sella.ecomms2s
Imports System.Xml

Partial Class BancaSella
    Inherits System.Web.UI.Page
    Public result As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim params As New Dictionary(Of String, String)
        params.add("@AziendaId", Session("AziendaId"))
        Dim dr = ExecuteQueryGetDataReader("*", "bancasella_impostazioni_azienda", "WHERE aziendeid = @AziendaId", params)
        Try
            Dim shopLogin As String = dr(0)("shopLogin")
            Dim currency As String = Request.QueryString("currency")
            Dim amount As String = Request.QueryString("amount")
            Dim shopTransactionId As String = Request.QueryString("shopTransactionId")
            Dim iddocumento As String = Request.QueryString("idDocumento")
            Dim sitoWeb As String = Request.QueryString("sitoWeb")
            Dim customInfo As String = "iddocumento=" & iddocumento & "*P1*sito=" & sitoWeb
            Dim buyername As String = Request.QueryString("buyerName")
            Dim buyeremail As String = Request.QueryString("buyerEmail")
            Dim PaymentTDetail As New PaymentTypeDetail()
            Dim ShipDetails As New ShippingDetails()
            Dim RedBilling As New RedBillingInfo()
            Dim RedCustomerData As New RedCustomerData()
            Dim RedCustomerInfo As New RedCustomerInfo()
            Dim RedItem As New RedItems()
            Dim RedShipping As New RedShippingInfo()
            Dim ConselCustomer As New ConselCustomerInfo()
            Dim PaymentTypes = New String() {""}
            Dim RedCustomInfo = New String() {""}
            Dim OrderDetail As New EcommGestpayPaymentDetails()
            Dim objCrypt As New WSCryptDecrypt()
            XMLOUT = objCrypt.Encrypt(shopLogin, currency, amount, shopTransactionId, "", "", "", "", "", "", "", customInfo, "", "", ShipDetails, PaymentTypes, PaymentTDetail, "", RedCustomerInfo, RedShipping, RedBilling, RedCustomerData, RedCustomInfo, RedItem, "", ConselCustomer, "", OrderDetail).OuterXml
            Dim XmlReturn As New XmlDocument()
            XmlReturn.LoadXml(XMLOUT)
            Dim ErrorCode As String = XmlReturn.SelectSingleNode("/GestPayCryptDecrypt/ErrorCode").InnerText
            If ErrorCode = "0" Then
                Dim encryptedData As String = XmlReturn.SelectSingleNode("//GestPayCryptDecrypt/CryptDecryptString").InnerText
                Response.Redirect("https://ecomm.sella.it/pagam/pagam.aspx?a=" & shopLogin & "&b=" & HttpUtility.UrlEncode(encryptedData))
            Else
                result = "Unexpected DB Error. Please contact site administrator"
            End If
        Catch ex As Exception
            result = result & "Unexpected DB Error. Please contact site administrator. " & ex.GetType().Name & ". " & ex.Message & ". " & ex.StackTrace
        End Try
    End Sub

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



