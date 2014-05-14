<!-- 
dspurchase.jsp jsp page that purchases an order from the DVD store by entering the order into a database running on mysql

 Copyright 2005 Dell

 Written by Todd Muirhead/Dave Jaffe      Last modified: 9/14/05

--> 

<%@ page language="java" import="java.sql.*, java.math.*, java.io.*, java.text.*, java.util.* " %>

<HTML>
<HEAD><TITLE>DVD Store Purchase Page</TITLE></HEAD>
<BODY>
<FONT FACE=\"Arial\" COLOR=\"#0000FF\">
<H1 ALIGN=CENTER>DVD Store</H1>


<%

// get values from URL
String customerid = request.getParameter("customerid");
String confirmpurchase = request.getParameter("confirmpurchase");

String[] item = {"1"};
int item_length = 0;
if ( request.getParameterValues("item") != null)  // if there are items, populate the item array
  { 
    item = request.getParameterValues("item"); 
    item_length = item.length;
  }
String[] quan = new String[item.length];
if ( request.getParameterValues("quan") != null)  //  populate item quantities in the quan array
  {
    quan = request.getParameterValues("quan");
  }
else   // initiailze all quanities to 1 for first time
  {
    for (int i=0; i < quan.length; i++)
      {
        quan[i] = "1";
      }
  }
String[] drop = {"1"};                           // drop array is used to keep track of items that have 
int drop_length = 0;                             // been selected to be removed from shopping cart
if ( request.getParameterValues("drop") != null)
  { 
    drop = request.getParameterValues("drop"); 
    drop_length = drop.length;
  }


if (confirmpurchase == null)              // if not confirmiming purchase - print list to allow for
  {                                       // changes in quantities and for removal of items from list  
  %>
  <H2>Selected Items: specify quantity desired; click Purchase when finished</H2>
  <BR>
  <FORM ACTION='./dspurchase.jsp' METHOD=GET>
  <TABLE border=2>
  <TR>
  <TH>Item</TH>
  <TH>Quantity</TH>
  <TH>Title</TH>
  <TH>Actor</TH>
  <TH>Price</TH>
  <TH>Remove From Order?</TH>
  </TR>
  <%
  String prod_id, title, actor, price, amt, purchase_query;
  NumberFormat money = NumberFormat.getCurrencyInstance();
  money.setMinimumFractionDigits(2);
  money.setMaximumFractionDigits(2);
  double netamount = 0;
  int j = 0;   // j is used for printing out item number in list on web page, to account for dropped items
  for (int i=0; i< item_length; i++) 
    {   
    Arrays.sort(drop);          // sort the drop array, so that it can be searched for specific itemid
    if (Arrays.binarySearch(drop,item[i]) < 0)     // if item was not selected to be dropped
      {  
      j = j+1;
      purchase_query = "select PROD_ID, TITLE, ACTOR, PRICE from DS2.PRODUCTS where PROD_ID="+ item[i] + ";";
      try {Class.forName("com.mysql.jdbc.Driver");}
      catch (Exception e) {System.out.println("Error opening connection");}
      Connection conn = DriverManager.getConnection("jdbc:mysql:///DS2?user=web&password=web"); 
      Statement purchasequeryStatement = conn.createStatement();
      ResultSet purchasequeryResult = purchasequeryStatement.executeQuery(purchase_query);
      purchasequeryResult.next();
      prod_id = purchasequeryResult.getString("PROD_ID");
      title = purchasequeryResult.getString("TITLE");
      actor = purchasequeryResult.getString("ACTOR");
      price = purchasequeryResult.getString("PRICE");
      amt = money.format( Float.parseFloat(price));
      conn.close();
      %>    
      <TR>
      <TD ALIGN=CENTER><%= j%></TD>
      <INPUT NAME='item' TYPE='HIDDEN' VALUE='<%= item[i]%>'>
      <TD><INPUT NAME='quan' TYPE='TEXT' SIZE=10 VALUE='<%= quan[i]%>'></TD>
      <TD><%= title%></TD>
      <TD><%= actor%></TD>
      <TD ALIGN=RIGHT><%= amt%></TD>
      <TD ALIGN=CENTER><INPUT NAME='drop' TYPE=CHECKBOX VALUE='<%= item[i]%>'></TD>
      </TR>
      <%
      netamount = netamount + Integer.parseInt(quan[i]) * Float.parseFloat(price);
      }
    }    // end of for loop to print items in list
    
  double taxpct = 8.25;                              // double must be used for numberformat 
  double taxamount = netamount * taxpct/100.0;
  double totalamount = taxamount + netamount;
  amt = money.format(netamount);

  %>
  <TR><TD></TD><TD></TD><TD></TD><TD>Subtotal</TD><TD ALIGN=RIGHT><%= amt%></TD></TR>
  <%
  amt = money.format(taxamount);
  %>
  <TR><TD></TD><TD></TD><TD></TD><TD>Tax (<%= taxpct%>%)</TD><TD ALIGN=RIGHT><%= amt%></TD></TR>
  <%
  amt = money.format(totalamount);
  %>
  <TR><TD></TD><TD></TD><TD></TD><TD>Total</TD><TD ALIGN=RIGHT><%= amt%></TD></TR>
  </TABLE><BR>
    
  <INPUT TYPE=HIDDEN NAME=customerid VALUE='<%= customerid%>'>

  <INPUT TYPE=SUBMIT VALUE='Update and Recalculate Total'>
  </FORM><BR>

  <FORM ACTION='./dspurchase.jsp' METHOD=GET>
  <INPUT TYPE=HIDDEN NAME='confirmpurchase' VALUE='yes'>
  <INPUT TYPE=HIDDEN NAME='customerid' VALUE='<%= customerid%>'>
  <%
  for (int i=0; i < item_length; i++)   // loop for building item and quan list for a purchase 
    {
    Arrays.sort(drop);
    if (Arrays.binarySearch(drop,item[i]) < 0)  // if current item was not selected to be dropped from cart
      {
      %>
      <INPUT NAME='item' TYPE='HIDDEN' VALUE='<%= item[i]%>'>
      <INPUT NAME='quan' TYPE='HIDDEN' VALUE='<%= quan[i]%>'>
      <%
      }
    } 
  %>   
  <INPUT TYPE=SUBMIT VALUE='Purchase'>
  <%  
  }
else  // confirmpurchase=yes  => update ORDERS, ORDERLINES, INVENTORY and CUSTOMER table
  {
  %>
  <H2>Purchase complete</H2>
  <TABLE border=2>
  <TR>
  <TH>Item</TH>
  <TH>Quantity</TH>
  <TH>Title</TH>
  <TH>Actor</TH>
  <TH>Price</TH>
  </TR>
  <%
  String prod_id, title, actor, price, amt, purchase_query;
  NumberFormat money = NumberFormat.getCurrencyInstance();
  money.setMinimumFractionDigits(2);
  money.setMaximumFractionDigits(2);
  double netamount = 0;
  for (int i=0; i< item_length; i++)    // loop to print out list of items purchased on purchase confirmation page
    {
      purchase_query = "select PROD_ID, TITLE, ACTOR, PRICE from DS2.PRODUCTS where PROD_ID="+ item[i] + ";";
      try {Class.forName("com.mysql.jdbc.Driver");}
      catch (Exception e) {System.out.println("Error opening connection");}
      Connection conn = DriverManager.getConnection("jdbc:mysql:///DS2?user=web&password=web"); 
      Statement purchasequeryStatement = conn.createStatement();
      ResultSet purchasequeryResult = purchasequeryStatement.executeQuery(purchase_query);
      purchasequeryResult.next();
      prod_id = purchasequeryResult.getString("PROD_ID");
      title = purchasequeryResult.getString("TITLE");
      actor = purchasequeryResult.getString("ACTOR");
      price = purchasequeryResult.getString("PRICE");
      amt = money.format( Float.parseFloat(price));
      conn.close();
      %>    
      <TR>
      <TD ALIGN=CENTER><%= i+1%></TD>
      <INPUT NAME='item' TYPE='HIDDEN' VALUE='<%= item[i]%>'>
      <TD><INPUT NAME='quan' TYPE='TEXT' SIZE=10 VALUE="<%= quan[i]%> "></TD>
      <TD><%= title%></TD>
      <TD><%= actor%></TD>
      <TD ALIGN=RIGHT><%= amt%></TD>
      </TR>
      <%
      netamount = netamount + Integer.parseInt(quan[i]) * Float.parseFloat(price);
    }   // end of for loop to print out items 
  double taxpct = 8.25;
  double taxamount = netamount * taxpct/100.0;
  double totalamount = taxamount + netamount;
  amt = money.format(netamount);
  %>
  <TR><TD></TD><TD></TD><TD></TD><TD>Subtotal</TD><TD ALIGN=RIGHT><%= amt%></TD></TR>
  <%
  amt = money.format(taxamount);
  %>
  <TR><TD></TD><TD></TD><TD></TD><TD>Tax (<%= taxpct%>%)</TD><TD ALIGN=RIGHT><%= amt%></TD></TR>
  <%
  amt = money.format(totalamount);
  %>
  <TR><TD></TD><TD></TD><TD></TD><TD>Total</TD><TD ALIGN=RIGHT><%= amt%></TD></TR>
  </TABLE><BR>
  <%
                // The following section does the transactional commit of update
                // on orders, orderlines, customers, and inventory on the orderconn connection
  String purchase_insert_query;
  String orderid = "1";
  boolean success = true;
  DateFormat ds2dateformat = new SimpleDateFormat("yyyy-MM-dd");    // set date format to match format in db
  String currentdate = ds2dateformat.format(new java.util.Date());  // get current date in right format
  Connection orderconn = null;
  try {Class.forName("com.mysql.jdbc.Driver");}
  catch (Exception e) {System.out.println("Error opening connection");}
  try
    {
    NumberFormat totals = NumberFormat.getInstance();
    totals.setMaximumFractionDigits(2);
    totals.setMinimumFractionDigits(2);
    orderconn = DriverManager.getConnection("jdbc:mysql:///DS2?user=web&password=web"); 
    orderconn.setAutoCommit(false);               // tell connection to not commit until instructed
    Statement purchaseupdateStatement = orderconn.createStatement();
    purchase_insert_query = "INSERT into DS2.ORDERS (ORDERDATE, CUSTOMERID, NETAMOUNT, TAX, TOTALAMOUNT)" +
    " VALUES ( '" + currentdate + "'," + customerid + "," + totals.format(netamount) + "," + totals.format(taxamount) + "," + totals.format(totalamount) + ");";
    purchaseupdateStatement.executeUpdate(purchase_insert_query,Statement.RETURN_GENERATED_KEYS);
    ResultSet orderIDResult = purchaseupdateStatement.getGeneratedKeys();  // to get orderid that is autogenerated by db
    orderIDResult.next();
    orderid = orderIDResult.getString(1);

    int isolevel = orderconn.getTransactionIsolation();

       
  // To do: check $orderid and handle error if = 0

  // loop through purchased items and make inserts into orderdetails table     
    int h = 0;
    int j;
    String query;
    success = true;
    String p_query = "INSERT into DS2.ORDERLINES (ORDERLINEID, ORDERID, PROD_ID, QUANTITY, ORDERDATE) VALUES"; 
    String c_insert= "INSERT INTO DS2.CUST_HIST (CUSTOMERID, ORDERID, PROD_ID) VALUES ";
    while (h < item.length)
      {
      j = h+1;
      query = "SELECT QUAN_IN_STOCK, SALES FROM DS2.INVENTORY WHERE PROD_ID=" + item[h] + ";";
      Connection quanconn = DriverManager.getConnection("jdbc:mysql:///DS2?user=web&password=web"); 
      Statement quanquery = quanconn.createStatement();      // use quanconn instead of orderconn for simple quantity queries
      ResultSet quanResult = quanquery.executeQuery(query);
      quanResult.next();
      int curr_quan = quanResult.getInt("QUAN_IN_STOCK");
      int curr_sales = quanResult.getInt("SALES");
      int new_quan = curr_quan - Integer.parseInt(quan[h]);
      int new_sales = curr_sales + Integer.parseInt(quan[h]);
      if (new_quan < 0)                                     // if insufficient stock on hand - then flag failure
        {
        %> Insufficient quantity for prod <%= item[h]%> <%
        success = false;
        }
      else                                        // if quantity does exist, update to new level   
        {
        query = "UPDATE DS2.INVENTORY SET QUAN_IN_STOCK=" + new_quan + ", SALES=" + new_sales + " WHERE PROD_ID=" + item[h] + ";";
        purchaseupdateStatement.executeUpdate(query);
        }
              
      p_query = p_query + "(" + j + "," + orderid + "," + item[h] + "," + quan[h] + ",'" + currentdate + "'),";
      if (h < 10) 
        { c_insert = c_insert + "( " + customerid + "," + orderid + "," + item[h] + "),"; }
      h = h +1;
      } // End of while (!empty)
    
    p_query = p_query.substring(0,p_query.length()-1) + ";";
    c_insert = c_insert.substring(0,c_insert.length()-1) + " ;";

    purchaseupdateStatement.executeUpdate(p_query);  // Insert into orderlines
    purchaseupdateStatement.executeUpdate(c_insert);  // Update customers with recent purchases
    
    if ( success == true )           // if no errors were found, commit all 
      {orderconn.commit();}
    else                             
      {orderconn.rollback();}        // otherwise, rollback
    orderconn.close();
    }  //end of try for order entry transaction
  catch  (SQLException e)                  // if any SQL exceptions were thrown, rollback
    {
    %> SQL Exception entering order - <%= e.getMessage()%> <%
    if (orderconn != null) 
      {
      try { orderconn.rollback(); }
      catch (SQLException rbexception) { %> Error rolling back <%= rbexception.getMessage()%> <% }
      }
    }
  finally { orderconn.close(); }
    
  if (success == true)
    {
      // To Do: verify credit card purchase against a second database

      // get credit card info and print confirmation message
      String[] cctypes = {"MasterCard", "Visa", "Discover", "Amex", "Dell Preferred"};

      String cc_query = "select CREDITCARDTYPE, CREDITCARD, CREDITCARDEXPIRATION from DS2.CUSTOMERS where CUSTOMERID=" + customerid + ";";


      Connection queryconn = DriverManager.getConnection("jdbc:mysql:///DS2?user=web&password=web"); 
      Statement ccqueryStatement = queryconn.createStatement();
      ResultSet ccqueryResult = ccqueryStatement.executeQuery(cc_query);
      ccqueryResult.next();
      int creditcardtype = ccqueryResult.getInt("CREDITCARDTYPE");
      String creditcard = ccqueryResult.getString("CREDITCARD");
      String creditcardexpiration = ccqueryResult.getString("CREDITCARDEXPIRATION");
      queryconn.close();
      %>
      <H3>$<%= totalamount%> charged to credit card <%= creditcard%> ( <%= cctypes[creditcardtype-1]%>), expiration <%= creditcardexpiration%></H3><BR>
      <H2>Order Completed Successfully --- ORDER NUMBER:  <%= orderid%></H2><BR>
      <%
    }
  else
    {
      %>
      <H3>Insufficient stock - order not processed</H3>
      <%
    }
  } 



%>
<HR>
<P ALIGN=CENTER>Thank You for Visiting the DVD Store!</A></P>
<HR>
<P ALIGN=CENTER>Copyright &copy; 2005 Dell</P>
</FONT>
</BODY>
</HTML>

