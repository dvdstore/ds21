

ds2_sqlserver_readme.txt


DVDStore 2.1 allows user to create any custom size database. 

User must use perl scripts in DVDStore 2.1 to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 2.1,
please go through document /ds2/ds2.1_Documentation.txt

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds2/ds2.1_Documentation.txt under prerequisites section)

-------------------------------------------------------------------------------------------------------------------------------------


Instructions for building and loading the SQL Server implementation of the DVD Store Version 2 (DS2) database

DS2 comes in 3 standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

Directories
-----------
./ds2/sqlserverds2
./ds2/sqlserverds2/build
./ds2/sqlserverds2/load
./ds2/sqlserverds2/load/cust
./ds2/sqlserverds2/load/orders
./ds2/sqlserverds2/load/prod

The ./ds2/sqlserverds2 directory contains a driver program:
ds2sqlserverdriver.exe      
To see the syntax run it with no arguments on a command line.
To compile use ds2sqlserverfns.cs with ./ds2/data_files/drivers/ds2xdriver.cs (see
that file's header).

The ./ds2/sqlserverds2/build directory contains SQL Server scripts to create the DS2
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

The ./ds2/sqlserverds2/load directories contain SQL Server load scripts to load the data
from the datafiles under ./ds2/data_files. You will need to modify the scripts
if the data is elsewhere. (Assumes data is in c:\ds2\data_files in Windows)
 

Instructions for building the small (10MB) DS SQL Server database 
(assumes create files and data under c: and SQL Server files under c:\sql\dbfiles).
Add sa password after -P if not blank.

On SQL Server machine:

 1) Install SQL Server (be sure full-text search is enabled)
 2) untar ds2.tar.gz to c:
 3) untar ds2_sqlserver.tar.gz to c:
 4) Create directory c:\sql\dbfiles 

 5) in c:\ds2\sqlserverds2\build:       osql -Usa -P -i sqlserverds2_create_db_small.sql
 6) in c:\ds2\sqlserverds2\load\cust:   osql -Usa -P -i sqlserverds2_load_cust.sql
 7) in c:\ds2\sqlserverds2\load\orders: osql -Usa -P -i sqlserverds2_load_orders.sql
 8) in c:\ds2\sqlserverds2\load\orders: osql -Usa -P -i sqlserverds2_load_orderlines.sql
 9) in c:\ds2\sqlserverds2\load\orders: osql -Usa -P -i sqlserverds2_load_cust_hist.sql
10) in c:\ds2\sqlserverds2\load\prod:   osql -Usa -P -i sqlserverds2_load_prod.sql
11) in c:\ds2\sqlserverds2\load\prod:   osql -Usa -P -i sqlserverds2_load_inv.sql
12) in c:\ds2\sqlserverds2\build:       osql -Usa -P -i sqlserverds2_create_ind.sql
13) in c:\ds2\sqlserverds2\build:       osql -Usa -P -i sqlserverds2_create_sp.sql
14) in c:\ds2\sqlserverds2\build:       osql -Usa -P -i sqlserverds2_create_user.sql

15) to run statistics: 
SQL Server 2000:
C:\"Program Files"\"Microsoft SQL Server"\MSSQL\Binn\sqlmaint.exe -U sa -P -S localhost -D DS2 -UpdOptiStats 18
SQL Server 2005:
C:\"Program Files"\"Microsoft SQL Server"\MSSQL.1\MSSQL\Binn\sqlmaint.exe -U sa -P -S localhost -D DS2 -UpdOptiStats 18
SQL Server 2008 
In SQL Server 2008 Management Studio(GUI), follow following steps:
	1) Go to Object Explorer and click and expand database server tree.
	2) Under server tree, expand management and right click on maintenance plans.
	3) Left Click on "Maintenance Plan Wizard Option".
	4) In the wizard opened, click next and enter name of plan as "ds2".
	5) Click next and check "Update Statistics" checkbox and again click next.
	6) Click next and then choose database as DS2 and click OK.
	7) Ensure "All existing statistics" and "Sample By" checkbox are set along with value "18" "percent".
	8) Once above step is done click next twice to create a task under "Maintenance Plans" under "Management" object under SQL Server tree.
	9) Now right click on this task "ds2" created from above steps and it will show a menu option for right click.
	10)Click execute to update statistics on all tables in DS2 database using task created due to above steps.

Steps 5 - 14 can be done in one call:
in c:\ds2\sqlserverds2: osql -Usa -P -i sqlserverds2_create_all_small.sql
   

To build large database you will need to create data files (preferably in
Linux due to larger RAND_MAX) using scripts in ./ds2/data_files, modify the
load programs to point to these files, and modify sqlserverds2_create_db.sql 
to point to where you want the SQL Server files to reside

Note: you can run osql from client machine with SQL Server Client Tools installed
using either hostname or IP address:
osql -Usa -P -S hostname -i sds_create_db.sql
  or
osql -Usa -P -S IPaddress -i sds_create_db.sql
but notice that directories referenced in the called sql scripts will refer to the 
directory structure of the target machine

Most of the directories contain readme's with further instructions

Note: if using DS2.1 driver program with SQL database created with D2.0 it is necessary to run step 14 above to create
user ds2user which is used by new driver program rather than user sa.

Note: with SQL Server 2005 we have noticed a problem with the initial full
text search hanging. It may be necessary to "warmup" the SQL 2005 database
with a short (1 min) run with the driver and then run a longer test.

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  7/1/07
SQL Server 2008 statistics generation procedure added by Girish Khadke 8/2010


