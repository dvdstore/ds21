package com.ds2web;

import com.ds2model.*;
import java.sql.*;
import java.math.*;
import java.io.*;
import java.lang.*;

import java.util.List;
import java.util.LinkedList;

import java.io.IOException;
import java.sql.SQLException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.springframework.web.servlet.ModelAndView;
import org.springframework.web.servlet.mvc.Controller;

public class loginController implements Controller {

	private List<PrevPurchase> PrevPurchaseList;
	
	public ModelAndView handleRequest(HttpServletRequest request,
			HttpServletResponse response) throws ServletException, IOException, SQLException {					
				
		ModelAndView modelAndView = new ModelAndView("dslogin");		
		
		String username = request.getParameter("username");
		String password = request.getParameter("password");
		String customerid = "novalue";	
	
		int IsUserNameNull = 0;
		int IsUserExists = 0;
		int IsPrevPurchaseExists = 0;
		
		if (username != null)          // if username and password have been entered, check if valid
	    {
			IsUserNameNull = 0;
			modelAndView.addObject("IsUserNameNull",IsUserNameNull);
			
			try 
			{
		        Class.forName("oracle.jdbc.driver.OracleDriver");
		    }
  	        catch (Exception e) 
  	        {
  	        	System.out.println("Error opening connection");
        	}
	        Connection conn = DriverManager.getConnection("jdbc:oracle:thin:@localhost:1521:orcl", "ds2", "ds2"); 
	        //Commented by GSK
	        //Statement userqueryStatement = conn.createStatement();
	        //ResultSet userqueryResult = userqueryStatement.executeQuery("select CUSTOMERID FROM DS2.CUSTOMERS where USERNAME='" + username + "' and PASSWORD='" + password + "'");
	        //Use of Parameterized SQL by GSK
	        PreparedStatement userqueryStatement = conn.prepareStatement("select CUSTOMERID FROM DS2.CUSTOMERS where USERNAME=? and PASSWORD=? ");
	        userqueryStatement.setString(1,username);
	        userqueryStatement.setString(2,password);
	        
	        ResultSet userqueryResult = userqueryStatement.executeQuery();
	        
	        if (userqueryResult.next())  // if user exists, then print out previous purchases and recommendataions
	  	    {
	        	IsUserExists = 1;
	        	modelAndView.addObject("IsUserExists",IsUserExists);
	        	
	        	customerid = userqueryResult.getString("CUSTOMERID");
	        	//Add this customerid as newobject to modelAndView object
	        	modelAndView.addObject("customerid",customerid);
	    	        	
	        	//Commented by GSK
	        	//Statement prevpod_queryStatement = conn.createStatement();
	            //ResultSet prevprod_queryResult = prevpod_queryStatement.executeQuery("select PROD_ID FROM DS2.CUST_HIST where CUSTOMERID=" + customerid + " AND ROWNUM < 10 ORDER BY ORDERID DESC ");
	        	//Parameterized query by GSK
	        	PreparedStatement prevpod_queryStatement = conn.prepareStatement("select PROD_ID FROM DS2.CUST_HIST where CUSTOMERID=? AND ROWNUM < 10 ORDER BY ORDERID DESC ");
	        	prevpod_queryStatement.setInt(1, Integer.parseInt(customerid));
	        	
	        	ResultSet prevprod_queryResult = prevpod_queryStatement.executeQuery();
	        	
	            if (prevprod_queryResult.next())  //if previous purchases exists
	            {
	            	IsPrevPurchaseExists = 1;
	            	modelAndView.addObject("IsPrevPurchaseExists",IsPrevPurchaseExists);
	            	String prev_id, prev_title, prev_actor, recommend_title, recommend_prod_id;
		            
	            	//Commented by GSK
	            	//Statement prevproddetail_queryStatement;
	            	PreparedStatement prevproddetail_queryStatement;
		            ResultSet prevproddetailResult;
		            //Statement relatedprod_queryStatement;
		            PreparedStatement relatedprod_queryStatement;
		            ResultSet relatedprodResult;
		            
		            PrevPurchaseList = new LinkedList<PrevPurchase>();
		            
		            do
		              {
		              prevproddetailResult = null;
		              relatedprodResult = null;
		              prev_id = prevprod_queryResult.getString("PROD_ID");
		              //Commented by GSK
		              //prevproddetail_queryStatement = conn.createStatement();
		              //prevproddetailResult = prevproddetail_queryStatement.executeQuery("select TITLE, ACTOR from DS2.PRODUCTS where PROD_ID=" + prev_id);
		              //Parameterized query by GSK
		              prevproddetail_queryStatement = conn.prepareStatement("select TITLE, ACTOR from DS2.PRODUCTS where PROD_ID= ? ");
		              prevproddetail_queryStatement.setInt(1,Integer.parseInt(prev_id));
		              prevproddetailResult = prevproddetail_queryStatement.executeQuery();
		              
		              prevproddetailResult.next();
		              prev_title = prevproddetailResult.getString("TITLE");
		              prev_actor = prevproddetailResult.getString("ACTOR");
		              //Commented by GSK
		              //relatedprod_queryStatement = conn.createStatement();
		              //relatedprodResult = relatedprod_queryStatement.executeQuery("select TITLE from DS2.PRODUCTS where PROD_ID= (select COMMON_PROD_ID from DS2.PRODUCTS where PROD_ID=" + prev_id + ")");
		              //Parameterized query by GSK
		              relatedprod_queryStatement = conn.prepareStatement("select TITLE from DS2.PRODUCTS where PROD_ID= (select COMMON_PROD_ID from DS2.PRODUCTS where PROD_ID= ? )");
		              relatedprod_queryStatement.setInt(1, Integer.parseInt(prev_id));
		              relatedprodResult = relatedprod_queryStatement.executeQuery();
		              
		              relatedprodResult.next();
		              recommend_title = relatedprodResult.getString("TITLE");
		              
		              //Add  prev_title ,prev_actor ,recommend_title to map and add new object to modelAndView object
		              PrevPurchase prvpurchase = new PrevPurchase();
		              prvpurchase.setPrev_title(prev_title);
		              prvpurchase.setPrev_actor(prev_actor);
		              prvpurchase.setRecommend_title(recommend_title);
		              PrevPurchaseList.add(prvpurchase);
		              
		              prevproddetail_queryStatement.close();
		              relatedprod_queryStatement.close();
		              }  while (prevprod_queryResult.next()); // end while for populating table with recommended other items
		              
		              modelAndView.addObject("PrevPurchaseList", PrevPurchaseList);		              
	            }	        	
	  	    }
	        else
	        { //username password did not exist and username password incorrect        	
	        	IsUserExists = 0;
	        	modelAndView.addObject("IsUserExists",IsUserExists);
	        	modelAndView.addObject("username",username);
	        }	        
	        try {userqueryStatement.close();}
	        catch (Exception e) {System.out.println("Error closing statement");}
	    }
		else
		{  // if no username, then must be first entry to site - give them the logon screen
			IsUserNameNull = 1;
			modelAndView.addObject("IsUserNameNull",IsUserNameNull);
		}
	     		
		return modelAndView;
	}
}
