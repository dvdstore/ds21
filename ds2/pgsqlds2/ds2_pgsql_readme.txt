ds2_pgsql_readme.txt


DVDStore 2.1 allows to create any custom size database. 

User can use perl scripts in DVDStore 2.1 to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 2.1,
please go through document /ds2/ds2.1_Documentation.txt

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds2/ds2.1_Documentation.txt under prerequisites section)

--------------------------------------------------------------------------------------------------------------------

Instructions for building and loading the PostgreSQL implementation of the DVD Store Version 2 (DS2) database

For users of the vPostgres version of Postgres, please see notes at the bottom of this readme.

DS2 comes in 3 standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

Directories
-----------
./ds2/pgsqlds2
./ds2/pgsqlds2/build
./ds2/pgsqlds2/load
./ds2/pgsqlds2/load/cust
./ds2/pgsqlds2/load/orders
./ds2/pgsqlds2/load/prod
./ds2/pgsqlds2/web
./ds2/pgsqlds2/web/jsp
./ds2/pgsqlds2/web/php

The ./ds2/pgsqlds2 directory contains two driver programs:
ds2pgsqldriver.exe      (ftp in binary to a Windows machine)
ds2pgsqldriver_mono.exe (run under Mono on Linux)
To see the syntax run program with no arguments on a command line.
To compile use ds2pgsqlfns.cs with ./ds2/drivers/ds2xdriver.cs (see
that file's header)

The ./ds2/pgsqlds2/build directory contains PostgreSQL scripts to create the DS2
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

The ./ds2/pgsqlds2/load directories contain PostgreSQL load scripts to load the data
from the datafiles under ./ds2/data_files. You will need to modify the scripts
if the data is elsewhere.
 
The ./ds2/pgsqlds2/web directories contain PHP and JSP applications to drive DS2

The build and load of the Small DS2 database may be accomplished with the
shell script, pgsqlds2_create_all.sh, in ./ds2/pgsqlds2:

On PostgreSQL machine:

1) Install PostgreSQL 9.x or later
2) As postgres user:
	a) initdb (eg - "initdb -D /pgdata") and start the database server (eg - "pg_ctl -D /pgdata -l logfile start").
	b) create ds2 user role (use psql to do "create role ds2 with superuser;" and then "ALTER ROLE ds2 LOGIN;")
3) logon to system as ds2
3) untar ds2.tar.gz from linux.dell.com/dvdstore
4) untar ds2_pgsql.tar.gz to the same place
5) Create softlink from /tmp/data_files to the ./ds2/data_files
   ln -s ./ds2/data_files /tmp/data_files
6) cd ./ds2/pgsqlds2
7) sh pgsqlds2_create_all.sh

By default the data_files that are included with the ds2.tar.gz tarball are for the small DVDStore database.  
To create the medium (1GB) or large (100GB) sizes you can use the data creation scripts directly in the 
ds2/data_files directories.  You can also use the Install_DVDStore.pl script in the ds2/ directory to create 
the load script and data files for any size database. When you run Install_DVDStore.pl it will ask you a series of
questions that allow you to specify the size and database type.  It then will produce the needed sripts and data files.

pgsqlds2_create_all_nosp.sh is the same thing as above with the create stored
procedure line commented out

postgresql.conf.example is the PostgreSQL configuration file used in our testing (append to $PGDATA/postgresql.conf)

In order to enable remote systems to be able to access the PostgreSQL database it is necessary to make two changes.
These two changes will enable completely open access, if you need more a restrictive policy these settings would be different:
1) In postgres.conf - listen_Addresses = '*'
2) In pg_hba.conf add a line:
host	all	all	0.0.0.0/0	trust

Driver Program
--------------

The ds2pgsqldriver.exe (and ds2pgsqldrivermono.exe) is the Postgresql implementation of the DS2 driver. It is 
based on ds2pgsqlfns.cs (in this directory) and ds2xdriver.cs (in ds2/drivers). It is a C# .Net program that uses the 
Microsoft .Net runtime (or mono on Linux).  ds2pgsqldriver.exe is a direct driver that generates load against the 
database that simulates users logging on, browsing, and purchasing items from the DVDStore website. 

You will need the npgsql data connector for .net which can be obtained at http://npgsql.projects.postgresql.org. 
After downloading this package, place a copy of the npgsql.dll, mono.security.dll, and policy-2.0.ngpsql.dll in the ds2\pgsqlds2 directory (so that they are in the same directory as ds2pgsqldriver.exe).

There is an included binary version of ds2pgsqldriver.exe that should work on your system if you are using the same
versions of .Net and postgres libraries, but it can also be easily recomipled from the command line using the C# 
complier (csc.exe)that is included with .Net.  The syntax is included in the header of ds2xdriver as well as here
(You will need to fix the paths to match your environment):

csc.exe /out:ds2pgsqldriver.exe ds2pgsqlfns.cs c:\ds2\drivers\ds2xdriver.cs /d:WIN_32_TIMER /d:GEN_PERF_CTRS "/r:c:\npgsql\ms.net4.0\Npgsql.dll"

or for mono

gmcs ds2pgsqlfns.cs ../drivers/ds2xdriver.cs /out:ds2pgsqldrivermono.exe /r:/usr/lib/mono/gac/System.Data/2.0.0.0__b77a5c561934e089/System.Data.dll /r:/root/Npgsql/Mono2.0/bin/Npgsql.dll

You can run ds2pgsqldriver.exe --help to get a full listing of parameters, a description of each, and their default values.

In addition to this direct driver there is ds2webdriver (located the ds2/drivers) that can be used to drive load though
the PHP or JSP version of the DVDStore webtier that have been implemented to use the Postgresql version of the DVDStore
datbase.  Please see the pgsqlds2/web directory for more info on these webtier versions.

Notes for VMware vFabric Data Director vPostgres 
------------------------------------------------

This postgresql version of the DVD Store will load and run on vPostgres.  The only changes needed for the build script is 
to comment out or remove the create_db script call becuase the db has already ben created and to change the connection string
to match for the vPostgres DB you are using.

The direct driver will not work with the vPostgres version at the time of this initial release (Nov 2011) becuase there
not an available version of the npgsql data connector for .Net that works with vPostgres.  It will be necesary to use 
either the included PHP or JSP pages and the ds2 webdriver to generate load against a vPostgres database.

Most of the directories contain readme's with further instructions

<jshah@vmware.com> and <tmuirhead@vmware.com>  11/1/11
