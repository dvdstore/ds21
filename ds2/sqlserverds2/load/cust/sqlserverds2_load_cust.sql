use DS2
go
alter database DS2 set recovery bulk_logged
go
bulk insert CUSTOMERS from 'c:\ds2\data_files\cust\us_cust.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUSTOMERS from 'c:\ds2\data_files\cust\row_cust.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS2 set recovery full
go
