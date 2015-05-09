SET NuGet="..\packages\NuGet.CommandLine.2.8.5\tools\NuGet.exe"
%NuGet% pack ".\SnkLib.App.CookieGetter.nuspec" -OutputDirectory "..\Publish" -Build -Properties Configuration=Release
%NuGet% pack ".\SnkLib.App.CookieGetter.Forms.nuspec" -OutputDirectory "..\Publish" -Build -Properties Configuration=Release
