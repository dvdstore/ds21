
-- sqlserverds2_create_all_large4.sql: 
-- DVD Store Database Version 2.1 Build, Load and Create Index Script - SQL Server version - Large DB, 4 LUNs
-- Copyright (C) 2007 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
-- Last updated 11/30/07


-- sqlserverds2_create_db.sql

IF EXISTS (SELECT * FROM SYSDATABASES WHERE NAME='DS2')
DROP DATABASE DS2
GO

CREATE DATABASE DS2 ON 
  PRIMARY
    (
    NAME = 'primary', 
    FILENAME = 'G:\ds.mdf'
    ),
  FILEGROUP DS_MISC_FG
    (
    NAME = 'ds_misc', 
    FILENAME = 'H:\ds_misc.ndf',
    SIZE = 1GB
    ),
  FILEGROUP DS_CUST_FG
    (
    NAME = 'cust1', 
    FILENAME = 'G:\cust1.ndf',
    SIZE = 30GB
    ),
    (
    NAME = 'cust2', 
    FILENAME = 'H:\cust2.ndf',
    SIZE = 30GB
    ),
    (
    NAME = 'cust3', 
    FILENAME = 'I:\cust3.ndf',
    SIZE = 30GB
    ),
    (
    NAME = 'cust4', 
    FILENAME = 'J:\cust4.ndf',
    SIZE = 30GB
    ),
  FILEGROUP DS_ORDERS_FG
    (
    NAME = 'orders1', 
    FILENAME = 'G:\orders1.ndf',
    SIZE = 15GB
    ),
    (
    NAME = 'orders2', 
    FILENAME = 'H:\orders2.ndf',
    SIZE = 15GB
    ),
    (
    NAME = 'orders3', 
    FILENAME = 'I:\orders3.ndf',
    SIZE = 15GB
    ),
    (
    NAME = 'orders4', 
    FILENAME = 'J:\orders4.ndf',
    SIZE = 15GB
    ),
  FILEGROUP DS_IND_FG
    (
    NAME = 'ind1', 
    FILENAME = 'G:\ind1.ndf',
    SIZE = 8GB
    ),
    (
    NAME = 'ind2', 
    FILENAME = 'H:\ind2.ndf',
    SIZE = 8GB
    ),
    (
    NAME = 'ind3', 
    FILENAME = 'I:\ind3.ndf',
    SIZE = 8GB
    ),
    (
    NAME = 'ind4', 
    FILENAME = 'J:\ind4.ndf',
    SIZE = 8GB
    )
  LOG ON
    (
    NAME = 'ds_log', 
    FILENAME = 'L:\ds_log.ldf',
    SIZE = 100GB
    )
GO

USE DS2
GO

-- Tables

CREATE TABLE CUSTOMERS
  (
  CUSTOMERID INT IDENTITY NOT NULL, 
  FIRSTNAME VARCHAR(50) NOT NULL, 
  LASTNAME VARCHAR(50) NOT NULL, 
  ADDRESS1 VARCHAR(50) NOT NULL, 
  ADDRESS2 VARCHAR(50), 
  CITY VARCHAR(50) NOT NULL, 
  STATE VARCHAR(50), 
  ZIP INT, 
  COUNTRY VARCHAR(50) NOT NULL, 
  REGION TINYINT NOT NULL,
  EMAIL VARCHAR(50),
  PHONE VARCHAR(50),
  CREDITCARDTYPE TINYINT NOT NULL,
  CREDITCARD VARCHAR(50) NOT NULL, 
  CREDITCARDEXPIRATION VARCHAR(50) NOT NULL, 
  USERNAME VARCHAR(50) NOT NULL, 
  PASSWORD VARCHAR(50) NOT NULL, 
  AGE TINYINT, 
  INCOME INT,
  GENDER VARCHAR(1)
  )
  ON DS_CUST_FG
GO  
  
CREATE TABLE CUST_HIST
  (
  CUSTOMERID INT NOT NULL, 
  ORDERID INT NOT NULL, 
  PROD_ID INT NOT NULL 
  )
  ON DS_CUST_FG
GO
  

CREATE TABLE ORDERS
  (
  ORDERID INT IDENTITY NOT NULL, 
  ORDERDATE DATETIME NOT NULL, 
  CUSTOMERID INT NOT NULL, 
  NETAMOUNT MONEY NOT NULL, 
  TAX MONEY NOT NULL, 
  TOTALAMOUNT MONEY NOT NULL
  ) 
  ON DS_ORDERS_FG
GO

CREATE TABLE ORDERLINES
  (
  ORDERLINEID SMALLINT NOT NULL, 
  ORDERID INT NOT NULL, 
  PROD_ID INT NOT NULL, 
  QUANTITY SMALLINT NOT NULL, 
  ORDERDATE DATETIME NOT NULL
  ) 
  ON DS_ORDERS_FG
GO

CREATE TABLE PRODUCTS
  (
  PROD_ID INT IDENTITY NOT NULL, 
  CATEGORY TINYINT NOT NULL, 
  TITLE VARCHAR(50) NOT NULL, 
  ACTOR VARCHAR(50) NOT NULL, 
  PRICE MONEY NOT NULL, 
  SPECIAL TINYINT,
  COMMON_PROD_ID INT NOT NULL
  )
  ON DS_MISC_FG
GO 

CREATE TABLE INVENTORY
  (
  PROD_ID INT NOT NULL,
  QUAN_IN_STOCK INT NOT NULL,
  SALES INT NOT NULL
  )
  ON DS_MISC_FG
GO

CREATE TABLE CATEGORIES
  (
  CATEGORY TINYINT IDENTITY NOT NULL, 
  CATEGORYNAME VARCHAR(50) NOT NULL, 
  )
  ON DS_MISC_FG
GO 

  SET IDENTITY_INSERT CATEGORIES ON 
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (1,'Action')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (2,'Animation')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (3,'Children')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (4,'Classics')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (5,'Comedy')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (6,'Documentary')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (7,'Drama')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (8,'Family')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (9,'Foreign')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (10,'Games')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (11,'Horror')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (12,'Music')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (13,'New')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (14,'Sci-Fi')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (15,'Sports')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (16,'Travel')
  GO

CREATE TABLE REORDER
  (
  PROD_ID INT NOT NULL,
  DATE_LOW DATETIME NOT NULL,
  QUAN_LOW INT NOT NULL,
  DATE_REORDERED DATETIME,
  QUAN_REORDERED INT,
  DATE_EXPECTED DATETIME
  )
  ON DS_MISC_FG
GO

-- This keeps the number of items with low QUAN_IN_STOCK constant so that the rollback rate is constant 
CREATE TRIGGER RESTOCK ON INVENTORY AFTER UPDATE
AS
  DECLARE @changedPROD_ID INT, @oldQUAN_IN_STOCK INT, @newQUAN_IN_STOCK INT;
  IF UPDATE(QUAN_IN_STOCK)
    BEGIN
      SELECT @changedPROD_ID = i.PROD_ID, @oldQUAN_IN_STOCK = d.QUAN_IN_STOCK, @newQUAN_IN_STOCK = i.QUAN_IN_STOCK
        FROM inserted i INNER JOIN deleted d ON i.PROD_ID = d.PROD_ID
      IF @newQUAN_IN_STOCK < 3    -- assumes quantity ordered is 1, 2, or 3 - change if different
        BEGIN
          INSERT INTO REORDER
            (
            PROD_ID,
            DATE_LOW,
            QUAN_LOW
            )
          VALUES
            (
            @changedPROD_ID,
            GETDATE(),
            @newQUAN_IN_STOCK
            )
          UPDATE INVENTORY SET QUAN_IN_STOCK  = @oldQUAN_IN_STOCK WHERE PROD_ID = @changedPROD_ID
        END
    END
  RETURN
GO

DECLARE @db_id int, @tbl_id int
USE DS2
SET @db_id = DB_ID('DS2')
SET @tbl_id = OBJECT_ID('DS2..CATEGORIES')
DBCC PINTABLE (@db_id, @tbl_id)

SET @db_id = DB_ID('DS2')
SET @tbl_id = OBJECT_ID('DS2..PRODUCTS')
DBCC PINTABLE (@db_id, @tbl_id)
USE DS2
GO

-- sqlserverds2_load_cust.sql

use DS2
go
alter database DS2 set recovery bulk_logged
go
bulk insert CUSTOMERS from 'c:\ds2\data_files\cust\us_cust.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUSTOMERS from 'c:\ds2\data_files\cust\row_cust.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds2_load_orders.sql

set dateformat ymd
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\jan_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\feb_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\mar_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\apr_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\may_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\jun_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\jul_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\aug_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\sep_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\oct_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\nov_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds2\data_files\orders\dec_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds2_load_orderlines.sql

bulk insert ORDERLINES from 'c:\ds2\data_files\orders\jan_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\feb_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\mar_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\apr_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\may_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\jun_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\jul_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\aug_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\sep_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\oct_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\nov_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds2\data_files\orders\dec_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds2_load_cust_hist.sql

bulk insert CUST_HIST from 'c:\ds2\data_files\orders\jan_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\feb_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\mar_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\apr_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\may_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\jun_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\jul_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\aug_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\sep_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\oct_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\nov_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds2\data_files\orders\dec_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds2_load_prod.sql

bulk insert PRODUCTS from 'c:\ds2\data_files\prod\prod.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds2_load_inv.sql

bulk insert INVENTORY from 'c:\ds2\data_files\prod\inv.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS2 set recovery full
go

-- sqlserverds2_create_ind.sql

USE DS2
GO

ALTER TABLE CATEGORIES ADD CONSTRAINT PK_CATEGORIES PRIMARY KEY CLUSTERED 
  (
  CATEGORY
  )  
  ON DS_MISC_FG 
GO

ALTER TABLE CUSTOMERS ADD CONSTRAINT PK_CUSTOMERS PRIMARY KEY CLUSTERED 
  (
  CUSTOMERID
  )  
  ON DS_CUST_FG 
GO

CREATE UNIQUE INDEX IX_CUST_UN_PW ON CUSTOMERS 
  (
  USERNAME, 
  PASSWORD
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_CUST_HIST_CUSTOMERID ON CUST_HIST
  (
  CUSTOMERID
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_CUST_HIST_CUSTOMERID_PRODID ON CUST_HIST 
  (
  CUSTOMERID ASC,
  PROD_ID ASC
  )
  ON DS_IND_FG
GO

ALTER TABLE CUST_HIST
  ADD CONSTRAINT FK_CUST_HIST_CUSTOMERID FOREIGN KEY (CUSTOMERID)
  REFERENCES CUSTOMERS (CUSTOMERID)
  ON DELETE CASCADE
GO

ALTER TABLE ORDERS ADD CONSTRAINT PK_ORDERS PRIMARY KEY CLUSTERED 
  (
  ORDERID
  )  
  ON DS_ORDERS_FG 
GO

CREATE INDEX IX_ORDER_CUSTID ON ORDERS
  (
  CUSTOMERID
  )
  ON DS_IND_FG
GO

ALTER TABLE ORDERLINES ADD CONSTRAINT PK_ORDERLINES PRIMARY KEY CLUSTERED 
  (
  ORDERID,
  ORDERLINEID
  )  
  ON DS_ORDERS_FG 
GO

ALTER TABLE ORDERLINES ADD CONSTRAINT FK_ORDERID FOREIGN KEY (ORDERID)
  REFERENCES ORDERS (ORDERID)
  ON DELETE CASCADE
GO

ALTER TABLE INVENTORY ADD CONSTRAINT PK_INVENTORY PRIMARY KEY CLUSTERED 
  (
  PROD_ID
  )  
  ON DS_MISC_FG 
GO

ALTER TABLE PRODUCTS ADD CONSTRAINT PK_PRODUCTS PRIMARY KEY CLUSTERED 
  (
  PROD_ID
  )  
  ON DS_MISC_FG 
GO

CREATE INDEX IX_PROD_PRODID ON PRODUCTS 
  (
  PROD_ID ASC
  )
  INCLUDE (TITLE)
  ON DS_IND_FG
GO

CREATE INDEX IX_PROD_PRODID_COMMON_PRODID ON PRODUCTS 
  (
  PROD_ID ASC,
  COMMON_PROD_ID ASC
  )
  INCLUDE (TITLE, ACTOR)
  ON DS_IND_FG
GO

CREATE INDEX IX_PROD_SPECIAL_CATEGORY_PRODID ON PRODUCTS 
  (
  SPECIAL ASC,
  CATEGORY ASC,
  PROD_ID ASC
  )
  INCLUDE (TITLE, ACTOR, PRICE, COMMON_PROD_ID)
  ON DS_IND_FG
GO


EXEC sp_fulltext_database 'enable'
EXEC sp_fulltext_catalog  'FULLTEXTCAT_DSPROD', 'create', 'G:\FullText'
EXEC sp_fulltext_table    'PRODUCTS',           'create', 'FULLTEXTCAT_DSPROD', 'PK_PRODUCTS'
EXEC sp_fulltext_column   'PRODUCTS',           'ACTOR', 'add'
EXEC sp_fulltext_column   'PRODUCTS',           'TITLE', 'add'
EXEC sp_fulltext_table    'PRODUCTS',           'activate'
EXEC sp_fulltext_catalog  'FULLTEXTCAT_DSPROD', 'start_full'
GO

CREATE INDEX IX_PROD_CATEGORY ON PRODUCTS 
  (
  CATEGORY
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_PROD_SPECIAL ON PRODUCTS
  (
  SPECIAL
  )
  ON DS_IND_FG
GO

CREATE STATISTICS stat_cust_cctype_username ON CUSTOMERS(CREDITCARDTYPE, USERNAME)
GO
CREATE STATISTICS stat_cust_cctype_customerid ON CUSTOMERS(CREDITCARDTYPE, CUSTOMERID)
GO
CREATE STATISTICS stat_prod_prodid_special ON PRODUCTS(PROD_ID, SPECIAL)
GO
CREATE STATISTICS stat_prod_category_prodid ON PRODUCTS(CATEGORY, PROD_ID)
GO

-- sqlserverds2_create_sp.sql

-- NEW_CUSTOMER

USE DS2
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'NEW_CUSTOMER' AND type = 'P')
  DROP PROCEDURE NEW_CUSTOMER
GO

USE DS2
GO

CREATE PROCEDURE NEW_CUSTOMER
  (
  @firstname_in             VARCHAR(50),
  @lastname_in              VARCHAR(50),
  @address1_in              VARCHAR(50),
  @address2_in              VARCHAR(50),
  @city_in                  VARCHAR(50),
  @state_in                 VARCHAR(50),
  @zip_in                   INT,
  @country_in               VARCHAR(50),
  @region_in                TINYINT,
  @email_in                 VARCHAR(50),
  @phone_in                 VARCHAR(50),
  @creditcardtype_in        TINYINT,
  @creditcard_in            VARCHAR(50),
  @creditcardexpiration_in  VARCHAR(50),
  @username_in              VARCHAR(50),
  @password_in              VARCHAR(50),
  @age_in                   TINYINT,
  @income_in                INT,
  @gender_in                VARCHAR(1)
  )

  AS 

  IF (SELECT COUNT(*) FROM CUSTOMERS WHERE USERNAME=@username_in) = 0
  BEGIN
    INSERT INTO CUSTOMERS 
      (
      FIRSTNAME,
      LASTNAME,
      ADDRESS1,
      ADDRESS2,
      CITY,
      STATE,
      ZIP,
      COUNTRY,
      REGION,
      EMAIL,
      PHONE,
      CREDITCARDTYPE,
      CREDITCARD,
      CREDITCARDEXPIRATION,
      USERNAME,
      PASSWORD,
      AGE,
      INCOME,
      GENDER
      ) 
    VALUES 
      ( 
      @firstname_in,
      @lastname_in,
      @address1_in,
      @address2_in,
      @city_in,
      @state_in,
      @zip_in,
      @country_in,
      @region_in,
      @email_in,
      @phone_in,
      @creditcardtype_in,
      @creditcard_in,
      @creditcardexpiration_in,
      @username_in,
      @password_in,
      @age_in,
      @income_in,
      @gender_in
      )
    SELECT @@IDENTITY
  END
  ELSE 
    SELECT 0
GO


-- LOGIN

USE DS2
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'LOGIN' AND type = 'P')
  DROP PROCEDURE LOGIN
GO

USE DS2
GO

CREATE PROCEDURE LOGIN
  (
  @username_in              VARCHAR(50),
  @password_in              VARCHAR(50)
  )

  AS
DECLARE @customerid_out INT
  
  SELECT @customerid_out=CUSTOMERID FROM CUSTOMERS WHERE USERNAME=@username_in AND PASSWORD=@password_in

  IF (@@ROWCOUNT > 0)
    BEGIN
      SELECT @customerid_out
      SELECT derivedtable1.TITLE, derivedtable1.ACTOR, PRODUCTS_1.TITLE AS RelatedPurchase
        FROM (SELECT PRODUCTS.TITLE, PRODUCTS.ACTOR, PRODUCTS.PROD_ID, PRODUCTS.COMMON_PROD_ID
          FROM CUST_HIST INNER JOIN
             PRODUCTS ON CUST_HIST.PROD_ID = PRODUCTS.PROD_ID
          WHERE (CUST_HIST.CUSTOMERID = @customerid_out)) AS derivedtable1 INNER JOIN
             PRODUCTS AS PRODUCTS_1 ON derivedtable1.COMMON_PROD_ID = PRODUCTS_1.PROD_ID
    END
  ELSE 
    SELECT 0 
GO

USE DS2
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'BROWSE_BY_CATEGORY' AND type = 'P')
  DROP PROCEDURE BROWSE_BY_CATEGORY
GO

USE DS2
GO

CREATE PROCEDURE BROWSE_BY_CATEGORY
  (
  @batch_size_in            INT,
  @category_in              INT
  )

  AS 
  SET ROWCOUNT @batch_size_in
  SELECT * FROM PRODUCTS WHERE CATEGORY=@category_in and SPECIAL=1
  SET ROWCOUNT 0
GO

USE DS2
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'BROWSE_BY_ACTOR' AND type = 'P')
  DROP PROCEDURE BROWSE_BY_ACTOR
GO

USE DS2
GO

CREATE PROCEDURE BROWSE_BY_ACTOR
  (
  @batch_size_in            INT,
  @actor_in                 VARCHAR(50)
  )

  AS 

  SET ROWCOUNT @batch_size_in
  SELECT * FROM PRODUCTS WITH(FORCESEEK) WHERE CONTAINS(ACTOR, @actor_in)
  SET ROWCOUNT 0
GO

USE DS2
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'BROWSE_BY_TITLE' AND type = 'P')
  DROP PROCEDURE BROWSE_BY_TITLE
GO

USE DS2
GO

CREATE PROCEDURE BROWSE_BY_TITLE
  (
  @batch_size_in            INT,
  @title_in                 VARCHAR(50)
  )

  AS 

  SET ROWCOUNT @batch_size_in
  SELECT * FROM PRODUCTS WITH(FORCESEEK) WHERE CONTAINS(TITLE, @title_in)
  SET ROWCOUNT 0
GO

USE DS2
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'PURCHASE' AND type = 'P')
  DROP PROCEDURE PURCHASE
GO

USE DS2
GO

CREATE PROCEDURE PURCHASE
  (
  @customerid_in            INT,
  @number_items             INT,
  @netamount_in             MONEY,
  @taxamount_in             MONEY,
  @totalamount_in           MONEY,
  @prod_id_in0              INT = 0,     @qty_in0     INT = 0,
  @prod_id_in1              INT = 0,     @qty_in1     INT = 0,
  @prod_id_in2              INT = 0,     @qty_in2     INT = 0,
  @prod_id_in3              INT = 0,     @qty_in3     INT = 0,
  @prod_id_in4              INT = 0,     @qty_in4     INT = 0,
  @prod_id_in5              INT = 0,     @qty_in5     INT = 0,
  @prod_id_in6              INT = 0,     @qty_in6     INT = 0,
  @prod_id_in7              INT = 0,     @qty_in7     INT = 0,
  @prod_id_in8              INT = 0,     @qty_in8     INT = 0,
  @prod_id_in9              INT = 0,     @qty_in9     INT = 0
  )

  AS 

  DECLARE
  @date_in                  DATETIME,
  @neworderid               INT,
  @item_id                  INT,
  @prod_id                  INT,
  @qty                      INT,
  @cur_quan		    INT,
  @new_quan		    INT,
  @cur_sales                INT,
  @new_sales                INT
  

  SET DATEFORMAT ymd

  SET @date_in = GETDATE()
--SET @date_in = '2005/10/31'

  BEGIN TRANSACTION
  -- CREATE NEW ENTRY IN ORDERS TABLE
  INSERT INTO ORDERS
    (
    ORDERDATE,
    CUSTOMERID,
    NETAMOUNT,
    TAX,
    TOTALAMOUNT
    )
  VALUES
    (
    @date_in,
    @customerid_in,
    @netamount_in,
    @taxamount_in,
    @totalamount_in
    )

  SET @neworderid = @@IDENTITY


  -- ADD LINE ITEMS TO ORDERLINES

  SET @item_id = 0

  WHILE (@item_id < @number_items)
  BEGIN
    SELECT @prod_id = CASE @item_id WHEN 0 THEN @prod_id_in0
	                                WHEN 1 THEN @prod_id_in1
	                                WHEN 2 THEN @prod_id_in2
	                                WHEN 3 THEN @prod_id_in3
	                                WHEN 4 THEN @prod_id_in4
	                                WHEN 5 THEN @prod_id_in5
	                                WHEN 6 THEN @prod_id_in6
	                                WHEN 7 THEN @prod_id_in7
	                                WHEN 8 THEN @prod_id_in8
	                                WHEN 9 THEN @prod_id_in9
    END

    SELECT @qty = CASE @item_id WHEN 0 THEN @qty_in0
	                            WHEN 1 THEN @qty_in1
	                            WHEN 2 THEN @qty_in2
	                            WHEN 3 THEN @qty_in3
	                            WHEN 4 THEN @qty_in4
	                            WHEN 5 THEN @qty_in5
	                            WHEN 6 THEN @qty_in6
	                            WHEN 7 THEN @qty_in7
	                            WHEN 8 THEN @qty_in8
	                            WHEN 9 THEN @qty_in9
    END

    SELECT @cur_quan=QUAN_IN_STOCK, @cur_sales=SALES FROM INVENTORY WHERE PROD_ID=@prod_id

    SET @new_quan = @cur_quan - @qty
    SET @new_sales = @cur_Sales + @qty

    IF (@new_quan < 0)
      BEGIN
        ROLLBACK TRANSACTION
        SELECT 0
        RETURN
      END
    ELSE
      BEGIN
        UPDATE INVENTORY SET QUAN_IN_STOCK=@new_quan, SALES=@new_sales WHERE PROD_ID=@prod_id
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
          @item_id + 1,
          @neworderid,
          @prod_id,
          @qty,
          @date_in
          )
        
        INSERT INTO CUST_HIST
          (
          CUSTOMERID,
          ORDERID,
          PROD_ID
          )
        VALUES
          (
          @customerid_in,
          @neworderid,
          @prod_id
          )
      
        SET @item_id = @item_id + 1
      END    
  END

  COMMIT

  SELECT @neworderid
GO

--Added by GSK Create Login and then add users and their specific roles for database
USE [master]
GO
IF NOT EXISTS(SELECT name FROM sys.server_principals WHERE name = 'ds2user')
BEGIN
	CREATE LOGIN [ds2user] WITH PASSWORD=N'',
	DEFAULT_DATABASE=[master],
	DEFAULT_LANGUAGE=[us_english],
	CHECK_EXPIRATION=OFF,
	CHECK_POLICY=OFF


	EXEC master..sp_addsrvrolemember @loginame = N'ds2user', @rolename = N'sysadmin'

	USE [DS2]
	CREATE USER [ds2DS2user] FOR LOGIN [ds2user]

	USE [DS2]
	EXEC sp_addrolemember N'db_owner', N'ds2DS2user'

	USE [master]
	CREATE USER [ds2masteruser] FOR LOGIN [ds2user]

	USE [master]
	EXEC sp_addrolemember N'db_owner', N'ds2masteruser'

	USE [model]
	CREATE USER [ds2modeluser] FOR LOGIN [ds2user]

	USE [model]
	EXEC sp_addrolemember N'db_owner', N'ds2modeluser'

	USE [msdb]
	CREATE USER [ds2msdbuser] FOR LOGIN [ds2user]

	USE [msdb]
	EXEC sp_addrolemember N'db_owner', N'ds2msdbuser'

	USE [tempdb]
	CREATE USER [ds2tempdbuser] FOR LOGIN [ds2user]

	USE [tempdb]
	EXEC sp_addrolemember N'db_owner', N'ds2tempdbuser'

END
GO