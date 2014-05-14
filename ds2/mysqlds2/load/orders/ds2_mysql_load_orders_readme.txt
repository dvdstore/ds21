ds2_mysql_load_orders_readme.txt

Instructions for creating and loading DVD Store Version 2 (DS2) database orders data
(assumes data files are in directory ../../../data_files/orders)

  mysql --password=pw < mysqlds2_load_orders.sql
  mysql --password=pw < mysqlds2_load_orderlines.sql
  mysql --password=pw < mysqlds2_load_cust_hist.sql

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  5/13/05
