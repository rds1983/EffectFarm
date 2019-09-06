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

As above config also defines all 3 possible targets(MonoGameDirectX, MonoGameOpenGL, FNA)

