
--pgsqlds2_cleanup_large.sql: cleans up new users and orders; restart pgsql after running to reset Identity columns



alter table CUSTOMERS DISABLE TRIGGER ALL;
alter table ORDERS DISABLE TRIGGER ALL;
alter table ORDERLINES DISABLE TRIGGER ALL;
alter table CUST_HIST DISABLE TRIGGER ALL;

delete from CUSTOMERS where CUSTOMERID > 2000000;
delete from ORDERS where ORDERID > 1200000;
delete from ORDERLINES where ORDERID > 1200000;
delete from CUST_HIST where ORDERID > 1200000;

alter table CUST_HIST ENABLE TRIGGER ALL;
alter table ORDERLINES ENABLE TRIGGER ALL;
alter table ORDERS ENABLE TRIGGER ALL;
alter table CUSTOMERS ENABLE TRIGGER ALL;


drop table INVENTORY;

CREATE TABLE INVENTORY
  (
  PROD_ID INT NOT NULL PRIMARY KEY,
  QUAN_IN_STOCK INT NOT NULL,
  SALES INT NOT NULL
  )
  ;

alter table INVENTORY DISABLE TRIGGER ALL;

COPY INVENTORY FROM  '/tmp/data_files/prod/inv.csv'
  WITH DELIMITER ',' 
-- OPTIONALLY ENCLOSED BY '''
 ;

alter table INVENTORY ENABLE TRIGGER ALL;

GRANT ALL PRIVILEGES on INVENTORY to WEB;
