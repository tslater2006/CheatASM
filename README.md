# CheatASM

CheatASM is a utility to disassemble Atmosphere Cheat Codes into an assembly-like language for better reading/writing of them. Similarly it can reassemble the assembly-like language into the format supported by Atmosphere.

### Prerequisites

In order to run the software your machine must have the dotnetcore 2.2+ runtime installed. To build the project you need the have the SDK installed instead.

.NET Core downloads can be found here: (https://dotnet.microsoft.com/download/dotnet-core/2.2)

### Installing

If running from an official release, simply extract the archive for your architecture and run like any other commmand line utility.

For running from source, see the "Building" section below

## Example

### Disassembly output

```
11130000 56C04A6C 0000000A
01100000 56C04A6C 0000000A
20000000
```

becomes

```
lt.b [HEAP+0x56C04A6C], 0xA
  mov.b [HEAP+R0+0x56C04A6C], 0xA
endcond
```

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

#### Disassemble Opcode Directly
CheatASM also allows for the dis/assembly of a opcode or assembly instruction directly using the -t parameter. If -o is not specified the result will be written to stdout, otherwise to the file specified by the -o parameter.

`
CheatASM -d -t "04010000 006C7634 0098967F"
`

`
CheatASM -a -t "mov.d [MAIN+R1+0x6C7634], 0x98967F"
`

## Assembly Mnemonics

The desire is for the assembly mnemonics to not be something you need to memorize or reference often. The aim is to be as intuitive as possible (assuming you have assembly exposure) and just let you write what you need. There are some general patterns that hte mnemonics follow:

`command(bitwidth) param1, param2`

Command can be something like "mov" or "add", BitWidth is one of the following: "b", "w", "d", "q" for Byte, Word, Dword, Qword respectively.

When using the more complex addressing modes the order that items should be specified is always `MemoryType -> Register -> Literal value`.

### OpCode Mnemonic Listing

Below you will find examples of every mnemonic supported by CheatASM. To find out what each opcode does, please reference the Atmosphere Cheat docs.

#### Opcode 0 (Store Static)

Moves 0x1234 to the QWORD at HEAP+R0+0x1234

```
mov.q [HEAP+R0+0x1234], 0x1234
```

#### Opcode 1 (Conditional)

Compares the byte at HEAP+0x1234 and enters the conditional block if it is less than 0x12.

```
lt.b [HEAP+0x1234], 0x12
   ...
endcond
```

#### Opcode 2 (End Conditional)

Terminates a conditional block

```
endcond
```

#### Opcode 3 (Begin/End Loop)

```
loop R0, 0x23
   ...
endloop R0
```

#### Opcode 4 (Static to Register)

Moves a static value into a register (bitwidth is always q)

```
mov R0, 0x1234
mov.q R0, 0x1234
```

#### Opcode 5 (Memory to Register)

Move a value from memory into a register

```
mov.b R0, [HEAP+0x1234]
mov.w R0, [R0+0x1234] 
```

#### Opcode 6 (Static to Address)

```
mov.q [R0], 0x1234
mov.q [R0], 0x1234 inc

mov.q [R0+R2], 0x1234
mov.q [R0+R2], 0x1234 inc
```

#### Opcode 7 (Legacy Arithmetic)

Supported arithmetic methods are `add, sub, mul, lsh, rsh`

```
add.q R0, 0x1234
```

#### Opcode 8  (KeyCheck)

Supported keys are: `A, B, X, Y, LSP, RSP, L, R, ZL, ZR, PLUS, MINUS, LEFT, UP, RIGHT, DOWN, LSL, LSU, LSR, LSD, RSL, RSU, RSR,RSD, SL, and SR`

```
keycheck <key>
```


#### Opcode 9 (Arithmetic)

Supported arithmetic methods are `add, sub, mul, lsh, rsh, and, or, not, xor, none`

```
add.q R0, R1, R2
add.q R0, R1, 0x1234
```

#### Opcode 10 (Register to Address)

TBD

#### Extended Opcode 0xC0 (Register Conditional)

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
$ cd CheatASM/bin/Release/netcoreapp2.2/linux-x64/publish/
$ ./CheatASM -h
```

## License

This project is licensed under the GNU Public License v2 License - see the [LICENSE.md](LICENSE.md) file for details

