
select count(*) as "New Orderlines" from DS2.ORDERLINES where TO_DATE('2009-12-31', 'yyyy-mm-dd') < ORDERDATE;
delete from DS2.ORDERLINES where TO_DATE('2009-12-31', 'yyyy-mm-dd') < ORDERDATE;

select count(*) as "New Orders" from DS2.ORDERS where TO_DATE('2009-12-31', 'yyyy-mm-dd') < ORDERDATE;
delete from DS2.ORDERS where TO_DATE('2009-12-31', 'yyyy-mm-dd') < ORDERDATE;

select count(*) as "New Customers" from DS2.CUSTOMERS where customerid > 20000;
delete from DS2.CUSTOMERS where customerid > 20000;


select count(*) as "Reorder table size" from DS2.REORDER;
delete from DS2.REORDER;

delete from DS2.INVENTORY;

-- Sequences


DROP SEQUENCE "DS2"."CUSTOMERID_SEQ";
CREATE SEQUENCE "DS2"."CUSTOMERID_SEQ" 
  INCREMENT BY 1 
  START WITH 20001 
--START WITH 2000001
--START WITH 200000001
  MAXVALUE 1.0E28 
  MINVALUE 1 
  NOCYCLE 
  CACHE 100000 
  NOORDER
  ;

DROP SEQUENCE "DS2"."ORDERID_SEQ";
CREATE SEQUENCE "DS2"."ORDERID_SEQ" 
  INCREMENT BY 1 
  START WITH 20001
--START WITH 2000001
--START WITH 200000001 
  MAXVALUE 1.0E28 
  MINVALUE 1 
  NOCYCLE 
  CACHE 100000 
  NOORDER
  ;
commit;

exit;
