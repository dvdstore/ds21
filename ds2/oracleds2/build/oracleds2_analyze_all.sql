-- run from commmand line (sqlplus ds2/ds2@db @oracleds2_analyze_all), then enter / in sqlplus to execute
declare
begin
dbms_stats.gather_table_stats(ownname=> 'DS2', tabname=> 'CATEGORIES', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'PK_CATEGORIES', partname=> NULL );
dbms_stats.gather_table_stats(ownname=> 'DS2', tabname=> 'PRODUCTS', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'PK_PROD_ID', partname=> NULL );
dbms_stats.gather_table_stats(ownname=> 'DS2', tabname=> 'INVENTORY', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'IX_INV_PROD_ID', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'IX_ACTOR_TEXT', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'IX_TITLE_TEXT', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'IX_PROD_CATEGORY', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'IX_PROD_SPECIAL', partname=> NULL );
dbms_stats.gather_table_stats(ownname=> 'DS2', tabname=> 'CUSTOMERS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'PK_CUSTOMERS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_table_stats(ownname=> 'DS2', tabname=> 'CUST_HIST', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'PK_CUST_HIST', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'IX_CUST_USERNAME', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_table_stats(ownname=> 'DS2', tabname=> 'ORDERS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'PK_ORDERS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_table_stats(ownname=> 'DS2', tabname=> 'ORDERLINES', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS2', indname=> 'PK_ORDERLINES', partname=> NULL , estimate_percent=> 18 );
end;
.
