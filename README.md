# Peon
## Motivation
Lookuping for a file ids of model is very boring, Peon gives solutions to do it quickly and mostly without user intervention.

## What can it do
Current features:
- Lookup for file ids in single model, for each model in models list or models in directory 
- Download files or generate listfile from files of readed models

Releases are provided for Windows x64, Linux x64 and MacOS x64. This has only been tested on Windows.

[Find the latest release here](https://github.com/illunix/Peon/releases).

## Commands and options
To list all commands type ```Peon.exe --help```,
to list all options of command type ```Peon.exe [command] --help```

## Examples of use
```
Peon.exe get --model "C:\Users\sample\Desktop\sylvanasshadowlands.m2"
```
```
Peon.exe generate --listfile "C:\Users\sample\Desktop\listfile.csv" --models-dir "C:\Users\sample\Desktop\MyModels" --verbose
```