This is the original readme for DVD Store Version 2.0 - Please see
ds2.1_Documentation.txt for more complete information for 2.1

---------------------------------------------------------------

Instructions for building and driving DVD Store Version 2 (DS2)
database

The DVD Store Version 2 (DS2) is a complete online e-commerce test
application, with a backend database component, a web application
layer, and driver programs.  The goal in designing the database
component as well as the midtier application was to utilize many
advanced database features (transactions, stored procedures, triggers,
referential integity) while keeping the database easy to install and
understand. The DS2 workload may be used to test databases or as a
stress tool for any purpose.

The distribution includes code for SQL Server, Oracle and MySQL.
Included in the release are data generation programs, shell scripts to 
build data for 10MB, 1GB and 100 GB versions of the DVD Store, database 
build scripts and stored procedure, PHP web pages, and a C# driver program.

The DS2 files are separated into database-independent data load files
under ./ds2/data_files and driver programs under ./ds2/drivers
and database-specific build scripts, loader programs, and driver
programs in directories
./ds2/mysqlds2
./ds2/oracleds2
./ds2/sqlserverds2

file ds2.tar.gz contains ./ds2/data_files and ./ds2/drivers
file ds2_mysql.tar.gz contains ./ds2/mysqlds2
file ds2_oracle.tar.gz contains ./ds2/oracleds2
file ds2_sqlserver.tar.gz contains ./ds2/sqlserverds2

To install:

In the directory in which you want to install ds2:
tar -xvzf ds2.tar.gz

and then install the implementation(s) of interest:
tar -xvzf ds2_mysql.tar.gz
tar -xvzf ds2_oracle.tar.gz
tar -xvzf ds2_sqlserver.tar.gz

The loader programs use relative addressing to reference the data
files. They will need to be changed if the data files are placed
elsewhere.

DS2 comes in 3 standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

ds2.tar.gz contains data files for the Small version.

Most of the directories contain readme's with further instructions

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  7/18/06
