param($installPath, $toolsPath, $package, $project)

$analyzersPath = join-path $toolsPath "analyzers"
$librariesPath = join-path $toolsPath "libraries"

# Install the language agnostic analyzers.
#foreach ($analyzerFilePath in Get-ChildItem $analyzersPath)
#{
#    $project.Object.AnalyzerReferences.Add($analyzerFilePath.FullName)
#}
#
#foreach ($libraryFilePath in Get-ChildItem $librariesPath)
#{
#    $project.Object.References.Add($libraryFilePath.FullName)
#}

# Install language specific analyzers.
# $project.Type gives the language name like (C# or VB.NET)
$languageAnalyzersPath = join-path $analyzersPath $project.Type
$languageLibrariesPath = join-path $librariesPath $project.Type

foreach ($analyzerFilePath in Get-ChildItem $languageAnalyzersPath)
{
    $project.Object.AnalyzerReferences.Add($analyzerFilePath.FullName)
}

foreach ($languageLibraryFilePath in Get-ChildItem $languageLibrariesPath)
{
    $project.Object.References.Add($languageLibraryFilePath.FullName)
}