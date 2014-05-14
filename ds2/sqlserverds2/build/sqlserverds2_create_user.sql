--Added by GSK Create Login and then add users and their specific roles for database
USE [master]
GO
IF NOT EXISTS(SELECT name FROM sys.server_principals WHERE name = 'ds2user')
BEGIN
	CREATE LOGIN [ds2user] WITH PASSWORD=N'',
	DEFAULT_DATABASE=[master],
	DEFAULT_LANGUAGE=[us_english],
	CHECK_EXPIRATION=OFF,
	CHECK_POLICY=OFF


	EXEC master..sp_addsrvrolemember @loginame = N'ds2user', @rolename = N'sysadmin'

	USE [DS2]
	CREATE USER [ds2DS2user] FOR LOGIN [ds2user]

	USE [DS2]
	EXEC sp_addrolemember N'db_owner', N'ds2DS2user'

	USE [master]
	CREATE USER [ds2masteruser] FOR LOGIN [ds2user]

	USE [master]
	EXEC sp_addrolemember N'db_owner', N'ds2masteruser'

	USE [model]
	CREATE USER [ds2modeluser] FOR LOGIN [ds2user]

	USE [model]
	EXEC sp_addrolemember N'db_owner', N'ds2modeluser'

	USE [msdb]
	CREATE USER [ds2msdbuser] FOR LOGIN [ds2user]

	USE [msdb]
	EXEC sp_addrolemember N'db_owner', N'ds2msdbuser'

	USE [tempdb]
	CREATE USER [ds2tempdbuser] FOR LOGIN [ds2user]

	USE [tempdb]
	EXEC sp_addrolemember N'db_owner', N'ds2tempdbuser'

END
GO