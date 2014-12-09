SET NuGet=".\packages\NuGet.CommandLine.2.8.3\tools\NuGet.exe"
%NuGet% pack "SnkLib.App.CookieGetter\SnkLib.App.CookieGetter.csproj" -OutputDirectory ".\Publish" -Build -Properties Configuration=Release -IncludeReferencedProjects
%NuGet% pack "SnkLib.App.CookieGetter.Forms\SnkLib.App.CookieGetter.Forms.csproj" -OutputDirectory ".\Publish" -Build -Properties Configuration=Release -IncludeReferencedProjects
%NuGet% pack "CookieGetterSharp\CookieGetterSharp.csproj" -OutputDirectory ".\Publish" -Build -Properties Configuration=Release -IncludeReferencedProjects