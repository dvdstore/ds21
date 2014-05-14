ds2_oracle_readme.txt

DVDStore 2.1 allows to create any custom size database. 

User must use perl scripts in DVDStore 2.1 to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 2.1,
please go through document /ds2/ds2.1_Documentation.txt

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds2/ds2.1_Documentation.txt under prerequisites section)

-------------------------------------------------------------------------------------------------------------------------------------

Instructions for building and loading the Oracle implementation of the DVD Store Version 2 (DS2) database

DS2 comes in 3 standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

Directories
-----------
./ds2/oracleds2
./ds2/oracleds2/build
./ds2/oracleds2/build/standard
./ds2/oracleds2/load
./ds2/oracleds2/load/cust
./ds2/oracleds2/load/cust/standard
./ds2/oracleds2/load/orders
./ds2/oracleds2/load/orders/standard
./ds2/oracleds2/load/prod
./ds2/oracleds2/web
./ds2/oracleds2/web/jsp
./ds2/oracleds2/web/springsource

The ./ds2/oracleds2/build directory contains Oracle scripts to create the DS2
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

The ./ds2/oracleds2/load directories contain Oracle load scripts to load the data
from the datafiles under ./ds2/data_files. You will need to modify the scripts
if the data is elsewhere.
 
The ./ds2/oracleds2/build/standard, ./ds2/oracleds2/load/cust/standard,  and ./ds2/oracleds2/load/orders/standard 
directories contain scripts to build the database schema for Oracle Standard Edition (no partitions). To use these you must 
copy them into their respective parent directories or modify the scripts

The ./ds2/oracleds2/web/jsp directory contains a Java Server Pages application to drive DS2

The ./ds2/oracleds2/web/springsource directory contains a SpringSource web Pages application to drive DS2

The build and load of the Small DS2 database may be accomplished with the
shell script, oracleds2_create_all.sh, in ./ds2/oracleds2. For details see 
build/ds2_oracle_build_readme.txt
                                                                            
In order to run the sh scripts on a windows system a sh utility of some sort is required. 

A C# .NET driver program is available:
ds2oracledriver.exe      (ftp in binary to a Windows machine)
To see the syntax run program with no arguments on a command line.
To compile use ds2oraclefns.cs with ./ds2/drivers/ds2xdriver.cs (see that file's header)

ds2oracledriver.exe is now compiled with the 64b Oracle 11g Oracle Data Provider for .NET
		
For backward compatibility the 32b client driver has been retained with the filename ds2oracledriver_32b_client.exe
and the source file has been renamed ds2oraclefns_32b_client.cs.


Most of the directories contain readme's with further instructions

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  5/12/11
