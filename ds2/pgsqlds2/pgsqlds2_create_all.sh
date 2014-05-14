# pgsqlds2_create_all.sh
# start in ./ds2/pgsqlds2
CONNSTR="-h localhost -p 5432"
# If using vFabrid Data Director vPostgres then connection string will look like this
# CONNSTR="-h {cc25670-1854-4476-9764-c384759f93d}.10.10.10.10 -p 5432"
DBNAME=ds2
SYSDBA=ds2
export PGPASSWORD="ds2"
createlang plpgsql ds2
cd build
# Assumes DB and SYSDBA are already created
# If building on vFabric Data Director vPostgres then you will need to comment out
#     pgsqlds2_create_db.sql line becuase the DB is already created
psql $CONNSTR -U $SYSDBA -d postgres < pgsqlds2_create_db.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_delete_all.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_create_tbl.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_create_sp.sql
cd ../load/cust
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_load_cust.sql
cd ../orders
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_load_orders.sql 
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_load_orderlines.sql 
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_load_cust_hist.sql 
cd ../prod
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_load_prod.sql 
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_load_inv.sql 
cd ../../build
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_create_ind.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_create_trig.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_reset_seq.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds2_create_user.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME -c "VACUUM ANALYZE;"

