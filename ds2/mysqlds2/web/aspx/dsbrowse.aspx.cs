
/*
* DVD Store Browse ASP Page - dsbrowse.aspx.cs
*
* Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
*
* Browses MySQL DVD store by author, title, or category
*
* Last Updated 6/30/05
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
  /// dsbrowse.aspx.cs: browses MySQL DVD Store database by author, title or category
  /// Copyright 2005 Dell, Inc.
  /// Written by Todd Muirhead/Dave Jaffe      Last modified: 6/7/05
    
  public class dsbrowse : System.Web.UI.Page
    {
    private void Page_Load(object sender, System.EventArgs e)
      {
      string customerid="", browsetype="", browse_title="", browse_actor="", 
        browse_category="", limit_num="";
      string form_str = null, conn_str = null, db_query = null, title = null;
      string[] categories = new string[]{"Action", "Animation", "Children", "Classics",
         "Comedy", "Documentary", "Drama", "Family", "Foreign", "Games", "Horror", "Music",
         "New", "Sci-Fi", "Sports", "Travel"};
      int i, j, item_len = 0, sel_item_len = 0;
      int[] item = new int[20];
      int[] selected_item = new int[20];
      MySqlCommand cmd = null;    
      MySqlConnection conn = null;
      MySqlDataReader reader = null;
      bool conn_open = false;
      
      Response.Write(dscommon.ds_html_header("DVD Store Browse Page"));     
         
      if (Request.QueryString.HasKeys())
        {
        if (Array.IndexOf(Request.QueryString.AllKeys, "customerid") >= 0) 
          customerid = Request.QueryString.GetValues("customerid")[0];
        if (Array.IndexOf(Request.QueryString.AllKeys, "browsetype") >= 0) 
          browsetype = Request.QueryString.GetValues("browsetype")[0];
        if (Array.IndexOf(Request.QueryString.AllKeys, "browse_title") >= 0) 
          browse_title = Request.QueryString.GetValues("browse_title")[0];
        if (Array.IndexOf(Request.QueryString.AllKeys, "browse_actor") >= 0)
          browse_actor = Request.QueryString.GetValues("browse_actor")[0];
        if (Array.IndexOf(Request.QueryString.AllKeys, "browse_category") >= 0) 
          browse_category = Request.QueryString.GetValues("browse_category")[0];
        if (Array.IndexOf(Request.QueryString.AllKeys, "limit_num") >= 0) 
          limit_num = Request.QueryString.GetValues("limit_num")[0];
        if (Array.IndexOf(Request.QueryString.AllKeys, "selected_item") >= 0)
          {
          sel_item_len = Request.QueryString.GetValues("selected_item").Length;
          for (i=0; i<sel_item_len; i++)
            { 
            selected_item[i] = 
              Convert.ToInt32(Request.QueryString.GetValues("selected_item")[i]);
            //Response.Write(i + " " + selected_item[i] + "<BR>");
            }
          }
            
        if (Array.IndexOf(Request.QueryString.AllKeys, "item") >= 0)
          {
          item_len = Request.QueryString.GetValues("item").Length;
          for (i=0; i<item_len; i++)
            { 
            item[i] = 
              Convert.ToInt32(Request.QueryString.GetValues("item")[i]);
            //Response.Write(i + " " + item[i] + "<BR>");
            }
          }
        }
      
      if (sel_item_len > 0)  // Add new selected items to shopping cart
        {
        for (i=0; i<sel_item_len; i++) item[item_len + i] = selected_item[i];
        item_len += sel_item_len;
        } 

      if (customerid!="") // Customerid is specified
        {
        form_str = "<H2>Select Type of Search</H2>\n";
        form_str = form_str + "<FORM ACTION='./dsbrowse.aspx' METHOD='GET'>\n";
        form_str = form_str + "<INPUT TYPE=RADIO NAME='browsetype' VALUE='title'";
          if(browsetype == "title") form_str = form_str + "CHECKED"; 
        form_str = form_str + ">Title  <INPUT NAME='browse_title' VALUE='"+ browse_title +
          "' TYPE=TEXT SIZE=15> <BR>\n";
        form_str = form_str + "<INPUT TYPE=RADIO NAME='browsetype' VALUE='actor'"; 
          if(browsetype == "actor") form_str = form_str + "CHECKED"; 
        form_str = form_str + ">Actor  <INPUT NAME='browse_actor' VALUE='" + browse_actor +
          "' TYPE=TEXT SIZE=15> <BR>\n";
        form_str = form_str + "<INPUT TYPE=RADIO NAME='browsetype' VALUE='category'"; 
          if(browsetype == "category") form_str = form_str + "CHECKED"; 
        form_str = form_str + ">Category\n";
        
        form_str = form_str + "<SELECT NAME='browse_category'>\n";
        for (i=0; i<categories.Length; i++)
          {
          j=i+1;
          if ((browse_category != "") && (j == Convert.ToInt32(browse_category)))
            form_str=form_str+ "  <OPTION VALUE="+j+" SELECTED>"+categories[i]+"</OPTION>\n";
          else
            form_str = form_str + "  <OPTION VALUE="+j+">"+categories[i]+"</OPTION>\n";
          }
        form_str = form_str + "</SELECT><BR>\n";
        form_str = form_str + "Number of search results to return\n";
        form_str = form_str + "<SELECT NAME='limit_num'>\n";
        for (i=1; i<11; i++)
          {
          if ((limit_num != "") && (i == Convert.ToInt32(limit_num)))
            form_str = form_str + "  <OPTION VALUE="+i+" SELECTED>"+i+"</OPTION>\n";
          else
            form_str = form_str + "  <OPTION VALUE="+i+">"+i+"</OPTION>\n";
          }
        form_str = form_str + "</SELECT><BR>\n";

        form_str = form_str + "<INPUT TYPE=HIDDEN NAME=customerid VALUE=" + 
          customerid + ">\n";

        for (i=0; i<item_len; i++) 
          form_str = form_str+"<INPUT TYPE=HIDDEN NAME='item' " + "VALUE=" + item[i] + ">\n";
        form_str = form_str + "<INPUT TYPE=SUBMIT VALUE='Search'>\n";
        form_str = form_str + "</FORM>\n";
        Response.Write(form_str);
        
        if (browsetype!="") // browsetype is specified - we are ready to query DB
          {
          conn_str = "Server=localhost;User ID=web;Password=web;Database=DS2;Pooling=false";
          conn = new MySqlConnection(conn_str);
          conn.Open();
          conn_open = true; 
          
          switch (browsetype)
            {
            case "title":
              db_query = "select * from PRODUCTS where MATCH (TITLE) AGAINST ('" +
                browse_title + "') LIMIT " + limit_num + ";";
            break;
            case "actor":
              db_query = "select * from PRODUCTS where MATCH (ACTOR) AGAINST ('" +
                browse_actor + "') LIMIT " + limit_num + ";";
            break;
            case "category":
              db_query = "select * from PRODUCTS where CATEGORY = " + 
                Convert.ToInt32(browse_category) + " and SPECIAL=1 LIMIT " + limit_num + ";";
            break;
            }
          //Response.Write("db_query= " + db_query);
          cmd = new MySqlCommand(db_query, conn);
          reader = cmd.ExecuteReader();
 
          if (!reader.HasRows) // No rows returned
            {
            Response.Write("<H2>No DVDs Found</H2>\n");
            }
          else  // Rows returned
            {
            form_str = "<BR>\n";
            form_str = form_str + "<H2>Search Results</H2>\n";
            form_str = form_str + "<FORM ACTION='./dsbrowse.aspx' METHOD=GET>\n";
            form_str = form_str + "<TABLE border=2>\n";
            form_str = form_str + "<TR>\n";
            form_str = form_str + "<TH>Add to Shopping Cart</TH>\n";
            form_str = form_str + "<TH>Title</TH>\n";
            form_str = form_str + "<TH>Actor</TH>\n";
            form_str = form_str + "<TH>Price</TH>\n";
            form_str = form_str + "</TR>\n";
            while (reader.Read())
              {
              form_str = form_str + " <TR>\n";
              form_str = form_str + "<TD><INPUT NAME=selected_item TYPE=CHECKBOX VALUE=" +
                reader.GetInt32(0) + "></TD>\n";
              form_str = form_str + "<TD>" + reader.GetString(2) + "</TD>\n";
              form_str = form_str + "<TD>" + reader.GetString(3) + "</TD>\n";
              form_str = form_str + "<TD>" + reader.GetString(4) + "</TD>\n";
              form_str = form_str + "</TR>\n";
              }      
            form_str = form_str + "</TABLE>\n";
            form_str = form_str + "<BR>\n";

            form_str = form_str + "<INPUT TYPE=HIDDEN NAME=customerid VALUE='" + 
              customerid + "'>\n";
            for (i=0; i<item_len; i++) form_str = form_str + 
              "<INPUT TYPE=HIDDEN NAME='item' " + "VALUE='" + item[i] + "'>\n";

            form_str = form_str + "<INPUT TYPE=SUBMIT VALUE='Update Shopping Cart'>\n";
            form_str = form_str + "</FORM>\n";
            Response.Write(form_str);
            } // End Rows returned
          reader.Close();
          } // End browsetype is specified - we are ready to query DB
        
        if (item_len> 0)  // Show shopping cart
          {
          form_str = "<H2>Shopping Cart</H2>\n";
          form_str = form_str + "<FORM ACTION='./dspurchase.aspx' METHOD='GET'>\n";
          form_str = form_str + "<TABLE border=2>\n";
          form_str = form_str + "<TR>";
          form_str = form_str + "<TH>Item</TH>";
          form_str = form_str + "<TH>Title</TH>";
          form_str = form_str + "</TR>\n";

          for (i=0; i<item_len; i++) 
            {
            j=i+1;
            db_query = "select TITLE from PRODUCTS where PROD_ID=" + item[i] + ";";
            if (!conn_open)
              {
              conn_str = "Server=localhost;User ID=web;Password=web;Database=DS2;Pooling=false";
              conn = new MySqlConnection(conn_str);
              conn.Open();
              }
            cmd = new MySqlCommand(db_query, conn);
            if(cmd.ExecuteScalar() != null) title = (string) cmd.ExecuteScalar();
            form_str = form_str + "<TD>" + j + "</TD><TD>" + title + "</TD></TR>\n";
            }
          form_str = form_str + "</TABLE>\n";
          form_str = form_str + "<BR>\n";

          form_str = form_str + "<INPUT TYPE=HIDDEN NAME=customerid VALUE=" + 
            customerid + ">\n";
          for (i=0; i<item_len; i++)
            form_str = form_str + "<INPUT TYPE=HIDDEN NAME='item' VALUE=" + item[i] + ">\n";
          form_str = form_str + "<INPUT TYPE=SUBMIT VALUE='Checkout'>\n";
          form_str = form_str + "</FORM>\n";
          Response.Write(form_str);
          } // End Show shopping cart
        } // End Customerid is specified

      else // Customerid is not specified
        {
        Response.Write
          ("<H2>You have not logged in - Please click below to Login to DVD Store</H2>\n");
        form_str = "<FORM ACTION='./dslogin.aspx' METHOD=GET>\n";
        form_str = form_str + "<INPUT TYPE=SUBMIT VALUE='Login'>\n";
        form_str = form_str + "</FORM>\n";
        Response.Write(form_str);
        }
      if (conn != null) conn.Close();
      Response.Write(dscommon.ds_html_footer());        
      } // End Page_Load

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
    
      } // End class dsbrowse
  } // End namespace ds2 
