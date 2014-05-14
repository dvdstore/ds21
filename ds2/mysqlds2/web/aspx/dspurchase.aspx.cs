
/*
* DVD Store Purchase ASP Page - dspurchase.aspx.cs
*
* Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
*
* Handles purchase of DVDs for MySQL DVD Store database
* Checks inventories, adds records to ORDERS and ORDERLINES tables transactionally
*
* Last Updated 6/7/05
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
  /// dspurchase.aspx.cs: purchase application for MySQL DVD Store database
  /// Copyright 2005 Dell, Inc.
  /// Written by Todd Muirhead/Dave Jaffe      Last modified: 6/7/05
    
  public class dspurchase : System.Web.UI.Page
    {
    private void Page_Load(object sender, System.EventArgs e)
      {
      int MAX_ROWS = 100;
      string customerid="", confirmpurchase="";
      string form_str = null, conn_str = null, db_query = null;
      int i, j, item_len = 0, quan_len = 0, drop_len = 0, orderid = 0;
      int curr_quan, curr_sales, new_quan, new_sales;
      int[] item = new int[MAX_ROWS];
      int[] quan = new int[MAX_ROWS];
      int[] drop = new int[MAX_ROWS];
      double netamount = 0, totalamount, taxamount, taxpct;
      string amt, netamount_fmt, totalamount_fmt, taxamount_fmt, currentdate;
      string o_insert_query,c_insert_query;
      MySqlCommand cmd = null;    
          MySqlConnection conn = null;
      MySqlDataReader reader = null;
      MySqlTransaction trans = null;
      bool conn_open = false, success = false;
      string[] cctypes = new string[]{"MasterCard", "Visa", "Discover", "Amex", 
        "Dell Preferred"};

      
      Response.Write(dscommon.ds_html_header("DVD Store Purchase Page"));       
         
      if (Request.QueryString.HasKeys())
        {
        if (Array.IndexOf(Request.QueryString.AllKeys, "customerid") >= 0) 
          customerid = Request.QueryString.GetValues("customerid")[0];
        if (Array.IndexOf(Request.QueryString.AllKeys, "confirmpurchase") >= 0) 
          confirmpurchase = Request.QueryString.GetValues("confirmpurchase")[0];
        if (Array.IndexOf(Request.QueryString.AllKeys, "item") >= 0) 
          {
          //Response.Write("item<BR>");
          item_len = Request.QueryString.GetValues("item").Length;
          for (i=0; i<item_len; i++)
            { 
            item[i] = Convert.ToInt32(Request.QueryString.GetValues("item")[i]);
            //Response.Write(i + " " + item[i] + "<BR>");
            }
          }
            
        if (Array.IndexOf(Request.QueryString.AllKeys, "quan") >= 0) 
          {
          //Response.Write("quan<BR>");
          quan_len = Request.QueryString.GetValues("quan").Length;
          for (i=0; i<quan_len; i++)
            { 
            quan[i] = Convert.ToInt32(Request.QueryString.GetValues("quan")[i]);
            //Response.Write(i + " " + quan[i] + "<BR>");
            }
          }
            
        if (Array.IndexOf(Request.QueryString.AllKeys, "drop") >= 0) 
          {
          //Response.Write("drop<BR>");
          drop_len = Request.QueryString.GetValues("drop").Length;
          for (i=0; i<drop_len; i++)
            { 
            drop[i] = Convert.ToInt32(Request.QueryString.GetValues("drop")[i]);
            //Response.Write(i + " " + drop[i] + "<BR>");
            }
          }
        } // End if (Request.QueryString.HasKeys())
      
      if (confirmpurchase=="") // Confirm purchase not selected
        {
        form_str = "<H2>Selected Items: specify quantity desired;" +
          " click Purchase when finished</H2>\n";
        form_str = form_str + "<BR>\n";
        form_str = form_str + "<FORM ACTION='./dspurchase.aspx' METHOD='GET'>\n";
        form_str = form_str + "<TABLE border=2>\n";
        form_str = form_str + "<TR>\n";
        form_str = form_str + "<TH>Item</TH>\n";
        form_str = form_str + "<TH>Quantity</TH>\n";
        form_str = form_str + "<TH>Title</TH>\n";
        form_str = form_str + "<TH>Actor</TH>\n";
        form_str = form_str + "<TH>Price</TH>\n";
        form_str = form_str + "<TH>Remove From Order?</TH>\n";
        form_str = form_str + "</TR>\n";
      
        conn_str = "Server=localhost;User ID=web;Password=web;Database=DS2;Pooling=false";
        conn = new MySqlConnection(conn_str);
        conn.Open();
        conn_open = true; 
      
        j = 0;
        for (i=0; i<item_len; i++)
          {
          if ((drop_len==0) || (Array.IndexOf(drop,i+1)<0))
            {
            ++j;
            db_query = "select PROD_ID, TITLE, ACTOR, PRICE from DS2.PRODUCTS where " +
              "PROD_ID=" + item[i] + ";";
            //Response.Write("db_query= " + db_query);
            cmd = new MySqlCommand(db_query, conn);
            reader = cmd.ExecuteReader();
            reader.Read();
            form_str = form_str + " <TR>";
            form_str = form_str + "<TD ALIGN=CENTER>" + j + "</TD>";
            form_str = form_str + "<INPUT NAME='item' TYPE=HIDDEN VALUE=" +
              reader.GetInt32(0) + "></TD>";
            form_str = form_str + "<TD><INPUT NAME='quan' TYPE=TEXT SIZE=10 VALUE=" +
              Math.Max(1,quan[i]) + "></TD>";          
            form_str = form_str + "<TD>" + reader.GetString(1) + "</TD>";
            form_str = form_str + "<TD>" + reader.GetString(2) + "</TD>";
            amt = String.Format("{0:F2}", reader.GetDecimal(3));
            form_str = form_str + "<TD ALIGN=RIGHT>" + amt + "</TD>";                              
            form_str = form_str + "<TD ALIGN=CENTER><INPUT NAME='drop' TYPE=CHECKBOX " +
              "VALUE=" + j + "></TD>";
            form_str = form_str + "</TR>\n";
            
            netamount = netamount + Math.Max(1,quan[i])*(double)reader.GetDecimal(3);

            reader.Close();
            } // End if ((drop_len==0) || (Array.IndexOf(drop,i)<0))
          } // End for (i=0; i<item_len; i++)   

        taxpct = 8.25;
        taxamount = netamount * taxpct/100.0;
        totalamount = taxamount + netamount;
        amt = String.Format("{0:F2}", netamount);
        form_str = form_str + "<TR><TD></TD><TD></TD><TD></TD><TD>Subtotal</TD><TD ALIGN=" + 
          "RIGHT>" + amt + "</TD></TR>\n";
        amt = String.Format("{0:F2}", taxamount);
        form_str = form_str + "<TR><TD></TD><TD></TD><TD></TD><TD>Tax (" + taxpct + "%)</TD>"
          + "<TD ALIGN=RIGHT>" + amt + "</TD></TR>\n";
        amt = String.Format("{0:F2}", totalamount);
        form_str = form_str + "<TR><TD></TD><TD></TD><TD></TD><TD>Total</TD><TD ALIGN=" +
          "RIGHT>" + amt + "</TD></TR>\n";
        form_str = form_str + "</TABLE><BR>\n";
          
        form_str = form_str + "<INPUT TYPE=HIDDEN NAME=customerid VALUE="+customerid+">\n";
      
        form_str = form_str + "<INPUT TYPE=SUBMIT VALUE='Update and Recalculate Total'>\n";
        form_str = form_str + "</FORM><BR>\n";
        
        form_str = form_str + "<FORM ACTION='./dspurchase.aspx' METHOD='GET'>\n";
        form_str = form_str + "<INPUT TYPE=HIDDEN NAME=confirmpurchase VALUE='yes'>\n";
        form_str = form_str + "<INPUT TYPE=HIDDEN NAME=customerid VALUE="+customerid+">\n";
        for (i=0; i<item_len; i++)
          {
          if ((drop_len==0) || (Array.IndexOf(drop,i)<0))
            {
            form_str = form_str + "<INPUT NAME='item' TYPE=HIDDEN VALUE=" + item[i] + ">";
            form_str = form_str + "<INPUT NAME='quan' TYPE=HIDDEN VALUE=" + quan[i] + ">\n";
            }
          }      
        form_str = form_str + "<INPUT TYPE=SUBMIT VALUE='Purchase'>\n";

        Response.Write(form_str);
                 
        } // End Confirm purchase not selected
          
          else // Confirm purchase
            {
            form_str = form_str + "<H2>Purchase complete</H2>\n";
        form_str = form_str + "<TABLE border=2>";
        form_str = form_str + "<TR>";
        form_str = form_str + "<TH>Item</TH>";
        form_str = form_str + "<TH>Quantity</TH>";
        form_str = form_str + "<TH>Title</TH>";
        form_str = form_str + "<TH>Actor</TH>";
        form_str = form_str + "<TH>Price</TH>";
        form_str = form_str + "</TR>\n";
      
        j = 0;
        for (i=0; i<item_len; i++)
          {
          //quan[i] = max(1,quan[i]);
          ++j;
          db_query = "select PROD_ID, TITLE, ACTOR, PRICE from DS2.PRODUCTS where " +
            "PROD_ID=" + item[i] + ";";
          if (!conn_open)
            {
            conn_str = "Server=localhost;User ID=web;Password=web;Database=DS2;Pooling=false";
            conn = new MySqlConnection(conn_str);
            conn.Open();
            }
          cmd = new MySqlCommand(db_query, conn);
          reader = cmd.ExecuteReader();
          reader.Read();
          form_str = form_str + " <TR>";
          form_str = form_str + "<TD ALIGN=CENTER>" + j + "</TD>";
          form_str = form_str + "<INPUT NAME='item' TYPE=HIDDEN VALUE=" +
            reader.GetInt32(0) + "></TD>";
          form_str = form_str + "<TD><INPUT NAME='quan' TYPE=TEXT SIZE=10 VALUE=" +
              Math.Max(1,quan[i]) + "></TD>";          
          form_str = form_str + "<TD>" + reader.GetString(1) + "</TD>";
          form_str = form_str + "<TD>" + reader.GetString(2) + "</TD>";
          amt = String.Format("{0:F2}", reader.GetDecimal(3));
          form_str = form_str + "<TD ALIGN=RIGHT>" + amt + "</TD>";                              
          form_str = form_str + "<TD ALIGN=CENTER><INPUT NAME='drop' TYPE=CHECKBOX " +
            "VALUE=" + i + "></TD>";
          form_str = form_str + "</TR>\n";
            
          netamount = netamount + Math.Max(1,quan[i])*(double)reader.GetDecimal(3);

          reader.Close();
          } // End for (i=0; i<item_len; i++)   

        taxpct = 8.25;
        taxamount = netamount * taxpct/100.0;
        totalamount = taxamount + netamount;
        netamount_fmt = String.Format("{0:F2}", netamount);
        form_str = form_str + "<TR><TD></TD><TD></TD><TD></TD><TD>Subtotal</TD><TD ALIGN=" + 
          "RIGHT>" + netamount_fmt + "</TD></TR>\n";
        taxamount_fmt = String.Format("{0:F2}", taxamount);
        form_str = form_str + "<TR><TD></TD><TD></TD><TD></TD><TD>Tax (" + taxpct + "%)</TD>"
          + "<TD ALIGN=RIGHT>" + taxamount_fmt + "</TD></TR>\n";
        totalamount_fmt = String.Format("{0:F2}", totalamount);
        form_str = form_str + "<TR><TD></TD><TD></TD><TD></TD><TD>Total</TD><TD ALIGN=" +
          "RIGHT>" + totalamount_fmt + "</TD></TR>\n";
        form_str = form_str + "</TABLE><BR>\n";
     
        // Insert new order into ORDERS table
        currentdate = DateTime.Today.ToString("yyyy'-'MM'-'dd");
        trans = conn.BeginTransaction(IsolationLevel.RepeatableRead);
        db_query = "INSERT into DS2.ORDERS (ORDERDATE, CUSTOMERID, NETAMOUNT, TAX," +
          "TOTALAMOUNT) VALUES" + 
          "('" + currentdate + "'," + customerid + "," + netamount_fmt + "," + 
          taxamount_fmt + "," + totalamount_fmt + ");";
        //Response.Write(db_query + "<BR>");    
        cmd = new MySqlCommand(db_query, conn, trans);
        cmd.ExecuteNonQuery();
        
        db_query = "select LAST_INSERT_ID();";
        cmd = new MySqlCommand(db_query, conn);
        if (cmd.ExecuteScalar() != null) 
          orderid = Convert.ToInt32(cmd.ExecuteScalar().ToString());
        if (orderid > 0) success = true;
      
        // loop through purchased items and make inserts into orderdetails table 
        // (o_insert_query) and cust_hist table (ch_insert_query)
            
        o_insert_query = "INSERT into DS2.ORDERLINES (ORDERLINEID, ORDERID, PROD_ID," +
          "QUANTITY, ORDERDATE) VALUES "; 
        c_insert_query = "INSERT into DS2.CUST_HIST (CUSTOMERID, ORDERID, PROD_ID) VALUES "; 
        
        for (i=0; i<item_len; i++)
          {
          j = i+1;
          db_query = "SELECT QUAN_IN_STOCK, SALES FROM DS2.INVENTORY WHERE PROD_ID=" +
            item[i] + ";";
          cmd = new MySqlCommand(db_query, conn);
          reader = cmd.ExecuteReader();
          reader.Read();
          curr_quan = reader.GetInt32(0);
          curr_sales = reader.GetInt32(1);
          reader.Close();
          new_quan = curr_quan - quan[i];
          new_sales = curr_sales + quan[i];
          if (new_quan < 0)
            {
            form_str = form_str + "Insufficient quantity for product " + item[i] + "\n";
            success = false;
            }
          else   
            {
            db_query = "UPDATE DS2.INVENTORY SET QUAN_IN_STOCK=" + new_quan + ", SALES=" + 
              new_sales + " WHERE PROD_ID=" + item[i] + ";";
            cmd = new MySqlCommand(db_query, conn, trans);
            cmd.ExecuteNonQuery();
            }

          o_insert_query = o_insert_query + 
            "(" + j + "," +  orderid + "," + item[i] + "," + quan[i] + ",'" +
              currentdate + "'),";
          c_insert_query = c_insert_query + 
            "(" + customerid + "," +  orderid + "," + item[i] + "),";
          } // End of for (i=0; i<item_len; i++)
          
        o_insert_query = o_insert_query.Remove(o_insert_query.Length-1,1) + ";";
        //Response.Write(o_insert_query + "<BR>"); 
       
        c_insert_query = c_insert_query.Remove(c_insert_query.Length-1,1) + ";";
        //Response.Write(c_insert_query + "<BR>");
          
        cmd = new MySqlCommand(o_insert_query, conn, trans);
        if (cmd.ExecuteNonQuery()<0)
          {
          form_str = form_str + "Insert into ORDERLINES table failed: " + 
           "query= o_insert_query\n";
          success = false;
          }
     
        cmd = new MySqlCommand(c_insert_query, conn, trans);
        if (cmd.ExecuteNonQuery()<0)
          {
          form_str = form_str + "Insert into CUST_HIST table failed: " + 
           "query= c_insert_query\n";
          success = false;
          }
                    
        if (success)trans.Commit();
        else trans.Rollback();
     
        if (success)
          {
          // To Do: verify credit card purchase against a second database
          db_query = "select CREDITCARDTYPE, CREDITCARD, CREDITCARDEXPIRATION " +
            "from DS2.CUSTOMERS where CUSTOMERID=" + customerid + ";";
          cmd = new MySqlCommand(db_query, conn);
          reader = cmd.ExecuteReader();
          reader.Read();
          form_str = form_str + "<H3>" + totalamount_fmt + " charged to credit card " +
            reader.GetString(1) + "(" + cctypes[reader.GetInt32(0)-1] + "). " +
            "Expiration: " + reader.GetString(2) + "</H3><BR>\n";
          form_str = form_str + "<H2>Order Completed Successfully --- ORDER NUMBER:" +
            orderid + "</H2><BR>\n";
          }
        else
          {
          form_str = form_str + "<H3>Insufficient stock - order not processed</H3>\n";
          }
        Response.Write(form_str);      
        } // End Confirm purchase
          
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
    
      } // End class dspurchase
  } // End namespace ds2 
