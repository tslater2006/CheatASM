.title {1234}
.build {1234}

floatTest: .f32 4.83
mainOffset: .u32 const 0x1234
coinOffset: .u32 const 0x12
ten: .u32 0xA

.cheat master "Setup"
mov.d [R0 + 0x123], floatTest
mov.q R0, [MAIN + mainOffset]

# .cheat "Always 10 coins"
# mov.d [R0 + coinOffset], ten