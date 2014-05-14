package com.ds2web;

import com.ds2model.*;
import java.sql.*;
import java.math.*;
import java.io.*;
import java.lang.*;
import java.util.*;
import java.text.*;

import java.util.List;
import java.util.LinkedList;

import java.io.IOException;
import java.sql.SQLException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.springframework.web.servlet.ModelAndView;
import org.springframework.web.servlet.mvc.Controller;

public class purchaseController implements Controller 
{
	private List<CheckOutItem> CheckOutItemList;
	private List<CheckOutItem> FinalPurchaseList;
	private List<CheckOutItem> InSufficientList;
	
	public ModelAndView handleRequest(HttpServletRequest request, HttpServletResponse response)
	throws ServletException, IOException, SQLException 
	{						
		ModelAndView modelAndView = new ModelAndView("dspurchase");
	
		int IsConfirmPurchaseNull = 0;
		int IsErrorOccured = 0;
		int IsSuccess = 0;
		int IsConnectionNull = 1;
		int IsRollBackError = 0;
		
		// get values from URL
		String customerid = request.getParameter("customerid");
		String confirmpurchase = request.getParameter("confirmpurchase");
		
		modelAndView.addObject("customerid",customerid);
		modelAndView.addObject("confirmpurchase",confirmpurchase);

		CheckOutItemList = new LinkedList<CheckOutItem>();
		
		String[] item = {"1"};
		int item_length = 0;
		if ( request.getParameterValues("item") != null)  // if there are items, populate the item array
		{ 
		    item = request.getParameterValues("item"); 
		    item_length = item.length;
		}
		
		modelAndView.addObject("item_length",item_length);
		
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
			IsConfirmPurchaseNull = 1;
			modelAndView.addObject("IsConfirmPurchaseNull",IsConfirmPurchaseNull);
			
			String prod_id, title, actor, price, amt, purchase_query;
		    NumberFormat money = NumberFormat.getCurrencyInstance();
		    money.setMinimumFractionDigits(2);
		    money.setMaximumFractionDigits(2);
		    double netamount = 0;
		    int j = 0;   
		    for (int i=0; i< item_length; i++) 
		    {   
			    Arrays.sort(drop);          // sort the drop array, so that it can be searched for specific itemid
			    if (Arrays.binarySearch(drop,item[i]) < 0)     // if item was not selected to be dropped
			      {  
				      j = j+1;
				      
			          try 
			          {
			        	  Class.forName("oracle.jdbc.driver.OracleDriver");
		        	  }
				      catch (Exception e) 
				      {
				    	  System.out.println("Error opening connection");
			    	  }
				      
				      //Parameterized query by GSK
				      //purchase_query = "select PROD_ID, TITLE, ACTOR, PRICE from DS2.PRODUCTS where PROD_ID="+ item[i];
				      purchase_query = "select PROD_ID, TITLE, ACTOR, PRICE from DS2.PRODUCTS where PROD_ID= ?";
				      
				      Connection conn = DriverManager.getConnection("jdbc:oracle:thin:@localhost:1521:orcl", "ds2", "ds2");
				      
				      //Statement purchasequeryStatement = conn.createStatement();
				      PreparedStatement purchasequeryStatement = conn.prepareStatement(purchase_query);
				      purchasequeryStatement.setInt(1,Integer.parseInt(item[i]));
				      
				      //ResultSet purchasequeryResult = purchasequeryStatement.executeQuery(purchase_query);
				      ResultSet purchasequeryResult = purchasequeryStatement.executeQuery();
				      
				      purchasequeryResult.next();
				      
				      prod_id = purchasequeryResult.getString("PROD_ID");
				      title = purchasequeryResult.getString("TITLE");
				      actor = purchasequeryResult.getString("ACTOR");
				      price = purchasequeryResult.getString("PRICE");
				      amt = money.format( Float.parseFloat(price));
				      conn.close();
				      netamount = netamount + Integer.parseInt(quan[i]) * Float.parseFloat(price);
				      
				      //Add object to the LinkedList used for display in web page
				      CheckOutItem citem = new CheckOutItem();
				      citem.setItem(item[i]);
				      citem.setQuantity(quan[i]);
				      citem.setTitle(title);
				      citem.setActor(actor);
				      citem.setPrice(amt);
				      
				      CheckOutItemList.add(citem);
				      
			      }
		    }    // end of for loop to print items in list
		    
		    //Add LinkedList object to model to display in web page
		    modelAndView.addObject("CheckOutItemList",CheckOutItemList);
		    
		    double taxpct = 8.25;                              // double must be used for numberformat
		    modelAndView.addObject("taxpct",taxpct);
		    
		    double taxamount = netamount * taxpct/100.0;
		    double totalamount = taxamount + netamount;
		    
		    amt = money.format(netamount);		    
		    modelAndView.addObject("netamount",amt);
		    
		    amt = money.format(taxamount);
		    modelAndView.addObject("taxamount",amt);
		    
		    amt = money.format(totalamount);
		    modelAndView.addObject("totalamount",amt);
		    
		    FinalPurchaseList = new LinkedList<CheckOutItem>();
		    
		    for(int m=0; m<item_length; m++)  // loop for building item and quan list for a purchase 
		    {
		    	Arrays.sort(drop);
		        if (Arrays.binarySearch(drop,item[m]) < 0)  // if current item was not selected to be dropped from cart
		        {
		        	//Add objects to link list used as model object for display on web page 
		        	CheckOutItem cit = new CheckOutItem();
		        	cit.setItem(item[m]);
		        	cit.setQuantity(quan[m]);
		        	FinalPurchaseList.add(cit);
		        }
		    }
		    
		    modelAndView.addObject("FinalPurchaseList",FinalPurchaseList);		    
		    
	    }
		else
		{
			IsConfirmPurchaseNull = 0;
			modelAndView.addObject("IsConfirmPurchaseNull",IsConfirmPurchaseNull);
			
			String prod_id, title, actor, price, amt, purchase_query;
		    NumberFormat money = NumberFormat.getCurrencyInstance();
		    money.setMinimumFractionDigits(2);
		    money.setMaximumFractionDigits(2);
		    double netamount = 0;
		    for (int i=0; i< item_length; i++)    // loop to print out list of items purchased on purchase confirmation page
		    {
		      //purchase_query = "select PROD_ID, TITLE, ACTOR, PRICE from DS2.PRODUCTS where PROD_ID="+ item[i];
		      purchase_query = "select PROD_ID, TITLE, ACTOR, PRICE from DS2.PRODUCTS where PROD_ID= ?"; 
		      try 
		      {
		    	  Class.forName("oracle.jdbc.driver.OracleDriver");
	    	  }
		      catch (Exception e) 
		      {
		    	  System.out.println("Error opening connection");
	    	  }
		      Connection conn = DriverManager.getConnection("jdbc:oracle:thin:@localhost:1521:orcl", "ds2", "ds2"); 
		      
		      //Statement purchasequeryStatement = conn.createStatement();
		      PreparedStatement purchasequeryStatement = conn.prepareStatement(purchase_query);
		      purchasequeryStatement.setInt(1,Integer.parseInt(item[i]));
		      
		      //ResultSet purchasequeryResult = purchasequeryStatement.executeQuery(purchase_query);
		      ResultSet purchasequeryResult = purchasequeryStatement.executeQuery();
		      
		      purchasequeryResult.next();
		      prod_id = purchasequeryResult.getString("PROD_ID");
		      title = purchasequeryResult.getString("TITLE");
		      actor = purchasequeryResult.getString("ACTOR");
		      price = purchasequeryResult.getString("PRICE");
		      amt = money.format( Float.parseFloat(price));
		      netamount = netamount + Integer.parseInt(quan[i]) * Float.parseFloat(price);
		      
		      conn.close();
		      
		      //Add object to the LinkedList used for display in web page
		      CheckOutItem citem = new CheckOutItem();
		      citem.setItem(item[i]);
		      citem.setQuantity(quan[i]);
		      citem.setTitle(title);
		      citem.setActor(actor);
		      citem.setPrice(amt);
		      
		      CheckOutItemList.add(citem);
		      
		    }
		    
		    //Add LinkedList object to model to display in web page
		    modelAndView.addObject("CheckOutItemList",CheckOutItemList);
		    
		    double taxpct = 8.25;
		    modelAndView.addObject("taxpct",taxpct);
		    
		    double taxamount = netamount * taxpct/100.0;
		    double totalamount = taxamount + netamount;
		    
		    amt = money.format(netamount);
		    modelAndView.addObject("netamount",amt);
		    
		    amt = money.format(taxamount);
		    modelAndView.addObject("taxamount",amt);
		    
		    amt = money.format(totalamount);
		    modelAndView.addObject("totalamount",amt);

		    // The following section does the transactional commit of update
            // on orders, orderlines, customers, and inventory on the orderconn connection
		    String purchase_insert_query;
		    String orderid = "1";
		    boolean success = true;
		    DateFormat ds2dateformat = new SimpleDateFormat("dd-MMM-yyyy");    // set date format to match format in db
		    String currentdate = ds2dateformat.format(new java.util.Date());  // get current date in right format
		    Connection orderconn = null;		
		    IsConnectionNull = 1;       //Used to check for rollback message in View
		    
		    try
		    {
		    	NumberFormat totals = NumberFormat.getInstance();
		        totals.setMaximumFractionDigits(2);
		        totals.setMinimumFractionDigits(2);
		        orderconn = DriverManager.getConnection("jdbc:oracle:thin:@localhost:1521:orcl", "ds2", "ds2"); 

		        orderconn.setAutoCommit(false);      // tell connection to not commit until instructed
		        
		        IsConnectionNull = 0;
		        Statement purchaseupdateStatement = orderconn.createStatement();
		        purchase_insert_query = "INSERT into DS2.ORDERS (ORDERDATE, CUSTOMERID, NETAMOUNT, TAX, TOTALAMOUNT)" +
		        " VALUES ( '" + currentdate + "'," + customerid + "," + totals.format(netamount) + "," + totals.format(taxamount) + "," + totals.format(totalamount) + ")";
		        String cols[] = {"ORDERID"};
		        purchaseupdateStatement.executeUpdate(purchase_insert_query,cols);
		        ResultSet orderIDResult = purchaseupdateStatement.getGeneratedKeys();  // to get orderid that is autogenerated by db
		        orderIDResult.next();
		        orderid = orderIDResult.getString(1);

		        int isolevel = orderconn.getTransactionIsolation();		           
		        // 
		        // loop through purchased items and make inserts into orderdetails table     

		        int h = 0;
		        int j;
		        String query;
		        success = true;
		        
		        InSufficientList = new LinkedList<CheckOutItem>();
		        
		        while (h < item.length)
		        {
			        j = h+1;
			        String p_query = "INSERT into DS2.ORDERLINES (ORDERLINEID, ORDERID, PROD_ID, QUANTITY, ORDERDATE) VALUES"; 
			        String c_insert= "INSERT INTO DS2.CUST_HIST (CUSTOMERID, ORDERID, PROD_ID) VALUES ";
	
			        //query = "SELECT QUAN_IN_STOCK, SALES FROM DS2.INVENTORY WHERE PROD_ID=" + item[h];
			        query = "SELECT QUAN_IN_STOCK, SALES FROM DS2.INVENTORY WHERE PROD_ID= ? ";
			        
			        Connection quanconn = DriverManager.getConnection("jdbc:oracle:thin:@localhost:1521:orcl", "ds2", "ds2"); 
			        
			        //Statement quanquery = quanconn.createStatement();      // use quanconn instead of orderconn for simple quantity queries
			        PreparedStatement quanquery = quanconn.prepareStatement(query);      // use quanconn instead of orderconn for simple quantity queries
			        quanquery.setInt(1,Integer.parseInt(item[h]));
			        
			        //ResultSet quanResult = quanquery.executeQuery(query);
			        ResultSet quanResult = quanquery.executeQuery();
			        
			        quanResult.next();
			        
			        int curr_quan = quanResult.getInt("QUAN_IN_STOCK");
			        int curr_sales = quanResult.getInt("SALES");
			        int new_quan = curr_quan - Integer.parseInt(quan[h]);
			        int new_sales = curr_sales + Integer.parseInt(quan[h]);
			        quanconn.close();
	
			        if (new_quan < 0)       // if insufficient stock on hand - then flag failure
		            {
			        	success = false;
			        	//Add item[h] to this InSufficientList list
			        	CheckOutItem citem = new CheckOutItem();
			        	citem.setItem(item[h]);
			        	InSufficientList.add(citem);			        	
		            }
			        else       // if quantity does exist, update to new level   
			        {
			        query = "UPDATE DS2.INVENTORY SET QUAN_IN_STOCK=" + new_quan + ", SALES=" + new_sales + " WHERE PROD_ID=" + item[h];
			        purchaseupdateStatement.executeUpdate(query);
			        }
			              
			        p_query = p_query + "(" + j + "," + orderid + "," + item[h] + "," + quan[h] + ",'" + currentdate + "')";
			        c_insert = c_insert + "( " + customerid + "," + orderid + "," + item[h] + ") ";
			        purchaseupdateStatement.executeUpdate(p_query);  // Insert into orderlines
			        purchaseupdateStatement.executeUpdate(c_insert);  // Update customers with recent purchases
	
			        h = h +1;
		        } // End of while (!empty)
		        
		        //Add insufficient stock list to be displayed in web page
		        modelAndView.addObject("InSufficientList",InSufficientList);
		        
		        if ( success == true )           // if no errors were found, commit all 
		        {
		        	orderconn.commit();
		        }
			    else                             
			    {
			    	orderconn.rollback();       // otherwise, rollback
			    }        
			    orderconn.close();
		        
			    IsErrorOccured = 0;
			    modelAndView.addObject("IsErrorOccured",IsErrorOccured);
			    
		    }
		    catch  (SQLException e)                  // if any SQL exceptions were thrown, rollback
		    {
		    	IsErrorOccured = 1;
		    	modelAndView.addObject("IsErrorOccured",IsErrorOccured);
		    	String msgException = "SQL Exception entering order - " + e.getMessage();
		    	modelAndView.addObject("msgException",msgException);
		    	
		    	if (orderconn != null) 
		        {
		    		IsConnectionNull = 0;
			        try 
			        {			        	
			        	orderconn.rollback();
			        	IsRollBackError = 0;
			        }
			        catch (SQLException rbexception) 
			        { 
			        	IsRollBackError = 1;
			        	String rollbackException = "Error rolling back " + rbexception.getMessage();
			        	modelAndView.addObject("rollbackException",rollbackException);			        	
			        }
			        modelAndView.addObject("IsRollBackError",IsRollBackError);
		        }
		    	modelAndView.addObject("IsConnectionNull",IsConnectionNull);
		    	
		    }
		    finally 
		    { 
		    	orderconn.close(); 
	    	}
		    
		    if (success == true)
		    { 
		      IsSuccess = 1;
		      modelAndView.addObject("IsSuccess",IsSuccess);
		      
		      //Order processed and money charged to credit card
		      // 
		      // get credit card info and print confirmation message
		      String[] cctypes = {"MasterCard", "Visa", "Discover", "Amex", "Dell Preferred"};

		      //String cc_query = "select CREDITCARDTYPE, CREDITCARD, CREDITCARDEXPIRATION from DS2.CUSTOMERS where CUSTOMERID=" + customerid;
		      String cc_query = "select CREDITCARDTYPE, CREDITCARD, CREDITCARDEXPIRATION from DS2.CUSTOMERS where CUSTOMERID= ?";

		      Connection queryconn = DriverManager.getConnection("jdbc:oracle:thin:@localhost:1521:orcl", "ds2", "ds2"); 

		      //Statement ccqueryStatement = queryconn.createStatement();
		      PreparedStatement ccqueryStatement = queryconn.prepareStatement(cc_query);
		      ccqueryStatement.setInt(1,Integer.parseInt(customerid));
		      //ResultSet ccqueryResult = ccqueryStatement.executeQuery(cc_query);
		      ResultSet ccqueryResult = ccqueryStatement.executeQuery();
		      
		      ccqueryResult.next();
		      int creditcardtype = ccqueryResult.getInt("CREDITCARDTYPE");
		      String creditcard = ccqueryResult.getString("CREDITCARD");
		      String creditcardexpiration = ccqueryResult.getString("CREDITCARDEXPIRATION");
		      queryconn.close();
		      
		      modelAndView.addObject("amtCharged",amt);
		      modelAndView.addObject("creditcardno",creditcard);
		      modelAndView.addObject("creditcardtype",cctypes[creditcardtype-1]);
		      modelAndView.addObject("creditcardexpiration",creditcardexpiration);
		      modelAndView.addObject("ordernumber",orderid);
		      
		    }
			else
			{	//Insufficient stock order not processed
				IsSuccess = 0;
		        modelAndView.addObject("IsSuccess",IsSuccess);				 
			}
		}
				
		return modelAndView;
	}

}