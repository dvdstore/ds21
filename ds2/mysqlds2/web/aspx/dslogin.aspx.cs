
/*
* DVD Store Login ASP Page - dslogin.aspx.cs
*
* Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
*
* Login to MySQL DVD store 
*
* Last Updated 7/3/05
*
*  This program is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/



using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using MySql.Data.MySqlClient;

namespace ds2
  {
  /// <summary>
  /// dslogin.aspx.cs: validates login to MySQL DVD Store database
  /// Copyright 2005 Dell, Inc.
  /// Written by Todd Muirhead/Dave Jaffe      Last modified: 7/3/05
  /// </summary>
    
  public class dslogin : System.Web.UI.Page
    {
    private void Page_Load(object sender, System.EventArgs e)
      {
      string username = null, password = null, conn_str = null, db_query = null, 
        db_query2 = null;
      string form_str = null;
      int customerid = 0;
      MySqlCommand cmd = null;    
      MySqlConnection conn = null, conn2 = null;
      MySqlDataReader reader = null, reader2 = null;
          
      Response.Write(dscommon.ds_html_header("DVD Store Login Page"));

      if (Request.QueryString.HasKeys())
        {
        username = Request.QueryString.GetValues("username")[0];
        }

      if (!(username == null))
        {
        password = Request.QueryString.GetValues("password")[0];
              
        conn_str = "Server=localhost;User ID=web;Password=web;Database=DS2;Pooling=false";
        conn = new MySqlConnection(conn_str);
        conn.Open();       
        db_query="select CUSTOMERID FROM DS2.CUSTOMERS where USERNAME='" + username +
          "' and PASSWORD='" + password + "';";
        cmd = new MySqlCommand(db_query, conn);
        if(cmd.ExecuteScalar() != null) customerid = (int) cmd.ExecuteScalar();
        
        if (customerid != 0)
          {
          Response.Write
            ("<H2>Welcome to the DVD Store - Click below to begin shopping</H2>\n");
          db_query = "SELECT DS2.PRODUCTS.TITLE, DS2.PRODUCTS.ACTOR, " + 
            "DS2.PRODUCTS.COMMON_PROD_ID FROM DS2.CUST_HIST INNER JOIN DS2.PRODUCTS " + 
            "ON DS2.CUST_HIST.PROD_ID = DS2.PRODUCTS.PROD_ID " +
            "WHERE DS2.CUST_HIST.CUSTOMERID =" + customerid  + 
            " ORDER BY ORDERID DESC, TITLE ASC LIMIT 10;";
          cmd = new MySqlCommand(db_query, conn);
          reader = cmd.ExecuteReader();
          if (!reader.HasRows)  // No previous order
            {
            //Response.Write("<H3>No previous orders</H3>\n");
            }
          else  // There is a previous order
            {
            form_str = 
              "<H3>Your previous order:</H3>\n" +
              "<TABLE border=2>\n" +
              "<TR>\n" +
              "<TH>Title</TH>\n" +
              "<TH>Actor</TH>\n" +
              "<TH>People who liked this DVD also liked</TH>\n" +
              "</TR>\n";
            Response.Write(form_str);
       
            while(reader.Read())
              {
              db_query2 = "select TITLE from DS2.PRODUCTS where PROD_ID=" 
                + reader.GetInt32(2) + ";";
              conn2 = new MySqlConnection(conn_str);
              conn2.Open();
              cmd = new MySqlCommand(db_query2, conn2);
              reader2 = cmd.ExecuteReader();
              reader2.Read();                         
              Response.Write(" <TR>\n");
              Response.Write("<TD>" + reader.GetString(0)+ "</TD>");
              Response.Write("<TD>" + reader.GetString(1)+ "</TD>");
              Response.Write("<TD>" + reader2.GetString(0)+ "</TD>");
              reader2.Close();    
              Response.Write("</TR>\n");
              }
            reader.Close();
            form_str =
              "</TABLE>\n" +
              "<BR>\n";
            Response.Write(form_str);
            } // End else There is a previous order
          form_str =
            "<FORM ACTION=\"./dsbrowse.aspx\" METHOD=GET>\n" +
            "<INPUT TYPE=HIDDEN NAME=customerid VALUE=" + customerid + ">\n" +
            "<INPUT TYPE=SUBMIT VALUE=\"Start Shopping\">\n";
          Response.Write(form_str);
          } // End if (customerid != 0) 
        
        else // if customer not found
          {
          form_str = 
            "<H2>Username/password incorrect. Please re-enter your username and password</H2>\n" +
            "<FORM  ACTION=\"./dslogin.aspx\" METHOD=GET>\n" +
            "Username <INPUT TYPE=TEXT NAME=\"username\" VALUE='" + username + 
            "' SIZE=16 MAXLENGTH=24>\n" +    
            "Password <INPUT TYPE=PASSWORD NAME=\"password\" SIZE=16 MAXLENGTH=24>\n" + 
            "<INPUT TYPE=SUBMIT VALUE=\"Login\">\n" + 
            "</FORM>\n" + 
            "<H2>New customer? Please click New Customer</H2>\n" +
            "<FORM  ACTION=\"./dsnewcustomer.aspx\" METHOD=GET >\n" +
            "<INPUT TYPE=SUBMIT VALUE=\"New Customer\">\n" +
            "</FORM>\n";
          Response.Write(form_str);
          } // End if customer not found
        } // End if (!(username == null))
      
      else // if no username specified
        {
        form_str = 
          "<H2>Returning customer? Please enter your username and password</H2>\n" +
          "<FORM  ACTION=\"./dslogin.aspx\" METHOD=GET>\n" +
          "Username <INPUT TYPE=TEXT NAME=\"username\" SIZE=16 MAXLENGTH=24>\n" +    
          "Password <INPUT TYPE=PASSWORD NAME=\"password\" SIZE=16 MAXLENGTH=24>\n" + 
          "<INPUT TYPE=SUBMIT VALUE=\"Login\">\n" + 
          "</FORM>\n" + 
          "<H2>New customer? Please click New Customer</H2>\n" +
          "<FORM  ACTION=\"./dsnewcustomer.aspx\" METHOD=GET >\n" +
          "<INPUT TYPE=SUBMIT VALUE=\"New Customer\">\n" +
          "</FORM>\n";
        Response.Write(form_str);
        } // End if no username specified

      if (conn != null) conn.Close();
      if (conn2 != null) conn2.Close();
      Response.Write(dscommon.ds_html_footer());
      } // End Page_load        
    #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }
        
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {    
            this.Load += new System.EventHandler(this.Page_Load);
        }
        #endregion
    } // End Class dslogin
  } // End namespace ds2
