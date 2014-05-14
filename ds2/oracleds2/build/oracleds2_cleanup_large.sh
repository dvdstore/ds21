sqlplus ds2/ds2 @oracleds2_cleanup_large_fk_disabled.sql
sqlldr ds2/ds2 CONTROL=../load/prod/inv.ctl, LOG=inv.log, BAD=inv.bad, DATA=../../data_files/prod/inv.csv 
