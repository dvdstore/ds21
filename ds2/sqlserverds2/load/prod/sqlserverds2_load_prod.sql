use DS2
go
alter database DS2 set recovery bulk_logged
go
bulk insert PRODUCTS from 'c:\ds2\data_files\prod\prod.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS2 set recovery full
go
