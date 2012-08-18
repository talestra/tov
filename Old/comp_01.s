.text:820D5570
.text:820D5570 # =============== S U B R O U T I N E =======================================
.text:820D5570
.text:820D5570 # r3 = %r28
.text:820D5570 # r4 = %r31
.text:820D5570 # r5 = buf_C_len
.text:820D5570
.text:820D5570 compression_loader:                     # CODE XREF: load_menu_map+94p
.text:820D5570                                         # sub_821C7AA0+80p ...
.text:820D5570
.text:820D5570 .set var_1060, -0x1060
.text:820D5570 .set var_105C, -0x105C
.text:820D5570 .set var_1050, -0x1050
.text:820D5570 .set var_1000, -0x1000

.text:820D5570                 mflr    %r12            # Move from link register
.text:820D5574                 bl      __savegprlr_27  # Branch
.text:820D5578                 ld      %r12, var_1000(%sp) # Load Double Word
.text:820D557C                 stwu    %sp, -0x10B0(%sp) # Store Word with Update
.text:820D5580                 mr      %r31, %r4  # Move Register
.text:820D5584                 lis     %r11, 0xFF5 # 0xFF512EE # Load Immediate Shifted
.text:820D5588                 mr      %r28, %r3  # Move Register
.text:820D558C                 ori     %r9, %r11, 0x12EE # 0xFF512EE # 0xFF512EE
.text:820D5590                 lwz     %r10, 0(%r31) # Load Word and Zero
.text:820D5594                 cmplw   cr6, %r10, %r9  # Check new magic: 0x0FF512EE
.text:820D5598                 bne     cr6, compression_old # Branch if not equal
.text:820D559C                 li      %r27, 0         # Load Immediate
.text:820D55A0                 lwz     %r5, 8(%r31) # *(uint *)(%r31 + 8)
.text:820D55A4                 addi    %r6, %sp, 0x10B0+var_105C # Add Immediate
.text:820D55A8                 stw     %r27, 0x10B0+var_105C(%sp) # Store Word
.text:820D55AC                 addi    %r4, %r31, 0xC # Add Immediate
.text:820D55B0                 li      %r3, 1          # Load Immediate
.text:820D55B4                 bl      compression_FFnew_begin # r3 = 1
.text:820D55B4                                         # r4 = %r31 + 0xC
.text:820D55B4                                         # r5 = *(uint *)(%r31 + 8)
.text:820D55B4                                         # r6 = buf_unknown
.text:820D55B4                                         #
.text:820D55B4                                         # error_code = 0x8007000E
.text:820D55B8                 cmpwi   cr6, %r3, 0     # Compare Word Immediate
.text:820D55BC                 bge     cr6, success1   # Get size
.text:820D55C0                 bl      j_DbgBreakPoint # E_OUTOFMEMORY
.text:820D55C0                                         # error_code = 0x8007000E
.text:820D55C4
.text:820D55C4 success1:                               # CODE XREF: compression_loader+4Cj
.text:820D55C4                 lwz     %r30, 0x1C(%r31) # Get size
.text:820D55C8                 addi    %r29, %r31, 0x30 # Add Immediate
.text:820D55CC                 stw     %r27, 0x10B0+var_1060(%sp) # Store Word
.text:820D55D0                 cmplwi  cr6, %r30, 0    # Compare Logical Word Immediate
.text:820D55D4                 beq     cr6, loc_820D562C # Branch if equal
.text:820D55D8
.text:820D55D8 loc_820D55D8:                           # CODE XREF: compression_loader+B8j
.text:820D55D8                 lwz     %r11, 0x28(%r31) # Load Word and Zero
.text:820D55DC                 stw     %r30, 0x10B0+var_1060(%sp) # Store Word
.text:820D55E0                 cmplw   cr6, %r30, %r11 # Compare Logical Word
.text:820D55E4                 blt     cr6, decompress_chunk_label # Branch if less than
.text:820D55E8                 stw     %r11, 0x10B0+var_1060(%sp) # Store Word
.text:820D55EC
.text:820D55EC decompress_chunk_label:                       # CODE XREF: compression_loader+74j
.text:820D55EC                 addi    %r6, %r29, 4 # Add Immediate
.text:820D55F0                 lwz     %r7, 0(%r29) # Load Word and Zero
.text:820D55F4                 addi    %r5, %sp, 0x10B0+var_1060 # Add Immediate
.text:820D55F8                 lwz     %r3, 0x10B0+var_105C(%sp) # Load Word and Zero
.text:820D55FC                 mr      %r4, %r28  # Move Register
.text:820D5600                 bl      compression_FFnew_inner # r3 = buf_unknown
.text:820D5600                                         # r4 = %r28
.text:820D5600                                         # r5 = buf_unknown2
.text:820D5600                                         # r6 = %r31
.text:820D5600                                         # r7 = buf_C_len
.text:820D5600                                         #
.text:820D5600                                         # error_codes =
.text:820D5600                                         #     0x80004005 E_FAIL
.text:820D5600                                         #     0x81DE2001 E_XMCDERR_MOREDATA
.text:820D5600                                         #     0x8007000E E_OUTOFMEMORY
.text:820D5604                 cmpwi   cr6, %r3, 0     # Compare Word Immediate
.text:820D5608                 bge     cr6, loc_820D5610 # Branch if greater than or equal
.text:820D560C                 bl      j_DbgBreakPoint # E_ERROR_FAILURE
.text:820D560C                                         # error_code = 0x80004005
.text:820D5610
.text:820D5610 loc_820D5610:                           # CODE XREF: compression_loader+98j
.text:820D5610                 lwz     %r11, 0x10B0+var_1060(%sp) # Load Word and Zero
.text:820D5614                 lwz     %r10, 0(%r29) # Load Word and Zero
.text:820D5618                 subf.   %r30, %r11, %r30 # Subtract from
.text:820D561C                 add     %r28, %r11, %r28 # Add
.text:820D5620                 add     %r11, %r10, %r29 # Add
.text:820D5624                 addi    %r29, %r11, 4 # Add Immediate
.text:820D5628                 bne     loc_820D55D8    # Branch if not equal
.text:820D562C
.text:820D562C loc_820D562C:                           # CODE XREF: compression_loader+64j
.text:820D562C                 lwz     %r3, 0x10B0+var_105C(%sp) # Load Word and Zero
.text:820D5630                 bl      compression_FFnew_cleanup # Branch
.text:820D5634                 li      %r3, 0          # Load Immediate
.text:820D5638                 addi    %sp, %sp, 0x10B0 # Add Immediate
.text:820D563C                 b       __restgprlr_27  # Branch
.text:820D5640 # ---------------------------------------------------------------------------
.text:820D5640
.text:820D5640 compression_old:                        # CODE XREF: compression_loader+28j
.text:820D5640                 lbz     %r11, 4(%r31) # Load Byte and Zero
.text:820D5644                 lbz     %r10, 3(%r31) # Load Byte and Zero
.text:820D5648                 rotlwi  %r11, %r11, 8   # Rotate Left Immediate
.text:820D564C                 lbz     %r8, 2(%r31) # Load Byte and Zero
.text:820D5650                 lbz     %r9, 1(%r31) # Load Byte and Zero
.text:820D5654                 add     %r10, %r11, %r10 # Add
.text:820D5658                 lbz     %r11, 0(%r31) # Load Byte and Zero
.text:820D565C                 slwi    %r10, %r10, 8   # Shift Left Immediate
.text:820D5660                 cmplwi  cr6, %r11, 1    # Compare Logical Word Immediate
.text:820D5664                 add     %r8, %r10, %r8  # Add
.text:820D5668                 slwi    %r10, %r8, 8    # Shift Left Immediate
.text:820D566C                 add     %r5, %r10, %r9  # r5 = length
.text:820D5670                 blt     cr6, compression_00_label # Branch if less than
.text:820D5674                 beq     cr6, compression_01_label # Branch if equal
.text:820D5678                 cmplwi  cr6, %r11, 3    # Compare Logical Word Immediate
.text:820D567C                 beq     cr6, compression_03_label # Branch if equal
.text:820D5680                 li      %r3, -1         # Load Immediate
.text:820D5684                 addi    %sp, %sp, 0x10B0 # Add Immediate
.text:820D5688                 b       __restgprlr_27  # Branch
.text:820D568C # ---------------------------------------------------------------------------
.text:820D568C
.text:820D568C compression_03_label:                   # CODE XREF: compression_loader+10Cj
.text:820D568C                 add     %r11, %r5, %r31 # Add
.text:820D5690                 addi    %r6, %sp, 0x10B0+var_1050 # slidding
.text:820D5694                 addi    %r5, %r11, 9    # buf_C_end
.text:820D5698                 addi    %r4, %r31, 9 # buf_C_start
.text:820D569C                 mr      %r3, %r28  # %r28
.text:820D56A0                 bl      compression_03_func # r3 = %r28
.text:820D56A0                                         # r4 = buf_C_start
.text:820D56A0                                         # r5 = buf_C_end
.text:820D56A0                                         # r6 = slidding_ptr
.text:820D56A4                 li      %r3, 0          # Load Immediate
.text:820D56A8                 addi    %sp, %sp, 0x10B0 # Add Immediate
.text:820D56AC                 b       __restgprlr_27  # Branch
.text:820D56B0 # ---------------------------------------------------------------------------
.text:820D56B0
.text:820D56B0 compression_01_label:                   # CODE XREF: compression_loader+104j
.text:820D56B0                 add     %r11, %r5, %r31 # Add
.text:820D56B4                 addi    %r6, %sp, 0x10B0+var_1050 # slidding pointer
.text:820D56B8                 addi    %r5, %r11, 9    # buf_C_end
.text:820D56BC                 addi    %r4, %r31, 9 # buf_C_start
.text:820D56C0                 mr      %r3, %r28  # %r28
.text:820D56C4                 bl      compression_01_func # r3 = %r28
.text:820D56C4                                         # r4 = buf_C_start
.text:820D56C4                                         # r5 = buf_C_end
.text:820D56C4                                         # r6 = slidding_ptr
.text:820D56C8                 li      %r3, 0          # Load Immediate
.text:820D56CC                 addi    %sp, %sp, 0x10B0 # Add Immediate
.text:820D56D0                 b       __restgprlr_27  # Branch
.text:820D56D4 # ---------------------------------------------------------------------------
.text:820D56D4
.text:820D56D4 compression_00_label:                   # CODE XREF: compression_loader+100j
.text:820D56D4                 addi    %r4, %r31, 9 # Add Immediate
.text:820D56D8                 mr      %r3, %r28  # Move Register
.text:820D56DC                 bl      memcpy          # r3 = dst_ptr
.text:820D56DC                                         # r4 = src_ptr
.text:820D56DC                                         # r5 = length
.text:820D56E0                 li      %r3, 0          # Load Immediate
.text:820D56E4                 addi    %sp, %sp, 0x10B0 # Add Immediate
.text:820D56E8                 b       __restgprlr_27  # Branch
.text:820D56E8 # End of function compression_loader
.text:820D56E8
.text:820D56E8 # ---------------------------------------------------------------------------


.text:820D51C0
.text:820D51C0 # =============== S U B R O U T I N E =======================================
.text:820D51C0
.text:820D51C0 # r3 = %r28
.text:820D51C0 # r4 = buf_C_start
.text:820D51C0 # r5 = buf_C_end
.text:820D51C0 # r6 = slidding_ptr
.text:820D51C0 # Attributes: library function static
.text:820D51C0
.text:820D51C0 compression_01_func:                    # CODE XREF: compression_loader+154p
.text:820D51C0                 mflr    %r12
.text:820D51C4                 bl      __savegprlr_29
.text:820D51C8                 li      %r7, 0
.text:820D51CC                 mr      %r11, %r6
.text:820D51D0                 mr      %r9, %r7
.text:820D51D4                 li      %r10, 0xFEE
.text:820D51D8                 mtctr   %r10
.text:820D51DC
.text:820D51DC loc_820D51DC:                           # CODE XREF: compression_01_func+24j
.text:820D51DC                 stb     %r9, 0(%r11)
.text:820D51E0                 addi    %r11, %r11, 1
.text:820D51E4                 bdnz    loc_820D51DC
.text:820D51E8                 mr      %r10, %r7
.text:820D51EC                 mr      %r11, %r7
.text:820D51F0
.text:820D51F0 loc_820D51F0:                           # CODE XREF: compression_01_func+7Cj
.text:820D51F0                 clrlwi  %r9, %r11, 24
.text:820D51F4                 addi    %r11, %r11, 1
.text:820D51F8                 stbx    %r9, %r10, %r6
.text:820D51FC                 addi    %r10, %r10, 1
.text:820D5200                 cmplwi  cr6, %r11, 0x100
.text:820D5204                 stbx    %r7, %r10, %r6
.text:820D5208                 addi    %r10, %r10, 1
.text:820D520C                 stbx    %r9, %r10, %r6
.text:820D5210                 addi    %r10, %r10, 1
.text:820D5214                 stbx    %r7, %r10, %r6
.text:820D5218                 addi    %r10, %r10, 1
.text:820D521C                 stbx    %r9, %r10, %r6
.text:820D5220                 addi    %r10, %r10, 1
.text:820D5224                 stbx    %r7, %r10, %r6
.text:820D5228                 addi    %r10, %r10, 1
.text:820D522C                 stbx    %r9, %r10, %r6
.text:820D5230                 addi    %r10, %r10, 1
.text:820D5234                 stbx    %r7, %r10, %r6
.text:820D5238                 addi    %r10, %r10, 1
.text:820D523C                 blt     cr6, loc_820D51F0
.text:820D5240                 mr      %r11, %r7
.text:820D5244                 add     %r10, %r10, %r6
.text:820D5248                 li      %r9, 0xFF
.text:820D524C
.text:820D524C loc_820D524C:                           # CODE XREF: compression_01_func+D0j
.text:820D524C                 clrlwi  %r8, %r11, 24
.text:820D5250                 addi    %r11, %r11, 1
.text:820D5254                 stb     %r8, 0(%r10)
.text:820D5258                 addi    %r10, %r10, 1
.text:820D525C                 cmplwi  cr6, %r11, 0x100
.text:820D5260                 stb     %r9, 0(%r10)
.text:820D5264                 addi    %r10, %r10, 1
.text:820D5268                 stb     %r8, 0(%r10)
.text:820D526C                 addi    %r10, %r10, 1
.text:820D5270                 stb     %r9, 0(%r10)
.text:820D5274                 addi    %r10, %r10, 1
.text:820D5278                 stb     %r8, 0(%r10)
.text:820D527C                 addi    %r10, %r10, 1
.text:820D5280                 stb     %r9, 0(%r10)
.text:820D5284                 addi    %r10, %r10, 1
.text:820D5288                 stb     %r8, 0(%r10)
.text:820D528C                 addi    %r10, %r10, 1
.text:820D5290                 blt     cr6, loc_820D524C
.text:820D5294                 li      %r10, 0xFEE
.text:820D5298                 mr      %r31, %r7
.text:820D529C                 cmplw   cr6, %r4, %r5
.text:820D52A0                 bge     cr6, loc_820D5344
.text:820D52A4
.text:820D52A4 loc_820D52A4:                           # CODE XREF: compression_01_func+180j
.text:820D52A4                 srwi    %r31, %r31, 1
.text:820D52A8                 rlwinm  %r11, %r31, 0,23,23
.text:820D52AC                 cmplwi  cr6, %r11, 0
.text:820D52B0                 bne     cr6, loc_820D52C0
.text:820D52B4                 lbz     %r11, 0(%r4)
.text:820D52B8                 addi    %r4, %r4, 1
.text:820D52BC                 ori     %r31, %r11, -0x100
.text:820D52C0
.text:820D52C0 loc_820D52C0:                           # CODE XREF: compression_01_func+F0j
.text:820D52C0                 clrlwi  %r11, %r31, 31
.text:820D52C4                 cmplwi  cr6, %r11, 0
.text:820D52C8                 beq     cr6, loc_820D52EC
.text:820D52CC                 lbz     %r11, 0(%r4)
.text:820D52D0                 addi    %r9, %r10, 1
.text:820D52D4                 addi    %r4, %r4, 1
.text:820D52D8                 stbx    %r11, %r10, %r6
.text:820D52DC                 clrlwi  %r10, %r9, 20
.text:820D52E0                 stb     %r11, 0(%r3)
.text:820D52E4                 addi    %r3, %r3, 1
.text:820D52E8                 b       loc_820D533C
.text:820D52EC # ---------------------------------------------------------------------------
.text:820D52EC
.text:820D52EC loc_820D52EC:                           # CODE XREF: compression_01_func+108j
.text:820D52EC                 lbz     %r8, 1(%r4)
.text:820D52F0                 addi    %r9, %r4, 1
.text:820D52F4                 lbz     %r30, 0(%r4)
.text:820D52F8                 mr      %r11, %r7
.text:820D52FC                 rlwinm  %r29, %r8, 4,20,23
.text:820D5300                 clrlwi  %r8, %r8, 28
.text:820D5304                 addi    %r4, %r9, 1
.text:820D5308                 or      %r9, %r29, %r30
.text:820D530C                 addi    %r8, %r8, 2
.text:820D5310
.text:820D5310 loc_820D5310:                           # CODE XREF: compression_01_func+178j
.text:820D5310                 add     %r30, %r9, %r11
.text:820D5314                 addi    %r29, %r10, 1
.text:820D5318                 clrlwi  %r30, %r30, 20
.text:820D531C                 addi    %r11, %r11, 1
.text:820D5320                 cmplw   cr6, %r11, %r8
.text:820D5324                 lbzx    %r30, %r30, %r6
.text:820D5328                 stbx    %r30, %r10, %r6
.text:820D532C                 clrlwi  %r10, %r29, 20
.text:820D5330                 stb     %r30, 0(%r3)
.text:820D5334                 addi    %r3, %r3, 1
.text:820D5338                 ble     cr6, loc_820D5310
.text:820D533C
.text:820D533C loc_820D533C:                           # CODE XREF: compression_01_func+128j
.text:820D533C                 cmplw   cr6, %r4, %r5
.text:820D5340                 blt     cr6, loc_820D52A4
.text:820D5344
.text:820D5344 loc_820D5344:                           # CODE XREF: compression_01_func+E0j
.text:820D5344                 b       __restgprlr_29
.text:820D5344 # End of function compression_01_func
.text:820D5344

.text:823AAF30 # =============== S U B R O U T I N E =======================================
.text:823AAF30
.text:823AAF30
.text:823AAF30 __savegprlr_14:                         # CODE XREF: sub_820B1EB0+4p
.text:823AAF30                                         # sub_820B2410+4p ...
.text:823AAF30
.text:823AAF30 .set var_98, -0x98
.text:823AAF30
.text:823AAF30                 std     %r14, var_98(%sp) # Store Double Word
.text:823AAF30 # End of function __savegprlr_14
.text:823AAF30
.text:823AAF34
.text:823AAF34 # =============== S U B R O U T I N E =======================================
.text:823AAF34
.text:823AAF34
.text:823AAF34 __savegprlr_15:                         # CODE XREF: .text:820C48FCp
.text:823AAF34                                         # sub_820C4EB8+4p ...
.text:823AAF34
.text:823AAF34 .set var_90, -0x90
.text:823AAF34
.text:823AAF34                 std     %r15, var_90(%sp) # Store Double Word
.text:823AAF34 # End of function __savegprlr_15
.text:823AAF34
.text:823AAF38
.text:823AAF38 # =============== S U B R O U T I N E =======================================
.text:823AAF38
.text:823AAF38
.text:823AAF38 __savegprlr_16:                         # CODE XREF: sub_820B29F0+4p
.text:823AAF38                                         # sub_820CF440+4p ...
.text:823AAF38
.text:823AAF38 .set var_88, -0x88
.text:823AAF38
.text:823AAF38                 std     %r16, var_88(%sp) # Store Double Word
.text:823AAF38 # End of function __savegprlr_16
.text:823AAF38
.text:823AAF3C
.text:823AAF3C # =============== S U B R O U T I N E =======================================
.text:823AAF3C
.text:823AAF3C
.text:823AAF3C __savegprlr_17:                         # CODE XREF: .text:820BE7DCp
.text:823AAF3C                                         # sub_820C6C50+4p ...
.text:823AAF3C
.text:823AAF3C .set var_80, -0x80
.text:823AAF3C
.text:823AAF3C                 std     %r17, var_80(%sp) # Store Double Word
.text:823AAF3C # End of function __savegprlr_17
.text:823AAF3C
.text:823AAF40
.text:823AAF40 # =============== S U B R O U T I N E =======================================
.text:823AAF40
.text:823AAF40
.text:823AAF40 __savegprlr_18:                         # CODE XREF: sub_820C3920+4p
.text:823AAF40                                         # sub_820ED148+4p ...
.text:823AAF40
.text:823AAF40 .set var_78, -0x78
.text:823AAF40
.text:823AAF40                 std     %r18, var_78(%sp) # Store Double Word
.text:823AAF40 # End of function __savegprlr_18
.text:823AAF40
.text:823AAF44
.text:823AAF44 # =============== S U B R O U T I N E =======================================
.text:823AAF44
.text:823AAF44
.text:823AAF44 __savegprlr_19:                         # CODE XREF: sub_820B8360+4p
.text:823AAF44                                         # sub_820C8308+4p ...
.text:823AAF44
.text:823AAF44 .set var_70, -0x70
.text:823AAF44
.text:823AAF44                 std     %r19, var_70(%sp) # Store Double Word
.text:823AAF44 # End of function __savegprlr_19
.text:823AAF44
.text:823AAF48
.text:823AAF48 # =============== S U B R O U T I N E =======================================
.text:823AAF48
.text:823AAF48
.text:823AAF48 __savegprlr_20:                         # CODE XREF: sub_820B2D08+4p
.text:823AAF48                                         # sub_820B3250+4p ...
.text:823AAF48
.text:823AAF48 .set var_68, -0x68
.text:823AAF48
.text:823AAF48                 std     %r20, var_68(%sp) # Store Double Word
.text:823AAF48 # End of function __savegprlr_20
.text:823AAF48
.text:823AAF4C
.text:823AAF4C # =============== S U B R O U T I N E =======================================
.text:823AAF4C
.text:823AAF4C
.text:823AAF4C __savegprlr_21:                         # CODE XREF: sub_820B1390+4p
.text:823AAF4C                                         # .text:820B7934p ...
.text:823AAF4C
.text:823AAF4C .set var_60, -0x60
.text:823AAF4C
.text:823AAF4C                 std     %r21, var_60(%sp) # Store Double Word
.text:823AAF4C # End of function __savegprlr_21
.text:823AAF4C
.text:823AAF50
.text:823AAF50 # =============== S U B R O U T I N E =======================================
.text:823AAF50
.text:823AAF50
.text:823AAF50 __savegprlr_22:                         # CODE XREF: sub_820B2FC8+4p
.text:823AAF50                                         # sub_820B3F78+4p ...
.text:823AAF50
.text:823AAF50 .set var_58, -0x58
.text:823AAF50
.text:823AAF50                 std     %r22, var_58(%sp) # Store Double Word
.text:823AAF50 # End of function __savegprlr_22
.text:823AAF50
.text:823AAF54
.text:823AAF54 # =============== S U B R O U T I N E =======================================
.text:823AAF54
.text:823AAF54
.text:823AAF54 __savegprlr_23:                         # CODE XREF: sub_820B0AA8+4p
.text:823AAF54                                         # .text:820B0C54p ...
.text:823AAF54
.text:823AAF54 .set var_50, -0x50
.text:823AAF54
.text:823AAF54                 std     %r23, var_50(%sp) # Store Double Word
.text:823AAF54 # End of function __savegprlr_23
.text:823AAF54
.text:823AAF58
.text:823AAF58 # =============== S U B R O U T I N E =======================================
.text:823AAF58
.text:823AAF58
.text:823AAF58 __savegprlr_24:                         # CODE XREF: sub_820B1790+4p
.text:823AAF58                                         # sub_820B22B8+4p ...
.text:823AAF58
.text:823AAF58 .set var_48, -0x48
.text:823AAF58
.text:823AAF58                 std     %r24, var_48(%sp) # Store Double Word
.text:823AAF58 # End of function __savegprlr_24
.text:823AAF58
.text:823AAF5C
.text:823AAF5C # =============== S U B R O U T I N E =======================================
.text:823AAF5C
.text:823AAF5C
.text:823AAF5C __savegprlr_25:                         # CODE XREF: sub_820B2888+4p
.text:823AAF5C                                         # sub_820B84C8+4p ...
.text:823AAF5C
.text:823AAF5C .set var_40, -0x40
.text:823AAF5C
.text:823AAF5C                 std     %r25, var_40(%sp) # Store Double Word
.text:823AAF5C # End of function __savegprlr_25
.text:823AAF5C
.text:823AAF60
.text:823AAF60 # =============== S U B R O U T I N E =======================================
.text:823AAF60
.text:823AAF60
.text:823AAF60 __savegprlr_26:                         # CODE XREF: sub_820B09B8+4p
.text:823AAF60                                         # sub_820B30E8+4p ...
.text:823AAF60
.text:823AAF60 .set var_38, -0x38
.text:823AAF60
.text:823AAF60                 std     %r26, var_38(%sp) # Store Double Word
.text:823AAF60 # End of function __savegprlr_26
.text:823AAF60
.text:823AAF64
.text:823AAF64 # =============== S U B R O U T I N E =======================================
.text:823AAF64
.text:823AAF64
.text:823AAF64 __savegprlr_27:                         # CODE XREF: sub_820B6700+4p
.text:823AAF64                                         # sub_820B6850+4p ...
.text:823AAF64
.text:823AAF64 .set var_30, -0x30
.text:823AAF64
.text:823AAF64                 std     %r27, var_30(%sp) # Store Double Word
.text:823AAF64 # End of function __savegprlr_27
.text:823AAF64
.text:823AAF68
.text:823AAF68 # =============== S U B R O U T I N E =======================================
.text:823AAF68
.text:823AAF68
.text:823AAF68 __savegprlr_28:                         # CODE XREF: sub_820B0000+4p
.text:823AAF68                                         # sub_820B0580+4p ...
.text:823AAF68
.text:823AAF68 .set var_28, -0x28
.text:823AAF68
.text:823AAF68                 std     %r28, var_28(%sp) # Store Double Word
.text:823AAF68 # End of function __savegprlr_28
.text:823AAF68
.text:823AAF6C
.text:823AAF6C # =============== S U B R O U T I N E =======================================
.text:823AAF6C
.text:823AAF6C
.text:823AAF6C __savegprlr_29:                         # CODE XREF: sub_820B0048+4p
.text:823AAF6C                                         # sub_820B02C8+4p ...
.text:823AAF6C
.text:823AAF6C .set var_20, -0x20
.text:823AAF6C
.text:823AAF6C                 std     %r29, var_20(%sp) # Store Double Word
.text:823AAF6C # End of function __savegprlr_29
.text:823AAF6C
.text:823AAF70
.text:823AAF70 # =============== S U B R O U T I N E =======================================
.text:823AAF70
.text:823AAF70
.text:823AAF70 __savegprlr_30:
.text:823AAF70
.text:823AAF70 .set var_18, -0x18
.text:823AAF70
.text:823AAF70                 std     %r30, var_18(%sp) # Store Double Word
.text:823AAF70 # End of function __savegprlr_30
.text:823AAF70
.text:823AAF74
.text:823AAF74 # =============== S U B R O U T I N E =======================================
.text:823AAF74
.text:823AAF74
.text:823AAF74 __savegprlr_31:
.text:823AAF74
.text:823AAF74 .set var_10, -0x10
.text:823AAF74 .set var_8, -8
.text:823AAF74
.text:823AAF74                 std     %r31, var_10(%sp) # Store Double Word
.text:823AAF78                 stw     %r12, var_8(%sp) # Store Word
.text:823AAF7C                 blr                     # Branch unconditionally
.text:823AAF7C # End of function __savegprlr_31
.text:823AAF7C
.text:823AAF80 # ---------------------------------------------------------------------------
.text:823AAF80 # START OF FUNCTION CHUNK FOR sub_823EA090
.text:823AAF80
.text:823AAF80 __restgprlr_14:                         # CODE XREF: sub_820B1EB0+36Cj
.text:823AAF80                                         # sub_820B2410+470j ...
.text:823AAF80                 ld      %r14, var_98(%sp) # Load Double Word
.text:823AAF84
.text:823AAF84 __restgprlr_15:                         # CODE XREF: .text:820C4D10j
.text:823AAF84                                         # sub_820C4EB8+6F4j ...
.text:823AAF84                 ld      %r15, var_90(%sp) # Load Double Word
.text:823AAF88
.text:823AAF88 __restgprlr_16:                         # CODE XREF: sub_820B29F0+314j
.text:823AAF88                                         # sub_820CF440+1298j ...
.text:823AAF88                 ld      %r16, var_88(%sp) # Load Double Word
.text:823AAF8C
.text:823AAF8C __restgprlr_17:                         # CODE XREF: .text:820BF5CCj
.text:823AAF8C                                         # sub_820C6C50+1D4j ...
.text:823AAF8C                 ld      %r17, var_80(%sp) # Load Double Word
.text:823AAF90
.text:823AAF90 __restgprlr_18:                         # CODE XREF: sub_820C3920+8A8j
.text:823AAF90                                         # sub_820ED148+690j ...
.text:823AAF90                 ld      %r18, var_78(%sp) # Load Double Word
.text:823AAF94
.text:823AAF94 __restgprlr_19:                         # CODE XREF: sub_820B8360+164j
.text:823AAF94                                         # sub_820C8308+165Cj ...
.text:823AAF94                 ld      %r19, var_70(%sp) # Load Double Word
.text:823AAF98
.text:823AAF98 __restgprlr_20:                         # CODE XREF: sub_820B2D08+270j
.text:823AAF98                                         # sub_820B3250+178j ...
.text:823AAF98                 ld      %r20, var_68(%sp) # Load Double Word
.text:823AAF98 # END OF FUNCTION CHUNK FOR sub_823EA090
.text:823AAF9C # START OF FUNCTION CHUNK FOR sub_82434A28
.text:823AAF9C
.text:823AAF9C __restgprlr_21:                         # CODE XREF: sub_820B1390:loc_820B157Cj
.text:823AAF9C                                         # .text:820B7A80j ...
.text:823AAF9C                 ld      %r21, var_60(%sp) # Load Double Word
.text:823AAFA0
.text:823AAFA0 __restgprlr_22:                         # CODE XREF: .text:820B30E4j
.text:823AAFA0                                         # sub_820B3F78+82Cj ...
.text:823AAFA0                 ld      %r22, var_58(%sp) # Load Double Word
.text:823AAFA0 # END OF FUNCTION CHUNK FOR sub_82434A28
.text:823AAFA4 # START OF FUNCTION CHUNK FOR sub_82222EB0
.text:823AAFA4
.text:823AAFA4 __restgprlr_23:                         # CODE XREF: sub_820B0AA8+1A4j
.text:823AAFA4                                         # .text:820B0E40j ...
.text:823AAFA4                 ld      %r23, var_50(%sp) # Load Double Word
.text:823AAFA8
.text:823AAFA8 __restgprlr_24:                         # CODE XREF: sub_820B1790+46Cj
.text:823AAFA8                                         # sub_820B22B8+118j ...
.text:823AAFA8                 ld      %r24, var_48(%sp) # Load Double Word
.text:823AAFAC
.text:823AAFAC __restgprlr_25:                         # CODE XREF: sub_820B2888+C8j
.text:823AAFAC                                         # sub_820B84C8+ACj ...
.text:823AAFAC                 ld      %r25, var_40(%sp) # Load Double Word
.text:823AAFAC # END OF FUNCTION CHUNK FOR sub_82222EB0
.text:823AAFB0 # START OF FUNCTION CHUNK FOR sub_82256CB8
.text:823AAFB0
.text:823AAFB0 __restgprlr_26:                         # CODE XREF: sub_820B09B8+ECj
.text:823AAFB0                                         # .text:820B3248j ...
.text:823AAFB0                 ld      %r26, var_38(%sp) # Load Double Word
.text:823AAFB4
.text:823AAFB4 __restgprlr_27:                         # CODE XREF: sub_820B6700+14Cj
.text:823AAFB4                                         # sub_820B6850+260j ...
.text:823AAFB4                 ld      %r27, var_30(%sp) # Load Double Word
.text:823AAFB4 # END OF FUNCTION CHUNK FOR sub_82256CB8
.text:823AAFB8 # START OF FUNCTION CHUNK FOR sub_82258330
.text:823AAFB8
.text:823AAFB8 __restgprlr_28:                         # CODE XREF: sub_820B0000+40j
.text:823AAFB8                                         # sub_820B0580+194j ...
.text:823AAFB8                 ld      %r28, var_28(%sp) # Load Double Word
.text:823AAFBC
.text:823AAFBC __restgprlr_29:                         # CODE XREF: sub_820B0048+64j
.text:823AAFBC                                         # sub_820B02C8+B0j ...
.text:823AAFBC                 ld      %r29, var_20(%sp) # Load Double Word
.text:823AAFC0
.text:823AAFC0 __restgprlr_30:                         # Load Double Word
.text:823AAFC0                 ld      %r30, var_18(%sp)
.text:823AAFC4
.text:823AAFC4 __restgprlr_31:                         # Load Double Word
.text:823AAFC4                 ld      %r31, var_10(%sp)
.text:823AAFC8                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823AAFCC                 mtlr    %r12            # Move to link register
.text:823AAFD0                 blr                     # Branch unconditionally

.text:820D5348
.text:820D5348 # =============== S U B R O U T I N E =======================================
.text:820D5348
.text:820D5348 # r3 = buf_U_ptr
.text:820D5348 # r4 = buf_C_start
.text:820D5348 # r5 = buf_C_end
.text:820D5348 # r6 = slidding_ptr
.text:820D5348 # Attributes: library function static
.text:820D5348
.text:820D5348 compression_03_func:                    # CODE XREF: compression_loader+130p
.text:820D5348                 mflr    %r12            # Move from link register
.text:820D534C                 bl      __savegprlr_29  # Branch
.text:820D5350                 li      %r7, 0          # Load Immediate
.text:820D5354                 mr      %r11, %r6       # Move Register
.text:820D5358                 mr      %r9, %r7        # Move Register
.text:820D535C                 li      %r10, 0xFEF     # Load Immediate
.text:820D5360                 mtctr   %r10            # Move to count register
.text:820D5364
.text:820D5364 loc_820D5364:                           # CODE XREF: compression_03_func+24j
.text:820D5364                 stb     %r9, 0(%r11)    # Store Byte
.text:820D5368                 addi    %r11, %r11, 1   # Add Immediate
.text:820D536C                 bdnz    loc_820D5364    # CTR--; branch if CTR non-zero
.text:820D5370                 mr      %r10, %r7       # Move Register
.text:820D5374                 mr      %r11, %r7       # Move Register
.text:820D5378
.text:820D5378 loc_820D5378:                           # CODE XREF: compression_03_func+7Cj
.text:820D5378                 clrlwi  %r9, %r11, 24   # Clear Left Immediate
.text:820D537C                 addi    %r11, %r11, 1   # Add Immediate
.text:820D5380                 stbx    %r9, %r10, %r6  # Store Byte Indexed
.text:820D5384                 addi    %r10, %r10, 1   # Add Immediate
.text:820D5388                 cmplwi  cr6, %r11, 0x100 # Compare Logical Word Immediate
.text:820D538C                 stbx    %r7, %r10, %r6  # Store Byte Indexed
.text:820D5390                 addi    %r10, %r10, 1   # Add Immediate
.text:820D5394                 stbx    %r9, %r10, %r6  # Store Byte Indexed
.text:820D5398                 addi    %r10, %r10, 1   # Add Immediate
.text:820D539C                 stbx    %r7, %r10, %r6  # Store Byte Indexed
.text:820D53A0                 addi    %r10, %r10, 1   # Add Immediate
.text:820D53A4                 stbx    %r9, %r10, %r6  # Store Byte Indexed
.text:820D53A8                 addi    %r10, %r10, 1   # Add Immediate
.text:820D53AC                 stbx    %r7, %r10, %r6  # Store Byte Indexed
.text:820D53B0                 addi    %r10, %r10, 1   # Add Immediate
.text:820D53B4                 stbx    %r9, %r10, %r6  # Store Byte Indexed
.text:820D53B8                 addi    %r10, %r10, 1   # Add Immediate
.text:820D53BC                 stbx    %r7, %r10, %r6  # Store Byte Indexed
.text:820D53C0                 addi    %r10, %r10, 1   # Add Immediate
.text:820D53C4                 blt     cr6, loc_820D5378 # Branch if less than
.text:820D53C8                 mr      %r11, %r7       # Move Register
.text:820D53CC                 add     %r10, %r10, %r6 # Add
.text:820D53D0                 li      %r9, 0xFF       # Load Immediate
.text:820D53D4
.text:820D53D4 loc_820D53D4:                           # CODE XREF: compression_03_func+D0j
.text:820D53D4                 clrlwi  %r8, %r11, 24   # Clear Left Immediate
.text:820D53D8                 addi    %r11, %r11, 1   # Add Immediate
.text:820D53DC                 stb     %r8, 0(%r10)    # Store Byte
.text:820D53E0                 addi    %r10, %r10, 1   # Add Immediate
.text:820D53E4                 cmplwi  cr6, %r11, 0x100 # Compare Logical Word Immediate
.text:820D53E8                 stb     %r9, 0(%r10)    # Store Byte
.text:820D53EC                 addi    %r10, %r10, 1   # Add Immediate
.text:820D53F0                 stb     %r8, 0(%r10)    # Store Byte
.text:820D53F4                 addi    %r10, %r10, 1   # Add Immediate
.text:820D53F8                 stb     %r9, 0(%r10)    # Store Byte
.text:820D53FC                 addi    %r10, %r10, 1   # Add Immediate
.text:820D5400                 stb     %r8, 0(%r10)    # Store Byte
.text:820D5404                 addi    %r10, %r10, 1   # Add Immediate
.text:820D5408                 stb     %r9, 0(%r10)    # Store Byte
.text:820D540C                 addi    %r10, %r10, 1   # Add Immediate
.text:820D5410                 stb     %r8, 0(%r10)    # Store Byte
.text:820D5414                 addi    %r10, %r10, 1   # Add Immediate
.text:820D5418                 blt     cr6, loc_820D53D4 # Branch if less than
.text:820D541C                 li      %r11, 0xFEF     # Load Immediate
.text:820D5420                 mr      %r31, %r7       # Move Register
.text:820D5424                 cmplw   cr6, %r4, %r5   # Compare Logical Word
.text:820D5428                 bge     cr6, loc_820D5520 # Branch if greater than or equal
.text:820D542C
.text:820D542C loc_820D542C:                           # CODE XREF: compression_03_func+1D4j
.text:820D542C                 srwi    %r31, %r31, 1   # Shift Right Immediate
.text:820D5430                 rlwinm  %r10, %r31, 0,23,23 # Rotate Left Word Immediate then AND with Mask
.text:820D5434                 cmplwi  cr6, %r10, 0    # Compare Logical Word Immediate
.text:820D5438                 bne     cr6, loc_820D5448 # Branch if not equal
.text:820D543C                 lbz     %r10, 0(%r4)    # Load Byte and Zero
.text:820D5440                 addi    %r4, %r4, 1     # Add Immediate
.text:820D5444                 ori     %r31, %r10, -0x100 # OR Immediate
.text:820D5448
.text:820D5448 loc_820D5448:                           # CODE XREF: compression_03_func+F0j
.text:820D5448                 clrlwi  %r10, %r31, 31  # Clear Left Immediate
.text:820D544C                 cmplwi  cr6, %r10, 0    # Compare Logical Word Immediate
.text:820D5450                 beq     cr6, loc_820D5474 # Branch if equal
.text:820D5454                 lbz     %r10, 0(%r4)    # Load Byte and Zero
.text:820D5458                 addi    %r9, %r11, 1    # Add Immediate
.text:820D545C                 addi    %r4, %r4, 1     # Add Immediate
.text:820D5460                 stbx    %r10, %r11, %r6 # Store Byte Indexed
.text:820D5464                 clrlwi  %r11, %r9, 20   # Clear Left Immediate
.text:820D5468                 stb     %r10, 0(%r3)    # Store Byte
.text:820D546C                 addi    %r3, %r3, 1     # Add Immediate
.text:820D5470                 b       loc_820D5518    # Branch
.text:820D5474 # ---------------------------------------------------------------------------
.text:820D5474
.text:820D5474 loc_820D5474:                           # CODE XREF: compression_03_func+108j
.text:820D5474                 addi    %r10, %r4, 1    # Add Immediate
.text:820D5478                 lbz     %r9, 1(%r4)     # Load Byte and Zero
.text:820D547C                 lbz     %r30, 0(%r4)    # Load Byte and Zero
.text:820D5480                 addi    %r4, %r10, 1    # Add Immediate
.text:820D5484                 clrlwi  %r10, %r9, 28   # Clear Left Immediate
.text:820D5488                 rlwinm  %r9, %r9, 4,20,23 # Rotate Left Word Immediate then AND with Mask
.text:820D548C                 addi    %r8, %r10, 2    # Add Immediate
.text:820D5490                 or      %r9, %r9, %r30  # OR
.text:820D5494                 cmplwi  cr6, %r8, 0x11  # Compare Logical Word Immediate
.text:820D5498                 bge     cr6, loc_820D54D0 # Branch if greater than or equal
.text:820D549C                 mr      %r10, %r7       # Move Register
.text:820D54A0
.text:820D54A0 loc_820D54A0:                           # CODE XREF: compression_03_func+180j
.text:820D54A0                 add     %r30, %r9, %r10 # Add
.text:820D54A4                 addi    %r29, %r11, 1   # Add Immediate
.text:820D54A8                 clrlwi  %r30, %r30, 20  # Clear Left Immediate
.text:820D54AC                 addi    %r10, %r10, 1   # Add Immediate
.text:820D54B0                 cmplw   cr6, %r10, %r8  # Compare Logical Word
.text:820D54B4                 lbzx    %r30, %r30, %r6 # Load Byte and Zero Indexed
.text:820D54B8                 stbx    %r30, %r11, %r6 # Store Byte Indexed
.text:820D54BC                 clrlwi  %r11, %r29, 20  # Clear Left Immediate
.text:820D54C0                 stb     %r30, 0(%r3)    # Store Byte
.text:820D54C4                 addi    %r3, %r3, 1     # Add Immediate
.text:820D54C8                 ble     cr6, loc_820D54A0 # Branch if less than or equal
.text:820D54CC                 b       loc_820D5518    # Branch
.text:820D54D0 # ---------------------------------------------------------------------------
.text:820D54D0
.text:820D54D0 loc_820D54D0:                           # CODE XREF: compression_03_func+150j
.text:820D54D0                 cmplwi  cr6, %r9, 0x100 # Compare Logical Word Immediate
.text:820D54D4                 bge     cr6, loc_820D54E8 # Branch if greater than or equal
.text:820D54D8                 lbz     %r10, 0(%r4)    # Load Byte and Zero
.text:820D54DC                 addi    %r4, %r4, 1     # Add Immediate
.text:820D54E0                 addi    %r8, %r9, 0x12  # Add Immediate
.text:820D54E4                 b       loc_820D54F4    # Branch
.text:820D54E8 # ---------------------------------------------------------------------------
.text:820D54E8
.text:820D54E8 loc_820D54E8:                           # CODE XREF: compression_03_func+18Cj
.text:820D54E8                 srwi    %r8, %r9, 8     # Shift Right Immediate
.text:820D54EC                 clrlwi  %r10, %r9, 24   # Clear Left Immediate
.text:820D54F0                 addi    %r8, %r8, 2     # Add Immediate
.text:820D54F4
.text:820D54F4 loc_820D54F4:                           # CODE XREF: compression_03_func+19Cj
.text:820D54F4                 clrlwi  %r9, %r10, 24   # Clear Left Immediate
.text:820D54F8                 addi    %r10, %r8, 1    # Add Immediate
.text:820D54FC
.text:820D54FC loc_820D54FC:                           # CODE XREF: compression_03_func+1CCj
.text:820D54FC                 addi    %r8, %r11, 1    # Add Immediate
.text:820D5500                 stbx    %r9, %r11, %r6  # Store Byte Indexed
.text:820D5504                 stb     %r9, 0(%r3)     # Store Byte
.text:820D5508                 addic.  %r10, %r10, -1  # Add Immediate Carrying
.text:820D550C                 addi    %r3, %r3, 1     # Add Immediate
.text:820D5510                 clrlwi  %r11, %r8, 20   # Clear Left Immediate
.text:820D5514                 bne     loc_820D54FC    # Branch if not equal
.text:820D5518
.text:820D5518 loc_820D5518:                           # CODE XREF: compression_03_func+128j
.text:820D5518                                         # compression_03_func+184j
.text:820D5518                 cmplw   cr6, %r4, %r5   # Compare Logical Word
.text:820D551C                 blt     cr6, loc_820D542C # Branch if less than
.text:820D5520
.text:820D5520 loc_820D5520:                           # CODE XREF: compression_03_func+E0j
.text:820D5520                 b       __restgprlr_29  # Branch
.text:820D5520 # End of function compression_03_func
.text:820D5520
.text:820D5520 # ---------------------------------------------------------------------------

.text:823C6DD0 # =============== S U B R O U T I N E =======================================
.text:823C6DD0
.text:823C6DD0 # r3 = 1
.text:823C6DD0 # r4 = buf_C_ptr + 0xC
.text:823C6DD0 # r5 = *(uint *)(buf_C_ptr + 8)
.text:823C6DD0 # r6 = buf_unknown
.text:823C6DD0 #
.text:823C6DD0 # error_code = 0x8007000E
.text:823C6DD0
.text:823C6DD0 compression_FFnew_begin:                # CODE XREF: compression_loader+44p
.text:823C6DD0 value_1 = %r31                          # 1
.text:823C6DD0 buf_data = %r29                         # buf_C_ptr + 0xC
.text:823C6DD0 buf_unk_value = %r28                    # *(uint *)(buf_C_ptr + 8)
.text:823C6DD0 buf_unknown = %r27                      # buf_unknown
.text:823C6DD0 __ptr_back__ = %r30                     # return cfunc_823C73F0()
.text:823C6DD0 ptr_realloc = %r5
.text:823C6DD0                 mflr    %r12            # Move from link register
.text:823C6DD4                 bl      __savegprlr_27  # Branch
.text:823C6DD8                 stwu    %sp, -0x80(%sp) # Store Word with Update
.text:823C6DDC                 mr      value_1, %r3    # Move Register
.text:823C6DE0                 mr      buf_data, %r4   # Move Register
.text:823C6DE4                 mr      buf_unk_value, ptr_realloc # Move Register
.text:823C6DE8                 mr      buf_unknown, %r6 # Move Register
.text:823C6DEC                 li      __ptr_back__, 0 # Load Immediate
.text:823C6DF0                 cmplwi  cr6, value_1, 2 # Compare Logical Word Immediate
.text:823C6DF4                 bge     cr6, object_create # 0x2483*4 = 0x920C
.text:823C6DF8                 mr      %r4, buf_unk_value # Move Register
.text:823C6DFC                 mr      %r3, buf_data   # Move Register
.text:823C6E00                 bl      compression_header_parse # r3 = ptr + 0xc
.text:823C6E00                                         # r4 = *(uint*)(ptr + 8)
.text:823C6E04                 mr      __ptr_back__, %r3 # Move Register
.text:823C6E08
.text:823C6E08 object_create:                          # CODE XREF: compression_FFnew_begin+24j
.text:823C6E08                 lis     %r4, 0x2483     # 0x2483*4 = 0x920C
.text:823C6E0C                 mr      %r3, __ptr_back__ # ptr
.text:823C6E10                 bl      __realloc       # r3 = ptr
.text:823C6E10                                         # r4 = len
.text:823C6E14                 mr.     ptr_realloc, %r3 # Move Register
.text:823C6E18                 bne     success_malloc  # Branch if not equal
.text:823C6E1C                 lis     %r3, -0x7FF9 # 0x8007000E # Load Immediate Shifted
.text:823C6E20                 ori     %r3, %r3, 0xE # 0x8007000E # OR Immediate
.text:823C6E24                 b       loc_823C6E5C       # Branch
.text:823C6E28 # ---------------------------------------------------------------------------
.text:823C6E28
.text:823C6E28 success_malloc:                         # CODE XREF: compression_FFnew_begin+48j
.text:823C6E28                 li      %r11, 0         # Load Immediate
.text:823C6E2C                 cmplwi  cr6, value_1, 2 # Compare Logical Word Immediate
.text:823C6E30                 bge     cr6, loc_823C6E48 # Branch if greater than or equal
.text:823C6E34                 mr      %r6, __ptr_back__ # Move Register
.text:823C6E38                 mr      %r4, buf_unk_value # Move Register
.text:823C6E3C                 mr      %r3, buf_data   # Move Register
.text:823C6E40                 bl      cfunc_initialize_slidding_and_others # r3 = buf_data (buf_C_ptr + 0xC)
.text:823C6E40                                         # r4 = buf_value (*(uint *)(buf_C_ptr + 8))
.text:823C6E40                                         # r5 = ptr_realloc
.text:823C6E40                                         # r6 = value_unknown
.text:823C6E44                 mr      %r11, %r3       # Move Register
.text:823C6E48
.text:823C6E48 loc_823C6E48:                           # CODE XREF: compression_FFnew_begin+60j
.text:823C6E48                 lwz     %r10, 8(%r11)   # Load Word and Zero
.text:823C6E4C                 li      %r3, 0          # Load Immediate
.text:823C6E50                 oris    %r10, %r10, 0x4000 # OR Immediate Shifted
.text:823C6E54                 stw     %r10, 8(%r11)   # Store Word
.text:823C6E58                 stw     %r11, 0(buf_unknown) # Store Word
.text:823C6E5C
.text:823C6E5C loc_823C6E5C:                              # CODE XREF: compression_FFnew_begin+54j
.text:823C6E5C                 addi    %sp, %sp, 0x80  # Add Immediate
.text:823C6E60                 b       __restgprlr_27  # Branch
.text:823C6E60 # End of function compression_FFnew_begin

.text:823C73F8 # ---------------------------------------------------------------------------
.text:823C73F8 # START OF FUNCTION CHUNK FOR compression_FFnew_inner
.text:823C73F8
.text:823C73F8 loc_823C73F8:                           # CODE XREF: compression_FFnew_inner+1Cj
.text:823C73F8                 mflr    %r12            # Move from link register
.text:823C73FC                 stw     %r12, var_8(%sp) # Store Word
.text:823C7400                 stwu    %sp, -0x60(%sp) # Store Word with Update
.text:823C7404                 cmplwi  cr6, %r7, 0     # Compare Logical Word Immediate
.text:823C7408                 bne     cr6, loc_823C7428 # Branch if not equal
.text:823C740C                 li      %r11, 0         # *r5 = 0
.text:823C7410                 stw     %r11, 0(%r5)    # Store Word
.text:823C7414
.text:823C7414 loc_823C7414:                           # CODE XREF: compression_FFnew_inner+6D8j
.text:823C7414                 li      %r3, 0          # Load Immediate
.text:823C7418
.text:823C7418 loc_823C7418:                              # CODE XREF: compression_FFnew_inner+6F0j
.text:823C7418                                         # compression_FFnew_inner+6FCj ...
.text:823C7418                 addi    %sp, %sp, 0x60  # Add Immediate
.text:823C741C                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823C7420                 mtlr    %r12            # Move to link register
.text:823C7424                 blr                     # Branch unconditionally
.text:823C7428 # ---------------------------------------------------------------------------
.text:823C7428
.text:823C7428 loc_823C7428:                           # CODE XREF: compression_FFnew_inner+6B0j
.text:823C7428                 bl      compression_FFnew_inner_real # r3 = ptr_state
.text:823C7428                                         # r4 = buf_U_ptr
.text:823C7428                                         # r5 = *buf_U_len
.text:823C7428                                         # r6 = buf_C_ptr
.text:823C7428                                         # r7 = buf_C_len
.text:823C7428                                         #
.text:823C742C                 cmplwi  cr6, %r3, 1     # 1 = OUTOFMEMORY
.text:823C7430                 blt     cr6, loc_823C7414 # Branch if less than
.text:823C7434                 beq     cr6, loc_823C7434_E_OUTOFMEMORY # loc_823C7434_E_OUTOFMEMORY = 0x8007000E
.text:823C7438                 cmplwi  cr6, %r3, 6     # 6 = MOREDATA
.text:823C743C                 beq     cr6, XMCDERR_MOREDATA # XMCDERR_MOREDATA = 0x81DE2001
.text:823C7440                 lis     %r3, 0x8000     # E_FAIL
.text:823C7444                 ori     %r3, %r3, 0x4005 # 0x80004005 # OR Immediate
.text:823C7448                 b       loc_823C7418       # Branch
.text:823C744C # ---------------------------------------------------------------------------
.text:823C744C
.text:823C744C XMCDERR_MOREDATA:                       # CODE XREF: compression_FFnew_inner+6E4j
.text:823C744C                 lis     %r3, 0x81DE     # XMCDERR_MOREDATA = 0x81DE2001
.text:823C7450                 ori     %r3, %r3, 0x2001 # 0x81DE2001 # OR Immediate
.text:823C7454                 b       loc_823C7418       # Branch
.text:823C7458 # ---------------------------------------------------------------------------
.text:823C7458
.text:823C7458 loc_823C7434_E_OUTOFMEMORY:                          # CODE XREF: compression_FFnew_inner+6DCj
.text:823C7458                 lis     %r3, 0x8007     # loc_823C7434_E_OUTOFMEMORY = 0x8007000E
.text:823C745C                 ori     %r3, %r3, 0xE # 0x8007000E # OR Immediate
.text:823C7460                 b       loc_823C7418       # Branch
.text:823C7460 # END OF FUNCTION CHUNK FOR compression_FFnew_inner
.text:823C7460 # ---------------------------------------------------------------------------




.text:823C7D50 # =============== S U B R O U T I N E =======================================
.text:823C7D50
.text:823C7D50 # r3 = ptr + 0xc
.text:823C7D50 # r4 = *(uint*)(ptr + 8)
.text:823C7D50
.text:823C7D50 compression_header_parse_func:          # CODE XREF: compression_header_parsej
.text:823C7D50
.text:823C7D50 .set var_30, -0x30
.text:823C7D50 .set struct_12, -0x28
.text:823C7D50 .set var_24, -0x24
.text:823C7D50 .set var_10, -0x10
.text:823C7D50 .set var_8, -8
.text:823C7D50
.text:823C7D50                 mflr    %r12            # Move from link register
.text:823C7D54                 stw     %r12, var_8(%sp) # Store Word
.text:823C7D58                 std     %r31, var_10(%sp) # Store Double Word
.text:823C7D5C                 stwu    %sp, -0x80(%sp) # Store Word with Update
.text:823C7D60                 mr      %r31, %r4       # Move Register
.text:823C7D64                 mr      %r4, %r3        # Move Register
.text:823C7D68                 addi    %r3, %sp, 0x80+struct_12 # Add Immediate
.text:823C7D6C                 bl      func_header_read_0x0c # r3 = ptr_store
.text:823C7D6C                                         # r4 = buf_data (buf_C_ptr + 0xC)
.text:823C7D70                 mr      %r7, %r31       # Move Register
.text:823C7D74                 li      %r6, 0x14       # Load Immediate
.text:823C7D78                 lwz     %r5, 0x80+var_24(%sp) # Load Word and Zero
.text:823C7D7C                 addi    %r4, %sp, 0x80+var_30 # Add Immediate
.text:823C7D80                 li      %r3, 0          # Load Immediate
.text:823C7D84                 bl      compression_initialize_sliding # r3 = 0 | ptr_realloc
.text:823C7D84                                         # r4 = ptr_to_store_stack
.text:823C7D84                                         # r5 = struct[1] (0x20000)
.text:823C7D84                                         # r6 = 0x14
.text:823C7D84                                         # r7 = *(uint*)(ptr + 8)
.text:823C7D88                 lwz     %r3, 0x80+var_30(%sp) # return *r4
.text:823C7D8C                 addi    %sp, %sp, 0x80  # Add Immediate
.text:823C7D90                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823C7D94                 mtlr    %r12            # Move to link register
.text:823C7D98                 ld      %r31, var_10(%sp) # Load Double Word
.text:823C7D9C                 blr                     # Branch unconditionally
.text:823C7D9C # End of function compression_header_parse_func
.text:823C7D9C

.text:823C73F0 # =============== S U B R O U T I N E =======================================
.text:823C73F0
.text:823C73F0 # r3 = ptr + 0xc
.text:823C73F0 # r4 = *(uint*)(ptr + 8)
.text:823C73F0 # Attributes: thunk
.text:823C73F0
.text:823C73F0 compression_header_parse:               # CODE XREF: compression_FFnew_begin+30p
.text:823C73F0                 b       compression_header_parse_func # r3 = ptr + 0xc
.text:823C73F0 # End of function compression_header_parse # r4 = *(uint*)(ptr + 8)

.text:823C7C68 # =============== S U B R O U T I N E =======================================
.text:823C7C68
.text:823C7C68 # r3 = ptr_store
.text:823C7C68 # r4 = buf_data (buf_C_ptr + 0xC)
.text:823C7C68
.text:823C7C68 func_header_read_0x0c:                  # CODE XREF: cfunc_initialize_slidding_and_others_real+28p
.text:823C7C68                                         # compression_header_parse_func+1Cp
.text:823C7C68
.text:823C7C68 .set var_10, -0x10
.text:823C7C68 .set var_8, -8
.text:823C7C68
.text:823C7C68                 mflr    %r12            # Move from link register
.text:823C7C6C                 stw     %r12, var_8(%sp) # Store Word
.text:823C7C70                 std     %r31, var_10(%sp) # Store Double Word
.text:823C7C74                 stwu    %sp, -0x60(%sp) # Store Word with Update
.text:823C7C78                 mr      %r31, %r3       # Move Register
.text:823C7C7C                 cmplwi  cr6, %r4, 0     # Compare Logical Word Immediate
.text:823C7C80                 li      %r5, 12         # 3 * 4 = 12
.text:823C7C84                 beq     cr6, r4__NULL   # r4==NULL
.text:823C7C88                 bl      memcpy          # r3 = dst_ptr
.text:823C7C88                                         # r4 = src_ptr
.text:823C7C88                                         # r5 = length
.text:823C7C8C                 b       loc_823C7C98    # Branch
.text:823C7C90 # ---------------------------------------------------------------------------
.text:823C7C90
.text:823C7C90 r4__NULL:                               # CODE XREF: func_header_read_0x0c+1Cj
.text:823C7C90                 li      %r4, 0          # r4==NULL
.text:823C7C94                 bl      memset          # r3 = ptr
.text:823C7C94                                         # r4 = byte
.text:823C7C94                                         # r5 = count
.text:823C7C98
.text:823C7C98 loc_823C7C98:                           # CODE XREF: func_header_read_0x0c+24j
.text:823C7C98                 lwz     %r11, 4(%r31)   # Load Word and Zero
.text:823C7C9C                 cmplwi  cr6, %r11, 0    # Compare Logical Word Immediate
.text:823C7CA0                 bne     cr6, loc_823C7CAC # Branch if not equal
.text:823C7CA4                 lis     %r11, 2         # Load Immediate Shifted
.text:823C7CA8                 stw     %r11, 4(%r31)   # Store Word
.text:823C7CAC
.text:823C7CAC loc_823C7CAC:                           # CODE XREF: func_header_read_0x0c+38j
.text:823C7CAC                 lwz     %r11, 8(%r31)   # Load Word and Zero
.text:823C7CB0                 cmplwi  cr6, %r11, 0    # Compare Logical Word Immediate
.text:823C7CB4                 bne     cr6, loc_823C7CC0  # Branch if not equal
.text:823C7CB8                 lis     %r11, 8         # Load Immediate Shifted
.text:823C7CBC                 stw     %r11, 8(%r31)   # Store Word
.text:823C7CC0
.text:823C7CC0 loc_823C7CC0:                              # CODE XREF: func_header_read_0x0c+4Cj
.text:823C7CC0                 addi    %sp, %sp, 0x60  # Add Immediate
.text:823C7CC4                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823C7CC8                 mtlr    %r12            # Move to link register
.text:823C7CCC                 ld      %r31, var_10(%sp) # Load Double Word
.text:823C7CD0                 blr                     # Branch unconditionally
.text:823C7CD0 # End of function func_header_read_0x0c


.text:823C8228 # =============== S U B R O U T I N E =======================================
.text:823C8228
.text:823C8228 # r3 = 0 | ptr_realloc
.text:823C8228 # r4 = ptr_to_store_stack
.text:823C8228 # r5 = struct[1] (0x20000)
.text:823C8228 # r6 = 0x14
.text:823C8228 # r7 = *(uint*)(ptr + 8)
.text:823C8228
.text:823C8228 compression_initialize_sliding:         # CODE XREF: cfunc_initialize_slidding_and_others_real+40p
.text:823C8228                                         # compression_header_parse_func+34p
.text:823C8228 r5_20000 = %r30
.text:823C8228                 mflr    %r12            # Move from link register
.text:823C822C                 bl      __savegprlr_25  # Branch
.text:823C8230                 stwu    %sp, -0x90(%sp) # Store Word with Update
.text:823C8234                 mr      r5_20000, %r5   # Move Register
.text:823C8238                 mr      %r25, %r7       # Move Register
.text:823C823C                 addi    %r27, r5_20000, 0x105 # Add Immediate
.text:823C8240                 lis     %r10, 0 # 0x8000 # Load Immediate Shifted
.text:823C8244                 add     %r11, %r27, %r6 # Add
.text:823C8248                 clrlwi. %r26, %r25, 31  # Clear Left Immediate
.text:823C824C                 ori     %r28, %r10, -0x8000 # 0x8000 # OR Immediate
.text:823C8250                 addi    %r11, %r11, 0x3010 # Add Immediate
.text:823C8254                 beq     loc_823C8264    # Branch if equal
.text:823C8258                 addis   %r11, %r11, 1   # Add Immediate Shifted
.text:823C825C                 addi    %r11, %r11, -0x67FB # Add Immediate
.text:823C8260                 b       loc_823C8270    # Branch
.text:823C8264 # ---------------------------------------------------------------------------
.text:823C8264
.text:823C8264 loc_823C8264:                           # CODE XREF: compression_initialize_sliding+2Cj
.text:823C8264                 clrrwi. %r10, %r25, 31  # Clear Right Immediate
.text:823C8268                 beq     loc_823C8270    # Branch if equal
.text:823C826C                 add     %r11, %r11, %r28 # Add
.text:823C8270
.text:823C8270 loc_823C8270:                           # CODE XREF: compression_initialize_sliding+38j
.text:823C8270                                         # compression_initialize_sliding+40j
.text:823C8270                 add     %r11, %r11, %r28 # Add
.text:823C8274                 cmplwi  cr6, %r3, 0     # Compare Logical Word Immediate
.text:823C8278                 stw     %r11, 0(%r4)    # Store Word
.text:823C827C                 beq     cr6, loc_823C8320  # r3 == NULL
.text:823C8280                 add     %r31, %r3, %r6  # ptr_realloc + 0x14
.text:823C8284                 mr      %r3, %r31       # Move Register
.text:823C8288                 addi    %r29, %r31, 0x3010 # Add Immediate
.text:823C828C                 bl      __compression_store_init_vals__ # r3 = ptr
.text:823C8290                 li      %r11, 0         # Load Immediate
.text:823C8294                 li      %r10, 4         # Load Immediate
.text:823C8298                 stw     r5_20000, 4(%r31) # Store Word
.text:823C829C                 addi    %r9, r5_20000, -1 # Add Immediate
.text:823C82A0                 stw     %r11, 0x2ED8(%r31) # Store Word
.text:823C82A4                 stw     %r11, 0x2EDC(%r31) # Store Word
.text:823C82A8                 stw     %r9, 8(%r31)    # Store Word
.text:823C82AC                 stw     %r11, 0x2EE0(%r31) # Store Word
.text:823C82B0                 stb     %r10, 0x2EB5(%r31) # Store Byte
.text:823C82B4
.text:823C82B4 loc_823C82B4:                           # CODE XREF: compression_initialize_sliding+B0j
.text:823C82B4                 lbz     %r9, 0x2EB5(%r31) # Load Byte and Zero
.text:823C82B8                 li      %r8, 1          # Load Immediate
.text:823C82BC                 add     %r7, %r9, %r31  # Add
.text:823C82C0                 addi    %r9, %r9, 1     # Add Immediate
.text:823C82C4                 lbz     %r7, 0x2EE4(%r7) # Load Byte and Zero
.text:823C82C8                 stb     %r9, 0x2EB5(%r31) # Store Byte
.text:823C82CC                 slw     %r9, %r8, %r7   # Shift Left Word
.text:823C82D0                 add     %r10, %r9, %r10 # Add
.text:823C82D4                 cmplw   cr6, %r10, r5_20000 # Compare Logical Word
.text:823C82D8                 blt     cr6, loc_823C82B4 # Branch if less than
.text:823C82DC                 add     %r10, %r29, %r27 # Add
.text:823C82E0                 stw     %r29, 0(%r31)   # Store Word
.text:823C82E4                 stw     %r11, 0x2FEC(%r31) # Store Word
.text:823C82E8                 cmplwi  cr6, %r26, 0    # Compare Logical Word Immediate
.text:823C82EC                 stw     %r10, 0x2FE8(%r31) # Store Word
.text:823C82F0                 add     %r10, %r10, %r28 # Add
.text:823C82F4                 stw     %r11, 0x2FF0(%r31) # Store Word
.text:823C82F8                 stw     %r11, 0x2FF4(%r31) # Store Word
.text:823C82FC                 stw     %r11, 0x2FF8(%r31) # Store Word
.text:823C8300                 stw     %r11, 0x2FFC(%r31) # Store Word
.text:823C8304                 stw     %r11, 0x3000(%r31) # Store Word
.text:823C8308                 stw     %r11, 0x3004(%r31) # Store Word
.text:823C830C                 stw     %r11, 0x3008(%r31) # Store Word
.text:823C8310                 beq     cr6, loc_823C831C # Branch if equal
.text:823C8314                 stw     %r10, 0x2FF4(%r31) # Store Word
.text:823C8318                 b       loc_823C8320       # r3 == NULL
.text:823C831C # ---------------------------------------------------------------------------
.text:823C831C
.text:823C831C loc_823C831C:                           # CODE XREF: compression_initialize_sliding+E8j
.text:823C831C                 clrrwi. %r11, %r25, 31  # Clear Right Immediate
.text:823C8320                 beq     loc_823C8320       # r3 == NULL
.text:823C8324                 stw     %r10, 0x300C(%r31) # Store Word
.text:823C8328
.text:823C8328 loc_823C8320:                              # CODE XREF: compression_initialize_sliding+54j
.text:823C8328                                         # compression_initialize_sliding+F0j ...
.text:823C8328                 addi    %sp, %sp, 0x90  # r3 == NULL
.text:823C832C                 b       __restgprlr_25  # Branch
.text:823C832C # End of function compression_initialize_sliding

.text:823C6D58 # =============== S U B R O U T I N E =======================================
.text:823C6D58
.text:823C6D58 # r3 = buf_unknown
.text:823C6D58 # r4 = buf_U_ptr
.text:823C6D58 # r5 = buf_unknown2
.text:823C6D58 # r6 = buf_C_ptr
.text:823C6D58 # r7 = buf_C_len
.text:823C6D58 #
.text:823C6D58 # error_codes =
.text:823C6D58 #     0x80004005 E_FAIL
.text:823C6D58 #     0x81DE2001 E_XMCDERR_MOREDATA
.text:823C6D58 #     0x8007000E E_OUTOFMEMORY
.text:823C6D58
.text:823C6D58 compression_FFnew_inner:                # CODE XREF: compression_loader+90p
.text:823C6D58
.text:823C6D58 .set var_8, -8
.text:823C6D58
.text:823C6D58 # FUNCTION CHUNK AT .text:823C73F8 SIZE 0000006C BYTES
.text:823C6D58
.text:823C6D58                 mr      %r11, %r3       # Move Register
.text:823C6D5C                 lis     %r3, -0x8000 # 0x80004005 # Unespecific error
.text:823C6D60                 ori     %r3, %r3, 0x4005 # 0x80004005 # OR Immediate
.text:823C6D64                 lwz     %r10, 4(%r11)   # Load Word and Zero
.text:823C6D68                 cmpwi   cr6, %r10, 1    # Compare Word Immediate
.text:823C6D6C                 bnelr   cr6             # Branch if not equal
.text:823C6D70                 mr      %r3, %r11       # Move Register
.text:823C6D74                 b       loc_823C73F8    # Branch
.text:823C6D74 # End of function compression_FFnew_inner


.text:823C7B80
.text:823C7B80 # =============== S U B R O U T I N E =======================================
.text:823C7B80
.text:823C7B80 # r3 = ptr_state
.text:823C7B80 # r4 = buf_U_ptr
.text:823C7B80 # r5 = *buf_U_len
.text:823C7B80 # r6 = buf_C_ptr
.text:823C7B80 # r7 = buf_C_len
.text:823C7B80 #
.text:823C7B80
.text:823C7B80 compression_FFnew_inner_real:           # CODE XREF: compression_FFnew_inner:loc_823C7428p
.text:823C7B80
.text:823C7B80 .set block_U_len, -0x50
.text:823C7B80
.text:823C7B80 bytes_left = %r29
.text:823C7B80 buf_C_ptr = %r31
.text:823C7B80 buf_U_ptr = %r28
.text:823C7B80 buf_C_len = %r30
.text:823C7B80 buf_U_len = %r8
.text:823C7B80 ptr_state_0x14 = %r26
.text:823C7B80 total_U_len = %r27
.text:823C7B80 stats_out = %r25
.text:823C7B80                 mflr    %r12            # Move from link register
.text:823C7B84                 bl      __savegprlr_25  # Branch
.text:823C7B88                 stwu    %sp, -0xA0(%sp) # Store Word with Update
.text:823C7B8C                 lwz     %r11, 8(%r3)    # Load Word and Zero
.text:823C7B90                 mr      buf_U_ptr, %r4  # 0x2ED4
.text:823C7B94                 mr      stats_out, %r5  # Move Register
.text:823C7B98                 mr      buf_C_ptr, %r6  # Move Register
.text:823C7B9C                 mr      bytes_left, %r7 # Move Register
.text:823C7BA0                 addi    ptr_state_0x14, %r3, 0x14 # Add Immediate
.text:823C7BA4                 clrlwi. %r11, %r11, 31  # Clear Left Immediate
.text:823C7BA8                 li      total_U_len, 0  # Load Immediate
.text:823C7BAC                 bne     loc_823C7BB4    # Branch if not equal
.text:823C7BB0                 bl      cfunc_823C7AF8_set_0_1 # 0x2ED4
.text:823C7BB4
.text:823C7BB4 loc_823C7BB4:                           # CODE XREF: compression_FFnew_inner_real+2Cj
.text:823C7BB4                 cmplwi  cr6, bytes_left, 0 # Compare Logical Word Immediate
.text:823C7BB8                 beq     cr6, loc_823C7BB8  # Branch if equal
.text:823C7BBC
.text:823C7BBC loc_823C7BBC:                           # CODE XREF: compression_FFnew_inner_real+D4j
.text:823C7BBC                 li      %r11, 0         # Load Immediate
.text:823C7BC0                 stw     %r11, 0xA0+block_U_len(%sp) # Store Word
.text:823C7BC4                 lbz     %r11, 0(buf_C_ptr) # Load Byte and Zero
.text:823C7BC8                 cmplwi  cr6, %r11, 0xFF # Compare Logical Word Immediate
.text:823C7BCC                 bne     cr6, not_FF     # 15 bits
.text:823C7BD0                 lbz     %r11, 3(buf_C_ptr) # 32 bits
.text:823C7BD4                 addi    bytes_left, bytes_left, -5 # Add Immediate
.text:823C7BD8                 lbz     %r10, 1(buf_C_ptr) # Load Byte and Zero
.text:823C7BDC                 lbz     %r9, 4(buf_C_ptr) # Load Byte and Zero
.text:823C7BE0                 rotlwi  %r11, %r11, 8   # Rotate Left Immediate
.text:823C7BE4                 lbz     buf_U_len, 2(buf_C_ptr) # Load Byte and Zero
.text:823C7BE8                 rotlwi  %r10, %r10, 8   # Rotate Left Immediate
.text:823C7BEC                 or      buf_C_len, %r11, %r9 # OR
.text:823C7BF0                 or      buf_U_len, %r10, buf_U_len # OR
.text:823C7BF4                 addi    buf_C_ptr, buf_C_ptr, 5 # Add Immediate
.text:823C7BF8                 cmplw   cr6, buf_C_len, bytes_left # Compare Logical Word
.text:823C7BFC                 bge     cr6, loc_823C7C24 # Branch if greater than or equal
.text:823C7C00                 mr      bytes_left, buf_C_len # Move Register
.text:823C7C04                 b       loc_823C7C24    # Branch
.text:823C7C08 # ---------------------------------------------------------------------------
.text:823C7C08
.text:823C7C08 not_FF:                                 # CODE XREF: compression_FFnew_inner_real+4Cj
.text:823C7C08                 lbz     %r10, 1(buf_C_ptr) # 15 bits
.text:823C7C0C                 slwi    %r11, %r11, 8   # Shift Left Immediate
.text:823C7C10                 lis     buf_U_len, 0 # 0x8000 # Load Immediate Shifted
.text:823C7C14                 or      buf_C_len, %r10, %r11 # r30 = (r11 << 8) | r10
.text:823C7C18                 ori     buf_U_len, buf_U_len, -0x8000 # 0x8000 # OR Immediate
.text:823C7C1C                 addi    buf_C_ptr, buf_C_ptr, 2 # Add Immediate
.text:823C7C20                 addi    bytes_left, bytes_left, -2 # Add Immediate
.text:823C7C24
.text:823C7C24 loc_823C7C24:                           # CODE XREF: compression_FFnew_inner_real+7Cj
.text:823C7C24                                         # compression_FFnew_inner_real+84j
.text:823C7C24                 addi    %r9, %sp, 0xA0+block_U_len # Add Immediate
.text:823C7C28                 mr      %r7, buf_U_ptr  # Move Register
.text:823C7C2C                 mr      %r6, buf_C_len  # Move Register
.text:823C7C30                 mr      %r5, buf_C_ptr  # Move Register
.text:823C7C34                 mr      %r4, buf_U_len  # Move Register
.text:823C7C38                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C7C3C                 bl      decompress_chunk # r3 = ptr_state_0x14
.text:823C7C3C                                         # r4 = buf_U_len
.text:823C7C3C                                         # r5 = buf_C_ptr
.text:823C7C3C                                         # r6 = buf_C_len
.text:823C7C3C                                         # r7 = buf_U_ptr
.text:823C7C3C                                         #
.text:823C7C3C                                         # r9 = *block_U_len
.text:823C7C3C                                         #
.text:823C7C40                 lwz     %r11, 0xA0+block_U_len(%sp) # Load Word and Zero
.text:823C7C44                 subf.   bytes_left, buf_C_len, bytes_left # Subtract from
.text:823C7C48                 add     buf_C_ptr, buf_C_len, buf_C_ptr # C_ptr += C_len
.text:823C7C4C                 add     buf_U_ptr, %r11, buf_U_ptr # U_ptr += U_len
.text:823C7C50                 add     total_U_len, %r11, total_U_len # Add
.text:823C7C54                 bne     loc_823C7BBC    # Branch if not equal
.text:823C7C58
.text:823C7C58 loc_823C7BB8:                              # CODE XREF: compression_FFnew_inner_real+38j
.text:823C7C58                 stw     total_U_len, 0(stats_out) # Store Word
.text:823C7C5C                 li      %r3, 0          # Load Immediate
.text:823C7C60                 addi    %sp, %sp, 0xA0  # Add Immediate
.text:823C7C64                 b       __restgprlr_25  # Branch
.text:823C7C64 # End of function compression_FFnew_inner_real
.text:823C7C64



.text:823C7AF8
.text:823C7AF8 # =============== S U B R O U T I N E =======================================
.text:823C7AF8
.text:823C7AF8 # 0x2ED4
.text:823C7AF8
.text:823C7AF8 cfunc_823C7AF8_set_0_1:                 # CODE XREF: compression_FFnew_inner_real+30p
.text:823C7AF8                                         # cfunc_initialize_slidding_and_others_real+58p
.text:823C7AF8
.text:823C7AF8 .set var_18, -0x18
.text:823C7AF8 .set var_10, -0x10
.text:823C7AF8 .set var_8, -8
.text:823C7AF8
.text:823C7AF8                 mflr    %r12            # Move from link register
.text:823C7AFC                 stw     %r12, var_8(%sp) # Store Word
.text:823C7B00                 std     %r30, var_18(%sp) # Store Double Word
.text:823C7B04                 std     %r31, var_10(%sp) # Store Double Word
.text:823C7B08                 stwu    %sp, -0x70(%sp) # Store Word with Update
.text:823C7B0C                 mr      %r30, %r3       # Move Register
.text:823C7B10                 addi    %r31, %r30, 0x14 # Add Immediate
.text:823C7B14                 mr      %r3, %r31       # Move Register
.text:823C7B18                 bl      cfunc_823C7ED0_set_0_1 # 0x2ED4
.text:823C7B1C                 lwz     %r11, 8(%r30)   # Load Word and Zero
.text:823C7B20                 clrlwi. %r10, %r11, 31  # Clear Left Immediate
.text:823C7B24                 beq     loc_823C7B34    # Branch if equal
.text:823C7B28                 li      %r11, 0         # Load Immediate
.text:823C7B2C                 stb     %r11, 0x2FFC(%r31) # Store Byte
.text:823C7B30                 b       loc_823C7B54    # Branch
.text:823C7B34 # ---------------------------------------------------------------------------
.text:823C7B34
.text:823C7B34 loc_823C7B34:                           # CODE XREF: cfunc_823C7AF8_set_0_1+2Cj
.text:823C7B34                 clrrwi. %r11, %r11, 31  # Clear Right Immediate
.text:823C7B38                 beq     loc_823C7B60    # Branch if equal
.text:823C7B3C                 li      %r11, 0         # Load Immediate
.text:823C7B40                 stw     %r11, 0x2FF4(%r31) # Store Word
.text:823C7B44                 stw     %r11, 0x2FFC(%r31) # Store Word
.text:823C7B48                 stw     %r11, 0x3000(%r31) # Store Word
.text:823C7B4C                 stw     %r11, 0x3004(%r31) # Store Word
.text:823C7B50                 stw     %r11, 0x3008(%r31) # Store Word
.text:823C7B54
.text:823C7B54 loc_823C7B54:                           # CODE XREF: cfunc_823C7AF8_set_0_1+38j
.text:823C7B54                 stw     %r11, 0x2FF8(%r31) # Store Word
.text:823C7B58                 stw     %r11, 0x2FF0(%r31) # Store Word
.text:823C7B5C                 stw     %r11, 0x2FEC(%r31) # Store Word
.text:823C7B60
.text:823C7B60 loc_823C7B60:                           # CODE XREF: cfunc_823C7AF8_set_0_1+40j
.text:823C7B60                 li      %r3, 0          # Load Immediate
.text:823C7B64                 addi    %sp, %sp, 0x70  # Add Immediate
.text:823C7B68                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823C7B6C                 mtlr    %r12            # Move to link register
.text:823C7B70                 ld      %r30, var_18(%sp) # Load Double Word
.text:823C7B74                 ld      %r31, var_10(%sp) # Load Double Word
.text:823C7B78                 blr                     # Branch unconditionally
.text:823C7B78 # End of function cfunc_823C7AF8_set_0_1
.text:823C7B78
.text:823C7B78 # ---------------------------------------------------------------------------





.text:823C7ED0
.text:823C7ED0 # =============== S U B R O U T I N E =======================================
.text:823C7ED0
.text:823C7ED0 # 0x2ED4
.text:823C7ED0
.text:823C7ED0 cfunc_823C7ED0_set_0_1:                 # CODE XREF: cfunc_823C7AF8_set_0_1+20p
.text:823C7ED0
.text:823C7ED0 .set var_10, -0x10
.text:823C7ED0 .set var_8, -8
.text:823C7ED0
.text:823C7ED0                 mflr    %r12            # Move from link register
.text:823C7ED4                 stw     %r12, var_8(%sp) # Store Word
.text:823C7ED8                 std     %r31, var_10(%sp) # Store Double Word
.text:823C7EDC                 stwu    %sp, -0x60(%sp) # Store Word with Update
.text:823C7EE0                 mr      %r31, %r3       # Move Register
.text:823C7EE4                 bl      cfunc_823C8330_set_zero # 0x2ED4
.text:823C7EE8                 mr      %r3, %r31       # Move Register
.text:823C7EEC                 bl      cfunc_823C83A8_set_0_1 # 0x2ED4
.text:823C7EF0                 mr      %r3, %r31       # Move Register
.text:823C7EF4                 bl      set_r3_2EC0_to_0 # Branch
.text:823C7EF8                 li      %r11, 0         # Load Immediate
.text:823C7EFC                 stw     %r11, 0x2EC4(%r31) # Store Word
.text:823C7F00                 addi    %sp, %sp, 0x60  # Add Immediate
.text:823C7F04                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823C7F08                 mtlr    %r12            # Move to link register
.text:823C7F0C                 ld      %r31, var_10(%sp) # Load Double Word
.text:823C7F10                 blr                     # Branch unconditionally
.text:823C7F10 # End of function cfunc_823C7ED0_set_0_1
.text:823C7F10



.text:823C8330
.text:823C8330 # =============== S U B R O U T I N E =======================================
.text:823C8330
.text:823C8330 # 0x2ED4
.text:823C8330
.text:823C8330 cfunc_823C8330_set_zero:                # CODE XREF: cfunc_823C7ED0_set_0_1+14p
.text:823C8330
.text:823C8330 .set var_10, -0x10
.text:823C8330 .set var_8, -8
.text:823C8330
.text:823C8330                 mflr    %r12            # Move from link register
.text:823C8334                 stw     %r12, var_8(%sp) # Store Word
.text:823C8338                 std     %r31, var_10(%sp) # Store Double Word
.text:823C833C                 stwu    %sp, -0x60(%sp) # Store Word with Update
.text:823C8340                 mr      %r31, %r3       # Move Register
.text:823C8344                 li      %r4, 0          # Load Immediate
.text:823C8348                 addi    %r3, %r31, 0xA18 # Add Immediate
.text:823C834C                 lbz     %r11, 0x2EB5(%r31) # Load Byte and Zero
.text:823C8350                 addi    %r11, %r11, 0x20 # Add Immediate
.text:823C8354                 slwi    %r5, %r11, 3    # Shift Left Immediate
.text:823C8358                 bl      memset          # r3 = ptr
.text:823C8358                                         # r4 = byte
.text:823C8358                                         # r5 = count
.text:823C835C                 lbz     %r11, 0x2EB5(%r31) # Load Byte and Zero
.text:823C8360                 li      %r4, 0          # Load Immediate
.text:823C8364                 addi    %r11, %r11, 0x20 # Add Immediate
.text:823C8368                 addi    %r3, %r31, 0x2B14 # Add Immediate
.text:823C836C                 slwi    %r5, %r11, 3    # Shift Left Immediate
.text:823C8370                 bl      memset          # r3 = ptr
.text:823C8370                                         # r4 = byte
.text:823C8370                                         # r5 = count
.text:823C8374                 li      %r5, 0xF9       # Load Immediate
.text:823C8378                 li      %r4, 0          # Load Immediate
.text:823C837C                 addi    %r3, %r31, 0xCB8 # Add Immediate
.text:823C8380                 bl      memset          # r3 = ptr
.text:823C8380                                         # r4 = byte
.text:823C8380                                         # r5 = count
.text:823C8384                 li      %r5, 0xF9       # Load Immediate
.text:823C8388                 li      %r4, 0          # Load Immediate
.text:823C838C                 addi    %r3, %r31, 0x2DB4 # Add Immediate
.text:823C8390                 bl      memset          # r3 = ptr
.text:823C8390                                         # r4 = byte
.text:823C8390                                         # r5 = count
.text:823C8394                 addi    %sp, %sp, 0x60  # Add Immediate
.text:823C8398                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823C839C                 mtlr    %r12            # Move to link register
.text:823C83A0                 ld      %r31, var_10(%sp) # Load Double Word
.text:823C83A4                 blr                     # Branch unconditionally
.text:823C83A4 # End of function cfunc_823C8330_set_zero
.text:823C83A4




.text:823C83A8
.text:823C83A8 # =============== S U B R O U T I N E =======================================
.text:823C83A8
.text:823C83A8 # 0x2ED4
.text:823C83A8
.text:823C83A8 cfunc_823C83A8_set_0_1:                 # CODE XREF: cfunc_823C7ED0_set_0_1+1Cp
.text:823C83A8                 li      %r11, 0         # Load Immediate
.text:823C83AC                 li      %r10, 1         # Load Immediate
.text:823C83B0                 stw     %r11, 0x2EB8(%r3) # Store Word
.text:823C83B4                 stw     %r10, 0xC(%r3)  # Store Word
.text:823C83B8                 stw     %r10, 0x10(%r3) # Store Word
.text:823C83BC                 stw     %r10, 0x14(%r3) # Store Word
.text:823C83C0                 stw     %r11, 0x2B10(%r3) # Store Word
.text:823C83C4                 stw     %r10, 0x2ED4(%r3) # Store Word
.text:823C83C8                 stw     %r11, 0x2ECC(%r3) # Store Word
.text:823C83CC                 stw     %r11, 0x2ED0(%r3) # Store Word
.text:823C83D0                 stb     %r10, 0x2EB6(%r3) # Store Byte
.text:823C83D4                 stw     %r11, 0x2EBC(%r3) # Store Word
.text:823C83D8                 stb     %r11, 0x2EB7(%r3) # Store Byte
.text:823C83DC                 blr                     # Branch unconditionally
.text:823C83DC # End of function cfunc_823C83A8_set_0_1
.text:823C83DC



.text:823C83E0
.text:823C83E0 # =============== S U B R O U T I N E =======================================
.text:823C83E0
.text:823C83E0
.text:823C83E0 set_r3_2EC0_to_0:                       # CODE XREF: cfunc_823C7ED0_set_0_1+24p
.text:823C83E0                 li      %r11, 0         # Load Immediate
.text:823C83E4                 stw     %r11, 0x2EC0(%r3) # Store Word
.text:823C83E8                 blr                     # Branch unconditionally
.text:823C83E8 # End of function set_r3_2EC0_to_0
.text:823C83E8
.text:823C83E8 # ---------------------------------------------------------------------------




.text:823C7F18
.text:823C7F18 # =============== S U B R O U T I N E =======================================
.text:823C7F18
.text:823C7F18 # r3 = ptr_state_0x14
.text:823C7F18 # r4 = buf_U_len
.text:823C7F18 # r5 = buf_C_ptr
.text:823C7F18 # r6 = buf_C_len
.text:823C7F18 # r7 = buf_U_ptr
.text:823C7F18 #
.text:823C7F18 # r9 = *block_U_len
.text:823C7F18 #
.text:823C7F18
.text:823C7F18 decompress_chunk:                       # CODE XREF: compression_FFnew_inner_real+BCp
.text:823C7F18                 mflr    %r12            # Move from link register
.text:823C7F1C                 bl      __savegprlr_29  # Branch
.text:823C7F20                 stwu    %sp, -0x70(%sp) # Store Word with Update
.text:823C7F24                 mr      %r31, %r3       # Move Register
.text:823C7F28                 add     %r11, %r5, %r6  # buf_C_ptr_end
.text:823C7F2C                 mr      %r3, %r7        # Move Register
.text:823C7F30                 addi    %r11, %r11, 4   # Add Immediate
.text:823C7F34                 mr      %r29, %r4       # Move Register
.text:823C7F38                 stw     %r7, 0x2B0C(%r31) # 0x2B0C(%r31) = buf_U_ptr
.text:823C7F3C                 mr      %r30, %r9       # Move Register
.text:823C7F40                 stw     %r5, 0x2B04(%r31) # 0x2B04(%r31) = buf_C_ptr
.text:823C7F44                 stw     %r11, 0x2B08(%r31) # 0x2B08(%r31) = buf_C_ptr_end + 4
.text:823C7F48                 bl      MmQueryAddressProtect # Branch
.text:823C7F4C                 rlwinm  %r11, %r3, 0,21,22 # Rotate Left Word Immediate then AND with Mask
.text:823C7F50                 mr      %r3, %r31       # Move Register
.text:823C7F54                 cntlzw  %r11, %r11      # Count Leading Zeros Word
.text:823C7F58                 extrwi  %r11, %r11, 1,26 # Extract and Right Justify Immediate
.text:823C7F5C                 xori    %r11, %r11, 1   # !(bit)
.text:823C7F60                 stb     %r11, 0x2FE4(%r31) # Store Byte
.text:823C7F64                 bl      j_cfunc_read_4_bytes # r3 = ptr_state
.text:823C7F68                 mr      %r4, %r29       # Move Register
.text:823C7F6C                 mr      %r3, %r31       # Move Register
.text:823C7F70                 bl      compression_FFnew_inner_real_kind_final # r3 = ptr_state_0x14
.text:823C7F70                                         # r4 = buf_U_len
.text:823C7F74                 lwz     %r10, 0x2EC4(%r31) # counter
.text:823C7F78                 mr.     %r11, %r3       # Move Register
.text:823C7F7C                 addi    %r10, %r10, 1   # Add Immediate
.text:823C7F80                 stw     %r10, 0x2EC4(%r31) # counter
.text:823C7F84                 bge     loc_823C7F98    # Branch if greater than or equal
.text:823C7F88                 li      %r11, 0         # Load Immediate
.text:823C7F8C                 li      %r3, 1          # Load Immediate
.text:823C7F90                 stw     %r11, 0(%r30)   # Store Word
.text:823C7F94                 b       loc_823C7F94       # Branch
.text:823C7F98 # ---------------------------------------------------------------------------
.text:823C7F98
.text:823C7F98 loc_823C7F98:                           # CODE XREF: decompress_chunk+6Cj
.text:823C7F98                 stw     %r11, 0(%r30)   # Store Word
.text:823C7F9C                 li      %r3, 0          # Load Immediate
.text:823C7FA0                 lwz     %r10, 0x2B10(%r31) # Load Word and Zero
.text:823C7FA4                 add     %r11, %r10, %r11 # Add
.text:823C7FA8                 stw     %r11, 0x2B10(%r31) # Store Word
.text:823C7FAC
.text:823C7FAC loc_823C7F94:                              # CODE XREF: decompress_chunk+7Cj
.text:823C7FAC                 addi    %sp, %sp, 0x70  # Add Immediate
.text:823C7FB0                 b       __restgprlr_29  # Branch
.text:823C7FB0 # End of function decompress_chunk
.text:823C7FB0





.text:823C8850
.text:823C8850 # =============== S U B R O U T I N E =======================================
.text:823C8850
.text:823C8850 # r3 = ptr_state
.text:823C8850 # Attributes: thunk
.text:823C8850
.text:823C8850 j_cfunc_read_4_bytes:                   # CODE XREF: decompress_chunk+4Cp
.text:823C8850                 b       cfunc_read_4_bytes # Branch
.text:823C8850 # End of function j_cfunc_read_4_bytes
.text:823C8850
.text:823C8850 # ---------------------------------------------------------------------------






.text:823C87F0
.text:823C87F0 # =============== S U B R O U T I N E =======================================
.text:823C87F0
.text:823C87F0
.text:823C87F0 cfunc_read_4_bytes:                     # CODE XREF: compression_FFnew_inner_real_kind_final+94p
.text:823C87F0                                         # compression_FFnew_inner_real_kind_final+22Cp ...
.text:823C87F0                 lwz     %r11, 0x2ED0(%r3) # Load Word and Zero
.text:823C87F4                 cmpwi   cr6, %r11, 3  # Compare Word Immediate
.text:823C87F8                 beqlr   cr6             # Branch if equal
.text:823C87FC                 lwz     %r11, 0x2B04(%r3) # buf_C_ptr
.text:823C8800                 lwz     %r9, 0x2B08(%r3) # buf_C_ptr_end
.text:823C8804                 addi    %r10, %r11, 4   # Add Immediate
.text:823C8808                 cmplw   cr6, %r10, %r9  # Compare Logical Word
.text:823C880C                 bgtlr   cr6             # Branch if greater than
.text:823C8810                 lbz     %r9, 1(%r11)    # Load Byte and Zero
.text:823C8814                 li      %r8, 0x10       # Load Immediate
.text:823C8818                 lbz     %r7, 0(%r11)    # Load Byte and Zero
.text:823C881C                 rotlwi  %r9, %r9, 8     # Rotate Left Immediate
.text:823C8820                 lbz     %r6, 3(%r11)    # Load Byte and Zero
.text:823C8824                 lbz     %r11, 2(%r11)   # Load Byte and Zero
.text:823C8828                 or      %r9, %r9, %r7   # r9 = read16bits
.text:823C882C                 stw     %r10, 0x2B04(%r3) # buf_C_ptr += 4
.text:823C8830                 stb     %r8, 0x2EB4(%r3) # Store Byte
.text:823C8834                 slwi    %r10, %r9, 8    # Shift Left Immediate
.text:823C8838                 or      %r10, %r10, %r6 # OR
.text:823C883C                 slwi    %r10, %r10, 8   # Shift Left Immediate
.text:823C8840                 or      %r11, %r10, %r11 # OR
.text:823C8844                 stw     %r11, 0x2EB0(%r3) # Store Word
.text:823C8848                 blr                     # Branch unconditionally
.text:823C8848 # End of function cfunc_read_4_bytes
.text:823C8848







.text:823C73A8
.text:823C73A8 # =============== S U B R O U T I N E =======================================
.text:823C73A8
.text:823C73A8 # r3 = buf_data (buf_C_ptr + 0xC)
.text:823C73A8 # r4 = buf_value (*(uint *)(buf_C_ptr + 8))
.text:823C73A8 # r5 = ptr_realloc
.text:823C73A8 # r6 = value_unknown
.text:823C73A8
.text:823C73A8 cfunc_initialize_slidding_and_others:   # CODE XREF: compression_FFnew_begin+70p
.text:823C73A8
.text:823C73A8 .set var_10, -0x10
.text:823C73A8 .set var_8, -8
.text:823C73A8
.text:823C73A8                 mflr    %r12            # Move from link register
.text:823C73AC                 stw     %r12, var_8(%sp) # Store Word
.text:823C73B0                 std     %r31, var_10(%sp) # Store Double Word
.text:823C73B4                 stwu    %sp, -0x60(%sp) # Store Word with Update
.text:823C73B8                 mr      %r31, %r5       # Move Register
.text:823C73BC                 lis     %r11, 0x76C3 # 0x76C3F251 # Load Immediate Shifted
.text:823C73C0                 li      %r10, 1         # Load Immediate
.text:823C73C4                 ori     %r11, %r11, -0xDAF # 0x76C3F251 # OR Immediate
.text:823C73C8                 stw     %r4, 8(%r31)    # Store Word
.text:823C73CC                 stw     %r11, 0(%r31)   # Store Word
.text:823C73D0                 stw     %r10, 4(%r31)   # Store Word
.text:823C73D4                 bl      cfunc_initialize_slidding_and_others_real # r3 = buf_data (buf_C_ptr + 0xC)
.text:823C73D4                                         # r4 = buf_value (*(uint *)(buf_C_ptr + 8))
.text:823C73D4                                         # r5 = ptr_realloc
.text:823C73D4                                         # r6 = value_unknown
.text:823C73D8                 mr      %r3, %r31       # Move Register
.text:823C73DC                 addi    %sp, %sp, 0x60  # Add Immediate
.text:823C73E0                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823C73E4                 mtlr    %r12            # Move to link register
.text:823C73E8                 ld      %r31, var_10(%sp) # Load Double Word
.text:823C73EC                 blr                     # Branch unconditionally
.text:823C73EC # End of function cfunc_initialize_slidding_and_others
.text:823C73EC






.text:823C7CD8
.text:823C7CD8 # =============== S U B R O U T I N E =======================================
.text:823C7CD8
.text:823C7CD8 # r3 = buf_data (buf_C_ptr + 0xC)
.text:823C7CD8 # r4 = buf_value (*(uint *)(buf_C_ptr + 8))
.text:823C7CD8 # r5 = ptr_realloc
.text:823C7CD8 # r6 = value_unknown
.text:823C7CD8
.text:823C7CD8 cfunc_initialize_slidding_and_others_real:
.text:823C7CD8                                         # CODE XREF: cfunc_initialize_slidding_and_others+2Cp
.text:823C7CD8
.text:823C7CD8 .set var_30, -0x30
.text:823C7CD8 .set var_2C, -0x2C
.text:823C7CD8 .set var_18, -0x18
.text:823C7CD8 .set var_10, -0x10
.text:823C7CD8 .set var_8, -8
.text:823C7CD8 .set arg_2C,  0x2C
.text:823C7CD8
.text:823C7CD8                 mflr    %r12            # Move from link register
.text:823C7CDC                 stw     %r12, var_8(%sp) # Store Word
.text:823C7CE0                 std     %r30, var_18(%sp) # Store Double Word
.text:823C7CE4                 std     %r31, var_10(%sp) # Store Double Word
.text:823C7CE8                 stwu    %sp, -0x80(%sp) # Store Word with Update
.text:823C7CEC                 stw     %r6, 0x80+arg_2C(%sp) # Store Word
.text:823C7CF0                 mr      %r30, %r4       # Move Register
.text:823C7CF4                 mr      %r4, %r3        # Move Register
.text:823C7CF8                 addi    %r3, %sp, 0x80+var_30 # Add Immediate
.text:823C7CFC                 mr      %r31, %r5       # Move Register
.text:823C7D00                 bl      func_header_read_0x0c # r3 = ptr_store
.text:823C7D00                                         # r4 = buf_data (buf_C_ptr + 0xC)
.text:823C7D04                 mr      %r7, %r30       # Move Register
.text:823C7D08                 li      %r6, 0x14       # Load Immediate
.text:823C7D0C                 lwz     %r5, 0x80+var_2C(%sp) # Load Word and Zero
.text:823C7D10                 addi    %r4, %sp, 0x80+arg_2C # Add Immediate
.text:823C7D14                 mr      %r3, %r31       # Move Register
.text:823C7D18                 bl      compression_initialize_sliding # r3 = 0 | ptr_realloc
.text:823C7D18                                         # r4 = ptr_to_store_stack
.text:823C7D18                                         # r5 = struct[1] (0x20000)
.text:823C7D18                                         # r6 = 0x14
.text:823C7D18                                         # r7 = *(uint*)(ptr + 8)
.text:823C7D1C                 clrlwi. %r11, %r30, 31  # Clear Left Immediate
.text:823C7D20                 lwz     %r11, 0x80+var_30(%sp) # Load Word and Zero
.text:823C7D24                 stw     %r11, 0xC(%r31) # Store Word
.text:823C7D28                 beq     loc_823C7D34       # Branch if equal
.text:823C7D2C                 mr      %r3, %r31       # Move Register
.text:823C7D30                 bl      cfunc_823C7AF8_set_0_1 # 0x2ED4
.text:823C7D34
.text:823C7D34 loc_823C7D34:                              # CODE XREF: cfunc_initialize_slidding_and_others_real+50j
.text:823C7D34                 addi    %sp, %sp, 0x80  # Add Immediate
.text:823C7D38                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823C7D3C                 mtlr    %r12            # Move to link register
.text:823C7D40                 ld      %r30, var_18(%sp) # Load Double Word
.text:823C7D44                 ld      %r31, var_10(%sp) # Load Double Word
.text:823C7D48                 blr                     # Branch unconditionally
.text:823C7D48 # End of function cfunc_initialize_slidding_and_others_real
.text:823C7D48
.text:823C7D48 # ---------------------------------------------------------------------------





.text:823C7FB8
.text:823C7FB8 # =============== S U B R O U T I N E =======================================
.text:823C7FB8
.text:823C7FB8 # r3 = ptr
.text:823C7FB8
.text:823C7FB8 __compression_store_init_vals__:        # CODE XREF: compression_initialize_sliding+64p
.text:823C7FB8                 mflr    %r12            # Move from link register
.text:823C7FBC                 bl      __savegprlr_17  # Branch
.text:823C7FC0                 lis     %r9, 0x101 # 0x1010202 # Load Immediate Shifted
.text:823C7FC4                 lis     %r8, 0x303 # 0x3030404 # Load Immediate Shifted
.text:823C7FC8                 ori     %r9, %r9, 0x202 # 0x1010202 # OR Immediate
.text:823C7FCC                 lis     %r6, 0x707 # 0x7070808 # Load Immediate Shifted
.text:823C7FD0                 lis     %r7, 0x505 # 0x5050606 # Load Immediate Shifted
.text:823C7FD4                 stw     %r9, 0x2EE8(%r3) # r3[0x2EE8] = 0x01010202
.text:823C7FD8                 ori     %r8, %r8, 0x404 # 0x3030404 # OR Immediate
.text:823C7FDC                 ori     %r7, %r7, 0x606 # 0x5050606 # OR Immediate
.text:823C7FE0                 ori     %r9, %r6, 0x808 # 0x7070808 # OR Immediate
.text:823C7FE4                 stw     %r8, 0x2EEC(%r3) # r3[0x2EEC] = 0x03030404
.text:823C7FE8                 lis     %r5, 0x909 # 0x9090A0A # Load Immediate Shifted
.text:823C7FEC                 stw     %r7, 0x2EF0(%r3) # r3[0x2EF0] = 0x05050606
.text:823C7FF0                 lis     %r4, 0xB0B # 0xB0B0C0C # Load Immediate Shifted
.text:823C7FF4                 stw     %r9, 0x2EF4(%r3) # r3[0x2EF4] = 0x07070808
.text:823C7FF8                 lis     %r31, 0xD0D # 0xD0D0E0E # Load Immediate Shifted
.text:823C7FFC                 ori     %r8, %r5, 0xA0A # 0x9090A0A # OR Immediate
.text:823C8000                 ori     %r7, %r4, 0xC0C # 0xB0B0C0C # OR Immediate
.text:823C8004                 ori     %r9, %r31, 0xE0E # 0xD0D0E0E # OR Immediate
.text:823C8008                 stw     %r8, 0x2EF8(%r3) # r3[0x2EF8] = 0x09090A0A
.text:823C800C                 lis     %r11, 0x1111 # 0x11111111 # Load Immediate Shifted
.text:823C8010                 stw     %r7, 0x2EFC(%r3) # r3[0x2EFC] = 0x0B0B0C0C
.text:823C8014                 lis     %r30, 0xF0F # 0xF0F1010 # Load Immediate Shifted
.text:823C8018                 stw     %r9, 0x2F00(%r3) # r3[0x2F00] = 0x0D0D0E0E
.text:823C801C                 ori     %r11, %r11, 0x1111 # 0x11111111 # OR Immediate
.text:823C8020                 li      %r10, 0         # Load Immediate
.text:823C8024                 ori     %r8, %r30, 0x1010 # 0xF0F1010 # OR Immediate
.text:823C8028                 stw     %r11, 0x2F08(%r3) # r3[0x2F08] = 0x11111111
.text:823C802C                 li      %r7, -2         # Load Immediate
.text:823C8030                 stw     %r10, 0x2EE4(%r3) # r3[0x2EE4] = 0x00000000
.text:823C8034                 li      %r9, -1         # Load Immediate
.text:823C8038                 stw     %r8, 0x2F04(%r3) # r3[0x2F04] = 0x0F0F1010
.text:823C803C                 stw     %r11, 0x2F0C(%r3) # Store Word
.text:823C8040                 li      %r8, 1          # Load Immediate
.text:823C8044                 stw     %r11, 0x2F10(%r3) # Store Word
.text:823C8048                 li      %r6, 2          # Load Immediate
.text:823C804C                 stw     %r11, 0x2F14(%r3) # Store Word
.text:823C8050                 li      %r5, 4          # Load Immediate
.text:823C8054                 stw     %r7, 0x2F18(%r3) # Store Word
.text:823C8058                 li      %r11, 6         # Load Immediate
.text:823C805C                 stw     %r9, 0x2F1C(%r3) # Store Word
.text:823C8060                 li      %r7, 0xA        # Load Immediate
.text:823C8064                 stw     %r10, 0x2F20(%r3) # Store Word
.text:823C8068                 li      %r9, 0xE        # Load Immediate
.text:823C806C                 li      %r10, 0x16      # Load Immediate
.text:823C8070                 stw     %r8, 0x2F24(%r3) # Store Word
.text:823C8074                 stw     %r6, 0x2F28(%r3) # Store Word
.text:823C8078                 li      %r8, 0x1E       # Load Immediate
.text:823C807C                 li      %r6, 0x2E       # Load Immediate
.text:823C8080                 stw     %r5, 0x2F2C(%r3) # Store Word
.text:823C8084                 stw     %r11, 0x2F30(%r3) # Store Word
.text:823C8088                 li      %r5, 0x3E       # Load Immediate
.text:823C808C                 stw     %r7, 0x2F34(%r3) # Store Word
.text:823C8090                 li      %r11, 0x5E      # Load Immediate
.text:823C8094                 stw     %r9, 0x2F38(%r3) # Store Word
.text:823C8098                 li      %r7, 0x7E       # Load Immediate
.text:823C809C                 stw     %r10, 0x2F3C(%r3) # Store Word
.text:823C80A0                 li      %r9, 0xBE       # Load Immediate
.text:823C80A4                 li      %r10, 0xFE      # Load Immediate
.text:823C80A8                 stw     %r8, 0x2F40(%r3) # Store Word
.text:823C80AC                 stw     %r6, 0x2F44(%r3) # Store Word
.text:823C80B0                 li      %r8, 0x17E      # Load Immediate
.text:823C80B4                 li      %r6, 0x1FE      # Load Immediate
.text:823C80B8                 stw     %r5, 0x2F48(%r3) # Store Word
.text:823C80BC                 stw     %r11, 0x2F4C(%r3) # Store Word
.text:823C80C0                 li      %r5, 0x2FE      # Load Immediate
.text:823C80C4                 stw     %r7, 0x2F50(%r3) # Store Word
.text:823C80C8                 li      %r11, 0x3FE     # Load Immediate
.text:823C80CC                 stw     %r9, 0x2F54(%r3) # Store Word
.text:823C80D0                 li      %r7, 0x5FE      # Load Immediate
.text:823C80D4                 stw     %r10, 0x2F58(%r3) # Store Word
.text:823C80D8                 li      %r9, 0x7FE      # Load Immediate
.text:823C80DC                 li      %r10, 0xBFE     # Load Immediate
.text:823C80E0                 stw     %r8, 0x2F5C(%r3) # Store Word
.text:823C80E4                 stw     %r6, 0x2F60(%r3) # Store Word
.text:823C80E8                 li      %r8, 0xFFE      # Load Immediate
.text:823C80EC                 stw     %r5, 0x2F64(%r3) # Store Word
.text:823C80F0                 li      %r6, 0x17FE     # Load Immediate
.text:823C80F4                 stw     %r11, 0x2F68(%r3) # Store Word
.text:823C80F8                 stw     %r7, 0x2F6C(%r3) # Store Word
.text:823C80FC                 stw     %r9, 0x2F70(%r3) # Store Word
.text:823C8100                 stw     %r10, 0x2F74(%r3) # Store Word
.text:823C8104                 lis     %r11, 0 # 0xBFFE # Load Immediate Shifted
.text:823C8108                 stw     %r8, 0x2F78(%r3) # Store Word
.text:823C810C                 lis     %r10, 0 # 0xFFFE # Load Immediate Shifted
.text:823C8110                 stw     %r6, 0x2F7C(%r3) # Store Word
.text:823C8114                 ori     %r11, %r11, -0x4002 # 0xBFFE # OR Immediate
.text:823C8118                 ori     %r10, %r10, -2 # 0xFFFE # OR Immediate
.text:823C811C                 lis     %r9, 1 # 0x17FFE # Load Immediate Shifted
.text:823C8120                 stw     %r11, 0x2F94(%r3) # Store Word
.text:823C8124                 lis     %r8, 1 # 0x1FFFE # Load Immediate Shifted
.text:823C8128                 stw     %r10, 0x2F98(%r3) # Store Word
.text:823C812C                 lis     %r7, 2 # 0x2FFFE # Load Immediate Shifted
.text:823C8130                 ori     %r9, %r9, 0x7FFE # 0x17FFE # OR Immediate
.text:823C8134                 ori     %r11, %r8, -2 # 0x1FFFE # OR Immediate
.text:823C8138                 ori     %r10, %r7, -2 # 0x2FFFE # OR Immediate
.text:823C813C                 stw     %r9, 0x2F9C(%r3) # Store Word
.text:823C8140                 lis     %r6, 3 # 0x3FFFE # Load Immediate Shifted
.text:823C8144                 stw     %r11, 0x2FA0(%r3) # Store Word
.text:823C8148                 lis     %r5, 5 # 0x5FFFE # Load Immediate Shifted
.text:823C814C                 stw     %r10, 0x2FA4(%r3) # Store Word
.text:823C8150                 lis     %r4, 7 # 0x7FFFE # Load Immediate Shifted
.text:823C8154                 ori     %r9, %r6, -2 # 0x3FFFE # OR Immediate
.text:823C8158                 ori     %r11, %r5, -2 # 0x5FFFE # OR Immediate
.text:823C815C                 ori     %r10, %r4, -2 # 0x7FFFE # OR Immediate
.text:823C8160                 stw     %r9, 0x2FA8(%r3) # Store Word
.text:823C8164                 lis     %r31, 9 # 0x9FFFE # Load Immediate Shifted
.text:823C8168                 stw     %r11, 0x2FAC(%r3) # Store Word
.text:823C816C                 lis     %r30, 0xB # 0xBFFFE # Load Immediate Shifted
.text:823C8170                 stw     %r10, 0x2FB0(%r3) # Store Word
.text:823C8174                 lis     %r29, 0xD # 0xDFFFE # Load Immediate Shifted
.text:823C8178                 ori     %r9, %r31, -2 # 0x9FFFE # OR Immediate
.text:823C817C                 ori     %r11, %r30, -2 # 0xBFFFE # OR Immediate
.text:823C8180                 ori     %r10, %r29, -2 # 0xDFFFE # OR Immediate
.text:823C8184                 stw     %r9, 0x2FB4(%r3) # Store Word
.text:823C8188                 lis     %r28, 0xF # 0xFFFFE # Load Immediate Shifted
.text:823C818C                 stw     %r11, 0x2FB8(%r3) # Store Word
.text:823C8190                 lis     %r27, 0x11 # 0x11FFFE # Load Immediate Shifted
.text:823C8194                 stw     %r10, 0x2FBC(%r3) # Store Word
.text:823C8198                 lis     %r26, 0x13 # 0x13FFFE # Load Immediate Shifted
.text:823C819C                 ori     %r9, %r28, -2 # 0xFFFFE # OR Immediate
.text:823C81A0                 ori     %r11, %r27, -2 # 0x11FFFE # OR Immediate
.text:823C81A4                 ori     %r10, %r26, -2 # 0x13FFFE # OR Immediate
.text:823C81A8                 stw     %r9, 0x2FC0(%r3) # Store Word
.text:823C81AC                 lis     %r25, 0x15 # 0x15FFFE # Load Immediate Shifted
.text:823C81B0                 stw     %r11, 0x2FC4(%r3) # Store Word
.text:823C81B4                 lis     %r24, 0x17 # 0x17FFFE # Load Immediate Shifted
.text:823C81B8                 stw     %r10, 0x2FC8(%r3) # Store Word
.text:823C81BC                 lis     %r23, 0x19 # 0x19FFFE # Load Immediate Shifted
.text:823C81C0                 li      %r19, 0x1FFE    # Load Immediate
.text:823C81C4                 li      %r18, 0x2FFE    # Load Immediate
.text:823C81C8                 ori     %r9, %r25, -2 # 0x15FFFE # OR Immediate
.text:823C81CC                 stw     %r19, 0x2F80(%r3) # Store Word
.text:823C81D0                 ori     %r11, %r24, -2 # 0x17FFFE # OR Immediate
.text:823C81D4                 stw     %r18, 0x2F84(%r3) # Store Word
.text:823C81D8                 ori     %r10, %r23, -2 # 0x19FFFE # OR Immediate
.text:823C81DC                 stw     %r9, 0x2FCC(%r3) # Store Word
.text:823C81E0                 lis     %r22, 0x1B # 0x1BFFFE # Load Immediate Shifted
.text:823C81E4                 stw     %r11, 0x2FD0(%r3) # Store Word
.text:823C81E8                 lis     %r21, 0x1D # 0x1DFFFE # Load Immediate Shifted
.text:823C81EC                 stw     %r10, 0x2FD4(%r3) # Store Word
.text:823C81F0                 lis     %r20, 0x1F # 0x1FFFFE # Load Immediate Shifted
.text:823C81F4                 li      %r17, 0x3FFE    # Load Immediate
.text:823C81F8                 li      %r19, 0x5FFE    # Load Immediate
.text:823C81FC                 li      %r18, 0x7FFE    # Load Immediate
.text:823C8200                 stw     %r17, 0x2F88(%r3) # Store Word
.text:823C8204                 ori     %r9, %r22, -2 # 0x1BFFFE # OR Immediate
.text:823C8208                 stw     %r19, 0x2F8C(%r3) # Store Word
.text:823C820C                 ori     %r11, %r21, -2 # 0x1DFFFE # OR Immediate
.text:823C8210                 stw     %r18, 0x2F90(%r3) # Store Word
.text:823C8214                 ori     %r10, %r20, -2 # 0x1FFFFE # OR Immediate
.text:823C8218                 stw     %r9, 0x2FD8(%r3) # Store Word
.text:823C821C                 stw     %r11, 0x2FDC(%r3) # Store Word
.text:823C8220                 stw     %r10, 0x2FE0(%r3) # Store Word
.text:823C8224                 b       __restgprlr_17  # Branch
.text:823C8224 # End of function __compression_store_init_vals__
.text:823C8224



.text:823C8580 # =============== S U B R O U T I N E =======================================
.text:823C8580
.text:823C8580 # r3 = ptr_state_0x14
.text:823C8580 # r4 = buf_U_len
.text:823C8580
.text:823C8580 compression_FFnew_inner_real_kind_final: # CODE XREF: decompress_chunk+58p
.text:823C8580 ptr_state_0x14 = %r31
.text:823C8580 size_left = %r29
.text:823C8580 _constant_0 = %r27
.text:823C8580                 mflr    %r12            # Move from link register
.text:823C8584                 bl      __savegprlr_26  # Branch
.text:823C8588                 stwu    %sp, -0x90(%sp) # Store Word with Update
.text:823C858C                 li      _constant_0, 0  # Load Immediate
.text:823C8590                 mr      ptr_state_0x14, %r3 # Move Register
.text:823C8594                 mr      size_left, %r4  # Move Register
.text:823C8598                 mr      %r28, _constant_0 # r28 = 0
.text:823C859C                 b       compression_FFnew_inner_real_kind_final_start          # Branch
.text:823C85A0 # ---------------------------------------------------------------------------
.text:823C85A0
.text:823C85A0 size_left_neq_0:                        # CODE XREF: compression_FFnew_inner_real_kind_final:loc_823C87B4j
.text:823C85A0                 lwz     %r11, 0x2ED4(ptr_state_0x14) # read 0x2ED4
.text:823C85A4                 cmpwi   cr6, %r11, 1    # Compare Word Immediate
.text:823C85A8                 bne     cr6, loc_823C8780 # Branch if not equal
.text:823C85AC                 lbz     %r11, 0x2EB6(ptr_state_0x14) # Load Byte and Zero
.text:823C85B0                 cmplwi  %r11, 0         # Compare Logical Word Immediate
.text:823C85B4                 beq     loc_823C8600    # Branch if equal
.text:823C85B8                 stb     _constant_0, 0x2EB6(ptr_state_0x14) # 0x2EB6 = 0
.text:823C85BC                 li      %r4, 1          # Load Immediate
.text:823C85C0                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C85C4                 bl      cfunc_read_2_bytes_ext4 # r3 = state
.text:823C85C4                                         # r4 =
.text:823C85C8                 cmplwi  %r3, 0          # Compare Logical Word Immediate
.text:823C85CC                 beq     loc_823C85FC    # prev_val == 0
.text:823C85D0                 li      %r4, 0x10       # Load Immediate
.text:823C85D4                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C85D8                 bl      cfunc_read_2_bytes_ext4 # r3 = state
.text:823C85D8                                         # r4 =
.text:823C85DC                 mr      %r30, %r3       # Move Register
.text:823C85E0                 li      %r4, 0x10       # Load Immediate
.text:823C85E4                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C85E8                 bl      cfunc_read_2_bytes_ext4 # r3 = state
.text:823C85E8                                         # r4 =
.text:823C85EC                 slwi    %r11, %r30, 16  # Shift Left Immediate
.text:823C85F0                 or      %r11, %r3, %r11 # OR
.text:823C85F4                 stw     %r11, 0x2EBC(ptr_state_0x14) # readed 32 bits value?
.text:823C85F8                 b       loc_823C8600    # Branch
.text:823C85FC # ---------------------------------------------------------------------------
.text:823C85FC
.text:823C85FC loc_823C85FC:                           # CODE XREF: compression_FFnew_inner_real_kind_final+4Cj
.text:823C85FC                 stw     _constant_0, 0x2EBC(ptr_state_0x14) # prev_val == 0
.text:823C8600
.text:823C8600 loc_823C8600:                           # CODE XREF: compression_FFnew_inner_real_kind_final+34j
.text:823C8600                                         # compression_FFnew_inner_real_kind_final+78j
.text:823C8600                 lwz     %r11, 0x2ED0(ptr_state_0x14) # Load Word and Zero
.text:823C8604                 cmpwi   cr6, %r11, 3    # Compare Word Immediate
.text:823C8608                 bne     cr6, loc_823C8618 # Branch if not equal
.text:823C860C                 stw     _constant_0, 0x2ED0(ptr_state_0x14) # Store Word
.text:823C8610                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C8614                 bl      cfunc_read_4_bytes # Branch
.text:823C8618
.text:823C8618 loc_823C8618:                           # CODE XREF: compression_FFnew_inner_real_kind_final+88j
.text:823C8618                 li      %r4, 3          # Load Immediate
.text:823C861C                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C8620                 bl      cfunc_read_2_bytes_ext4 # r3 = state
.text:823C8620                                         # r4 =
.text:823C8624                 stw     %r3, 0x2ED0(ptr_state_0x14) # Store Word
.text:823C8628                 li      %r4, 8          # Load Immediate
.text:823C862C                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C8630                 bl      cfunc_read_2_bytes_ext4 # r3 = state
.text:823C8630                                         # r4 =
.text:823C8634                 mr      %r26, %r3       # Move Register
.text:823C8638                 li      %r4, 8          # Load Immediate
.text:823C863C                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C8640                 bl      cfunc_read_2_bytes_ext4 # r3 = state
.text:823C8640                                         # r4 =
.text:823C8644                 mr      %r30, %r3       # Move Register
.text:823C8648                 li      %r4, 8          # Load Immediate
.text:823C864C                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C8650                 bl      cfunc_read_2_bytes_ext4 # r3 = state
.text:823C8650                                         # r4 =
.text:823C8654                 slwi    %r11, %r26, 8   # Shift Left Immediate
.text:823C8658                 lwz     %r10, 0x2ED0(ptr_state_0x14) # Load Word and Zero
.text:823C865C                 add     %r11, %r11, %r30 # Add
.text:823C8660                 cmpwi   cr6, %r10, 2    # Compare Word Immediate
.text:823C8664                 slwi    %r11, %r11, 8   # Shift Left Immediate
.text:823C8668                 add     %r11, %r3, %r11 # Add
.text:823C866C                 stw     %r11, 0x2EC8(ptr_state_0x14) # Store Word
.text:823C8670                 stw     %r11, 0x2ECC(ptr_state_0x14) # Store Word
.text:823C8674                 bne     cr6, loc_823C8680 # Branch if not equal
.text:823C8678                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C867C                 bl      sub_823CA130    # Branch
.text:823C8680
.text:823C8680 loc_823C8680:                           # CODE XREF: compression_FFnew_inner_real_kind_final+F4j
.text:823C8680                 lwz     %r11, 0x2ED0(ptr_state_0x14) # Load Word and Zero
.text:823C8684                 cmpwi   cr6, %r11, 1    # Compare Word Immediate
.text:823C8688                 beq     cr6, loc_823C86B4 # Branch if equal
.text:823C868C                 cmpwi   cr6, %r11, 2    # Compare Word Immediate
.text:823C8690                 beq     cr6, loc_823C86B4 # Branch if equal
.text:823C8694                 cmpwi   cr6, %r11, 3    # Compare Word Immediate
.text:823C8698                 bne     cr6, loc_823C86AC # Branch if not equal
.text:823C869C                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C86A0                 bl      sub_823C8B20    # Branch
.text:823C86A4                 clrlwi. %r11, %r3, 24   # Clear Left Immediate
.text:823C86A8                 bne     loc_823C86E4    # Branch if not equal
.text:823C86AC
.text:823C86AC loc_823C86AC:                           # CODE XREF: compression_FFnew_inner_real_kind_final+118j
.text:823C86AC                                         # compression_FFnew_inner_real_kind_final+18Cj ...
.text:823C86AC                 li      %r3, -1         # Load Immediate
.text:823C86B0                 b       loc_823C87E4    # Branch
.text:823C86B4 # ---------------------------------------------------------------------------
.text:823C86B4
.text:823C86B4 loc_823C86B4:                           # CODE XREF: compression_FFnew_inner_real_kind_final+108j
.text:823C86B4                                         # compression_FFnew_inner_real_kind_final+110j
.text:823C86B4                 lbz     %r11, 0x2EB5(ptr_state_0x14) # Load Byte and Zero
.text:823C86B8                 addi    %r4, ptr_state_0x14, 0xA18 # Add Immediate
.text:823C86BC                 addi    %r3, ptr_state_0x14, 0x2B14 # Add Immediate
.text:823C86C0                 addi    %r11, %r11, 0x20 # Add Immediate
.text:823C86C4                 slwi    %r5, %r11, 3    # Shift Left Immediate
.text:823C86C8                 bl      memcpy          # r3 = dst_ptr
.text:823C86C8                                         # r4 = src_ptr
.text:823C86C8                                         # r5 = length
.text:823C86CC                 li      %r5, 0xF9       # Load Immediate
.text:823C86D0                 addi    %r4, ptr_state_0x14, 0xCB8 # Add Immediate
.text:823C86D4                 addi    %r3, ptr_state_0x14, 0x2DB4 # Add Immediate
.text:823C86D8                 bl      memcpy          # r3 = dst_ptr
.text:823C86D8                                         # r4 = src_ptr
.text:823C86D8                                         # r5 = length
.text:823C86DC                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C86E0                 bl      sub_823CA040    # Branch
.text:823C86E4
.text:823C86E4 loc_823C86E4:                           # CODE XREF: compression_FFnew_inner_real_kind_final+128j
.text:823C86E4                 li      %r11, 2         # Load Immediate
.text:823C86E8                 stw     %r11, 0x2ED4(ptr_state_0x14) # Store Word
.text:823C86EC                 b       loc_823C8780    # Branch
.text:823C86F0 # ---------------------------------------------------------------------------
.text:823C86F0
.text:823C86F0 loc_823C86F0:                           # CODE XREF: compression_FFnew_inner_real_kind_final+208j
.text:823C86F0                 cmpwi   cr6, size_left, 0 # Compare Word Immediate
.text:823C86F4                 ble     cr6, loc_823C878C # Branch if less than or equal
.text:823C86F8                 lwz     %r30, 0x2ECC(ptr_state_0x14) # Load Word and Zero
.text:823C86FC                 cmpw    cr6, %r30, size_left # Compare Word
.text:823C8700                 blt     cr6, loc_823C8708 # Branch if less than
.text:823C8704                 mr      %r30, size_left # Move Register
.text:823C8708
.text:823C8708 loc_823C8708:                           # CODE XREF: compression_FFnew_inner_real_kind_final+180j
.text:823C8708                 cmplwi  cr6, %r30, 0    # Compare Logical Word Immediate
.text:823C870C                 beq     cr6, loc_823C86AC # Branch if equal
.text:823C8710                 lwz     %r11, 0x2ED0(ptr_state_0x14) # Load Word and Zero
.text:823C8714                 lwz     %r4, 0x2EB8(ptr_state_0x14) # Load Word and Zero
.text:823C8718                 cmpwi   cr6, %r11, 2    # Compare Word Immediate
.text:823C871C                 bne     cr6, loc_823C8730 # Branch if not equal
.text:823C8720                 mr      %r5, %r30       # Move Register
.text:823C8724                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C8728                 bl      sub_823C99E0    # Branch
.text:823C872C                 b       loc_823C8764    # Branch
.text:823C8730 # ---------------------------------------------------------------------------
.text:823C8730
.text:823C8730 loc_823C8730:                           # CODE XREF: compression_FFnew_inner_real_kind_final+19Cj
.text:823C8730                 cmpwi   cr6, %r11, 1    # Compare Word Immediate
.text:823C8734                 bne     cr6, loc_823C8748 # Branch if not equal
.text:823C8738                 mr      %r5, %r30       # Move Register
.text:823C873C                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C8740                 bl      sub_823C91D0    # Branch
.text:823C8744                 b       loc_823C8764    # Branch
.text:823C8748 # ---------------------------------------------------------------------------
.text:823C8748
.text:823C8748 loc_823C8748:                           # CODE XREF: compression_FFnew_inner_real_kind_final+1B4j
.text:823C8748                 cmpwi   cr6, %r11, 3    # Compare Word Immediate
.text:823C874C                 bne     cr6, loc_823C8760 # Branch if not equal
.text:823C8750                 mr      %r5, %r30       # Move Register
.text:823C8754                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C8758                 bl      loc_823C8960    # Branch
.text:823C875C                 b       loc_823C8764    # Branch
.text:823C8760 # ---------------------------------------------------------------------------
.text:823C8760
.text:823C8760 loc_823C8760:                           # CODE XREF: compression_FFnew_inner_real_kind_final+1CCj
.text:823C8760                 li      %r3, -1         # Load Immediate
.text:823C8764
.text:823C8764 loc_823C8764:                           # CODE XREF: compression_FFnew_inner_real_kind_final+1ACj
.text:823C8764                                         # compression_FFnew_inner_real_kind_final+1C4j ...
.text:823C8764                 cmpwi   cr6, %r3, 0     # Compare Word Immediate
.text:823C8768                 bne     cr6, loc_823C86AC # Branch if not equal
.text:823C876C                 lwz     %r11, 0x2ECC(ptr_state_0x14) # Load Word and Zero
.text:823C8770                 subf    size_left, %r30, size_left # size_left -= xxx
.text:823C8774                 add     %r28, %r30, %r28 # Add
.text:823C8778                 subf    %r11, %r30, %r11 # Subtract from
.text:823C877C                 stw     %r11, 0x2ECC(ptr_state_0x14) # Store Word
.text:823C8780
.text:823C8780 loc_823C8780:                           # CODE XREF: compression_FFnew_inner_real_kind_final+28j
.text:823C8780                                         # compression_FFnew_inner_real_kind_final+16Cj
.text:823C8780                 lwz     %r11, 0x2ECC(ptr_state_0x14) # Load Word and Zero
.text:823C8784                 cmpwi   cr6, %r11, 0    # Compare Word Immediate
.text:823C8788                 bgt     cr6, loc_823C86F0 # Branch if greater than
.text:823C878C
.text:823C878C loc_823C878C:                           # CODE XREF: compression_FFnew_inner_real_kind_final+174j
.text:823C878C                 lwz     %r11, 0x2ECC(ptr_state_0x14) # Load Word and Zero
.text:823C8790                 cmpwi   cr6, %r11, 0    # Compare Word Immediate
.text:823C8794                 bne     cr6, loc_823C87A0 # Branch if not equal
.text:823C8798                 li      %r11, 1         # Load Immediate
.text:823C879C                 stw     %r11, 0x2ED4(ptr_state_0x14) # Store Word
.text:823C87A0
.text:823C87A0 loc_823C87A0:                           # CODE XREF: compression_FFnew_inner_real_kind_final+214j
.text:823C87A0                 cmpwi   cr6, size_left, 0 # Compare Word Immediate
.text:823C87A4                 bne     cr6, loc_823C87B4 # Branch if not equal
.text:823C87A8                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C87AC                 bl      cfunc_read_4_bytes # Branch
.text:823C87B0
.text:823C87B0 compression_FFnew_inner_real_kind_final_start:                                 # CODE XREF: compression_FFnew_inner_real_kind_final+1Cj
.text:823C87B0                 cmpwi   cr6, size_left, 0 # Compare Word Immediate
.text:823C87B4
.text:823C87B4 loc_823C87B4:                           # CODE XREF: compression_FFnew_inner_real_kind_final+224j
.text:823C87B4                 bgt     cr6, size_left_neq_0 # read 0x2ED4
.text:823C87B8                 lwz     %r11, 0x2EB8(ptr_state_0x14) # Load Word and Zero
.text:823C87BC                 lwz     %r10, 0(ptr_state_0x14) # Load Word and Zero
.text:823C87C0                 cmpwi   cr6, %r11, 0    # Compare Word Immediate
.text:823C87C4                 bne     cr6, loc_823C87CC # Branch if not equal
.text:823C87C8                 lwz     %r11, 4(ptr_state_0x14) # Load Word and Zero
.text:823C87CC
.text:823C87CC loc_823C87CC:                           # CODE XREF: compression_FFnew_inner_real_kind_final+244j
.text:823C87CC                 subf    %r11, %r28, %r11 # Subtract from
.text:823C87D0                 mr      %r4, %r28       # Move Register
.text:823C87D4                 add     %r5, %r11, %r10 # Add
.text:823C87D8                 mr      %r3, ptr_state_0x14 # Move Register
.text:823C87DC                 bl      sub_823C9A50    # Branch
.text:823C87E0                 mr      %r3, %r28       # Move Register
.text:823C87E4
.text:823C87E4 loc_823C87E4:                           # CODE XREF: compression_FFnew_inner_real_kind_final+130j
.text:823C87E4                 addi    %sp, %sp, 0x90  # Add Immediate
.text:823C87E8                 b       __restgprlr_26  # Branch
.text:823C87E8 # End of function compression_FFnew_inner_real_kind_final
.text:823C87E8
.text:823C87E8 # ---------------------------------------------------------------------------





.text:823C8928
.text:823C8928 # =============== S U B R O U T I N E =======================================
.text:823C8928
.text:823C8928 # r3 = state
.text:823C8928 # r4 =
.text:823C8928
.text:823C8928 cfunc_read_2_bytes_ext4:                # CODE XREF: compression_FFnew_inner_real_kind_final+44p
.text:823C8928                                         # compression_FFnew_inner_real_kind_final+58p ...
.text:823C8928
.text:823C8928 .set var_10, -0x10
.text:823C8928 .set var_8, -8
.text:823C8928
.text:823C8928                 mflr    %r12            # Move from link register
.text:823C892C                 stw     %r12, var_8(%sp) # Store Word
.text:823C8930                 std     %r31, var_10(%sp) # Store Double Word
.text:823C8934                 stwu    %sp, -0x60(%sp) # Store Word with Update
.text:823C8938                 lwz     %r11, 0x2EB0(%r3) # Load Word and Zero
.text:823C893C                 subfic  %r10, %r4, 0x20 # Subtract from Immediate Carrying
.text:823C8940                 srw     %r31, %r11, %r10 # Shift Right Word
.text:823C8944                 bl      cfunc_read_2_bytes_ext4_real # r3 = state
.text:823C8944                                         # r4 =
.text:823C8948                 mr      %r3, %r31       # Move Register
.text:823C894C                 addi    %sp, %sp, 0x60  # Add Immediate
.text:823C8950                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823C8954                 mtlr    %r12            # Move to link register
.text:823C8958                 ld      %r31, var_10(%sp) # Load Double Word
.text:823C895C                 blr                     # Branch unconditionally
.text:823C895C # End of function cfunc_read_2_bytes_ext4
.text:823C895C






.text:823C8858
.text:823C8858 # =============== S U B R O U T I N E =======================================
.text:823C8858
.text:823C8858 # r3 = state
.text:823C8858 # r4 =
.text:823C8858
.text:823C8858 cfunc_read_2_bytes_ext4_real:           # CODE XREF: cfunc_read_2_bytes_ext4+1Cp
.text:823C8858                 lbz     %r11, 0x2EB4(%r3) # Load Byte and Zero
.text:823C885C                 lwz     %r10, 0x2EB0(%r3) # Load Word and Zero
.text:823C8860                 subf    %r11, %r4, %r11 # Subtract from
.text:823C8864                 slw     %r9, %r10, %r4  # Shift Left Word
.text:823C8868                 extsb.  %r11, %r11      # Extend Sign Byte
.text:823C886C                 stw     %r9, 0x2EB0(%r3) # Store Word
.text:823C8870                 stb     %r11, 0x2EB4(%r3) # Store Byte
.text:823C8874                 bgtlr                   # Branch if greater than
.text:823C8878                 lwz     %r8, 0x2B08(%r3) # Load Word and Zero
.text:823C887C                 lwz     %r10, 0x2B04(%r3) # Load Word and Zero
.text:823C8880                 cmplw   cr6, %r10, %r8  # Compare Logical Word
.text:823C8884                 bge     cr6, loc_823C88D4 # Branch if greater than or equal
.text:823C8888                 lbz     %r6, 0x2EB4(%r3) # Load Byte and Zero
.text:823C888C                 addi    %r11, %r10, 2   # buf_C_ptr += 2;
.text:823C8890                 lbz     %r7, 1(%r10)    # Load Byte and Zero
.text:823C8894                 extsb   %r5, %r6        # Extend Sign Byte
.text:823C8898                 lbz     %r4, 0(%r10)    # Load Byte and Zero
.text:823C889C                 rotlwi  %r7, %r7, 8     # Rotate Left Immediate
.text:823C88A0                 stw     %r11, 0x2B04(%r3) # Store Word
.text:823C88A4                 extsb   %r6, %r6        # Extend Sign Byte
.text:823C88A8                 or      %r7, %r7, %r4   # OR
.text:823C88AC                 neg     %r6, %r6        # Negate
.text:823C88B0                 addi    %r10, %r5, 0x10 # Add Immediate
.text:823C88B4                 slw     %r7, %r7, %r6   # Shift Left Word
.text:823C88B8                 extsb.  %r10, %r10      # Extend Sign Byte
.text:823C88BC                 or      %r9, %r7, %r9   # OR
.text:823C88C0                 stb     %r10, 0x2EB4(%r3) # Store Byte
.text:823C88C4                 stw     %r9, 0x2EB0(%r3) # Store Word
.text:823C88C8                 bgtlr                   # Branch if greater than
.text:823C88CC                 cmplw   cr6, %r11, %r8  # Compare Logical Word
.text:823C88D0                 blt     cr6, read_if_more_data # Branch if less than
.text:823C88D4
.text:823C88D4 loc_823C88D4:                           # CODE XREF: cfunc_read_2_bytes_ext4_real+2Cj
.text:823C88D4                 li      %r11, 1         # Load Immediate
.text:823C88D8                 stb     %r11, 0x2EB7(%r3) # Store Byte
.text:823C88DC                 blr                     # Branch unconditionally
.text:823C88E0 # ---------------------------------------------------------------------------
.text:823C88E0
.text:823C88E0 read_if_more_data:                      # CODE XREF: cfunc_read_2_bytes_ext4_real+78j
.text:823C88E0                 lbz     %r8, 0x2EB4(%r3) # Load Byte and Zero
.text:823C88E4                 addi    %r9, %r11, 2    # buf_C_ptr += 2
.text:823C88E8                 lbz     %r10, 1(%r11)   # Load Byte and Zero
.text:823C88EC                 extsb   %r7, %r8        # Extend Sign Byte
.text:823C88F0                 lbz     %r11, 0(%r11)   # Load Byte and Zero
.text:823C88F4                 rotlwi  %r10, %r10, 8   # Rotate Left Immediate
.text:823C88F8                 lwz     %r6, 0x2EB0(%r3) # Load Word and Zero
.text:823C88FC                 extsb   %r8, %r8        # Extend Sign Byte
.text:823C8900                 stw     %r9, 0x2B04(%r3) # Store Word
.text:823C8904                 or      %r10, %r10, %r11 # OR
.text:823C8908                 neg     %r8, %r8        # Negate
.text:823C890C                 addi    %r11, %r7, 0x10 # Add Immediate
.text:823C8910                 slw     %r10, %r10, %r8 # Shift Left Word
.text:823C8914                 or      %r10, %r10, %r6 # OR
.text:823C8918                 stb     %r11, 0x2EB4(%r3) # Store Byte
.text:823C891C                 stw     %r10, 0x2EB0(%r3) # Store Word
.text:823C8920                 blr                     # Branch unconditionally
.text:823C8920 # End of function cfunc_read_2_bytes_ext4_real
.text:823C8920
.text:823C8920 # ---------------------------------------------------------------------------
.text:823C8924










.text:823CA130
.text:823CA130 # =============== S U B R O U T I N E =======================================
.text:823CA130
.text:823CA130
.text:823CA130 sub_823CA130:                           # CODE XREF: compression_FFnew_inner_real_kind_final+FCp
.text:823CA130                 mflr    %r12            # Move from link register
.text:823CA134                 bl      __savegprlr_29  # Branch
.text:823CA138                 stwu    %sp, -0x70(%sp) # Store Word with Update
.text:823CA13C                 mr      %r31, %r3       # Move Register
.text:823CA140                 li      %r30, 0         # Load Immediate
.text:823CA144                 addi    %r29, %r31, 0xE34 # Add Immediate
.text:823CA148
.text:823CA148 loc_823CA148:                           # CODE XREF: sub_823CA130+30j
.text:823CA148                 li      %r4, 3          # Load Immediate
.text:823CA14C                 mr      %r3, %r31       # Move Register
.text:823CA150                 bl      cfunc_read_2_bytes_ext4 # r3 = state
.text:823CA150                                         # r4 =
.text:823CA154                 stbx    %r3, %r29, %r30 # Store Byte Indexed
.text:823CA158                 addi    %r30, %r30, 1   # Add Immediate
.text:823CA15C                 cmpwi   cr6, %r30, 8    # Compare Word Immediate
.text:823CA160                 blt     cr6, loc_823CA148 # Branch if less than
.text:823CA164                 lbz     %r11, 0x2EB7(%r31) # Load Byte and Zero
.text:823CA168                 cmplwi  %r11, 0         # Compare Logical Word Immediate
.text:823CA16C                 beq     loc_823CA178    # Branch if equal
.text:823CA170                 li      %r3, 0          # Load Immediate
.text:823CA174                 b       loc_823CA19C    # Branch
.text:823CA178 # ---------------------------------------------------------------------------
.text:823CA178
.text:823CA178 loc_823CA178:                           # CODE XREF: sub_823CA130+3Cj
.text:823CA178                 addi    %r5, %r31, 0xDB4 # Add Immediate
.text:823CA17C                 mr      %r4, %r29       # Move Register
.text:823CA180                 mr      %r3, %r31       # Move Register
.text:823CA184                 bl      sub_823CA470    # Branch
.text:823CA188                 clrlwi  %r11, %r3, 24   # Clear Left Immediate
.text:823CA18C                 addi    %r11, %r11, 0   # Add Immediate
.text:823CA190                 cntlzw  %r11, %r11      # Count Leading Zeros Word
.text:823CA194                 extrwi  %r11, %r11, 1,26 # Extract and Right Justify Immediate
.text:823CA198                 xori    %r3, %r11, 1    # XOR Immediate
.text:823CA19C
.text:823CA19C loc_823CA19C:                           # CODE XREF: sub_823CA130+44j
.text:823CA19C                 addi    %sp, %sp, 0x70  # Add Immediate
.text:823CA1A0                 b       __restgprlr_29  # Branch
.text:823CA1A0 # End of function sub_823CA130
.text:823CA1A0




.text:823CA470
.text:823CA470 # =============== S U B R O U T I N E =======================================
.text:823CA470
.text:823CA470
.text:823CA470 sub_823CA470:                           # CODE XREF: sub_823CA130+54p
.text:823CA470
.text:823CA470 .set var_80, -0x80
.text:823CA470 .set var_50, -0x50
.text:823CA470 .set var_4E, -0x4E
.text:823CA470 .set var_2E, -0x2E
.text:823CA470
.text:823CA470                 mflr    %r12            # Move from link register
.text:823CA474                 bl      __savegprlr_29  # Branch
.text:823CA478                 stwu    %sp, -0xD0(%sp) # Store Word with Update
.text:823CA47C                 li      %r7, 1          # Load Immediate
.text:823CA480                 mr      %r30, %r4       # Move Register
.text:823CA484                 mr      %r29, %r5       # Move Register
.text:823CA488                 mr      %r11, %r7       # Move Register
.text:823CA48C                 li      %r31, 0         # Load Immediate
.text:823CA490
.text:823CA490 loc_823CA490:                           # CODE XREF: sub_823CA470+38j
.text:823CA490                 slwi    %r9, %r11, 1    # Shift Left Immediate
.text:823CA494                 addi    %r10, %r11, 1   # Add Immediate
.text:823CA498                 addi    %r8, %sp, 0xD0+var_80 # Add Immediate
.text:823CA49C                 clrlwi  %r11, %r10, 16  # Clear Left Immediate
.text:823CA4A0                 cmplwi  cr6, %r11, 0x10 # Compare Logical Word Immediate
.text:823CA4A4                 sthx    %r31, %r9, %r8  # Store Half Word Indexed
.text:823CA4A8                 ble     cr6, loc_823CA490 # Branch if less than or equal
.text:823CA4AC                 mr      %r11, %r31      # Move Register
.text:823CA4B0
.text:823CA4B0 loc_823CA4B0:                           # CODE XREF: sub_823CA470+64j
.text:823CA4B0                 lbzx    %r9, %r11, %r30 # Load Byte and Zero Indexed
.text:823CA4B4                 addi    %r10, %sp, 0xD0+var_80 # Add Immediate
.text:823CA4B8                 addi    %r11, %r11, 1   # Add Immediate
.text:823CA4BC                 rotlwi  %r9, %r9, 1     # Rotate Left Immediate
.text:823CA4C0                 clrlwi  %r11, %r11, 16  # Clear Left Immediate
.text:823CA4C4                 cmplwi  cr6, %r11, 8    # Compare Logical Word Immediate
.text:823CA4C8                 lhzx    %r8, %r9, %r10  # Load Half Word and Zero Indexed
.text:823CA4CC                 addi    %r8, %r8, 1     # Add Immediate
.text:823CA4D0                 sthx    %r8, %r9, %r10  # Store Half Word Indexed
.text:823CA4D4                 blt     cr6, loc_823CA4B0 # Branch if less than
.text:823CA4D8                 mr      %r11, %r7       # Move Register
.text:823CA4DC                 sth     %r31, 0xD0+var_4E(%sp) # Store Half Word
.text:823CA4E0
.text:823CA4E0 loc_823CA4E0:                           # CODE XREF: sub_823CA470+A4j
.text:823CA4E0                 slwi    %r8, %r11, 1    # Shift Left Immediate
.text:823CA4E4                 addi    %r10, %sp, 0xD0+var_80 # Add Immediate
.text:823CA4E8                 addi    %r9, %sp, 0xD0+var_50 # Add Immediate
.text:823CA4EC                 subfic  %r6, %r11, 0x10 # Subtract from Immediate Carrying
.text:823CA4F0                 addi    %r11, %r11, 1   # Add Immediate
.text:823CA4F4                 addi    %r5, %sp, 0xD0+var_4E # Add Immediate
.text:823CA4F8                 lhzx    %r10, %r8, %r10 # Load Half Word and Zero Indexed
.text:823CA4FC                 clrlwi  %r11, %r11, 16  # Clear Left Immediate
.text:823CA500                 lhzx    %r9, %r8, %r9   # Load Half Word and Zero Indexed
.text:823CA504                 slw     %r10, %r10, %r6 # Shift Left Word
.text:823CA508                 add     %r10, %r10, %r9 # Add
.text:823CA50C                 cmplwi  cr6, %r11, 0x10 # Compare Logical Word Immediate
.text:823CA510                 sthx    %r10, %r8, %r5  # Store Half Word Indexed
.text:823CA514                 ble     cr6, loc_823CA4E0 # Branch if less than or equal
.text:823CA518                 lhz     %r11, 0xD0+var_2E(%sp) # Load Half Word and Zero
.text:823CA51C                 cmplwi  %r11, 0         # Compare Logical Word Immediate
.text:823CA520                 beq     loc_823CA52C    # Branch if equal
.text:823CA524
.text:823CA524 loc_823CA524:                           # CODE XREF: sub_823CA470+168j
.text:823CA524                 li      %r3, 0          # Load Immediate
.text:823CA528                 b       loc_823CA618    # Branch
.text:823CA52C # ---------------------------------------------------------------------------
.text:823CA52C
.text:823CA52C loc_823CA52C:                           # CODE XREF: sub_823CA470+B0j
.text:823CA52C                 mr      %r11, %r7       # Move Register
.text:823CA530
.text:823CA530 loc_823CA530:                           # CODE XREF: sub_823CA470+F0j
.text:823CA530                 slwi    %r10, %r11, 1   # Shift Left Immediate
.text:823CA534                 addi    %r9, %sp, 0xD0+var_50 # Add Immediate
.text:823CA538                 subfic  %r8, %r11, 7    # Subtract from Immediate Carrying
.text:823CA53C                 addi    %r11, %r11, 1   # Add Immediate
.text:823CA540                 addi    %r5, %sp, 0xD0+var_80 # Add Immediate
.text:823CA544                 slw     %r8, %r7, %r8   # Shift Left Word
.text:823CA548                 lhzx    %r6, %r10, %r9  # Load Half Word and Zero Indexed
.text:823CA54C                 clrlwi  %r11, %r11, 16  # Clear Left Immediate
.text:823CA550                 sthx    %r8, %r10, %r5  # Store Half Word Indexed
.text:823CA554                 srwi    %r6, %r6, 9     # Shift Right Immediate
.text:823CA558                 cmplwi  cr6, %r11, 7    # Compare Logical Word Immediate
.text:823CA55C                 sthx    %r6, %r10, %r9  # Store Half Word Indexed
.text:823CA560                 ble     cr6, loc_823CA530 # Branch if less than or equal
.text:823CA564                 clrlwi  %r11, %r11, 16  # Clear Left Immediate
.text:823CA568                 cmplwi  cr6, %r11, 0x10 # Compare Logical Word Immediate
.text:823CA56C                 bgt     cr6, loc_823CA594 # Branch if greater than
.text:823CA570
.text:823CA570 loc_823CA570:                           # CODE XREF: sub_823CA470+120j
.text:823CA570                 subfic  %r10, %r11, 0x10 # Subtract from Immediate Carrying
.text:823CA574                 slwi    %r8, %r11, 1    # Shift Left Immediate
.text:823CA578                 addi    %r9, %r11, 1    # Add Immediate
.text:823CA57C                 addi    %r6, %sp, 0xD0+var_80 # Add Immediate
.text:823CA580                 slw     %r10, %r7, %r10 # Shift Left Word
.text:823CA584                 clrlwi  %r11, %r9, 16   # Clear Left Immediate
.text:823CA588                 sthx    %r10, %r8, %r6  # Store Half Word Indexed
.text:823CA58C                 cmplwi  cr6, %r11, 0x10 # Compare Logical Word Immediate
.text:823CA590                 ble     cr6, loc_823CA570 # Branch if less than or equal
.text:823CA594
.text:823CA594 loc_823CA594:                           # CODE XREF: sub_823CA470+FCj
.text:823CA594                 li      %r5, 0x80       # Load Immediate
.text:823CA598                 li      %r4, 0          # Load Immediate
.text:823CA59C                 mr      %r3, %r29       # Move Register
.text:823CA5A0                 bl      memset          # r3 = ptr
.text:823CA5A0                                         # r4 = byte
.text:823CA5A0                                         # r5 = count
.text:823CA5A4                 mr      %r6, %r31       # Move Register
.text:823CA5A8                 mr      %r5, %r31       # Move Register
.text:823CA5AC
.text:823CA5AC loc_823CA5AC:                           # CODE XREF: sub_823CA470+1A0j
.text:823CA5AC                 lbzx    %r11, %r5, %r30 # Load Byte and Zero Indexed
.text:823CA5B0                 cmplwi  %r11, 0         # Compare Logical Word Immediate
.text:823CA5B4                 beq     loc_823CA600    # Branch if equal
.text:823CA5B8                 slwi    %r9, %r11, 1    # Shift Left Immediate
.text:823CA5BC                 addi    %r7, %sp, 0xD0+var_50 # Add Immediate
.text:823CA5C0                 addi    %r10, %sp, 0xD0+var_80 # Add Immediate
.text:823CA5C4                 lhzx    %r11, %r9, %r7  # Load Half Word and Zero Indexed
.text:823CA5C8                 lhzx    %r10, %r9, %r10 # Load Half Word and Zero Indexed
.text:823CA5CC                 add     %r10, %r10, %r11 # Add
.text:823CA5D0                 clrlwi  %r10, %r10, 16  # Clear Left Immediate
.text:823CA5D4                 cmplwi  cr6, %r10, 0x80 # Compare Logical Word Immediate
.text:823CA5D8                 bgt     cr6, loc_823CA524 # Branch if greater than
.text:823CA5DC                 clrlwi  %r11, %r11, 16  # Clear Left Immediate
.text:823CA5E0                 cmplw   cr6, %r11, %r10 # Compare Logical Word
.text:823CA5E4                 bge     cr6, loc_823CA5FC # Branch if greater than or equal
.text:823CA5E8
.text:823CA5E8 loc_823CA5E8:                           # CODE XREF: sub_823CA470+188j
.text:823CA5E8                 addi    %r8, %r11, 1    # Add Immediate
.text:823CA5EC                 stbx    %r6, %r11, %r29 # Store Byte Indexed
.text:823CA5F0                 clrlwi  %r11, %r8, 16   # Clear Left Immediate
.text:823CA5F4                 cmplw   cr6, %r11, %r10 # Compare Logical Word
.text:823CA5F8                 blt     cr6, loc_823CA5E8 # Branch if less than
.text:823CA5FC
.text:823CA5FC loc_823CA5FC:                           # CODE XREF: sub_823CA470+174j
.text:823CA5FC                 sthx    %r10, %r9, %r7  # Store Half Word Indexed
.text:823CA600
.text:823CA600 loc_823CA600:                           # CODE XREF: sub_823CA470+144j
.text:823CA600                 addi    %r11, %r5, 1    # Add Immediate
.text:823CA604                 clrlwi  %r6, %r11, 24   # Clear Left Immediate
.text:823CA608                 mr      %r5, %r6        # Move Register
.text:823CA60C                 cmplwi  cr6, %r6, 8     # Compare Logical Word Immediate
.text:823CA610                 blt     cr6, loc_823CA5AC # Branch if less than
.text:823CA614                 li      %r3, 1          # Load Immediate
.text:823CA618
.text:823CA618 loc_823CA618:                           # CODE XREF: sub_823CA470+B8j
.text:823CA618                 addi    %sp, %sp, 0xD0  # Add Immediate
.text:823CA61C                 b       __restgprlr_29  # Branch
.text:823CA61C # End of function sub_823CA470



.text:823CA040
.text:823CA040 # =============== S U B R O U T I N E =======================================
.text:823CA040
.text:823CA040
.text:823CA040 sub_823CA040:                           # CODE XREF: compression_FFnew_inner_real_kind_final+160p
.text:823CA040
.text:823CA040 .set var_18, -0x18
.text:823CA040 .set var_10, -0x10
.text:823CA040 .set var_8, -8
.text:823CA040
.text:823CA040                 mflr    %r12            # Move from link register
.text:823CA044                 stw     %r12, var_8(%sp) # Store Word
.text:823CA048                 std     %r30, var_18(%sp) # Store Double Word
.text:823CA04C                 std     %r31, var_10(%sp) # Store Double Word
.text:823CA050                 stwu    %sp, -0x70(%sp) # Store Word with Update
.text:823CA054                 mr      %r31, %r3       # Move Register
.text:823CA058                 li      %r4, 0x100      # Load Immediate
.text:823CA05C                 addi    %r30, %r31, 0xA18 # Add Immediate
.text:823CA060                 addi    %r5, %r31, 0x2B14 # Add Immediate
.text:823CA064                 mr      %r6, %r30       # Move Register
.text:823CA068                 bl      sub_823C9B00    # Branch
.text:823CA06C                 clrlwi. %r11, %r3, 24   # Clear Left Immediate
.text:823CA070                 bne     loc_823CA07C    # Branch if not equal
.text:823CA074
.text:823CA074 loc_823CA074:                           # CODE XREF: sub_823CA040+58j
.text:823CA074                                         # sub_823CA040+84j ...
.text:823CA074                 li      %r3, 0          # Load Immediate
.text:823CA078                 b       loc_823CA118    # Branch
.text:823CA07C # ---------------------------------------------------------------------------
.text:823CA07C
.text:823CA07C loc_823CA07C:                           # CODE XREF: sub_823CA040+30j
.text:823CA07C                 lbz     %r11, 0x2EB5(%r31) # Load Byte and Zero
.text:823CA080                 addi    %r6, %r31, 0xB18 # Add Immediate
.text:823CA084                 addi    %r5, %r31, 0x2C14 # Add Immediate
.text:823CA088                 mr      %r3, %r31       # Move Register
.text:823CA08C                 rotlwi  %r4, %r11, 3    # Rotate Left Immediate
.text:823CA090                 bl      sub_823C9B00    # Branch
.text:823CA094                 clrlwi. %r11, %r3, 24   # Clear Left Immediate
.text:823CA098                 beq     loc_823CA074    # Branch if equal
.text:823CA09C                 lbz     %r11, 0x2EB5(%r31) # Load Byte and Zero
.text:823CA0A0                 addi    %r8, %r31, 0xE3C # Add Immediate
.text:823CA0A4                 addi    %r7, %r31, 0x18 # Add Immediate
.text:823CA0A8                 addi    %r11, %r11, 0x20 # Add Immediate
.text:823CA0AC                 li      %r6, 0xA        # Load Immediate
.text:823CA0B0                 mr      %r5, %r30       # Move Register
.text:823CA0B4                 slwi    %r4, %r11, 3    # Shift Left Immediate
.text:823CA0B8                 mr      %r3, %r31       # Move Register
.text:823CA0BC                 bl      compression_innermost #
.text:823CA0BC                                         # r4=
.text:823CA0BC                                         # r5=
.text:823CA0BC                                         # r6=
.text:823CA0BC                                         # r7=
.text:823CA0BC                                         # r8=
.text:823CA0C0                 clrlwi. %r11, %r3, 24   # Clear Left Immediate
.text:823CA0C4                 beq     loc_823CA074    # Branch if equal
.text:823CA0C8                 addi    %r30, %r31, 0xCB8 # Add Immediate
.text:823CA0CC                 addi    %r5, %r31, 0x2DB4 # Add Immediate
.text:823CA0D0                 mr      %r6, %r30       # Move Register
.text:823CA0D4                 li      %r4, 0xF9       # Load Immediate
.text:823CA0D8                 mr      %r3, %r31       # Move Register
.text:823CA0DC                 bl      sub_823C9B00    # Branch
.text:823CA0E0                 clrlwi. %r11, %r3, 24   # Clear Left Immediate
.text:823CA0E4                 beq     loc_823CA074    # Branch if equal
.text:823CA0E8                 addi    %r8, %r31, 0x233C # Add Immediate
.text:823CA0EC                 addi    %r7, %r31, 0x818 # Add Immediate
.text:823CA0F0                 li      %r6, 8          # Load Immediate
.text:823CA0F4                 mr      %r5, %r30       # Move Register
.text:823CA0F8                 li      %r4, 0xF9       # Load Immediate
.text:823CA0FC                 mr      %r3, %r31       # Move Register
.text:823CA100                 bl      compression_innermost #
.text:823CA100                                         # r4=
.text:823CA100                                         # r5=
.text:823CA100                                         # r6=
.text:823CA100                                         # r7=
.text:823CA100                                         # r8=
.text:823CA104                 clrlwi  %r11, %r3, 24   # Clear Left Immediate
.text:823CA108                 addi    %r11, %r11, 0   # Add Immediate
.text:823CA10C                 cntlzw  %r11, %r11      # Count Leading Zeros Word
.text:823CA110                 extrwi  %r11, %r11, 1,26 # Extract and Right Justify Immediate
.text:823CA114                 xori    %r3, %r11, 1    # XOR Immediate
.text:823CA118
.text:823CA118 loc_823CA118:                           # CODE XREF: sub_823CA040+38j
.text:823CA118                 addi    %sp, %sp, 0x70  # Add Immediate
.text:823CA11C                 lwz     %r12, var_8(%sp) # Load Word and Zero
.text:823CA120                 mtlr    %r12            # Move to link register
.text:823CA124                 ld      %r30, var_18(%sp) # Load Double Word
.text:823CA128                 ld      %r31, var_10(%sp) # Load Double Word
.text:823CA12C                 blr                     # Branch unconditionally
.text:823CA12C # End of function sub_823CA040
.text:823CA12C


.text:823C9B00
.text:823C9B00 # =============== S U B R O U T I N E =======================================
.text:823C9B00
.text:823C9B00
.text:823C9B00 sub_823C9B00:                           # CODE XREF: sub_823CA040+28p
.text:823C9B00                                         # sub_823CA040+50p ...
.text:823C9B00
.text:823C9B00 .set var_320, -0x320
.text:823C9B00 .set var_300, -0x300
.text:823C9B00 .set var_240, -0x240
.text:823C9B00
.text:823C9B00                 mflr    %r12            # Move from link register
.text:823C9B04                 bl      __savegprlr_25  # Branch
.text:823C9B08                 stwu    %sp, -0x370(%sp) # Store Word with Update
.text:823C9B0C                 mr      %r30, %r3       # Move Register
.text:823C9B10                 mr      %r29, %r4       # Move Register
.text:823C9B14                 mr      %r27, %r5       # Move Register
.text:823C9B18                 mr      %r28, %r6       # Move Register
.text:823C9B1C                 li      %r31, 0         # Load Immediate
.text:823C9B20
.text:823C9B20 loc_823C9B20:                           # CODE XREF: sub_823C9B00+3Cj
.text:823C9B20                 li      %r4, 4          # Load Immediate
.text:823C9B24                 mr      %r3, %r30       # Move Register
.text:823C9B28                 bl      cfunc_read_2_bytes_ext4 # r3 = state
.text:823C9B28                                         # r4 =
.text:823C9B2C                 addi    %r11, %sp, 0x370+var_320 # Add Immediate
.text:823C9B30                 stbx    %r3, %r31, %r11 # Store Byte Indexed
.text:823C9B34                 addi    %r31, %r31, 1   # Add Immediate
.text:823C9B38                 cmpwi   cr6, %r31, 0x14 # Compare Word Immediate
.text:823C9B3C                 blt     cr6, loc_823C9B20 # Branch if less than
.text:823C9B40                 lbz     %r11, 0x2EB7(%r30) # Load Byte and Zero
.text:823C9B44                 cmplwi  %r11, 0         # Compare Logical Word Immediate
.text:823C9B48                 beq     loc_823C9B54    # Branch if equal
.text:823C9B4C                 li      %r3, 0          # Load Immediate
.text:823C9B50                 b       loc_823CA034    # Branch
.text:823C9B54 # ---------------------------------------------------------------------------
.text:823C9B54
.text:823C9B54 loc_823C9B54:                           # CODE XREF: sub_823C9B00+48j
.text:823C9B54                 addi    %r8, %sp, 0x370+var_300 # Add Immediate
.text:823C9B58                 addi    %r7, %sp, 0x370+var_240 # Add Immediate
.text:823C9B5C                 li      %r6, 8          # Load Immediate
.text:823C9B60                 addi    %r5, %sp, 0x370+var_320 # Add Immediate
.text:823C9B64                 li      %r4, 0x14       # Load Immediate
.text:823C9B68                 mr      %r3, %r30       # Move Register
.text:823C9B6C                 bl      compression_innermost #
.text:823C9B6C                                         # r4=
.text:823C9B6C                                         # r5=
.text:823C9B6C                                         # r6=
.text:823C9B6C                                         # r7=
.text:823C9B6C                                         # r8=
.text:823C9B70                 lwz     %r11, 0x2B04(%r30) # Load Word and Zero
.text:823C9B74                 lwz     %r10, 0x2EB0(%r30) # Load Word and Zero
.text:823C9B78                 li      %r4, 0          # Load Immediate
.text:823C9B7C                 lbz     %r6, 0x2EB4(%r30) # Load Byte and Zero
.text:823C9B80                 cmpwi   cr6, %r29, 0    # Compare Word Immediate
.text:823C9B84                 lbz     %r3, 0x2EB7(%r30) # Load Byte and Zero
.text:823C9B88                 lwz     %r31, 0x2B08(%r30) # Load Word and Zero
.text:823C9B8C                 ble     cr6, loc_823CA018 # Branch if less than or equal
.text:823C9B90
.text:823C9B90 loc_823C9B90:                           # CODE XREF: sub_823C9B00+514j
.text:823C9B90                 addi    %r8, %sp, 0x370+var_240 # Add Immediate
.text:823C9B94                 rlwinm  %r9, %r10, 9,23,30 # Rotate Left Word Immediate then AND with Mask
.text:823C9B98                 lhzx    %r9, %r9, %r8   # Load Half Word and Zero Indexed
.text:823C9B9C                 extsh.  %r8, %r9        # Extend Sign Half Word
.text:823C9BA0                 bge     loc_823C9BE0    # Branch if greater than or equal
.text:823C9BA4                 lis     %r7, 0x80       # Load Immediate Shifted
.text:823C9BA8
.text:823C9BA8 loc_823C9BA8:                           # CODE XREF: sub_823C9B00+DCj
.text:823C9BA8                 neg     %r9, %r8        # Negate
.text:823C9BAC                 and     %r8, %r7, %r10  # AND
.text:823C9BB0                 extsh   %r9, %r9        # Extend Sign Half Word
.text:823C9BB4                 cntlzw  %r8, %r8        # Count Leading Zeros Word
.text:823C9BB8                 slwi    %r9, %r9, 1     # Shift Left Immediate
.text:823C9BBC                 srwi    %r8, %r8, 5     # Shift Right Immediate
.text:823C9BC0                 addi    %r5, %sp, 0x370+var_300 # Add Immediate
.text:823C9BC4                 xori    %r8, %r8, 1     # XOR Immediate
.text:823C9BC8                 srwi    %r7, %r7, 1     # Shift Right Immediate
.text:823C9BCC                 add     %r9, %r8, %r9   # Add
.text:823C9BD0                 slwi    %r9, %r9, 1     # Shift Left Immediate
.text:823C9BD4                 lhzx    %r9, %r9, %r5   # Load Half Word and Zero Indexed
.text:823C9BD8                 extsh.  %r8, %r9        # Extend Sign Half Word
.text:823C9BDC                 blt     loc_823C9BA8    # Branch if less than
.text:823C9BE0
.text:823C9BE0 loc_823C9BE0:                           # CODE XREF: sub_823C9B00+A0j
.text:823C9BE0                 extsh   %r8, %r9        # Extend Sign Half Word
.text:823C9BE4                 addi    %r9, %sp, 0x370+var_320 # Add Immediate
.text:823C9BE8                 lbzx    %r9, %r8, %r9   # Load Byte and Zero Indexed
.text:823C9BEC                 subf    %r7, %r9, %r6   # Subtract from
.text:823C9BF0                 slw     %r10, %r10, %r9 # Shift Left Word
.text:823C9BF4                 extsb   %r6, %r7        # Extend Sign Byte
.text:823C9BF8                 mr.     %r9, %r6        # Move Register
.text:823C9BFC                 bgt     loc_823C9C70    # Branch if greater than
.text:823C9C00                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9C04                 bge     cr6, loc_823C9C40 # Branch if greater than or equal
.text:823C9C08                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9C0C                 addi    %r6, %r9, 0x10  # Add Immediate
.text:823C9C10                 neg     %r26, %r9       # Negate
.text:823C9C14                 lbz     %r5, 0(%r11)    # Load Byte and Zero
.text:823C9C18                 rotlwi  %r9, %r7, 8     # Rotate Left Immediate
.text:823C9C1C                 extsb   %r6, %r6        # Extend Sign Byte
.text:823C9C20                 or      %r7, %r9, %r5   # OR
.text:823C9C24                 mr.     %r9, %r6        # Move Register
.text:823C9C28                 slw     %r7, %r7, %r26  # Shift Left Word
.text:823C9C2C                 or      %r10, %r7, %r10 # OR
.text:823C9C30                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9C34                 bgt     loc_823C9C70    # Branch if greater than
.text:823C9C38                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9C3C                 blt     cr6, loc_823C9C48 # Branch if less than
.text:823C9C40
.text:823C9C40 loc_823C9C40:                           # CODE XREF: sub_823C9B00+104j
.text:823C9C40                 li      %r3, 1          # Load Immediate
.text:823C9C44                 b       loc_823C9C70    # Branch
.text:823C9C48 # ---------------------------------------------------------------------------
.text:823C9C48
.text:823C9C48 loc_823C9C48:                           # CODE XREF: sub_823C9B00+13Cj
.text:823C9C48                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9C4C                 neg     %r5, %r9        # Negate
.text:823C9C50                 lbz     %r6, 0(%r11)    # Load Byte and Zero
.text:823C9C54                 addi    %r9, %r9, 0x10  # Add Immediate
.text:823C9C58                 rotlwi  %r7, %r7, 8     # Rotate Left Immediate
.text:823C9C5C                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9C60                 or      %r7, %r7, %r6   # OR
.text:823C9C64                 extsb   %r6, %r9        # Extend Sign Byte
.text:823C9C68                 slw     %r9, %r7, %r5   # Shift Left Word
.text:823C9C6C                 or      %r10, %r9, %r10 # OR
.text:823C9C70
.text:823C9C70 loc_823C9C70:                           # CODE XREF: sub_823C9B00+FCj
.text:823C9C70                                         # sub_823C9B00+134j ...
.text:823C9C70                 clrlwi. %r9, %r3, 24    # Clear Left Immediate
.text:823C9C74                 bne     loc_823CA018    # Branch if not equal
.text:823C9C78                 cmpwi   cr6, %r8, 0x11  # Compare Word Immediate
.text:823C9C7C                 bne     cr6, loc_823C9D48 # Branch if not equal
.text:823C9C80                 addi    %r9, %r6, -4    # Add Immediate
.text:823C9C84                 srwi    %r8, %r10, 28   # Shift Right Immediate
.text:823C9C88                 extsb   %r6, %r9        # Extend Sign Byte
.text:823C9C8C                 slwi    %r10, %r10, 4   # Shift Left Immediate
.text:823C9C90                 mr.     %r9, %r6        # Move Register
.text:823C9C94                 bgt     loc_823C9D08    # Branch if greater than
.text:823C9C98                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9C9C                 bge     cr6, loc_823C9CD8 # Branch if greater than or equal
.text:823C9CA0                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9CA4                 addi    %r6, %r9, 0x10  # Add Immediate
.text:823C9CA8                 neg     %r26, %r9       # Negate
.text:823C9CAC                 lbz     %r5, 0(%r11)    # Load Byte and Zero
.text:823C9CB0                 rotlwi  %r9, %r7, 8     # Rotate Left Immediate
.text:823C9CB4                 extsb   %r6, %r6        # Extend Sign Byte
.text:823C9CB8                 or      %r7, %r9, %r5   # OR
.text:823C9CBC                 mr.     %r9, %r6        # Move Register
.text:823C9CC0                 slw     %r7, %r7, %r26  # Shift Left Word
.text:823C9CC4                 or      %r10, %r7, %r10 # OR
.text:823C9CC8                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9CCC                 bgt     loc_823C9D08    # Branch if greater than
.text:823C9CD0                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9CD4                 blt     cr6, loc_823C9CE0 # Branch if less than
.text:823C9CD8
.text:823C9CD8 loc_823C9CD8:                           # CODE XREF: sub_823C9B00+19Cj
.text:823C9CD8                 li      %r3, 1          # Load Immediate
.text:823C9CDC                 b       loc_823C9D08    # Branch
.text:823C9CE0 # ---------------------------------------------------------------------------
.text:823C9CE0
.text:823C9CE0 loc_823C9CE0:                           # CODE XREF: sub_823C9B00+1D4j
.text:823C9CE0                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9CE4                 neg     %r5, %r9        # Negate
.text:823C9CE8                 lbz     %r6, 0(%r11)    # Load Byte and Zero
.text:823C9CEC                 addi    %r9, %r9, 0x10  # Add Immediate
.text:823C9CF0                 rotlwi  %r7, %r7, 8     # Rotate Left Immediate
.text:823C9CF4                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9CF8                 or      %r7, %r7, %r6   # OR
.text:823C9CFC                 extsb   %r6, %r9        # Extend Sign Byte
.text:823C9D00                 slw     %r9, %r7, %r5   # Shift Left Word
.text:823C9D04                 or      %r10, %r9, %r10 # OR
.text:823C9D08
.text:823C9D08 loc_823C9D08:                           # CODE XREF: sub_823C9B00+194j
.text:823C9D08                                         # sub_823C9B00+1CCj ...
.text:823C9D08                 addi    %r9, %r8, 4     # Add Immediate
.text:823C9D0C                 add     %r8, %r9, %r4   # Add
.text:823C9D10                 cmpw    cr6, %r8, %r29  # Compare Word
.text:823C9D14                 blt     cr6, loc_823C9D1C # Branch if less than
.text:823C9D18                 subf    %r9, %r4, %r29  # Subtract from
.text:823C9D1C
.text:823C9D1C loc_823C9D1C:                           # CODE XREF: sub_823C9B00+214j
.text:823C9D1C                 cmpwi   cr6, %r9, 0     # Compare Word Immediate
.text:823C9D20                 ble     cr6, loc_823C9FE8 # Branch if less than or equal
.text:823C9D24                 add     %r8, %r4, %r28  # Add
.text:823C9D28                 li      %r7, 0          # Load Immediate
.text:823C9D2C                 cmplwi  %r9, 0          # Compare Logical Word Immediate
.text:823C9D30                 beq     loc_823C9E14    # Branch if equal
.text:823C9D34                 mtctr   %r9             # Move to count register
.text:823C9D38
.text:823C9D38 loc_823C9D38:                           # CODE XREF: sub_823C9B00+240j
.text:823C9D38                 stb     %r7, 0(%r8)     # Store Byte
.text:823C9D3C                 addi    %r8, %r8, 1     # Add Immediate
.text:823C9D40                 bdnz    loc_823C9D38    # CTR--; branch if CTR non-zero
.text:823C9D44                 b       loc_823C9E14    # Branch
.text:823C9D48 # ---------------------------------------------------------------------------
.text:823C9D48
.text:823C9D48 loc_823C9D48:                           # CODE XREF: sub_823C9B00+17Cj
.text:823C9D48                 cmpwi   cr6, %r8, 0x12  # Compare Word Immediate
.text:823C9D4C                 bne     cr6, loc_823C9E1C # Branch if not equal
.text:823C9D50                 addi    %r9, %r6, -5    # Add Immediate
.text:823C9D54                 srwi    %r8, %r10, 27   # Shift Right Immediate
.text:823C9D58                 extsb   %r6, %r9        # Extend Sign Byte
.text:823C9D5C                 slwi    %r10, %r10, 5   # Shift Left Immediate
.text:823C9D60                 mr.     %r9, %r6        # Move Register
.text:823C9D64                 bgt     loc_823C9DD8    # Branch if greater than
.text:823C9D68                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9D6C                 bge     cr6, loc_823C9DA8 # Branch if greater than or equal
.text:823C9D70                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9D74                 addi    %r6, %r9, 0x10  # Add Immediate
.text:823C9D78                 neg     %r26, %r9       # Negate
.text:823C9D7C                 lbz     %r5, 0(%r11)    # Load Byte and Zero
.text:823C9D80                 rotlwi  %r9, %r7, 8     # Rotate Left Immediate
.text:823C9D84                 extsb   %r6, %r6        # Extend Sign Byte
.text:823C9D88                 or      %r7, %r9, %r5   # OR
.text:823C9D8C                 mr.     %r9, %r6        # Move Register
.text:823C9D90                 slw     %r7, %r7, %r26  # Shift Left Word
.text:823C9D94                 or      %r10, %r7, %r10 # OR
.text:823C9D98                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9D9C                 bgt     loc_823C9DD8    # Branch if greater than
.text:823C9DA0                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9DA4                 blt     cr6, loc_823C9DB0 # Branch if less than
.text:823C9DA8
.text:823C9DA8 loc_823C9DA8:                           # CODE XREF: sub_823C9B00+26Cj
.text:823C9DA8                 li      %r3, 1          # Load Immediate
.text:823C9DAC                 b       loc_823C9DD8    # Branch
.text:823C9DB0 # ---------------------------------------------------------------------------
.text:823C9DB0
.text:823C9DB0 loc_823C9DB0:                           # CODE XREF: sub_823C9B00+2A4j
.text:823C9DB0                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9DB4                 neg     %r5, %r9        # Negate
.text:823C9DB8                 lbz     %r6, 0(%r11)    # Load Byte and Zero
.text:823C9DBC                 addi    %r9, %r9, 0x10  # Add Immediate
.text:823C9DC0                 rotlwi  %r7, %r7, 8     # Rotate Left Immediate
.text:823C9DC4                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9DC8                 or      %r7, %r7, %r6   # OR
.text:823C9DCC                 extsb   %r6, %r9        # Extend Sign Byte
.text:823C9DD0                 slw     %r9, %r7, %r5   # Shift Left Word
.text:823C9DD4                 or      %r10, %r9, %r10 # OR
.text:823C9DD8
.text:823C9DD8 loc_823C9DD8:                           # CODE XREF: sub_823C9B00+264j
.text:823C9DD8                                         # sub_823C9B00+29Cj ...
.text:823C9DD8                 addi    %r9, %r8, 0x14  # Add Immediate
.text:823C9DDC                 add     %r8, %r9, %r4   # Add
.text:823C9DE0                 cmpw    cr6, %r8, %r29  # Compare Word
.text:823C9DE4                 blt     cr6, loc_823C9DEC # Branch if less than
.text:823C9DE8                 subf    %r9, %r4, %r29  # Subtract from
.text:823C9DEC
.text:823C9DEC loc_823C9DEC:                           # CODE XREF: sub_823C9B00+2E4j
.text:823C9DEC                 cmpwi   cr6, %r9, 0     # Compare Word Immediate
.text:823C9DF0                 ble     cr6, loc_823C9FE8 # Branch if less than or equal
.text:823C9DF4                 add     %r8, %r4, %r28  # Add
.text:823C9DF8                 li      %r7, 0          # Load Immediate
.text:823C9DFC                 cmplwi  %r9, 0          # Compare Logical Word Immediate
.text:823C9E00                 beq     loc_823C9E14    # Branch if equal
.text:823C9E04                 mtctr   %r9             # Move to count register
.text:823C9E08
.text:823C9E08 loc_823C9E08:                           # CODE XREF: sub_823C9B00+310j
.text:823C9E08                 stb     %r7, 0(%r8)     # Store Byte
.text:823C9E0C                 addi    %r8, %r8, 1     # Add Immediate
.text:823C9E10                 bdnz    loc_823C9E08    # CTR--; branch if CTR non-zero
.text:823C9E14
.text:823C9E14 loc_823C9E14:                           # CODE XREF: sub_823C9B00+230j
.text:823C9E14                                         # sub_823C9B00+244j ...
.text:823C9E14                 add     %r4, %r9, %r4   # Add
.text:823C9E18                 b       loc_823C9FE8    # Branch
.text:823C9E1C # ---------------------------------------------------------------------------
.text:823C9E1C
.text:823C9E1C loc_823C9E1C:                           # CODE XREF: sub_823C9B00+24Cj
.text:823C9E1C                 cmpwi   cr6, %r8, 0x13  # Compare Word Immediate
.text:823C9E20                 bne     cr6, loc_823C9FF0 # Branch if not equal
.text:823C9E24                 mr      %r9, %r6        # Move Register
.text:823C9E28                 srwi    %r8, %r10, 31   # Shift Right Immediate
.text:823C9E2C                 addi    %r7, %r9, -1    # Add Immediate
.text:823C9E30                 slwi    %r9, %r10, 1    # Shift Left Immediate
.text:823C9E34                 extsb   %r6, %r7        # Extend Sign Byte
.text:823C9E38                 mr.     %r10, %r6       # Move Register
.text:823C9E3C                 bgt     loc_823C9EB0    # Branch if greater than
.text:823C9E40                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9E44                 bge     cr6, loc_823C9E80 # Branch if greater than or equal
.text:823C9E48                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9E4C                 addi    %r6, %r10, 0x10 # Add Immediate
.text:823C9E50                 neg     %r26, %r10      # Negate
.text:823C9E54                 lbz     %r5, 0(%r11)    # Load Byte and Zero
.text:823C9E58                 rotlwi  %r10, %r7, 8    # Rotate Left Immediate
.text:823C9E5C                 extsb   %r6, %r6        # Extend Sign Byte
.text:823C9E60                 or      %r7, %r10, %r5  # OR
.text:823C9E64                 mr.     %r10, %r6       # Move Register
.text:823C9E68                 slw     %r7, %r7, %r26  # Shift Left Word
.text:823C9E6C                 or      %r9, %r7, %r9   # OR
.text:823C9E70                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9E74                 bgt     loc_823C9EB0    # Branch if greater than
.text:823C9E78                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9E7C                 blt     cr6, loc_823C9E88 # Branch if less than
.text:823C9E80
.text:823C9E80 loc_823C9E80:                           # CODE XREF: sub_823C9B00+344j
.text:823C9E80                 li      %r3, 1          # Load Immediate
.text:823C9E84                 b       loc_823C9EB0    # Branch
.text:823C9E88 # ---------------------------------------------------------------------------
.text:823C9E88
.text:823C9E88 loc_823C9E88:                           # CODE XREF: sub_823C9B00+37Cj
.text:823C9E88                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9E8C                 neg     %r5, %r10       # Negate
.text:823C9E90                 lbz     %r6, 0(%r11)    # Load Byte and Zero
.text:823C9E94                 addi    %r10, %r10, 0x10 # Add Immediate
.text:823C9E98                 rotlwi  %r7, %r7, 8     # Rotate Left Immediate
.text:823C9E9C                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9EA0                 or      %r7, %r7, %r6   # OR
.text:823C9EA4                 extsb   %r6, %r10       # Extend Sign Byte
.text:823C9EA8                 slw     %r10, %r7, %r5  # Shift Left Word
.text:823C9EAC                 or      %r9, %r10, %r9  # OR
.text:823C9EB0
.text:823C9EB0 loc_823C9EB0:                           # CODE XREF: sub_823C9B00+33Cj
.text:823C9EB0                                         # sub_823C9B00+374j ...
.text:823C9EB0                 addi    %r5, %r8, 4     # Add Immediate
.text:823C9EB4                 add     %r10, %r5, %r4  # Add
.text:823C9EB8                 cmpw    cr6, %r10, %r29 # Compare Word
.text:823C9EBC                 blt     cr6, loc_823C9EC4 # Branch if less than
.text:823C9EC0                 subf    %r5, %r4, %r29  # Subtract from
.text:823C9EC4
.text:823C9EC4 loc_823C9EC4:                           # CODE XREF: sub_823C9B00+3BCj
.text:823C9EC4                 addi    %r8, %sp, 0x370+var_240 # Add Immediate
.text:823C9EC8                 rlwinm  %r10, %r9, 9,23,30 # Rotate Left Word Immediate then AND with Mask
.text:823C9ECC                 lhzx    %r10, %r10, %r8 # Load Half Word and Zero Indexed
.text:823C9ED0                 extsh.  %r8, %r10       # Extend Sign Half Word
.text:823C9ED4                 bge     loc_823C9F14    # Branch if greater than or equal
.text:823C9ED8                 lis     %r7, 0x80       # Load Immediate Shifted
.text:823C9EDC
.text:823C9EDC loc_823C9EDC:                           # CODE XREF: sub_823C9B00+410j
.text:823C9EDC                 neg     %r10, %r8       # Negate
.text:823C9EE0                 and     %r8, %r7, %r9   # AND
.text:823C9EE4                 extsh   %r10, %r10      # Extend Sign Half Word
.text:823C9EE8                 cntlzw  %r8, %r8        # Count Leading Zeros Word
.text:823C9EEC                 srwi    %r7, %r7, 1     # Shift Right Immediate
.text:823C9EF0                 srwi    %r26, %r8, 5    # Shift Right Immediate
.text:823C9EF4                 slwi    %r8, %r10, 1    # Shift Left Immediate
.text:823C9EF8                 xori    %r10, %r26, 1   # XOR Immediate
.text:823C9EFC                 addi    %r26, %sp, 0x370+var_300 # Add Immediate
.text:823C9F00                 add     %r10, %r10, %r8 # Add
.text:823C9F04                 slwi    %r10, %r10, 1   # Shift Left Immediate
.text:823C9F08                 lhzx    %r10, %r10, %r26 # Load Half Word and Zero Indexed
.text:823C9F0C                 extsh.  %r8, %r10       # Extend Sign Half Word
.text:823C9F10                 blt     loc_823C9EDC    # Branch if less than
.text:823C9F14
.text:823C9F14 loc_823C9F14:                           # CODE XREF: sub_823C9B00+3D4j
.text:823C9F14                 extsh   %r8, %r10       # Extend Sign Half Word
.text:823C9F18                 addi    %r10, %sp, 0x370+var_320 # Add Immediate
.text:823C9F1C                 lbzx    %r10, %r8, %r10 # Load Byte and Zero Indexed
.text:823C9F20                 subf    %r7, %r10, %r6  # Subtract from
.text:823C9F24                 slw     %r10, %r9, %r10 # Shift Left Word
.text:823C9F28                 extsb   %r6, %r7        # Extend Sign Byte
.text:823C9F2C                 mr.     %r9, %r6        # Move Register
.text:823C9F30                 bgt     loc_823C9FA4    # Branch if greater than
.text:823C9F34                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9F38                 bge     cr6, loc_823C9F74 # Branch if greater than or equal
.text:823C9F3C                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9F40                 addi    %r6, %r9, 0x10  # Add Immediate
.text:823C9F44                 neg     %r25, %r9       # Negate
.text:823C9F48                 lbz     %r26, 0(%r11)   # Load Byte and Zero
.text:823C9F4C                 rotlwi  %r9, %r7, 8     # Rotate Left Immediate
.text:823C9F50                 extsb   %r6, %r6        # Extend Sign Byte
.text:823C9F54                 or      %r7, %r9, %r26  # OR
.text:823C9F58                 mr.     %r9, %r6        # Move Register
.text:823C9F5C                 slw     %r7, %r7, %r25  # Shift Left Word
.text:823C9F60                 or      %r10, %r7, %r10 # OR
.text:823C9F64                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9F68                 bgt     loc_823C9FA4    # Branch if greater than
.text:823C9F6C                 cmplw   cr6, %r11, %r31 # Compare Logical Word
.text:823C9F70                 blt     cr6, loc_823C9F7C # Branch if less than
.text:823C9F74
.text:823C9F74 loc_823C9F74:                           # CODE XREF: sub_823C9B00+438j
.text:823C9F74                 li      %r3, 1          # Load Immediate
.text:823C9F78                 b       loc_823C9FA4    # Branch
.text:823C9F7C # ---------------------------------------------------------------------------
.text:823C9F7C
.text:823C9F7C loc_823C9F7C:                           # CODE XREF: sub_823C9B00+470j
.text:823C9F7C                 lbz     %r7, 1(%r11)    # Load Byte and Zero
.text:823C9F80                 neg     %r26, %r9       # Negate
.text:823C9F84                 lbz     %r6, 0(%r11)    # Load Byte and Zero
.text:823C9F88                 addi    %r9, %r9, 0x10  # Add Immediate
.text:823C9F8C                 rotlwi  %r7, %r7, 8     # Rotate Left Immediate
.text:823C9F90                 addi    %r11, %r11, 2   # Add Immediate
.text:823C9F94                 or      %r7, %r7, %r6   # OR
.text:823C9F98                 extsb   %r6, %r9        # Extend Sign Byte
.text:823C9F9C                 slw     %r9, %r7, %r26  # Shift Left Word
.text:823C9FA0                 or      %r10, %r9, %r10 # OR
.text:823C9FA4
.text:823C9FA4 loc_823C9FA4:                           # CODE XREF: sub_823C9B00+430j
.text:823C9FA4                                         # sub_823C9B00+468j ...
.text:823C9FA4                 lbzx    %r9, %r4, %r27  # Load Byte and Zero Indexed
.text:823C9FA8                 subf    %r9, %r8, %r9   # Subtract from
.text:823C9FAC                 addi    %r9, %r9, 0x11  # Add Immediate
.text:823C9FB0                 cmpwi   cr6, %r9, 0x11  # Compare Word Immediate
.text:823C9FB4                 blt     cr6, loc_823C9FBC # Branch if less than
.text:823C9FB8                 addi    %r9, %r9, -0x11 # Add Immediate
.text:823C9FBC
.text:823C9FBC loc_823C9FBC:                           # CODE XREF: sub_823C9B00+4B4j
.text:823C9FBC                 clrlwi  %r8, %r9, 24    # Clear Left Immediate
.text:823C9FC0                 cmpwi   cr6, %r5, 0     # Compare Word Immediate
.text:823C9FC4                 ble     cr6, loc_823C9FE8 # Branch if less than or equal
.text:823C9FC8                 add     %r9, %r4, %r28  # Add
.text:823C9FCC                 cmplwi  %r5, 0          # Compare Logical Word Immediate
.text:823C9FD0                 beq     loc_823C9FE4    # Branch if equal
.text:823C9FD4                 mtctr   %r5             # Move to count register
.text:823C9FD8
.text:823C9FD8 loc_823C9FD8:                           # CODE XREF: sub_823C9B00+4E0j
.text:823C9FD8                 stb     %r8, 0(%r9)     # Store Byte
.text:823C9FDC                 addi    %r9, %r9, 1     # Add Immediate
.text:823C9FE0                 bdnz    loc_823C9FD8    # CTR--; branch if CTR non-zero
.text:823C9FE4
.text:823C9FE4 loc_823C9FE4:                           # CODE XREF: sub_823C9B00+4D0j
.text:823C9FE4                 add     %r4, %r5, %r4   # Add
.text:823C9FE8
.text:823C9FE8 loc_823C9FE8:                           # CODE XREF: sub_823C9B00+220j
.text:823C9FE8                                         # sub_823C9B00+2F0j ...
.text:823C9FE8                 addi    %r4, %r4, -1    # Add Immediate
.text:823C9FEC                 b       loc_823CA00C    # Branch
.text:823C9FF0 # ---------------------------------------------------------------------------
.text:823C9FF0
.text:823C9FF0 loc_823C9FF0:                           # CODE XREF: sub_823C9B00+320j
.text:823C9FF0                 lbzx    %r9, %r4, %r27  # Load Byte and Zero Indexed
.text:823C9FF4                 subf    %r9, %r8, %r9   # Subtract from
.text:823C9FF8                 addi    %r9, %r9, 0x11  # Add Immediate
.text:823C9FFC                 cmpwi   cr6, %r9, 0x11  # Compare Word Immediate
.text:823CA000                 blt     cr6, loc_823CA008 # Branch if less than
.text:823CA004                 addi    %r9, %r9, -0x11 # Add Immediate
.text:823CA008
.text:823CA008 loc_823CA008:                           # CODE XREF: sub_823C9B00+500j
.text:823CA008                 stbx    %r9, %r4, %r28  # Store Byte Indexed
.text:823CA00C
.text:823CA00C loc_823CA00C:                           # CODE XREF: sub_823C9B00+4ECj
.text:823CA00C                 addi    %r4, %r4, 1     # Add Immediate
.text:823CA010                 cmpw    cr6, %r4, %r29  # Compare Word
.text:823CA014                 blt     cr6, loc_823C9B90 # Branch if less than
.text:823CA018
.text:823CA018 loc_823CA018:                           # CODE XREF: sub_823C9B00+8Cj
.text:823CA018                                         # sub_823C9B00+174j
.text:823CA018                 clrlwi  %r9, %r3, 24    # Clear Left Immediate
.text:823CA01C                 stw     %r11, 0x2B04(%r30) # Store Word
.text:823CA020                 stb     %r3, 0x2EB7(%r30) # Store Byte
.text:823CA024                 cntlzw  %r11, %r9       # Count Leading Zeros Word
.text:823CA028                 stw     %r10, 0x2EB0(%r30) # Store Word
.text:823CA02C                 stb     %r6, 0x2EB4(%r30) # Store Byte
.text:823CA030                 extrwi  %r3, %r11, 1,26 # Extract and Right Justify Immediate
.text:823CA034
.text:823CA034 loc_823CA034:                           # CODE XREF: sub_823C9B00+50j
.text:823CA034                 addi    %sp, %sp, 0x370 # Add Immediate
.text:823CA038                 b       __restgprlr_25  # Branch
.text:823CA038 # End of function sub_823C9B00
.text:823CA038







.text:823CA1A8
.text:823CA1A8 # =============== S U B R O U T I N E =======================================
.text:823CA1A8
.text:823CA1A8 #
.text:823CA1A8 # r4=
.text:823CA1A8 # r5=
.text:823CA1A8 # r6=
.text:823CA1A8 # r7=
.text:823CA1A8 # r8=
.text:823CA1A8
.text:823CA1A8 compression_innermost:                  # CODE XREF: sub_823C9B00+6Cp
.text:823CA1A8                                         # sub_823CA040+7Cp ...
.text:823CA1A8
.text:823CA1A8 .set var_F0, -0xF0
.text:823CA1A8 .set var_EC, -0xEC
.text:823CA1A8 .set var_E8, -0xE8
.text:823CA1A8 .set var_AC, -0xAC
.text:823CA1A8 .set var_A0, -0xA0
.text:823CA1A8 .set var_9C, -0x9C
.text:823CA1A8
.text:823CA1A8                 mflr    %r12            # Move from link register
.text:823CA1AC                 bl      __savegprlr_23  # Branch
.text:823CA1B0                 stwu    %sp, -0x140(%sp) # Store Word with Update
.text:823CA1B4                 li      %r25, 0         # Load Immediate
.text:823CA1B8                 mr      %r26, %r4       # Move Register
.text:823CA1BC                 mr      %r24, %r5       # Move Register
.text:823CA1C0                 mr      %r29, %r7       # Move Register
.text:823CA1C4                 mr      %r27, %r8       # Move Register
.text:823CA1C8                 addi    %r10, %sp, 0x140+var_9C # Add Immediate
.text:823CA1CC                 mr      %r9, %r25       # Move Register
.text:823CA1D0                 li      %r11, 0x10      # Load Immediate
.text:823CA1D4                 mtctr   %r11            # Move to count register
.text:823CA1D8
.text:823CA1D8 loc_823CA1D8:                           # CODE XREF: compression_innermost+38j
.text:823CA1D8                 stw     %r9, 0(%r10)    # Store Word
.text:823CA1DC                 addi    %r10, %r10, 4   # Add Immediate
.text:823CA1E0                 bdnz    loc_823CA1D8    # CTR--; branch if CTR non-zero
.text:823CA1E4                 mr      %r11, %r25      # Move Register
.text:823CA1E8                 cmplwi  cr6, %r26, 0    # Compare Logical Word Immediate
.text:823CA1EC                 beq     cr6, loc_823CA214 # Branch if equal
.text:823CA1F0
.text:823CA1F0 loc_823CA1F0:                           # CODE XREF: compression_innermost+68j
.text:823CA1F0                 lbzx    %r9, %r11, %r24 # Load Byte and Zero Indexed
.text:823CA1F4                 addi    %r10, %sp, 0x140+var_A0 # Add Immediate
.text:823CA1F8                 addi    %r11, %r11, 1   # Add Immediate
.text:823CA1FC                 rotlwi  %r9, %r9, 2     # Rotate Left Immediate
.text:823CA200                 cmplw   cr6, %r11, %r26 # Compare Logical Word
.text:823CA204                 lwzx    %r8, %r9, %r10  # Load Word and Zero Indexed
.text:823CA208                 addi    %r8, %r8, 1     # Add Immediate
.text:823CA20C                 stwx    %r8, %r9, %r10  # Store Word Indexed
.text:823CA210                 blt     cr6, loc_823CA1F0 # Branch if less than
.text:823CA214
.text:823CA214 loc_823CA214:                           # CODE XREF: compression_innermost+44j
.text:823CA214                 li      %r28, 1         # Load Immediate
.text:823CA218                 stw     %r25, 0x140+var_EC(%sp) # Store Word
.text:823CA21C                 mr      %r11, %r25      # Move Register
.text:823CA220                 mr      %r10, %r28      # Move Register
.text:823CA224
.text:823CA224 loc_823CA224:                           # CODE XREF: compression_innermost+ACj
.text:823CA224                 addi    %r9, %sp, 0x140+var_9C # Add Immediate
.text:823CA228                 addi    %r8, %sp, 0x140+var_EC # Add Immediate
.text:823CA22C                 subfic  %r7, %r10, 0x10 # Subtract from Immediate Carrying
.text:823CA230                 addi    %r5, %sp, 0x140+var_E8 # Add Immediate
.text:823CA234                 addi    %r10, %r10, 1   # Add Immediate
.text:823CA238                 lwzx    %r9, %r11, %r9  # Load Word and Zero Indexed
.text:823CA23C                 lwzx    %r8, %r11, %r8  # Load Word and Zero Indexed
.text:823CA240                 cmplwi  cr6, %r10, 0x10 # Compare Logical Word Immediate
.text:823CA244                 slw     %r9, %r9, %r7   # Shift Left Word
.text:823CA248                 add     %r9, %r9, %r8   # Add
.text:823CA24C                 stwx    %r9, %r11, %r5  # Store Word Indexed
.text:823CA250                 addi    %r11, %r11, 4   # Add Immediate
.text:823CA254                 ble     cr6, loc_823CA224 # Branch if less than or equal
.text:823CA258                 lwz     %r11, 0x140+var_AC(%sp) # Load Word and Zero
.text:823CA25C                 lis     %r4, 1          # Load Immediate Shifted
.text:823CA260                 cmplw   cr6, %r11, %r4  # Compare Logical Word
.text:823CA264                 beq     cr6, loc_823CA294 # Branch if equal
.text:823CA268                 cmplwi  cr6, %r11, 0    # Compare Logical Word Immediate
.text:823CA26C                 bne     cr6, loc_823CA28C # Branch if not equal
.text:823CA270                 clrlwi  %r11, %r6, 24   # Clear Left Immediate
.text:823CA274                 li      %r4, 0          # Load Immediate
.text:823CA278                 slw     %r11, %r28, %r11 # Shift Left Word
.text:823CA27C                 slwi    %r5, %r11, 1    # Shift Left Immediate
.text:823CA280                 mr      %r3, %r29       # Move Register
.text:823CA284                 bl      memset          # r3 = ptr
.text:823CA284                                         # r4 = byte
.text:823CA284                                         # r5 = count
.text:823CA288                 b       loc_823CA460    # Branch
.text:823CA28C # ---------------------------------------------------------------------------
.text:823CA28C
.text:823CA28C loc_823CA28C:                           # CODE XREF: compression_innermost+C4j
.text:823CA28C                                         # compression_innermost+1F4j
.text:823CA28C                 li      %r3, 0          # Load Immediate
.text:823CA290                 b       loc_823CA464    # Branch
.text:823CA294 # ---------------------------------------------------------------------------
.text:823CA294
.text:823CA294 loc_823CA294:                           # CODE XREF: compression_innermost+BCj
.text:823CA294                 clrlwi  %r31, %r6, 24   # Clear Left Immediate
.text:823CA298                 mr      %r8, %r28       # Move Register
.text:823CA29C                 subfic  %r11, %r31, 0x10 # Subtract from Immediate Carrying
.text:823CA2A0                 cmplwi  cr6, %r31, 1    # Compare Logical Word Immediate
.text:823CA2A4                 clrlwi  %r5, %r11, 24   # Clear Left Immediate
.text:823CA2A8                 blt     cr6, loc_823CA2F4 # Branch if less than
.text:823CA2AC                 clrlwi  %r6, %r5, 24    # Clear Left Immediate
.text:823CA2B0                 addi    %r9, %r31, -1   # Add Immediate
.text:823CA2B4                 mr      %r11, %r25      # Move Register
.text:823CA2B8                 mr      %r10, %r31      # Move Register
.text:823CA2BC                 addi    %r8, %r31, 1    # Add Immediate
.text:823CA2C0
.text:823CA2C0 loc_823CA2C0:                           # CODE XREF: compression_innermost+140j
.text:823CA2C0                 addi    %r7, %sp, 0x140+var_EC # Add Immediate
.text:823CA2C4                 addi    %r3, %sp, 0x140+var_9C # Add Immediate
.text:823CA2C8                 slw     %r30, %r28, %r9 # Shift Left Word
.text:823CA2CC                 lwzx    %r23, %r11, %r7 # Load Word and Zero Indexed
.text:823CA2D0                 addic.  %r10, %r10, -1  # Add Immediate Carrying
.text:823CA2D4                 stwx    %r30, %r11, %r3 # Store Word Indexed
.text:823CA2D8                 addi    %r9, %r9, -1    # Add Immediate
.text:823CA2DC                 srw     %r3, %r23, %r6  # Shift Right Word
.text:823CA2E0                 stwx    %r3, %r11, %r7  # Store Word Indexed
.text:823CA2E4                 addi    %r11, %r11, 4   # Add Immediate
.text:823CA2E8                 bne     loc_823CA2C0    # Branch if not equal
.text:823CA2EC                 cmplwi  cr6, %r8, 0x10  # Compare Logical Word Immediate
.text:823CA2F0                 bgt     cr6, loc_823CA320 # Branch if greater than
.text:823CA2F4
.text:823CA2F4 loc_823CA2F4:                           # CODE XREF: compression_innermost+100j
.text:823CA2F4                 slwi    %r11, %r8, 2    # Shift Left Immediate
.text:823CA2F8                 addi    %r9, %sp, 0x140+var_A0 # Add Immediate
.text:823CA2FC                 subfic  %r10, %r8, 0x10 # Subtract from Immediate Carrying
.text:823CA300                 add     %r9, %r11, %r9  # Add
.text:823CA304                 subfic  %r11, %r8, 0x11 # Subtract from Immediate Carrying
.text:823CA308
.text:823CA308 loc_823CA308:                           # CODE XREF: compression_innermost+174j
.text:823CA308                 slw     %r8, %r28, %r10 # Shift Left Word
.text:823CA30C                 stw     %r8, 0(%r9)     # Store Word
.text:823CA310                 addic.  %r11, %r11, -1  # Add Immediate Carrying
.text:823CA314                 addi    %r10, %r10, -1  # Add Immediate
.text:823CA318                 addi    %r9, %r9, 4     # Add Immediate
.text:823CA31C                 bne     loc_823CA308    # Branch if not equal
.text:823CA320
.text:823CA320 loc_823CA320:                           # CODE XREF: compression_innermost+148j
.text:823CA320                 slwi    %r11, %r31, 2   # Shift Left Immediate
.text:823CA324                 addi    %r10, %sp, 0x140+var_EC # Add Immediate
.text:823CA328                 clrlwi  %r30, %r5, 24   # Clear Left Immediate
.text:823CA32C                 lwzx    %r11, %r11, %r10 # Load Word and Zero Indexed
.text:823CA330                 srw     %r11, %r11, %r30 # Shift Right Word
.text:823CA334                 cmplw   cr6, %r11, %r4  # Compare Logical Word
.text:823CA338                 beq     cr6, loc_823CA358 # Branch if equal
.text:823CA33C                 slw     %r9, %r28, %r31 # Shift Left Word
.text:823CA340                 slwi    %r10, %r11, 1   # Shift Left Immediate
.text:823CA344                 subf    %r11, %r11, %r9 # Subtract from
.text:823CA348                 li      %r4, 0          # Load Immediate
.text:823CA34C                 slwi    %r5, %r11, 1    # Shift Left Immediate
.text:823CA350                 add     %r3, %r10, %r29 # Add
.text:823CA354                 bl      memset          # r3 = ptr
.text:823CA354                                         # r4 = byte
.text:823CA354                                         # r5 = count
.text:823CA358
.text:823CA358 loc_823CA358:                           # CODE XREF: compression_innermost+190j
.text:823CA358                 mr      %r5, %r26       # Move Register
.text:823CA35C                 mr      %r4, %r25       # Move Register
.text:823CA360                 cmpwi   cr6, %r26, 0    # Compare Word Immediate
.text:823CA364                 ble     cr6, loc_823CA460 # Branch if less than or equal
.text:823CA368
.text:823CA368 loc_823CA368:                           # CODE XREF: compression_innermost+2B4j
.text:823CA368                 lbzx    %r9, %r4, %r24  # Load Byte and Zero Indexed
.text:823CA36C                 cmplwi  %r9, 0          # Compare Logical Word Immediate
.text:823CA370                 beq     loc_823CA454    # Branch if equal
.text:823CA374                 slwi    %r8, %r9, 2     # Shift Left Immediate
.text:823CA378                 addi    %r7, %sp, 0x140+var_F0 # Add Immediate
.text:823CA37C                 addi    %r10, %sp, 0x140+var_A0 # Add Immediate
.text:823CA380                 cmplw   cr6, %r9, %r31  # Compare Logical Word
.text:823CA384                 lwzx    %r11, %r8, %r7  # Load Word and Zero Indexed
.text:823CA388                 lwzx    %r10, %r8, %r10 # Load Word and Zero Indexed
.text:823CA38C                 add     %r10, %r10, %r11 # Add
.text:823CA390                 bgt     cr6, loc_823CA3D4 # Branch if greater than
.text:823CA394                 slw     %r9, %r28, %r31 # Shift Left Word
.text:823CA398                 cmplw   cr6, %r10, %r9  # Compare Logical Word
.text:823CA39C                 bgt     cr6, loc_823CA28C # Branch if greater than
.text:823CA3A0                 cmplw   cr6, %r11, %r10 # Compare Logical Word
.text:823CA3A4                 bge     cr6, loc_823CA3CC # Branch if greater than or equal
.text:823CA3A8                 slwi    %r9, %r11, 1    # Shift Left Immediate
.text:823CA3AC                 subf.   %r11, %r11, %r10 # Subtract from
.text:823CA3B0                 add     %r9, %r9, %r29  # Add
.text:823CA3B4                 extsh   %r6, %r4        # Extend Sign Half Word
.text:823CA3B8                 beq     loc_823CA3CC    # Branch if equal
.text:823CA3BC                 mtctr   %r11            # Move to count register
.text:823CA3C0
.text:823CA3C0 loc_823CA3C0:                           # CODE XREF: compression_innermost+220j
.text:823CA3C0                 sth     %r6, 0(%r9)     # Store Half Word
.text:823CA3C4                 addi    %r9, %r9, 2     # Add Immediate
.text:823CA3C8                 bdnz    loc_823CA3C0    # CTR--; branch if CTR non-zero
.text:823CA3CC
.text:823CA3CC loc_823CA3CC:                           # CODE XREF: compression_innermost+1FCj
.text:823CA3CC                                         # compression_innermost+210j
.text:823CA3CC                 stwx    %r10, %r8, %r7  # Store Word Indexed
.text:823CA3D0                 b       loc_823CA454    # Branch
.text:823CA3D4 # ---------------------------------------------------------------------------
.text:823CA3D4
.text:823CA3D4 loc_823CA3D4:                           # CODE XREF: compression_innermost+1E8j
.text:823CA3D4                 srw     %r6, %r11, %r30 # Shift Right Word
.text:823CA3D8                 stwx    %r10, %r8, %r7  # Store Word Indexed
.text:823CA3DC                 subf    %r10, %r31, %r9 # Subtract from
.text:823CA3E0                 slwi    %r8, %r6, 1     # Shift Left Immediate
.text:823CA3E4                 slwi    %r9, %r5, 2     # Shift Left Immediate
.text:823CA3E8                 slw     %r6, %r11, %r31 # Shift Left Word
.text:823CA3EC                 clrlwi  %r10, %r10, 24  # Clear Left Immediate
.text:823CA3F0                 add     %r8, %r8, %r29  # Add
.text:823CA3F4                 neg     %r7, %r5        # Negate
.text:823CA3F8                 add     %r11, %r9, %r27 # Add
.text:823CA3FC
.text:823CA3FC loc_823CA3FC:                           # CODE XREF: compression_innermost+2A4j
.text:823CA3FC                 lhz     %r9, 0(%r8)     # Load Half Word and Zero
.text:823CA400                 cmplwi  cr6, %r9, 0     # Compare Logical Word Immediate
.text:823CA404                 bne     cr6, loc_823CA420 # Branch if not equal
.text:823CA408                 sth     %r25, 2(%r11)   # Store Half Word
.text:823CA40C                 addi    %r5, %r5, 1     # Add Immediate
.text:823CA410                 sth     %r25, 0(%r11)   # Store Half Word
.text:823CA414                 addi    %r11, %r11, 4   # Add Immediate
.text:823CA418                 sth     %r7, 0(%r8)     # Store Half Word
.text:823CA41C                 addi    %r7, %r7, -1    # Add Immediate
.text:823CA420
.text:823CA420 loc_823CA420:                           # CODE XREF: compression_innermost+25Cj
.text:823CA420                 extsh.  %r9, %r6        # Extend Sign Half Word
.text:823CA424                 lha     %r9, 0(%r8)     # Load Half Word Algebraic
.text:823CA428                 slwi    %r9, %r9, 2     # Shift Left Immediate
.text:823CA42C                 bge     loc_823CA43C    # Branch if greater than or equal
.text:823CA430                 subf    %r9, %r9, %r27  # Subtract from
.text:823CA434                 addi    %r8, %r9, 2     # Add Immediate
.text:823CA438                 b       loc_823CA440    # Branch
.text:823CA43C # ---------------------------------------------------------------------------
.text:823CA43C
.text:823CA43C loc_823CA43C:                           # CODE XREF: compression_innermost+284j
.text:823CA43C                 subf    %r8, %r9, %r27  # Subtract from
.text:823CA440
.text:823CA440 loc_823CA440:                           # CODE XREF: compression_innermost+290j
.text:823CA440                 addi    %r10, %r10, 0xFF # Add Immediate
.text:823CA444                 slwi    %r6, %r6, 1     # Shift Left Immediate
.text:823CA448                 clrlwi. %r10, %r10, 24  # Clear Left Immediate
.text:823CA44C                 bne     loc_823CA3FC    # Branch if not equal
.text:823CA450                 sth     %r4, 0(%r8)     # Store Half Word
.text:823CA454
.text:823CA454 loc_823CA454:                           # CODE XREF: compression_innermost+1C8j
.text:823CA454                                         # compression_innermost+228j
.text:823CA454                 addi    %r4, %r4, 1     # Add Immediate
.text:823CA458                 cmpw    cr6, %r4, %r26  # Compare Word
.text:823CA45C                 blt     cr6, loc_823CA368 # Branch if less than
.text:823CA460
.text:823CA460 loc_823CA460:                           # CODE XREF: compression_innermost+E0j
.text:823CA460                                         # compression_innermost+1BCj
.text:823CA460                 li      %r3, 1          # Load Immediate
.text:823CA464
.text:823CA464 loc_823CA464:                           # CODE XREF: compression_innermost+E8j
.text:823CA464                 addi    %sp, %sp, 0x140 # Add Immediate
.text:823CA468                 b       __restgprlr_23  # Branch
.text:823CA468 # End of function compression_innermost
.text:823CA468
.text:823CA468 # ---------------------------------------------------------------------------





.text:823C99E0
.text:823C99E0 # =============== S U B R O U T I N E =======================================
.text:823C99E0
.text:823C99E0
.text:823C99E0 sub_823C99E0:                           # CODE XREF: compression_FFnew_inner_real_kind_final+1A8p
.text:823C99E0                 mflr    %r12            # Move from link register
.text:823C99E4                 bl      __savegprlr_29  # Branch
.text:823C99E8                 stwu    %sp, -0x70(%sp) # Store Word with Update
.text:823C99EC                 mr      %r30, %r4       # Move Register
.text:823C99F0                 mr      %r29, %r3       # Move Register
.text:823C99F4                 mr      %r31, %r5       # Move Register
.text:823C99F8                 cmpwi   cr6, %r30, 0x101 # Compare Word Immediate
.text:823C99FC                 bge     cr6, loc_823C9A38 # Branch if greater than or equal
.text:823C9A00                 subfic  %r5, %r30, 0x101 # Subtract from Immediate Carrying
.text:823C9A04                 cmpw    cr6, %r5, %r31  # Compare Word
.text:823C9A08                 blt     cr6, loc_823C9A10 # Branch if less than
.text:823C9A0C                 mr      %r5, %r31       # Move Register
.text:823C9A10
.text:823C9A10 loc_823C9A10:                           # CODE XREF: sub_823C99E0+28j
.text:823C9A10                 mr      %r4, %r30       # Move Register
.text:823C9A14                 mr      %r3, %r29       # Move Register
.text:823C9A18                 bl      sub_823C9240    # Branch
.text:823C9A1C                 subf    %r11, %r3, %r30 # Subtract from
.text:823C9A20                 stw     %r3, 0x2EB8(%r29) # Store Word
.text:823C9A24                 mr      %r30, %r3       # Move Register
.text:823C9A28                 add.    %r31, %r11, %r31 # Add
.text:823C9A2C                 bgt     loc_823C9A38    # Branch if greater than
.text:823C9A30                 mr      %r3, %r31       # Move Register
.text:823C9A34                 b       loc_823C9A48    # Branch
.text:823C9A38 # ---------------------------------------------------------------------------
.text:823C9A38
.text:823C9A38 loc_823C9A38:                           # CODE XREF: sub_823C99E0+1Cj
.text:823C9A38                                         # sub_823C99E0+4Cj
.text:823C9A38                 mr      %r5, %r31       # Move Register
.text:823C9A3C                 mr      %r4, %r30       # Move Register
.text:823C9A40                 mr      %r3, %r29       # Move Register
.text:823C9A44                 bl      loc_823C95B8    # Branch
.text:823C9A48
.text:823C9A48 loc_823C9A48:                           # CODE XREF: sub_823C99E0+54j
.text:823C9A48                 addi    %sp, %sp, 0x70  # Add Immediate
.text:823C9A4C                 b       __restgprlr_29  # Branch
.text:823C9A4C # End of function sub_823C99E0
.text:823C9A4C

.text:823C9240
.text:823C9240 # =============== S U B R O U T I N E =======================================
.text:823C9240
.text:823C9240
.text:823C9240 sub_823C9240:                           # CODE XREF: sub_823C99E0+38p
.text:823C9240                 mflr    %r12            # Move from link register
.text:823C9244                 bl      __savegprlr_18  # Branch
.text:823C9248                 mr      %r11, %r3       # Move Register
.text:823C924C                 add     %r26, %r4, %r5  # Add
.text:823C9250                 addi    %r23, %r11, 0xE3C # Add Immediate
.text:823C9254                 addi    %r22, %r11, 0x233C # Add Immediate
.text:823C9258                 addi    %r21, %r11, 0x18 # Add Immediate
.text:823C925C                 lwz     %r19, 0x2B08(%r11) # Load Word and Zero
.text:823C9260                 addi    %r20, %r11, 0x818 # Add Immediate
.text:823C9264                 lwz     %r30, 0(%r11)   # Load Word and Zero
.text:823C9268                 addi    %r25, %r11, 0xA18 # Add Immediate
.text:823C926C                 lbz     %r9, 0x2EB4(%r11) # Load Byte and Zero
.text:823C9270                 addi    %r24, %r11, 0xCB8 # Add Immediate
.text:823C9274                 lwz     %r10, 0x2EB0(%r11) # Load Word and Zero
.text:823C9278                 addi    %r28, %r11, 0xDB4 # Add Immediate
.text:823C927C                 lwz     %r31, 0x2B04(%r11) # Load Word and Zero
.text:823C9280                 addi    %r27, %r11, 0xE34 # Add Immediate
.text:823C9284                 addi    %r29, %r11, 0xC # Add Immediate
.text:823C9288                 b       loc_823C9590    # Branch
.text:823C928C # ---------------------------------------------------------------------------
.text:823C928C
.text:823C928C loc_823C928C:                           # CODE XREF: sub_823C9240+354j
.text:823C928C                 rlwinm  %r8, %r10, 11,21,30 # Rotate Left Word Immediate then AND with Mask
.text:823C9290                 lhax    %r7, %r8, %r21  # Load Half Word Algebraic Indexed
.text:823C9294                 cmpwi   %r7, 0          # Compare Word Immediate
.text:823C9298                 bge     loc_823C92CC    # Branch if greater than or equal
.text:823C929C                 lis     %r8, 0x20       # Load Immediate Shifted
.text:823C92A0
.text:823C92A0 loc_823C92A0:                           # CODE XREF: sub_823C9240+88j
.text:823C92A0                 and     %r6, %r8, %r10  # AND
.text:823C92A4                 slwi    %r7, %r7, 1     # Shift Left Immediate
.text:823C92A8                 cntlzw  %r6, %r6        # Count Leading Zeros Word
.text:823C92AC                 srwi    %r8, %r8, 1     # Shift Right Immediate
.text:823C92B0                 srwi    %r6, %r6, 5     # Shift Right Immediate
.text:823C92B4                 xori    %r6, %r6, 1     # XOR Immediate
.text:823C92B8                 subf    %r7, %r7, %r6   # Subtract from
.text:823C92BC                 slwi    %r7, %r7, 1     # Shift Left Immediate
.text:823C92C0                 lhax    %r7, %r7, %r23  # Load Half Word Algebraic Indexed
.text:823C92C4                 cmpwi   %r7, 0          # Compare Word Immediate
.text:823C92C8                 blt     loc_823C92A0    # Branch if less than
.text:823C92CC
.text:823C92CC loc_823C92CC:                           # CODE XREF: sub_823C9240+58j
.text:823C92CC                 cmplw   cr6, %r31, %r19 # Compare Logical Word
.text:823C92D0                 bge     cr6, loc_823C95AC # Branch if greater than or equal
.text:823C92D4                 lbzx    %r8, %r7, %r25  # Load Byte and Zero Indexed
.text:823C92D8                 subf    %r9, %r8, %r9   # Subtract from
.text:823C92DC                 slw     %r10, %r10, %r8 # Shift Left Word
.text:823C92E0                 extsb   %r9, %r9        # Extend Sign Byte
.text:823C92E4                 mr.     %r8, %r9        # Move Register
.text:823C92E8                 bgt     loc_823C9314    # Branch if greater than
.text:823C92EC                 lbz     %r9, 1(%r31)    # Load Byte and Zero
.text:823C92F0                 neg     %r6, %r8        # Negate
.text:823C92F4                 lbz     %r5, 0(%r31)    # Load Byte and Zero
.text:823C92F8                 addi    %r8, %r8, 0x10  # Add Immediate
.text:823C92FC                 rotlwi  %r9, %r9, 8     # Rotate Left Immediate
.text:823C9300                 addi    %r31, %r31, 2   # Add Immediate
.text:823C9304                 or      %r5, %r9, %r5   # OR
.text:823C9308                 extsb   %r9, %r8        # Extend Sign Byte
.text:823C930C                 slw     %r8, %r5, %r6   # Shift Left Word
.text:823C9310                 or      %r10, %r8, %r10 # OR
.text:823C9314
.text:823C9314 loc_823C9314:                           # CODE XREF: sub_823C9240+A8j
.text:823C9314                 addic.  %r6, %r7, -0x100 # Add Immediate Carrying
.text:823C9318                 bge     loc_823C9338    # Branch if greater than or equal
.text:823C931C                 lwz     %r8, 4(%r11)    # Load Word and Zero
.text:823C9320                 clrlwi  %r7, %r6, 24    # Clear Left Immediate
.text:823C9324                 add     %r8, %r8, %r30  # Add
.text:823C9328                 stbx    %r7, %r30, %r4  # Store Byte Indexed
.text:823C932C                 stbx    %r7, %r8, %r4   # Store Byte Indexed
.text:823C9330                 addi    %r4, %r4, 1     # Add Immediate
.text:823C9334                 b       loc_823C9590    # Branch
.text:823C9338 # ---------------------------------------------------------------------------
.text:823C9338
.text:823C9338 loc_823C9338:                           # CODE XREF: sub_823C9240+D8j
.text:823C9338                 clrlwi  %r5, %r6, 29    # Clear Left Immediate
.text:823C933C                 cmpwi   cr6, %r5, 7     # Compare Word Immediate
.text:823C9340                 bne     cr6, loc_823C93C8 # Branch if not equal
.text:823C9344                 rlwinm  %r8, %r10, 9,23,30 # Rotate Left Word Immediate then AND with Mask
.text:823C9348                 lhax    %r8, %r8, %r20  # Load Half Word Algebraic Indexed
.text:823C934C                 cmpwi   %r8, 0          # Compare Word Immediate
.text:823C9350                 bge     loc_823C9384    # Branch if greater than or equal
.text:823C9354                 lis     %r7, 0x80       # Load Immediate Shifted
.text:823C9358
.text:823C9358 loc_823C9358:                           # CODE XREF: sub_823C9240+140j
.text:823C9358                 and     %r5, %r7, %r10  # AND
.text:823C935C                 slwi    %r8, %r8, 1     # Shift Left Immediate
.text:823C9360                 cntlzw  %r5, %r5        # Count Leading Zeros Word
.text:823C9364                 srwi    %r7, %r7, 1     # Shift Right Immediate
.text:823C9368                 srwi    %r5, %r5, 5     # Shift Right Immediate
.text:823C936C                 xori    %r5, %r5, 1     # XOR Immediate
.text:823C9370                 subf    %r8, %r8, %r5   # Subtract from
.text:823C9374                 slwi    %r8, %r8, 1     # Shift Left Immediate
.text:823C9378                 lhax    %r8, %r8, %r22  # Load Half Word Algebraic Indexed
.text:823C937C                 cmpwi   %r8, 0          # Compare Word Immediate
.text:823C9380                 blt     loc_823C9358    # Branch if less than
.text:823C9384
.text:823C9384 loc_823C9384:                           # CODE XREF: sub_823C9240+110j
.text:823C9384                 lbzx    %r7, %r8, %r24  # Load Byte and Zero Indexed
.text:823C9388                 subf    %r9, %r7, %r9   # Subtract from
.text:823C938C                 slw     %r10, %r10, %r7 # Shift Left Word
.text:823C9390                 extsb   %r9, %r9        # Extend Sign Byte
.text:823C9394                 mr.     %r7, %r9        # Move Register
.text:823C9398                 bgt     loc_823C93C4    # Branch if greater than
.text:823C939C                 lbz     %r9, 1(%r31)    # Load Byte and Zero
.text:823C93A0                 neg     %r5, %r7        # Negate
.text:823C93A4                 lbz     %r3, 0(%r31)    # Load Byte and Zero
.text:823C93A8                 addi    %r7, %r7, 0x10  # Add Immediate
.text:823C93AC                 rotlwi  %r9, %r9, 8     # Rotate Left Immediate
.text:823C93B0                 addi    %r31, %r31, 2   # Add Immediate
.text:823C93B4                 or      %r3, %r9, %r3   # OR
.text:823C93B8                 extsb   %r9, %r7        # Extend Sign Byte
.text:823C93BC                 slw     %r7, %r3, %r5   # Shift Left Word
.text:823C93C0                 or      %r10, %r7, %r10 # OR
.text:823C93C4
.text:823C93C4 loc_823C93C4:                           # CODE XREF: sub_823C9240+158j
.text:823C93C4                 addi    %r5, %r8, 7     # Add Immediate
.text:823C93C8
.text:823C93C8 loc_823C93C8:                           # CODE XREF: sub_823C9240+100j
.text:823C93C8                 srawi   %r8, %r6, 3     # Shift Right Algebraic Word Immediate
.text:823C93CC                 extsb   %r6, %r8        # Extend Sign Byte
.text:823C93D0                 cmpwi   cr6, %r6, 2     # Compare Word Immediate
.text:823C93D4                 ble     cr6, loc_823C9540 # Branch if less than or equal
.text:823C93D8                 add     %r8, %r6, %r11  # Add
.text:823C93DC                 lbz     %r7, 0x2EE4(%r8) # Load Byte and Zero
.text:823C93E0                 cmplwi  cr6, %r7, 3     # Compare Logical Word Immediate
.text:823C93E4                 blt     cr6, loc_823C94C0 # Branch if less than
.text:823C93E8                 addic.  %r7, %r7, -3    # Add Immediate Carrying
.text:823C93EC                 beq     loc_823C9450    # Branch if equal
.text:823C93F0                 lbz     %r8, 0x2EE4(%r8) # Load Byte and Zero
.text:823C93F4                 extsb   %r7, %r9        # Extend Sign Byte
.text:823C93F8                 extsb   %r3, %r8        # Extend Sign Byte
.text:823C93FC                 mr      %r9, %r8        # Move Register
.text:823C9400                 subf    %r8, %r3, %r7   # Subtract from
.text:823C9404                 subfic  %r7, %r9, 0x23  # Subtract from Immediate Carrying
.text:823C9408                 addi    %r8, %r8, 3     # Add Immediate
.text:823C940C                 addi    %r3, %r9, -3    # Add Immediate
.text:823C9410                 extsb   %r9, %r8        # Extend Sign Byte
.text:823C9414                 srw     %r7, %r10, %r7  # Shift Right Word
.text:823C9418                 mr.     %r8, %r9        # Move Register
.text:823C941C                 slw     %r10, %r10, %r3 # Shift Left Word
.text:823C9420                 bgt     loc_823C9454    # Branch if greater than
.text:823C9424                 lbz     %r9, 1(%r31)    # Load Byte and Zero
.text:823C9428                 neg     %r3, %r8        # Negate
.text:823C942C                 lbz     %r18, 0(%r31)   # Load Byte and Zero
.text:823C9430                 addi    %r8, %r8, 0x10  # Add Immediate
.text:823C9434                 rotlwi  %r9, %r9, 8     # Rotate Left Immediate
.text:823C9438                 addi    %r31, %r31, 2   # Add Immediate
.text:823C943C                 or      %r18, %r9, %r18 # OR
.text:823C9440                 extsb   %r9, %r8        # Extend Sign Byte
.text:823C9444                 slw     %r8, %r18, %r3  # Shift Left Word
.text:823C9448                 or      %r10, %r8, %r10 # OR
.text:823C944C                 b       loc_823C9454    # Branch
.text:823C9450 # ---------------------------------------------------------------------------
.text:823C9450
.text:823C9450 loc_823C9450:                           # CODE XREF: sub_823C9240+1ACj
.text:823C9450                 li      %r7, 0          # Load Immediate
.text:823C9454
.text:823C9454 loc_823C9454:                           # CODE XREF: sub_823C9240+1E0j
.text:823C9454                                         # sub_823C9240+20Cj
.text:823C9454                 srwi    %r3, %r10, 25   # Shift Right Immediate
.text:823C9458                 slwi    %r8, %r7, 3     # Shift Left Immediate
.text:823C945C                 addi    %r7, %r6, 0xBC6 # Add Immediate
.text:823C9460                 mr      %r18, %r9       # Move Register
.text:823C9464                 slwi    %r9, %r7, 2     # Shift Left Immediate
.text:823C9468                 lbzx    %r7, %r3, %r28  # Load Byte and Zero Indexed
.text:823C946C                 extsb   %r7, %r7        # Extend Sign Byte
.text:823C9470                 lwzx    %r9, %r9, %r11  # Load Word and Zero Indexed
.text:823C9474                 add     %r6, %r9, %r8   # Add
.text:823C9478                 lbzx    %r9, %r7, %r27  # Load Byte and Zero Indexed
.text:823C947C                 subf    %r8, %r9, %r18  # Subtract from
.text:823C9480                 slw     %r10, %r10, %r9 # Shift Left Word
.text:823C9484                 extsb   %r9, %r8        # Extend Sign Byte
.text:823C9488                 mr.     %r8, %r9        # Move Register
.text:823C948C                 bgt     loc_823C94B8    # Branch if greater than
.text:823C9490                 lbz     %r9, 1(%r31)    # Load Byte and Zero
.text:823C9494                 neg     %r3, %r8        # Negate
.text:823C9498                 lbz     %r18, 0(%r31)   # Load Byte and Zero
.text:823C949C                 addi    %r8, %r8, 0x10  # Add Immediate
.text:823C94A0                 rotlwi  %r9, %r9, 8     # Rotate Left Immediate
.text:823C94A4                 addi    %r31, %r31, 2   # Add Immediate
.text:823C94A8                 or      %r18, %r9, %r18 # OR
.text:823C94AC                 extsb   %r9, %r8        # Extend Sign Byte
.text:823C94B0                 slw     %r8, %r18, %r3  # Shift Left Word
.text:823C94B4                 or      %r10, %r8, %r10 # OR
.text:823C94B8
.text:823C94B8 loc_823C94B8:                           # CODE XREF: sub_823C9240+24Cj
.text:823C94B8                 add     %r8, %r6, %r7   # Add
.text:823C94BC                 b       loc_823C952C    # Branch
.text:823C94C0 # ---------------------------------------------------------------------------
.text:823C94C0
.text:823C94C0 loc_823C94C0:                           # CODE XREF: sub_823C9240+1A4j
.text:823C94C0                 cmplwi  cr6, %r7, 0     # Compare Logical Word Immediate
.text:823C94C4                 beq     cr6, loc_823C9528 # Branch if equal
.text:823C94C8                 lbz     %r8, 0x2EE4(%r8) # Load Byte and Zero
.text:823C94CC                 mr      %r3, %r8        # Move Register
.text:823C94D0                 subf    %r9, %r8, %r9   # Subtract from
.text:823C94D4                 subfic  %r8, %r3, 0x20  # Subtract from Immediate Carrying
.text:823C94D8                 extsb   %r9, %r9        # Extend Sign Byte
.text:823C94DC                 srw     %r7, %r10, %r8  # Shift Right Word
.text:823C94E0                 mr.     %r8, %r9        # Move Register
.text:823C94E4                 slw     %r10, %r10, %r3 # Shift Left Word
.text:823C94E8                 bgt     loc_823C9514    # Branch if greater than
.text:823C94EC                 lbz     %r9, 1(%r31)    # Load Byte and Zero
.text:823C94F0                 neg     %r3, %r8        # Negate
.text:823C94F4                 lbz     %r18, 0(%r31)   # Load Byte and Zero
.text:823C94F8                 addi    %r8, %r8, 0x10  # Add Immediate
.text:823C94FC                 rotlwi  %r9, %r9, 8     # Rotate Left Immediate
.text:823C9500                 addi    %r31, %r31, 2   # Add Immediate
.text:823C9504                 or      %r18, %r9, %r18 # OR
.text:823C9508                 extsb   %r9, %r8        # Extend Sign Byte
.text:823C950C                 slw     %r8, %r18, %r3  # Shift Left Word
.text:823C9510                 or      %r10, %r8, %r10 # OR
.text:823C9514
.text:823C9514 loc_823C9514:                           # CODE XREF: sub_823C9240+2A8j
.text:823C9514                 addi    %r8, %r6, 0xBC6 # Add Immediate
.text:823C9518                 slwi    %r8, %r8, 2     # Shift Left Immediate
.text:823C951C                 lwzx    %r8, %r8, %r11  # Load Word and Zero Indexed
.text:823C9520                 add     %r8, %r8, %r7   # Add
.text:823C9524                 b       loc_823C952C    # Branch
.text:823C9528 # ---------------------------------------------------------------------------
.text:823C9528
.text:823C9528 loc_823C9528:                           # CODE XREF: sub_823C9240+284j
.text:823C9528                 li      %r8, 1          # Load Immediate
.text:823C952C
.text:823C952C loc_823C952C:                           # CODE XREF: sub_823C9240+27Cj
.text:823C952C                                         # sub_823C9240+2E4j
.text:823C952C                 lwz     %r7, 4(%r29)    # Load Word and Zero
.text:823C9530                 lwz     %r6, 0(%r29)    # Load Word and Zero
.text:823C9534                 stw     %r7, 8(%r29)    # Store Word
.text:823C9538                 stw     %r6, 4(%r29)    # Store Word
.text:823C953C                 b       loc_823C9550    # Branch
.text:823C9540 # ---------------------------------------------------------------------------
.text:823C9540
.text:823C9540 loc_823C9540:                           # CODE XREF: sub_823C9240+194j
.text:823C9540                 slwi    %r7, %r6, 2     # Shift Left Immediate
.text:823C9544                 lwz     %r6, 0(%r29)    # Load Word and Zero
.text:823C9548                 lwzx    %r8, %r7, %r29  # Load Word and Zero Indexed
.text:823C954C                 stwx    %r6, %r7, %r29  # Store Word Indexed
.text:823C9550
.text:823C9550 loc_823C9550:                           # CODE XREF: sub_823C9240+2FCj
.text:823C9550                 lwz     %r3, 8(%r11)    # Load Word and Zero
.text:823C9554                 addi    %r5, %r5, 2     # Add Immediate
.text:823C9558                 stw     %r8, 0(%r29)    # Store Word
.text:823C955C                 subf    %r7, %r8, %r4   # Subtract from
.text:823C9560
.text:823C9560 loc_823C9560:                           # CODE XREF: sub_823C9240+34Cj
.text:823C9560                 and     %r8, %r7, %r3   # AND
.text:823C9564                 cmpwi   cr6, %r4, 0x101 # Compare Word Immediate
.text:823C9568                 lbzx    %r8, %r8, %r30  # Load Byte and Zero Indexed
.text:823C956C                 stbx    %r8, %r30, %r4  # Store Byte Indexed
.text:823C9570                 bge     cr6, loc_823C9580 # Branch if greater than or equal
.text:823C9574                 lwz     %r6, 4(%r11)    # Load Word and Zero
.text:823C9578                 add     %r6, %r6, %r30  # Add
.text:823C957C                 stbx    %r8, %r6, %r4   # Store Byte Indexed
.text:823C9580
.text:823C9580 loc_823C9580:                           # CODE XREF: sub_823C9240+330j
.text:823C9580                 addic.  %r5, %r5, -1    # Add Immediate Carrying
.text:823C9584                 addi    %r4, %r4, 1     # Add Immediate
.text:823C9588                 addi    %r7, %r7, 1     # Add Immediate
.text:823C958C                 bgt     loc_823C9560    # Branch if greater than
.text:823C9590
.text:823C9590 loc_823C9590:                           # CODE XREF: sub_823C9240+48j
.text:823C9590                                         # sub_823C9240+F4j
.text:823C9590                 cmpw    cr6, %r4, %r26  # Compare Word
.text:823C9594                 blt     cr6, loc_823C928C # Branch if less than
.text:823C9598                 stb     %r9, 0x2EB4(%r11) # Store Byte
.text:823C959C                 mr      %r3, %r4        # Move Register
.text:823C95A0                 stw     %r10, 0x2EB0(%r11) # Store Word
.text:823C95A4                 stw     %r31, 0x2B04(%r11) # Store Word
.text:823C95A8
.text:823C95A8 loc_823C95A8:                           # CODE XREF: sub_823C9240+370j
.text:823C95A8                 b       __restgprlr_18  # Branch
.text:823C95AC # ---------------------------------------------------------------------------
.text:823C95AC
.text:823C95AC loc_823C95AC:                           # CODE XREF: sub_823C9240+90j
.text:823C95AC                 li      %r3, -1         # Load Immediate
.text:823C95B0                 b       loc_823C95A8    # Branch
.text:823C95B0 # End of function sub_823C9240
.text:823C95B0
.text:823C95B0 # ---------------------------------------------------------------------------











.text:823C95B8 # ---------------------------------------------------------------------------
.text:823C95B8
.text:823C95B8 loc_823C95B8:                           # CODE XREF: sub_823C99E0+64p
.text:823C95B8                 mflr    %r12            # Move from link register
.text:823C95BC                 bl      __savegprlr_14  # Branch
.text:823C95C0                 mr      %r11, %r3       # Move Register
.text:823C95C4                 add     %r28, %r4, %r5  # Add
.text:823C95C8                 addi    %r26, %r11, 0x2EB4 # Add Immediate
.text:823C95CC                 addi    %r25, %r11, 0x2EB0 # Add Immediate
.text:823C95D0                 addi    %r24, %r11, 0x2B04 # Add Immediate
.text:823C95D4                 lwz     %r14, 0x2B08(%r11) # Load Word and Zero
.text:823C95D8                 addi    %r18, %r11, 0xE3C # Add Immediate
.text:823C95DC                 lwz     %r29, 0(%r11)   # Load Word and Zero
.text:823C95E0                 addi    %r17, %r11, 0x233C # Add Immediate
.text:823C95E4                 lbz     %r8, 0x2EB4(%r11) # Load Byte and Zero
.text:823C95E8                 addi    %r16, %r11, 0x18 # Add Immediate
.text:823C95EC                 lwz     %r10, 0x2EB0(%r11) # Load Word and Zero
.text:823C95F0                 addi    %r15, %r11, 0x818 # Add Immediate
.text:823C95F4                 lwz     %r30, 0x2B04(%r11) # Load Word and Zero
.text:823C95F8                 addi    %r20, %r11, 0xA18 # Add Immediate
.text:823C95FC                 addi    %r19, %r11, 0xCB8 # Add Immediate
.text:823C9600                 addi    %r23, %r11, 0xDB4 # Add Immediate
.text:823C9604                 addi    %r22, %r11, 0xE34 # Add Immediate
.text:823C9608                 addi    %r27, %r11, 0xC # Add Immediate
.text:823C960C                 cmpw    cr6, %r4, %r28  # Compare Word
.text:823C9610                 bge     cr6, loc_823C99B8 # Branch if greater than or equal
.text:823C9614                 lis     %r9, unk_82026940@h # Load Immediate Shifted
.text:823C9618                 li      %r21, 0x10      # Load Immediate
.text:823C961C                 addi    %r9, %r9, unk_82026940@l # Add Immediate
.text:823C961C # ---------------------------------------------------------------------------
.text:823C9620                 .long 0x100048C3
.text:823C9624 # ---------------------------------------------------------------------------
.text:823C9624
.text:823C9624 loc_823C9624:                           # CODE XREF: .text:823C99B4j
.text:823C9624                 rlwinm  %r9, %r10, 11,21,30 # Rotate Left Word Immediate then AND with Mask
.text:823C9628                 lhax    %r7, %r9, %r16  # Load Half Word Algebraic Indexed
.text:823C962C                 cmpwi   %r7, 0          # Compare Word Immediate
.text:823C9630                 bge     loc_823C9664    # Branch if greater than or equal
.text:823C9634                 lis     %r9, 0x20       # Load Immediate Shifted
.text:823C9638
.text:823C9638 loc_823C9638:                           # CODE XREF: .text:823C9660j
.text:823C9638                 and     %r6, %r9, %r10  # AND
.text:823C963C                 slwi    %r7, %r7, 1     # Shift Left Immediate
.text:823C9640                 cntlzw  %r6, %r6        # Count Leading Zeros Word
.text:823C9644                 srwi    %r9, %r9, 1     # Shift Right Immediate
.text:823C9648                 srwi    %r6, %r6, 5     # Shift Right Immediate
.text:823C964C                 xori    %r6, %r6, 1     # XOR Immediate
.text:823C9650                 subf    %r7, %r7, %r6   # Subtract from
.text:823C9654                 slwi    %r7, %r7, 1     # Shift Left Immediate
.text:823C9658                 lhax    %r7, %r7, %r18  # Load Half Word Algebraic Indexed
.text:823C965C                 cmpwi   %r7, 0          # Compare Word Immediate
.text:823C9660                 blt     loc_823C9638    # Branch if less than
.text:823C9664
.text:823C9664 loc_823C9664:                           # CODE XREF: .text:823C9630j
.text:823C9664                 cmplw   cr6, %r30, %r14 # Compare Logical Word
.text:823C9668                 bge     cr6, loc_823C99D8 # Branch if greater than or equal
.text:823C966C                 lbzx    %r9, %r7, %r20  # Load Byte and Zero Indexed
.text:823C9670                 subf    %r8, %r9, %r8   # Subtract from
.text:823C9674                 slw     %r10, %r10, %r9 # Shift Left Word
.text:823C9678                 extsb   %r8, %r8        # Extend Sign Byte
.text:823C967C                 mr.     %r9, %r8        # Move Register
.text:823C9680                 bgt     loc_823C96AC    # Branch if greater than
.text:823C9684                 lbz     %r8, 1(%r30)    # Load Byte and Zero
.text:823C9688                 neg     %r6, %r9        # Negate
.text:823C968C                 lbz     %r5, 0(%r30)    # Load Byte and Zero
.text:823C9690                 addi    %r9, %r9, 0x10  # Add Immediate
.text:823C9694                 rotlwi  %r8, %r8, 8     # Rotate Left Immediate
.text:823C9698                 addi    %r30, %r30, 2   # Add Immediate
.text:823C969C                 or      %r5, %r8, %r5   # OR
.text:823C96A0                 extsb   %r8, %r9        # Extend Sign Byte
.text:823C96A4                 slw     %r9, %r5, %r6   # Shift Left Word
.text:823C96A8                 or      %r10, %r9, %r10 # OR
.text:823C96AC
.text:823C96AC loc_823C96AC:                           # CODE XREF: .text:823C9680j
.text:823C96AC                 addic.  %r6, %r7, -0x100 # Add Immediate Carrying
.text:823C96B0                 bge     loc_823C96C0    # Branch if greater than or equal
.text:823C96B4                 stbx    %r6, %r29, %r4  # Store Byte Indexed
.text:823C96B8                 addi    %r4, %r4, 1     # Add Immediate
.text:823C96BC                 b       loc_823C99B0    # Branch
.text:823C96C0 # ---------------------------------------------------------------------------
.text:823C96C0
.text:823C96C0 loc_823C96C0:                           # CODE XREF: .text:823C96B0j
.text:823C96C0                 clrlwi  %r5, %r6, 29    # Clear Left Immediate
.text:823C96C4                 cmpwi   cr6, %r5, 7     # Compare Word Immediate
.text:823C96C8                 bne     cr6, loc_823C9750 # Branch if not equal
.text:823C96CC                 rlwinm  %r9, %r10, 9,23,30 # Rotate Left Word Immediate then AND with Mask
.text:823C96D0                 lhax    %r9, %r9, %r15  # Load Half Word Algebraic Indexed
.text:823C96D4                 cmpwi   %r9, 0          # Compare Word Immediate
.text:823C96D8                 bge     loc_823C970C    # Branch if greater than or equal
.text:823C96DC                 lis     %r7, 0x80       # Load Immediate Shifted
.text:823C96E0
.text:823C96E0 loc_823C96E0:                           # CODE XREF: .text:823C9708j
.text:823C96E0                 and     %r5, %r7, %r10  # AND
.text:823C96E4                 slwi    %r9, %r9, 1     # Shift Left Immediate
.text:823C96E8                 cntlzw  %r5, %r5        # Count Leading Zeros Word
.text:823C96EC                 srwi    %r7, %r7, 1     # Shift Right Immediate
.text:823C96F0                 srwi    %r5, %r5, 5     # Shift Right Immediate
.text:823C96F4                 xori    %r5, %r5, 1     # XOR Immediate
.text:823C96F8                 subf    %r9, %r9, %r5   # Subtract from
.text:823C96FC                 slwi    %r9, %r9, 1     # Shift Left Immediate
.text:823C9700                 lhax    %r9, %r9, %r17  # Load Half Word Algebraic Indexed
.text:823C9704                 cmpwi   %r9, 0          # Compare Word Immediate
.text:823C9708                 blt     loc_823C96E0    # Branch if less than
.text:823C970C
.text:823C970C loc_823C970C:                           # CODE XREF: .text:823C96D8j
.text:823C970C                 lbzx    %r7, %r9, %r19  # Load Byte and Zero Indexed
.text:823C9710                 subf    %r8, %r7, %r8   # Subtract from
.text:823C9714                 slw     %r10, %r10, %r7 # Shift Left Word
.text:823C9718                 extsb   %r8, %r8        # Extend Sign Byte
.text:823C971C                 mr.     %r7, %r8        # Move Register
.text:823C9720                 bgt     loc_823C974C    # Branch if greater than
.text:823C9724                 lbz     %r8, 1(%r30)    # Load Byte and Zero
.text:823C9728                 neg     %r5, %r7        # Negate
.text:823C972C                 lbz     %r3, 0(%r30)    # Load Byte and Zero
.text:823C9730                 addi    %r7, %r7, 0x10  # Add Immediate
.text:823C9734                 rotlwi  %r8, %r8, 8     # Rotate Left Immediate
.text:823C9738                 addi    %r30, %r30, 2   # Add Immediate
.text:823C973C                 or      %r3, %r8, %r3   # OR
.text:823C9740                 extsb   %r8, %r7        # Extend Sign Byte
.text:823C9744                 slw     %r7, %r3, %r5   # Shift Left Word
.text:823C9748                 or      %r10, %r7, %r10 # OR
.text:823C974C
.text:823C974C loc_823C974C:                           # CODE XREF: .text:823C9720j
.text:823C974C                 addi    %r5, %r9, 7     # Add Immediate
.text:823C9750
.text:823C9750 loc_823C9750:                           # CODE XREF: .text:823C96C8j
.text:823C9750                 srawi   %r9, %r6, 3     # Shift Right Algebraic Word Immediate
.text:823C9754                 extsb   %r6, %r9        # Extend Sign Byte
.text:823C9758                 cmpwi   cr6, %r6, 2     # Compare Word Immediate
.text:823C975C                 ble     cr6, loc_823C98CC # Branch if less than or equal
.text:823C9760                 add     %r9, %r6, %r11  # Add
.text:823C9764                 lbz     %r7, 0x2EE4(%r9) # Load Byte and Zero
.text:823C9768                 cmplwi  cr6, %r7, 3     # Compare Logical Word Immediate
.text:823C976C                 blt     cr6, loc_823C9844 # Branch if less than
.text:823C9770                 addic.  %r7, %r7, -3    # Add Immediate Carrying
.text:823C9774                 beq     loc_823C97D4    # Branch if equal
.text:823C9778                 lbz     %r9, 0x2EE4(%r9) # Load Byte and Zero
.text:823C977C                 extsb   %r8, %r8        # Extend Sign Byte
.text:823C9780                 extsb   %r7, %r9        # Extend Sign Byte
.text:823C9784                 addi    %r3, %r9, -3    # Add Immediate
.text:823C9788                 subf    %r8, %r7, %r8   # Subtract from
.text:823C978C                 subfic  %r7, %r9, 0x23  # Subtract from Immediate Carrying
.text:823C9790                 addi    %r8, %r8, 3     # Add Immediate
.text:823C9794                 srw     %r7, %r10, %r7  # Shift Right Word
.text:823C9798                 extsb   %r8, %r8        # Extend Sign Byte
.text:823C979C                 slw     %r10, %r10, %r3 # Shift Left Word
.text:823C97A0                 mr.     %r9, %r8        # Move Register
.text:823C97A4                 bgt     loc_823C97D8    # Branch if greater than
.text:823C97A8                 lbz     %r8, 1(%r30)    # Load Byte and Zero
.text:823C97AC                 neg     %r3, %r9        # Negate
.text:823C97B0                 lbz     %r31, 0(%r30)   # Load Byte and Zero
.text:823C97B4                 addi    %r9, %r9, 0x10  # Add Immediate
.text:823C97B8                 rotlwi  %r8, %r8, 8     # Rotate Left Immediate
.text:823C97BC                 addi    %r30, %r30, 2   # Add Immediate
.text:823C97C0                 or      %r31, %r8, %r31 # OR
.text:823C97C4                 extsb   %r8, %r9        # Extend Sign Byte
.text:823C97C8                 slw     %r9, %r31, %r3  # Shift Left Word
.text:823C97CC                 or      %r10, %r9, %r10 # OR
.text:823C97D0                 b       loc_823C97D8    # Branch
.text:823C97D4 # ---------------------------------------------------------------------------
.text:823C97D4
.text:823C97D4 loc_823C97D4:                           # CODE XREF: .text:823C9774j
.text:823C97D4                 li      %r7, 0          # Load Immediate
.text:823C97D8
.text:823C97D8 loc_823C97D8:                           # CODE XREF: .text:823C97A4j
.text:823C97D8                                         # .text:823C97D0j
.text:823C97D8                 srwi    %r3, %r10, 25   # Shift Right Immediate
.text:823C97DC                 slwi    %r9, %r7, 3     # Shift Left Immediate
.text:823C97E0                 addi    %r7, %r6, 0xBC6 # Add Immediate
.text:823C97E4                 mr      %r31, %r8       # Move Register
.text:823C97E8                 slwi    %r8, %r7, 2     # Shift Left Immediate
.text:823C97EC                 lbzx    %r7, %r3, %r23  # Load Byte and Zero Indexed
.text:823C97F0                 extsb   %r7, %r7        # Extend Sign Byte
.text:823C97F4                 lwzx    %r8, %r8, %r11  # Load Word and Zero Indexed
.text:823C97F8                 add     %r6, %r8, %r9   # Add
.text:823C97FC                 lbzx    %r9, %r7, %r22  # Load Byte and Zero Indexed
.text:823C9800                 subf    %r8, %r9, %r31  # Subtract from
.text:823C9804                 slw     %r10, %r10, %r9 # Shift Left Word
.text:823C9808                 extsb   %r8, %r8        # Extend Sign Byte
.text:823C980C                 mr.     %r9, %r8        # Move Register
.text:823C9810                 bgt     loc_823C983C    # Branch if greater than
.text:823C9814                 lbz     %r8, 1(%r30)    # Load Byte and Zero
.text:823C9818                 neg     %r3, %r9        # Negate
.text:823C981C                 lbz     %r31, 0(%r30)   # Load Byte and Zero
.text:823C9820                 addi    %r9, %r9, 0x10  # Add Immediate
.text:823C9824                 rotlwi  %r8, %r8, 8     # Rotate Left Immediate
.text:823C9828                 addi    %r30, %r30, 2   # Add Immediate
.text:823C982C                 or      %r31, %r8, %r31 # OR
.text:823C9830                 extsb   %r8, %r9        # Extend Sign Byte
.text:823C9834                 slw     %r9, %r31, %r3  # Shift Left Word
.text:823C9838                 or      %r10, %r9, %r10 # OR
.text:823C983C
.text:823C983C loc_823C983C:                           # CODE XREF: .text:823C9810j
.text:823C983C                 add     %r9, %r6, %r7   # Add
.text:823C9840                 b       loc_823C98B8    # Branch
.text:823C9844 # ---------------------------------------------------------------------------
.text:823C9844
.text:823C9844 loc_823C9844:                           # CODE XREF: .text:823C976Cj
.text:823C9844                 cmplwi  cr6, %r7, 0     # Compare Logical Word Immediate
.text:823C9848                 beq     cr6, loc_823C98AC # Branch if equal
.text:823C984C                 lbz     %r9, 0x2EE4(%r9) # Load Byte and Zero
.text:823C9850                 mr      %r3, %r9        # Move Register
.text:823C9854                 subf    %r9, %r9, %r8   # Subtract from
.text:823C9858                 subfic  %r7, %r3, 0x20  # Subtract from Immediate Carrying
.text:823C985C                 extsb   %r8, %r9        # Extend Sign Byte
.text:823C9860                 srw     %r7, %r10, %r7  # Shift Right Word
.text:823C9864                 mr.     %r9, %r8        # Move Register
.text:823C9868                 slw     %r10, %r10, %r3 # Shift Left Word
.text:823C986C                 bgt     loc_823C9898    # Branch if greater than
.text:823C9870                 lbz     %r8, 1(%r30)    # Load Byte and Zero
.text:823C9874                 neg     %r3, %r9        # Negate
.text:823C9878                 lbz     %r31, 0(%r30)   # Load Byte and Zero
.text:823C987C                 addi    %r9, %r9, 0x10  # Add Immediate
.text:823C9880                 rotlwi  %r8, %r8, 8     # Rotate Left Immediate
.text:823C9884                 addi    %r30, %r30, 2   # Add Immediate
.text:823C9888                 or      %r31, %r8, %r31 # OR
.text:823C988C                 extsb   %r8, %r9        # Extend Sign Byte
.text:823C9890                 slw     %r9, %r31, %r3  # Shift Left Word
.text:823C9894                 or      %r10, %r9, %r10 # OR
.text:823C9898
.text:823C9898 loc_823C9898:                           # CODE XREF: .text:823C986Cj
.text:823C9898                 addi    %r9, %r6, 0xBC6 # Add Immediate
.text:823C989C                 slwi    %r9, %r9, 2     # Shift Left Immediate
.text:823C98A0                 lwzx    %r9, %r9, %r11  # Load Word and Zero Indexed
.text:823C98A4                 add     %r9, %r9, %r7   # Add
.text:823C98A8                 b       loc_823C98B8    # Branch
.text:823C98AC # ---------------------------------------------------------------------------
.text:823C98AC
.text:823C98AC loc_823C98AC:                           # CODE XREF: .text:823C9848j
.text:823C98AC                 addi    %r9, %r6, 0xBC6 # Add Immediate
.text:823C98B0                 slwi    %r9, %r9, 2     # Shift Left Immediate
.text:823C98B4                 lwzx    %r9, %r9, %r11  # Load Word and Zero Indexed
.text:823C98B8
.text:823C98B8 loc_823C98B8:                           # CODE XREF: .text:823C9840j
.text:823C98B8                                         # .text:823C98A8j
.text:823C98B8                 lwz     %r7, 4(%r27)    # Load Word and Zero
.text:823C98BC                 lwz     %r6, 0(%r27)    # Load Word and Zero
.text:823C98C0                 stw     %r7, 8(%r27)    # Store Word
.text:823C98C4                 stw     %r6, 4(%r27)    # Store Word
.text:823C98C8                 b       loc_823C98DC    # Branch
.text:823C98CC # ---------------------------------------------------------------------------
.text:823C98CC
.text:823C98CC loc_823C98CC:                           # CODE XREF: .text:823C975Cj
.text:823C98CC                 slwi    %r7, %r6, 2     # Shift Left Immediate
.text:823C98D0                 lwz     %r6, 0(%r27)    # Load Word and Zero
.text:823C98D4                 lwzx    %r9, %r7, %r27  # Load Word and Zero Indexed
.text:823C98D8                 stwx    %r6, %r7, %r27  # Store Word Indexed
.text:823C98DC
.text:823C98DC loc_823C98DC:                           # CODE XREF: .text:823C98C8j
.text:823C98DC                 lwz     %r7, 8(%r11)    # Load Word and Zero
.text:823C98E0                 subf    %r6, %r9, %r4   # Subtract from
.text:823C98E4                 stw     %r9, 0(%r27)    # Store Word
.text:823C98E8                 addi    %r9, %r5, 2     # Add Immediate
.text:823C98EC                 and     %r7, %r6, %r7   # AND
.text:823C98F0                 subf    %r6, %r7, %r4   # Subtract from
.text:823C98F4                 cmplw   cr6, %r6, %r9   # Compare Logical Word
.text:823C98F8                 bge     cr6, loc_823C9924 # Branch if greater than or equal
.text:823C98FC                 cmplwi  cr6, %r6, 0x10  # Compare Logical Word Immediate
.text:823C9900                 bge     cr6, loc_823C9924 # Branch if greater than or equal
.text:823C9904                 add     %r7, %r7, %r29  # Add
.text:823C9908
.text:823C9908 loc_823C9908:                           # CODE XREF: .text:823C991Cj
.text:823C9908                 lbz     %r6, 0(%r7)     # Load Byte and Zero
.text:823C990C                 addic.  %r9, %r9, -1    # Add Immediate Carrying
.text:823C9910                 addi    %r7, %r7, 1     # Add Immediate
.text:823C9914                 stbx    %r6, %r29, %r4  # Store Byte Indexed
.text:823C9918                 addi    %r4, %r4, 1     # Add Immediate
.text:823C991C                 bgt     loc_823C9908    # Branch if greater than
.text:823C9920                 b       loc_823C99B0    # Branch
.text:823C9924 # ---------------------------------------------------------------------------
.text:823C9924
.text:823C9924 loc_823C9924:                           # CODE XREF: .text:823C98F8j
.text:823C9924                                         # .text:823C9900j
.text:823C9924                 cmpwi   cr6, %r9, 0x10  # Compare Word Immediate
.text:823C9928                 ble     cr6, loc_823C9974 # Branch if less than or equal
.text:823C992C                 addi    %r6, %r9, -0x11 # Add Immediate
.text:823C9930                 add     %r31, %r29, %r4 # Add
.text:823C9934                 srwi    %r6, %r6, 4     # Shift Right Immediate
.text:823C9938                 add     %r3, %r7, %r29  # Add
.text:823C993C                 addi    %r5, %r6, 1     # Add Immediate
.text:823C9940                 slwi    %r6, %r5, 4     # Shift Left Immediate
.text:823C9944                 add     %r4, %r6, %r4   # Add
.text:823C9948                 subf    %r9, %r6, %r9   # Subtract from
.text:823C994C                 add     %r7, %r6, %r7   # Add
.text:823C994C # ---------------------------------------------------------------------------
.text:823C9950 dword_823C9950: .long 0x13E01C07        # CODE XREF: .text:823C9970j
.text:823C9954 # ---------------------------------------------------------------------------
.text:823C9954                 addic.  %r5, %r5, -1    # Add Immediate Carrying
.text:823C9954 # ---------------------------------------------------------------------------
.text:823C9958                 .long 0x13D51C47
.text:823C995C # ---------------------------------------------------------------------------
.text:823C995C                 addi    %r3, %r3, 0x10  # Add Immediate
.text:823C995C # ---------------------------------------------------------------------------
.text:823C9960                 .long 0x17FFF2F5, 0x13E0FD07, 0x13FFAD47
.text:823C996C # ---------------------------------------------------------------------------
.text:823C996C                 addi    %r31, %r31, 0x10 # Add Immediate
.text:823C9970                 bne     dword_823C9950  # Branch if not equal
.text:823C9974
.text:823C9974 loc_823C9974:                           # CODE XREF: .text:823C9928j
.text:823C9974                 add     %r6, %r29, %r4  # Add
.text:823C9978                 add     %r7, %r7, %r29  # Add
.text:823C997C                 li      %r5, -1         # Load Immediate
.text:823C997C # ---------------------------------------------------------------------------
.text:823C9980                 .long 0x13F53447, 0x13C03C07, 0x13B53C47, 0x13803407, 0x15BEEAF1, 0x7D89284C, 0x157CFAF1, 0x11806206, 0x11AB6B2A, 0x7DBD250E
.text:823C99A8 # ---------------------------------------------------------------------------
.text:823C99A8                 add     %r4, %r9, %r4   # Add
.text:823C99A8 # ---------------------------------------------------------------------------
.text:823C99AC                 .long 0x7DA6AD4E
.text:823C99B0 # ---------------------------------------------------------------------------
.text:823C99B0
.text:823C99B0 loc_823C99B0:                           # CODE XREF: .text:823C96BCj
.text:823C99B0                                         # .text:823C9920j
.text:823C99B0                 cmpw    cr6, %r4, %r28  # Compare Word
.text:823C99B4                 blt     cr6, loc_823C9624 # Branch if less than
.text:823C99B8
.text:823C99B8 loc_823C99B8:                           # CODE XREF: .text:823C9610j
.text:823C99B8                 lwz     %r9, 8(%r11)    # Load Word and Zero
.text:823C99BC                 subf    %r3, %r28, %r4  # Subtract from
.text:823C99C0                 stb     %r8, 0(%r26)    # Store Byte
.text:823C99C4                 and     %r9, %r9, %r4   # AND
.text:823C99C8                 stw     %r10, 0(%r25)   # Store Word
.text:823C99CC                 stw     %r30, 0(%r24)   # Store Word
.text:823C99D0                 stw     %r9, 0x2EB8(%r11) # Store Word
.text:823C99D4
.text:823C99D4 loc_823C99D4:                           # CODE XREF: .text:823C99DCj
.text:823C99D4                 b       __restgprlr_14  # Branch
.text:823C99D8 # ---------------------------------------------------------------------------
.text:823C99D8
.text:823C99D8 loc_823C99D8:                           # CODE XREF: .text:823C9668j
.text:823C99D8                 li      %r3, -1         # Load Immediate
.text:823C99DC                 b       loc_823C99D4    # Branch







.text:823C9A50 # =============== S U B R O U T I N E =======================================
.text:823C9A50
.text:823C9A50
.text:823C9A50 sub_823C9A50:                           # CODE XREF: compression_FFnew_inner_real_kind_final+25Cp
.text:823C9A50                 mflr    %r12            # Move from link register
.text:823C9A54                 bl      __savegprlr_28  # Branch
.text:823C9A58                 stwu    %sp, -0x80(%sp) # Store Word with Update
.text:823C9A5C                 mr      %r30, %r3       # Move Register
.text:823C9A60                 mr      %r29, %r4       # Move Register
.text:823C9A64                 mr      %r4, %r5        # Move Register
.text:823C9A68                 lwz     %r31, 0x2B0C(%r30) # Load Word and Zero
.text:823C9A6C                 cmplwi  cr6, %r31, 0    # Compare Logical Word Immediate
.text:823C9A70                 beq     cr6, loc_823C9AEC # Branch if equal
.text:823C9A74                 lwz     %r11, 0x2EBC(%r30) # Load Word and Zero
.text:823C9A78                 cmplwi  cr6, %r11, 0    # Compare Logical Word Immediate
.text:823C9A7C                 beq     cr6, loc_823C9A90 # Branch if equal
.text:823C9A80                 lwz     %r11, 0x2EC4(%r30) # Load Word and Zero
.text:823C9A84                 li      %r28, 1         # Load Immediate
.text:823C9A88                 cmplwi  cr6, %r11, -0x8000 # Compare Logical Word Immediate
.text:823C9A8C                 blt     cr6, loc_823C9A94 # Branch if less than
.text:823C9A90
.text:823C9A90 loc_823C9A90:                           # CODE XREF: sub_823C9A50+2Cj
.text:823C9A90                 li      %r28, 0         # Load Immediate
.text:823C9A94
.text:823C9A94 loc_823C9A94:                           # CODE XREF: sub_823C9A50+3Cj
.text:823C9A94                 lbz     %r11, 0x2FE4(%r30) # Load Byte and Zero
.text:823C9A98                 mr      %r5, %r29       # Move Register
.text:823C9A9C                 cmplwi  %r11, 0         # Compare Logical Word Immediate
.text:823C9AA0                 beq     loc_823C9AB0    # Branch if equal
.text:823C9AA4                 cmpwi   cr6, %r28, 0    # Compare Word Immediate
.text:823C9AA8                 beq     cr6, loc_823C9AF4 # Branch if equal
.text:823C9AAC                 lwz     %r31, 0x2FE8(%r30) # Load Word and Zero
.text:823C9AB0
.text:823C9AB0 loc_823C9AB0:                           # CODE XREF: sub_823C9A50+50j
.text:823C9AB0                 mr      %r3, %r31       # Move Register
.text:823C9AB4                 bl      loc_823720F0    # Branch
.text:823C9AB8
.text:823C9AB8 loc_823C9AB8:                           # CODE XREF: sub_823C9A50+ACj
.text:823C9AB8                 cmpwi   cr6, %r28, 0    # Compare Word Immediate
.text:823C9ABC                 beq     cr6, loc_823C9AEC # Branch if equal
.text:823C9AC0                 mr      %r5, %r29       # Move Register
.text:823C9AC4                 mr      %r4, %r31       # Move Register
.text:823C9AC8                 mr      %r3, %r30       # Move Register
.text:823C9ACC                 bl      loc_823C83F0    # Branch
.text:823C9AD0                 lbz     %r11, 0x2FE4(%r30) # Load Byte and Zero
.text:823C9AD4                 cmplwi  %r11, 0         # Compare Logical Word Immediate
.text:823C9AD8                 beq     loc_823C9AEC    # Branch if equal
.text:823C9ADC                 mr      %r5, %r29       # Move Register
.text:823C9AE0                 lwz     %r3, 0x2B0C(%r30) # Load Word and Zero
.text:823C9AE4                 mr      %r4, %r31       # Move Register
.text:823C9AE8                 bl      sub_82372040    # Branch
.text:823C9AEC
.text:823C9AEC loc_823C9AEC:                           # CODE XREF: sub_823C9A50+20j
.text:823C9AEC                                         # sub_823C9A50+6Cj ...
.text:823C9AEC                 addi    %sp, %sp, 0x80  # Add Immediate
.text:823C9AF0                 b       __restgprlr_28  # Branch
.text:823C9AF4 # ---------------------------------------------------------------------------
.text:823C9AF4
.text:823C9AF4 loc_823C9AF4:                           # CODE XREF: sub_823C9A50+58j
.text:823C9AF4                 mr      %r3, %r31       # Move Register
.text:823C9AF8                 bl      sub_82372040    # Branch
.text:823C9AFC                 b       loc_823C9AB8    # Branch
.text:823C9AFC # End of function sub_823C9A50









.text:82372040
.text:82372040 # =============== S U B R O U T I N E =======================================
.text:82372040
.text:82372040
.text:82372040 sub_82372040:                           # CODE XREF: sub_823C9A50+98p
.text:82372040                                         # sub_823C9A50+A8p
.text:82372040                 mflr    %r12            # Move from link register
.text:82372044                 bl      __savegprlr_27  # Branch
.text:82372048                 stwu    %sp, -0x80(%sp) # Store Word with Update
.text:8237204C                 mr      %r27, %r3       # Move Register
.text:82372050                 mr      %r31, %r5       # Move Register
.text:82372054                 neg     %r11, %r27      # Negate
.text:82372058                 mr      %r29, %r4       # Move Register
.text:8237205C                 mr      %r28, %r27      # Move Register
.text:82372060                 clrlwi  %r30, %r11, 25  # Clear Left Immediate
.text:82372064                 cmplwi  cr6, %r31, 0x100 # Compare Logical Word Immediate
.text:82372068                 bge     cr6, loc_82372078 # Branch if greater than or equal
.text:8237206C                 bl      memcpy          # r3 = dst_ptr
.text:8237206C                                         # r4 = src_ptr
.text:8237206C                                         # r5 = length
.text:82372070                 addi    %sp, %sp, 0x80  # Add Immediate
.text:82372074                 b       __restgprlr_27  # Branch
.text:82372078 # ---------------------------------------------------------------------------
.text:82372078
.text:82372078 loc_82372078:                           # CODE XREF: sub_82372040+28j
.text:82372078                 cmplwi  cr6, %r30, 0    # Compare Logical Word Immediate
.text:8237207C                 beq     cr6, loc_8237209C # Branch if equal
.text:82372080                 mr      %r5, %r30       # Move Register
.text:82372084                 mr      %r4, %r29       # Move Register
.text:82372088                 mr      %r3, %r27       # Move Register
.text:8237208C                 bl      memcpy          # r3 = dst_ptr
.text:8237208C                                         # r4 = src_ptr
.text:8237208C                                         # r5 = length
.text:82372090                 add     %r28, %r30, %r27 # Add
.text:82372094                 add     %r29, %r30, %r29 # Add
.text:82372098                 subf    %r31, %r30, %r31 # Subtract from
.text:8237209C
.text:8237209C loc_8237209C:                           # CODE XREF: sub_82372040+3Cj
.text:8237209C                 clrrwi  %r30, %r31, 7   # Clear Right Immediate
.text:823720A0                 clrlwi  %r11, %r29, 28  # Clear Left Immediate
.text:823720A4                 clrlwi  %r31, %r31, 25  # Clear Left Immediate
.text:823720A8                 cmplwi  cr6, %r11, 0    # Compare Logical Word Immediate
.text:823720AC                 mr      %r5, %r30       # Move Register
.text:823720B0                 mr      %r4, %r29       # Move Register
.text:823720B4                 mr      %r3, %r28       # Move Register
.text:823720B8                 beq     cr6, loc_823720C4 # Branch if equal
.text:823720BC                 bl      loc_8237ACB4    # Branch
.text:823720C0                 b       loc_823720C8    # Branch
.text:823720C4 # ---------------------------------------------------------------------------
.text:823720C4
.text:823720C4 loc_823720C4:                           # CODE XREF: sub_82372040+78j
.text:823720C4                 bl      loc_8237A990    # Branch
.text:823720C8
.text:823720C8 loc_823720C8:                           # CODE XREF: sub_82372040+80j
.text:823720C8                 add     %r3, %r30, %r28 # Add
.text:823720CC                 add     %r4, %r30, %r29 # Add
.text:823720D0                 cmplwi  cr6, %r31, 0    # Compare Logical Word Immediate
.text:823720D4                 beq     cr6, loc_823720E0 # Branch if equal
.text:823720D8                 mr      %r5, %r31       # Move Register
.text:823720DC                 bl      memcpy          # r3 = dst_ptr
.text:823720DC                                         # r4 = src_ptr
.text:823720DC                                         # r5 = length
.text:823720E0
.text:823720E0 loc_823720E0:                           # CODE XREF: sub_82372040+94j
.text:823720E0                 mr      %r3, %r27       # Move Register
.text:823720E4                 addi    %sp, %sp, 0x80  # Add Immediate
.text:823720E8                 b       __restgprlr_27  # Branch
.text:823720E8 # End of function sub_82372040
.text:823720E8

