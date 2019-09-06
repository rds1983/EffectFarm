# Overview
EffectFarm is MonoGame/FNA framework to compile multiple variants of one effect. It consists of two parts:
* efc.exe - a command line utility that compiles multiple variants of one effect according to provided config file in xml format.
* EffectFarm - a library to consume result produced by efc.exe

# efc.exe
## Obtaining
The easiest way to obtain efc is to download latest binary release(EffectFarm.v.v.v.v.zip) from [releases](https://github.com/rds1983/EffectFarm/releases) and unpack it.

## Compiling
`efc.exe DefaultEffect.fx DefaultEffect.xml`

## Config
Following is example of compiler config file:
```xml
<Root>
    <Targets>
        <MonoGameDirectX/>
        <MonoGameOpenGL/>
        <FNA/>
    </Targets>
    <RootEntry>
        <Define/>
        <Define Name="LIGHTNING" />
        <Entry>
            <Define/>
            <Define Name="BONES" Value="1"/>
            <Define Name="BONES" Value="2"/>
            <Define Name="BONES" Value="4"/>
        </Entry>
    </RootEntry>
</Root>
```
It consists of targets lists and variants tree.
Above config would compile the effect with following variants of defines:
1. [No defines]
2. BONES=1
3. BONES=2
4. BONES=4
5. LIGHTNING=1
6. LIGHTNING=1;BONES=1
7. LIGHTNING=1;BONES=2
8. LIGHTNING=1;BONES=4

As above config also defines all 3 possible targets(MonoGameDirectX, MonoGameOpenGL, FNA), total amount of different effect variants would be 24(3 * 8).
And efc.exe would output following:
```
Effect farm compiler to efb 1.
24 variants of effects are going to be compiled.
#0: MonoGameDirectX/
#1: MonoGameDirectX/BONES=1
#2: MonoGameDirectX/BONES=2
#3: MonoGameDirectX/BONES=4
#4: MonoGameDirectX/LIGHTNING=1
#5: MonoGameDirectX/BONES=1;LIGHTNING=1
#6: MonoGameDirectX/BONES=2;LIGHTNING=1
#7: MonoGameDirectX/BONES=4;LIGHTNING=1
#8: MonoGameOpenGL/
#9: MonoGameOpenGL/BONES=1
#10: MonoGameOpenGL/BONES=2
#11: MonoGameOpenGL/BONES=4
#12: MonoGameOpenGL/LIGHTNING=1
#13: MonoGameOpenGL/BONES=1;LIGHTNING=1
#14: MonoGameOpenGL/BONES=2;LIGHTNING=1
#15: MonoGameOpenGL/BONES=4;LIGHTNING=1
#16: FNA/
#17: FNA/BONES=1
#18: FNA/BONES=2
#19: FNA/BONES=4
#20: FNA/LIGHTNING=1
#21: FNA/BONES=1;LIGHTNING=1
#22: FNA/BONES=2;LIGHTNING=1
#23: FNA/BONES=4;LIGHTNING=1
Compilation to DefaultEffect.efb was succesful.
```
*DefaultEffect.efb* would be resulting file containing all 24 variants.

## EffectFarm Library
Now to use .efb file, first of all, it is required to reference EffectFarm library.

## Adding reference to EffectFarm Library
There are two ways of referencing EffectFarm in the project:
1. Through nuget: `install-package EffectFarm.MonoGame` for MonoGame(or `install-package EffectFarm.FNA` for FNA)
2. As submodule:
    
    a. `git submodule add https://github.com/rds1983/EffectFarm.git`

    b. `git submodule update --init --recursive`
    
    b. Copy SolutionDefines.targets from EffectFarm/build/MonoGame(or EffectFarm/build/FNA) to your solution folder.

      * If FNA is used, SolutionDefines.targets needs to be edited and FNAProj variable should be updated to the location of FNA.csproj next to the EffectFarm location.
    
    c. Add EffectFarm/src/EffectFarm/EffectFarm.csproj to the solution.
    
## MultiVariantEffect
To create MultiVariantEffect it is required to provide Func<Stream> that opens a stream with .efb file. As it is going to be opened multiple times: one time in constructor to gather information what variants does .efb contains and one time for every GetEffect call witn unique defines.
This is example code of MultiVariantEffect creation:
```c#
  MultiVariantEffect multiEffect = new MultiVariantEffect(() => File.OpenRead("DefaultEffect.efb"));
```
    
And this code loads specified effect:
```c#
  Effect effect = multiEffect.GetEffect(graphicsDevice, new Dictionary<string, string>
  {
    ["BONES"]="1",
    ["LIGHTNING"]="1"
  });
```
    
