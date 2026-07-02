using System;
using System.Configuration;
using System.Data.SqlClient;

/// <summary>
/// Summary description for connection
/// </summary>
public class connection
{
    public connection()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static SqlConnection open_connection()
    {
        // DO NOT open here — let caller decide OR DataAdapter handle it
        return new SqlConnection(
            ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString
        );
    }
}
