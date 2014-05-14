
/*
 * DVD Store 2 Oracle Functions - ds2oraclefns.cs
 *
 * Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
 *
 * Provides interface functions for DVD Store driver program ds2xdriver.cs
 * Requires Oracle Data Provider for .NET
 * See ds2xdriver.cs for compilation and syntax
 *
 * Last Updated 5/25/06
 * Last Updated 6/14/2010 by GSK (Single instance of driver driving multiple DB instances and Parameterization of IN query)
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA  */



using System;
using System.IO;
using System.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;


namespace ds2xdriver
  {
  /// <summary>
  /// ds2oraclefns.cs: DVD Store 2 Oracle Functions
  /// </summary>
  public class ds2Interface
    {
#if (USE_WIN32_TIMER)
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceCounter(ref long x);
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceFrequency(ref long x);  
#endif

    int ds2Interfaceid, i;
    OracleConnection objConn;
    OracleCommand Login, New_Customer, Browse_By_Category, Browse_By_Actor, Browse_By_Title, Purchase;
    
    OracleParameter[] Login_prm = new OracleParameter[5];
    OracleParameter[] New_Customer_prm = new OracleParameter[20];
    OracleParameter[] Browse_By_Category_prm = new OracleParameter[3];
    OracleParameter[] Browse_By_Actor_prm = new OracleParameter[3];
    OracleParameter[] Browse_By_Title_prm = new OracleParameter[3];
    OracleParameter[] Purchase_prm = new OracleParameter[6];
    
    OracleParameter Login_title_out, Login_actor_out, Login_related_title_out;
    OracleParameter Browse_By_Category_prod_id_out, Browse_By_Category_category_out, Browse_By_Category_title_out,
       Browse_By_Category_actor_out, Browse_By_Category_price_out,
       Browse_By_Category_special_out, Browse_By_Category_common_prod_id_out;
    OracleParameter Browse_By_Actor_prod_id_out, Browse_By_Actor_category_out, Browse_By_Actor_title_out,
       Browse_By_Actor_actor_out, Browse_By_Actor_price_out,   
       Browse_By_Actor_special_out, Browse_By_Actor_common_prod_id_out;
    OracleParameter Browse_By_Title_prod_id_out, Browse_By_Title_category_out, Browse_By_Title_title_out,
       Browse_By_Title_actor_out, Browse_By_Title_price_out,
       Browse_By_Title_special_out, Browse_By_Title_common_prod_id_out;
    OracleParameter Purchase_prod_id_in, Purchase_qty_in;
      
    OracleString[] o_title_out = new OracleString[GlobalConstants.MAX_ROWS];
    OracleString[] o_actor_out = new OracleString[GlobalConstants.MAX_ROWS];
    OracleString[] o_related_title_out = new OracleString[GlobalConstants.MAX_ROWS];
    int[] o_prod_id_out = new int[GlobalConstants.MAX_ROWS];
    int[] o_special_out = new int[GlobalConstants.MAX_ROWS];
    int[] o_common_prod_id_out = new int[GlobalConstants.MAX_ROWS]; 
    byte[] o_category_out = new byte[GlobalConstants.MAX_ROWS];
    double[] o_price_out = new double[GlobalConstants.MAX_ROWS];
    
    //Added by GSK (This variable will have target server name to which thread is tied to and users will login to the database on this server)
    string target_server_name;
//
//-------------------------------------------------------------------------------------------------
// 
    public ds2Interface(int ds2interfaceid)
      {
      ds2Interfaceid = ds2interfaceid;
      //Console.WriteLine("ds2Interface {0} created", ds2Interfaceid);
      }
//
//-------------------------------------------------------------------------------------------------
    
      //Added by GSK (Overloaded constructor to to consider scenario where single Driver program is driving workload on multiple machines)
    public ds2Interface ( int ds2interfaceid , string target_name)
        {
        ds2Interfaceid = ds2interfaceid;
        target_server_name = target_name;
        //Console.WriteLine("ds2Interface {0} created", ds2Interfaceid);
        }
//
//-------------------------------------------------------------------------------------------------
// 
    public  bool ds2initialize()
      {
      return(true);
      } // end ds2initialize()
 
//
//-------------------------------------------------------------------------------------------------
//  
    public bool ds2connect()
      {
      //string sConnectionString = "User ID=ds2;Password=ds2;Connection Timeout=120;Data Source=" + Controller.target;
      //Changed by GSK
      string sConnectionString = "User ID=ds2;Password=ds2;Connection Timeout=120;Data Source=" + target_server_name;
      try
        {
        objConn = new OracleConnection(sConnectionString);
        objConn.Open();
        //Console.WriteLine("Thread {0}: connected to database {1}",  Thread.CurrentThread.Name, Controller.target);
        //changed by GSK
        Console.WriteLine ( "Thread {0}: connected to database {1}" , Thread.CurrentThread.Name , target_server_name );
        }
      catch (OracleException e)
        {
        //Console.WriteLine("Thread {0}: error in connecting to database {1}: {2}",  Thread.CurrentThread.Name,
        //  Controller.target, e.Message);
        //Changed by GSK
        Console.WriteLine ( "Thread {0}: error in connecting to database {1}: {2}" , Thread.CurrentThread.Name ,
        target_server_name , e.Message );
        return(false);
        }

      // Set up Oracle stored procedure calls and associated parameters
      
      // Login
      Login = new OracleCommand("", objConn);
      Login.CommandText = "Login";
      Login.CommandType = CommandType.StoredProcedure;
      Login_prm[0] = Login.Parameters.Add("username_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      Login_prm[1] = Login.Parameters.Add("password_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      Login_prm[2] = Login.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Login_prm[3] = Login.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Login_prm[4] = Login.Parameters.Add("customerid_out", OracleDbType.Int32, ParameterDirection.Output);
      
      Login_title_out = 
        Login.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Login_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Login_title_out.Value = null;
      Login_title_out.Size = GlobalConstants.MAX_ROWS;     
      Login_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Login_title_out.ArrayBindSize[i] = 50;
            
      Login_actor_out = 
        Login.Parameters.Add("actor_out", OracleDbType.Varchar2, ParameterDirection.Output);      
      Login_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Login_actor_out.Value = null;
      Login_actor_out.Size = GlobalConstants.MAX_ROWS;  
      Login_actor_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Login_actor_out.ArrayBindSize[i] = 50;
                  
      Login_related_title_out = 
        Login.Parameters.Add("related_title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Login_related_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Login_related_title_out.Value = null;
      Login_related_title_out.Size = GlobalConstants.MAX_ROWS;     
      Login_related_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Login_related_title_out.ArrayBindSize[i] = 50;
            
      // New_Customer
      New_Customer = new OracleCommand("", objConn);
      New_Customer.CommandText = "New_Customer";
      New_Customer.CommandType = CommandType.StoredProcedure; 
      New_Customer_prm[0] = 
        New_Customer.Parameters.Add("firstname_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[1] = 
        New_Customer.Parameters.Add("lastname_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[2] = 
        New_Customer.Parameters.Add("address1_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[3] = 
        New_Customer.Parameters.Add("address2_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[4] = 
        New_Customer.Parameters.Add("city_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[5] = 
        New_Customer.Parameters.Add("state_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[6] = 
        New_Customer.Parameters.Add("zip_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Customer_prm[7] = 
        New_Customer.Parameters.Add("country_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[8] = 
        New_Customer.Parameters.Add("region_in", OracleDbType.Int16, ParameterDirection.Input);
      New_Customer_prm[9] = 
        New_Customer.Parameters.Add("email_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[10] = 
        New_Customer.Parameters.Add("phone_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[11] = 
        New_Customer.Parameters.Add("creditcardtype_in", OracleDbType.Int16, ParameterDirection.Input);
      New_Customer_prm[12] = 
        New_Customer.Parameters.Add("creditcard_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[13] = 
        New_Customer.Parameters.Add("creditcardexpiration_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[14] = 
        New_Customer.Parameters.Add("username_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[15] = 
        New_Customer.Parameters.Add("password_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      New_Customer_prm[16] = 
        New_Customer.Parameters.Add("age_in", OracleDbType.Int16, ParameterDirection.Input);                   
      New_Customer_prm[17] = 
        New_Customer.Parameters.Add("income_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Customer_prm[18] = 
        New_Customer.Parameters.Add("gender_in", OracleDbType.Varchar2, 1, ParameterDirection.Input);
      New_Customer_prm[19] = 
        New_Customer.Parameters.Add("customerid_out", OracleDbType.Int32, ParameterDirection.Output);                   

          
      //Browse_By_Category
      Browse_By_Category = new OracleCommand("", objConn);
      Browse_By_Category.CommandText = "Browse_By_Category";
      Browse_By_Category.CommandType = CommandType.StoredProcedure;
      
      Browse_By_Category_prm[0] = 
        Browse_By_Category.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Browse_By_Category_prm[1] = 
        Browse_By_Category.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Category_prm[2] = 
        Browse_By_Category.Parameters.Add("category_in", OracleDbType.Int32, ParameterDirection.Input);
      
      Browse_By_Category_prod_id_out = 
        Browse_By_Category.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Category_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_prod_id_out.Value = null;
      Browse_By_Category_prod_id_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Category_category_out = 
        Browse_By_Category.Parameters.Add("category_out", OracleDbType.Byte, ParameterDirection.Output);
      Browse_By_Category_category_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_category_out.Value = null;
      Browse_By_Category_category_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Category_title_out = 
        Browse_By_Category.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Browse_By_Category_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_title_out.Value = null;
      Browse_By_Category_title_out.Size = GlobalConstants.MAX_ROWS;     
      Browse_By_Category_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Category_title_out.ArrayBindSize[i] = 50;
            
      Browse_By_Category_actor_out = 
        Browse_By_Category.Parameters.Add("actor_out", OracleDbType.Varchar2, 50, ParameterDirection.Output);
      Browse_By_Category_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_actor_out.Value = null;
      Browse_By_Category_actor_out.Size = GlobalConstants.MAX_ROWS;  
      Browse_By_Category_actor_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Category_actor_out.ArrayBindSize[i] = 50;      
            
      Browse_By_Category_price_out = 
        Browse_By_Category.Parameters.Add("price_out", OracleDbType.Double, ParameterDirection.Output);
      Browse_By_Category_price_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_price_out.Value = null;
      Browse_By_Category_price_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Category_special_out = 
        Browse_By_Category.Parameters.Add("special_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Category_special_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_special_out.Value = null;
      Browse_By_Category_special_out.Size = GlobalConstants.MAX_ROWS;
              
      Browse_By_Category_common_prod_id_out = 
        Browse_By_Category.Parameters.Add("common_prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Category_common_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_common_prod_id_out.Value = null;
      Browse_By_Category_common_prod_id_out.Size = GlobalConstants.MAX_ROWS;
            
      //Browse_By_Actor
      Browse_By_Actor = new OracleCommand("", objConn);
      Browse_By_Actor.CommandText = "Browse_By_Actor";
      Browse_By_Actor.CommandType = CommandType.StoredProcedure;
      
      Browse_By_Actor_prm[0] = 
        Browse_By_Actor.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Browse_By_Actor_prm[1] = 
        Browse_By_Actor.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Actor_prm[2] = 
        Browse_By_Actor.Parameters.Add("actor_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      
      Browse_By_Actor_prod_id_out = 
        Browse_By_Actor.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Actor_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_prod_id_out.Value = null;
      Browse_By_Actor_prod_id_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Actor_category_out = 
        Browse_By_Actor.Parameters.Add("category_out", OracleDbType.Byte, ParameterDirection.Output);
      Browse_By_Actor_category_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_category_out.Value = null;
      Browse_By_Actor_category_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Actor_title_out = 
        Browse_By_Actor.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Browse_By_Actor_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_title_out.Value = null;
      Browse_By_Actor_title_out.Size = GlobalConstants.MAX_ROWS;     
      Browse_By_Actor_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Actor_title_out.ArrayBindSize[i] = 50;
      
      Browse_By_Actor_actor_out = 
        Browse_By_Actor.Parameters.Add("actor_out", OracleDbType.Varchar2, 50, ParameterDirection.Output);
      Browse_By_Actor_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_actor_out.Value = null;
      Browse_By_Actor_actor_out.Size = GlobalConstants.MAX_ROWS;  
      Browse_By_Actor_actor_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Actor_actor_out.ArrayBindSize[i] = 50;      
            
      Browse_By_Actor_price_out = 
        Browse_By_Actor.Parameters.Add("price_out", OracleDbType.Double, ParameterDirection.Output);
      Browse_By_Actor_price_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_price_out.Value = null;
      Browse_By_Actor_price_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Actor_special_out = 
        Browse_By_Actor.Parameters.Add("special_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Actor_special_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_special_out.Value = null;
      Browse_By_Actor_special_out.Size = GlobalConstants.MAX_ROWS;
              
      Browse_By_Actor_common_prod_id_out = 
        Browse_By_Actor.Parameters.Add("common_prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Actor_common_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_common_prod_id_out.Value = null;
      Browse_By_Actor_common_prod_id_out.Size = GlobalConstants.MAX_ROWS;
                
      //Browse_By_Title
      Browse_By_Title = new OracleCommand("", objConn);
      Browse_By_Title.CommandText = "Browse_By_Title";
      Browse_By_Title.CommandType = CommandType.StoredProcedure;
      
      Browse_By_Title_prm[0] = 
        Browse_By_Title.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Browse_By_Title_prm[1] = 
        Browse_By_Title.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Title_prm[2] = 
        Browse_By_Title.Parameters.Add("title_in", OracleDbType.Varchar2, 50, ParameterDirection.Input);
      
      Browse_By_Title_prod_id_out = 
        Browse_By_Title.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Title_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_prod_id_out.Value = null;
      Browse_By_Title_prod_id_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Title_category_out = 
        Browse_By_Title.Parameters.Add("category_out", OracleDbType.Byte, ParameterDirection.Output);
      Browse_By_Title_category_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_category_out.Value = null;
      Browse_By_Title_category_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Title_title_out = 
        Browse_By_Title.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Browse_By_Title_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_title_out.Value = null;
      Browse_By_Title_title_out.Size = GlobalConstants.MAX_ROWS;     
      Browse_By_Title_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Title_title_out.ArrayBindSize[i] = 50;
      
      Browse_By_Title_actor_out = 
        Browse_By_Title.Parameters.Add("actor_out", OracleDbType.Varchar2, 50, ParameterDirection.Output);
      Browse_By_Title_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_actor_out.Value = null;
      Browse_By_Title_actor_out.Size = GlobalConstants.MAX_ROWS;  
      Browse_By_Title_actor_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Title_actor_out.ArrayBindSize[i] = 50;      
            
      Browse_By_Title_price_out = 
        Browse_By_Title.Parameters.Add("price_out", OracleDbType.Double, ParameterDirection.Output);
      Browse_By_Title_price_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_price_out.Value = null;
      Browse_By_Title_price_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Title_special_out = 
        Browse_By_Title.Parameters.Add("special_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Title_special_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_special_out.Value = null;
      Browse_By_Title_special_out.Size = GlobalConstants.MAX_ROWS;
              
      Browse_By_Title_common_prod_id_out = 
        Browse_By_Title.Parameters.Add("common_prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Title_common_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_common_prod_id_out.Value = null;
      Browse_By_Title_common_prod_id_out.Size = GlobalConstants.MAX_ROWS;
            

      //Purchase
      Purchase = new OracleCommand("", objConn);
      Purchase.CommandText = "Purchase";
      Purchase.CommandType = CommandType.StoredProcedure;
      
      Purchase_prm[0] = 
        Purchase.Parameters.Add("customerid_in", OracleDbType.Int32, ParameterDirection.Input);
      Purchase_prm[1] = 
        Purchase.Parameters.Add("number_items", OracleDbType.Int32, ParameterDirection.Input);
      Purchase_prm[2] = 
        Purchase.Parameters.Add("netamount_in", OracleDbType.Double, ParameterDirection.Input);
      Purchase_prm[3] = 
        Purchase.Parameters.Add("taxamount_in", OracleDbType.Double, ParameterDirection.Input);
      Purchase_prm[4] = 
        Purchase.Parameters.Add("totalamount_in", OracleDbType.Double, ParameterDirection.Input);
      Purchase_prm[5] = 
        Purchase.Parameters.Add("neworderid_out", OracleDbType.Int32, ParameterDirection.Output);
              
      Purchase_prod_id_in = 
        Purchase.Parameters.Add("prod_id_in", OracleDbType.Int32, ParameterDirection.Input);
      Purchase_prod_id_in.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Purchase_prod_id_in.Size = 10;
           
      Purchase_qty_in = 
        Purchase.Parameters.Add("qty_in", OracleDbType.Int32, ParameterDirection.Input);
      Purchase_qty_in.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Purchase_qty_in.Size = 10;
   
      return(true);
      } // end ds2connect()
 
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2login(string username_in, string password_in, ref int customerid_out, ref int rows_returned, 
      ref string[] title_out, ref string[] actor_out, ref string[] related_title_out, ref double rt)
      {
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif     

      int batch_size = 10;
      
      Login_prm[0].Value = username_in;
      Login_prm[1].Value = password_in;
      Login_prm[2].Value = batch_size;
         
#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif
        
      try 
        {
        Login.ExecuteNonQuery();
        rows_returned = (int) Login_prm[3].Value;
        customerid_out = (int) Login_prm[4].Value;
        o_title_out = (OracleString[]) Login_title_out.Value;
        o_actor_out = (OracleString[]) Login_actor_out.Value;
        o_related_title_out = (OracleString[]) Login_related_title_out.Value;
        }
      catch (OracleException e) 
        {
        Console.WriteLine("Thread {0}: Error in Login: {1}", Thread.CurrentThread.Name, e.Message);
        return (false);
        }
      
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif
            
//    Console.WriteLine("Thread {0}: {1} successfully logged in;   rows_returned={2}  customerid_out={3}",  
//     Thread.CurrentThread.Name, username_in, rows_returned, customerid_out);
      for (int i_row=0; i_row<rows_returned; i_row++)
        {
        title_out[i_row] = o_title_out[i_row].ToString();
        actor_out[i_row] = o_actor_out[i_row].ToString();
        related_title_out[i_row] = o_related_title_out[i_row].ToString();
//      Console.WriteLine("  title= {0}  actor= {1}  related_title= {2}",
//        title_out[i_row], actor_out[i_row], related_title_out[i_row]);
        }
                    
      return(true);
      }  // end ds2login
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2newcustomer(string username_in, string password_in, string firstname_in, 
      string lastname_in, string address1_in, string address2_in, string city_in, string state_in, 
      string zip_in, string country_in, string email_in, string phone_in, int creditcardtype_in, 
      string creditcard_in, int ccexpmon_in, int ccexpyr_in, int age_in, int income_in, 
      string gender_in, ref int customerid_out, ref double rt) 
      {
      int region_in = (country_in == "US") ? 1:2;
      string creditcardexpiration_in = String.Format("{0:D4}/{1:D2}", ccexpyr_in, ccexpmon_in);
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif   
     
      New_Customer_prm[0].Value = firstname_in;
      New_Customer_prm[1].Value = lastname_in;
      New_Customer_prm[2].Value = address1_in;
      New_Customer_prm[3].Value = address2_in;
      New_Customer_prm[4].Value = city_in;
      New_Customer_prm[5].Value = state_in;
      New_Customer_prm[6].Value = (zip_in=="") ? 0 : Convert.ToInt32(zip_in);
      New_Customer_prm[7].Value = country_in;
      New_Customer_prm[8].Value = region_in;
      New_Customer_prm[9].Value = email_in;
      New_Customer_prm[10].Value = phone_in;
      New_Customer_prm[11].Value = creditcardtype_in;
      New_Customer_prm[12].Value = creditcard_in;
      New_Customer_prm[13].Value = creditcardexpiration_in;
      New_Customer_prm[14].Value = username_in;
      New_Customer_prm[15].Value = password_in;
      New_Customer_prm[16].Value = age_in;
      New_Customer_prm[17].Value = income_in;
      New_Customer_prm[18].Value = gender_in;
    

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  
      
      try 
        {
        New_Customer.ExecuteNonQuery();
        customerid_out = (int) New_Customer_prm[19].Value;
        }       
      catch (System.Exception e) 
        {
        Console.WriteLine("Thread {0}: ERROR in New_Customer.ExecuteNonQuery(): {1}", 
          Thread.CurrentThread.Name, e.Message);
        }
     
            
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif        
    
//    Console.WriteLine("Thread {0}: New_Customer created w/username_in= {1}  region={2}  customerid={3}",
//      Thread.CurrentThread.Name, username_in, region_in, customerid_out);

      return(true);
      } // end ds2newcustomer()
    
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2browse(string browse_type_in, string browse_category_in, string browse_actor_in,
      string browse_title_in, int batch_size_in, int customerid_out, ref int rows_returned, 
      ref int[] prod_id_out, ref string[] title_out, ref string[] actor_out, ref decimal[] price_out, 
      ref int[] special_out, ref int[] common_prod_id_out, ref double rt)
      {
      // Products table: PROD_ID INT, CATEGORY TINYINT, TITLE VARCHAR(50), ACTOR VARCHAR(50), 
      //   PRICE DECIMAL(12,2), SPECIAL TINYINT, COMMON_PROD_ID INT
      string data_in = null;
      int[] category_out = new int[GlobalConstants.MAX_ROWS];

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif  
      switch(browse_type_in)
        {
        case "category":
          Browse_By_Category_prm[0].Value = batch_size_in;
          Browse_By_Category_prm[2].Value = Convert.ToInt32(browse_category_in);
          data_in = browse_category_in;
          break;
        case "actor":
          Browse_By_Actor_prm[0].Value = batch_size_in;
          Browse_By_Actor_prm[2].Value = browse_actor_in;
          data_in = browse_actor_in;
          break;
        case "title":
          Browse_By_Title_prm[0].Value = batch_size_in;
          Browse_By_Title_prm[2].Value = browse_title_in;
          data_in = browse_title_in;
          break;
        }

//    Console.WriteLine("Thread {0}: Calling Browse w/ browse_type= {1}  batch_size_in= {2}  data_in= {3}",  
//      Thread.CurrentThread.Name, browse_type_in, batch_size_in, data_in); 

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif 
                 
      try 
        {
        switch(browse_type_in)
          {
          case "category":
            Browse_By_Category.ExecuteNonQuery();
            rows_returned = (int) Browse_By_Category_prm[1].Value;
            o_prod_id_out = (int[]) Browse_By_Category_prod_id_out.Value;
            o_category_out = (byte[]) Browse_By_Category_category_out.Value;
            o_title_out = (OracleString[]) Browse_By_Category_title_out.Value;
            o_actor_out = (OracleString[]) Browse_By_Category_actor_out.Value;
            o_price_out = (double[]) Browse_By_Category_price_out.Value;         
            o_special_out = (int[]) Browse_By_Category_special_out.Value;
            o_common_prod_id_out = (int[]) Browse_By_Category_common_prod_id_out.Value;
            break;
          case "actor":
            Browse_By_Actor.ExecuteNonQuery();        
            rows_returned = (int) Browse_By_Actor_prm[1].Value;
            o_prod_id_out = (int[]) Browse_By_Actor_prod_id_out.Value;
            o_category_out = (byte[]) Browse_By_Actor_category_out.Value;
            o_title_out = (OracleString[]) Browse_By_Actor_title_out.Value;
            o_actor_out = (OracleString[]) Browse_By_Actor_actor_out.Value;
            o_price_out = (double[]) Browse_By_Actor_price_out.Value;         
            o_special_out = (int[]) Browse_By_Actor_special_out.Value;
            o_common_prod_id_out = (int[]) Browse_By_Actor_common_prod_id_out.Value;            
            break;
          case "title":
            Browse_By_Title.ExecuteNonQuery();
            rows_returned = (int) Browse_By_Title_prm[1].Value;
            o_prod_id_out = (int[]) Browse_By_Title_prod_id_out.Value;
            o_category_out = (byte[]) Browse_By_Title_category_out.Value;
            o_title_out = (OracleString[]) Browse_By_Title_title_out.Value;
            o_actor_out = (OracleString[]) Browse_By_Title_actor_out.Value;
            o_price_out = (double[]) Browse_By_Title_price_out.Value;         
            o_special_out = (int[]) Browse_By_Title_special_out.Value;            
            o_common_prod_id_out = (int[]) Browse_By_Title_common_prod_id_out.Value;
            break;
          }
        }
      catch (OracleException e) 
        {
        Console.WriteLine("Thread {0}: Error in Browse: {1}", Thread.CurrentThread.Name, e.Message);
        return(false);
        }
            
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif  

//    Console.WriteLine("Thread {0}: Browse successful: type= {1}  rows_returned={2}",  
//       Thread.CurrentThread.Name, browse_type_in, rows_returned);
      for (int i_row=0; i_row<rows_returned; i_row++)
        {
        prod_id_out[i_row] = o_prod_id_out[i_row];
        category_out[i_row] = o_category_out[i_row];
        title_out[i_row] = o_title_out[i_row].ToString();
        actor_out[i_row] = o_actor_out[i_row].ToString();
        price_out[i_row] = (decimal) o_price_out[i_row];
        special_out[i_row] = o_special_out[i_row];
        common_prod_id_out[i_row] = o_common_prod_id_out[i_row];
        
//      Console.WriteLine("  prod_id= {0} category= {1} title= {2} actor= {3} price= {4} special= {5} common_prod_id= {6}",
//        prod_id_out[i_row], category_out[i_row], title_out[i_row], actor_out[i_row], price_out[i_row], 
//        special_out[i_row], common_prod_id_out[i_row]);
        }           

      return(true);
      } // end ds2browse()
    
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2purchase(int cart_items, int[] prod_id_in, int[] qty_in, int customerid_out,
      ref int neworderid_out, ref bool IsRollback, ref double rt)
      {
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif 


      //Cap cart_items at 10 for this implementation of stored procedure
      cart_items = System.Math.Min(10, cart_items);
      
      // Extra, non-stored procedure query to find total cost of purchase
 
      int i, j; 
      Decimal netamount_in = 0;
      //Modified by GSK for parameterized query
      //string cost_query = "select PROD_ID, PRICE from PRODUCTS where PROD_ID in (" + prod_id_in[0];
      //for (i=1; i<cart_items; i++) cost_query = cost_query + "," + prod_id_in[i];
      //cost_query = cost_query + ")";

      //Implementation for parameterizing IN query by GSK
      string cost_query = "select PROD_ID, PRICE from PRODUCTS where PROD_ID in ( :ARG0 ";
      for (i = 1; i < cart_items; i++) cost_query = cost_query + ", :ARG" + i;
      cost_query = cost_query + ")";

      //Console.WriteLine(cost_query);
      OracleCommand cost_command = new OracleCommand(cost_query, objConn);

      string argHolder;
      for (i = 0; i < cart_items; i++)
      {
          argHolder = ":ARG" + i;
          cost_command.Parameters.Add(argHolder, OracleDbType.Int32);
          cost_command.Parameters[argHolder].Value = prod_id_in[i];
      }

      OracleDataReader Rdr = cost_command.ExecuteReader();
      while (Rdr.Read())
        {
        j = 0;
        int prod_id = (int) Rdr.GetDecimal(0);
        while (prod_id_in[j] != prod_id) ++j; // Find which product was returned
        netamount_in = netamount_in + qty_in[j] * Rdr.GetDecimal(1);
        //Console.WriteLine(j + " " + prod_id + " " + qty_in[j] + " " + Rdr.GetDecimal(1));
        }
      Rdr.Close();

      // Can use following code instead if you don't want extra roundtrip to database:
      //Random rr = new Random(DateTime.Now.Millisecond);
      //Decimal netamount_in = (Decimal) (0.01 * (1 + rr.Next(40000)));
      //Console.WriteLine(netamount_in);
      Decimal taxamount_in =  (Decimal) 0.0825 * netamount_in;
      Decimal totalamount_in = netamount_in + taxamount_in;
      
      Purchase_prm[0].Value = customerid_out;
      Purchase_prm[1].Value = cart_items;
      Purchase_prm[2].Value = netamount_in;
      Purchase_prm[3].Value = taxamount_in;
      Purchase_prm[4].Value = totalamount_in;
      
      Purchase_prod_id_in.Value = prod_id_in; 
      Purchase_qty_in.Value = qty_in;     

//    Console.WriteLine("Thread {0}: Calling Purchase w/ customerid = {1}  number_items= {2}",  
//      Thread.CurrentThread.Name, customerid_out, cart_items);

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)  
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  
        
      try 
        {
        Purchase.ExecuteNonQuery();
        neworderid_out = (int) Purchase_prm[5].Value;
        }

      catch(System.Exception e)
        {
        Console.WriteLine("Thread {0}: ERROR in Purchase.ExecuteNonQuery(): {1}",  
          Thread.CurrentThread.Name, e.Message);
        }

#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif  
      if (neworderid_out == 0) IsRollback = true;    

      return(true);
      } // end ds2purchase()
    
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2close()
      {
      objConn.Close();   
      return(true);   
      } // end ds2close()
    } // end Class ds2Interface
  } // end namespace ds2xdriver
  
        
