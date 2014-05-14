<!-- 
dsbrowse.jsp jsp page that browses DVD store by author, title, or category on oracle database
 
It also collects selected items into a list and allows customer to purchase them

 Copyright 2005 Dell

 Written by Todd Muirhead/Dave Jaffe      Last modified: 9/14/05
 Converted to Spring implementation by GSK 	Last Modified: 07/06/2010
--> 
<%@ page contentType="text/html" %> 
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="fn" uri="http://java.sun.com/jsp/jstl/functions" %>
<% pageContext.setAttribute("categories",new String[] {"Action", "Animation", "Children", "Classics", "Comedy", "Documentary", "Drama", "Family", "Foreign","Games", "Horror", "Music", "New", "Sci-Fi", "Sports", "Travel"});%>

<HTML>
<HEAD><TITLE>DVD Store Browse Page</TITLE></HEAD>
<BODY>
<FONT FACE=\"Arial\" COLOR=\"#0000FF\">
<H1 ALIGN=CENTER>DVD Store</H1>
<c:choose>
	<c:when test="${IsCustomerIdNull==1}">
		<H2>You have not logged in - Please click below to Login to DVD Store</H2>
	    <FORM ACTION="./dslogin.html" METHOD=GET>
	    <INPUT TYPE=SUBMIT VALUE="Login">
	    </FORM>
	</c:when>
	<c:otherwise>
	
		 <H2>Select Type of Search</H2>
		 <FORM ACTION="./dsbrowse.html" METHOD=GET>
			 
			 <c:choose> 
			 	<c:when test="${browsetype == 'title'}">
			 		<INPUT NAME="browsetype" TYPE=RADIO VALUE="title" CHECKED>Title<INPUT NAME="browse_title" VALUE="${browse_title}" TYPE=TEXT SIZE=15> <BR>
		 	 	</c:when>
		 	 	<c:otherwise>
		 	 		<INPUT NAME="browsetype" TYPE=RADIO VALUE="title">Title<INPUT NAME="browse_title" VALUE="${browse_title}" TYPE=TEXT SIZE=15> <BR>
		 	 	</c:otherwise>
			 </c:choose>
			 
			 <c:choose>
			 	<c:when test="${browsetype == 'actor'}">
			 		<INPUT NAME="browsetype" TYPE=RADIO VALUE="actor" CHECKED>Actor<INPUT NAME="browse_actor" VALUE="${browse_actor}" TYPE=TEXT SIZE=15> <BR>
		 	 	</c:when>
		 	 	<c:otherwise>
		 	 		<INPUT NAME="browsetype" TYPE=RADIO VALUE="actor">Actor<INPUT NAME="browse_actor" VALUE="${browse_actor}" TYPE=TEXT SIZE=15> <BR>
		 	 	</c:otherwise>	
			 </c:choose>			 
			 
			 <c:choose>
			 	<c:when test="${browsetype == 'category'}">
				 	<INPUT NAME="browsetype" TYPE=RADIO VALUE="category" CHECKED>Category
				 	<SELECT NAME="browse_category">
					
					<c:forEach items="${categories}" var="varcategory" varStatus="catg">
	    				<c:set var="currcategory" value="${catg.current}"></c:set>
	    				<c:set var="currcategorycount" value="${catg.count}"></c:set>
	    				<c:choose>
	    					<c:when test="${currcategorycount == browse_category}">
	    						<OPTION VALUE="${currcategorycount}" SELECTED>${currcategory}</OPTION>
	    					</c:when>
	    					<c:otherwise>
	    						<OPTION VALUE="${currcategorycount}">${currcategory}</OPTION>
	    					</c:otherwise>
	    				</c:choose>
	    			</c:forEach>
	    				 	
					</SELECT><BR>
				</c:when>
				<c:otherwise>
				 	<INPUT NAME="browsetype" TYPE=RADIO VALUE="category">Category
				 	<SELECT NAME="browse_category">
					
					<c:forEach items="${categories}" var="varcategory" varStatus="catg">
	    				<c:set var="currcategory" value="${catg.current}"></c:set>
	    				<c:set var="currcategorycount" value="${catg.count}"></c:set>
	    				<c:choose>
	    					<c:when test="${currcategorycount == browse_category}">
	    						<OPTION VALUE="${currcategorycount}" SELECTED>${currcategory}</OPTION>
	    					</c:when>
	    					<c:otherwise>
	    						<OPTION VALUE="${currcategorycount}">${currcategory}</OPTION>
	    					</c:otherwise>
	    				</c:choose>
	    			</c:forEach>
	    				 	
					</SELECT><BR>
				</c:otherwise>
			 </c:choose>
			 

			Number of search results to return
			<SELECT NAME="limit_num">
			
			<c:forEach var="i" begin="1" end="10" step="1">
    			<c:choose>
    				<c:when test="${i == limit_num}">
    					 <OPTION VALUE="${i}" SELECTED>${i}</OPTION>
    				</c:when>
    				<c:otherwise>
    					<OPTION VALUE="${i}">${i}</OPTION>
    				</c:otherwise>
    			</c:choose>
   			</c:forEach>	
			
			</SELECT><BR>
			
			<INPUT TYPE=HIDDEN NAME="customerid" VALUE="${customerid}">
			
			<c:forEach items="${FinalItemList}" var="CartItem">
				<INPUT TYPE=HIDDEN NAME="item" VALUE="${CartItem.str_Item}"></INPUT>
			</c:forEach>
			
			<INPUT TYPE=SUBMIT VALUE="Search">
  		</FORM>							    				 	
			
	</c:otherwise>
</c:choose>

<c:choose>
	<c:when test="${IsBrowseTypeNull==0}">
		<c:choose>
			<c:when test="${IsSearchResultFound==0}">
				 <H2>No DVDs Found</H2>
			</c:when>
			<c:otherwise>
				  <BR>
			      <H2>Search Results</H2>
			      <FORM ACTION="./dsbrowse.html" METHOD=GET>
			      <TABLE border=2>
				      <TR>
				      <TH>Add to Shopping Cart</TH>
				      <TH>Title</TH>
				      <TH>Actor</TH>
				      <TH>Price</TH>
				      </TR>
				      
				      <c:forEach items="${BrowseResultList}" var="BrowseResult">
				      	<TR>
				          <TD><INPUT NAME="selected_item" TYPE=CHECKBOX VALUE="${BrowseResult.prod_id}"></TD>
				          <TD>${BrowseResult.title}</TD>
				          <TD>${BrowseResult.actor}</TD>
				          <TD>${BrowseResult.price}</TD>
				        </TR>
				      </c:forEach>
				      
				      </TABLE>
				      <BR>
				
				      <INPUT TYPE=HIDDEN NAME="customerid" VALUE="${customerid}">
				      
				      <c:forEach items="${ExistingItemList}" var="CartItem">
				      	<INPUT TYPE=HIDDEN NAME="item" VALUE="${CartItem.str_Item}"></INPUT>
				      </c:forEach>				      
				      
				      <INPUT TYPE=SUBMIT VALUE="Update Shopping Cart">
				      </FORM>
				      				
			</c:otherwise>
		</c:choose>
	</c:when>
	<c:otherwise>	
	</c:otherwise>
</c:choose>

<c:choose>
	<c:when test="${IsShopCartEmpty==0}">
		<H2>Shopping Cart</H2>
	    <FORM ACTION='./dspurchase.html' METHOD='GET'>
		    
		    <TABLE border=2>
		    <TR>
			    <TH>Item</TH>
			    <TH>Title</TH>
		    </TR>
		    <c:forEach items="${TitleBrowseList}" var="BrowseResult">
		    	<TR>
			    	<TD>${BrowseResult.prod_id}</TD>
			    	<TD>${BrowseResult.title}</TD>
		    	</TR>
		    </c:forEach>
		    </TABLE>
			
			<BR>
			<INPUT TYPE=HIDDEN NAME="customerid" VALUE="${customerid}">
			<INPUT TYPE=HIDDEN NAME="num_of_items" VALUE="${new_item_length}">
			
			<c:forEach items="${FinalItemList}" var="CartItem">
				<INPUT TYPE=HIDDEN NAME="item" VALUE="${CartItem.str_Item}"></INPUT>
			</c:forEach>
						
		    <INPUT TYPE=SUBMIT VALUE="Checkout">
		    
		</FORM>
			
	</c:when>
	<c:otherwise>	
	</c:otherwise>
</c:choose>

<HR>
<P ALIGN=CENTER>Thank You for Visiting the DVD Store!</A></P>
<HR>
<P ALIGN=CENTER>Copyright &copy; 2005 Dell</P>
</FONT>
</BODY>
</HTML>