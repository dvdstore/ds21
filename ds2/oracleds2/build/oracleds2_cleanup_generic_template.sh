#!/bin/sh
sqlplus ds2/ds2 @{SQL_FNAME}
sqlldr ds2/ds2 CONTROL=../load/prod/inv.ctl, LOG=inv.log, BAD=inv.bad, DATA=../../data_files/prod/inv.csv 
