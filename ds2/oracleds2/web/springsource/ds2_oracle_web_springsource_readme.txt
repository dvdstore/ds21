Using SpringSource Web Tier for Oracle database testing using web driver
 	
	In DVDStore 2.1, SpringSource MVC Implementation of Web Pages for Oracle database is provided along with old JSP implementation.
	If user wants to use SpringSource web tier instead of JSP, here are the steps to ensure Web Tier is setup correctly on Web Server:

	1) To compile SpringSource code, we need JDK to be installed on Web Server preferably JDK 1.6 or later.

	2) After installing set environment variable JAVA_HOME to path where JDK is installed.
		e.g. JAVA_HOME will have value C:\Program Files\Java\jdk1.6.0_20\

	3) Also Install Ant to compile SpringSource code using ant build commands.

	4) After installing ant, set ANT_HOME environment variable to path where ant is installed.
		e.g. ANT_HOME will have value C:\apache-ant-1.8.1\

	5) Add similar entries for PATH enviroment variable too.
		e.g. For java, the entry in PATH environment variable will be  C:\Program Files\Java\jdk1.6.0_20\bin\
		     For ant, the entry in PATH environment variable will be C:\apache-ant-1.8.1\bin\

	6) Install Apache Tomcat Web Server (Version 5.5 or above)

	7) Set CATALINA_HOME environment variable to path where Tomcat is installed.
		e.g. CATALINA_HOME will have value C:\Program Files\Apache Software Foundation\Tomcat 5.5\

	8) Download OpenSource SpringSource Framework 2.5.5 from following link:
		http://sourceforge.net/projects/springframework-2/2.5.5/
		(Download Framework with dependencies)
	
	9) Unzip the zip file downloaded in above step in a folder on system on which web server is installed.
		e.g. Unzip above zip file into folder c:\springsource-framework2.5.5\

	10) Go to /ds2/oracleds2/web/springsource/ and copy ds2web folder into webapps directory of Tomcat 5.5 installation directory.
		e.g. Copy ds2web folder in DVDStore 2.1 from C:\ds2\oracleds2\web\springsource\ to 
 		     C:\Program Files\Apache Software Foundation\Tomcat 5.5\webapps\ directory.

	11) Copy following files from SpringSource Framework to ds2web folder in webapps directory of Tomcat i.e. /ds2web/WEB-INF/lib/.
		FileName			CopyFrom Path										CopyTo Path
		spring.jar			C:\springsource-framework2.5.5\dist\spring.jar					C:\Program Files\Apache Software Foundation\Tomcat 5.5\webapps\ds2web\WEB-INF\lib\
		spring-webmvc.jar 	C:\springsource-framework2.5.5\dist\modules\spring-webmvc.jar		C:\Program Files\Apache Software Foundation\Tomcat 5.5\webapps\ds2web\WEB-INF\lib\
		standard.jar		C:\springsource-framework2.5.5\lib\jakarta-taglibs\standard.jar		C:\Program Files\Apache Software Foundation\Tomcat 5.5\webapps\ds2web\WEB-INF\lib\
		commons-logging.jar 	C:\springsource-framework2.5.5\lib\jakarta-commons\commons-logging.jar  C:\Program Files\Apache Software Foundation\Tomcat 5.5\webapps\ds2web\WEB-INF\lib\
		servlet-api.jar		C:\springsource-framework2.5.5\lib\j2ee\servlet-api.jar			C:\Program Files\Apache Software Foundation\Tomcat 5.5\webapps\ds2web\WEB-INF\lib\
		jstl.jar			C:\springsource-framework2.5.5\lib\j2ee\jstl.jar				C:\Program Files\Apache Software Foundation\Tomcat 5.5\webapps\ds2web\WEB-INF\lib\
		
		Also Obtain Oracle J Connector (ojdbc14.jar) from http://www.oracle.com/technology/software/tech/java/sqlj_jdbc/index.html

		Copy ojdbc14.jar to lib folder under ds2web folder in Apache Tomcat webapps. i.e. C:\Program Files\Apache Software Foundation\Tomcat 5.5\webapps\ds2web\WEB-INF\lib\

	12) Open command prompt and go to /webapps/ds2web/WEB-INF/ folder under tomcat installation.
	
	13) Type "ant clean" and press enter. (This step will clean up the older *.class files under ds2web folde	
	14) Type "ant" and press enter. (This step will build all code under ds2web folder using build.xml file in /ds2web/WEB-INF/ and will dump *.class files in their respective folders)
	
	15) For Web Driver to work for Oracle Web Tier, it is necessary to create two triggers on Database Server.
	    The script to create triggers is located at /ds2/oracleds2/web/springsource/oracleds2_create_trigger_springsource_only.sql
		Run this script on command prompt as: sqlplus ds2/ds2 @oracleds2_create_trigger_springsource_only.sql
	
	16) One build using ant is complete and all above steps are complete, start apache tomcat web server and check whether DVDStore Web tier is up and running using following URL:
		http://<hostname>:8080/dslogin.html
	
	17) Now to run the web driver program and with SpringSource Web pages being hosted by Tomcat, some the parameter values for driver program should be as follows:
		virt_dir=ds2web
		page_type=html



