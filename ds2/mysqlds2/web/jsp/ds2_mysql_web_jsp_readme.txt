ds2_mysql_web_jsp_readme.txt

jsp interface to the MySQL DVD Store database
Requires MySQL5 and MySQL j/Connector
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



MySQL J Connector

Download latest version from www.mysql.com
	Version downloaded - mysql-connector-java-3.1.7.tar.gz

Unzip and UnTar.  

Copy mysql-connector-java-3.1.7-bin.jar to /tomcat/jakarta-tomcat-5.5.7/webapps/ROOT/WEB-INF/lib/

Edit the $CATALINA_HOME/bin/catalina.sh file
	add the location of the mysql j connector into the CLASSPATH in the section commented as
	#Add on extra jar files to CLASSPATH

Make a ds2 directory under $CATALINE_HOME/webapps/ROOT and then copy the ds jsp files into $CATALINA_HOME/webapps/ROOT/ds2

Restart Tomcat to get CLASSPATH change to take effect
	$CATALINA_HOME/bin/shutdown.sh
	$CATALINA_HOME/bin/startup.sh

Open browser to test
	http://<hostname>:8080/dslogin.jsp


<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  6/30/05
