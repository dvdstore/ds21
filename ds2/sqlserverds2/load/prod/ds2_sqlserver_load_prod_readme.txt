ds2_sqlserver_load_prod_readme.txt

Instructions for loading DVD Store Version 2 (DS2) database product data
(assumes data files are in directory c:\ds2\data_files\prod)

  osql -Usa -P -i sqlserverds2_load_prod.sql
  osql -Usa -P -i sqlserverds2_load_inv.sql

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  6/28/05
