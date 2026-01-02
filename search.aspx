<%@ Page Language="VB" AutoEventWireup="false" CodeFile="search.aspx.vb" Inherits="search" %>

        <asp:SqlDataSource ID="Search_Items" runat="server" ConnectionString="<%$ ConnectionStrings:EntropicConnectionString %>"
            ProviderName="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
            SelectCommand="SELECT id, Descrizione1, Img1 FROM varticolibase"></asp:SqlDataSource>
<asp:sqldatasource id="Sql_Occorrenze" runat="server" connectionstring="<%$ ConnectionStrings:EntropicConnectionString %>"
    providername="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
    selectcommand="SELECT QString, Conteggio FROM conteggia_querystring"></asp:sqldatasource>
<asp:sqldatasource id="Sql_TabellaQueryString" runat="server" connectionstring="<%$ ConnectionStrings:EntropicConnectionString %>"
    deletecommand="DELETE * FROM query_string" providername="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
    selectcommand="SELECT QString, Conteggio FROM query_string"></asp:sqldatasource>
<asp:sqldatasource id="Sql_VistaCompleta" runat="server" connectionstring="<%$ ConnectionStrings:EntropicConnectionString %>"
    providername="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>" selectcommand="SELECT QString, Conteggio FROM conteggia_querystring"></asp:sqldatasource>
<asp:sqldatasource id="Sql_NumeroRighe" runat="server" connectionstring="<%$ ConnectionStrings:EntropicConnectionString %>"
    providername="<%$ ConnectionStrings:EntropicConnectionString.ProviderName %>"
    selectcommand="SELECT COUNT(*) AS RECORD_COUNT FROM query_string"></asp:sqldatasource>
<asp:gridview id="Grid_VistaCompleta" runat="server" autogeneratecolumns="False"
    datasourceid="Sql_VistaCompleta" visible="False"><Columns>
<asp:BoundField DataField="QString" HeaderText="QString" SortExpression="QString"></asp:BoundField>
<asp:BoundField DataField="Conteggio" HeaderText="Conteggio" SortExpression="Conteggio"></asp:BoundField>
</Columns>
</asp:gridview>
<asp:gridview id="Grid_ConteggioRecord" runat="server"
    visible="False" AutoGenerateColumns="False" DataSourceID="Sql_NumeroRighe"><Columns>
<asp:BoundField DataField="RECORD_COUNT" HeaderText="RECORD_COUNT" SortExpression="RECORD_COUNT"></asp:BoundField>
</Columns>
</asp:gridview>
        
        <asp:GridView ID="Grid_Items" runat="server" AutoGenerateColumns="False" DataKeyNames="id"
            DataSourceID="Search_Items" Visible="False">
            <Columns>
                <asp:BoundField DataField="id" HeaderText="id" InsertVisible="False" ReadOnly="True"
                    SortExpression="id" />
                    <asp:BoundField DataField="tcid" HeaderText="tcid" InsertVisible="False" ReadOnly="True"
                    SortExpression="tcid" />
                <asp:BoundField DataField="Descrizione1" HeaderText="Descrizione1" SortExpression="Descrizione1" />
                <asp:BoundField DataField="Img1" HeaderText="Img1" SortExpression="Img1" />
            </Columns>
        </asp:GridView>
<asp:gridview id="Grid_Occorrenze" runat="server" autogeneratecolumns="False"
    datasourceid="Sql_Occorrenze" visible="False"><Columns>
<asp:BoundField DataField="QString" HeaderText="QString" SortExpression="QString"></asp:BoundField>
<asp:BoundField DataField="Conteggio" HeaderText="Conteggio" SortExpression="Conteggio"></asp:BoundField>
</Columns>
</asp:gridview>

