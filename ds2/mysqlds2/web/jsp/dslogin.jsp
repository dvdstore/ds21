<!-- 
dslogin.jsp JSP page that validates login to DVD store on mysql database
 
 Copyright 2005 Dell

 Written by Todd Muirhead/Dave Jaffe      Last modified: 9/14/05
--> 

<%@ page import="java.sql.*" %>
<%@ page import="java.math.*" %>
<%@ page import="java.io.*" %>
<%@ page import="java.lang.*" %>

<%@ page language="java" import="java.sql.*, java.math.*, java.io.*" %>

<HTML>
<HEAD><TITLE>DVD Store Login Page</TITLE></HEAD>
<BODY>
<FONT FACE=\"Arial\" COLOR=\"#0000FF\">
<H1 ALIGN=CENTER>DVD Store</H1>


<%
  String username = request.getParameter("username");      // get username and password
  String password = request.getParameter("password");
  String customerid = "novalue";

  if (username != null)          // if username and password have been entered, check if valid
    {
      try {
        Class.forName("com.mysql.jdbc.Driver");
      }
      catch (Exception e) {System.out.println("Error opening connection");}
      Connection conn = DriverManager.getConnection("jdbc:mysql://localhost/DS2?user=web&password=web"); 
      String query = "select CUSTOMERID FROM DS2.CUSTOMERS where USERNAME='" + username + "' and PASSWORD='" + password + "';";
      Statement userqueryStatement = conn.createStatement();
      ResultSet userqueryResult = userqueryStatement.executeQuery(query);
      if (userqueryResult.next())  // if user exists, then print out previous purchases and recommendataions
	  {
          customerid = userqueryResult.getString("CUSTOMERID");
          %>
          <H2>Welcome to the DVD Store - Click below to begin shopping</H2>
          <%
          query = "select PROD_ID FROM DS2.CUST_HIST where CUSTOMERID=" + customerid + " ORDER BY ORDERID DESC LIMIT 10;";         
          Statement prevpod_queryStatement = conn.createStatement();
          ResultSet prevprod_queryResult = prevpod_queryStatement.executeQuery(query);
          
          if (prevprod_queryResult.next())
            {
            %>
            <H3>Your previous purchases:</H3>
            <TABLE border=2>
            <TR>
            <TH>Title</TH>
            <TH>Actor</TH>
            <TH>People who liked this DVD also liked</TH>
            </TR>
            <TR>
            <%
            String prev_id, prev_title, prev_actor, recommend_title, recommend_prod_id;
            Statement prevproddetail_queryStatement;
            ResultSet prevproddetailResult;
            Statement relatedprod_queryStatement;
            ResultSet relatedprodResult;
            do
              {
              prevproddetailResult = null;
              relatedprodResult = null;
              prev_id = prevprod_queryResult.getString("PROD_ID");
              prevproddetail_queryStatement = conn.createStatement();
              prevproddetailResult = prevproddetail_queryStatement.executeQuery("select TITLE, ACTOR from DS2.PRODUCTS where PROD_ID=" + prev_id + ";");
              prevproddetailResult.next();
              prev_title = prevproddetailResult.getString("TITLE");
              prev_actor = prevproddetailResult.getString("ACTOR");
              relatedprod_queryStatement = conn.createStatement();
              relatedprodResult = relatedprod_queryStatement.executeQuery("select TITLE from DS2.PRODUCTS where PROD_ID= (select COMMON_PROD_ID from DS2.PRODUCTS where PROD_ID=" + prev_id + ");");
              relatedprodResult.next();
              recommend_title = relatedprodResult.getString("TITLE");
              %>
              <TR>
              <TD> <%= prev_title %> </TD>
              <TD> <%= prev_actor %> </TD>
              <TD> <%= recommend_title %> </TD>
              <%
              prevproddetail_queryStatement.close();
              relatedprod_queryStatement.close();
              } while (prevprod_queryResult.next()); // end while for populating table with recommended other items
            %>
            </TABLE>
            <BR>
            <%
           } // end if customer has previous purchases 
          %>
          <FORM ACTION="./dsbrowse.jsp" METHOD=GET>
          <INPUT TYPE=HIDDEN NAME=customerid VALUE= <%= customerid %> >
          <INPUT TYPE=SUBMIT VALUE="Start Shopping">
          <%
        }
        else    // else, if the username password did not exsist
          {
            %>
            <H2>Username/password incorrect. Please re-enter your username and password</H2>
            <FORM  ACTION="./dslogin.jsp" METHOD=GET>
            Username <INPUT TYPE=TEXT NAME="username" VALUE='<%= username %>' SIZE=16 MAXLENGTH=24>
            Password <INPUT TYPE=PASSWORD NAME="password" SIZE=16 MAXLENGTH=24>
            <INPUT TYPE=SUBMIT VALUE="Login"> 
            </FORM>
            <H2>New customer? Please click New Customer</H2>
            <FORM  ACTION="./dsnewcustomer.jsp" METHOD=GET >
            <INPUT TYPE=SUBMIT VALUE="New Customer"> 
            </FORM>
            <%
          }  // end else
      try {userqueryStatement.close();}
      catch (Exception e) {System.out.println("Error closing statement");}
      
    } // end if username not null
  else  // if no username, then must be first entry to site - give them the logon screen
    {
      %>
      <H2>Returning customer? Please enter your username and password</H2>
      <FORM  ACTION="./dslogin.jsp" METHOD=GET >
      Username <INPUT TYPE=TEXT NAME="username" SIZE=16 MAXLENGTH=24>
      Password <INPUT TYPE=PASSWORD NAME="password" SIZE=16 MAXLENGTH=24>
      <INPUT TYPE=SUBMIT VALUE="Login"> 
      </FORM>
      <H2>New customer? Please click New Customer</H2>
      <FORM  ACTION="./dsnewcustomer.jsp" METHOD=GET >
      <INPUT TYPE=SUBMIT VALUE="New Customer"> 
      </FORM>
      <%
    }

  
%>

<HR>
<P ALIGN=CENTER>Thank You for Visiting the DVD Store!</A></P>
<HR>
<P ALIGN=CENTER>Copyright &copy; 2005 Dell</P>
</FONT>
</BODY>
</HTML>

 
