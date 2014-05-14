<%@ page contentType="text/html" %> 
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="fn" uri="http://java.sun.com/jsp/jstl/functions" %>

<HTML>
<HEAD><TITLE>DVD Store Login Page</TITLE></HEAD>
<BODY>
<FONT FACE=\"Arial\" COLOR=\"#0000FF\">
<H1 ALIGN=CENTER>DVD Store</H1>

<c:choose>

<c:when test="${IsUserNameNull==0}">
	<c:choose>
			
		<c:when test="${IsUserExists==1}">
			<H2>Welcome to the DVD Store - Click below to begin shopping</H2>
			
			<c:choose>
			
				<c:when test= "${IsPrevPurchaseExists==1}">
					<H3>Your previous purchases:</H3>
					<TABLE border=2>
			            <TR>
			            <TH>Title</TH>
			            <TH>Actor</TH>
			            <TH>People who liked this DVD also liked</TH>
			            </TR>
						<c:forEach items="${PrevPurchaseList}" var="PrevPurchase">
				            <TR>
				            	<TD>${PrevPurchase.prev_title}</TD>
				            	<TD>${PrevPurchase.prev_actor}</TD>
				            	<TD>${PrevPurchase.recommend_title}</TD>		            	
				            </TR>
						</c:forEach>					
		        	</TABLE>
            		<BR>
            		<FORM ACTION="./dsbrowse.html" METHOD=GET>
			        <INPUT TYPE=HIDDEN NAME=customerid VALUE="${customerid}">
			        <INPUT TYPE=SUBMIT VALUE="Start Shopping">
			        </FORM>                  		
				</c:when>
								
				<c:otherwise>	
					<FORM ACTION="./dsbrowse.html" METHOD=GET>
			        <INPUT TYPE=HIDDEN NAME=customerid VALUE="${customerid}">
			        <INPUT TYPE=SUBMIT VALUE="Start Shopping">
			        </FORM>			
				</c:otherwise>
				
			</c:choose>
				
		</c:when>
		
		<c:otherwise>
			<H2>Username/password incorrect. Please re-enter your username and password</H2>
            <FORM  ACTION="./dslogin.html" METHOD=GET>
            Username <INPUT TYPE=TEXT NAME="username" VALUE=${username} SIZE=16 MAXLENGTH=24>
            Password <INPUT TYPE=PASSWORD NAME="password" SIZE=16 MAXLENGTH=24>
            <INPUT TYPE=SUBMIT VALUE="Login"> 
            </FORM>
            <H2>New customer? Please click New Customer</H2>
            <FORM  ACTION="./dsnewcustomer.html" METHOD=GET >
            <INPUT TYPE=SUBMIT VALUE="New Customer"> 
            </FORM>
		</c:otherwise>
		
	</c:choose>
	
</c:when>

<c:otherwise>
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
</c:otherwise>

</c:choose>

<HR>
<P ALIGN=CENTER>Thank You for Visiting the DVD Store!</A></P>
<HR>
<P ALIGN=CENTER>Copyright &copy; 2005 Dell</P>
</FONT>
</BODY>
</HTML>
