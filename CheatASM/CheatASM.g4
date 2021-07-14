grammar CheatASM;

program: gameInfo? titleID? buildID? variableDecl* cheatEntry+
       | variableDecl* statement+;

statement: opCode1
         | opCode2
         | opCode3
         | opCode7
         | opCode8
         | opCode9
         | movInstr
		 | opCodeC0
		 | opCodeC1C2
		 | opCodeC3
		 | opCodeFF0
		 | opCodeFF1
		 | opCodeFFF
		 | COMMENT_LINE;

cheatEntry: (header=cheatHeader) statement+;

titleID: DOT 'title'  (title=ID);
buildID: DOT 'build'  (build=ID);
gameInfo: DOT 'gameinfo' (info=CHEAT_NAME);

cheatHeader: DOT 'cheat'  (master=MASTER)? (name=CHEAT_NAME);

/* cond.d [HEAP + num], num */
opCode1: (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) LSQUARE(memType=MEM_TYPE)PLUS_SIGN(offset=numRef)RSQUARE COMMA (value=numRef);
opCode2: END_COND;
opCode3: LOOP (register=regRef) COMMA(value=numRef)
       | (endloop=END_LOOP) (register=regRef);

opCode7: (func=LEGACY_ARITHMETIC) DOT (bitWidth=BIT_WIDTH) (register=regRef) COMMA (value=numRef);
opCode8: KEYCHECK (key=KEY);

opCode9: (func=ARITHMETIC) DOT (bitWidth=BIT_WIDTH) (dest=regRef) COMMA (leftReg=regRef) COMMA (right=anyRef);

movInstr: MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE) PLUS_SIGN (regOffset=regRef) (PLUS_SIGN (numOffset=numRef))? RSQUARE COMMA (value=numRef) #opCode0
        | MOVE (DOT BIT_WIDTH)? (register=regRef) COMMA (value=numRef) # opCode4
        | MOVE DOT (bitWidth=BIT_WIDTH) (register=regRef) COMMA LSQUARE (memType=MEM_TYPE) (PLUS_SIGN (numOffset=numRef))? RSQUARE # opCode5
        | MOVE DOT (bitWidth=BIT_WIDTH) (register=regRef) COMMA LSQUARE (baseRegister=regRef) (PLUS_SIGN (numOffset=numRef))? RSQUARE # opCode5
        | MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (base=regRef) (PLUS_SIGN (regOffset=regRef))? RSQUARE COMMA (value=numRef) (increment=INCREMENT)? #opCode6
        | MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (base=regRef) (PLUS_SIGN (regOffset=regRef))? RSQUARE COMMA (regValue=regRef) (increment=INCREMENT)? #opCodeA
        | MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (base=regRef) (PLUS_SIGN (numOffset=numRef))? RSQUARE COMMA (regValue=regRef) (increment=INCREMENT)? #opCodeA
        | MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE) (PLUS_SIGN (regOffset=regRef))? (PLUS_SIGN (numOffset=numRef))? RSQUARE COMMA (regValue=regRef) #opCodeA;

opCodeC0: (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offset=anyRef) RSQUARE
		| (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA LSQUARE (addrReg=regRef) PLUS_SIGN (offset=anyRef) RSQUARE
		| (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA (value=anyRef);

opCodeC1C2: (func=SAVE) DOT (type=REG) (index=numRef) COMMA (reg=regRef)
        | (func=LOAD) DOT (type=REG) (reg=regRef) COMMA (index=numRef)
        | (func=SAVE) DOT (type=REGS) (regs=regList)
        | (func=LOAD) DOT (type=REGS) (regs=regList)
        | (func=CLEAR) DOT (type=REG) (reg=regRef)
        | (func=CLEAR) DOT (type=SAVED) (index=numRef)
        | (func=CLEAR) DOT (type=REGS) (regs=regList)
        | (func=CLEAR) DOT (type=SAVED) (indexes=indexList);

opCodeC3: (func=SAVE) DOT (type=STATIC) (sreg=SREGISTER) COMMA (reg=regRef)
        | (func=LOAD) DOT (type=STATIC) (reg=regRef) COMMA (sreg=SREGISTER);

opCodeFF0: (func=PAUSE);

opCodeFF1: (func=RESUME);

opCodeFFF: (func=LOG) DOT (bitWidth=BIT_WIDTH) (id=HEX_NUMBER) COMMA LSQUARE (memType=MEM_TYPE) (PLUS_SIGN (offset=anyRef))? RSQUARE
        |  (func=LOG) DOT (bitWidth=BIT_WIDTH) (id=HEX_NUMBER) COMMA LSQUARE (addrReg=regRef) (PLUS_SIGN (offset=anyRef))? RSQUARE
        |  (func=LOG) DOT (bitWidth=BIT_WIDTH) (id=HEX_NUMBER) COMMA (value=regRef);

numberLiteral : IntegerLiteral | DecimalLiteral | HEX_NUMBER;
regRef: (reg=REGISTER) | (var=VARIABLE_NAME);

numRef: (num=HEX_NUMBER) | (var=VARIABLE_NAME);

anyRef: (reg=REGISTER) | (num=HEX_NUMBER) | (var=VARIABLE_NAME);

variableDecl:  (name=VARIABLE_NAME) COLON (type=VARIABLE_TYPE) (const=CONST)? (val=numberLiteral);

regList: (reg=regRef) (COMMA regRef)*;
indexList: (index=numRef) (COMMA numRef)*;

// Lexer Rules
MOVE: M O V;
CONDITIONAL: GT | GE | LT | LE | EQ | NE;
LOOP: L O O P;
END_LOOP: E N D L O O P;
KEYCHECK: K E Y C H E C K;
END_COND: E N D C O N D;
INCREMENT: I N C;
SAVE: S A V E ;
SAVEREG: S A V E R E G;
SAVEALL: S A V E A L L;
LOAD: L O A D;
LOADREG: L O A D R E G;
LOADALL: L O A D A L L;
CLEAR: C L E A R;
REG: R E G;
REGS: R E G S;
SAVED: S A V E D;
PAUSE: P A U S E;
RESUME: R E S U M E;
STATIC: S T A T I C;
LOG: L O G;
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
ARITHMETIC: AND | OR | NOT | XOR | NONE;

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

CONST: C O N S T;

MASTER: M A S T E R;

REGISTER: [Rr][0-9a-fA-F];
SREGISTER: [Ss][Rr][0-9a-fA-F]+;
HEX_NUMBER: '0' X [0-9a-fA-F]+;
WS: [ \t\r\n]+ -> skip;
DOT: '.';
COLON: ':';
VARIABLE_NAME: [A-Za-z][A-Za-z0-9]*;
DecimalLiteral  : IntegerLiteral '.' [0-9]+ ;
IntegerLiteral  : '0' | '1'..'9' '0'..'9'* ;
VARIABLE_TYPE: DOT ((U | S) ('8' | '16' | '32' | '64') | F ('32' | '64'));

ID:  '{'[0-9A-Fa-f]+ '}' ;

CHEAT_NAME: '"' [0-9A-Za-z \\.()_]+ '"';

COMMENT_LINE: '#'  ~[\n\r]* -> skip;

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