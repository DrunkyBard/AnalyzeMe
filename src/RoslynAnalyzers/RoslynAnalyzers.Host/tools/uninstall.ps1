param($installPath, $toolsPath, $package, $project)

# Uninstall the language agnostic analyzers.
$analyzersPath = join-path $toolsPath "analyzers"
$librariesPath = join-path $toolsPath "libraries"

#foreach ($analyzerFilePath in Get-ChildItem $analyzersPath)
#{
#	Write-Host $analyzerFilePath.FullName
#	echo $analyzerFilePath.FullName
#    $project.Object.AnalyzerReferences.Remove($analyzerFilePath.FullName)
#}
#
#foreach ($libraryFilePath in Get-ChildItem $librariesPath)
#{
#    $project.Object.References.Item($libraryFilePath.BaseName).Remove()
#}

# Uninstall language specific analyzers.
# $project.Type gives the language name like (C# or VB.NET)
$languageAnalyzersPath = join-path $analyzersPath $project.Type
$languageLibraryPath = join-path $librariesPath $project.Type

foreach ($analyzerFilePath in Get-ChildItem $languageAnalyzersPath)
{
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath.FullName)
}

foreach ($libraryFilePath in Get-ChildItem $languageLibraryPath)
{
    $project.Object.References.Item($libraryFilePath.BaseName).Remove()
}