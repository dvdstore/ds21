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

public class browseController implements Controller 
{
	private List<CartItem> ExistingItemList;	
	private List<CartItem> FinalItemList;	
	
	private List<BrowseResult> BrowseResultList;
	private List<BrowseResult> TitleBrowseList;
	
	public ModelAndView handleRequest(HttpServletRequest request, HttpServletResponse response)
	throws ServletException, IOException, SQLException 
	{						
		ModelAndView modelAndView = new ModelAndView("dsbrowse");
				
		int IsCustomerIdNull = 0;
		int IsBrowseTypeNull = 0;
		int IsSearchResultFound = 0;
		int IsShopCartEmpty = 0;
		
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
				
		ExistingItemList = new LinkedList<CartItem>();
		FinalItemList = new LinkedList<CartItem>();
		
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


		modelAndView.addObject("customerid",customerid);
		modelAndView.addObject("browsetype",browsetype);
		modelAndView.addObject("browse_title",browse_title);
		modelAndView.addObject("browse_actor",browse_actor);
		modelAndView.addObject("browse_category",browse_category);
		modelAndView.addObject("limit_num",limit_num);
		
		modelAndView.addObject("item_length",Integer.toString(item_length));
		modelAndView.addObject("new_item_length",Integer.toString(new_item_length));
		modelAndView.addObject("selected_item_length",Integer.toString(selected_item_length));
				
		if (customerid == null)   // check to see if the user has logged in
	    {
			IsCustomerIdNull = 1;
			modelAndView.addObject("IsCustomerIdNull",IsCustomerIdNull);
			
		
	    }
		else
		{
			IsCustomerIdNull = 0;
			modelAndView.addObject("IsCustomerIdNull",IsCustomerIdNull);
			
			for (int i=0; i< item_length; i++) 
			{
				new_item[i] = item[i];  // Put existing items into new item array
				CartItem citem = new CartItem();
				citem.setStr_Item(item[i].toString());				
				ExistingItemList.add(citem);			
				FinalItemList.add(citem);
			}
			if ( selected_item_length > 0 )    // Add new selected items to end new item array
		    {
		    	for (int i=0; i< selected_item_length; i++) 
		    	{
		    		new_item[item_length+i] = selected_item[i];
		    		CartItem citem = new CartItem();
					citem.setStr_Item(selected_item[i].toString());
					FinalItemList.add(citem);					
		    	}
		    }		   
			
			modelAndView.addObject("ExistingItemList",ExistingItemList);
			modelAndView.addObject("FinalItemList",FinalItemList);		    			
		}
		
		if (browsetype != null) // if a browse type of either TITLE AUTHOR or CATEGORY was selected
	    {
		  
		  IsBrowseTypeNull = 0;
		  modelAndView.addObject("IsBrowseTypeNull",IsBrowseTypeNull);
		  
  		  try 
  		  {
  			  Class.forName("oracle.jdbc.driver.OracleDriver");
  		  }
		  catch (Exception e) 
		  {
			  System.out.println("Error opening connection");
		  }
	      Connection conn = DriverManager.getConnection("jdbc:oracle:thin:@localhost:1521:orcl", "ds2", "ds2"); 
	
		  String browse_query = "";     // init string variable
		  //Parameterized queries by GSK
		  switch (browsetype.charAt(0))  // switch on browsetype that was selected
	      {
		    case 't':
		      //browse_query = "select * from PRODUCTS where CONTAINS(TITLE,'" + browse_title + "') > 0 AND rownum <=" + limit_num;
		    	browse_query = "select * from PRODUCTS where CONTAINS(TITLE,?) > 0 AND rownum <= ?" ;
		      break;
		    case 'a':
		      //browse_query = "select * from PRODUCTS where CONTAINS(ACTOR,'" + browse_actor + "') > 0 AND rownum <=" + limit_num;
		    	browse_query = "select * from PRODUCTS where CONTAINS(ACTOR,?) > 0 AND rownum <= ?" ;
		      break;
		    case 'c':
		      //browse_query = "select * from PRODUCTS where CATEGORY = " + browse_category + " and SPECIAL=1 AND rownum <=" + limit_num;
		    	browse_query = "select * from PRODUCTS where CATEGORY = ? and SPECIAL=1 AND rownum <= ?" ;	
		      break;
		  }
	
		  //Statement browseStatement = conn.createStatement();
		  PreparedStatement browseStatement = conn.prepareStatement(browse_query);
		  
		  switch (browsetype.charAt(0))  // switch on browsetype that was selected
	      {
		    case 't':
		    	browseStatement.setString(1,browse_title);
		    	browseStatement.setInt(2,Integer.parseInt(limit_num));
		    	break;
		    case 'a':
		    	browseStatement.setString(1,browse_actor);
		    	browseStatement.setInt(2,Integer.parseInt(limit_num));
		    	break;
		    case 'c':
		    	browseStatement.setInt(1,Integer.parseInt(browse_category));
		    	browseStatement.setInt(2,Integer.parseInt(limit_num));
		    	break;
	      }
		  
		  //ResultSet browseResult = browseStatement.executeQuery(browse_query);    // submit browse query to Oracle                             
		  ResultSet browseResult = browseStatement.executeQuery();    // submit browse query to Oracle
		  if (!browseResult.next())  //No Search result found
		  {
			  IsSearchResultFound = 0;
			  modelAndView.addObject("IsSearchResultFound",IsSearchResultFound);			  
		  }
		  else  //Search result found
		  {
			  IsSearchResultFound = 1;
			  modelAndView.addObject("IsSearchResultFound",IsSearchResultFound);
			  
			  BrowseResultList = new LinkedList<BrowseResult>();
			  
			  String browse_result_row_prod_id, browse_result_row_title, browse_result_row_actor, browse_result_row_price;
		      do
	          {
		          browse_result_row_prod_id = browseResult.getString("PROD_ID");
		          browse_result_row_title = browseResult.getString("TITLE");
		          browse_result_row_actor = browseResult.getString("ACTOR");
		          browse_result_row_price = browseResult.getString("PRICE");
		          
		          //Add object to Link list
		          BrowseResult br = new BrowseResult();
		          br.setProd_id(browse_result_row_prod_id);
		          br.setTitle(browse_result_row_title);
		          br.setActor(browse_result_row_actor);
		          br.setPrice(browse_result_row_price);		          
		          BrowseResultList.add(br);
		          
		      } while (browseResult.next());     // loop to display search results in HTML table
		      
		      //Add LinkedList object to modelandview object for display in HTML using jstl
		      modelAndView.addObject("BrowseResultList",BrowseResultList);
		      
		  }
		  conn.close();
	    }
		else
		{
			IsBrowseTypeNull = 1;
		    modelAndView.addObject("IsBrowseTypeNull",IsBrowseTypeNull);
		}
		
		if ( new_item_length > 0 )  // If the shopping cart is not empty then - Show shopping cart
		{
			IsShopCartEmpty = 0;
			modelAndView.addObject("IsShopCartEmpty",IsShopCartEmpty);
			
			TitleBrowseList = new LinkedList<BrowseResult>();
			int j;
		    String title, query;
		    for (int i=0; i< new_item_length; i++) 
		    {
			    j = i+1;
			    //Parameterized query by GSK
			    //query = "select TITLE from PRODUCTS where PROD_ID=" + new_item[i];
			    query = "select TITLE from PRODUCTS where PROD_ID= ?";
			    try 
			    {
			    	Class.forName("oracle.jdbc.driver.OracleDriver");
		    	}
			    catch (Exception e) 
			    {
			    	System.out.println("Error opening connection");
		    	}
			    Connection conn = DriverManager.getConnection("jdbc:oracle:thin:@localhost:1521:orcl", "ds2", "ds2"); 
	
			    //Statement titlebrowseStatement = conn.createStatement();
			    PreparedStatement titlebrowseStatement = conn.prepareStatement(query);
			    titlebrowseStatement.setInt(1,Integer.parseInt(new_item[i]));
			    
			    //ResultSet titlebrowseResult = titlebrowseStatement.executeQuery(query);
			    ResultSet titlebrowseResult = titlebrowseStatement.executeQuery();
			    titlebrowseResult.next();
			    title = titlebrowseResult.getString("TITLE");
			    
			    //Add object to TitleResultList object
			    BrowseResult br = new BrowseResult();
			    br.setProd_id(Integer.toString(j));
			    br.setTitle(title);
			    TitleBrowseList.add(br);
			    
			    conn.close();
		    }
		    
		    //Add whole title list as mvc object to be displayed in html
		    modelAndView.addObject("TitleBrowseList",TitleBrowseList);
		    
		}
		else
		{
			IsShopCartEmpty = 1;
			modelAndView.addObject("IsShopCartEmpty",IsShopCartEmpty);
		}
		
		return modelAndView;		
	}
	
}