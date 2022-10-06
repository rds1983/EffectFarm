$version = $args[0]
echo "Version: $version"

# Recreate "ZipPackage"
Remove-Item -Recurse -Force "ZipPackage" -ErrorAction Ignore
Remove-Item -Recurse -Force "efc.$version" -ErrorAction Ignore

New-Item -ItemType directory -Path "ZipPackage"

function Copy-File([string]$FileName) {
	Copy-Item -Path "src\efc\bin\Release\net6.0\win-x64\$FileName" -Destination "ZipPackage"
}

Copy-File "efc.deps.json"
Copy-File "efc.dll"
Copy-File "efc.exe"
Copy-File "efc.pdb"
Copy-File "efc.runtimeconfig.json"
Copy-File "MonoGame.Framework.Content.Pipeline.dll"
Copy-File "MonoGame.Framework.dll"
Copy-File "SharpDX.D3DCompiler.dll"
Copy-File "SharpDX.dll"

# Compress
Rename-Item "ZipPackage" "efc.$version"
Compress-Archive -Path "efc.$version" -DestinationPath "efc.$version.zip" -Force

# Delete the folder
Remove-Item -Recurse -Force "efc.$version"