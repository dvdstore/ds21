@echo off
Rem make_ds2_mt.bat:  make Multithreaded Oracle Driver for DS
Rem usage: make_ds2_mt ds2_mt_fn
Rem based on pcmake.bat from Oracle demos
Rem NOTES:
Rem NOTES:
Rem copied C:\Program Files\Microsoft Visual Studio .NET 2003\VC7 to C:\VC7 to eliminate spaces in pathnames  

@echo on

C:\oracle\ora92\bin\proc threads=yes parse=full iname=%1.pc sqlcheck=semantics userid=ds2/ds2@rhel32 include="C:\oracle\ora92\oci\include" include="C:\oracle\ora92\precomp\public" include="C:\VC7\include" include="C:\\VC7\PlatformSDK\include"

cl -IC:\oracle\ora92\oci\include -IC:\oracle\ora92\precomp\public -I. -IC:\\VC7\include -IC:\VC7\PlatformSDK\Include -D_MT -D_DLL -Zi %1.c /link C:\oracle\ora92\oci\lib\msvc\oramts.lib /libpath:C:\oracle\ora92\precomp\lib /libpath:C:\oracle\ora92\precomp\lib\msvc orasql9.LIB /LIBPATH:C:\VC7\lib msvcrt.lib /nod:libc
