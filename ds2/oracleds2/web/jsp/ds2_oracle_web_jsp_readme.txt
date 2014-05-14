ds2_oracle_web_jsp_readme.txt

jsp interface to the Oracle DVD Store database
Requires Oracle j/Connector

Files:

dspurchase.jsp
dsnewcustomer.jsp
dsbrowse.jsp
dslogin.jsp
oracleds2_create_trigger_jsp_only.sql

First, it is necessary to add two triggers to the DS2 Oracle database to
automatically generate CUSTOMERID and ORDERID upon insert of new rows to the
CUSTOMER and ORDER tables, respectively:

sqlplus ds2/ds2 @oracleds2_create_trigger_jsp_only

Tomcat is the platform used to host the jsp pages.  To setup Tomcat the following was done:

Download TomCat 5.5.9 from jakarta.apache.org and untar:
	tar -xvf jakarta-tomcat-5.5.9.tar
		- note this should be done in the directory where you want to install it!

Download j2se5 from java.sun.com for linux and execute self-installing rpm:
        sh jre-1_5_0_04-linux-i586.rpm


Set needed enviornment variables:

	export CATALINA_HOME=/jakarta-tomcat-5.5.9
	export JRE_HOME=/usr/java/jre1.5.0_02

Startup Tomcat
	$CATALINA_HOME/bin/startup.sh

Check to see if it is working

	http://<hostname>:8080/

If not check log - $CATALINA_HOME/logs/catalina.out



Obtain Oracle J Connector (ojdbc14.jar) from http://www.oracle.com/technology/software/tech/java/sqlj_jdbc/index.html

Copy ojdbc14.jar to $CATALINA_HOME/webapps/ROOT/WEB-INF/lib/

Copy the ds*.jsp files into $CATALINA_HOME/webapps/ROOT

Restart Tomcat to get CLASSPATH change to take effect
	$CATALINA_HOME/bin/shutdown.sh
	$CATALINA_HOME/bin/startup.sh

Open browser to test
	http://<hostname>:8080/dslogin.jsp


<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  9/13/05
