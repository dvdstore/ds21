

ds2_sqlserver_build_readme.txt

DVDStore 2.1 allows to create any custom size database. 

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

The ./ds2/sqlserverds2/build directory contains SQL Server scripts to create the DS2
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

Instructions for building the small (10MB) DS SQL Server database 
(assumes create files and data under c: and SQL Server files under c:\sql\dbfiles)

On SQL Server machine:

 1) Install SQL Server
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

14) to run statistics:
SQL Server 2000:
C:\Program Files\Microsoft SQL Server\MSSQL\Binn\sqlmaint.exe -U sa -P -S localhost -D DS2 -UpdOptiStats 18
SQL Server 2005:
C:\Program Files\Microsoft SQL Server\MSSQL.1\MSSQL\Binn\sqlmaint.exe -U sa -P -S localhost -D DS2 -UpdOptiStats 18

Steps 5 - 13 can be done in one call:
in c:\ds2\sqlserverds2: osql -Usa -P -i sqlserverds2_create_all.sql
   

To build large database you will need to create data files (preferably in
Linux due to larger RAND_MAX) using scripts in ./ds2/data_files, modify the
load programs to point to these files, and modify sqlserverds2_create_db_large.sql 
to point to where you want the SQL Server files to reside

Note: you can run osql from client machine with SQL Server Client Tools installed
using either hostname or IP address:
osql -Usa -P -S hostname -i sds_create_db.sql
  or
osql -Usa -P -S IPaddress -i sds_create_db.sql
but notice that directories referenced in the called sql scripts will refer to the 
directory structure of the target machine

Most of the directories contain readme's with further instructions

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  6/28/05

