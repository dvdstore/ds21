<!-- 
dsnewcustomer.jsp JSP page that creates new user entry in ds database running on mysql

 Copyright 2005 Dell

 Written by Todd Muirhead/Dave Jaffe      Last modified: 9/14/05
--> 


<%@ page language="java" import="java.sql.*, java.math.*, java.io.*, java.text.* " %>

<HTML>
<HEAD><TITLE>DVD Store New Customer Login</TITLE></HEAD>
<BODY>
<FONT FACE=\"Arial\" COLOR=\"#0000FF\">
<H1 ALIGN=CENTER>DVD Store</H1>


<%              // Get all parameters from URL, if not there then set to empty string
String firstname = request.getParameter("firstname"); if (firstname == null ) { firstname = "";}
String lastname = request.getParameter("lastname"); if (lastname == null ) { lastname = "";}
String address1 = request.getParameter("address1"); if (address1 == null ) { address1 = "";}
String address2 = request.getParameter("address2"); if (address2 == null ) { address2 = "";}
String city = request.getParameter("city"); if (city == null ) { city = "";}
String state = request.getParameter("state"); if (state == null ) { state = "";}
String zip = request.getParameter("zip"); if (zip == null ) { zip = "";}
String country = request.getParameter("country"); if (country == null ) { country = "";}
String gender = request.getParameter("gender"); if (gender == null ) { gender = "";}
String username = request.getParameter("username"); if (username == null ) { username = "";}
String password = request.getParameter("password"); if (password == null ) { password = "";}
String email = request.getParameter("email"); if (email == null ) { email = "";}
String phone = request.getParameter("phone"); if (phone == null ) { phone = "";}
String creditcardtype = request.getParameter("creditcardtype"); if (creditcardtype == null ) { creditcardtype = "";}
String creditcard = request.getParameter("creditcard"); if (creditcard == null ) { creditcard = "";}
String ccexpmon = request.getParameter("ccexpmon"); if (ccexpmon == null ) { ccexpmon = "";}
String ccexpyr = request.getParameter("ccexpyr"); if (ccexpyr == null ) { ccexpyr = "";}
String age = request.getParameter("age"); if (age == null ) { age = "";}
String income = request.getParameter("income"); if (income == null ) { income = "";}



//  Check to see if all required fields are complete, if they are complete then try to add new user 

if ( (firstname != "")&&(lastname != "")&&(address1 != "")&&(city != "")&&(country != "")&&(username != "")&&(password != "") )
  {
  try {Class.forName("com.mysql.jdbc.Driver");}                            // Connect to mysql db
  catch (Exception e) {System.out.println("Error opening connection");}
  Connection conn = DriverManager.getConnection("jdbc:mysql:///DS2?user=web&password=web"); 
  String query = "select COUNT(*) from DS2.CUSTOMERS where USERNAME='" + username + "';";
  Statement userqueryStatement = conn.createStatement();            // run query to check to see if username already exists
  ResultSet userqueryResult = userqueryStatement.executeQuery(query);
  userqueryResult.next();
  int check_username = userqueryResult.getInt("count(*)");    // get result from db into an int
  conn.close();               // close connection to db
  if (check_username > 0)  // if username did exist in database already, print form again for retry
    {
    %>
    <H2>Username already in use! Please try another username</H2>
    <%
    String[] countries = {"United States", "Australia", "Canada", "Chile", "China", "France", "Germany", "Japan", "Russia", "South Africa", "UK"};
    String[] cctypes = {"MasterCard", "Visa", "Discover", "Amex", "Dell Preferred"};
    String[] months = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};
    %>
    <FORM ACTION='./dsnewcustomer.jsp' METHOD=GET>
    Firstname <INPUT TYPE=TEXT NAME='firstname' VALUE='<%= firstname%>' SIZE=16 MAXLENGTH=50>* <BR>
    Lastname <INPUT TYPE=TEXT NAME='lastname' VALUE='<%= lastname%>' SIZE=16 MAXLENGTH=50>* <BR>
    Address1 <INPUT TYPE=TEXT NAME='address1' VALUE='<%= address1%>' SIZE=16 MAXLENGTH=50>* <BR>
    Address2 <INPUT TYPE=TEXT NAME='address2' VALUE='<%= address2%>' SIZE=16 MAXLENGTH=50> <BR>
    City <INPUT TYPE=TEXT NAME='city' VALUE='<%= city%>' SIZE=16 MAXLENGTH=50>* <BR>
    State <INPUT TYPE=TEXT NAME='state' VALUE='<%= state%>' SIZE=16 MAXLENGTH=50> <BR>
    Zipcode <INPUT TYPE=TEXT NAME='zip' VALUE='<%= zip%>' SIZE=16 MAXLENGTH=5> <BR>
    Country <SELECT NAME='country' SIZE=1>
    <%
    for (int i=0; i<(countries.length); i++)       // loop through countries for dropdown creation
      {
        if (countries[i] == country)
          { %>  <OPTION VALUE="<%= countries[i] %>" SELECTED> <%= countries[i]%></OPTION> <%}
        else
          { %>  <OPTION VALUE="<%= countries[i] %>"> <%=countries[i] %></OPTION> <%}
      }
    %>
    </SELECT>* <BR>
    Email <INPUT TYPE=TEXT NAME='email' VALUE='<%= email%>' SIZE=16 MAXLENGTH=50> <BR>
    Phone <INPUT TYPE=TEXT NAME='phone' VALUE='<%= phone%>' SIZE=16 MAXLENGTH=50> <BR>
    Credit Card Type
    <SELECT NAME='creditcardtype' SIZE=1>
    <% 
    int j = 0;
    for (int i=0; i<5; i++)                  // loop through creditcardtypes for dropdown creation
      {
        j = i + 1;
        if (Integer.toString(j) == creditcardtype)
          { %>  <OPTION VALUE="<%= j%>" SELECTED><%= cctypes[i]%></OPTION> <% }
        else
          { %>  <OPTION VALUE="<%= j%>"><%= cctypes[i]%></OPTION> <% }
      }
    %>  
    </SELECT>

    Credit Card Number <INPUT TYPE=TEXT NAME='creditcard' VALUE='<%= creditcard%>' SIZE=16 MAXLENGTH=50>

    Credit Card Expiration
    <SELECT NAME='ccexpmon' SIZE=1>
    <%
    for (int i=0; i<12; i++)                 // loop to create credit card expiration month dropdown
      {
        j = i+1;
        if (Integer.toString(j) == ccexpmon)
          { %>  <OPTION VALUE="<%= j%>" SELECTED><%= months[i]%></OPTION> <% }
        else
          { %>  <OPTION VALUE="<%= j%>"><%= months[i]%> </OPTION> <% }
      }
    %>  
    </SELECT>
    <SELECT NAME='ccexpyr' SIZE=1>
    <%
    int yr;
    for (int i=0; i<6; i++)             // loop to create credit card expiration year dropdown
      {
        yr = 2005 + i;
        if (Integer.toString(yr) == ccexpyr)
          { %>  <OPTION VALUE="<%= yr%>" SELECTED><%= yr%></OPTION> <% }
        else
          { %>  <OPTION VALUE="<%= yr%>"><%= yr%></OPTION> <% }
      }
    %>
    </SELECT><BR>

    Username <INPUT TYPE=TEXT NAME='username' VALUE='<%= username%>' SIZE=16 MAXLENGTH=50>* <BR>
    Password <INPUT TYPE='PASSWORD' NAME='password' VALUE='<%= password%>' SIZE=16 MAXLENGTH=50>* <BR>
    Age <INPUT TYPE=TEXT NAME='age' VALUE='<%= age%>' SIZE=3 MAXLENGTH=3> <BR>
    Income (\$US) <INPUT TYPE=TEXT NAME='income' VALUE='<%= income%>' SIZE=16 MAXLENGTH=50> <BR>
    Gender <INPUT TYPE=RADIO NAME='gender' VALUE="M" <% if(gender == "M") { %>CHECKED<% } %>> Male
           <INPUT TYPE=RADIO NAME='gender' VALUE="F" <% if(gender == "F") { %>CHECKED<% } %>> Female 
           <INPUT TYPE=RADIO NAME='gender' VALUE="?" <% if(gender == "?") { %>CHECKED<% } %>> Don't Know <BR>
    <INPUT TYPE='submit' VALUE='Submit New Customer Data'>
    </FORM>
    <%
    }
  else         // executed if the username requested did not exist - insert the new user
    {
    int region = 1;
    if (country != "US") 
      { region = 2; }
    if ( ccexpmon.length() < 2 )
      { ccexpmon = "0" + ccexpmon; } 
    String creditcardexpiration = ccexpyr + ccexpmon;
    	    
    if (zip == "") { zip = "0";}
    if (income == "") { income = "0";}

    String insert_newuser_query = "INSERT INTO DS2.CUSTOMERS (FIRSTNAME, LASTNAME, ADDRESS1, ADDRESS2, " + 
      "CITY, STATE, ZIP, COUNTRY, REGION, EMAIL, PHONE, CREDITCARDTYPE, CREDITCARD, CREDITCARDEXPIRATION," +
      " USERNAME, PASSWORD, AGE, INCOME, GENDER) " +
      " VALUES ('" + firstname + "','" + lastname + " ','" + address1 + " ','" + address2 + "','" + city + "','" + state + "','" + zip + "','" + country + "','" + 
      region + "','" + email + "','" + phone + "','" + creditcardtype + "','" + creditcard + "','" + creditcardexpiration + "','" + 
      username + "','" + password + "','" + age + "','" + income + "','" + gender + "');";
    
    try {   Class.forName("com.mysql.jdbc.Driver").newInstance();   }
    catch (Exception e) {System.out.println("Error opening connection");}
    Connection newuserconn = DriverManager.getConnection("jdbc:mysql://localhost/DS2?user=web&password=web"); 
    Statement userInsertStatement = newuserconn.createStatement(ResultSet.TYPE_FORWARD_ONLY, ResultSet.CONCUR_UPDATABLE);
    userInsertStatement.executeUpdate(insert_newuser_query,Statement.RETURN_GENERATED_KEYS);
          // the RETURN_GENERATED_KEYS option on the executeUpdate is needed for the autoincrement
          // customerid colum to be returned.  This value is then forwarded to each subsquent page
    ResultSet userInsertResult = userInsertStatement.getGeneratedKeys();
          //  Get the auto generated key into a result set
    userInsertResult.next();
    String customerid = userInsertResult.getString(1);   // get autocreated customerid into string
                                                         // it is passed below as a hidden value to next page  
    %> 
    <H2>New Customer Successfully Added.  Click below to begin shopping<H2>
    <FORM ACTION='./dsbrowse.jsp' METHOD=GET>
    <INPUT TYPE=HIDDEN NAME=customerid VALUE=<%= customerid%>>  
    <INPUT TYPE=SUBMIT VALUE='Start Shopping'>
    </FORM>
    <%
    }
  }
else   // this is executed if all of the required fields were not created
  {
  %>
  <H2>New Customer - Please Complete All Required Fields Below (marked with *)</H2>
    
  <%
  String[] countries = {"United States", "Australia", "Canada", "Chile", "China", "France", "Germany", "Japan", "Russia", "South Africa", "UK"};
  String[] cctypes = {"MasterCard", "Visa", "Discover", "Amex", "Dell Preferred"};
  String[] months = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};
  %>
  <FORM ACTION='./dsnewcustomer.jsp' METHOD=GET>
  Firstname <INPUT TYPE=TEXT NAME='firstname' VALUE='<%= firstname%>' SIZE=16 MAXLENGTH=50>* <BR>
  Lastname <INPUT TYPE=TEXT NAME='lastname' VALUE='<%= lastname%>' SIZE=16 MAXLENGTH=50>* <BR>
  Address1 <INPUT TYPE=TEXT NAME='address1' VALUE='<%= address1%>' SIZE=16 MAXLENGTH=50>* <BR>
  Address2 <INPUT TYPE=TEXT NAME='address2' VALUE='<%= address2%>' SIZE=16 MAXLENGTH=50> <BR>
  City <INPUT TYPE=TEXT NAME='city' VALUE='<%= city%>' SIZE=16 MAXLENGTH=50>* <BR>
  State <INPUT TYPE=TEXT NAME='state' VALUE='<%= state%>' SIZE=16 MAXLENGTH=50> <BR>
  Zipcode <INPUT TYPE=TEXT NAME='zip' VALUE='<%= zip%>' SIZE=16 MAXLENGTH=5> <BR>
  Country <SELECT NAME='country' SIZE=1>
  <%
  for (int i=0; i<(countries.length); i++)
    {
      if (countries[i] == country)
        { %>  <OPTION VALUE="<%= countries[i] %>" SELECTED> <%= countries[i]%></OPTION> <%}
      else
        { %>  <OPTION VALUE="<%= countries[i] %>"> <%=countries[i] %></OPTION> <%}
    }
  %>
  </SELECT>* <BR>
  Email <INPUT TYPE=TEXT NAME='email' VALUE='<%= email%>' SIZE=16 MAXLENGTH=50> <BR>
  Phone <INPUT TYPE=TEXT NAME='phone' VALUE='<%= phone%>' SIZE=16 MAXLENGTH=50> <BR>
  Credit Card Type
  <SELECT NAME='creditcardtype' SIZE=1>
  <% 
  int j = 0;
  for (int i=0; i<5; i++)
    {
      j = i + 1;
      if (Integer.toString(j) == creditcardtype)
        { %>  <OPTION VALUE="<%= j%>" SELECTED><%= cctypes[i]%></OPTION> <% }
      else
        { %>  <OPTION VALUE="<%= j%>"><%= cctypes[i]%></OPTION> <% }
    }
  %>  
  </SELECT>

  Credit Card Number <INPUT TYPE=TEXT NAME='creditcard' VALUE='<%= creditcard%>' SIZE=16 MAXLENGTH=50>

  Credit Card Expiration
  <SELECT NAME='ccexpmon' SIZE=1>
  <%
  for (int i=0; i<12; i++)
    {
      j = i+1;
      if (Integer.toString(j) == ccexpmon)
        { %>  <OPTION VALUE="<%= j%>" SELECTED><%= months[i]%></OPTION> <% }
      else
        { %>  <OPTION VALUE="<%= j%>"><%= months[i]%> </OPTION> <% }
    }
  %>  
  </SELECT>
  <SELECT NAME='ccexpyr' SIZE=1>
  <%
  int yr;
  for (int i=0; i<6; i++)
    {
      yr = 2005 + i;
      if (Integer.toString(yr) == ccexpyr)
        { %>  <OPTION VALUE="<%= yr%>" SELECTED><%= yr%></OPTION> <% }
      else
        { %>  <OPTION VALUE="<%= yr%>"><%= yr%></OPTION> <% }
    }
  %>
  </SELECT><BR>
  Username <INPUT TYPE=TEXT NAME='username' VALUE='<%= username%>' SIZE=16 MAXLENGTH=50>* <BR>
  Password <INPUT TYPE='PASSWORD' NAME='password' VALUE='<%= password%>' SIZE=16 MAXLENGTH=50>* <BR>
  Age <INPUT TYPE=TEXT NAME='age' VALUE='<%= age%>' SIZE=3 MAXLENGTH=3> <BR>
  Income (US) <INPUT TYPE=TEXT NAME='income' VALUE='<%= income%>' SIZE=16 MAXLENGTH=50> <BR>
  Gender <INPUT TYPE=RADIO NAME='gender' VALUE="M" <% if(gender == "M") { %>CHECKED<% } %>> Male
         <INPUT TYPE=RADIO NAME='gender' VALUE="F" <% if(gender == "F") { %>CHECKED<% } %>> Female 
         <INPUT TYPE=RADIO NAME='gender' VALUE="?" <% if(gender == "?") { %>CHECKED<% } %>> Don't Know <BR>
  <INPUT TYPE='submit' VALUE='Submit New Customer Data'>
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

