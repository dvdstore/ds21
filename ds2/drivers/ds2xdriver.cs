
/*
 * Generalized DVD Store 2 Driver Program - ds2xdriver.cs
 *
 * Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
 *
 * Generates orders against DVD Store Database V.2 through web interface or directly against database
 * Simulates users logging in to store or creating new customer data; browsing for DVDs by title, actor or 
 * category, and purchasing selected DVDs
 *
 * To see syntax: ds2xdriver   where x= web, mysql, sqlserver or oracle
 *
 * Compile with appropriate functions file to generate driver for web, SQL Server, MySQL, Oracle or PostgreSQL target:
 *  csc /out:ds2webdriver.exe       ds2xdriver.cs ds2webfns.cs       /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS
 *  csc /out:ds2sqlserverdriver.exe ds2xdriver.cs ds2sqlserverfns.cs /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS
 *  csc /out:ds2mysqldriver.exe     ds2xdriver.cs ds2mysqlfns.cs     /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  /r:<path>MySql.Data.dll
 *  csc /out:ds2oracledriver.exe    ds2xdriver.cs ds2oraclefns.cs    /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  /r:<path>Oracle.DataAccess.dll
 *  csc /out:ds2pgsqldriver.exe     ds2xdriver.cs ds2pgsqlfns.cs     /d:USE_WIN32_TIMER /d:GENPERF_CTRS   /r:<path>Npgsql.dll
 *
 *  USE_WIN32_TIMER: if defined, program will use high resolution WIN32 timers
 *  GEN_PERF_CTRS: if defined, program will generate Windows Perfmon performance counters
 *
 *  csc is installed with Microsoft.NET   Typical location: C:\WINNT\Microsoft.NET\Framework\v2.0.50727
 *
 * Updated 6/14/2010 by GSK(girish.khadke@gmail.com)
 * Last Updated 5/12/11 by DJ (cleaned up output; minor fixes)
 *
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
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Text;   //Added by GSK

namespace ds2xdriver
  {
  /// <summary>
  /// ds2xdriver: drives DVD Store 2 Database through web interface or directly against database
  /// </summary>

  public class GlobalConstants
    {
    public const int MAX_USERS = 1000;
    public const int MAX_CATEGORY = 16;
    public const int MAX_ROWS = 100;
    public const int LAST_N = 100;
    }

  //
  //-------------------------------------------------------------------------------------------------
  //
  class Controller
    {
    // If compile option /d:USE_WIN32_TIMER is specified will use 64b QueryPerformance counter from Win32
    // Else will use .NET DateTime class      
#if (USE_WIN32_TIMER)
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceCounter(ref long x);
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceFrequency(ref long x);   
#endif

    // Variables needed by User objects 
    public static string target , windows_perf_host = null;
    public static string[] target_servers;                   //Added by GSK (for single instance of driver program driving multiple database servers)
    public static string[] windows_perf_host_servers;       //Added by GSK
    public static int n_target_servers = 1;                 //Added by GSK to keep track of number of Servers/DB instances on which threads spawned
    public static object UpdateLock = 1;
    public static int n_threads , n_threads_running = 0 , n_threads_connected = 0;
    public static int n_overall = 0 , n_login_overall = 0 , n_newcust_overall = 0 , n_browse_overall = 0 ,
      n_purchase_overall = 0 , n_rollbacks_overall = 0 , n_rollbacks_from_start = 0 , n_purchase_from_start = 0 , n_cpu_pct_samples = 0;

    //Added by GSK
    public static int[] arr_n_login_overall;
    public static double[] arr_rt_login_overall;
    public static int[] arr_n_newcust_overall;
    public static double[] arr_rt_newcust_overall;
    public static int[] arr_n_browse_overall;
    public static double[] arr_rt_browse_overall;
    public static int[] arr_n_purchase_overall;
    public static double[] arr_rt_purchase_overall;
    public static int[] arr_n_rollbacks_overall;
    public static int[] arr_n_overall;
    public static double[] arr_rt_tot_overall;
    public static int[] arr_n_purchase_from_start;
    public static int[] arr_n_rollbacks_from_start;
    public static double[,] arr_rt_tot_lastn;
    public static double[] arr_cpu_pct_tot;
    public static int[] arr_n_cpu_pct_samples;         

    public static int pct_newcustomers = 0 , n_searches , search_batch_size , n_line_items , ramp_rate;
    public static double think_time , rt_tot_overall = 0.0 , rt_login_overall = 0.0 , rt_newcust_overall = 0.0 ,
      rt_browse_overall = 0.0 , rt_purchase_overall = 0.0 , cpu_pct_tot = 0.0;
    public static double[] rt_tot_lastn = new double[GlobalConstants.LAST_N];
    public static bool Start = false , End = false;
    public static int[] MAX_CUSTOMER = new int[] { 20000 , 2000000 , 200000000 };
    public static int[] MAX_PRODUCT = new int[] { 10000 , 100000 , 1000000 };
    public static int max_customer , max_product , prod_array_size;
    //public static int[] prod_array = new int[1100000];
    //Changed by GSK (size of this array will depend on number of rows in product table)
    public static int[] prod_array;
    public static string virt_dir = "ds2" , page_type = "php";

    //Added new parameter database_custom_size and new variables by GSK 
    //Note that order_rows are per month
    public static int customer_rows , order_rows , product_rows;
    public static string db_size = "10MB";

    //Added by GSK (New parameter to Print detailed or aggregate output  Values = "Y" or "N" Default value = "N"
    public static string detailed_view = "N";
    public static bool is_detailed_view = true;

    //Added by GSK( New parameter to print Linux CPU utilization statistics)
    public static string linux_perf_host = null;
    public static string[] linux_perf_host_servers;
    public static string[] linux_unames;
    public static string[] linux_passwd;
    public static double[] arr_linux_cpu_utilization;       //Used for book keeping purposes
    //Keep track of number of windows and linux VM's on which to drive workload on 
    public static int n_windows_servers = 0;
    public static int n_linux_servers = 0;
    //Boolean values to check if there are linux and windows target VM's
    public static bool is_Lin_VM = false;
    public static bool is_Win_VM = false;

    // Variables needed within Controller class
    // Added new Parameter db_size by GSK
    // db_size will indicate actual database size (e.g. Values for this parameter can be like 10MB or 150GB) 
    //db_size_str parameter is removed since it would not be used in code anywhere
    //Instead at same place we need db_size parameter
    //Added new parameter detailed_view by GSK default value = N
    //Added new parameter linux_perf_host by GSK 
    static string[] input_parm_names = new string[] {"config_file", "target", "n_threads", "ramp_rate",
      "run_time", "db_size", "warmup_time", "think_time", "pct_newcustomers", "n_searches",
      "search_batch_size", "n_line_items", "virt_dir", "page_type", "windows_perf_host", "linux_perf_host", "detailed_view"};
    static string[] input_parm_desc = new string[] {"config file path", 
      "database/web server hostname or IP address", "number of driver threads", "startup rate (users/sec)",
      "run time (min) - 0 is infinite", "S | M | L or database size (e.g. 30MB, 80GB)", "warmup_time (min)", "think time (sec)", 
      "percent of customers that are new customers", "average number of searches per order", 
      "average number of items returned in each search", "average number of items per order",
      "virtual directory (for web driver)", "web page type (for web driver)", "target hostname for Perfmon CPU% display (Windows only)",
      "username:password:target hostname/IP Address for Linux CPU% display (Linux Only)",
      "Detailed statistics View (Y / N)"};
    static string[] input_parm_values = new string[] {"none", "localhost", "1", "10", "0", "10MB", "1", "0",
      "20", "3", "5", "5", "ds2", "php", "","","N"};

    int server_id = 0;          //Added by GSK

    //
    //-------------------------------------------------------------------------------------------------
    //    
    [STAThread]
    static void Main ( string[] args )
      {
      new Controller ( args );
      }
    //
    //-------------------------------------------------------------------------------------------------
    //
    //Added by GSK to register RSA fingerprint / host key in registry before using plink to get CPU data
    bool RegisterRSAHostKey ( string machine_name , string user , string passwd )
      {
      try
        {
        Process p = new Process ( );
        //These arguments will ensure than yes = y will automatically be answered
        // -l root -pw password 11.22.33.44 exit
        string p_args = " -l " + user + " -pw " + passwd + " " + machine_name + " exit";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = false;
        //We need to set environment variable PLINK_PATH to give full path of plink.exe on machine on which driver program is executing
        p.StartInfo.FileName = System.Environment.GetEnvironmentVariable ( "PLINK_PATH");
        p.StartInfo.Arguments = p_args;
        //Run plink to register Host key in registry
        p.Start ( );
        StreamWriter strm_Writer = p.StandardInput;
        strm_Writer.AutoFlush = true;
        strm_Writer.Write ( "y" );      //This will automatically give answer as y when yes/no question is asked to add host key
        strm_Writer.Write ( "\n" );     //Simulate pressing enter key
        p.WaitForExit ( );              //Wait till process finishes
        }
      catch(System.Exception e)
        {
        //In case of any exception like error in connection to target linux host, directly throw exception to caller of this function
        throw e;
        }
      return true;
      }

    //
    //-------------------------------------------------------------------------------------------------
    //      

    //Run BackGround Mpstat to target machine
    void RunBackGroundmpStat ( string machine_name , string user , string passwd )
      {
      try
        {
        String s_retValue = "";                    
        Process p = new Process ( );
        //These arguments will ensure than yes = y will automatically be answered
        // -l root -pw password 11.22.33.44 exit
        //Submit background task to write mpstat output for 10 seconds to a file
        string p_args = " -l " + user + " -pw " + passwd + " " + machine_name + " cd /; nohup mpstat 1 10 > /cpuutil.txt 2> /cpuutil.err < /dev/null &";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = false;
        p.StartInfo.RedirectStandardOutput = true;
        //We need to set environment variable PLINK_PATH to give full path of plink.exe on machine on which driver program is executing
        p.StartInfo.FileName = System.Environment.GetEnvironmentVariable ( "PLINK_PATH");
        p.StartInfo.Arguments = p_args;
        //Run plink to get CPU utilization by running bash script on remote shell
        p.Start ( );
        StreamReader strm_Reader = p.StandardOutput;
        s_retValue = strm_Reader.ReadToEnd ( );
        p.WaitForExit ( );              //Wait till process finishes
        }
      catch ( System.Exception e )
        {
        //In case of exception throw exception directly to caller of this function
        throw e;
        }                
      }

    //Read remove text file to get CPUutilization
    double ReadRemoteTextFile ( string machine_name , string user , string passwd )
      {
      double cpuutilizn = 0.0;
      try
        {
        String s_retValue;                    
        Process p = new Process ( );
        //These arguments will ensure than yes = y will automatically be answered
        // -l root -pw password 11.22.33.44 exit
        //Submit background task to write mpstat output for 10 seconds to a file
        string p_args = " -l " + user + " -pw " + passwd + " " + machine_name + " cd /; grep 'Average:' /cpuutil.txt ; exit;";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = false;
        p.StartInfo.RedirectStandardOutput = true;
        //We need to set environment variable PLINK_PATH to give full path of plink.exe on machine on which driver program is executing
        p.StartInfo.FileName = System.Environment.GetEnvironmentVariable ( "PLINK_PATH");
        p.StartInfo.Arguments = p_args;
        //Run plink to get CPU utilization by running bash script on remote shell
        p.Start ( );
        StreamReader strm_Reader = p.StandardOutput;
        s_retValue = strm_Reader.ReadToEnd ( );
        p.WaitForExit ( );              //Wait till process finishes

        if(s_retValue == "")
          {
          throw new System.Exception("No value returned after reading file!! Check whether file created on target system or not!!");
          }

        //Remove all extra white spaces and have only one whitespace
        //String before:Average:     all   13.56    0.00    1.16    4.50    0.06    0.16    0.00    0.00   80.55
        //String after: Average: all 13.56 0.00 1.16 4.50 0.06 0.16 0.00 0.00 80.55
        s_retValue = System.Text.RegularExpressions.Regex.Replace(s_retValue,@"\s{2,}", " ");
        String[] arr_strSplit = s_retValue.Split(' ');
                    
        //Get User, Nice, System values from string and add to get CPU utilization
        cpuutilizn = Convert.ToDouble(arr_strSplit[2]) + Convert.ToDouble(arr_strSplit[3]) + Convert.ToDouble(arr_strSplit[4]);                    
        }
      catch ( System.Exception e )
        {
        //In case of exception throw exception directly to caller of this function
        throw e;
        }    
      return cpuutilizn;
      }

    //-------------------------------------------------------------------------------------------------
    //    
    //Function written by GSK to calculate number of Rows in tables of database according to database size
    void CalculateNumberOfRows ( string str_db_size )
      {
      string db_custom_size = str_db_size;
      int i_db_custom_size = 10;          //Default 10mb
      string str_is_mb_gb = "mb";
      db_custom_size = db_custom_size.ToLower ( );  //For case insensitivity
      if ( db_custom_size.IndexOf ( "mb" ) != -1 )
        {
        str_is_mb_gb = db_custom_size.Substring ( db_custom_size.IndexOf ( "mb" ) , 2 );
        try
          {
          i_db_custom_size = Convert.ToInt32 ( db_custom_size.Substring ( 0 , db_custom_size.IndexOf ( "mb" ) ) );
          if ( i_db_custom_size <= 0 )
            {
            throw new System.Exception ( "db_size must be greater than 0!!" );
            }
          }
        catch ( System.Exception e )
          {
          throw e;
          }
        }
      else if ( db_custom_size.IndexOf ( "gb" ) != -1 )
        {
        str_is_mb_gb = db_custom_size.Substring ( db_custom_size.IndexOf ( "gb" ) , 2 );
        try
          {
          i_db_custom_size = Convert.ToInt32 ( db_custom_size.Substring ( 0 , db_custom_size.IndexOf ( "gb" ) ) );
          if ( i_db_custom_size <= 0 )
            {
            throw new System.Exception ( "db_size must be greater than 0!!" );
            }
          }
        catch ( System.Exception e )
          {
          throw e;
          }
        }
      else
        {
        //Wrong parameter specified
        throw new Exception ( "Wrong value for parameter db_size specified!!" );
        }

      //Everything is OK in parameter, so now calculate number of rows in each of customers, orders and products tables
      //Note that order_rows are per month
      int mult_cust_rows = 0 , mult_ord_rows = 0 , mult_prod_rows = 0;
      double ratio = 0;
      //Size is in MB  (Database can be only in range 1 mb to 1024 mb - Small instance S)
      if ( String.Compare ( str_is_mb_gb , "mb" ) == 0 )
        {
        ratio = ( double ) ( i_db_custom_size / 10.0 );
        mult_cust_rows = 20000;
        mult_ord_rows = 1000;
        mult_prod_rows = 10000;
        }
      else if ( String.Compare ( str_is_mb_gb , "gb" ) == 0 ) //Size is in GB (database can be 1 GB (Medium instance M) or > 1 GB (Larger instance L)
        {
        if ( i_db_custom_size == 1 )  //Medium M size 1 GB database
          {
          ratio = ( double ) ( i_db_custom_size / 1.0 );
          mult_cust_rows = 2000000;
          mult_ord_rows = 100000;
          mult_prod_rows = 100000;
          }
        else  //Size > 1 GB Large L size database
          {
          ratio = ( double ) ( i_db_custom_size / 100.0 );
          mult_cust_rows = 200000000;
          mult_ord_rows = 10000000;
          mult_prod_rows = 1000000;
          }
        }

      //Initialize number of rows in table according to ratio calculated for custom database size
      customer_rows = ( int ) ( ratio * mult_cust_rows );
      order_rows = ( int ) ( ratio * mult_ord_rows );
      product_rows = ( int ) ( ratio * mult_prod_rows );

      }

    //    
    //-------------------------------------------------------------------------------------------------
    //   
    Controller ( string[] argarray )
      {
      //Console.Error.WriteLine("Controller constructor: " + argarray.Length + " args");

      int i;
            int z;
      int i_sec , run_time = 0 , warmup_time = 1;
      //Changed by GSK
      //int db_size=0;
      //string db_size_str, errmsg=null;
      string errmsg = null;
      double et;
      int opm , rt_login_avg_msec , rt_newcust_avg_msec , rt_browse_avg_msec , rt_purchase_avg_msec ,
        rt_tot_lastn_max_msec , rt_tot_avg_msec;
      double rt_tot_lastn_max;

      //Added by GSK
      int old_n_overall = 0;
      int[] arr_old_n_overall;
      int diff_n_overall = 0;
      int[] arr_diff_n_overall;
      double old_rt_tot_overall = 0.0;
      double[] arr_old_rt_tot_overall;
      double diff_rt_tot_overall = 0.0;
      double[] arr_diff_rt_tot_overall;
      int[] arr_rt_tot_sampled;
      int rt_tot_sampled = 0;

      //Added by GSK
      int[] arr_opm;
      int[] arr_rt_login_avg_msec;
      int[] arr_rt_newcust_avg_msec;
      int[] arr_rt_browse_avg_msec;
      int[] arr_rt_purchase_avg_msec;
      int[] arr_rt_tot_lastn_max_msec;
      int[] arr_rt_tot_avg_msec;
      double arr_rt_tot_lastn_max;

      //Added by GSK (Keeps track of total and utilizn of bunch of linux and windows VM's)
      double total_cpu_utilzn = 0.0;
      double total_win_cpu_utilzn = 0.0;
      double total_lin_cpu_utilzn = 0.0;

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan ( );
      DateTime DT0;
#endif

      User[] users = new User[GlobalConstants.MAX_USERS];
      Thread[] threads = new Thread[GlobalConstants.MAX_USERS];

      if ( argarray.Length == 0 )
        {
        // display input parameter info
        Console.Error.WriteLine ( "\nEnter parameters with format --parm_name=parm_value" );
        Console.Error.WriteLine ( "And/or use a config file with argument --config_file=(config file path)" );
        Console.Error.WriteLine ( "Parms will be evaluated left to right" );
        Console.Error.WriteLine ( "\n{0,-20}{1,-52}{2}\n" , "Parameter Name" , "Description" , "Default Value" );
        for ( i = 0 ; i < input_parm_names.Length ; i++ )
          {
          Console.Error.WriteLine ( "{0,-20}{1,-52}{2}" , input_parm_names[i] , input_parm_desc[i] , input_parm_values[i] );
          }
        return;
        }

      // send args to parse_args, return 0 or # of parms set, error_message if any
      // parsed values are in array input_parm_values
      i = parse_args ( argarray , ref errmsg );
      if ( i != 0 ) { }//Console.Error.WriteLine("{0} parameters parsed", i);
      else
        {
        Console.Error.WriteLine ( errmsg );
        return;
        }

      // Set parameters from input_parm_values 
      //target = input_parm_values[Array.IndexOf ( input_parm_names , "target" )];

      //Added try catch block by GSK
      try
        {

        target = input_parm_values[Array.IndexOf ( input_parm_names , "target" )];                
        target_servers = target.Split ( ';' );
        n_target_servers = target_servers.Length;   //Added by GSK to keep track of number of Target Servers
        //Added by GSK
        //Dynamically allocate memory Initialize arrays for book keeping for individual Servers on which test runs
        arr_n_login_overall = new int[n_target_servers];
        arr_rt_login_overall = new double[n_target_servers];
        arr_n_newcust_overall = new int[n_target_servers];
        arr_rt_newcust_overall = new double[n_target_servers];
        arr_n_browse_overall = new int[n_target_servers];
        arr_rt_browse_overall = new double[n_target_servers];
        arr_n_purchase_overall = new int[n_target_servers];
        arr_rt_purchase_overall = new double[n_target_servers];
        arr_n_rollbacks_overall = new int[n_target_servers];
        arr_n_overall = new int[n_target_servers];
        arr_rt_tot_overall = new double[n_target_servers];
        arr_n_purchase_from_start = new int[n_target_servers];
        arr_n_rollbacks_from_start = new int[n_target_servers];
        arr_rt_tot_lastn = new double[n_target_servers,GlobalConstants.LAST_N];

        arr_opm = new int[n_target_servers];
        arr_rt_login_avg_msec = new int[n_target_servers];
        arr_rt_newcust_avg_msec = new int[n_target_servers];
        arr_rt_browse_avg_msec = new int[n_target_servers];
        arr_rt_purchase_avg_msec = new int[n_target_servers];
        arr_rt_tot_lastn_max_msec = new int[n_target_servers];
        arr_rt_tot_avg_msec = new int[n_target_servers];

        arr_rt_tot_lastn_max = 0.0;

        old_n_overall = 0;
        diff_n_overall = 0;
        old_rt_tot_overall = 0.0;
        diff_rt_tot_overall = 0.0;

        //Added on 8/8/2010
        arr_old_n_overall = new int[n_target_servers];
        arr_diff_n_overall = new int[n_target_servers];
        arr_old_rt_tot_overall = new double[n_target_servers];
        arr_diff_rt_tot_overall = new double[n_target_servers];
        arr_rt_tot_sampled = new int[n_target_servers];

        for ( i = 0 ; i < n_target_servers ; i++ )
          {
          arr_n_login_overall[i] = 0;
          arr_rt_login_overall[i] = 0.0;
          arr_n_newcust_overall[i] = 0;
          arr_rt_newcust_overall[i] = 0.0;
          arr_n_browse_overall[i] = 0;
          arr_rt_browse_overall[i] = 0.0;
          arr_n_purchase_overall[i] = 0;
          arr_rt_purchase_overall[i] = 0.0;
          arr_n_rollbacks_overall[i] = 0;
          arr_n_overall[i] = 0;
          arr_rt_tot_overall[i] = 0.0;
          arr_n_purchase_from_start[i] = 0;
          arr_n_rollbacks_from_start[i] = 0;

          arr_opm[i] = 0;
          arr_rt_login_avg_msec[i] = 0;
          arr_rt_newcust_avg_msec[i] = 0;
          arr_rt_browse_avg_msec[i] = 0;
          arr_rt_purchase_avg_msec[i] = 0;
          arr_rt_tot_lastn_max_msec[i] = 0;
          arr_rt_tot_avg_msec[i] = 0;

          //Added on 8/8/2010
          arr_old_n_overall[i] = 0;
          arr_diff_n_overall[i] = 0;
          arr_old_rt_tot_overall[i] = 0.0;
          arr_diff_rt_tot_overall[i] = 0.0;
          arr_rt_tot_sampled[i] = 0;

          for ( int l = 0 ; l < GlobalConstants.LAST_N ; l++ )
            {
            arr_rt_tot_lastn[i,l] = 0.0;
            }
          }                
        }
        
      catch(System.Exception e)
        {
        Console.Error.WriteLine ( "Error in converting parameter target: {0}" , e.Message );
        return;
        }

      try
        {
        n_threads = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "n_threads" )] );                
                //Changed by GSK -- n_threads represents threads spawned per DB/Web Server
                //Hence total number of threads spawned by Controller Driver Program = no of threads per Server * number of servers to Drive Workload on
                n_threads = n_threads * n_target_servers;
                Console.Error.WriteLine ( "Total number of Threads to be Spawned across multiple servers are n_threads: {0}" , n_threads );
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter n_threads: {0}" , e.Message );
        return;
        }
      try
        {
        ramp_rate = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "ramp_rate" )] );
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter ramp_rate: {0}" , e.Message );
        return;
        }
      try
        {
        run_time = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "run_time" )] );
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter run_time: {0}" , e.Message );
        return;
        }


      //db_size_str = input_parm_values[Array.IndexOf(input_parm_names, "db_size_str")];

            //Changed by GSK
      //This parameter db_size_str will not be used in case of Custom database size since CalculateNumberOfRows() calculates rows in tables 
      //on the fly according to database size passed as parameter            
      //string sizes= "SML";
      //if ((db_size = sizes.IndexOf(db_size_str.ToUpper())) < 0)
      //  {
      //      Console.Error.WriteLine("Error: db_size_str must be one of S, M or L");
      //      return;
      //  }

      //Code for new parameter and new function to initialize number of rows 
      //Added by GSK
      db_size = input_parm_values[Array.IndexOf ( input_parm_names , "db_size" )];
      if ( db_size == "" )
        {
        Console.Error.WriteLine ( "Error: Wrong db_size parameter value specified" );
        return;
        }
      try
        {
        if ( db_size.ToUpper ( ) == "S" ) db_size = "10MB";        //These if and else if's are to ensure code works with older S | M | L parameters too
        else if ( db_size.ToUpper ( ) == "M" ) db_size = "1GB";
        else if ( db_size.ToUpper ( ) == "L" ) db_size = "100GB";
        CalculateNumberOfRows ( db_size );
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in Calculating number of rows in table according to db_size parameter: {0}" , e.Message );
        return;
        }

      try
        {
        warmup_time = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "warmup_time" )] );
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter warmup_time: {0}" , e.Message );
        return;
        }
      try
        {
        think_time = Convert.ToDouble ( input_parm_values[Array.IndexOf ( input_parm_names , "think_time" )] );
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter think_time: {0}" , e.Message );
        return;
        }
      try
        {
        pct_newcustomers =
          Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "pct_newcustomers" )] );
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter pct_newcustomers: {0}" , e.Message );
        return;
        }
      try
        {
        n_searches = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "n_searches" )] );
        if ( n_searches <= 0 )
          {
          Console.Error.WriteLine ( "n_searches must be greater than 0" );
          return;
          }
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter n_searches: {0}" , e.Message );
        return;
        }
      try
        {
        search_batch_size = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names ,
          "search_batch_size" )] );
        if ( search_batch_size <= 0 )
          {
          Console.Error.WriteLine ( "search_batch_size must be greater than 0" );
          return;
          }
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter search_batch_size: {0}" , e.Message );
        return;
        }
      try
        {
        n_line_items = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "n_line_items" )] );
        if ( n_line_items <= 0 )
          {
          Console.Error.WriteLine ( "n_line_items must be greater than 0" );
          return;
          }
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter n_line_items: {0}" , e.Message );
        return;
        }

      virt_dir = input_parm_values[Array.IndexOf ( input_parm_names , "virt_dir" )];
      page_type = input_parm_values[Array.IndexOf ( input_parm_names , "page_type" )];
           
      //windows_perf_host = input_parm_values[Array.IndexOf ( input_parm_names , "windows_perf_host" )];
      //if ( windows_perf_host == "" ) windows_perf_host = null;

      //Added by GSK
      try
        {
        windows_perf_host = input_parm_values[Array.IndexOf ( input_parm_names , "windows_perf_host" )];
        if ( windows_perf_host == "" )
          {
          windows_perf_host = null;
          windows_perf_host_servers = null;
          n_windows_servers = 0;
          }
        else
          {
          windows_perf_host_servers = windows_perf_host.Split ( ';' );
          n_windows_servers = windows_perf_host_servers.Length;
          is_Win_VM = true;

          //Allocate memory and initialize
          arr_cpu_pct_tot = new double[n_windows_servers];
          arr_n_cpu_pct_samples = new int[n_windows_servers];
          for ( i = 0 ; i < n_windows_servers ; i++ )
            {
            arr_cpu_pct_tot[i] = 0.0;
            arr_n_cpu_pct_samples[i] = 0;
            }
          }
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter windows_perf_host: {0}" , e.Message );
        return;
        }

      //Added by GSK for new parameter linux_perf_host only in case of linux
      try
        {
        linux_perf_host = input_parm_values[Array.IndexOf ( input_parm_names , "linux_perf_host" )];
        if ( linux_perf_host == "" )
          {
          linux_perf_host = null;
          linux_perf_host_servers = null;
          n_linux_servers = 0;
          arr_linux_cpu_utilization = null;
          }
        else
          {
          string []str_SplitSemiColons;

          str_SplitSemiColons = linux_perf_host.Split ( ';' );
                    
          n_linux_servers = str_SplitSemiColons.Length;

          linux_unames = new String[n_linux_servers];
          linux_passwd = new String[n_linux_servers];
          linux_perf_host_servers = new String[n_linux_servers];

          i = 0;
          foreach (string splitline in str_SplitSemiColons)
            {
            string []str_SplitColon = new String[3];
            str_SplitColon = splitline.Split ( ':' );
            linux_unames[i] = str_SplitColon[0];
            linux_passwd[i] = str_SplitColon[1];
            linux_perf_host_servers[i] = str_SplitColon[2];
            i++;
            }

          is_Lin_VM = true;
          arr_linux_cpu_utilization = new double[n_linux_servers];        //Used to store CPU utilizations for book keeping

          for ( i = 0 ; i < n_linux_servers ; i++ )
            {
            arr_linux_cpu_utilization[i] = 0.0;
            }
          }
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter linux_perf_host: {0}" , e.Message );
        return;
        }

      //Added by GSK
      try
        {
        detailed_view = input_parm_values[Array.IndexOf ( input_parm_names , "detailed_view" )];
        if ( detailed_view.ToUpper ( ) == "Y" )
            is_detailed_view = true;
        else if ( detailed_view.ToUpper ( ) == "N" )
            is_detailed_view = false;
        else
            throw new System.Exception ( "Wrong value of parameter detailed_view specified!!" );
        }
      catch ( System.Exception e )
        {
        Console.Error.WriteLine ( "Error in converting parameter detailed_view: {0}" , e.Message );
        return;
        }

      
      Console.Error.WriteLine ( "target= {0}  n_threads= {1}  ramp_rate= {2}  run_time= {3}  db_size= {4}" +
        "  warmup_time= {5}  think_time= {6}\npct_newcustomers= {7}  n_searches= {8}  search_batch_size= {9}" +
        "  n_line_items= {10}  virt_dir= {11}  page_type= {12}  windows_perf_host= {13} detailed_view= {14} linux_perf_host= {15}" ,
        target , n_threads , ramp_rate , run_time , db_size , warmup_time , think_time , pct_newcustomers ,
              n_searches , search_batch_size , n_line_items , virt_dir , page_type , windows_perf_host , detailed_view , linux_perf_host );

#if (USE_WIN32_TIMER)
      Console.Error.WriteLine("\nUsing WIN32 QueryPerformanceCounters for measuring response time\n");
#else
      Console.Error.WriteLine ( "\nUsing .NET DateTime for measuring response time\n" );
#endif

      //Changed by GSK
      //max_customer = MAX_CUSTOMER[db_size];
      //max_product = MAX_PRODUCT[db_size];

      max_customer = customer_rows;
      max_product = product_rows;

      //Changed by GSK (size of array prod_array = number of rows in product table + (10000 * 10)
      //Reason : Every 10000th product wil be popular and will have 10 entries in list
      //Set up array to choose product ids from, weighted with more entries for popular products
      //Popular products (in this case every 10,000th) will have 10 entries in list, others just 1
      int prod_arr_size = product_rows + 100000;
      prod_array = new int[prod_arr_size];
      i = 0;
      for ( int j = 1 ; j <= max_product ; j++ )
        {
        if ( ( j % 10000 ) == 0 ) for ( int k = 0 ; k < 10 ; k++ ) prod_array[i++] = j;
        else prod_array[i++] = j;
        }
      prod_array_size = i;
      //Console.Error.WriteLine("{0} products in array", prod_array_size);

      for ( i = 0 ; i < GlobalConstants.LAST_N ; i++ ) { rt_tot_lastn[i] = 0.0; }

#if (GEN_PERF_CTRS)      
      if (!PerformanceCounterCategory.Exists("Test")) // Create Performance Counter object if necessary
        {
        CounterCreationDataCollection CCDC = new CounterCreationDataCollection();
        CounterCreationData MaxRT = new CounterCreationData();
        MaxRT.CounterType = PerformanceCounterType.NumberOfItems32;
        MaxRT.CounterName = "MaxRT";
        CCDC.Add(MaxRT);
        CounterCreationData OPM = new CounterCreationData();
        OPM.CounterType = PerformanceCounterType.NumberOfItems32;
        OPM.CounterName = "OPM";
        CCDC.Add(OPM);       
    // For Visual Studio 2003: PerformanceCounterCategory.Create("Test", "DB Stress Data", CCDC);
        PerformanceCounterCategory.Create("Test", "DB Stress Data", PerformanceCounterCategoryType.SingleInstance, CCDC);
        Console.Error.WriteLine("Performance Counter Category Test and Counters MaxRT and OPM created");
        }          
      else
        {
        if ( !( PerformanceCounterCategory.CounterExists("MaxRT", "Test") && 
          PerformanceCounterCategory.CounterExists("OPM", "Test")) )
          { 
          PerformanceCounterCategory.Delete("Test");
          CounterCreationDataCollection CCDC = new CounterCreationDataCollection();
          CounterCreationData MaxRT = new CounterCreationData();
          MaxRT.CounterType = PerformanceCounterType.NumberOfItems32;
          MaxRT.CounterName = "MaxRT";
          CCDC.Add(MaxRT);
          CounterCreationData OPM = new CounterCreationData();
          OPM.CounterType = PerformanceCounterType.NumberOfItems32;
          OPM.CounterName = "OPM";
          CCDC.Add(OPM);       
      // For Visual Studio 2003: PerformanceCounterCategory.Create("Test", "DB Stress Data", CCDC);
          PerformanceCounterCategory.Create("Test", "DB Stress Data", PerformanceCounterCategoryType.SingleInstance, CCDC); 
          Console.Error.WriteLine
            ("Performance Counter Category Test deleted; Category Test and Counters MaxRT/OPM created");
          }
        else
          {
          Console.Error.WriteLine("Performance Counter Category Test and Counter MaxRT exist");
          }
        }
      PerformanceCounter MaxRTC = new PerformanceCounter("Test", "MaxRT", false); // Max response time
      PerformanceCounter OPMC = new PerformanceCounter("Test", "OPM", false); // Orders per minute
      
      // Read CPU Utilization % of target host (if Windows)
      //PerformanceCounter CPU_PCT = null;
      //if (windows_perf_host != null)
        //CPU_PCT = new PerformanceCounter("Processor", "% Processor Time", "_Total", windows_perf_host);
      
      //Changed by GSK
      //Need an array of PerfCounter Class objects to capture Processor Time for each Machine

      PerformanceCounter[] CPU_PCT = new PerformanceCounter[n_windows_servers];
      if (windows_perf_host != null)
        {           
        //Create PerfMon counter on Each target machine

        for ( i = 0 ; i < n_windows_servers ; i++)
          {
          CPU_PCT[i] = new PerformanceCounter("Processor", "% Processor Time", "_Total", windows_perf_host_servers[i]);
          }            
        }
        
      
#else
            Console.Error.WriteLine ( "Not generating Windows Performance Monitor Counters" );
#endif

      //for ( i = 0 ; i < n_threads ; i++ ) // Create User objects; associate each with new Thread running Emulate method
      //    {
      //    users[i] = new User ( i );
      //    threads[i] = new Thread ( new ThreadStart ( users[i].Emulate ) );
      //    }

                   
      for ( i = 0 , server_id = 0 ; i < n_threads ; i++ ) // Create User objects; associate each with new Thread running Emulate method
        {
        if ( server_id < n_target_servers )
          {
          users[i] = new User ( i , server_id );
          threads[i] = new Thread ( new ThreadStart ( users[i].Emulate ) );
          server_id++;
          }
        else if ( server_id == n_target_servers )
          {
          server_id = 0;
          users[i] = new User ( i , server_id );
          threads[i] = new Thread ( new ThreadStart ( users[i].Emulate ) );
          server_id++;
          }
        }

      //Added by GSK
      //Before each thread will try to connect to remote systems and then running the loop to start the warmup and then actual run
      //We will plink all linux targets if there are any
      //this will ensure each target is registered in registry of machine on which driver program runs
      //This will avoid giving any add RSA fingerprint message when actual run stats are getting printed out
      // 
      if (linux_perf_host != null)     //Added by GSK for getting Linux CPU Utilization
        {
        for (i = 0; i < n_linux_servers; i++)
          {
          try
            {
            RegisterRSAHostKey(linux_perf_host_servers[i].ToString(), linux_unames[i].ToString(), linux_passwd[i].ToString());
            }
          catch (System.Exception e)
            {
            Console.Error.WriteLine("Error in adding RSA fingerprint for target linux host: {0}: {1}", linux_perf_host_servers[i].ToString(), e.Message);
            return;
            }
          }
        Console.Error.WriteLine(" ");
        }


      for ( i = 0 ; i < n_threads ; i++ ) // Start threads
        {
        threads[i].Start ( );
        }

      while ( n_threads_running < n_threads ) // Wait for all threads to start
        {
        //Console.Error.WriteLine("Controller: n_threads_running = {0}", n_threads_running);
        //Console.Error.WriteLine("Controller: Thread status:");
        //for (i=0; i<n_threads; i++) Console.Error.WriteLine("  Thread {0}: {1}", i, threads[i].ThreadState);
        Thread.Sleep ( 1000 );
        }
      Console.Error.WriteLine ( "Controller ({0}): all threads running" , DateTime.Now );
      //for (i=0; i<n_threads; i++) Console.Error.WriteLine("  Thread {0}: {1}", i, threads[i].ThreadState);   

      int ConnectTimeout = 60;  // Used to limit the amount of time that driver program will try to get all threads conencted
      while ( (n_threads_connected < n_threads) && (ConnectTimeout > 0) )   
        {
        for ( int j = 0 ; j < n_threads ; j++ )  // If one of the threads has stopped quit
          if ( threads[j].ThreadState == System.Threading.ThreadState.Stopped ) return;
        Console.Error.WriteLine ( "Controller: n_threads_connected = {0} : ConnectionTimeOut remaining {1}" , n_threads_connected,ConnectTimeout );
        Thread.Sleep ( 1000 );
        --ConnectTimeout;
        }
      
      if (n_threads_connected < n_threads)   // If all threads are not connected, then timeout was exceeded
        { 
        Console.Error.WriteLine ( "Controller: ConnectTimeout reached : could not connect all threads, Aborting...");
        Thread.Sleep ( 500 );
        for ( i = 0 ; i < n_threads ; i++ )
          {
              threads[i].Abort();
          }
        return;
        }
      
      Console.Error.WriteLine ( "Controller ({0}): all threads connected - issuing Start" , DateTime.Now );
      Start = true;

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif

      if ( run_time == 0 ) run_time = 1000000;  // test run time in minutes, 0 => forever
      run_time += warmup_time;  // Add warmup time for total run time
            
      for ( i_sec = 1 ; i_sec <= run_time * 60 ; i_sec++ ) // run for run_time*60 seconds
        {          
        //Call plink to execute mpstat on remote linux machine to store CPU data in File on remote system
        if (i_sec % 10 == 1)  //At start of every 10 second interval, start background process for mpstat CPU monitoring on each linux machine
          {
          if (linux_perf_host != null)     //Added by GSK for getting Linux CPU Utilization
            {
            for (i = 0; i < n_linux_servers; i++)
              {
              try
                {
                RunBackGroundmpStat(linux_perf_host_servers[i].ToString(), linux_unames[i].ToString(), linux_passwd[i].ToString());
                }
              catch (System.Exception e)
                {
                Console.Error.WriteLine("Error in getting CPU Utilization for host: {0}: {1}", linux_perf_host_servers[i].ToString(), e.Message);
                return;
                }
              }
            }
          }

        Thread.Sleep ( 1000 );     // Update perfmon stats about every second
        Monitor.Enter ( UpdateLock );  // Block User threads from accessing code to update these values (below)       
#if (USE_WIN32_TIMER)
          QueryPerformanceCounter(ref ctr);
          et = (ctr-ctr0)/(double) freq;   
#else
        TS = DateTime.Now - DT0;
        et = TS.TotalSeconds;
#endif          
      
        //opm, rt_tot_lastn_max_msec will maintain overall runtime stats for all threads that connect to DB Servers on multiple VM's
        opm = ( int ) Math.Floor ( 60.0 * n_overall / et );
        rt_tot_lastn_max = 0.0;
        for ( int j = 0 ; j < GlobalConstants.LAST_N ; j++ )
            rt_tot_lastn_max = ( rt_tot_lastn[j] > rt_tot_lastn_max ) ? rt_tot_lastn[j] : rt_tot_lastn_max;
        rt_tot_lastn_max_msec = ( int ) Math.Floor ( 1000 * rt_tot_lastn_max );
           
        //Following code will maintain runtime stats for threads that connect to DB Servers on individual VM's
        for ( i = 0 ; i < n_target_servers ; i++ )
          {
          arr_opm[i] = ( int ) Math.Floor ( 60.0 * arr_n_overall[i] / et );
          arr_rt_tot_lastn_max = 0.0;
          for ( int m = 0 ; m < GlobalConstants.LAST_N ; m++ )
            {
            arr_rt_tot_lastn_max = ( arr_rt_tot_lastn[i , m] > arr_rt_tot_lastn_max ) ? arr_rt_tot_lastn[i , m] : arr_rt_tot_lastn_max;
            }
          arr_rt_tot_lastn_max_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_tot_lastn_max );
          }
                
                
#if (GEN_PERF_CTRS)  
          MaxRTC.RawValue = rt_tot_lastn_max_msec;
          OPMC.RawValue = opm;
          //Changed by GSK
          if (windows_perf_host != null)
            {
            //cpu_pct_tot += CPU_PCT.NextValue();
            //++n_cpu_pct_samples;

                for ( i = 0 ; i < n_windows_servers ; i++ )
                {
                    arr_cpu_pct_tot[i] += CPU_PCT[i].NextValue();
                    ++arr_n_cpu_pct_samples[i];
                }
            }
#endif               
                

        if ( i_sec % 10 == 0 ) // print out stats every 10 seconds
          {
          //rt_login_avg_msec = (int) Math.Floor(1000*rt_login_overall/n_login_overall);
          //rt_newcust_avg_msec = (int) Math.Floor(1000*rt_newcust_overall/n_newcust_overall);
          //rt_browse_avg_msec = (int) Math.Floor(1000*rt_browse_overall/n_browse_overall);
          //rt_purchase_avg_msec = (int) Math.Floor(1000*rt_purchase_overall/n_purchase_overall);
          rt_tot_avg_msec = ( int ) Math.Floor ( 1000 * rt_tot_overall / n_overall );

          //Added on 8/8/2010
          diff_n_overall = Math.Abs(n_overall - old_n_overall);
          old_n_overall = n_overall;
          diff_rt_tot_overall = Math.Abs(rt_tot_overall - old_rt_tot_overall);                    
          old_rt_tot_overall = rt_tot_overall;
          rt_tot_sampled = (int) Math.Floor(1000 * diff_rt_tot_overall / diff_n_overall);

          //Console.Error.Write ( "\n" );      
          //Console.Error.WriteLine("et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max_msec={3} rt_tot_avg_msec={4} " +
          //  "rollbacks: n={5} %={6,5:F1}  ", et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec, n_rollbacks_overall,
          //  (100.0 * n_rollbacks_overall) / n_overall);
          //Changed on 8/8/2010
          Console.Error.WriteLine("et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max_msec={3} rt_tot_avg_msec={4} " +
            "rt_tot_sampled={5} " +
            "rollbacks: n={6} %={7,5:F1} ", et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec,
            rt_tot_sampled,
            n_rollbacks_overall,(100.0 * n_rollbacks_overall) / n_overall                      
            );

          total_cpu_utilzn = 0.0;
          total_lin_cpu_utilzn = 0.0;
          total_win_cpu_utilzn = 0.0;
          if ( windows_perf_host != null )
            {
            //Changed by GSK to get total average cpu utilization                                                
            for ( i = 0 ; i < n_windows_servers ; i++ )
              {
              total_win_cpu_utilzn += ( arr_cpu_pct_tot[i] / arr_n_cpu_pct_samples[i] );
              }                        
            }                     
          if ( linux_perf_host != null )     //Added by GSK for getting Linux CPU Utilization
            {                        
            for ( i = 0 ; i < n_linux_servers ; i++ )
              {
              try
                {
                //Call plink to Read mpstat data in a text file on remote linux machine to give CPU data
                //Store CPU utilization for each linux target for bookkeeping                    
                arr_linux_cpu_utilization[i] = ReadRemoteTextFile(linux_perf_host_servers[i].ToString(), linux_unames[i].ToString(), linux_passwd[i].ToString());
                total_lin_cpu_utilzn += arr_linux_cpu_utilization[i];
                }
              catch(System.Exception e)
                {
                Console.Error.WriteLine ( "Error in getting CPU Utilization for host: {0}: {1}" , linux_perf_host_servers[i].ToString ( ) , e.Message);
                return;
                }
              }                        
            }

          if ( is_Win_VM == true && is_Lin_VM == true )       //Get perf stats from both linux and windows machines                        
            {
            total_cpu_utilzn = total_win_cpu_utilzn + total_lin_cpu_utilzn;
            //Instead of getting Sum of cpu utilization of all machines, we take average of total since it is good indication of utilization of Physical Processor
            total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
            StringBuilder sb_linux = new StringBuilder();
            for ( z= 0 ; z < n_linux_servers ; z++ )
              {
              sb_linux.Append ( linux_perf_host_servers[z] ).Append ( ";" );
              }
            Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host + ";" + sb_linux.ToString() , total_cpu_utilzn );
            }
          else if ( is_Win_VM == true && is_Lin_VM == false )  //Get perf stats from windows machines                        
            {
            total_cpu_utilzn = total_win_cpu_utilzn;
                        
            total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
            Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host  , total_cpu_utilzn );
            }
          else if ( is_Lin_VM == true && is_Win_VM == false )  //Get perf stats from linux machines                        
            {
            total_cpu_utilzn = total_lin_cpu_utilzn;
                        
            total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
            StringBuilder sb_linux = new StringBuilder ( );
            for ( z = 0 ; z < n_linux_servers ; z++ )
              {
              sb_linux.Append ( linux_perf_host_servers[z] ).Append ( ";" );
              }
            Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , sb_linux.ToString() , total_cpu_utilzn );
            }
          else
            {
                // Console.Error.Write ( "\n" );
            }
                  

          //Added by GSK
          //Call Write individual stats only when detailed_view parameter is YES and more than one target servers                   
          if ( is_detailed_view == true && n_target_servers > 1)
            {
            Console.Error.WriteLine ( "\nIndividual Stats for each DB / Web Server: " );
            for ( i = 0 ; i < n_target_servers ; i++ )
              {
              arr_rt_tot_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_tot_overall[i] / arr_n_overall[i] );

              //Added on 8/8/2010
              arr_diff_n_overall[i] = Math.Abs(arr_n_overall[i] - arr_old_n_overall[i]);
              arr_old_n_overall[i] = arr_n_overall[i];
              arr_diff_rt_tot_overall[i] = Math.Abs(arr_rt_tot_overall[i] - arr_old_rt_tot_overall[i]);
              arr_old_rt_tot_overall[i] = arr_rt_tot_overall[i];
              arr_rt_tot_sampled[i] = (int)Math.Floor(1000 * arr_diff_rt_tot_overall[i] / arr_diff_n_overall[i]);

              //Console.Error.WriteLine("et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max_msec={3} rt_tot_avg_msec={4} " +
              //  "rollbacks: n={5} %={6,5:F1}  ",
              //  et, arr_n_overall[i], arr_opm[i], arr_rt_tot_lastn_max_msec[i], arr_rt_tot_avg_msec[i], arr_n_rollbacks_overall[i],
              //  (100.0 * arr_n_rollbacks_overall[i]) / arr_n_overall[i]);
              //Changed on 8/8/2010
              Console.Error.WriteLine("et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max_msec={3} rt_tot_avg_msec={4} " +
                "rt_tot_sampled={5} " +
                "rollbacks: n={6} %={7,5:F1} ",
                et, arr_n_overall[i], arr_opm[i], arr_rt_tot_lastn_max_msec[i], arr_rt_tot_avg_msec[i], arr_rt_tot_sampled[i],
                arr_n_rollbacks_overall[i],(100.0 * arr_n_rollbacks_overall[i]) / arr_n_overall[i]                              
                );
                            

              //Added by GSK
              //Following condition i < n_windows_servers ensure that stats for windows VM's will be outputted first and then linux VM's
              //For this to work, target parameter should always specify all windows targets first followed by linux targets (all targets selerated by semi colon ;)
              if ( windows_perf_host != null && i < n_windows_servers )
                  {
                  //Need individual CPU utilization of Virtual Machines on which DB / Web Servers are running
                  Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host_servers[i] , arr_cpu_pct_tot[i] / arr_n_cpu_pct_samples[i] );
                  }
              if(linux_perf_host != null && i >= n_windows_servers)
                {
                try
                  {
                  //We only get CPU Utilization data which is book keeped in array arr_linux_cpu_utilization above
                  Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , linux_perf_host_servers[i - n_windows_servers] ,
                      arr_linux_cpu_utilization[i - n_windows_servers] );
                  }
                catch ( System.Exception e )
                  {
                  Console.Error.WriteLine ( "Error in getting CPU Utilization for host: {0}: {1}" , linux_perf_host_servers[i - n_windows_servers].ToString ( ) , e.Message );
                  return;
                  }
                }
              else Console.Error.Write ( "\n" );
              }                        
            }
      //Till this point Added by GSK                    

          for ( int j = 0 ; j < n_threads ; j++ )
            {
            if ( threads[j].ThreadState == System.Threading.ThreadState.Stopped )
              {
              Console.Error.WriteLine ( "threads[{0}].ThreadState= {1}" , j , threads[j].ThreadState );
              }
            }

          }

        Monitor.Exit ( UpdateLock );

        if ( i_sec == 60 * warmup_time ) // reset params after specified warmump
          {
          n_overall = 0; n_login_overall = 0; n_newcust_overall = 0; n_browse_overall = 0; n_purchase_overall = 0;
          n_rollbacks_overall = 0;
          rt_tot_overall = 0.0; rt_login_overall = 0.0; rt_newcust_overall = 0.0; rt_browse_overall = 0.0;
          rt_purchase_overall = 0.0;
          for ( int j = 0 ; j < GlobalConstants.LAST_N ; j++ ) rt_tot_lastn[j] = 0.0;
          cpu_pct_tot = 0.0;
          n_cpu_pct_samples = 0;

          //Added on 8/8/2010
          old_n_overall= 0;
          diff_n_overall= 0;
          old_rt_tot_overall= 0.0;
          diff_rt_tot_overall= 0.0;
          rt_tot_sampled = 0;

          //Added by GSK
          for ( i = 0 ; i < n_target_servers ; i++ )
            {
            arr_n_overall[i] = 0; 
            arr_n_login_overall[i] = 0; 
            arr_n_newcust_overall[i] = 0;
            arr_n_browse_overall[i] = 0; 
            arr_n_purchase_overall[i] = 0;
            arr_n_rollbacks_overall[i] = 0;
            arr_rt_tot_overall[i] = 0.0; 
            arr_rt_login_overall[i] = 0.0; 
            arr_rt_newcust_overall[i] = 0.0; 
            arr_rt_browse_overall[i] = 0.0;
            arr_rt_purchase_overall[i] = 0.0;

            //Added on 8/8/2010
            arr_old_n_overall[i] = 0;
            arr_diff_n_overall[i] = 0;
            arr_old_rt_tot_overall[i] = 0.0;
            arr_diff_rt_tot_overall[i] = 0.0;
            arr_rt_tot_sampled[i] = 0;

            for ( int n = 0 ; n < GlobalConstants.LAST_N ; n++ ) arr_rt_tot_lastn[i,n] = 0.0;
            
            cpu_pct_tot = 0.0;
            n_cpu_pct_samples = 0;
            }

          for ( i = 0 ; i < n_windows_servers ; i++ )
            {
            arr_n_cpu_pct_samples[i] = 0;
            arr_cpu_pct_tot[i] = 0.0;
            }

          for ( i = 0 ; i < n_linux_servers ; i++ )
            {
            arr_linux_cpu_utilization[i] = 0.0;
            }
          //Till this point Added by GSK

#if (USE_WIN32_TIMER)
          QueryPerformanceCounter(ref ctr0);   
#else
          DT0 = DateTime.Now;
#endif

          Console.Error.WriteLine ( "Stats reset" );
          }
        } // End for i_sec<run_time

      Monitor.Enter ( UpdateLock );  // Block User threads from accessing code to update these values (below)
#if (USE_WIN32_TIMER)
        QueryPerformanceCounter(ref ctr);
        et = (ctr-ctr0)/(double) freq;   
#else
      TS = DateTime.Now - DT0;
      et = TS.TotalSeconds;
#endif
            
      //Variables below will maintain Aggregate Final stats data for all DB servers running on all VM's
      opm = ( int ) Math.Floor ( 60.0 * n_overall / et );
      rt_login_avg_msec = ( int ) Math.Floor ( 1000 * rt_login_overall / n_login_overall );
      rt_newcust_avg_msec = ( int ) Math.Floor ( 1000 * rt_newcust_overall / n_newcust_overall );
      rt_browse_avg_msec = ( int ) Math.Floor ( 1000 * rt_browse_overall / n_browse_overall );
      rt_purchase_avg_msec = ( int ) Math.Floor ( 1000 * rt_purchase_overall / n_purchase_overall );
      rt_tot_lastn_max = 0.0;
      for ( int j = 0 ; j < GlobalConstants.LAST_N ; j++ )
        rt_tot_lastn_max = ( rt_tot_lastn[j] > rt_tot_lastn_max ) ? rt_tot_lastn[j] : rt_tot_lastn_max;
      rt_tot_lastn_max_msec = ( int ) Math.Floor ( 1000 * rt_tot_lastn_max );
      rt_tot_avg_msec = ( int ) Math.Floor ( 1000 * rt_tot_overall / n_overall );

      //Added by GSK
      //Variables/ Arrays below will maintain individual stats data for each DB Server running on each VM
      for ( i = 0 ; i < n_target_servers ; i++ )
        {
        arr_opm[i] = ( int ) Math.Floor ( 60.0 * arr_n_overall[i] / et );
        arr_rt_login_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_login_overall[i] / arr_n_login_overall[i] );
        arr_rt_newcust_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_newcust_overall[i] / arr_n_newcust_overall[i] );
        arr_rt_browse_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_browse_overall[i] / arr_n_browse_overall[i] );
        arr_rt_purchase_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_purchase_overall[i] / arr_n_purchase_overall[i] );
        arr_rt_tot_lastn_max = 0.0;
        for ( int p = 0 ; p < GlobalConstants.LAST_N ; p++ )
          arr_rt_tot_lastn_max = ( arr_rt_tot_lastn[i , p] > arr_rt_tot_lastn_max ) ? arr_rt_tot_lastn[i , p] : arr_rt_tot_lastn_max;
        arr_rt_tot_lastn_max_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_tot_lastn_max );
        arr_rt_tot_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_tot_overall[i] / arr_n_overall[i] );
        }
      //Till this point Added by GSK

#if (GEN_PERF_CTRS)  
        MaxRTC.RawValue = rt_tot_lastn_max_msec;
        OPMC.RawValue = opm;
#endif
      //Console.Error.WriteLine("\nFinal: et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max={3} rt_tot_avg={4} " +
      //  "n_login_overall={5} n_newcust_overall={6} n_browse_overall={7} n_purchase_overall={8} " +
      //  "rt_login_avg_msec={9} rt_newcust_avg_msec={10} rt_browse_avg_msec={11} rt_purchase_avg_msec={12} " +
      //  "n_rollbacks_overall={13} rollback_rate = {14,5:F1}%  ",
      //  et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec, n_login_overall, n_newcust_overall,
      //  n_browse_overall, n_purchase_overall, rt_login_avg_msec, rt_newcust_avg_msec, rt_browse_avg_msec,
      //  rt_purchase_avg_msec, n_rollbacks_overall, (100.0 * n_rollbacks_overall) / n_overall);
      //Changed on 8/8/2010
      Console.Error.WriteLine("\nFinal ({0}): et={1,7:F1} n_overall={2} opm={3} rt_tot_lastn_max={4} rt_tot_avg={5} " +
        "n_login_overall={6} n_newcust_overall={7} n_browse_overall={8} n_purchase_overall={9} " +
        "rt_login_avg_msec={10} rt_newcust_avg_msec={11} rt_browse_avg_msec={12} rt_purchase_avg_msec={13} " +
        "rt_tot_sampled={14} n_rollbacks_overall={15} rollback_rate = {16,5:F1}%",
        DateTime.Now, et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec, n_login_overall, n_newcust_overall,
        n_browse_overall, n_purchase_overall, rt_login_avg_msec, rt_newcust_avg_msec, rt_browse_avg_msec,
        rt_purchase_avg_msec, rt_tot_sampled, 
        n_rollbacks_overall, (100.0 * n_rollbacks_overall) / n_overall);
            

      total_cpu_utilzn = 0.0;
      total_win_cpu_utilzn = 0.0;
      total_lin_cpu_utilzn = 0.0;

      if ( windows_perf_host != null )
        {
        //Changed by GSK to get total average cpu utilization                                        
        for ( i = 0 ; i < n_windows_servers ; i++ )
          {
          total_win_cpu_utilzn += ( arr_cpu_pct_tot[i] / arr_n_cpu_pct_samples[i] );
          }                
        }            
      if ( linux_perf_host != null )     //Added by GSK for getting Linux CPU Utilization
        {                
        for ( i = 0 ; i < n_linux_servers ; i++ )
          {
          try
              {
              //Use bookkeeped CPU utilization 
              total_lin_cpu_utilzn += arr_linux_cpu_utilization[i];
              }
          catch ( System.Exception e )
              {
              Console.Error.WriteLine ( "Error in getting CPU Utilization for host: {0}: {1}" , linux_perf_host_servers[i].ToString ( ) , e.Message );
              return;
              }
            }                
          }

      if ( is_Win_VM == true && is_Lin_VM == true )       //Get perf stats from both linux and windows machines                        
        {
        total_cpu_utilzn = total_win_cpu_utilzn + total_lin_cpu_utilzn;
        //Instead of getting Sum of cpu utilization of all machines, we take average of total since it is good indication of utilization of Physical Processor
        total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
        StringBuilder sb_linux = new StringBuilder ( );
        for ( z = 0 ; z < n_linux_servers ; z++ )
          {
          sb_linux.Append ( linux_perf_host_servers[z] ).Append ( ";" );
          }
        Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host + ";" + sb_linux.ToString() , total_cpu_utilzn );
        }
      else if ( is_Win_VM == true && is_Lin_VM == false )  //Get perf stats from windows machines                        
        {
        total_cpu_utilzn = total_win_cpu_utilzn;
                
        total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
        Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host , total_cpu_utilzn );
        }
      else if ( is_Lin_VM == true && is_Win_VM == false )  //Get perf stats from linux machines                        
        {
        total_cpu_utilzn = total_lin_cpu_utilzn;
                
        total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
        StringBuilder sb_linux = new StringBuilder ( );
        for ( z = 0 ; z < n_linux_servers ; z++ )
          {
          sb_linux.Append ( linux_perf_host_servers[z] ).Append ( ";" );
          }
        Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , sb_linux.ToString() , total_cpu_utilzn );
        }
      else
        {
        Console.Error.Write ( "\n" );
        }           

      //Added by GSK
      //Call Write individual stats only when there are more than one target servers                   
      if ( n_target_servers > 1 )
        {
        Console.Error.WriteLine ( "\nIndividual Stats for each DB / Web Server: " );
        for ( i = 0 ; i < n_target_servers ; i++ )
          {
          //Console.Error.Write ( "\nFinal: et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max={3} rt_tot_avg={4} " +
          //  "n_login_overall={5} n_newcust_overall={6} n_browse_overall={7} n_purchase_overall={8} " +
          //  "rt_login_avg_msec={9} rt_newcust_avg_msec={10} rt_browse_avg_msec={11} rt_purchase_avg_msec={12} " +
          //  "n_rollbacks_overall={13} rollback_rate = {14,5:F1}% " ,
          //  et , arr_n_overall[i] , arr_opm[i] , arr_rt_tot_lastn_max_msec[i] , arr_rt_tot_avg_msec[i] , arr_n_login_overall[i] , arr_n_newcust_overall[i] ,
          //  arr_n_browse_overall[i] , arr_n_purchase_overall[i] , arr_rt_login_avg_msec[i] , arr_rt_newcust_avg_msec[i] , arr_rt_browse_avg_msec[i] ,
          //  arr_rt_purchase_avg_msec[i] , arr_n_rollbacks_overall[i] , ( 100.0 * arr_n_rollbacks_overall[i] ) / arr_n_overall[i]);
          //Changed on 8/8/2010
          Console.Error.WriteLine("Final: et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max={3} rt_tot_avg={4} " +
            "n_login_overall={5} n_newcust_overall={6} n_browse_overall={7} n_purchase_overall={8} " +
            "rt_login_avg_msec={9} rt_newcust_avg_msec={10} rt_browse_avg_msec={11} rt_purchase_avg_msec={12} " +
            "rt_tot_sampled={13} " +
            "n_rollbacks_overall={14} rollback_rate = {15,5:F1}%  ",
            et, arr_n_overall[i], arr_opm[i], arr_rt_tot_lastn_max_msec[i], arr_rt_tot_avg_msec[i], arr_n_login_overall[i], arr_n_newcust_overall[i],
            arr_n_browse_overall[i], arr_n_purchase_overall[i], arr_rt_login_avg_msec[i], arr_rt_newcust_avg_msec[i], arr_rt_browse_avg_msec[i],
            arr_rt_purchase_avg_msec[i], arr_rt_tot_sampled[i], 
            arr_n_rollbacks_overall[i], (100.0 * arr_n_rollbacks_overall[i]) / arr_n_overall[i]                      
            );

          //Added by GSK
          //Following condition i < n_windows_servers ensure that stats for windows VM's will be outputted first and then linux VM's
          //For this to work, target parameter should always specify all windows targets first followed by linux targets (all targets selerated by semi colon ;)
          if ( windows_perf_host != null && i < n_windows_servers )
            {
            //Need individual CPU utilization for Virtual Machines on which DB/ Web Servers are running
            Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host_servers[i] , arr_cpu_pct_tot[i] / arr_n_cpu_pct_samples[i] );
            }
          else if ( linux_perf_host != null && i >= n_windows_servers )     //Added by GSK for getting CPU Utilization of Linux Systems
            {          
            try
              {
              //We only get CPU Utilization data which is book keeped in array arr_linux_cpu_utilization above
              Console.Error.WriteLine ( "host {0} CPU%= {1,5:F1}" , linux_perf_host_servers[i - n_windows_servers] ,
                  arr_linux_cpu_utilization[i - n_windows_servers]);
              }
            catch ( System.Exception e )
              {
              Console.Error.WriteLine ( "Error in getting CPU Utilization for host: {0}: {1}" , linux_perf_host_servers[i - n_windows_servers].ToString ( ) , e.Message );
              return;
              }
            }
          else Console.Error.Write ( "\n" );
          }
        }                      
      //Till this point Added by GSK

      Monitor.Exit ( UpdateLock );

      // Signal threads to end, wait for 'em to stop
      End = true;
      bool all_stopped;
      do
        {
        Thread.Sleep ( 500 );
        all_stopped = true;
        for ( i = 0 ; i < n_threads ; i++ ) all_stopped &= ( threads[i].ThreadState == System.Threading.ThreadState.Stopped );
        }
      while ( !all_stopped );
      Console.Error.WriteLine ( "Controller ({0}): all threads stopped, exiting", DateTime.Now);
      Console.Error.WriteLine ( "n_purchase_from_start= {0} n_rollbacks_from_start= {1}" , n_purchase_from_start , n_rollbacks_from_start );

      //Added by GSK
      //Call Write individual stats only when there are more than one target servers                   
      if ( n_target_servers > 1 )
        {
        Console.Error.WriteLine ( "\nIndividual Stats for each DB / Web Server: " );
        for ( i = 0 ; i < n_target_servers ; i++ )
          Console.Error.WriteLine ( "n_purchase_from_start= {0} n_rollbacks_from_start= {1}", 
            arr_n_purchase_from_start[i] , arr_n_rollbacks_from_start[i] );
        }            
      //Till this point Added by GSK

  Console.WriteLine ( "Run over" );
#if (GEN_PERF_CTRS)  
      MaxRTC.RawValue = 0;
      OPMC.RawValue = 0;
#endif
      } // End of Controller() Constructor
    //
    //-------------------------------------------------------------------------------------------------
    //      
    static int parse_args ( string[] argstring , ref string errmsg )
      {
      int parm_idx = -1 , parm_count = 0;
      string[] split = null;
      string config_fname = null , parmline = null;
      char[] delimeter = { '=' };

      for ( int i = 0 ; i < argstring.Length ; i++ )
        {
        //Console.Error.WriteLine(argstring[i]);
        if ( ( argstring[i].StartsWith ( "--" ) ) && ( argstring[i].IndexOf ( '=' ) > 2 ) )
          {
          split = argstring[i].Substring ( 2 ).Split ( delimeter );
          if ( split[0] == "config_file" )
            {
            config_fname = split[1];
            if ( File.Exists ( config_fname ) )
              {
              StreamReader sr = new StreamReader ( config_fname );
              while ( ( parmline = sr.ReadLine ( ) ) != null )
                {
                //Console.Error.WriteLine(parmline);            
                if ( parmline.IndexOf ( '=' ) > 0 )
                  {
                  split = parmline.Split ( delimeter );
                  parm_idx = Array.IndexOf ( input_parm_names , split[0] );
                  if ( parm_idx > -1 )
                    {
                    //Console.Error.WriteLine("Parameter {0} parsed; was {1}, now {2}", 
                    //  split[0], input_parm_values[parm_idx], split[1]);
                    input_parm_values[parm_idx] = split[1];
                    ++parm_count;
                    }
                  else
                    {
                    errmsg = "Parameter " + split[0] + " doesn't exist";
                    return ( 0 );
                    }
                  }
                else
                  {
                  errmsg = "Incorrect format in parameter: " + argstring[i];
                  return ( 0 );
                  }
                } // End while((parmline = sr.ReadLine()) != null)
              sr.Close ( );
              }
            else
              {
              errmsg = "File " + split[1] + " doesn't exist";
              return ( 0 );
              }
            }  // End if (split[0] == "config_file")
          else  // Param is not a config file name
            {
            parm_idx = Array.IndexOf ( input_parm_names , split[0] );
            if ( parm_idx > -1 )
              {
              //Console.Error.WriteLine("Parameter {0} parsed; was {1}, now {2}", 
              //  split[0], input_parm_values[parm_idx], split[1]);
              input_parm_values[parm_idx] = split[1];
              ++parm_count;
              }
            else
              {
              errmsg = "Parameter " + split[0] + " doesn't exist";
              return ( 0 );
              }
            } // End else Param is not a config file name       
          } // End if ((argstring[i].StartsWith("--") ...
        else
          {
          errmsg = "Incorrect format in parameter: " + argstring[i];
          return ( 0 );
          }
        } // End for (int i=0; i<argstring.Length; i++)
      return ( parm_count );
      } // End of parse_args

    } // End of class Controller
  //
  //-------------------------------------------------------------------------------------------------
  //    
  class User
    {
    // If compile option /d:USE_WIN32_TIMER is specified will use 64b QueryPerformance counter from Win32
    // Else will use .NET DateTime class      
#if (USE_WIN32_TIMER)
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceCounter(ref long x);
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceFrequency(ref long x);  
#endif

    int Userid;
    ds2Interface[] ds2interfaces = new ds2Interface[GlobalConstants.MAX_USERS];
    Random r;
    string username_in , password_in , firstname_in , lastname_in , address1_in , address2_in , city_in , state_in;
    string zip_in , country_in , email_in , phone_in , creditcard_in , gender_in;
    int creditcardtype_in , ccexpmon_in , ccexpyr_in , income_in , age_in;
    string actor_in , title_in;

    public int target_server_id = 0;   //Added by GSK (Need this public since it is used by Controller to find out which thread belongs to which DB/Web Server)

    public User ( int userid )
      {
      Userid = userid;
      //Console.Error.WriteLine("user {0} created", userid);
      }

    //Added by GSK Overloaded constructor which will take care of Single instance of Driver Program driving multiple servers on ESX Host(s)
    public User ( int userid , int server_id)
      {
      Userid = userid;
      target_server_id = server_id;
      //Console.Error.WriteLine("user {0} created", userid);
      }
    //
    //-------------------------------------------------------------------------------------------------
    //
    public void Emulate ( )
      {
      int i , customerid_out = 0 , neworderid_out = 0 , rows_returned = 0;
      bool IsLogin , IsRollback;
      double rt = 0 , rt_tot , rt_login , rt_newcust , rt_browse , rt_purchase;

      string[] title_out = new string[GlobalConstants.MAX_ROWS];         // Login, Browse
      string[] actor_out = new string[GlobalConstants.MAX_ROWS];         // Login, Browse
      string[] related_title_out = new string[GlobalConstants.MAX_ROWS]; // Login
      int[] prod_id_out = new int[GlobalConstants.MAX_ROWS];             // Browse
      decimal[] price_out = new decimal[GlobalConstants.MAX_ROWS];       // Browse
      int[] special_out = new int[GlobalConstants.MAX_ROWS];             // Browse
      int[] common_prod_id_out = new int[GlobalConstants.MAX_ROWS];      // Browse
      int[] prod_id_in = new int[GlobalConstants.MAX_ROWS];              // Purchase
      int[] qty_in = new int[GlobalConstants.MAX_ROWS];                  // Purchase

      Thread.CurrentThread.Name = Userid.ToString ( );
      Console.Error.WriteLine ( "Thread {0}: created for User {1}" , Thread.CurrentThread.Name , Userid );

      lock ( typeof ( User ) )  // Only allow one instance of User to access this code at a time
        {
        ++Controller.n_threads_running;
        }

      // Create random stream r with very randomized seed
      Random rtemp = new Random ( Userid * 1000 ); // Temporary seed
      // For multi-thread runs sleep between 0 - 10 second to spread out Ticks (100 nsecs)
      if ( Controller.n_threads > 1 ) Thread.Sleep ( rtemp.Next ( 10000 ) );
      long DTNT = DateTime.Now.Ticks;
      uint lowDTNT = ( uint ) ( 0x00000000ffffffff & DTNT );
      uint rev_lowDTNT = 0;  // take low 32 bits of Tick counter and reverse them
      for ( i = 0 ; i < 32 ; i++ ) rev_lowDTNT = rev_lowDTNT | ( ( 0x1 & ( lowDTNT >> i ) ) << ( 31 - i ) );
      //Console.Error.WriteLine("DTNT= 0x{0:x16}  lowDTNT= 0x{1:x8}  rev_lowDTNT= 0x{2:x8}", DTNT, lowDTNT, rev_lowDTNT);
      r = new Random ( ( int ) rev_lowDTNT );

      //ds2interfaces[Userid] = new ds2Interface ( Userid );
      //Changed by GSK
      ds2interfaces[Userid] = new ds2Interface ( Userid , Controller.target_servers[target_server_id].ToString() );

      if ( !ds2interfaces[Userid].ds2initialize ( ) )
        {
        //Console.Error.WriteLine ( "Can't initialize " + Controller.target + "; exiting" );
        //Changed by GSK
        Console.Error.WriteLine ( "Can't initialize " + Controller.target_servers[target_server_id].ToString ( ) + "; exiting" );
        return;
        }

      // Users randomly start connecting over a (#users/ramp_rate) sec period
      Thread.Sleep ( r.Next ( ( int ) Math.Floor ( 1000.0 * Controller.n_threads / ( double ) Controller.ramp_rate ) ) );

      if ( !ds2interfaces[Userid].ds2connect ( ) )
        {
        //Console.Error.WriteLine ( "Thread {0}: can't connect to {1}; exiting" , Thread.CurrentThread.Name ,
        //  Controller.target );
        //Changed by GSK
        Console.Error.WriteLine ( "Thread {0}: can't connect to {1}; exiting" , Thread.CurrentThread.Name ,
          Controller.target_servers[target_server_id].ToString ( ) );
        return;
        }

      //Console.Error.WriteLine ( "Thread {0}: connected to {1}" , Thread.CurrentThread.Name , Controller.target );
      //Changed by GSK
      Console.Error.WriteLine ( "Thread {0}: connected to {1}" , Thread.CurrentThread.Name , Controller.target_servers[target_server_id].ToString ( ) );

      lock ( typeof ( User ) )  // Only allow one instance of User to access this code at a time
        {
        ++Controller.n_threads_connected;
        }

      // Wait for all threads to connect
      while ( !Controller.Start ) Thread.Sleep ( 100 );

      // Thread emulation loop - execute until Controller signals END      
      do
        {
        //Console.Error.WriteLine ( "Thread {0}: Running for User {1}" , Thread.CurrentThread.Name , Userid );
        // Initialize response time accumulators and other variables
        rt_tot = 0.0;  //  total response time for all phases of this emulation loop order
        rt_login = 0.0;  //  response time for login in this emulation loop
        rt_newcust = 0.0;  //  response time for new cust registration in this emulation loop
        rt_browse = 0.0;  //  total response time for browses in this emulation loop
        rt_purchase = 0.0;  //  response time for purchase in this emulation loop       

        IsLogin = false;
        IsRollback = false;

        // Login/New Customer Phase

        double user_type = r.NextDouble ( );

        if ( user_type >= Controller.pct_newcustomers / 100.0 ) // If this is true we have a returning customer 
          {
          IsLogin = true;
          //Returning user with randomized username
          int i_user = 1 + r.Next ( Controller.max_customer );
          username_in = "user" + i_user;
          password_in = "password";
          rows_returned = 0;

          if ( !ds2interfaces[Userid].ds2login ( username_in , password_in , ref customerid_out , ref rows_returned ,
            ref title_out , ref actor_out , ref related_title_out , ref rt ) )
            {
            Console.Error.WriteLine ( "Thread {0}: Error in Login for User {1}, thread exiting" ,
              Thread.CurrentThread.Name , username_in );
            return;
            }

          if ( customerid_out == 0 )
            {
            Console.Error.WriteLine ( "Thread {0}: User {1} not found, thread exiting" ,
              Thread.CurrentThread.Name , username_in );
            return;
            }

          //        Console.Error.WriteLine("Thread {0}: User {1} logged in, customerid= {2}, previous DVDs ordered= {3}, " +
          //          "RT= {4,10:F3}", Thread.CurrentThread.Name, username_in, customerid_out, rows_returned, rt);  
          //        for (i=0; i<rows_returned; i++)
          //          Console.Error.WriteLine("Thread {0}: title= {1} actor= {2} related_title= {3}", 
          //            Thread.CurrentThread.Name, title_out[i], actor_out[i], related_title_out[i]);

          rt_login = rt;
          rt_tot += rt;
          }  // end returning customer

        // New Customer with randomized username

        else   // New user
          {
          CreateUserData ( );
          do  // Try newcustomer until find a userid that doesn't exist
            {
            int i_user = 1 + r.Next ( Controller.max_customer );
            username_in = "newuser" + i_user;
            password_in = "password";

            if ( !ds2interfaces[Userid].ds2newcustomer ( username_in , password_in , firstname_in , lastname_in ,
              address1_in , address2_in , city_in , state_in , zip_in , country_in , email_in , phone_in ,
              creditcardtype_in , creditcard_in , ccexpmon_in , ccexpyr_in , age_in , income_in , gender_in ,
              ref customerid_out , ref rt ) )
              {
              Console.Error.WriteLine ( "Thread {0}: Error in Newcustomer {1}, thread exiting" ,
                Thread.CurrentThread.Name , username_in );
              return;
              }

            if ( customerid_out == 0 ) Console.Error.WriteLine ( "User name {0} already exists" , username_in );
            } while ( customerid_out == 0 ); // end of do/while try newcustomer

//        Console.Error.WriteLine("Thread {0}: New user {1} logged in, customerid = {2}, RT= {3,10:F3}", 
//           Thread.CurrentThread.Name, username_in, customerid_out, rt);  

          rt_newcust = rt;  // Just count last iteration if had to retry username
          rt_tot += rt;

          } //End of Else (new user)

        // End of Login/New Customer Phase

        // Browse Phase

        // Search Product table different ways:
        // Browse by Category: with category randomized between 1 and MAX_CATEGORY (and SPECIAL=1)
        // Browse by Actor:  with first and last names selected randomly from list of names
        // Browse by Title:  with first and last words in title selected randomly from list of title words

        string browse_type_in = "" , browse_category_in = "" , browse_actor_in = "" , browse_title_in = "";
        string browse_criteria = "";
        int batch_size_in;

        int n_browse = 1 + r.Next ( 2 * Controller.n_searches - 1 );   // Perform average of n_searches searches
        for ( int ib = 0 ; ib < n_browse ; ib++ )
          {
          batch_size_in = 1 + r.Next ( 2 * Controller.search_batch_size - 1 ); // request avg of search_batch_size lines
          int search_type = r.Next ( 3 ); // randomly select search type
          switch ( search_type )
            {
            case 0:  // Search by Category
              browse_type_in = "category";
              browse_category_in = ( 1 + r.Next ( GlobalConstants.MAX_CATEGORY ) ).ToString ( );
              browse_actor_in = "";
              browse_title_in = "";
              browse_criteria = browse_category_in;
              break;
            case 1:  // Search by Actor 
              browse_type_in = "actor";
              browse_category_in = "";
              CreateActor ( );
              browse_actor_in = actor_in;
              browse_title_in = "";
              browse_criteria = browse_actor_in;
              break;
            case 2:  // Search by Title
              browse_type_in = "title";
              browse_category_in = "";
              browse_actor_in = "";
              CreateTitle ( );
              browse_title_in = title_in;
              browse_criteria = browse_title_in;
              break;
            }

          if ( !ds2interfaces[Userid].ds2browse ( browse_type_in , browse_category_in , browse_actor_in ,
            browse_title_in , batch_size_in , customerid_out , ref rows_returned , ref prod_id_out , ref title_out ,
            ref actor_out , ref price_out , ref special_out , ref common_prod_id_out , ref rt ) )
            {
            Console.Error.WriteLine ( "Thread {0}: Error in Browse by {1}, thread exiting" , Thread.CurrentThread.Name ,
              browse_type_in );
            return;
            }

//        Console.Error.WriteLine("Thread {0}: Search by {1}={2} returned {3} DVDs ({4} requested), RT= {5,10:F3}", 
//        Thread.CurrentThread.Name, browse_type_in, browse_criteria, rows_returned, batch_size_in,rt);
//        for (i=0; i<rows_returned; i++)
//          Console.Error.WriteLine("  Thread {0}: prod_id= {1} title= {2} actor= {3} price= {4} special= {5}" + 
//            " common_prod_id= {6}", 
//            Thread.CurrentThread.Name, prod_id_out[i], title_out[i], actor_out[i],
//            price_out[i], special_out[i], common_prod_id_out[i]);

          rt_browse += rt;
          }  // End of for ib=0 to n_browse

        rt_tot += rt_browse;

        // End of Browse Phase

        // Purchase Phase

        for ( i = 0 ; i < GlobalConstants.MAX_ROWS ; i++ )
          {
          prod_id_in[i] = 0;
          qty_in[i] = 0;
          }

        // Randomize number of cart items with average n_line_items
        int cart_items = 1 + r.Next ( 2 * Controller.n_line_items - 1 );

        //For each cart item take product_id from search results or randomly select
        //for (i=0; i<cart_items; i++)
        //  {
        //  prod_id_in[i] = (rows_returned > i) ? prod_id_out[i] : (1 + r.Next(Controller.max_product));
        //  qty_in[i] = 1 + r.Next(3);  // qty (1, 2 or 3)
        //  }

        // For each cart item randomly select product_id using weighted prod_array
        for ( i = 0 ; i < cart_items ; i++ )
          {
          prod_id_in[i] = Controller.prod_array[r.Next ( Controller.prod_array_size )];
          qty_in[i] = 1 + r.Next ( 3 );  // qty (1, 2 or 3)
          //        Console.Error.WriteLine("Thread {0}: Purchase prod_id_in[{1}] = {2}  qty_in[{1}]= {3}", 
          //          Thread.CurrentThread.Name, i, prod_id_in[i], qty_in[i]);
          }

        if ( !ds2interfaces[Userid].ds2purchase ( cart_items , prod_id_in , qty_in , customerid_out , ref neworderid_out ,
          ref IsRollback , ref rt ) )
          {
          Console.Error.WriteLine ( "Thread {0}: Error in Purchase, thread exiting" , Thread.CurrentThread.Name );
          return;
          }

        //      Console.Error.WriteLine("Thread {0}: Purchase completed successfully, neworderid = {1}, rollback= {2}, " +
        //        "RT= {3,10:F3}", Thread.CurrentThread.Name, neworderid_out, IsRollback, rt);

        rt_purchase = rt;
        rt_tot += rt;

        // End of Purchase Phase
        // End of Order sequence

        // Block other User threads or Controller from accessing this code while we update these values
        Monitor.Enter ( Controller.UpdateLock );
        if ( IsLogin )
          {
          ++Controller.n_login_overall;
          ++Controller.arr_n_login_overall[target_server_id];                 //Added by GSK (all Controller class members starting with arr_%)
          Controller.rt_login_overall += rt_login;
          Controller.arr_rt_login_overall[target_server_id] += rt_login;      
          }
        else
          {
          ++Controller.n_newcust_overall;
          ++Controller.arr_n_newcust_overall[target_server_id];               
          Controller.rt_newcust_overall += rt_newcust;
          Controller.arr_rt_newcust_overall[target_server_id] += rt_newcust;  
          }
        Controller.n_browse_overall += n_browse;
        Controller.arr_n_browse_overall[target_server_id] += n_browse;          
        Controller.rt_browse_overall += rt_browse;
        Controller.arr_rt_browse_overall[target_server_id] += rt_browse;        
        ++Controller.n_purchase_overall;
        ++Controller.arr_n_purchase_overall[target_server_id];                  
        ++Controller.n_purchase_from_start;                                     
        ++Controller.arr_n_purchase_from_start[target_server_id];               
        Controller.rt_purchase_overall += rt_purchase;
        Controller.arr_rt_purchase_overall[target_server_id] += rt_purchase;    
        
        if ( IsRollback )
          {
          ++Controller.n_rollbacks_overall;
          ++Controller.arr_n_rollbacks_overall[target_server_id];             
          ++Controller.n_rollbacks_from_start;                                
          ++Controller.arr_n_rollbacks_from_start[target_server_id];          
          }

        ++Controller.n_overall;
        ++Controller.arr_n_overall[target_server_id];                                           
        Controller.rt_tot_overall += rt_tot;
        Controller.arr_rt_tot_overall[target_server_id] += rt_tot;                              
        Controller.rt_tot_lastn[Controller.n_overall % GlobalConstants.LAST_N] = rt_tot;
                
        int arrIndex = Controller.arr_n_overall[target_server_id] % GlobalConstants.LAST_N;    
        Controller.arr_rt_tot_lastn[target_server_id,arrIndex] = rt_tot;                        
                                            
        Monitor.Exit ( Controller.UpdateLock );

        Thread.Sleep ( r.Next ( 2 * ( int ) Math.Floor ( 1000 * Controller.think_time ) ) ); // Delay think time seconds               

        } while ( !Controller.End ); // End of Thread Emulation loop

      ds2interfaces[Userid].ds2close ( );

      Console.Error.WriteLine ( "Thread {0}: exiting" , Thread.CurrentThread.Name ); Console.Out.Flush ( );
      }  // End of Emulate()
    //
    //-------------------------------------------------------------------------------------------------
    //          
    void CreateUserData ( )
      {
      string[] states = new string[] {"AK", "AL", "AR", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "GA", "HI", "IA", 
                        "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MD", "ME", "MI", "MN", "MO", "MS", "MT", "NC", 
                        "ND", "NE", "NH", "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", 
                        "TX", "UT", "VA", "VT", "WA", "WI", "WV", "WY"};

      string[] countries = new string[] {"Australia", "Canada", "Chile", "China", "France", "Germany", "Japan", 
                           "Russia", "South Africa", "UK"};

      int j;
      firstname_in = ""; for ( j = 0 ; j < 6 ; j++ ) { firstname_in = firstname_in + ( char ) ( 65 + r.Next ( 26 ) ); }
      lastname_in = ""; for ( j = 0 ; j < 10 ; j++ ) { lastname_in = lastname_in + ( char ) ( 65 + r.Next ( 26 ) ); }
      city_in = ""; for ( j = 0 ; j < 7 ; j++ ) { city_in = city_in + ( char ) ( 65 + r.Next ( 26 ) ); }

      if ( r.Next ( 2 ) == 1 ) // Select region (US or ROW)
        { //ROW    
        zip_in = "";
        state_in = "";
        country_in = countries[r.Next ( 10 )];
        }
      else //US
        {
        zip_in = ( r.Next ( 100000 ) ).ToString ( );
        state_in = states[r.Next ( 50 )];
        country_in = "US";
        } //End Else

      phone_in = "" + r.Next ( 100 , 1000 ) + r.Next ( 10000000 );
      creditcardtype_in = 1 + r.Next ( 5 );
      creditcard_in = "" + r.Next ( 10000000 , 100000000 ) + r.Next ( 10000000 , 100000000 );
      ccexpmon_in = 1 + r.Next ( 12 );
      ccexpyr_in = 2008 + r.Next ( 5 );
      address1_in = phone_in + " Dell Way";
      address2_in = "";
      email_in = lastname_in + "@dell.com";
      age_in = r.Next ( 18 , 100 );
      income_in = 20000 * r.Next ( 1 , 6 ); // >$20,000, >$40,000, >$60,000, >$80,000, >$100,000
      gender_in = ( r.Next ( 2 ) == 1 ) ? "M" : "F";

      }  // End of CreateUserData

    //
    //-------------------------------------------------------------------------------------------------
    //      
    void CreateActor ( )
      {
      // Names compiled by Dara Jaffe

      // 200 actor/actress firstnames
      string[] actor_firstnames = new string[]
        {
        "ADAM", "ADRIEN", "AL", "ALAN", "ALBERT", "ALEC", "ALICIA", "ANDY", "ANGELA", "ANGELINA", "ANJELICA", 
        "ANNE", "ANNETTE", "ANTHONY", "AUDREY", "BELA", "BEN", "BETTE", "BOB", "BRAD", "BRUCE", "BURT", "CAMERON", 
        "CANDICE", "CARMEN", "CARRIE", "CARY", "CATE", "CHARLES", "CHARLIZE", "CHARLTON", "CHEVY", "CHRIS", 
        "CHRISTIAN", "CHRISTOPHER", "CLARK", "CLINT", "CUBA", "DAN", "DANIEL", "DARYL", "DEBBIE", "DENNIS", 
        "DENZEL", "DIANE", "DORIS", "DREW", "DUSTIN", "ED", "EDWARD", "ELIZABETH", "ELLEN", "ELVIS", "EMILY", 
        "ETHAN", "EWAN", "FARRAH", "FAY", "FRANCES", "FRANK", "FRED", "GARY", "GENE", "GEOFFREY", "GINA", "GLENN", 
        "GOLDIE", "GRACE", "GREG", "GREGORY", "GRETA", "GROUCHO", "GWYNETH", "HALLE", "HARRISON", "HARVEY", 
        "HELEN", "HENRY", "HILARY", "HUGH", "HUME", "HUMPHREY", "IAN", "INGRID", "JACK", "JADA", "JAMES", "JANE", 
        "JAYNE", "JEFF", "JENNIFER", "JEREMY", "JESSICA", "JIM", "JOAN", "JODIE", "JOE", "JOHN", "JOHNNY", "JON", 
        "JUDE", "JUDI", "JUDY", "JULIA", "JULIANNE", "JULIETTE", "KARL", "KATE", "KATHARINE", "KENNETH", "KEVIN", 
        "KIM", "KIRK", "KIRSTEN", "LANA", "LAURA", "LAUREN", "LAURENCE", "LEELEE", "LENA", "LEONARDO", "LIAM", 
        "LISA", "LIV", "LIZA", "LUCILLE", "MADELINE", "MAE", "MARILYN", "MARISA", "MARLENE", "MARLON", "MARY", 
        "MATT", "MATTHEW", "MEG", "MEL", "MENA", "MERYL", "MICHAEL", "MICHELLE", "MILLA", "MINNIE", "MIRA", 
        "MORGAN", "NATALIE", "NEVE", "NICK", "NICOLAS", "NICOLE", "OLYMPIA", "OPRAH", "ORLANDO", "PARKER", 
        "PAUL", "PEARL", "PENELOPE", "RALPH", "RAY", "REESE", "RENEE", "RICHARD", "RIP", "RITA", "RIVER", 
        "ROBERT", "ROBIN", "ROCK", "ROSIE", "RUBY", "RUSSELL", "SALLY", "SALMA", "SANDRA", "SCARLETT", "SEAN", 
        "SHIRLEY", "SIDNEY", "SIGOURNEY", "SISSY", "SOPHIA", "SPENCER", "STEVE", "SUSAN", "SYLVESTER", "THORA", 
        "TIM", "TOM", "UMA", "VAL", "VIVIEN", "WALTER", "WARREN", "WHOOPI", "WILL", "WILLEM", "WILLIAM", "WINONA", 
        "WOODY", "ZERO"
        };

      // 200 actor/actress lastnames  
      string[] actor_lastnames = new string[]
        {
        "AFFLECK", "AKROYD", "ALLEN", "ANISTON", "ASTAIRE", "BACALL", "BAILEY", "BALE", "BALL", "BARRYMORE", 
        "BASINGER", "BEATTY", "BENING", "BERGEN", "BERGMAN", "BERRY", "BIRCH", "BLANCHETT", "BLOOM", "BOGART", 
        "BOLGER", "BRANAGH", "BRANDO", "BRIDGES", "BRODY", "BULLOCK", "CAGE", "CAINE", "CAMPBELL", "CARREY", 
        "CHAPLIN", "CHASE", "CLOSE", "COOPER", "COSTNER", "CRAWFORD", "CRONYN", "CROWE", "CRUISE", "CRUZ", 
        "DAFOE", "DAMON", "DAVIS", "DAY", "DAY-LEWIS", "DEAN", "DEE", "DEGENERES", "DENCH", "DENIRO", 
        "DEPP", "DERN", "DIAZ", "DICAPRIO", "DIETRICH", "DOUGLAS", "DREYFUSS", "DRIVER", "DUKAKIS", "DUNST", 
        "EASTWOOD", "FAWCETT", "FIELD", "FIENNES", "FINNEY", "FISHER", "FONDA", "FORD", "FOSTER", "FREEMAN", 
        "GABLE", "GARBO", "GARCIA", "GARLAND", "GIBSON", "GOLDBERG", "GOODING", "GRANT", "GUINESS", "HACKMAN", 
        "HANNAH", "HARRIS", "HAWKE", "HAWN", "HAYEK", "HECHE", "HEPBURN", "HESTON", "HOFFMAN", "HOPE", 
        "HOPKINS", "HOPPER", "HORNE", "HUDSON", "HUNT", "HURT", "HUSTON", "IRONS", "JACKMAN", "JOHANSSON", 
        "JOLIE", "JOVOVICH", "KAHN", "KEATON", "KEITEL", "KELLY", "KIDMAN", "KILMER", "KINNEAR", "KUDROW", 
        "LANCASTER", "LANSBURY", "LAW", "LEIGH", "LEWIS", "LOLLOBRIGIDA", "LOREN", "LUGOSI", "MALDEN", "MANSFIELD", 
        "MARTIN", "MARX", "MATTHAU", "MCCONAUGHEY", "MCDORMAND", "MCGREGOR", "MCKELLEN", "MCQUEEN", "MINELLI", "MIRANDA",  
        "MONROE", "MOORE", "MOSTEL", "NEESON", "NEWMAN", "NICHOLSON", "NOLTE", "NORTON", "ODONNELL", "OLIVIER", 
        "PACINO", "PALTROW", "PECK", "PENN", "PESCI", "PFEIFFER", "PHOENIX", "PINKETT", "PITT", "POITIER", 
        "POSEY", "PRESLEY", "REYNOLDS", "RICKMAN", "ROBBINS", "ROBERTS", "RUSH", "RUSSELL", "RYAN", "RYDER", 
        "SANDLER", "SARANDON", "SILVERSTONE", "SINATRA", "SMITH", "SOBIESKI", "SORVINO", "SPACEK", "STALLONE", "STREEP", 
        "SUVARI", "SWANK", "TANDY", "TAUTOU", "TAYLOR", "TEMPLE", "THERON", "THURMAN", "TOMEI", "TORN", 
        "TRACY", "TURNER", "TYLER", "VOIGHT", "WAHLBERG", "WALKEN", "WASHINGTON", "WATSON", "WAYNE", "WEAVER", 
        "WEST", "WILLIAMS", "WILLIS", "WILSON", "WINFREY", "WINSLET", "WITHERSPOON", "WOOD", "WRAY", "ZELLWEGER"
        };

      actor_in = actor_firstnames[r.Next ( 200 )] + " " + actor_lastnames[r.Next ( 200 )];

      }  // End of CreateActor

    //
    //-------------------------------------------------------------------------------------------------
    //    
    void CreateTitle ( )
      {
      // Names compiled by Dara Jaffe

      // 1000 movie title words

      string[] movie_titles = new string[]
        {
        "ACADEMY", "ACE", "ADAPTATION", "AFFAIR", "AFRICAN", "AGENT", "AIRPLANE", "AIRPORT", "ALABAMA", "ALADDIN", 
        "ALAMO", "ALASKA", "ALI", "ALICE", "ALIEN", "ALLEY", "ALONE", "ALTER", "AMADEUS", "AMELIE", 
        "AMERICAN", "AMISTAD", "ANACONDA", "ANALYZE", "ANGELS", "ANNIE", "ANONYMOUS", "ANTHEM", "ANTITRUST", "ANYTHING", 
        "APACHE", "APOCALYPSE", "APOLLO", "ARABIA", "ARACHNOPHOBIA", "ARGONAUTS", "ARIZONA", "ARK", "ARMAGEDDON", "ARMY", 
        "ARSENIC", "ARTIST", "ATLANTIS", "ATTACKS", "ATTRACTION", "AUTUMN", "BABY", "BACKLASH", "BADMAN", "BAKED", 
        "BALLOON", "BALLROOM", "BANG", "BANGER", "BARBARELLA", "BAREFOOT", "BASIC", "BEACH", "BEAR", "BEAST", 
        "BEAUTY", "BED", "BEDAZZLED", "BEETHOVEN", "BEHAVIOR", "BENEATH", "BERETS", "BETRAYED", "BEVERLY", "BIKINI", 
        "BILKO", "BILL", "BINGO", "BIRCH", "BIRD", "BIRDCAGE", "BIRDS", "BLACKOUT", "BLADE", "BLANKET", 
        "BLINDNESS", "BLOOD", "BLUES", "BOILED", "BONNIE", "BOOGIE", "BOONDOCK", "BORN", "BORROWERS", "BOULEVARD", 
        "BOUND", "BOWFINGER", "BRANNIGAN", "BRAVEHEART", "BREAKFAST", "BREAKING", "BRIDE", "BRIGHT", "BRINGING", "BROOKLYN", 
        "BROTHERHOOD", "BUBBLE", "BUCKET", "BUGSY", "BULL", "BULWORTH", "BUNCH", "BUTCH", "BUTTERFLY", "CABIN", 
        "CADDYSHACK", "CALENDAR", "CALIFORNIA", "CAMELOT", "CAMPUS", "CANDIDATE", "CANDLES", "CANYON", "CAPER", "CARIBBEAN", 
        "CAROL", "CARRIE", "CASABLANCA", "CASPER", "CASSIDY", "CASUALTIES", "CAT", "CATCH", "CAUSE", "CELEBRITY", 
        "CENTER", "CHAINSAW", "CHAMBER", "CHAMPION", "CHANCE", "CHAPLIN", "CHARADE", "CHARIOTS", "CHASING", "CHEAPER", 
        "CHICAGO", "CHICKEN", "CHILL", "CHINATOWN", "CHISUM", "CHITTY", "CHOCOLAT", "CHOCOLATE", "CHRISTMAS", "CIDER", 
        "CINCINATTI", "CIRCUS", "CITIZEN", "CLASH", "CLEOPATRA", "CLERKS", "CLOCKWORK", "CLONES", "CLOSER", "CLUB", 
        "CLUE", "CLUELESS", "CLYDE", "COAST", "COLDBLOODED", "COLOR", "COMA", "COMANCHEROS", "COMFORTS", "COMMAND", 
        "COMMANDMENTS", "CONEHEADS", "CONFESSIONS", "CONFIDENTIAL", "CONFUSED", "CONGENIALITY", "CONNECTICUT", "CONNECTION", 
        "CONQUERER", "CONSPIRACY", "CONTACT", "CONTROL", "CONVERSATION", "CORE", "COWBOY", "CRAFT", "CRANES", "CRAZY", 
        "CREATURES", "CREEPERS", "CROOKED", "CROSSING", "CROSSROADS", "CROW", "CROWDS", "CRUELTY", "CRUSADE", "CRYSTAL", 
        "CUPBOARD", "CURTAIN", "CYCLONE", "DADDY", "DAISY", "DALMATIONS", "DANCES", "DANCING", "DANGEROUS", "DARES", 
        "DARKNESS", "DARKO", "DARLING", "DARN", "DATE", "DAUGHTER", "DAWN", "DAY", "DAZED", "DECEIVER", "DEEP", "DEER", 
        "DELIVERANCE", "DESERT", "DESIRE", "DESPERATE", "DESTINATION", "DESTINY", "DETAILS", "DETECTIVE", "DEVIL", "DIARY", 
        "DINOSAUR", "DIRTY", "DISCIPLE", "DISTURBING", "DIVIDE", "DIVINE", "DIVORCE", "DOCTOR", "DOGMA", "DOLLS", 
        "DONNIE", "DOOM", "DOORS", "DORADO", "DOUBLE", "DOUBTFIRE", "DOWNHILL", "DOZEN", "DRACULA", "DRAGON", 
        "DRAGONFLY", "DREAM", "DRIFTER", "DRIVER", "DRIVING", "DROP", "DRUMLINE", "DRUMS", "DUCK", "DUDE", 
        "DUFFEL", "DUMBO", "DURHAM", "DWARFS", "DYING", "DYNAMITE", "EAGLES", "EARLY", "EARRING", "EARTH", 
        "EASY", "EDGE", "EFFECT", "EGG", "EGYPT", "ELEMENT", "ELEPHANT", "ELF", "ELIZABETH", "EMPIRE", 
        "ENCINO", "ENCOUNTERS", "ENDING", "ENEMY", "ENGLISH", "ENOUGH", "ENTRAPMENT", "ESCAPE", "EVE", "EVERYONE", "EVOLUTION", 
        "EXCITEMENT", "EXORCIST", "EXPECATIONS", "EXPENDABLE", "EXPRESS", "EXTRAORDINARY", "EYES", "FACTORY", "FALCON", 
        "FAMILY", "FANTASIA", "FANTASY", "FARGO", "FATAL", "FEATHERS", "FELLOWSHIP", "FERRIS", "FEUD", "FEVER", 
        "FICTION", "FIDDLER", "FIDELITY", "FIGHT", "FINDING", "FIRE", "FIREBALL", "FIREHOUSE", "FISH", "FLAMINGOS", 
        "FLASH", "FLATLINERS", "FLIGHT", "FLINTSTONES", "FLOATS", "FLYING", "FOOL", "FOREVER", "FORREST", "FORRESTER", 
        "FORWARD", "FRANKENSTEIN", "FREAKY", "FREDDY", "FREEDOM", "FRENCH", "FRIDA", "FRISCO", "FROGMEN", "FRONTIER", 
        "FROST", "FUGITIVE", "FULL", "FURY", "GABLES", "GALAXY", "GAMES", "GANDHI", "GANGS", "GARDEN", 
        "GASLIGHT", "GATHERING", "GENTLEMEN", "GHOST", "GHOSTBUSTERS", "GIANT", "GILBERT", "GILMORE", "GLADIATOR", "GLASS", 
        "GLEAMING", "GLORY", "GO", "GODFATHER", "GOLD", "GOLDFINGER", "GOLDMINE", "GONE", "GOODFELLAS", "GORGEOUS", 
        "GOSFORD", "GRACELAND", "GRADUATE", "GRAFFITI", "GRAIL", "GRAPES", "GREASE", "GREATEST", "GREEDY", "GREEK", 
        "GRINCH", "GRIT", "GROOVE", "GROSSE", "GROUNDHOG", "GUMP", "GUN", "GUNFIGHT", "GUNFIGHTER", "GUYS", 
        "HALF", "HALL", "HALLOWEEN", "HAMLET", "HANDICAP", "HANGING", "HANKY", "HANOVER", "HAPPINESS", "HARDLY", 
        "HAROLD", "HARPER", "HARRY", "HATE", "HAUNTED", "HAUNTING", "HAWK", "HEAD", "HEARTBREAKERS", "HEAVEN", 
        "HEAVENLY", "HEAVYWEIGHTS", "HEDWIG", "HELLFIGHTERS", "HIGH", "HIGHBALL", "HILLS", "HOBBIT", "HOCUS", "HOLES", 
        "HOLIDAY", "HOLLOW", "HOLLYWOOD", "HOLOCAUST", "HOLY", "HOME", "HOMEWARD", "HOMICIDE", "HONEY", "HOOK", 
        "HOOSIERS", "HOPE", "HORN", "HORROR", "HOTEL", "HOURS", "HOUSE", "HUMAN", "HUNCHBACK", "HUNGER", 
        "HUNTER", "HUNTING", "HURRICANE", "HUSTLER", "HYDE", "HYSTERICAL", "ICE", "IDAHO", "IDENTITY", "IDOLS", 
        "IGBY", "ILLUSION", "IMAGE", "IMPACT", "IMPOSSIBLE", "INCH", "INDEPENDENCE", "INDIAN", "INFORMER", "INNOCENT", 
        "INSECTS", "INSIDER", "INSTINCT", "INTENTIONS", "INTERVIEW", "INTOLERABLE", "INTRIGUE", "INVASION", "IRON", "ISHTAR", 
        "ISLAND", "ITALIAN", "JACKET", "JADE", "JAPANESE", "JASON", "JAWBREAKER", "JAWS", "JEDI", "JEEPERS", 
        "JEKYLL", "JEOPARDY", "JERICHO", "JERK", "JERSEY", "JET", "JINGLE", "JOON", "JUGGLER", "JUMANJI", 
        "JUMPING", "JUNGLE", "KANE", "KARATE", "KENTUCKIAN", "KICK", "KILL", "KILLER", "KING", "KISS", 
        "KISSING", "KNOCK", "KRAMER", "KWAI", "LABYRINTH", "LADY", "LADYBUGS", "LAMBS", "LANGUAGE", "LAWLESS", 
        "LAWRENCE", "LEAGUE", "LEATHERNECKS", "LEBOWSKI", "LEGALLY", "LEGEND", "LESSON", "LIAISONS", "LIBERTY", "LICENSE", 
        "LIES", "LIFE", "LIGHTS", "LION", "LOATHING", "LOCK", "LOLA", "LOLITA", "LONELY", "LORD", 
        "LOSE", "LOSER", "LOST", "LOUISIANA", "LOVE", "LOVELY", "LOVER", "LOVERBOY", "LUCK", "LUCKY", 
        "LUKE", "LUST", "MADIGAN", "MADISON", "MADNESS", "MADRE", "MAGIC", "MAGNIFICENT", "MAGNOLIA", "MAGUIRE", 
        "MAIDEN", "MAJESTIC", "MAKER", "MALKOVICH", "MALLRATS", "MALTESE", "MANCHURIAN", "MANNEQUIN", "MARRIED", "MARS", 
        "MASK", "MASKED", "MASSACRE", "MASSAGE", "MATRIX", "MAUDE", "MEET", "MEMENTO", "MENAGERIE", "MERMAID", 
        "METAL", "METROPOLIS", "MICROCOSMOS", "MIDNIGHT", "MIDSUMMER", "MIGHTY", "MILE", "MILLION", "MINDS", "MINE", 
        "MINORITY", "MIRACLE", "MISSION", "MIXED", "MOB", "MOCKINGBIRD", "MOD", "MODEL", "MODERN", "MONEY", 
        "MONSOON", "MONSTER", "MONTEREY", "MONTEZUMA", "MOON", "MOONSHINE", "MOONWALKER", "MOSQUITO", "MOTHER", "MOTIONS", 
        "MOULIN", "MOURNING", "MOVIE", "MULAN", "MULHOLLAND", "MUMMY", "MUPPET", "MURDER", "MUSCLE", "MUSIC", 
        "MUSKETEERS", "MUSSOLINI", "MYSTIC", "NAME", "NASH", "NATIONAL", "NATURAL", "NECKLACE", "NEIGHBORS", "NEMO", 
        "NETWORK", "NEWSIES", "NEWTON", "NIGHTMARE", "NONE", "NOON", "NORTH", "NORTHWEST", "NOTORIOUS", "NOTTING", 
        "NOVOCAINE", "NUTS", "OCTOBER", "ODDS", "OKLAHOMA", "OLEANDER", "OPEN", "OPERATION", "OPPOSITE", "OPUS", 
        "ORANGE", "ORDER", "ORIENT", "OSCAR", "OTHERS", "OUTBREAK", "OUTFIELD", "OUTLAW", "OZ", "PACIFIC", 
        "PACKER", "PAJAMA", "PANIC", "PANKY", "PANTHER", "PAPI", "PARADISE", "PARIS", "PARK", "PARTY", 
        "PAST", "PATHS", "PATIENT", "PATRIOT", "PATTON", "PAYCHECK", "PEACH", "PEAK", "PEARL", "PELICAN", 
        "PERDITION", "PERFECT", "PERSONAL", "PET", "PHANTOM", "PHILADELPHIA", "PIANIST", "PICKUP", "PILOT", "PINOCCHIO", 
        "PIRATES", "PITTSBURGH", "PITY", "PIZZA", "PLATOON", "PLUTO", "POCUS", "POLISH", "POLLOCK", "POND", 
        "POSEIDON", "POTLUCK", "POTTER", "PREJUDICE", "PRESIDENT", "PRIDE", "PRIMARY", "PRINCESS", "PRIVATE", "PRIX", 
        "PSYCHO", "PULP", "PUNK", "PURE", "PURPLE", "QUEEN", "QUEST", "QUILLS", "RACER", "RAGE", 
        "RAGING", "RAIDERS", "RAINBOW", "RANDOM", "RANGE", "REAP", "REAR", "REBEL", "RECORDS", "REDEMPTION", 
        "REDS", "REEF", "REIGN", "REMEMBER", "REQUIEM", "RESERVOIR", "RESURRECTION", "REUNION", "RIDER", "RIDGEMONT", 
        "RIGHT", "RINGS", "RIVER", "ROAD", "ROBBERS", "ROBBERY", "ROCK", "ROCKETEER", "ROCKY", "ROLLERCOASTER", 
        "ROMAN", "ROOF", "ROOM", "ROOTS", "ROSES", "ROUGE", "ROXANNE", "RUGRATS", "RULES", "RUN", 
        "RUNAWAY", "RUNNER", "RUSH", "RUSHMORE", "SABRINA", "SADDLE", "SAGEBRUSH", "SAINTS", "SALUTE", "SAMURAI", 
        "SANTA", "SASSY", "SATISFACTION", "SATURDAY", "SATURN", "SAVANNAH", "SCALAWAG", "SCARFACE", "SCHOOL", "SCISSORHANDS", 
        "SCORPION", "SEA", "SEABISCUIT", "SEARCHERS", "SEATTLE", "SECRET", "SECRETARY", "SECRETS", "SENSE", "SENSIBILITY", 
        "SEVEN", "SHAKESPEARE", "SHANE", "SHANGHAI", "SHAWSHANK", "SHEPHERD", "SHINING", "SHIP", "SHOCK", "SHOOTIST", 
        "SHOW", "SHREK", "SHRUNK", "SIDE", "SIEGE", "SIERRA", "SILENCE", "SILVERADO", "SIMON", "SINNERS", 
        "SISTER", "SKY", "SLACKER", "SLEEPING", "SLEEPLESS", "SLEEPY", "SLEUTH", "SLING", "SLIPPER", "SLUMS", 
        "SMILE", "SMOKING", "SMOOCHY", "SNATCH", "SNATCHERS", "SNOWMAN", "SOLDIERS", "SOMETHING", "SONG", "SONS", 
        "SORORITY", "SOUP", "SOUTH", "SPARTACUS", "SPEAKEASY", "SPEED", "SPICE", "SPIKING", "SPINAL", "SPIRIT", 
        "SPIRITED", "SPLASH", "SPLENDOR", "SPOILERS", "SPY", "SQUAD", "STAGE", "STAGECOACH", "STALLION", "STAMPEDE", 
        "STAR", "STATE", "STEEL", "STEERS", "STEPMOM", "STING", "STOCK", "STONE", "STORM", "STORY", 
        "STRAIGHT", "STRANGELOVE", "STRANGER", "STRANGERS", "STREAK", "STREETCAR", "STRICTLY", "SUBMARINE", "SUGAR", "SUICIDES", 
        "SUIT", "SUMMER", "SUN", "SUNDANCE", "SUNRISE", "SUNSET", "SUPER", "SUPERFLY", "SUSPECTS", "SWARM", 
        "SWEDEN", "SWEET", "SWEETHEARTS", "TADPOLE", "TALENTED", "TARZAN", "TAXI", "TEEN", "TELEGRAPH", "TELEMARK", 
        "TEMPLE", "TENENBAUMS", "TEQUILA", "TERMINATOR", "TEXAS", "THEORY", "THIEF", "THIN", "TIES", "TIGHTS", 
        "TIMBERLAND", "TITANIC", "TITANS", "TOMATOES", "TOMORROW", "TOOTSIE", "TORQUE", "TOURIST", "TOWERS", "TOWN", 
        "TRACY", "TRADING", "TRAFFIC", "TRAIN", "TRAINSPOTTING", "TRAMP", "TRANSLATION", "TRAP", "TREASURE", "TREATMENT", 
        "TRIP", "TROJAN", "TROOPERS", "TROUBLE", "TRUMAN", "TURN", "TUXEDO", "TWISTED", "TYCOON", "UNBREAKABLE", 
        "UNCUT", "UNDEFEATED", "UNFAITHFUL", "UNFORGIVEN", "UNITED", "UNTOUCHABLES", "UPRISING", "UPTOWN", "USUAL", "VACATION", 
        "VALENTINE", "VALLEY", "VAMPIRE", "VANILLA", "VANISHED", "VANISHING", "VARSITY", "VELVET", "VERTIGO", "VICTORY", 
        "VIDEOTAPE", "VIETNAM", "VILLAIN", "VIRGIN", "VIRGINIAN", "VIRTUAL", "VISION", "VOICE", "VOLCANO", "VOLUME", 
        "VOYAGE", "WAGON", "WAIT", "WAKE", "WALLS", "WANDA", "WAR", "WARDROBE", "WARLOCK", "WARS", 
        "WASH", "WASTELAND", "WATCH", "WATERFRONT", "WATERSHIP", "WEDDING", "WEEKEND", "WEREWOLF", "WEST", "WESTWARD", 
        "WHALE", "WHISPERER", "WIFE", "WILD", "WILLOW", "WIND", "WINDOW", "WISDOM", "WITCHES", "WIZARD", 
        "WOLVES", "WOMEN", "WON", "WONDERFUL", "WONDERLAND", "WONKA", "WORDS", "WORKER", "WORKING", "WORLD", 
        "WORST", "WRATH", "WRONG", "WYOMING", "YENTL", "YOUNG", "YOUTH", "ZHIVAGO", "ZOOLANDER", "ZORRO", 
        };
      title_in = movie_titles[r.Next ( 1000 )] + " " + movie_titles[r.Next ( 1000 )];
      }  // End of CreateTitle   

    } // End of Class User

  } // End of Namespace ds2xdriver



