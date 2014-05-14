
/*
* DVD Store New Customer ASP Page - dsnewcustomer.aspx.cs
*
* Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
*
* Prompts for new customer data; creates new entry in MySQL DVD Store CUSTOMERS table
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
  /// dsnewcustomer.aspx.cs: newcustomer login to MySQL DVD Store database
  /// Copyright 2005 Dell, Inc.
  /// Written by Todd Muirhead/Dave Jaffe      Last modified: 6/7/05
    
  public class dsnewcustomer : System.Web.UI.Page
    {
    private void Page_Load(object sender, System.EventArgs e)
      {
      string firstname="", lastname="", address1="", address2="", city="", state="",
        zip="", country="", email="", phone="", creditcardtype="",  creditcard="",
        ccexpmon="", ccexpyr="", username="", password="", age="",  income="", gender="";
      string form_str = null, conn_str = null, db_query = null, creditcardexpiration=null;
      int count = 999999, region=1, customerid=0;;
      MySqlCommand cmd = null;    
          MySqlConnection conn = null;
      
      Response.Write(dscommon.ds_html_header("DVD Store New Customer Page"));       
         
      if (Request.QueryString.HasKeys())
        {
        firstname =      Request.QueryString.GetValues("firstname")[0];
        lastname =       Request.QueryString.GetValues("lastname")[0];
        address1 =       Request.QueryString.GetValues("address1")[0];
        address2 =       Request.QueryString.GetValues("address2")[0];
        city =           Request.QueryString.GetValues("city")[0];
        state =          Request.QueryString.GetValues("state")[0];
        zip =            Request.QueryString.GetValues("zip")[0];
        country =        Request.QueryString.GetValues("country")[0];
        email =          Request.QueryString.GetValues("email")[0];
        phone =          Request.QueryString.GetValues("phone")[0];
        creditcardtype = Request.QueryString.GetValues("creditcardtype")[0];
        creditcard =     Request.QueryString.GetValues("creditcard")[0];
        ccexpmon =       Request.QueryString.GetValues("ccexpmon")[0];
        ccexpyr =        Request.QueryString.GetValues("ccexpyr")[0];
        username =       Request.QueryString.GetValues("username")[0];
        password =       Request.QueryString.GetValues("password")[0];
        age =            Request.QueryString.GetValues("age")[0];
        income =         Request.QueryString.GetValues("income")[0];
        gender =         Request.QueryString.GetValues("gender")[0];
        }
      
      if ((firstname!="") && (lastname!="") && (address1!="") && (city!="") && 
         (country!="") &&  (username!="") && (password!=""))
        {
        conn_str = "Server=localhost;User ID=web;Password=web;Database=DS2;Pooling=false";
        conn = new MySqlConnection(conn_str);
        conn.Open();       
        db_query="select count(*) FROM DS2.CUSTOMERS where USERNAME='" + username + "';";
        cmd = new MySqlCommand(db_query, conn);
        if(cmd.ExecuteScalar() != null) count = 
          Convert.ToInt32(cmd.ExecuteScalar().ToString());
        if (count > 0) // Non-unique username
          {
          Response.Write("<H2>Username already in use! Please try another username</H2>\n");
          form_str = dsnewcustomer_form(firstname,  lastname,  address1, address2,  city,
            state,  zip,  country,  email, phone,  creditcardtype,  creditcard,  ccexpmon,
            ccexpyr, username,  password,  age,  income,  gender);
          Response.Write(form_str);
          }
       
        else  // Unique username - insert customer data into CUSTOMERS table
          {
          if (country != "US") region = 2;
          creditcardexpiration = String.Format("{0:D4}/{1:D2}", ccexpyr, 
            Convert.ToInt32(ccexpmon));
          db_query = "call DS2.NEW_CUSTOMER('" + firstname + "','" + lastname + "','" +
            address1 + "','" + address2 + "','" + city + "','" + state + "','" + zip + 
            "','" + country + "','" + region + "','" + email + "','" + phone + "','" +
            creditcardtype + "','" + creditcard + "','" + creditcardexpiration + "','" +
            username + "','" + password + "','" + age +"','" + income + "','" + gender +
            "',@customerid_out);"; 

          //Response.Write(db_query);
          cmd = new MySqlCommand(db_query, conn);
          cmd.ExecuteNonQuery();
          db_query = "select @customerid_out;";
          cmd = new MySqlCommand(db_query, conn);
          if(cmd.ExecuteScalar() != null) customerid = 
            Convert.ToInt32(cmd.ExecuteScalar().ToString());

          form_str =
            "<H2>New Customer Successfully Added.  Click below to begin shopping<H2>\n";
          form_str = form_str + "<FORM ACTION='./dsbrowse.aspx' METHOD=GET>\n";
          form_str = form_str + "<INPUT TYPE=HIDDEN NAME=customerid VALUE="+customerid+
            ">\n";
          form_str = form_str + "<INPUT TYPE=SUBMIT VALUE='Start Shopping'>\n";
          form_str = form_str + "</FORM>\n";
          Response.Write(form_str);
          } // End else - unique username
        } // End if (firstname!="") ...

      else // Incomplete customer info
        {
        Response.Write
         ("<H2>New Customer - Please Complete All Required Fields Below (marked with *)</H2>");
        form_str = dsnewcustomer_form(firstname,  lastname,  address1, address2,  city,
        state,  zip,  country,  email, phone,  creditcardtype,  creditcard,  ccexpmon,
        ccexpyr, username,  password,  age,  income,  gender);
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
      
    
    private string dsnewcustomer_form(string firstname, string lastname, string address1,
      string address2, string city, string state, string zip, string country, string email,
      string phone, string creditcardtype, string creditcard, string ccexpmon,string ccexpyr,
      string username, string password, string age, string income, string gender)
      {
      int i, j, yr;
      bool gender_checked=false;
      string[] countries = new string[] {"United States", "Australia", "Canada", "Chile",
        "China", "France", "Germany", "Japan", "Russia", "South Africa", "UK"};
      string[] cctypes = new string[] {"MasterCard", "Visa", "Discover", "Amex",
        "Dell Preferred"};
      string[] months = new string[] {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug",
         "Sep", "Oct", "Nov", "Dec"};
      string form = "<FORM ACTION='./dsnewcustomer.aspx' METHOD='GET'>\n";
      form = form + "Firstname <INPUT TYPE=TEXT NAME='firstname' VALUE='" + firstname + 
        "' SIZE=16 MAXLENGTH=50>* <BR>\n";
      form = form + "Lastname <INPUT TYPE=TEXT NAME='lastname' VALUE='" + lastname + 
        "' SIZE=16 MAXLENGTH=50>* <BR>\n";
      form = form + "Address1 <INPUT TYPE=TEXT NAME='address1' VALUE='" + address1 +
        "' SIZE=16 MAXLENGTH=50>* <BR>\n";
      form = form + "Address2 <INPUT TYPE=TEXT NAME='address2' VALUE='" + address2 +
        "' SIZE=16 MAXLENGTH=50> <BR>\n";
      form = form + "City <INPUT TYPE=TEXT NAME='city' VALUE='" + city +
        "' SIZE=16 MAXLENGTH=50>* <BR>\n";
      form = form + "State <INPUT TYPE=TEXT NAME='state' VALUE='" + state +
        "' SIZE=16 MAXLENGTH=50>* <BR>\n";
      form = form + "Zipcode <INPUT TYPE=TEXT NAME='zip' VALUE='" + zip +
        "' SIZE=16 MAXLENGTH='5'> <BR>\n";
      
      form = form + "Country <SELECT NAME='country' SIZE=1>\n";
      for (i=0; i<countries.Length; i++)
        {
        if (countries[i] == country)
          form = form + "  <OPTION VALUE='" + countries[i] + "' SELECTED>" + countries[i] +
            "</OPTION>\n";
        else
          form = form + "  <OPTION VALUE='" + countries[i] + "'>" + countries[i] +
            "</OPTION>\n";
        }
      form = form + "</SELECT>* <BR>\n";
      form = form + "Email <INPUT TYPE=TEXT NAME='email' VALUE='" + email +
        "' SIZE=16 MAXLENGTH=50> <BR>\n";
      form = form + "Phone <INPUT TYPE=TEXT NAME='phone' VALUE='" + phone + 
        "' SIZE=16 MAXLENGTH=50> <BR>\n";
      
      form = form + "Credit Card Type "; 
      form = form + "<SELECT NAME='creditcardtype' SIZE=1>\n";
      for (i=0; i<5; i++)
        {
        j = i + 1;
        if ((creditcardtype!= "") && (j == Convert.ToInt32(creditcardtype)))
          form = form + "  <OPTION VALUE='" + j + "' SELECTED>" + cctypes[i] + "</OPTION>\n";
        else
          form = form + "  <OPTION VALUE='" + j + "'>" + cctypes[i] + "</OPTION>\n";
        }
      form = form + "</SELECT>\n";

      form = form + "  Credit Card Number <INPUT TYPE=TEXT NAME='creditcard' VALUE='" +
        creditcard + "' SIZE=16 MAXLENGTH=50>\n";

      form = form + "  Credit Card Expiration "; 
      form = form + "<SELECT NAME='ccexpmon' SIZE=1>\n";
      for (i=0; i<12; i++)
        {
        j = i+1;
        if ((ccexpmon != "") && (j == Convert.ToInt32(ccexpmon)))
          form = form + "  <OPTION VALUE='" + j + "' SELECTED>" + months[i] + "</OPTION>\n";
        else
          form = form + "  <OPTION VALUE='" + j + "'>" + months[i] + "</OPTION>\n";
        }
      form = form + "</SELECT>\n";
      form = form + "<SELECT NAME='ccexpyr' SIZE=1>\n";
      for (i=0; i<6; i++)
        {
        yr = 2008 + i;
        if ((ccexpyr != "") && (yr == Convert.ToInt32(ccexpyr)))
          form = form + "  <OPTION VALUE='"+ yr + "' SELECTED>" + yr + "</OPTION>\n";
        else
          form = form + "  <OPTION VALUE='" + yr + "'>" + yr + "</OPTION>\n";
        }
      form = form + "</SELECT><BR>\n";

      form = form + "Username <INPUT TYPE=TEXT NAME='username' VALUE='" + username + 
        "' SIZE=16 MAXLENGTH=50>* <BR>\n";
      form = form + "Password <INPUT TYPE=PASSWORD NAME='password' VALUE='" + password +
        "' SIZE=16 MAXLENGTH=50>* <BR>\n";
      form = form + "Age <INPUT TYPE=TEXT NAME='age' VALUE='" + age +
        "' SIZE=3 MAXLENGTH=3> <BR>\n";
      form = form + "Income ($US) <INPUT TYPE=TEXT NAME='income' VALUE='" + income + 
        "' SIZE=16 MAXLENGTH=50> <BR>\n";
      form = form + "Gender <INPUT TYPE=RADIO NAME='gender' VALUE=\"M\" "; 
        if (gender == "M") {form = form + "CHECKED"; gender_checked = true;}
        form = form + "> Male \n";
      form = form + "       <INPUT TYPE=RADIO NAME='gender' VALUE=\"F\" "; 
        if (gender == "F") {form = form + "CHECKED"; gender_checked = true;}
        form = form + "> Female \n";
      form = form + "       <INPUT TYPE=RADIO NAME='gender' VALUE=\"?\" ";
         if ((gender == "?") || (gender_checked==false)) form = form + "CHECKED";
         form = form + "> Don't Know <BR>\n";
         
      form = form + "<INPUT TYPE=SUBMIT VALUE='Submit New Customer Data'>\n";
      form = form + "</FORM>\n";
      return form;
      } // End dsnewcustomer_form
    } // End class dsnewcustomer
  } // End namespace ds2 
