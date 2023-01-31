# Overview
[![NuGet](https://img.shields.io/nuget/v/EffectFarm.MonoGame.svg)](https://www.nuget.org/packages/EffectFarm.MonoGame/)
[![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

EffectFarm is MonoGame/FNA framework that helps the compilation of many effects.

It consists of two parts:
* efscriptgen.exe - a command line utility that generates batch scripts for compiling all effects in the specified folder
* EffectFarm - a class library that loads effects generated through those batch scripts 

# efscriptgen.exe
## Installation
`dotnet tool install -g efscriptgen`

## Usage
efscriptgen usage is quite simple: `efscriptgen.exe <folder>`

The utility would go through every .fx file in the specified folder and generate 3 batch scripts:
* compile_fna.bat
* compile_mgdx11.bat
* compile_mgogl.bat

Every batch script would contain commands to compile effects for the corresponding platform.

I.e. sample compile_mgogl.bat would look like this:
```
mgfxc ".\a.fx" ".\MonoGameOGL\a.mgfxo" /Profile:OpenGL
mgfxc ".\b.fx" ".\MonoGameOGL\b.mgfxo" /Profile:OpenGL
mgfxc ".\c.fx" ".\MonoGameOGL\c.mgfxo" /Profile:OpenGL
```

If you want a specific effect to be compiled with one or more set of defines, then you need to create corresponding .xml file in the effects folder.

I.e. if we create file a.xml with the following contents:
```
<Defines>
	<Define Value="LIGHTNING"/>
	<Define Value="CLIP_PLANE;LIGHTNING"/>
	<Define Value="BONES=4;LIGHTNING"/>
	<Define Value="BONES=4;CLIP_PLANE;LIGHTNING"/>
</Defines>
```
and then rerun efscriptgen.exe, then the compile_mgogl.bat would look:
```
mgfxc ".\a.fx" ".\MonoGameOGL\a_LIGHTNING.mgfxo" /Profile:OpenGL /Defines:LIGHTNING
mgfxc ".\a.fx" ".\MonoGameOGL\a_CLIP_PLANE_LIGHTNING.mgfxo" /Profile:OpenGL /Defines:CLIP_PLANE;LIGHTNING
mgfxc ".\a.fx" ".\MonoGameOGL\a_BONES_4_LIGHTNING.mgfxo" /Profile:OpenGL /Defines:BONES=4;LIGHTNING
mgfxc ".\a.fx" ".\MonoGameOGL\a_BONES_4_CLIP_PLANE_LIGHTNING.mgfxo" /Profile:OpenGL /Defines:BONES=4;CLIP_PLANE;LIGHTNING
mgfxc ".\b.fx" ".\MonoGameOGL\b.mgfxo" /Profile:OpenGL
mgfxc ".\c.fx" ".\MonoGameOGL\c.mgfxo" /Profile:OpenGL
```

As you can see the defines would be included in the names of compiled effects.

# EffectFarm Class Library
## Installation for MonoGame
https://www.nuget.org/packages/EffectFarm.MonoGame/
## Installation for FNA
## Usage
    
```c#
  EffectsRepository effectsRepo = EffectsRepository.CreateFromFolder(@"D:\Projects\Nursia\src\Nursia\EffectsSource\FNA");
  Effect effect = effectsRepo.GetEffect(graphicsDevice, new Dictionary<string, string>
  {
    ["BONES"]="1",
    ["LIGHTNING"]="1"
  });
```
