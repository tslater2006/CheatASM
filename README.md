# CheatASM

CheatASM is a utility to disassemble Atmosphere Cheat Codes into an assembly-like language for better reading/writing of them. Similarly it can reassemble the assembly-like language into the format supported by Atmosphere.

### Prerequisites

In order to run the software your machine must have the dotnetcore 2.2+ runtime installed. To build the project you need the have the SDK installed instead.

.NET Core downloads can be found here: (https://dotnet.microsoft.com/download/dotnet-core/2.2)

### Installing

If running from an official release, simply extract the archive for your architecture and run like any other commmand line utility.

For running from source, see the "Building" section below

## Usage
Here are some useful examples of how to use this utility (please see -h for a full listing of command line arguments)

CheatASM has 2 overall modes -d for disassembly and -a for assembly, you cannot specify both flags at the same time.

#### Disassemble File to Standard Out
If no -o flag is provided CheatASM will output the result to stdout.

`CheatASM -d -i 03fd1524e17a841c.txt`

#### Disassemble File to File
By providing an -o flag, the output will be writen to file

`CheatASM -d -i 03fd1524e17a841c.txt -o 03fd1524e17a841c_asm.txt`

#### Assembling File to File
`CheatASM -a -i 03fd1524e17a841c_asm.txt -o 03fd1524e17a841c.txt`

#### Disassemble Directory Recursively
When specifying a directory for the -i flag a corresponding -o flag MUST be provided.
If the -o flag specifies the same folder as -i the files will be overwritten.
`
CheatASM -d -r -i CheatsFolder -o CheatsDisassembled
`

## Assembly Mnemonics

TBD

## Building From Source

### Ubuntu 18.04
#### Install Prerequisites 
Instructions taken from (https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/sdk-2.2.104)
```
$ wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
$ sudo dpkg -i packages-microsoft-prod.deb

$ sudo add-apt-repository universe
$ sudo apt-get install apt-transport-https
$ sudo apt-get update
$ sudo apt-get install dotnet-sdk-2.2
```
#### Clone Repository and Build
```
$ git clone https://github.com/tslater2006/CheatASM.git
$ cd CheatASM
$ dotnet publish -c Release -r linux-x64 --self-contained false
$ cd ///
$ ./CheatASM -h
```

## License

This project is licensed under the GNU Public License v2 License - see the [LICENSE.md](LICENSE.md) file for details

