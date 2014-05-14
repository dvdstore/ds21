<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="form" uri="http://www.springframework.org/tags/form" %>
<HTML>
<HEAD><TITLE>DVD Store Login Page</TITLE></HEAD>
<BODY>
<FONT FACE="Arial" COLOR="#0000FF">
<H1 ALIGN=CENTER>DVD Store</H1>
  <H2>Returning customer? Please enter your username and password</H2>
  <FORM  ACTION="./dslogin.html" METHOD=GET >
  Username <INPUT TYPE=TEXT NAME="username" SIZE=16 MAXLENGTH=24>
  Password <INPUT TYPE=PASSWORD NAME="password" SIZE=16 MAXLENGTH=24>
  <INPUT TYPE=SUBMIT VALUE="Login"> 
  </FORM>
  <H2>New customer? Please click New Customer</H2>
  <FORM  ACTION="./dsnewcustomer.html" METHOD=GET >
  <INPUT TYPE=SUBMIT VALUE="New Customer"> 
  </FORM>
<HR>
<P ALIGN=CENTER>Thank You for Visiting the DVD Store!</A></P>
<HR>
<P ALIGN=CENTER>Copyright &copy; 2005 Dell</P>
</FONT>
</BODY>
</HTML>
