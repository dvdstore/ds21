# ds21
DVD Store version 2.1

DVD Store 2.1 (DS2) is an open source test / benchmark tool that simultaes an online store that sells DVDs. Customers can login, browse DVDs, and purchase DVDs. Everything needed to create, load, and stress this online store is included in the DVD Store project.

DVD Store 2.1 is based on the previous DVD Store 2. The big new feature in DVD Store 2.1 is the ability to create any size test database.

This is a database test workload. The database is created using the included tools and then stressed with the included driver program that simultes user activity.

Full details are included in the various readme files in the DS2 subdirectories.

DVD Store 2 supports Oracle, MySQL, SQL Server, and PostGres databases.

There has also been work done by Raghu Ramakrishnan on a Java Jmeter based tool to drive load against the DVD Store 2.1 database.
It includes the needed jmeter .jmx scripts to be able to use jmeter with DVD Store 2.1.  Jmeter provides a way to have a GUI interface
for the driver as well as several other options.  I have tried it out and was able to get it running without too much hassle.  
If interested please see their github page - https://github.com/dvdstorejm/ds21
