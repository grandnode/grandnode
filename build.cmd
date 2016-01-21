:build

msbuild GrandNode.proj /p:DebugSymbols=false /p:DebugType=None /P:SlnName=GrandNode /maxcpucount %*
