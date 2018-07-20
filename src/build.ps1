#!/usr/bin/env pwsh

$BasePath = (Get-Item -Path "./").FullName
$BaseGutenapp = Join-Path $BasePath "gutenapp"
$GutenappDir = Join-Path $BaseGutenapp "gutenapp"
$Gutenapp = Join-Path $GutenappDir "gutenapp.csproj"
$WordcountDir = Join-Path $BaseGutenapp "wordcount"
$Wordcount = Join-Path $WordcountDir "wordcount.csproj"
$MostcommonwordsDir = Join-Path $BaseGutenapp "mostcommonwords"
$Mostcommonwords = Join-Path $MostcommonwordsDir "mostcommonwords.csproj"

dotnet build $Gutenapp
dotnet build $Wordcount /p:WordcountAssemblyVersion="1.0.0"
dotnet build $Wordcount /p:WordcountAssemblyVersion="1.2.0" -o (Join-Path $WordcountDir "bin/1.2/Debug/netstandard2.0")
dotnet build (Join-Path $BaseGutenapp "tasktest")
dotnet build $Mostcommonwords
