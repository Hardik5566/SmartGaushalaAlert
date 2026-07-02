using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using System.Web;

/// <summary>
/// Summary description for parameter
/// </summary>
public class parameter
{
	public parameter()
	{
		//
		// TODO: Add constructor logic here
		//
	}
    public SqlParameter intparam(string name, string value)
    {
        SqlParameter param = new SqlParameter();
        param.DbType = DbType.Int32;
        param.ParameterName = name;
        param.Value = value;
        return param;
    }

    public SqlParameter floatparam(string name, string value)
    {
        SqlParameter param = new SqlParameter();
        param.DbType = DbType.Double;
        param.ParameterName = name;
        param.Value = value;
        return param;
    }

    public SqlParameter stringparam(string name, string value)
    {
        SqlParameter param = new SqlParameter();
        param.DbType = DbType.String;
        param.ParameterName = name;
        param.Value = value;
        return param;
    }

    public SqlParameter datetimeparam(string name, string value)
    {
        SqlParameter param = new SqlParameter();
        param.DbType = DbType.DateTime;
        param.ParameterName = name;
        param.Value = value;
        return param;
    }

    public SqlParameter boolparam(string name, string value)
    {
        SqlParameter param = new SqlParameter();
        param.DbType = DbType.Boolean;
        param.ParameterName = name;
        param.Value = value;
        return param;
    }

    public SqlParameter TableParam(string name, DataTable dt, string typeName)
    {
        SqlParameter param = new SqlParameter();
        param.ParameterName = name;
        param.SqlDbType = SqlDbType.Structured;
        param.TypeName = typeName; // This is the SQL type name like 'dbo.AcCalDetailType'
        param.Value = dt;
        return param;
    }

}