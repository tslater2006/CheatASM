grammar CheatASM;

statementList: statement+;

statement: opCode0
         | opCode1
         | opCode2
         | opCode3
         | opCode4
         | opCode5
         | opCode6
         | opCode7
         | opCode8
         | opCode9
         | opCodeA
		 | opCodeC0;

opCode0: MOVE(bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE)PLUS_SIGN(register=REGISTER)PLUS_SIGN(offset=HEX_NUMBER) RSQUARE COMMA (value=HEX_NUMBER);
opCode1: (cond=CONDITIONAL)(bitWidth=BIT_WIDTH) LSQUARE(memType=MEM_TYPE)PLUS_SIGN(offset=HEX_NUMBER)RSQUARE COMMA (value=HEX_NUMBER);
opCode2: END_COND;
opCode3: LOOP (register=REGISTER) COMMA(value=HEX_NUMBER)
       | (endloop=END_LOOP) (register=REGISTER);
opCode4: MOVE BIT_WIDTH (register=REGISTER) COMMA (value=HEX_NUMBER);
opCode5: MOVE(bitWidth=BIT_WIDTH) (register=REGISTER) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offset=HEX_NUMBER) RSQUARE
       |  MOVE(bitWidth=BIT_WIDTH) (register=REGISTER) COMMA LSQUARE (baseRegister=REGISTER) PLUS_SIGN (offset=HEX_NUMBER) RSQUARE;
opCode6: MOVE(bitWidth=BIT_WIDTH) LSQUARE (register=REGISTER)(PLUS_SIGN(offsetReg=REGISTER))? RSQUARE COMMA (value=HEX_NUMBER) (increment=INCREMENT)?;
opCode7: (func=LEGACY_ARITHMETIC)(bitWidth=BIT_WIDTH) (register=REGISTER) COMMA (value=HEX_NUMBER);
opCode8: KEYCHECK (key=KEY);
opCode9: (func=ARTIHMETIC)(bitWidth=BIT_WIDTH) (dest=REGISTER) COMMA (leftReg=REGISTER) COMMA (rightReg=REGISTER)
       | (func=ARTIHMETIC)(bitWidth=BIT_WIDTH) (dest=REGISTER) COMMA (leftReg=REGISTER) COMMA (value=HEX_NUMBER);
opCodeA: MOVE(bitWidth=BIT_WIDTH) LSQUARE (baseReg=REGISTER) RSQUARE COMMA (sourceReg=REGISTER) (increment=INCREMENT)?
       | MOVE(bitWidth=BIT_WIDTH) LSQUARE (baseReg=REGISTER) PLUS_SIGN (regIndex=REGISTER) RSQUARE COMMA (sourceReg=REGISTER) (increment=INCREMENT)?
       | MOVE(bitWidth=BIT_WIDTH) LSQUARE (baseReg=REGISTER) PLUS_SIGN (value=HEX_NUMBER) RSQUARE COMMA (sourceReg=REGISTER) (increment=INCREMENT)?;
opCodeC0: (cond=CONDITIONAL)(bitWidth=BIT_WIDTH) (source=REGISTER) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offset=HEX_NUMBER) RSQUARE
        | (cond=CONDITIONAL)(bitWidth=BIT_WIDTH) (source=REGISTER) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offsetReg=REGISTER) RSQUARE
		| (cond=CONDITIONAL)(bitWidth=BIT_WIDTH) (source=REGISTER) COMMA LSQUARE (addrReg=REGISTER) PLUS_SIGN (offset=HEX_NUMBER) RSQUARE
		| (cond=CONDITIONAL)(bitWidth=BIT_WIDTH) (source=REGISTER) COMMA LSQUARE (addrReg=REGISTER) PLUS_SIGN (offsetReg=REGISTER) RSQUARE
		| (cond=CONDITIONAL)(bitWidth=BIT_WIDTH) (source=REGISTER) COMMA (value=HEX_NUMBER)
		| (cond=CONDITIONAL)(bitWidth=BIT_WIDTH) (source=REGISTER) COMMA (otherReg=REGISTER);

// opcodes
MOVE: 'mov';
CONDITIONAL: GT | GE | LT | LE | EQ | NE;
LOOP: 'loop';
END_LOOP: 'endloop';
KEYCHECK: 'keycheck';
END_COND: 'endcond';
INCREMENT: 'inc';
LSQUARE: '[';
RSQUARE: ']';
LCURL: '{';
RCURL: '}';
PLUS_SIGN: '+';
COMMA: ',';
// memory types

MEM_TYPE: MEMORY_HEAP | MEMORY_MAIN;
MEMORY_MAIN: 'MAIN';
MEMORY_HEAP: 'HEAP';

BIT_WIDTH: BIT_BYTE | BIT_WORD | BIT_DOUBLE | BIT_QUAD;
BIT_BYTE: 'b';
BIT_WORD: 'w';
BIT_DOUBLE: 'd';
BIT_QUAD: 'q';


// comparisons
GT: 'gt';
GE: 'ge';
LT: 'lt';
LE: 'le';
EQ: 'eq';
NE:'ne';

// arithmetic
LEGACY_ARITHMETIC: ADD | SUB | MUL | LSH | RSH ;
ARTIHMETIC: LEGACY_ARITHMETIC | AND | OR | NOT | XOR | NONE;

ADD: 'add';
SUB: 'sub';
MUL: 'mul';
LSH: 'lsh';
RSH: 'rsh';
AND: 'and';
OR: 'or';
NOT: 'not';
XOR: 'xor';
NONE: 'none';

// keys
KEY: A | B | X | Y | LSP | RSP | L | R | ZL | ZR | PLUS | MINUS | LEFT | UP | RIGHT | DOWN | LSL | LSU | LSR | LSD | RSL | RSU | RSR | RSD | SL | SR;
A: 'A';
B: 'B';
X: 'X';
Y: 'Y';
LSP: 'LSP';
RSP: 'RSP';
L: 'L';
R: 'R';
ZL: 'ZL';
ZR: 'ZR';
PLUS: 'PLUS';
MINUS: 'MINUS';
LEFT: 'LEFT';
UP: 'UP';
RIGHT: 'RIGHT';
DOWN: 'DOWN';
LSL: 'LSL';
LSU: 'LSU';
LSR: 'LSR';
LSD: 'LSD';
RSL: 'RSL';
RSU: 'RSU';
RSR: 'RSR';
RSD: 'RSD';
SL: 'SL';
SR: 'SR';

REGISTER: [Rr][0-9a-fA-F];
HEX_NUMBER: '0x'[0-9a-fA-F]+;
WS: [ \t\r\n]+ -> skip;
MASTER_CODE: '{' [0-9a-fA-F]+ '}';
CHEAT_ENTRY: '[' [0-9a-fA-F]+ ']';
COMMENT: '#'.* -> skip;