$f = ls -Recurse | where { $_ -is [IO.FileInfo] -and [IO.Path]::GetExtension($_.Name) -Match "\.(cs|csproj|nsi|xaml|sln|config|user|resx|settings|ps1)" }
($f | measure).Count
$size = 0
$lines = 0
$f | foreach { $size += $_.Length };
$size
$f | foreach { $lines += (gc $_.FullName | Measure-Object -Line).Lines };
$lines