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

opCode0: MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE)PLUS_SIGN(register=REGISTER)PLUS_SIGN(offset=HEX_NUMBER) RSQUARE COMMA (value=HEX_NUMBER);
opCode1: (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) LSQUARE(memType=MEM_TYPE)PLUS_SIGN(offset=HEX_NUMBER)RSQUARE COMMA (value=HEX_NUMBER);
opCode2: END_COND;
opCode3: LOOP (register=REGISTER) COMMA(value=HEX_NUMBER)
       | (endloop=END_LOOP) (register=REGISTER);
opCode4: MOVE (DOT BIT_WIDTH)? (register=REGISTER) COMMA (value=HEX_NUMBER);
opCode5: MOVE DOT (bitWidth=BIT_WIDTH) (register=REGISTER) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offset=HEX_NUMBER) RSQUARE
       |  MOVE DOT (bitWidth=BIT_WIDTH) (register=REGISTER) COMMA LSQUARE (baseRegister=REGISTER) PLUS_SIGN (offset=HEX_NUMBER) RSQUARE;
opCode6: MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (register=REGISTER)(PLUS_SIGN(offsetReg=REGISTER))? RSQUARE COMMA (value=HEX_NUMBER) (increment=INCREMENT)?;
opCode7: (func=LEGACY_ARITHMETIC) DOT (bitWidth=BIT_WIDTH) (register=REGISTER) COMMA (value=HEX_NUMBER);
opCode8: KEYCHECK (key=KEY);
opCode9: (func=ARTIHMETIC) DOT (bitWidth=BIT_WIDTH) (dest=REGISTER) COMMA (leftReg=REGISTER) COMMA (rightReg=REGISTER)
       | (func=ARTIHMETIC) DOT (bitWidth=BIT_WIDTH) (dest=REGISTER) COMMA (leftReg=REGISTER) COMMA (value=HEX_NUMBER);
opCodeA: MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (baseReg=REGISTER) RSQUARE COMMA (sourceReg=REGISTER) (increment=INCREMENT)?
       | MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (baseReg=REGISTER) PLUS_SIGN (regIndex=REGISTER) RSQUARE COMMA (sourceReg=REGISTER) (increment=INCREMENT)?
       | MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (baseReg=REGISTER) PLUS_SIGN (value=HEX_NUMBER) RSQUARE COMMA (sourceReg=REGISTER) (increment=INCREMENT)?
	   | MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE) PLUS_SIGN (baseReg=REGISTER) RSQUARE COMMA (sourceReg=REGISTER) (increment=INCREMENT)?
	   | MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE) PLUS_SIGN (value=HEX_NUMBER) RSQUARE COMMA (sourceReg=REGISTER) (increment=INCREMENT)?
	   | MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE) PLUS_SIGN (baseReg=REGISTER) PLUS_SIGN (value=HEX_NUMBER) RSQUARE COMMA (sourceReg=REGISTER) (increment=INCREMENT)?;

opCodeC0: (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=REGISTER) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offset=HEX_NUMBER) RSQUARE
        | (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=REGISTER) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offsetReg=REGISTER) RSQUARE
		| (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=REGISTER) COMMA LSQUARE (addrReg=REGISTER) PLUS_SIGN (offset=HEX_NUMBER) RSQUARE
		| (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=REGISTER) COMMA LSQUARE (addrReg=REGISTER) PLUS_SIGN (offsetReg=REGISTER) RSQUARE
		| (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=REGISTER) COMMA (value=HEX_NUMBER)
		| (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=REGISTER) COMMA (otherReg=REGISTER);

// Lexer Rules
MOVE: M O V;
CONDITIONAL: GT | GE | LT | LE | EQ | NE;
LOOP: L O O P;
END_LOOP: E N D L O O P;
KEYCHECK: K E Y C H E C K;
END_COND: E N D C O N D;
INCREMENT: I N C;
LSQUARE: '[';
RSQUARE: ']';
LCURL: '{';
RCURL: '}';
PLUS_SIGN: '+';
COMMA: ',';
// memory types

MEM_TYPE: MEMORY_HEAP | MEMORY_MAIN;
MEMORY_MAIN: M A I N;
MEMORY_HEAP: H E A P;

BIT_WIDTH: BIT_BYTE | BIT_WORD | BIT_DOUBLE | BIT_QUAD;
BIT_BYTE: B;
BIT_WORD: W;
BIT_DOUBLE: D;
BIT_QUAD: Q;


// comparisons
GT: G T;
GE: G E;
LT: L T;
LE: L E;
EQ: E Q;
NE: N E;

// arithmetic
LEGACY_ARITHMETIC: ADD | SUB | MUL | LSH | RSH ;
ARTIHMETIC: LEGACY_ARITHMETIC | AND | OR | NOT | XOR | NONE;

ADD: A D D;
SUB: S U B;
MUL: M U L;
LSH: L S H;
RSH: R S H;
AND: A N D;
OR: O R;
NOT: N O T;
XOR: X O R;
NONE: N O N E;

// keys
KEY: A_KEY | B_KEY | X_KEY | Y_KEY | LSP_KEY | RSP_KEY | L_KEY 
   | R_KEY | ZL_KEY | ZR_KEY | PLUS_KEY | MINUS_KEY | LEFT_KEY 
   | UP_KEY | RIGHT_KEY | DOWN_KEY | LSL_KEY | LSU_KEY | LSR_KEY 
   | LSD_KEY | RSL_KEY | RSU_KEY | RSR_KEY | RSD_KEY | SL_KEY | SR_KEY;
A_KEY: A;
B_KEY: B;
X_KEY: X;
Y_KEY: Y;
LSP_KEY: L S P;
RSP_KEY: R S P;
L_KEY: L;
R_KEY: R;
ZL_KEY: Z L;
ZR_KEY: Z R;
PLUS_KEY: P L U S;
MINUS_KEY: M I N U S;
LEFT_KEY: L E F T;
UP_KEY: U P;
RIGHT_KEY: R I G H T;
DOWN_KEY: D O W N;
LSL_KEY: L S L;
LSU_KEY: L S U;
LSR_KEY: L S R;
LSD_KEY: L S D;
RSL_KEY: R S L;
RSU_KEY: R S U;
RSR_KEY: R S R;
RSD_KEY: R S D;
SL_KEY: S L;
SR_KEY: S R;

REGISTER: [Rr][0-9a-fA-F];
HEX_NUMBER: '0' X [0-9a-fA-F]+;
WS: [ \t\r\n]+ -> skip;
MASTER_CODE: '{' [0-9a-zA-Z]+ '}';
CHEAT_ENTRY: '[' [0-9a-zA-Z]+ ']';
COMMENT: '#'.* -> skip;
DOT: '.';
fragment A : [aA]; // match either an 'a' or 'A'
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];