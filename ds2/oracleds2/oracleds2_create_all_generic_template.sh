#!/bin/sh
cd ./build
sqlplus "/ as sysdba" @{TBLSPACE_SQLFNAME}
sqlplus "/ as sysdba" @{CREATEDB_SQLFNAME}
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

