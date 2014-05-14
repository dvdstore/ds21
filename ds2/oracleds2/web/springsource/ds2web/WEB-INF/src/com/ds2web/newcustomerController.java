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

public class newcustomerController implements Controller 
{
	
	public ModelAndView handleRequest(HttpServletRequest request, HttpServletResponse response)
	throws ServletException, IOException, SQLException 
	{						
		ModelAndView modelAndView = new ModelAndView("dsnewcustomer");
		
		// Get all parameters from URL, if not there then set to empty string
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
		
		modelAndView.addObject("firstname",firstname);
		modelAndView.addObject("lastname",lastname);
		modelAndView.addObject("address1",address1);
		modelAndView.addObject("address2",address2);
		modelAndView.addObject("city",city);
		modelAndView.addObject("state",state);
		modelAndView.addObject("zip",zip);
		modelAndView.addObject("country",country);
		modelAndView.addObject("gender",gender);
		modelAndView.addObject("username",username);
		modelAndView.addObject("password",password);
		modelAndView.addObject("email",email);
		modelAndView.addObject("phone",phone);
		
		modelAndView.addObject("creditcardtype",creditcardtype);		
		
		modelAndView.addObject("creditcard",creditcard);
		
		modelAndView.addObject("ccexpmon",ccexpmon);
		modelAndView.addObject("ccexpyr",ccexpyr);
		
		modelAndView.addObject("age",age);
		modelAndView.addObject("income",income);
		
		int IsAllFieldsComplete = 0;
		int IsUserExistsAlready = 0;
   	    //Check to see if all required fields are complete, if they are complete then try to add new user 
		if ( (firstname != "")&&(lastname != "")&&(address1 != "")&&(city != "")&&(country != "")&&(username != "")&&(password != "") )
	    {
		  IsAllFieldsComplete = 1;
		  modelAndView.addObject("IsAllFieldsComplete",IsAllFieldsComplete);
		  		  
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
		  //String query = "select COUNT(*) from DS2.CUSTOMERS where USERNAME='" + username + "'";
		  //Statement userqueryStatement = conn.createStatement();            // run query to check to see if username already exists
		  //ResultSet userqueryResult = userqueryStatement.executeQuery(query);
		  //Parameterized query by GSK
		  String query = "select COUNT(*) from DS2.CUSTOMERS where USERNAME= ? ";
		  PreparedStatement userqueryStatement = conn.prepareStatement(query);
		  userqueryStatement.setString(1,username);
		  ResultSet userqueryResult = userqueryStatement.executeQuery();
		  
		  userqueryResult.next();
		  int check_username = userqueryResult.getInt("count(*)");    // get result from db into an int
		  conn.close();               // close connection to db
		  if (check_username > 0)  // if username did exist in database already, print form again for retry
	      {
			  IsUserExistsAlready = 1;
			  modelAndView.addObject("IsUserExistsAlready",IsUserExistsAlready);			  			  			  
			  
	      }
		  else  // executed if the username requested did not exist - insert the new user
		  {
			  IsUserExistsAlready = 0;
			  modelAndView.addObject("IsUserExistsAlready",IsUserExistsAlready);
			  
			  int region = 1;
			  if (country != "US") 
			  {
				  region = 2; 
			  }
			  if ( ccexpmon.length() < 2 )
			  {
				  ccexpmon = "0" + ccexpmon; 
			  } 

			  String creditcardexpiration = ccexpyr + ccexpmon;

			  String insert_newuser_query = "INSERT INTO DS2.CUSTOMERS (FIRSTNAME, LASTNAME, ADDRESS1, ADDRESS2, " + 
			      "CITY, STATE, ZIP, COUNTRY, REGION, EMAIL, PHONE, CREDITCARDTYPE, CREDITCARD, CREDITCARDEXPIRATION," +
			      " USERNAME, PASSWORD, AGE, INCOME, GENDER) " +
			      " VALUES ('" + firstname + "','" + lastname + " ','" + address1 + " ','" + address2 + "','" + city + "','" + state + "','" + zip + "','" + country + "','" + 
			      region + "','" + email + "','" + phone + "','" + creditcardtype + "','" + creditcard + "','" + creditcardexpiration + "','" + 
			      username + "','" + password + "','" + age + "','" + income + "','" + gender + "')";
			    
			  try 
			  {   
				  Class.forName("oracle.jdbc.driver.OracleDriver").newInstance();   
			  }
			  catch (Exception e) 
			  {
				  System.out.println("Error opening connection");
			  }
			  Connection newuserconn = DriverManager.getConnection("jdbc:oracle:thin:@localhost:1521:orcl", "ds2", "ds2"); 
			  
			  Statement userInsertStatement = newuserconn.createStatement(ResultSet.TYPE_FORWARD_ONLY, ResultSet.CONCUR_UPDATABLE);
			  
			  String[] cols = {"CUSTOMERID"};
			  
			  userInsertStatement.executeUpdate(insert_newuser_query,cols);
			  
	          // the RETURN_GENERATED_KEYS option on the executeUpdate is needed for the autoincrement
	          // customerid colum to be returned.  This value is then forwarded to each subsquent page
		      ResultSet userInsertResult = userInsertStatement.getGeneratedKeys();
	          //  Get the auto generated key into a result set
		      userInsertResult.next();
			  String customerid = userInsertResult.getString(1);   // get autocreated customerid into string
			  // it is passed below as a hidden value to next page , so add MVC object
			  
			  modelAndView.addObject("customerid",customerid);
			  
		  }
	    }
		else
		{ //All fields not complete
			IsAllFieldsComplete = 0;
			modelAndView.addObject("IsAllFieldsComplete",IsAllFieldsComplete);			
		}
		return modelAndView;
	}
}
