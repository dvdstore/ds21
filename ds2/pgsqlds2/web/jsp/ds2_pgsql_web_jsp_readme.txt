ds2_pgsql_web_jsp_readme.txt

jsp interface to the PostgreSQL DVD Store database
Requires PostgreSQL5 and PostgreSQL j/Connector
Files:

dspurchase.jsp
dsnewcustomer.jsp
dsbrowse.jsp
dslogin.jsp

Tomcat is the platform used to test the jsp pages.  To setup Tomcat the following was done:

Download TomCat 5.5.7 from jakarta.apache.org
	Untar
		tar -xvf jakarta-tomcat-5.5.7.tar
		- note this should be done in the directory where you want to install it!

Download j2se5 from java.sun.com for linux
	execute self-installing rpm
	./jre-1_5_0_02-linux-i586-rpm.bin

Set needed enviornment variables:

	export CATALINA_HOME=/tomcat/jakarta-tomcat-5.5.7
	export JRE_HOME=/usr/java/jre1.5.0_02

Startup Tomcat
	$CATALINA_HOME/bin/startup.sh

Check to see if it is working

	http://<hostname>:8080/

If not check log - $CATALINA_HOME/logs/catalina.out



PostgreSQL JDBC Driver

Download latest version from jdbc.postgresql.org
	Version downloaded - 

Unzip and UnTar.  

Copy postgresql.jar  to /tomcat/jakarta-tomcat-5.5.7/webapps/ROOT/WEB-INF/lib/

Edit the $CATALINA_HOME/bin/catalina.sh file
	add the location of the postgresql  jdbc jar file  into the CLASSPATH in the section commented as
	#Add on extra jar files to CLASSPATH

Copy the ds jsp files into $CATALINA_HOME/webapps/ROOT

Restart Tomcat to get CLASSPATH change to take effect
	$CATALINA_HOME/bin/shutdown.sh
	$CATALINA_HOME/bin/startup.sh

Open browser to test
	http://<hostname>:8080/dslogin.jsp


<jshah@vmware.com> and <tmuirhead@vmware.com>  11/1/11
