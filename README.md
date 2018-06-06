# CyberButler
CyberButler Discord Bot

## Getting SQLite to work on Ubuntu/Linux

There’s no System.Data.SQLite package for Linux, so you’ll have to build it yourself on your target Linux machine. You can build using this procedure, which is tested in Raspbian Jessie on a Raspberry Pi 3 and Ubuntu 16.04.1 on a PC:

1. Download System.Data.SQLite full source code from this download page. There’s a ton of files there, and the one you should look for is named something like sqlite-netFx-full-source-<version no>.zip.
1. Unzip it and transfer it to a directory on your Linux machine. In the rest of this description, I’ll call this directory “<source root>”.
1. Issue these commands in a Linux terminal:

        sudo apt-get update
        sudo apt-get install build-essential
        cd <source root>/Setup
        chmod +x compile-interop-assembly-release.sh
        ./compile-interop-assembly-release.sh  

1. Now, you will have a freshly built library file called libSQLite.Interop.so in the <source root>/bin/2013/Release/bin directory. This file might have execution permission which isn’t relevant for a library, so remove it by
`chmod -x <source root>/bin/2013/Release/bin/libSQLite.Interop.so`
1. Copy libSQLite.Interop.so the directory where your Mono/.NET application’s binaries reside (not the x64 or x86 subdirectories containing SQLite.Interop.dll), and you’re set to go.

[Credit](http://blog.wezeku.com/2016/10/09/using-system-data-sqlite-under-linux-and-mono/)