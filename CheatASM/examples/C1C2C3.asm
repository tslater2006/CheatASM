someVar: .u32 0x0

# 0xC1 Tests
# save.reg 0x3, R1
# load.reg R1, 0x1
# clear.reg R1
# clear.saved 0x1

# C2 Tests
save.regs R1, someVar, R3
load.regs R1, someVar, R3
clear.regs R1, someVar, R3
clear.saved 0x1, 0x2, 0x3

# 0xC3 Read/Write Static opcode 
# save.static SR1, R2
# load.static R2, SR1

# 0xFFF Debug Log opcode 
# log.d 0x3, [MAIN + R3]
# log.d 0x3, [HEAP + 0x4]
# log.b 0x1, R4
# log.d 0x2, [R4 + 0x123]