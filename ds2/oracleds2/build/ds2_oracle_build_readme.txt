
ds2_oracle_build_readme.txt

Instructions for building and loading the Oracle implementation of the DVD Store Version 2 (DS2) database

DS2 comes in 3 standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

The ./ds2/oracleds2/build directory contains Oracle scripts to create the DS2
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

The scripts in this directory use data partitions and thus require Oracle
Enterprise Edition. Standard edition files are included in the standard
subdirectory.

The build and load of the Small DS2 database may be accomplished with the
shell script, oracleds2_create_all.sh, in ./ds2/oracleds2:

1) Install Oracle
2) untar ds2.tar.gz from linux.dell.com/dvdstore
3) untar ds2_oracle.tar.gz to the same place
For linux make sure the oracle files are owned by your Oracle installer user/group (by default oracle/oinstall)
4) Modify ./ds/oracleds2/build/oracleds2_create_tablespaces_small.sql to
point to directory where Oracle datafiles go. Samples are included for both
Windows (directory c:\oracledbfiles) and Linux (/oracledbfiles)
Change the password in the connect statement to your sys password
5) In directory ./ds2/oracleds2: sh oracleds2_create_all.sh

# oracleds2_create_all.sh
# start in ./ds2/oracleds2
cd build
sqlplus "/ as sysdba" @oracleds2_create_tablespaces_small.sql
sqlplus "/ as sysdba" @oracleds2_create_db_small.sql
cd ../load/cust
sh oracleds2_cust_sqlldr.sh
cd ../orders
sh oracleds2_orders_sqlldr.sh
sh oracleds2_orderlines_sqlldr.sh
sh oracleds2_cust_hist_sqlldr.sh
cd ../prod
sh oracleds2_prod_sqlldr.sh
sh oracleds2_inv_sqlldr.sh
cd ../../build
sqlplus ds2/ds2 @oracleds2_create_ind.sql
sqlplus ds2/ds2 @oracleds2_create_fulltextindex.sql
sqlplus ds2/ds2 @oracleds2_create_sp.sql

In order to run the sh scripts on a windows system a sh utility of some sort is required. 

To build Medium or Large database you will need to create data files (preferably in
Linux due to larger RAND_MAX) using scripts in ./ds2/data_files, modify the
load programs to point to these files (if necessary), and either modify
oracleds2_create_all.sh or run the appropriate scripts manually. The oracleds2_create_all_large.sh 
will automate the creation of the large database.

For best performance run oracleds2_analyze_all.sql to analyze all tables and indexes:
run sqlplus ds2/ds2@db @oracleds2_analyze_all from command line, then enter / in sqlplus to execute

oracleds2_create_tablespaces_large_asm.sql is a sample script using Oracle Automated Storage Management (ASM).

oracleds2_cleanup_small.sh (and medium and large versions) will restore the
database to its original condition. The INVENTORY table is completely reloaded
so the scripts will need to be modified if the data directory is not in the
default location. Also, it has been found to be much quicker to clean the
Large database with one foreign key disabled. For this reason
oracleds2_cleanup_large.sh points to oracleds2_cleanup_large_fk_disabled.sql.

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  10/24/06
