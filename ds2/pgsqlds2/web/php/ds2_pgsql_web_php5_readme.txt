ds2_pgsql_web_php5_readme.txt

php5 interface to the PostgreSQL DVD Store database
Requires php5/PostgreSQL
Files:

dscommon.inc
dspurchase.php
dsnewcustomer.php      - new customer page with stored procedure call
dsnewcustomer.php.sp   - new customer page with stored procedure call
dsbrowse.php
dslogin.php

The driver programs expect these files in a virtual directory "ds2".
In your web server you either need to create a virtual directory that points
to this directory or copy these files to the appropriate directory (eg /var/www/html/ds2)


<dave_jaffe@dell.com>, <tmuirhead@vmware.com>, <jshah@vmware.com>  11/1/11
