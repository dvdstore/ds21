ds2_mysql_readme.txt

Note: MySQL 5.5 requires some modifications to the kit. Please see ds2_lamp_setup_rhel5.txt in this directory.

DVDStore 2.1 allows to create any custom size database. 

User must use perl scripts in DVDStore 2.1 to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 2.1,
please go through document /ds2/ds2.1_Documentation.txt

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds2/ds2.1_Documentation.txt under prerequisites section)

--------------------------------------------------------------------------------------------------------------------

Instructions for building and loading the MySQL implementation of the DVD Store Version 2 (DS2) database

DS2 comes in 3 standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

Directories
-----------
./ds2/mysqlds2
./ds2/mysqlds2/build
./ds2/mysqlds2/load
./ds2/mysqlds2/load/cust
./ds2/mysqlds2/load/orders
./ds2/mysqlds2/load/prod
./ds2/mysqlds2/web
./ds2/mysqlds2/web/aspx
./ds2/mysqlds2/web/aspx/bin
./ds2/mysqlds2/web/jsp
./ds2/mysqlds2/web/php

The ./ds2/mysqlds2 directory contains two driver programs:
ds2mysqldriver.exe      (ftp in binary to a Windows machine)
ds2mysqldriver_mono.exe (run under Mono on Linux)
To see the syntax run program with no arguments on a command line.
To compile use ds2mysqlfns.cs with ./ds2/drivers/ds2xdriver.cs (see
that file's header)

The ./ds2/mysqlds2/build directory contains MySQL scripts to create the DS2
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

The ./ds2/mysqlds2/load directories contain MySQL load scripts to load the data
from the datafiles under ./ds2/data_files. You will need to modify the scripts
if the data is elsewhere.
 
The ./ds2/mysqlds2/web directories contain PHP, JSP and ASPX applications to drive DS2

The build and load of the Small DS2 database may be accomplished with the
shell script, mysqlds2_create_all.sh, in ./ds2/mysqlds2:

On MySQL machine:

Add user web with default home directory (/home/web); set password
    - As root: useradd web; passwd web
  - Fix permissions on /home directories
    - As root: chmod 755 /home/web;

1) Install MySQL
2) untar ds2.tar.gz from linux.dell.com/dvdstore
3) untar ds2_mysql.tar.gz to the same place
4) cd ./ds2/mysqlds2
5) sh mysqlds2_create_all.sh

# mysqlds2_create_all.sh
# start in ./ds2/mysqlds2
cd build
mysql -u web --password=web < mysqlds2_create_db.sql
mysql -u web --password=web < mysqlds2_create_ind.sql
mysql -u web --password=web < mysqlds2_create_sp.sql
cd ../load/cust
mysql -u web --password=web < mysqlds2_load_cust.sql
cd ../orders
mysql -u web --password=web < mysqlds2_load_orders.sql 
mysql -u web --password=web < mysqlds2_load_orderlines.sql 
mysql -u web --password=web < mysqlds2_load_cust_hist.sql 
cd ../prod
mysql -u web --password=web < mysqlds2_load_prod.sql 
mysql -u web --password=web < mysqlds2_load_inv.sql 

mysqlds2_create_all_nosp.sh is the same thing as above with the create stored
procedure line commented out

my.cnf.example is the MySQL configuration file used in our testing (copy to /etc/my.cnf)
my.cnf.example.diff shows the differences between my.cnf.example and /usr/share/mysql/my-large.cnf

monitor_load.txt describes how to monitor the load of InnoDB tables using showinnodb.sql

Most of the directories contain readme's with further instructions

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  12/16/05
