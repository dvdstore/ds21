ds2_drivers_readme.txt


In this directory are the components necessary to create a database-independent web driver as well as
instructions for compiling database-dependent direct drivers.

When compiled with the appropriate set of web- or database-dependent functions, ds2xdriver generates 
orders against the DVD Store Database V.2 through web interface or directly against database.
Simulates users logging in to store or creating new customer data; browsing for DVDs by title, actor or 
category, and purchasing selected DVDs

Compile with appropriate functions file to generate driver for web, SQL Server, MySQL or Oracle target:
 csc /out:ds2webdriver.exe       ds2xdriver.cs ds2webfns.cs       /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS
 csc /out:ds2sqlserverdriver.exe ds2xdriver.cs ds2sqlserverfns.cs /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS
 csc /out:ds2mysqldriver.exe     ds2xdriver.cs ds2mysqlfns.cs     /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  /r:<path>MySql.Data.dll
 csc /out:ds2oracledriver.exe    ds2xdriver.cs ds2oraclefns.cs    /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  /r:<path>Oracle.DataAccess.dll
 csc /out:ds2pgsqldriver.exe     ds2xdriver.cs ds2pgsqlfns.cs     /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  "/r:<path>Npgsql.dll

 USE_WIN32_TIMER: if defined, program will use high resolution WIN32 timers (not supported in mono)
 GEN_PERF_CTRS: if defined, program will generate Windows Perfmon performance counters (not supported in mono) 

The database functions files are found in the database directories, i.e.
./ds2/mysqlds2
./ds2/sqlserverds2
./ds2/oracleds2
./ds2/pgsqlds2

csc is installed with Microsoft.NET   Typical location: C:\WINNT\Microsoft.NET\Framework\v2.0.50727

To see syntax, type program name by itself on a command line.

Note: the MySQL direct driver requires the MySQL Connector.NET
      the Oracle direct driver requires the Oracle Data Provider for .NET
      the PostgreSQL drirect driver requires Npgsql, the Postgresql Data Provider for .Net

Directory structure:

./ds2/drivers/ds2xdriver.cs          main driver program
./ds2/drivers/ds2xdriver.cs2003      main driver program for Visual Studio 2003
./ds2/drivers/ds2webfns.cs           web driver functions
./ds2/drivers/ds2webdriver.exe       web driver compiled as above
./ds2/drivers/ds2webdriver_mono.exe  web driver compiled without defines, for mono

To know more about DVDStore v2.1 and how to use driver programs in DVDStore 2.1, please go through document ds2.1_Documentation.txt 
under /ds2/ folder.

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  6/29/06
