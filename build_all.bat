dotnet --version
dotnet build build\EffectFarm.Compiler\EffectFarm.Compiler.sln /p:Configuration=Release --no-incremental
dotnet build build\EffectFarm\Monogame\EffectFarm.sln /p:Configuration=Release --no-incremental
dotnet build build\EffectFarm\FNA\EffectFarm.sln /p:Configuration=Release --no-incremental
call copy_zip_package_files.bat
rename "ZipPackage" "efc.%APPVEYOR_BUILD_VERSION%"
7z a efc.%APPVEYOR_BUILD_VERSION%.zip efc.%APPVEYOR_BUILD_VERSION%
