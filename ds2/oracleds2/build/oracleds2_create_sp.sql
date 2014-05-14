  
-- DS2 Stored Procedures Build Scripts
-- Dave Jaffe, Todd Muirhead and Deepak Janakiraman   Last modified 12/03/07
-- Copyright Dell Inc. 2007 


CREATE GLOBAL TEMPORARY TABLE derivedtable1 
  ON COMMIT PRESERVE ROWS
  AS SELECT PRODUCTS.TITLE, PRODUCTS.ACTOR, PRODUCTS.PROD_ID, PRODUCTS.COMMON_PROD_ID
  FROM DS2.CUST_HIST INNER JOIN
    DS2.PRODUCTS ON CUST_HIST.PROD_ID = PRODUCTS.PROD_ID;

  
CREATE OR REPLACE  PROCEDURE "DS2"."NEW_CUSTOMER" 
  (
  firstname_in DS2.CUSTOMERS.FIRSTNAME%TYPE,
  lastname_in DS2.CUSTOMERS.LASTNAME%TYPE,
  address1_in DS2.CUSTOMERS.ADDRESS1%TYPE,
  address2_in DS2.CUSTOMERS.ADDRESS2%TYPE,
  city_in DS2.CUSTOMERS.CITY%TYPE,
  state_in DS2.CUSTOMERS.STATE%TYPE,
  zip_in DS2.CUSTOMERS.ZIP%TYPE,
  country_in DS2.CUSTOMERS.COUNTRY%TYPE,
  region_in DS2.CUSTOMERS.REGION%TYPE,
  email_in DS2.CUSTOMERS.EMAIL%TYPE,
  phone_in DS2.CUSTOMERS.PHONE%TYPE,
  creditcardtype_in DS2.CUSTOMERS.CREDITCARDTYPE%TYPE,
  creditcard_in DS2.CUSTOMERS.CREDITCARD%TYPE,
  creditcardexpiration_in DS2.CUSTOMERS.CREDITCARDEXPIRATION%TYPE,
  username_in DS2.CUSTOMERS.USERNAME%TYPE,
  password_in DS2.CUSTOMERS.PASSWORD%TYPE,
  age_in DS2.CUSTOMERS.AGE%TYPE,
  income_in DS2.CUSTOMERS.INCOME%TYPE,
  gender_in DS2.CUSTOMERS.GENDER%TYPE,
  customerid_out OUT INTEGER
  )
  IS
  rows_returned INTEGER;
  BEGIN

    SELECT COUNT(*) INTO rows_returned FROM CUSTOMERS WHERE USERNAME = username_in;

    IF rows_returned = 0
    THEN
      SELECT CUSTOMERID_SEQ.NEXTVAL INTO customerid_out FROM DUAL;
      INSERT INTO CUSTOMERS
        (
        CUSTOMERID,
        FIRSTNAME,
        LASTNAME,
        EMAIL,
        PHONE,
        USERNAME,
        PASSWORD,
        ADDRESS1,
        ADDRESS2,
        CITY,
        STATE,
        ZIP,
        COUNTRY,
        REGION,
        CREDITCARDTYPE,
        CREDITCARD,
        CREDITCARDEXPIRATION,
        AGE,
        INCOME,
        GENDER
        )
      VALUES
        (
        customerid_out,
        firstname_in,
        lastname_in,
        email_in,
        phone_in,
        username_in,
        password_in,
        address1_in,
        address2_in,
        city_in,
        state_in,
        zip_in,
        country_in,
        region_in,
        creditcardtype_in,
        creditcard_in,
        creditcardexpiration_in,
        age_in,
        income_in,
        gender_in
        )
        ;
      COMMIT;

    ELSE customerid_out := 0;

    END IF;

    END NEW_CUSTOMER;
/


CREATE OR REPLACE  PROCEDURE "DS2"."LOGIN" 
  (
  username_in        IN  DS2.CUSTOMERS.USERNAME%TYPE,
  password_in        IN  DS2.CUSTOMERS.PASSWORD%TYPE,
  batch_size         IN  INTEGER,
  found              OUT INTEGER,
  customerid_out     OUT INTEGER,
  title_out          OUT DS2_TYPES.ARRAY_TYPE,
  actor_out          OUT DS2_TYPES.ARRAY_TYPE,
  related_title_out  OUT DS2_TYPES.ARRAY_TYPE
  )
  AS
  result_cv DS2_TYPES.DS2_CURSOR;
  i INTEGER;

  BEGIN
    
    SELECT CUSTOMERID INTO customerid_out FROM CUSTOMERS WHERE USERNAME = username_in AND PASSWORD = password_in;
    
    delete from derivedtable1;

    insert into derivedtable1 select products.title, products.actor, products.prod_id, products.common_prod_id
        from cust_hist inner join products on cust_hist.prod_id = products.prod_id
       where (cust_hist.customerid = customerid_out);
    OPEN result_cv FOR
      SELECT derivedtable1.TITLE, derivedtable1.ACTOR, PRODUCTS.TITLE AS RelatedTitle
        FROM
          derivedtable1 INNER JOIN
            PRODUCTS ON derivedtable1.COMMON_PROD_ID = PRODUCTS.PROD_ID;
    
    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO title_out(i), actor_out(i), related_title_out(i);
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;

  EXCEPTION
    WHEN NO_DATA_FOUND THEN
    customerid_out := 0;
  
  END LOGIN;
/


CREATE OR REPLACE PROCEDURE "DS2"."BROWSE_BY_CATEGORY" 
  (
  batch_size   IN INTEGER,
  found        OUT INTEGER,
  category_in  IN INTEGER,
  prod_id_out  OUT DS2_TYPES.N_TYPE,
  category_out OUT DS2_TYPES.N_TYPE,
  title_out    OUT DS2_TYPES.ARRAY_TYPE,
  actor_out    OUT DS2_TYPES.ARRAY_TYPE,
  price_out    OUT DS2_TYPES.N_TYPE,
  special_out  OUT DS2_TYPES.N_TYPE,
  common_prod_id_out  OUT DS2_TYPES.N_TYPE
  )
  AS
  result_cv DS2_TYPES.DS2_CURSOR;
  i INTEGER;
  
  BEGIN
  
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM PRODUCTS WHERE CATEGORY = category_in AND SPECIAL = 1;
    END IF;
  
    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO prod_id_out(i), category_out(i), title_out(i), actor_out(i), price_out(i), special_out(i), common_prod_id_out(i);
      IF result_cv%NOTFOUND THEN 
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END BROWSE_BY_CATEGORY;
/  
  
  
CREATE OR REPLACE  PROCEDURE "DS2"."BROWSE_BY_ACTOR"
  (
  batch_size   IN INTEGER,
  found        OUT INTEGER,
  actor_in     IN  CHAR,
  prod_id_out  OUT DS2_TYPES.N_TYPE,
  category_out OUT DS2_TYPES.N_TYPE,
  title_out    OUT DS2_TYPES.ARRAY_TYPE,
  actor_out    OUT DS2_TYPES.ARRAY_TYPE,
  price_out    OUT DS2_TYPES.N_TYPE,
  special_out  OUT DS2_TYPES.N_TYPE,
  common_prod_id_out  OUT DS2_TYPES.N_TYPE
  )
  AS
  result_cv DS2_TYPES.DS2_CURSOR;
  i INTEGER;
  
  BEGIN
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM PRODUCTS WHERE CONTAINS(ACTOR, actor_in) > 0;
    END IF;
  
    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO prod_id_out(i), category_out(i), title_out(i), actor_out(i), price_out(i), special_out(i), common_prod_id_out(i);
      IF result_cv%NOTFOUND THEN 
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END BROWSE_BY_ACTOR;
/
  
  
CREATE OR REPLACE  PROCEDURE "DS2"."BROWSE_BY_TITLE"
  (
  batch_size   IN  INTEGER,
  found        OUT INTEGER,
  title_in     IN  VARCHAR2,
  prod_id_out  OUT DS2_TYPES.N_TYPE,
  category_out OUT DS2_TYPES.N_TYPE,
  title_out    OUT DS2_TYPES.ARRAY_TYPE,
  actor_out    OUT DS2_TYPES.ARRAY_TYPE,
  price_out    OUT DS2_TYPES.N_TYPE,
  special_out  OUT DS2_TYPES.N_TYPE,
  common_prod_id_out  OUT DS2_TYPES.N_TYPE
  )
  AS
  result_cv DS2_TYPES.DS2_CURSOR;
  i INTEGER;
  
  BEGIN
  
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM PRODUCTS WHERE CONTAINS(TITLE, title_in) > 0;
    END IF;
  
    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO prod_id_out(i), category_out(i), title_out(i), actor_out(i), price_out(i), special_out(i), common_prod_id_out(i);
      IF result_cv%NOTFOUND THEN 
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END BROWSE_BY_TITLE;
/
  
  
CREATE OR REPLACE  PROCEDURE "DS2"."PURCHASE"
  (
  customerid_in   IN INTEGER,
  number_items    IN INTEGER,
  netamount_in    IN NUMBER,
  taxamount_in    IN NUMBER,
  totalamount_in  IN NUMBER,
  neworderid_out  OUT INTEGER,
  prod_id_in      IN DS2_TYPES.N_TYPE,
  qty_in          IN DS2_TYPES.N_TYPE
  )
  AS
  date_in        DATE;
  item_id        INTEGER;
  price          NUMBER;
  cur_quan       NUMBER;
  new_quan       NUMBER;
  cur_sales      NUMBER;
  new_sales      NUMBER;
  prod_id_temp   DS2_TYPES.N_TYPE;

  BEGIN

    SELECT ORDERID_SEQ.NEXTVAL INTO neworderid_out FROM DUAL;

    date_in := SYSDATE;
--  date_in := TO_DATE('2005/1/1', 'YYYY/MM/DD');

    COMMIT;

  -- Start Transaction
    SET TRANSACTION NAME 'FillOrder';

  

  -- CREATE NEW ENTRY IN ORDERS TABLE
    INSERT INTO ORDERS
      (
      ORDERID,
      ORDERDATE,
      CUSTOMERID,
      NETAMOUNT,
      TAX,
      TOTALAMOUNT
      )
    VALUES
      (
      neworderid_out,
      date_in,
      customerid_in,
      netamount_in,
      taxamount_in,
      totalamount_in
      )
      ;

    -- ADD LINE ITEMS TO ORDERLINES

    FOR item_id IN 1..number_items LOOP
      INSERT INTO ORDERLINES
        (
        ORDERLINEID,
        ORDERID,
        PROD_ID,
        QUANTITY,
        ORDERDATE
        )
      VALUES
        (
        item_id,
        neworderid_out,
        prod_id_in(item_id),
        qty_in(item_id),
        date_in
        )
        ;
   -- Check and update quantity in stock
      SELECT QUAN_IN_STOCK, SALES into cur_quan, cur_sales FROM INVENTORY WHERE PROD_ID=prod_id_in(item_id);
      new_quan := cur_quan - qty_in(item_id);
      new_sales := cur_sales + qty_in(item_id);
      IF new_quan < 0 THEN
        ROLLBACK;
        neworderid_out := 0;
        RETURN;
      ELSE
        IF new_quan < 3 THEN  -- this is kluge to keep rollback rate constant - assumes 1, 2 or 3 quan ordered
          UPDATE INVENTORY SET SALES= new_sales WHERE PROD_ID=prod_id_in(item_id);
        ELSE
          UPDATE INVENTORY SET QUAN_IN_STOCK = new_quan, SALES= new_sales WHERE PROD_ID=prod_id_in(item_id);
        END IF;
        INSERT INTO CUST_HIST
          (
          CUSTOMERID,
          ORDERID,
          PROD_ID
          )
        VALUES
          (
          customerid_in,
          neworderid_out,
          prod_id_in(item_id)
          );
      END IF;
    END LOOP;

    COMMIT;

  END PURCHASE;
/

CREATE OR REPLACE TRIGGER "DS2"."RESTOCK" 
AFTER UPDATE OF "QUAN_IN_STOCK" ON "DS2"."INVENTORY" 
FOR EACH ROW WHEN (NEW.QUAN_IN_STOCK < 10) 

DECLARE
  X NUMBER;
BEGIN 
  SELECT COUNT(*) INTO X FROM DS2.REORDER WHERE PROD_ID = :NEW.PROD_ID;
  IF x = 0 THEN
    INSERT INTO DS2.REORDER(PROD_ID, DATE_LOW, QUAN_LOW) VALUES(:NEW.PROD_ID, SYSDATE, :NEW.QUAN_IN_STOCK);
  END IF;
END RESTOCK;
/

exit;
