rem delete existing
rmdir "ZipPackage" /Q /S

rem Create required folders
mkdir "ZipPackage"

set "CONFIGURATION=bin\Release\net472"

rem Copy output files
copy "src\EffectFarm.Compiler\%CONFIGURATION%\CppNet.dll" "ZipPackage" /Y
copy "src\EffectFarm.Compiler\%CONFIGURATION%\efc.exe" "ZipPackage" /Y
copy "src\EffectFarm.Compiler\%CONFIGURATION%\efc.pdb" "ZipPackage" /Y
copy "src\EffectFarm.Compiler\%CONFIGURATION%\libmojoshader_64.dll" "ZipPackage" /Y
copy "src\EffectFarm.Compiler\%CONFIGURATION%\MonoGame.Build.Tasks.dll" "ZipPackage" /Y
copy "src\EffectFarm.Compiler\%CONFIGURATION%\MonoGame.Framework.dll" "ZipPackage" /Y
copy "src\EffectFarm.Compiler\%CONFIGURATION%\MonoGame.Framework.Content.Pipeline.dll" "ZipPackage" /Y
copy "src\EffectFarm.Compiler\%CONFIGURATION%\SharpDX.dll" "ZipPackage" /Y
copy "src\EffectFarm.Compiler\%CONFIGURATION%\SharpDX.D3DCompiler.dll" "ZipPackage" /Y
