package com.ds2web;

import com.ds2model.*;
import java.sql.*;
import java.math.*;
import java.io.*;
import java.lang.*;
import java.util.*;
import java.text.*;

import java.io.IOException;
import java.sql.SQLException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import org.springframework.web.servlet.ModelAndView;
import org.springframework.web.servlet.mvc.Controller;

public class indexController implements Controller 
{
	
	public ModelAndView handleRequest(HttpServletRequest request, HttpServletResponse response)
	throws ServletException, IOException, SQLException 
	{						
		ModelAndView modelAndView = new ModelAndView("index");		
		return modelAndView;
	}
	
}