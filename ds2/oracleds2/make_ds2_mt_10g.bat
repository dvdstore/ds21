@echo off
Rem make_ds2_mt_10g.bat:  make Multithreaded Oracle Driver for 10g
Rem usage: make_ds2_mt_10g ds_mt_fn
Rem based on pcmake.bat from Oracle demos

Rem copied C:\Program Files\Microsoft Visual Studio .NET 2003\Vc7 to C:\Vc7 to eliminate spaces in pathnames  

@echo on

C:\oracle\product\10.1.0\db_1\bin\proc threads=yes parse=full iname=%1.pc sqlcheck=semantics userid=ds2/ds2 include="C:\oracle\product\10.1.0\db_1\oci\include" include="C:\oracle\product\10.1.0\db_1\precomp\public" include="C:\Vc7\include" include="C:\Vc7\PlatformSDK\include"

cl -IC:\oracle\product\10.1.0\db_1\oci\include -IC:\oracle\product\10.1.0\db_1\precomp\public -I. -IC:\Vc7\include -IC:\Vc7\PlatformSDK\Include -D_MT -D_DLL -Zi %1.c /link C:\oracle\product\10.1.0\db_1\oci\lib\msvc\oci.lib /libpath:C:\oracle\product\10.1.0\db_1\precomp\lib /libpath:C:\oracle\product\10.1.0\db_1\precomp\lib\msvc orasql10.LIB /LIBPATH:C:\Vc7\lib msvcrt.lib /nod:libc
