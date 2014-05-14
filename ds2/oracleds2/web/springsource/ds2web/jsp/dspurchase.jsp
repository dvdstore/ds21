<!-- 
dspurchase.jsp jsp page that purchases an order from the DVD store by entering the order into a database running on Oracle

 Copyright 2005 Dell

 Written by Todd Muirhead/Dave Jaffe      Last modified: 9/14/05
 Converted to Spring implementation by GSK 	Last Modified: 07/06/2010
--> 

<%@ page contentType="text/html" %> 
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="fn" uri="http://java.sun.com/jsp/jstl/functions" %>

<HTML>
<HEAD><TITLE>DVD Store Purchase Page</TITLE></HEAD>
<BODY>
<FONT FACE=\"Arial\" COLOR=\"#0000FF\">
<H1 ALIGN=CENTER>DVD Store</H1>

<c:choose>
	<c:when test="${IsConfirmPurchaseNull==1}">
		<H2>Selected Items: specify quantity desired; click Purchase when finished</H2>
		<BR>
		<FORM ACTION='./dspurchase.html' METHOD=GET>
			<TABLE border=2>
				<TR>
					<TH>Item</TH>
					<TH>Quantity</TH>
					<TH>Title</TH>
					<TH>Actor</TH>
					<TH>Price</TH>
					<TH>Remove From Order?</TH>
				</TR>
				<c:forEach items="${CheckOutItemList}" var="CheckOutItem" varStatus="istat">
					<c:set var="varitemno" value="${istat.count}"></c:set>
					<TR>
				      <TD ALIGN=CENTER>${varitemno}</TD>
				      <INPUT NAME="item" TYPE="HIDDEN" VALUE="${CheckOutItem.item}">
				      <TD><INPUT NAME="quan" TYPE="TEXT" SIZE=10 VALUE="${CheckOutItem.quantity}"></TD>
				      <TD>${CheckOutItem.title}</TD>
				      <TD>${CheckOutItem.actor}</TD>
				      <TD ALIGN=RIGHT>${CheckOutItem.price}</TD>
				      <TD ALIGN=CENTER><INPUT NAME="drop" TYPE=CHECKBOX VALUE="${CheckOutItem.item}"></TD>
			        </TR>
				</c:forEach>
				
				<TR><TD></TD><TD></TD><TD></TD><TD>Subtotal</TD><TD ALIGN=RIGHT>${netamount}</TD></TR>
				<TR><TD></TD><TD></TD><TD></TD><TD>Tax (${taxpct}%)</TD><TD ALIGN=RIGHT>${taxamount}</TD></TR>
				<TR><TD></TD><TD></TD><TD></TD><TD>Total</TD><TD ALIGN=RIGHT>${totalamount}</TD></TR>
  			</TABLE><BR>
  			
  			<INPUT TYPE=HIDDEN NAME=customerid VALUE="${customerid}">

  			<INPUT TYPE=SUBMIT VALUE="Update and Recalculate Total">
  		</FORM><BR>
  			
		<FORM ACTION="./dspurchase.html" METHOD=GET>
		  <INPUT TYPE=HIDDEN NAME="confirmpurchase" VALUE="yes">
		  <INPUT TYPE=HIDDEN NAME="customerid" VALUE="${customerid}">
		  
		  <c:forEach items="${FinalPurchaseList}" var="CheckOutItem">
		  	<INPUT NAME="item" TYPE="HIDDEN" VALUE="${CheckOutItem.item}">
      		<INPUT NAME='quan' TYPE='HIDDEN' VALUE="${CheckOutItem.quantity}">		  
		  </c:forEach>  			
		  
		  <INPUT TYPE=SUBMIT VALUE="Purchase">
		  
		</FORM>  			
	</c:when>
	<c:otherwise>
		<H2>Purchase complete</H2>
		<TABLE border=2>
		  <TR>
			  <TH>Item</TH>
			  <TH>Quantity</TH>
			  <TH>Title</TH>
			  <TH>Actor</TH>
			  <TH>Price</TH>
		  </TR>
		  <c:forEach items="${CheckOutItemList}" var="CheckOutItem" varStatus="ckoil">
		  	<c:set var="varitemno" value="${ckoil.count}"></c:set>
			<TR>
		      <TD ALIGN=CENTER>${varitemno}</TD>
		      <INPUT NAME="item" TYPE="HIDDEN" VALUE="${CheckOutItem.item}">
		      <TD><INPUT NAME="quan" TYPE="TEXT" SIZE=10 VALUE="${CheckOutItem.quantity}"></TD>
		      <TD>${CheckOutItem.title}</TD>
		      <TD>${CheckOutItem.actor}</TD>
		      <TD ALIGN=RIGHT>${CheckOutItem.price}</TD>
      		</TR>  	
		  </c:forEach>
		  <TR><TD></TD><TD></TD><TD></TD><TD>Subtotal</TD><TD ALIGN=RIGHT>${netamount}</TD></TR>
		  <TR><TD></TD><TD></TD><TD></TD><TD>Tax (${taxpct}%)</TD><TD ALIGN=RIGHT>${taxamount}</TD></TR>
		  <TR><TD></TD><TD></TD><TD></TD><TD>Total</TD><TD ALIGN=RIGHT>${totalamount}</TD></TR>
		  </TABLE><BR>
		  
		  <c:choose>
		  	<c:when test="${IsErrorOccured==1}">
		  		${msgException}
		  		<c:choose>
		  			<c:when test="${IsConnectionNull==0}">
		  				<c:choose>
		  					<c:when test="${IsRollBackError==1}">
		  						${rollbackException}
		  					</c:when>
		  				</c:choose>
		  			</c:when>
		  		</c:choose>		  		
		  	</c:when>
		  	<c:otherwise>
		  		<c:forEach items="${InSufficientList}" var="CheckOutItem">
		  			Insufficient quantity for prod ${CheckOutItem.item}
		  		</c:forEach>
		  	</c:otherwise>
		  </c:choose>
		  
		  <c:choose>
		  	<c:when test="${IsSuccess==1}">
		  		<H3>${amtCharged} charged to credit card ${creditcardno} (${creditcardtype}), expiration ${creditcardexpiration}</H3><BR>
      			<H2>Order Completed Successfully --- ORDER NUMBER:  ${ordernumber}</H2><BR>
		  	</c:when>
		  	<c:otherwise>
		  		<H3>Insufficient stock - order not processed</H3>
		  	</c:otherwise>
		  </c:choose>
		  		  		  		  
	</c:otherwise>

</c:choose>

<HR>
<P ALIGN=CENTER>Thank You for Visiting the DVD Store!</A></P>
<HR>
<P ALIGN=CENTER>Copyright &copy; 2005 Dell</P>
</FONT>
</BODY>
</HTML>

