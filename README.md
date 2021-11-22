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

`command.bitwidth param1, param2`

Command can be something like "mov" or "add", BitWidth is one of the following: "b", "w", "d", "q" for Byte, Word, Dword, Qword respectively.

When using the more complex addressing modes the order that items should be specified is always `MemoryType -> Register -> Literal value`.

### Variables

Cheat ASM allows for you to define mutable variables as well as constant variables. During assembly constant variables will have their literal value substituted, while mutable variables will be assigned a register number. The assigned register number if effectively reserved for use and the assembler will error in the event of an explicit use of a register number that is assigned to a variable.

Here is a concrete example of how to use variables with CheatASM:

Note: current issue with mutable variables, while the syntax allows you to provide an initial value, the cheats are not assembled with init logic, this is in progress.

```
floatTest: .f32 4.83
mainOffset: .u32 const 0x1234
coinOffset: .u32 const 0x12
ten: .u32 0xA

.cheat master "Setup"
mov.d[R0 + 0x123], floatTest
mov.q R0, [MAIN + mainOffset]

.cheat "Sample"
mov.d[R0 + mainOffset], R7
mov.q R0, [MAIN + mainOffset]
```

### OpCode Mnemonic Listing

Below you will find examples of every mnemonic supported by CheatASM. To find out what each opcode does, please reference the Atmosphere Cheat docs.

#### Opcode 0 (Store Static)

```
mov.d [MAIN + R2], 0x123
mov.q [MAIN + R2], 0x123
mov.d [MAIN + R8 + 0x7], 0x345
mov.w [HEAP + R7], 0x236
mov.b [HEAP + R7 + 0x9], 0xFE"
```

#### Opcode 1 (Conditional)

Compares the byte at address and enters the conditional block if it is less than value.

```
gt.d [MAIN + 0x123], 0x456
ge.b [HEAP + 0x123], 0x56
lt.q [MAIN + 0x123456789A], 0x4564123478
le.d [MAIN + 0x123], 0x456
eq.d [MAIN + 0x123], 0x456
ne.d [MAIN + 0x123], 0x456
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
mov.b R0, 0x1
mov R2, 0x3
mov.d R7, 0x12345678
mov.q RA, 0x1122334455667788
```

#### Opcode 5 (Memory to Register)

Move a value from memory into a register

```
mov.b R0, [MAIN + 0x12]
mov.d R0, [MAIN + 0x0]
mov.w R0, [MAIN]
mov.q R0, [HEAP + 0x1122334455]
mov.b R0, [R0 + 0x12]
mov.d R0, [R0 + 0x0]
mov.w R0, [R0]
```

#### Opcode 6 (Static to Address)

```
mov.b [R7], 0x12
mov.w [R4], 0x12 inc
mov.d [R3 + R2], 0x12
mov.q [RB + R2], 0x1122334455667788
```

#### Opcode 7 (Legacy Arithmetic)

Supported arithmetic methods are `add, sub, mul, lsh, rsh`

```
add.b R7, 0x12
sub.w R4, 0x12
mul.d R3,  0x12
lsh.q RB, 0x11223344
rsh.d R4, 0x55667788
```

#### Opcode 8  (KeyCheck)

Supported keys are: `A, B, X, Y, LSP, RSP, L, R, ZL, ZR, PLUS, MINUS, LEFT, UP, RIGHT, DOWN, LSL, LSU, LSR, LSD, RSL, RSU, RSR,RSD, SL, and SR`

```
keycheck <key>
```


#### Opcode 9 (Arithmetic)

Supported arithmetic methods are `add, sub, mul, lsh, rsh, and, or, not, xor, none`

```
add.b R7, R3, 0x12
sub.w R4, R1, 0x12
mul.d R3, R0, 0x12
lsh.q RB, RD, 0x112233445566
rsh.d R4, R7, 0x55667788
and.b R7, R3, 0x12
or.b R7, R3, 0x12
xor.b R7, R3, 0x12
add.b R7, R3, R2
sub.w R4, R1, R2
mul.d R3, R0, R7
lsh.q RB, RD, R8
rsh.d R4, R7, R1
and.b R7, R3, R0
or.b R7, R3, R4
xor.b R7, R3, R6
not.b R7, R3
copy.b R7, R3
```

#### Opcode 10 (Register to Address)

```
mov.d [R0], R1
mov.d [R9], R2 inc
mov.d [RA+ R1], R3
mov.d [RB + R1], R4 inc
mov.d [RC + 0x123], R5
mov.d [RD + 0x123], R6 inc
mov.d [MAIN + RE], R7
mov.d [MAIN + 0x123], R7
mov.d [HEAP + RF + 0x123456789], R8
```

#### Extended Opcode 0xC0 (Register Conditional)

```
gt.b R0, R1
ge.w R0, 0x123
lt.d R0, [R1]
le.q R0, [R1 + 0x123]
eq.b R0, [R1 + R2]
ne.w R0, [MAIN + 0x123]
gt.d R0, [HEAP + 0x123]
ge.q R0, [MAIN]
lt.b R0, [MAIN + R0]                
```

#### Save/Load/Clear Register
```
save.reg 0x1, R3
load.reg R3, 0x1
clear.reg R3
clear.saved 0x1
```

#### Save/Load/Clear Registers
```
save.regs R1
save.regs R1,R2
save.regs R0, R1,R2,R3,R4,R5,R6,R7,R8,R9,RA,RB,RC,RD,RE,RF

load.regs R1
load.regs R1,R2
load.regs R0,R1,R2,R3,R4,R5,R6,R7,R8,R9,RA,RB,RC,RD,RE,RF

clear.regs R1
clear.regs R1,R2
clear.regs R0,R1,R2,R3,R4,R5,R6,R7,R8,R9,RA,RB,RC,RD,RE,RF
```

#### Save/Load/Clear Static Register

Static registers are referenced by SR\<hex number\>

```
save.static SR1, R0
load.static R0, SR7
save.static SR7F, R1
load.static R6, SR7F
```

#### Pause Process
```
pause
```

#### Resume Process
```
resume
```

#### Debug Log

Format is `log.<bit_width> 0xLogID, value`

```
log.b 0x1, [MAIN]
log.w 0x1, [MAIN + 0x123]
log.d 0x1, [MAIN + R3]
log.q 0x1, [HEAP + 0x123]
log.b 0x1, [R2]
log.b 0x1, [R2 + R3]
log.w 0x1, [R4 + 0x123]
log.d 0x1, R7
```



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

