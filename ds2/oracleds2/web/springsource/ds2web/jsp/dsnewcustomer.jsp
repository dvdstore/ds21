<!-- 
dsnewcustomer.jsp JSP page that creates new user entry in ds database running on Oracle

 Copyright 2005 Dell

 Written by Todd Muirhead/Dave Jaffe      Last modified: 9/14/05
 Converted JSP to Spring MVC by GSK		  Last Modified: 07/05/2010
--> 
<%@ page contentType="text/html" %> 
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="fn" uri="http://java.sun.com/jsp/jstl/functions" %>
<% pageContext.setAttribute("countries",new String[] {"United States", "Australia", "Canada", "Chile", "China", "France", "Germany", "Japan", "Russia", "South Africa", "UK"});%>
<% pageContext.setAttribute("cctypes",new String[] {"MasterCard", "Visa", "Discover", "Amex", "Dell Preferred"});%>
<% pageContext.setAttribute("months",new String[]  {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"});%>
<HTML>
<HEAD><TITLE>DVD Store New Customer Login</TITLE></HEAD>
<BODY>
<FONT FACE=\"Arial\" COLOR=\"#0000FF\">
<H1 ALIGN=CENTER>DVD Store</H1>
<c:choose>
	<c:when test="${IsAllFieldsComplete==1}">
		<c:choose>
			<c:when test="${IsUserExistsAlready==1}">
				<H2>Username already in use! Please try another username</H2>
				<FORM ACTION="./dsnewcustomer.html" METHOD=GET>
			    Firstname <INPUT TYPE=TEXT NAME="firstname" VALUE="${firstname}" SIZE=16 MAXLENGTH=50>* <BR>
			    Lastname <INPUT TYPE=TEXT NAME="lastname" VALUE="${lastname}" SIZE=16 MAXLENGTH=50>* <BR>
			    Address1 <INPUT TYPE=TEXT NAME="address1" VALUE="${address1}" SIZE=16 MAXLENGTH=50>* <BR>
			    Address2 <INPUT TYPE=TEXT NAME="address2" VALUE="${address2}" SIZE=16 MAXLENGTH=50> <BR>
			    City <INPUT TYPE=TEXT NAME="city" VALUE="${city}" SIZE=16 MAXLENGTH=50>* <BR>
			    State <INPUT TYPE=TEXT NAME="state" VALUE="${state}" SIZE=16 MAXLENGTH=50> <BR>
			    Zipcode <INPUT TYPE=TEXT NAME="zip" VALUE="${zip}" SIZE=16 MAXLENGTH=5> <BR>
			    Country <SELECT NAME="country" SIZE=1>
			    			    
				<c:forEach items="${countries}" var="varcountry" varStatus="ctry">
					<c:set var="currcountry" value="${ctry.current}"></c:set>
					<c:choose>						
						<c:when test="${country == currcountry}">
							<OPTION VALUE="${ctry.current}" SELECTED>${ctry.current}</OPTION> 
							<H1>hit</H1>
						</c:when>
						<c:otherwise>
							<OPTION VALUE="${ctry.current}">${ctry.current}</OPTION>							
						</c:otherwise>
					</c:choose>
				</c:forEach>
				</SELECT>* <BR>
			    
			    Email <INPUT TYPE=TEXT NAME="email" VALUE="${email}" SIZE=16 MAXLENGTH=50> <BR>
			    Phone <INPUT TYPE=TEXT NAME="phone" VALUE="${phone}" SIZE=16 MAXLENGTH=50> <BR>
				Credit Card Type
    			<SELECT NAME='creditcardtype' SIZE=1>
    			
    			<c:forEach items="${cctypes}" var="varcctypes" varStatus="cctp">
    				<c:set var="currcctype" value="${cctp.current}"></c:set>
    				<c:set var="currcctypecount" value="${cctp.count}"></c:set>
    				<c:choose>
    					<c:when test="${currcctypecount == creditcardtype}">
    						<OPTION VALUE="${currcctypecount}" SELECTED>${currcctype}</OPTION>
    					</c:when>
    					<c:otherwise>
    						<OPTION VALUE="${currcctypecount}">${currcctype}</OPTION>
    					</c:otherwise>
    				</c:choose>
    			</c:forEach>
    			    			
    			</SELECT>

			    Credit Card Number <INPUT TYPE=TEXT NAME='creditcard' VALUE="${creditcard}" SIZE=16 MAXLENGTH=50>
			    Credit Card Expiration
    			<SELECT NAME='ccexpmon' SIZE=1>
    			
    			<c:forEach items="${months}" var="varmonths" varStatus="mnth" >
    				<c:set var="currmonth" value="${mnth.current}"></c:set>
    				<c:set var="currmonthcount" value="${mnth.count}"></c:set>
    				<c:choose>
    					<c:when test="${currmonthcount == ccexpmon}">
    						 <OPTION VALUE="${currmonthcount}" SELECTED>${currmonth}</OPTION> 
    					</c:when>
    					<c:otherwise>
    						<OPTION VALUE="${currmonthcount}">${currmonth}</OPTION>
    					</c:otherwise>
    				</c:choose>
    			</c:forEach>
    			
    			</SELECT>
    			
    			<SELECT NAME='ccexpyr' SIZE=1>
    			
    			<c:forEach var="yr" begin="2008" end="2013" step="1">
	    			<c:choose>
	    				<c:when test="${yr == ccexpyr}">
	    					 <OPTION VALUE="${yr}" SELECTED>${yr}</OPTION>
	    				</c:when>
	    				<c:otherwise>
	    					<OPTION VALUE="${yr}">${yr}</OPTION>
	    				</c:otherwise>
	    			</c:choose>
    			</c:forEach>
    			
    			</SELECT><BR>
    			
    			Username <INPUT TYPE=TEXT NAME="username" VALUE="${username}" SIZE=16 MAXLENGTH=50>*<BR>
			    Password <INPUT TYPE="PASSWORD" NAME="password" VALUE="${password}" SIZE=16 MAXLENGTH=50>*<BR>
			    Age <INPUT TYPE=TEXT NAME="age" VALUE="${age}" SIZE=3 MAXLENGTH=3> <BR>
			    Income (\$US) <INPUT TYPE=TEXT NAME="income" VALUE="${income}" SIZE=16 MAXLENGTH=50> <BR>
			    Gender
			    <c:choose>
			    	<c:when test="${gender == 'M'}">
	    				<INPUT TYPE=RADIO NAME="gender" VALUE="M" CHECKED> Male
    		    	</c:when> 
    		    	<c:otherwise>
    		    		<INPUT TYPE=RADIO NAME="gender" VALUE="M"> Male
    		    	</c:otherwise>
			    </c:choose> 
			    <c:choose>
		    	       <c:when test="${gender == 'F'}">
		    	       		<INPUT TYPE=RADIO NAME="gender" VALUE="F" CHECKED > Female	
		    	       </c:when>
		    	       <c:otherwise>
		    	       		<INPUT TYPE=RADIO NAME="gender" VALUE="F"> Female
		    	       </c:otherwise>
			    </c:choose>
			    <c:choose>
		    	       <c:when test="${gender == '?'}">
		    	       		<INPUT TYPE=RADIO NAME="gender" VALUE="?" CHECKED > Dont Know <BR>
	    	       	   </c:when>
	    	       	   <c:otherwise>
    	       	   			<INPUT TYPE=RADIO NAME="gender" VALUE="?"> Dont Know <BR>
	    	       	   </c:otherwise>
			    </c:choose>
			    
			    
			    <INPUT TYPE="submit" VALUE="Submit New Customer Data">
			    </FORM>
			    
			</c:when>
			<c:otherwise>
				<H2>New Customer Successfully Added.  Click below to begin shopping<H2>
			    <FORM ACTION='./dsbrowse.html' METHOD=GET>
			    <INPUT TYPE=HIDDEN NAME=customerid VALUE="${customerid}">  
			    <INPUT TYPE=SUBMIT VALUE='Start Shopping'>
			    </FORM>			
			</c:otherwise>			
		</c:choose>
	</c:when>
	<c:otherwise>
		<H2>New Customer - Please Complete All Required Fields Below (marked with *)</H2>
		<FORM ACTION="./dsnewcustomer.html" METHOD=GET>
		  Firstname <INPUT TYPE=TEXT NAME="firstname" VALUE="${firstname}" SIZE=16 MAXLENGTH=50>* <BR>
		  Lastname <INPUT TYPE=TEXT NAME="lastname" VALUE="${lastname}" SIZE=16 MAXLENGTH=50>* <BR>
		  Address1 <INPUT TYPE=TEXT NAME="address1" VALUE="${address1}" SIZE=16 MAXLENGTH=50>* <BR>
		  Address2 <INPUT TYPE=TEXT NAME="address2" VALUE="${address2}" SIZE=16 MAXLENGTH=50> <BR>
		  City <INPUT TYPE=TEXT NAME="city" VALUE="${city}" SIZE=16 MAXLENGTH=50>* <BR>
		  State <INPUT TYPE=TEXT NAME="state" VALUE="${state}" SIZE=16 MAXLENGTH=50> <BR>
		  Zipcode <INPUT TYPE=TEXT NAME="zip" VALUE="${zip}" SIZE=16 MAXLENGTH=5> <BR>
		  Country <SELECT NAME="country" SIZE=1>
		  
		  <c:forEach items="${countries}" var="varcountry" varStatus="ctry">
				<c:set var="currcountry" value="${ctry.current}"></c:set>
				<c:choose>						
					<c:when test="${country == currcountry}">
						<OPTION VALUE="${ctry.current}" SELECTED>${ctry.current}</OPTION> 
						<H1>hit</H1>
					</c:when>
					<c:otherwise>
						<OPTION VALUE="${ctry.current}">${ctry.current}</OPTION>							
					</c:otherwise>
				</c:choose>
		  </c:forEach>
		  
		  </SELECT>* <BR>
				  
		  Email <INPUT TYPE=TEXT NAME="email" VALUE="${email}" SIZE=16 MAXLENGTH=50> <BR>
		    Phone <INPUT TYPE=TEXT NAME="phone" VALUE="${phone}" SIZE=16 MAXLENGTH=50> <BR>
			Credit Card Type
   			<SELECT NAME='creditcardtype' SIZE=1>
   			
   			<c:forEach items="${cctypes}" var="varcctypes" varStatus="cctp">
   				<c:set var="currcctype" value="${cctp.current}"></c:set>
   				<c:set var="currcctypecount" value="${cctp.count}"></c:set>
   				<c:choose>
   					<c:when test="${currcctypecount == creditcardtype}">
   						<OPTION VALUE="${currcctypecount}" SELECTED>${currcctype}</OPTION>
   					</c:when>
   					<c:otherwise>
   						<OPTION VALUE="${currcctypecount}">${currcctype}</OPTION>
   					</c:otherwise>
   				</c:choose>
   			</c:forEach>
   			    			
   			</SELECT>

		    Credit Card Number <INPUT TYPE=TEXT NAME='creditcard' VALUE="${creditcard}" SIZE=16 MAXLENGTH=50>
		    Credit Card Expiration
   			<SELECT NAME='ccexpmon' SIZE=1>
   			
   			<c:forEach items="${months}" var="varmonths" varStatus="mnth" >
   				<c:set var="currmonth" value="${mnth.current}"></c:set>
   				<c:set var="currmonthcount" value="${mnth.count}"></c:set>
   				<c:choose>
   					<c:when test="${currmonthcount == ccexpmon}">
   						 <OPTION VALUE="${currmonthcount}" SELECTED>${currmonth}</OPTION> 
   					</c:when>
   					<c:otherwise>
   						<OPTION VALUE="${currmonthcount}">${currmonth}</OPTION>
   					</c:otherwise>
   				</c:choose>
   			</c:forEach>
   			
   			</SELECT>
   			
   			<SELECT NAME='ccexpyr' SIZE=1>
   			
   			<c:forEach var="yr" begin="2008" end="2013" step="1">
    			<c:choose>
    				<c:when test="${yr == ccexpyr}">
    					 <OPTION VALUE="${yr}" SELECTED>${yr}</OPTION>
    				</c:when>
    				<c:otherwise>
    					<OPTION VALUE="${yr}">${yr}</OPTION>
    				</c:otherwise>
    			</c:choose>
   			</c:forEach>
   			
   			</SELECT><BR>
   			
   			Username <INPUT TYPE=TEXT NAME="username" VALUE="${username}" SIZE=16 MAXLENGTH=50>*<BR>
		    Password <INPUT TYPE="PASSWORD" NAME="password" VALUE="${password}" SIZE=16 MAXLENGTH=50>*<BR>
		    Age <INPUT TYPE=TEXT NAME="age" VALUE="${age}" SIZE=3 MAXLENGTH=3> <BR>
		    Income (\$US) <INPUT TYPE=TEXT NAME="income" VALUE="${income}" SIZE=16 MAXLENGTH=50> <BR>
		    Gender
			    <c:choose>
			    	<c:when test="${gender == 'M'}">
	    				<INPUT TYPE=RADIO NAME="gender" VALUE="M" CHECKED> Male
    		    	</c:when> 
    		    	<c:otherwise>
    		    		<INPUT TYPE=RADIO NAME="gender" VALUE="M"> Male
    		    	</c:otherwise>
			    </c:choose> 
			    <c:choose>
		    	       <c:when test="${gender == 'F'}">
		    	       		<INPUT TYPE=RADIO NAME="gender" VALUE="F" CHECKED > Female	
		    	       </c:when>
		    	       <c:otherwise>
		    	       		<INPUT TYPE=RADIO NAME="gender" VALUE="F"> Female
		    	       </c:otherwise>
			    </c:choose>
			    <c:choose>
		    	       <c:when test="${gender == '?'}">
		    	       		<INPUT TYPE=RADIO NAME="gender" VALUE="?" CHECKED > Dont Know <BR>
	    	       	   </c:when>
	    	       	   <c:otherwise>
    	       	   			<INPUT TYPE=RADIO NAME="gender" VALUE="?"> Dont Know <BR>
	    	       	   </c:otherwise>
			    </c:choose>
			    
		    <INPUT TYPE="submit" VALUE="Submit New Customer Data">
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
