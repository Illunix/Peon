# Peon
## Motivation
Lookuping for a file ids of model is very boring, Peon gives solutions to do it quickly and mostly without user interaction.

## What can it do
Current features:
- Lookup for file ids in single model, for each model in models list or models in directory 
- Download files or generate listfile from files of readed models

It works only for 26629+ build models (for some builds it may not work)

Releases are provided for Windows x64, Linux x64 and MacOS x64. This has only been tested on Windows.

[Find the latest release here](https://github.com/illunix/Peon/releases).

## Commands and options
To list all commands type ```Peon.exe --help```,
to list all options of command type ```Peon.exe [command] --help```

## How it works?
* Read ``M2`` -> Get all ``anim``, ``skin`` and ``blp`` files
* Read ``WMO`` -> Get all ``m2`` and ``blp`` files ->  Read ``M2``
* Read ``ADT`` -> Get all ``wmo``and ``blp`` files -> Read ``WMO``-> Read ``M2``

## Examples of use
```
Peon.exe get --model "C:\Users\sample\Desktop\sylvanasshadowlands.m2"
```
```
Peon.exe generate --listfile "C:\Users\sample\Desktop\listfile.csv" --models-dir "C:\Users\sample\Desktop\MyModels" --verbose
```

## Links
- [Discord](https://discord.gg/vcpwDVN)
- [Model Changing](https://model-changing.net/)
