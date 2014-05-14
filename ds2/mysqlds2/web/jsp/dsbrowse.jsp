<!-- 
dsbrowse.jsp jsp page that browses DVD store by author, title, or category on mysql database
 
It also collects selected items into a list and allows customer to purchase them

 Copyright 2005 Dell

 Written by Todd Muirhead/Dave Jaffe      Last modified: 9/14/05
--> 

<%@ page language="java" import="java.sql.*, java.math.*, java.io.*, java.text.* " %>

<HTML>
<HEAD><TITLE>DVD Store Browse Page</TITLE></HEAD>
<BODY>
<FONT FACE=\"Arial\" COLOR=\"#0000FF\">
<H1 ALIGN=CENTER>DVD Store</H1>


<%
String customerid = request.getParameter("customerid");
String browsetype = request.getParameter("browsetype");
String browse_title = request.getParameter("browse_title");
String browse_actor = request.getParameter("browse_actor");
String browse_category = request.getParameter("browse_category");
String limit_num = request.getParameter("limit_num");
int new_item_length = 0;
int item_length = 0;
int selected_item_length = 0;
if (browse_title == null)
  browse_title = "";
if (browse_actor == null)
  browse_actor = "";
String[] item = {"1"};
String[] selected_item = {"5000"};
if ( request.getParameterValues("item") != null)  // check to see if there are any items
 { 
    item = request.getParameterValues("item");   // populate item array
    item_length = item.length;
    new_item_length = item.length;
  }
if (request.getParameterValues("selected_item") != null )   // check to see if there are any new selected items
  { 
    selected_item = request.getParameterValues("selected_item");   // populate selected items array
    selected_item_length = selected_item.length;
    new_item_length = new_item_length + selected_item.length;
  }

// if (new_item_length > 0)
    String[] new_item = new String[new_item_length];   // initialize size of array to hold both existing items in cart and new selected items

String[] categories = {"Action", "Animation", "Children", "Classics", "Comedy", "Documentary", "Drama", "Family", "Foreign", 
  "Games", "Horror", "Music", "New", "Sci-Fi", "Sports", "Travel"};

if (customerid == null)   // check to see if the user has logged in
  {
  %>
  <H2>You have not logged in - Please click below to Login to DVD Store</H2>
  <FORM ACTION='./dslogin.jsp' METHOD=GET>
  <INPUT TYPE=SUBMIT VALUE='Login'>
  </FORM>
  <%
  }  // end of if customerid is blank
else
  {
  for (int i=0; i< item_length; i++) new_item[i] = item[i];  // Put existing items into new item array
  if ( selected_item_length > 0 )                          // Add new selected items to end new item array
    {
    for (int i=0; i< selected_item_length; i++) new_item[item_length+i] = selected_item[i];
    }
 

  %>
  <H2>Select Type of Search</H2>

  <FORM ACTION='./dsbrowse.jsp' METHOD=GET>

  <INPUT NAME='browsetype' TYPE=RADIO VALUE='title'<% if(browsetype == "title") { %> CHECKED <% } %>
  >Title  <INPUT NAME='browse_title' VALUE='<%= browse_title%>' TYPE=TEXT SIZE=15> <BR>
  <INPUT NAME='browsetype' TYPE=RADIO VALUE='actor'<% if(browsetype == "actor") { %> CHECKED <% } %> 
  >Actor  <INPUT NAME='browse_actor' VALUE='<%= browse_actor%>' TYPE=TEXT SIZE=15> <BR>
  <INPUT NAME='browsetype' TYPE=RADIO VALUE='category'<% if(browsetype == "category") { %> CHECKED <% } %>>Category
  <SELECT NAME='browse_category'>
  <% 
  int j;
  for (int i=0; i < categories.length; i++)        // loop to create category dropdown
    {
      j=i+1;
      if (Integer.toString(j) == browse_category)
        { %>  <OPTION VALUE=<%= j%> SELECTED><%= categories[i]%></OPTION><% }
      else
        { %>  <OPTION VALUE=<%= j%>><%= categories[i]%></OPTION><% }
    }
  %>
  </SELECT><BR>

  Number of search results to return
  <SELECT NAME='limit_num'>
  <%
  for (int i=1; i<11; i++)                   // loop to create number of results to return dropdown
    {
    if (Integer.toString(i) == limit_num)
      { %>  <OPTION VALUE=<%= i%> SELECTED><%= i%></OPTION><% }
    else
      { %>  <OPTION VALUE=<%= i%>><%= i%></OPTION><% }
    }
  %>
  </SELECT><BR>

  <INPUT TYPE=HIDDEN NAME='customerid' VALUE=<%= customerid%>>
  <% 
  for (int i=0; i< new_item_length; i++)
    {
      %><INPUT TYPE=HIDDEN NAME='item' VALUE=<%= new_item[i]%>> <%
    }
  %>
  <INPUT TYPE=SUBMIT VALUE='Search'>
  </FORM>
  <%
  }
if (browsetype != null)          // if a browse type of either TITLE AUTHOR or CATEGORY was selected
  {
  try {Class.forName("com.mysql.jdbc.Driver");}
  catch (Exception e) {System.out.println("Error opening connection");}
  Connection conn = DriverManager.getConnection("jdbc:mysql:///DS2?user=web&password=web");  // connect to mysql 
  String browse_query = "";     // init string variable
  switch (browsetype.charAt(0))  // switch on browsetype that was selected
    {
    case 't':
      browse_query = "select * from PRODUCTS where MATCH (TITLE) AGAINST ('" + browse_title + "') LIMIT " + limit_num + ";";
      break;
    case 'a':
      browse_query = "select * from PRODUCTS where MATCH (ACTOR) AGAINST ('" + browse_actor + "') LIMIT " + limit_num + ";";
      break;
    case 'c':
      browse_query = "select * from PRODUCTS where CATEGORY = " + browse_category + " and SPECIAL=1 LIMIT " + limit_num + ";";
      break;
    }
   // query syntax used for TITLE and ACTOR is specific to doing a fulltext search on the PRODUCTS MyISAM table
  Statement browseStatement = conn.createStatement();
  ResultSet browseResult = browseStatement.executeQuery(browse_query);    // submit browse query to mysql                             
  if (!browseResult.next())
    {
      %> <H2>No DVDs Found</H2> <%
    }
  else  // search results were returned
    {
      %>
      <BR>
      <H2>Search Results</H2>
      <FORM ACTION='./dsbrowse.jsp' METHOD=GET>
      <TABLE border=2>
      <TR>
      <TH>Add to Shopping Cart</TH>
      <TH>Title</TH>
      <TH>Actor</TH>
      <TH>Price</TH>
      </TR>
      <%
      String browse_result_row_prod_id, browse_result_row_title, browse_result_row_actor, browse_result_row_price;
      do
        {
          browse_result_row_prod_id = browseResult.getString("PROD_ID");
          browse_result_row_title = browseResult.getString("TITLE");
          browse_result_row_actor = browseResult.getString("ACTOR");
          browse_result_row_price = browseResult.getString("PRICE");
          %>
          <TR>
          <TD><INPUT NAME='selected_item' TYPE=CHECKBOX VALUE=<%= browse_result_row_prod_id%>></TD>
          <TD><%= browse_result_row_title%></TD>
          <TD><%= browse_result_row_actor%></TD>
          <TD><%= browse_result_row_price%></TD>
          </TR>
          <%
        } while (browseResult.next());     // loop to display search results in HTML table
      %>
      </TABLE>
      <BR>

      <INPUT TYPE=HIDDEN NAME='customerid' VALUE=<%= customerid%>>
      <% for (int i=0; i< item_length; i++) { %><INPUT TYPE=HIDDEN NAME='item' VALUE=<%= item[i]%>><% } %>
      <INPUT TYPE=SUBMIT VALUE='Update Shopping Cart'>
      </FORM>
      <%
    }   // end of else
  }  // end of if browsetype not equal to null


if ( new_item_length > 0 )  // If the shopping cart is not empty then - Show shopping cart
  {
  %>
  <H2>Shopping Cart</H2>
  <FORM ACTION='./dspurchase.jsp' METHOD=GET>
  <TABLE border=2>
  <TR>
  <TH>Item</TH>
  <TH>Title</TH>
  </TR>
  <%
  int j;
  String title, query;
  for (int i=0; i< new_item_length; i++) 
    {
    j=i+1;
    query = "select TITLE from PRODUCTS where PROD_ID=" + new_item[i] + ";";
    try {Class.forName("com.mysql.jdbc.Driver");}
    catch (Exception e) {System.out.println("Error opening connection");}
    Connection conn = DriverManager.getConnection("jdbc:mysql:///DS2?user=web&password=web"); 
    Statement titlebrowseStatement = conn.createStatement();
    ResultSet titlebrowseResult = titlebrowseStatement.executeQuery(query);
    titlebrowseResult.next();
    title = titlebrowseResult.getString("TITLE");
    conn.close();
    %>    
    <TD><%= j%></TD><TD><%= title%></TD></TR>
    <%
    }
  %>
  </TABLE>
  <BR>
  <INPUT TYPE=HIDDEN NAME='customerid' VALUE=<%= customerid%>>
  <INPUT TYPE=HIDDEN NAME='num_of_items' VALUE=<%= new_item_length%>>
  <%
  for (int i=0; i< new_item_length; i++) { %><INPUT TYPE=HIDDEN NAME='item' VALUE=<%= new_item[i]%>> <% } %>
  <INPUT TYPE=SUBMIT VALUE='Checkout'>
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

