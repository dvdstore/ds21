ds2_mysql_load_prod_readme.txt

Instructions for creating and loading DVD Store Version 2 (DS2) database product data
(assumes data files are in directory ../../../data_files/prod)

  mysql --password=pw < mysqlds2_load_prod.sql
  mysql --password=pw < mysqlds2_load_inv.sql

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  5/13/05
