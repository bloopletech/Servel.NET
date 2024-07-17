Set-StrictMode -Version Latest
cd "$PSScriptRoot"

$gitStatus = (git status --porcelain) | Out-String
if($gitStatus -ne "")
{
  echo "Your git repo isn't clean, aborting."
  exit 1
}

$servelVersionNode = (Select-Xml -Path Servel.NET\Servel.NET.csproj -XPath '//Version[1]/text()[1]').Node

$existingVersion = [Version]::new($servelVersionNode.InnerText)
$newVersion = [Version]::new($existingVersion.Major, $existingVersion.Minor + 1, $existingVersion.Build)

$servelVersionNode.InnerText = $newVersion.ToString(3)

$xmlSettings = New-Object System.Xml.XmlWriterSettings
$xmlSettings.OmitXmlDeclaration = $true

$xmlWriter = [System.XML.XmlWriter]::Create((Join-Path -Path $PSScriptRoot -ChildPath Servel.NET\Servel.NET.csproj), $xmlSettings)
$servelVersionNode.OwnerDocument.Save($xmlWriter)
$xmlWriter.Close()

git add -A && git commit -m 'Bump version'