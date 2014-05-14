
-- pgsqlds2_create_db.sql: DVD Store Database Version 2.1 Build Script - Postgres version
-- Copyright (C) 2011 Vmware, Inc. 
-- Last updated 2/13/11


-- Database for PostgreSQL . Not needed for Cloud

DROP DATABASE IF EXISTS DS2;
CREATE DATABASE DS2;
CREATE USER  DS2 WITH SUPERUSER;
ALTER USER DS2 WITH PASSWORD 'ds2';

