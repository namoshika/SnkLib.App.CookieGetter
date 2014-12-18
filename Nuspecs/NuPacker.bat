SET NuGet="..\packages\NuGet.CommandLine.2.8.3\tools\NuGet.exe"
%NuGet% pack ".\SnkLib.App.CookieGetter.nuspec" -OutputDirectory "..\Publish" -Build -Properties Configuration=Release
%NuGet% pack ".\SnkLib.App.CookieGetter.Forms.nuspec" -OutputDirectory "..\Publish" -Build -Properties Configuration=Release
%NuGet% pack ".\CookieGetterSharp.nuspec" -OutputDirectory "..\Publish" -Build -Properties Configuration=Release