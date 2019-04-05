.title {1234}
.build {1234}

.cheat master "Setup"
mov.q R0, [MAIN + 0x1234]

.cheat "Always 10 coins"
mov.d R1, 0xA
mov.d [R0 + 0x1234], R1