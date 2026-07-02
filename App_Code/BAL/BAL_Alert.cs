using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for BAL_Alert
/// </summary>
public class BAL_Alert
{
	public BAL_Alert()
	{
		//
		// TODO: Add constructor logic here
		//
	}


    public static DataSet send_bimar_animal_push_notificaton()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "send_bimar_animal_push_notificaton_sp";
        return command.ExtQueryDS(cmd);
    }

    public static DataSet send_pregancy_check_push_notificaton()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "send_pregancy_check_push_notificaton_sp";
        return command.ExtQueryDS(cmd);
    }
    public static DataSet send_delivery_push_notificaton()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "send_delivery_push_notificaton_sp";
        return command.ExtQueryDS(cmd);
    }

    public static DataSet send_adult_cow_push_notificaton()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "send_adult_cow_push_notificaton_sp";
        return command.ExtQueryDS(cmd);
    }

}