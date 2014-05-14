ds2_mysql_web_aspx_readme.txt

aspx interface to the MySQL DVD Store database

Run under IIS or under Apache/Mono

Requires aspx/MySQL5/MySQL Connector for .NET

Files:

dsbrowse.aspx
dsbrowse.aspx.cs
dscommon.cs
dslogin.aspx
dslogin.aspx.cs
dsnewcustomer.aspx
dsnewcustomer.aspx.cs
dspurchase.aspx
dspurchase.aspx.cs
MySql.Data.dll    <-  from mysql-connector-net-1.0.4.zip from MySQL site

To compile under mono use compile.sh:
mcs /t:library /out:bin/ds2.dll -r:System.Web -r:System.Data -r:System.Drawing -r:MySql.Data AssemblyInfo.cs dslogin.aspx.cs dsnewcustomer.aspx.cs dsbrowse.aspx.cs dspurchase.aspx.cs dscommon.cs

Then restart Apache, eg.:
service httpd restart

Mono setup under Red Hat Enterprise Linux 3:
(assumes aspx pages in /home/web/ds2/mysqlds2/web/aspx)

Add following lines to /etc/httpd/conf.d/mod_mono.conf:

LoadModule mono_module /usr/lib/httpd/modules/mod_mono.so

Alias /ds2_aspx "/home/web/ds2/mysqlds2/web/aspx"
MonoApplications "/ds2_aspx:/home/web/ds2/mysqlds2/web/aspx"
<Location /ds2_aspx>
  SetHandler mono
</Location>

Then access page using URL http://(webserver_hostname)/ds2_aspx

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  6/28/05
