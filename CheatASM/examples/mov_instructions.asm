.title {1234}
.build {1234}

reg: .u32 0x0
num: .u32 const 0x1234

.cheat "Testing"
mov.d [reg], reg
mov.d [reg + reg], reg
mov.d [reg + num], reg
mov.d [HEAP + reg], reg
mov.d [HEAP + num], reg
mov.d [HEAP + reg + num], reg
mov.d [reg], num
mov.d [reg + reg], num
mov.d [HEAP + reg + num], num 

mov.d [R0], R1
mov.d [R0 + R1], R2
mov.d [R0 + 0x1], R2
mov.d [HEAP + R0], R1
mov.d [HEAP + 0x1], R0
mov.d [HEAP + R0 + 0x1], R2
mov.d [R0], 0x1
mov.d [R0 + R1], 0x1
mov.d [HEAP + R0 + 0x1], 0x2

