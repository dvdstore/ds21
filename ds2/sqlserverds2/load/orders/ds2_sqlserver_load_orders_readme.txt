ds2_sqlserver_load_orders_readme.txt

Instructions for loading DVD Store Version 2 (DS2) database orders data
(assumes data files are in directory c:\ds2\data_files\orders)

  osql -Usa -P -i sqlserverds2_load_orders.sql
  osql -Usa -P -i sqlserverds2_load_orderlines.sql
  osql -Usa -P -i sqlserverds2_load_cust_hist.sql

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  6/28/05
