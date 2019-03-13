using System;
using System.Collections.Generic;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public enum CheatOpcodeType : uint
    {
        StoreStatic = 0,
        BeginConditionalBlock = 1,
        EndConditionalBlock = 2,
        ControlLoop = 3,
        LoadRegisterStatic = 4,
        LoadRegisterMemory = 5,
        StoreStaticToAddress = 6,
        PerformArithmeticStatic = 7,
        BeginKeypressConditionalBlock = 8,
        PerformArithmeticRegister = 9,
        StoreRegisterToAddress = 10,
        ExtendedWidth = 12,
    }

    public enum BitWidthType : uint
    {
        b = 1,
        w = 2,
        d = 4,
        q = 8
    }
    public enum MemoryAccessType : uint
    {
        MAIN = 0,
        HEAP = 1,
    };

    public enum ConditionalComparisonType : uint
    {
        gt = 1,
        ge = 2,
        lt = 3,
        le = 4,
        eq = 5,
        ne = 6,
    };

    public enum RegisterArithmeticType : uint
    {
        add = 0,
        sub = 1,
        mul = 2,
        lsh = 3,
        rsh = 4,
        and = 5,
        or = 6,
        not = 7,
        xor = 8,

        none = 9,
    };

    public enum StoreRegisterOffsetType : uint
    {
        None = 0,
        Reg = 1,
        Imm = 2,
    };

    public enum KeyMask: uint
    {
        A = 0x1,
        B = 0x2,
        X = 0x4,
        Y = 0x8,
        LSP = 0x10,
        RSP = 0x20,
        L = 0x40,
        R = 0x80,
        ZL = 0x100,
        ZR = 0x200,
        PLUS = 0x400,
        MINUS = 0x800,
        LEFT = 0x1000,
        UP = 0x2000,
        RIGHT = 0x4000,
        DOWN = 0x8000,
        LSL = 0x10000,
        LSU = 0x20000,
        LSR = 0x40000,
        LSD = 0x80000,
        RSL = 0x100000,
        RSU = 0x200000,
        RSR = 0x400000,
        RSD = 0x800000,
        SL = 0x1000000,
        SR = 0x2000000
    }

    public abstract class CheatOpcode
    {
        public static CheatOpcode GetOpcodeByNumber(int num)
        {
            switch(num)
            {
                case 0:
                    return new StoreStaticOpcode();
                case 1:
                    return new ConditionalOpcode();
                case 2:
                    return new EndConditionalOpcode();
                case 3:
                    return new LoopOpcode();
                case 4:
                    return new LoadRegisterStaticOpcode();
                case 5:
                    return new LoadRegisterMemoryOpcode();
                case 6:
                    return new StoreStaticToAddressOpcode();
                case 7:
                    return new LegacyArithmeticOpcode();
                case 8:
                    return new KeypressConditionalOpcode();
                case 9:
                    return new ArithmeticOpcode();
                case 0xA:
                    return new StoreRegisterToAddressOpcode();
            }
            return null;
        }
        protected static uint GetNibble(UInt32 block, uint index)
        {
            return (block >> (int)(32 - (index * 4))) & 0xF;
        }

        protected static void SetNibble(ref uint block, uint index, uint value)
        {
            uint byteMask = (uint)0xFFFFFFFF - (uint)(0xF << (int)(32 - (index * 4)));
            block = block & byteMask;

            value = value & 0xF;
            value = value << (int)(32 - (index * 4));
            block = block | value;
        }

        public abstract string ToASM();
        public abstract string ToByteString();
    }
}
