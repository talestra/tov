//
// Xbox360 Basefile Info - Created by XexTool
//

#include <idc.idc>
#include <x360_imports.idc>


static MakeNameForce(addr, name)
{
    auto num, name_fixed;
    if( MakeNameEx(addr, name, SN_NOWARN) )
        return;
    for(num=0; num<999; num++)
    {
        name_fixed = form("%s_%d", name, num);
        if( MakeNameEx(addr, name_fixed, SN_NOWARN) )
            return;
    }
}

static GetSectionAddr(sectName)
{
	auto seg_addr, seg_base;
	seg_base = SegByName(sectName);
	return SegByBase(seg_base);
}

static SetupSection(startAddr, endAddr, segClass, perms, name, base)
{
    SetSelector(base, 0);
    SegCreate(startAddr, endAddr, base, 1, 3, 2);
    SegClass(startAddr, segClass);
    SegRename(startAddr, name);
    SetSegmentAttr(startAddr, SEGATTR_PERM, perms); // 4=read, 2=write, 1=execute
    SetSegmentAttr(startAddr, SEGATTR_FLAGS, 0x10); // SFL_LOADER
    SegDefReg(startAddr, "%r26", 0);
    SegDefReg(startAddr, "%r27", 0);
    SegDefReg(startAddr, "%r28", 0);
    SegDefReg(startAddr, "%r29", 0);
    SegDefReg(startAddr, "%r30", 0);
    SegDefReg(startAddr, "%r31", 0);
}

static SetupSections()
{
    auto addr;

    SetupSection(0x82000600, 0x8208C368, "DATA", 4, ".rdata", 1);
    SetupSection(0x8208C400, 0x820A2EC8, "DATA", 4, ".pdata", 2);
    SetupSection(0x820B0000, 0x824D3D84, "CODE", 5, ".text", 3);
    SetupSection(0x824D3E00, 0x824EBC64, "CODE", 5, ".embsec_", 4);
    SetupSection(0x824EBE00, 0x82509CEC, "CODE", 5, ".embsec_", 5);
    SetupSection(0x82509E00, 0x8250B8B8, "CODE", 5, ".embsec_", 6);
    SetupSection(0x8250BA00, 0x8251265C, "CODE", 5, ".embsec_", 7);
    SetupSection(0x82512800, 0x825290A4, "CODE", 5, ".embsec_", 8);
    SetupSection(0x82529200, 0x8252A478, "CODE", 5, ".embsec_", 9);
    SetupSection(0x8252A600, 0x8252A848, "CODE", 5, ".embsec_", 10);
    SetupSection(0x8252AA00, 0x8252D6C4, "CODE", 5, ".embsec_", 11);
    SetupSection(0x82530000, 0x82E92FB8, "DATA", 6, ".data", 12);
    SetupSection(0x82E93000, 0x82E93021, "DATA", 6, ".tls", 13);
    SetupSection(0x82E93200, 0x82E9320C, "DATA", 6, ".XBMOVIE", 14);

    // remove unused "leftovers" of the original binary segment
    while( (addr = SegByBase(0)) != BADADDR )
        DelSeg(addr, SEGMOD_KILL|SEGMOD_SILENT);
}

static SetupResources()
{
    SetupSection(0x82EA0000, 0x82F32EFD, "DATA", 4, "4E4D07E9", 15);
    SetupSection(0x82F32F00, 0x82F342CE, "DATA", 4, "FFFF7658", 16);
}

static RemoveEmptySections()
{
    auto seg_addr, seg_num;
    for(seg_num=0; seg_num<500; seg_num=seg_num+1)
    {
        seg_addr = GetSectionAddr(form( "seg%03d", seg_num) );
        if(seg_addr != -1)
            SegDelete(seg_addr, 1);
    }
}


static SetupImportFunc(importAddr, funcAddr, importNum, name)
{
    auto func_name;
    func_name = DoNameGen(name, 0, importNum);

    MakeNameForce(importAddr, "__imp__" + func_name);
    MakeDword(importAddr);

    PatchWord(funcAddr, 0x3860);
    PatchWord(funcAddr + 4, 0x3880);
    MakeUnknown(funcAddr, 0x10, 0); // DOUNK_SIMPLE
    MakeCode(funcAddr);
    MakeNameForce(funcAddr, func_name);
    MakeFunction(funcAddr, funcAddr + 0x10);
    SetFunctionFlags(funcAddr, FUNC_LIB);
}

static SetupImportData(importAddr, importNum, name)
{
    auto data_name;
    data_name = DoNameGen(name, 0, importNum);

    MakeNameForce(importAddr, data_name);
    MakeDword(importAddr);
}

static setupImports_0_xam_xex_0()
{
    SetupImportFunc(0x82000600, 0x824D31C4, 0x28B, "xam.xex");
    SetupImportFunc(0x82000604, 0x824D31D4, 0x2C1, "xam.xex");
    SetupImportFunc(0x82000608, 0x824D31E4, 0x2D5, "xam.xex");
    SetupImportFunc(0x8200060C, 0x824D31F4, 0x2C7, "xam.xex");
    SetupImportFunc(0x82000610, 0x824D3204, 0x2CB, "xam.xex");
    SetupImportFunc(0x82000614, 0x824D3214, 0x2CA, "xam.xex");
    SetupImportFunc(0x82000618, 0x824D3224, 0x2D9, "xam.xex");
    SetupImportFunc(0x8200061C, 0x824D3234, 0x28A, "xam.xex");
    SetupImportFunc(0x82000620, 0x824D3244, 0x250, "xam.xex");
    SetupImportFunc(0x82000624, 0x824D3254, 0x1F7, "xam.xex");
    SetupImportFunc(0x82000628, 0x824D3264, 0x210, "xam.xex");
    SetupImportFunc(0x8200062C, 0x824D3274, 0x213, "xam.xex");
    SetupImportFunc(0x82000630, 0x824D3284, 0x212, "xam.xex");
    SetupImportFunc(0x82000634, 0x824D3294, 0x282, "xam.xex");
    SetupImportFunc(0x82000638, 0x824D32A4, 0x2F7, "xam.xex");
    SetupImportFunc(0x8200063C, 0x824D32B4, 0x2EE, "xam.xex");
    SetupImportFunc(0x82000640, 0x824D32C4, 0x20A, "xam.xex");
    SetupImportFunc(0x82000644, 0x824D32D4, 0x190, "xam.xex");
    SetupImportFunc(0x82000648, 0x824D32E4, 0x191, "xam.xex");
    SetupImportFunc(0x8200064C, 0x824D32F4, 0x192, "xam.xex");
    SetupImportFunc(0x82000650, 0x824D3304, 0x198, "xam.xex");
    SetupImportFunc(0x82000654, 0x824D3314, 0x227, "xam.xex");
    SetupImportFunc(0x82000658, 0x824D3324, 0x259, "xam.xex");
    SetupImportFunc(0x8200065C, 0x824D3334, 0x25A, "xam.xex");
    SetupImportFunc(0x82000660, 0x824D3344, 0x25C, "xam.xex");
    SetupImportFunc(0x82000664, 0x824D3354, 0x265, "xam.xex");
    SetupImportFunc(0x82000668, 0x824D3364, 0x25E, "xam.xex");
    SetupImportFunc(0x8200066C, 0x824D3374, 0x2DC, "xam.xex");
    SetupImportFunc(0x82000670, 0x824D3384, 0x3CD, "xam.xex");
    SetupImportFunc(0x82000674, 0x824D3394, 0x3CB, "xam.xex");
    SetupImportFunc(0x82000678, 0x824D33A4, 0x1A9, "xam.xex");
    SetupImportFunc(0x8200067C, 0x824D33B4, 0x1A4, "xam.xex");
    SetupImportFunc(0x82000680, 0x824D33C4, 0x280, "xam.xex");
    SetupImportFunc(0x82000684, 0x824D33D4, 0x1B3, "xam.xex");
    SetupImportFunc(0x82000688, 0x824D33E4, 0x1B1, "xam.xex");
    SetupImportFunc(0x8200068C, 0x824D33F4, 0x1AF, "xam.xex");
    SetupImportFunc(0x82000690, 0x824D3C64, 0x1FC, "xam.xex");
    SetupImportFunc(0x82000694, 0x824D3C54, 0x001, "xam.xex");
    SetupImportFunc(0x82000698, 0x824D3B84, 0x1F4, "xam.xex");
    SetupImportFunc(0x8200069C, 0x824D3B74, 0x316, "xam.xex");
    SetupImportFunc(0x820006A0, 0x824D3B64, 0x317, "xam.xex");
    SetupImportFunc(0x820006A4, 0x824D3B54, 0x3D1, "xam.xex");
}

static setupImports_1_xboxkrnl_exe_0()
{
    SetupImportFunc(0x820006AC, 0x824D37D4, 0x259, "xboxkrnl.exe");
    SetupImportFunc(0x820006B0, 0x824D37E4, 0x257, "xboxkrnl.exe");
    SetupImportFunc(0x820006B4, 0x824D37F4, 0x256, "xboxkrnl.exe");
    SetupImportFunc(0x820006B8, 0x824D3804, 0x0DC, "xboxkrnl.exe");
    SetupImportFunc(0x820006BC, 0x824D3814, 0x0CC, "xboxkrnl.exe");
    SetupImportFunc(0x820006C0, 0x824D3824, 0x0DB, "xboxkrnl.exe");
    SetupImportFunc(0x820006C4, 0x824D3834, 0x03B, "xboxkrnl.exe");
    SetupImportFunc(0x820006C8, 0x824D3844, 0x103, "xboxkrnl.exe");
    SetupImportFunc(0x820006CC, 0x824D3854, 0x104, "xboxkrnl.exe");
    SetupImportFunc(0x820006D0, 0x824D3864, 0x08F, "xboxkrnl.exe");
    SetupImportFunc(0x820006D4, 0x824D3874, 0x015, "xboxkrnl.exe");
    SetupImportFunc(0x820006D8, 0x824D3884, 0x0F6, "xboxkrnl.exe");
    SetupImportData(0x820006DC,             0x0AD, "xboxkrnl.exe");
    SetupImportFunc(0x820006E0, 0x824D3894, 0x195, "xboxkrnl.exe");
    SetupImportFunc(0x820006E4, 0x824D38A4, 0x0F5, "xboxkrnl.exe");
    SetupImportFunc(0x820006E8, 0x824D38B4, 0x05A, "xboxkrnl.exe");
    SetupImportFunc(0x820006EC, 0x824D38C4, 0x0BA, "xboxkrnl.exe");
    SetupImportFunc(0x820006F0, 0x824D38D4, 0x0BD, "xboxkrnl.exe");
    SetupImportData(0x820006F4,             0x1AE, "xboxkrnl.exe");
    SetupImportFunc(0x820006F8, 0x824D38E4, 0x028, "xboxkrnl.exe");
    SetupImportFunc(0x820006FC, 0x824D38F4, 0x03C, "xboxkrnl.exe");
    SetupImportFunc(0x82000700, 0x824D3904, 0x126, "xboxkrnl.exe");
    SetupImportFunc(0x82000704, 0x824D3914, 0x053, "xboxkrnl.exe");
    SetupImportFunc(0x82000708, 0x824D3924, 0x066, "xboxkrnl.exe");
    SetupImportFunc(0x8200070C, 0x824D3934, 0x11B, "xboxkrnl.exe");
    SetupImportFunc(0x82000710, 0x824D3944, 0x0EE, "xboxkrnl.exe");
    SetupImportFunc(0x82000714, 0x824D3954, 0x136, "xboxkrnl.exe");
    SetupImportFunc(0x82000718, 0x824D3964, 0x019, "xboxkrnl.exe");
    SetupImportFunc(0x8200071C, 0x824D37C4, 0x25A, "xboxkrnl.exe");
    SetupImportFunc(0x82000720, 0x824D3984, 0x1C3, "xboxkrnl.exe");
    SetupImportFunc(0x82000724, 0x824D3994, 0x04D, "xboxkrnl.exe");
    SetupImportFunc(0x82000728, 0x824D39A4, 0x1D9, "xboxkrnl.exe");
    SetupImportFunc(0x8200072C, 0x824D39B4, 0x1DF, "xboxkrnl.exe");
    SetupImportData(0x82000730,             0x1BE, "xboxkrnl.exe");
    SetupImportFunc(0x82000734, 0x824D39C4, 0x0BE, "xboxkrnl.exe");
    SetupImportFunc(0x82000738, 0x824D39D4, 0x1B6, "xboxkrnl.exe");
    SetupImportFunc(0x8200073C, 0x824D39E4, 0x13B, "xboxkrnl.exe");
    SetupImportFunc(0x82000740, 0x824D39F4, 0x083, "xboxkrnl.exe");
    SetupImportFunc(0x82000744, 0x824D3A04, 0x1C7, "xboxkrnl.exe");
    SetupImportFunc(0x82000748, 0x824D3A14, 0x25B, "xboxkrnl.exe");
    SetupImportFunc(0x8200074C, 0x824D3A24, 0x1BD, "xboxkrnl.exe");
    SetupImportData(0x82000750,             0x266, "xboxkrnl.exe");
    SetupImportFunc(0x82000754, 0x824D3A34, 0x14D, "xboxkrnl.exe");
    SetupImportFunc(0x82000758, 0x824D3A44, 0x052, "xboxkrnl.exe");
    SetupImportFunc(0x8200075C, 0x824D3A54, 0x1CA, "xboxkrnl.exe");
    SetupImportFunc(0x82000760, 0x824D3A64, 0x1B1, "xboxkrnl.exe");
    SetupImportFunc(0x82000764, 0x824D3A74, 0x1C5, "xboxkrnl.exe");
    SetupImportFunc(0x82000768, 0x824D3A84, 0x1C9, "xboxkrnl.exe");
    SetupImportData(0x8200076C,             0x1C0, "xboxkrnl.exe");
    SetupImportFunc(0x82000770, 0x824D3A94, 0x1BA, "xboxkrnl.exe");
    SetupImportFunc(0x82000774, 0x824D3AA4, 0x1D3, "xboxkrnl.exe");
    SetupImportFunc(0x82000778, 0x824D3AB4, 0x1DC, "xboxkrnl.exe");
    SetupImportFunc(0x8200077C, 0x824D3AC4, 0x1C6, "xboxkrnl.exe");
    SetupImportFunc(0x82000780, 0x824D3AD4, 0x1D5, "xboxkrnl.exe");
    SetupImportFunc(0x82000784, 0x824D3AE4, 0x1C2, "xboxkrnl.exe");
    SetupImportFunc(0x82000788, 0x824D3AF4, 0x1B9, "xboxkrnl.exe");
    SetupImportFunc(0x8200078C, 0x824D3B04, 0x269, "xboxkrnl.exe");
    SetupImportFunc(0x82000790, 0x824D3B14, 0x26A, "xboxkrnl.exe");
    SetupImportData(0x82000794,             0x1C1, "xboxkrnl.exe");
    SetupImportFunc(0x82000798, 0x824D3B24, 0x1B4, "xboxkrnl.exe");
    SetupImportFunc(0x8200079C, 0x824D3B34, 0x06B, "xboxkrnl.exe");
    SetupImportFunc(0x820007A0, 0x824D3B44, 0x06C, "xboxkrnl.exe");
    SetupImportFunc(0x820007A4, 0x824D37B4, 0x045, "xboxkrnl.exe");
    SetupImportFunc(0x820007A8, 0x824D37A4, 0x084, "xboxkrnl.exe");
    SetupImportFunc(0x820007AC, 0x824D3794, 0x047, "xboxkrnl.exe");
    SetupImportFunc(0x820007B0, 0x824D3784, 0x034, "xboxkrnl.exe");
    SetupImportFunc(0x820007B4, 0x824D3B94, 0x152, "xboxkrnl.exe");
    SetupImportFunc(0x820007B8, 0x824D3BA4, 0x154, "xboxkrnl.exe");
    SetupImportFunc(0x820007BC, 0x824D3BB4, 0x155, "xboxkrnl.exe");
    SetupImportFunc(0x820007C0, 0x824D3BC4, 0x153, "xboxkrnl.exe");
    SetupImportFunc(0x820007C4, 0x824D3BD4, 0x147, "xboxkrnl.exe");
    SetupImportFunc(0x820007C8, 0x824D3BE4, 0x119, "xboxkrnl.exe");
    SetupImportFunc(0x820007CC, 0x824D3BF4, 0x0C6, "xboxkrnl.exe");
    SetupImportFunc(0x820007D0, 0x824D3C04, 0x14E, "xboxkrnl.exe");
    SetupImportFunc(0x820007D4, 0x824D3C14, 0x127, "xboxkrnl.exe");
    SetupImportFunc(0x820007D8, 0x824D3C24, 0x142, "xboxkrnl.exe");
    SetupImportFunc(0x820007DC, 0x824D3C34, 0x12D, "xboxkrnl.exe");
    SetupImportFunc(0x820007E0, 0x824D3C44, 0x133, "xboxkrnl.exe");
    SetupImportFunc(0x820007E4, 0x824D3774, 0x109, "xboxkrnl.exe");
    SetupImportFunc(0x820007E8, 0x824D3764, 0x149, "xboxkrnl.exe");
    SetupImportFunc(0x820007EC, 0x824D3754, 0x00D, "xboxkrnl.exe");
    SetupImportFunc(0x820007F0, 0x824D3744, 0x0B1, "xboxkrnl.exe");
    SetupImportFunc(0x820007F4, 0x824D3734, 0x0B4, "xboxkrnl.exe");
    SetupImportFunc(0x820007F8, 0x824D3724, 0x035, "xboxkrnl.exe");
    SetupImportFunc(0x820007FC, 0x824D3714, 0x192, "xboxkrnl.exe");
    SetupImportFunc(0x82000800, 0x824D3704, 0x140, "xboxkrnl.exe");
    SetupImportFunc(0x82000804, 0x824D36F4, 0x13F, "xboxkrnl.exe");
    SetupImportFunc(0x82000808, 0x824D36E4, 0x00F, "xboxkrnl.exe");
    SetupImportFunc(0x8200080C, 0x824D36D4, 0x00B, "xboxkrnl.exe");
    SetupImportFunc(0x82000810, 0x824D36C4, 0x09D, "xboxkrnl.exe");
    SetupImportFunc(0x82000814, 0x824D36B4, 0x0B0, "xboxkrnl.exe");
    SetupImportFunc(0x82000818, 0x824D36A4, 0x05F, "xboxkrnl.exe");
    SetupImportFunc(0x8200081C, 0x824D3694, 0x039, "xboxkrnl.exe");
    SetupImportFunc(0x82000820, 0x824D3684, 0x07D, "xboxkrnl.exe");
    SetupImportFunc(0x82000824, 0x824D3674, 0x037, "xboxkrnl.exe");
    SetupImportFunc(0x82000828, 0x824D3664, 0x10F, "xboxkrnl.exe");
    SetupImportFunc(0x8200082C, 0x824D3654, 0x041, "xboxkrnl.exe");
    SetupImportFunc(0x82000830, 0x824D3644, 0x197, "xboxkrnl.exe");
    SetupImportFunc(0x82000834, 0x824D3634, 0x0DA, "xboxkrnl.exe");
    SetupImportFunc(0x82000838, 0x824D3624, 0x0F0, "xboxkrnl.exe");
    SetupImportFunc(0x8200083C, 0x824D3614, 0x0F1, "xboxkrnl.exe");
    SetupImportFunc(0x82000840, 0x824D3604, 0x0E4, "xboxkrnl.exe");
    SetupImportFunc(0x82000844, 0x824D35F4, 0x1A5, "xboxkrnl.exe");
    SetupImportFunc(0x82000848, 0x824D35E4, 0x003, "xboxkrnl.exe");
    SetupImportFunc(0x8200084C, 0x824D35D4, 0x010, "xboxkrnl.exe");
    SetupImportFunc(0x82000850, 0x824D35C4, 0x001, "xboxkrnl.exe");
    SetupImportData(0x82000854,             0x059, "xboxkrnl.exe");
    SetupImportFunc(0x82000858, 0x824D35B4, 0x097, "xboxkrnl.exe");
    SetupImportFunc(0x8200085C, 0x824D35A4, 0x0FC, "xboxkrnl.exe");
    SetupImportFunc(0x82000860, 0x824D3594, 0x081, "xboxkrnl.exe");
    SetupImportData(0x82000864,             0x01B, "xboxkrnl.exe");
    SetupImportFunc(0x82000868, 0x824D3584, 0x110, "xboxkrnl.exe");
    SetupImportFunc(0x8200086C, 0x824D3574, 0x099, "xboxkrnl.exe");
    SetupImportFunc(0x82000870, 0x824D3C74, 0x1F7, "xboxkrnl.exe");
    SetupImportFunc(0x82000874, 0x824D3C84, 0x088, "xboxkrnl.exe");
    SetupImportFunc(0x82000878, 0x824D3C94, 0x0AF, "xboxkrnl.exe");
    SetupImportFunc(0x8200087C, 0x824D3CA4, 0x0B3, "xboxkrnl.exe");
    SetupImportFunc(0x82000880, 0x824D3CB4, 0x1F8, "xboxkrnl.exe");
    SetupImportFunc(0x82000884, 0x824D3CC4, 0x092, "xboxkrnl.exe");
    SetupImportFunc(0x82000888, 0x824D3CD4, 0x074, "xboxkrnl.exe");
    SetupImportFunc(0x8200088C, 0x824D3CE4, 0x085, "xboxkrnl.exe");
    SetupImportFunc(0x82000890, 0x824D3CF4, 0x141, "xboxkrnl.exe");
    SetupImportFunc(0x82000894, 0x824D3D04, 0x224, "xboxkrnl.exe");
    SetupImportFunc(0x82000898, 0x824D3D14, 0x226, "xboxkrnl.exe");
    SetupImportFunc(0x8200089C, 0x824D3D24, 0x1F5, "xboxkrnl.exe");
    SetupImportFunc(0x820008A0, 0x824D3D34, 0x1F4, "xboxkrnl.exe");
    SetupImportFunc(0x820008A4, 0x824D3D44, 0x1F3, "xboxkrnl.exe");
    SetupImportFunc(0x820008A8, 0x824D3D54, 0x0D5, "xboxkrnl.exe");
    SetupImportFunc(0x820008AC, 0x824D3D64, 0x0F3, "xboxkrnl.exe");
    SetupImportFunc(0x820008B0, 0x824D3D74, 0x009, "xboxkrnl.exe");
    SetupImportFunc(0x820008B4, 0x824D3564, 0x105, "xboxkrnl.exe");
    SetupImportData(0x820008B8,             0x158, "xboxkrnl.exe");
    SetupImportData(0x820008BC,             0x194, "xboxkrnl.exe");
}

static setupImports_1_xboxkrnl_exe_1()
{
    SetupImportData(0x824D3554,             0x194, "xboxkrnl.exe");
    SetupImportFunc(0x820008C0, 0x824D3544, 0x0E7, "xboxkrnl.exe");
    SetupImportFunc(0x820008C4, 0x824D3534, 0x13A, "xboxkrnl.exe");
    SetupImportFunc(0x820008C8, 0x824D3524, 0x135, "xboxkrnl.exe");
    SetupImportFunc(0x820008CC, 0x824D3514, 0x19B, "xboxkrnl.exe");
    SetupImportFunc(0x820008D0, 0x824D3504, 0x0D9, "xboxkrnl.exe");
    SetupImportFunc(0x820008D4, 0x824D34F4, 0x0D2, "xboxkrnl.exe");
    SetupImportData(0x820008D8,             0x193, "xboxkrnl.exe");
    SetupImportFunc(0x820008DC, 0x824D34E4, 0x12B, "xboxkrnl.exe");
    SetupImportFunc(0x820008E0, 0x824D34D4, 0x0E8, "xboxkrnl.exe");
    SetupImportFunc(0x820008E4, 0x824D34C4, 0x0DF, "xboxkrnl.exe");
    SetupImportFunc(0x820008E8, 0x824D34B4, 0x0EF, "xboxkrnl.exe");
    SetupImportFunc(0x820008EC, 0x824D34A4, 0x0FF, "xboxkrnl.exe");
    SetupImportFunc(0x820008F0, 0x824D3494, 0x0CF, "xboxkrnl.exe");
    SetupImportFunc(0x820008F4, 0x824D3484, 0x11D, "xboxkrnl.exe");
    SetupImportData(0x820008F8,             0x156, "xboxkrnl.exe");
    SetupImportFunc(0x820008FC, 0x824D3474, 0x0F7, "xboxkrnl.exe");
    SetupImportFunc(0x82000900, 0x824D3464, 0x125, "xboxkrnl.exe");
    SetupImportFunc(0x82000904, 0x824D3454, 0x130, "xboxkrnl.exe");
    SetupImportFunc(0x82000908, 0x824D3444, 0x12E, "xboxkrnl.exe");
    SetupImportFunc(0x8200090C, 0x824D3434, 0x12C, "xboxkrnl.exe");
    SetupImportFunc(0x82000910, 0x824D3424, 0x0FD, "xboxkrnl.exe");
    SetupImportFunc(0x82000914, 0x824D3414, 0x0D1, "xboxkrnl.exe");
    SetupImportFunc(0x82000918, 0x824D3404, 0x0C4, "xboxkrnl.exe");
    SetupImportFunc(0x8200091C, 0x824D3974, 0x089, "xboxkrnl.exe");
}

static SetupImports()
{
    setupImports_0_xam_xex_0();
    setupImports_1_xboxkrnl_exe_0();
    setupImports_1_xboxkrnl_exe_1();
}


static SetupExportFunc(funcAddr, exportNum, funcName)
{
    MakeUnkn(funcAddr, 0);
    MakeCode(funcAddr); 
    MakeNameForce(funcAddr, funcName);
    MakeFunction(funcAddr, BADADDR);
    AddEntryPoint(exportNum, funcAddr, funcName, 1);
}

static SetupExportData(dataAddr, exportNum, name)
{
    auto data_name;
    data_name = DoNameGen(name, 0, exportNum);

    AddEntryPoint(exportNum, dataAddr, data_name, 0);
    MakeNameForce(dataAddr, data_name);
    MakeDword(dataAddr);
}

static SetupExports()
{
    auto name;
    name = GetInputFile();


    // set start entry point
    SetupExportFunc(0x8237F018, 0x8237F018, "start");
}

static SetupExportsByName()
{
}

static SetupRegSaves()
{
	auto currAddr, i;
	
	// find all saves of gp regs
	for(currAddr=0; currAddr != BADADDR; currAddr=currAddr+4)
	{
		// find "std %r14, -0x98(%sp)" followed by "std %r15, -0x90(%sp)"
		currAddr = FindBinary(currAddr, SEARCH_DOWN, "F9 C1 FF 68 F9 E1 FF 70");
		if(currAddr == BADADDR)
			break;
		for(i=14; i<=31; i++)
		{
			MakeUnknown(currAddr, 4, 0); // DOUNK_SIMPLE
			MakeCode(currAddr);
			if(i != 31)
				MakeFunction(currAddr, currAddr + 4);
			else
				MakeFunction(currAddr, currAddr + 0x0C);
			MakeNameForce(currAddr, form("__savegprlr_%d", i));
			currAddr = currAddr + 4;
		}
	}
	
	// find all loads of gp regs
	for(currAddr=0; currAddr != BADADDR; currAddr=currAddr+4)
	{
		// find "ld  %r14, var_98(%sp)" followed by "ld  %r15, var_90(%sp)"
		currAddr = FindBinary(currAddr, SEARCH_DOWN, "E9 C1 FF 68 E9 E1 FF 70");
		if(currAddr == BADADDR)
			break;
		for(i=14; i<=31; i++)
		{
			MakeUnknown(currAddr, 4, 0); // DOUNK_SIMPLE
			MakeCode(currAddr);
			if(i != 31)
				MakeFunction(currAddr, currAddr + 4);
			else
				MakeFunction(currAddr, currAddr + 0x10);
			MakeNameForce(currAddr, form("__restgprlr_%d", i));
			currAddr = currAddr + 4;
		}
	}
}

static ConvertToCode(startAddr, endAddr)
{
    auto addr;
    if(startAddr == BADADDR || endAddr == BADADDR || startAddr>endAddr)
        return;
    
    MakeUnknown(startAddr, endAddr-startAddr, 0); // DOUNK_SIMPLE
    for(addr=startAddr&0xFFFFFFFC; addr<endAddr; addr=addr+4)
    {
        MakeCode(addr);
    }
    AnalyzeArea(startAddr, endAddr);
}

static main()
{
    // ensure file was loaded in as binary
    // if it was loaded in as PE then addresses will be incorrect
    if( GetShortPrm(INF_FILETYPE) != FT_BIN )
    {
        Warning("The file must be loaded as a BINARY file to use this script.\n"
                "Close this database and create a new one, ensuring you\n"
                "select \"Binary File\" on IDAs \"Load a new file\" dialog.");
        return;
    }
    
    // ensure file was loaded in as PPC
    if( GetCharPrm(INF_PROCNAME+0) != 'P' ||
        GetCharPrm(INF_PROCNAME+1) != 'P' ||
        GetCharPrm(INF_PROCNAME+2) != 'C' ||
        GetCharPrm(INF_PROCNAME+3) != '\0' )
    {
        Warning("The file must be loaded for the PPC processor.\n"
                "Close this database and create a new one, ensuring you\n"
                "select \"PowerPC: ppc\" on IDAs \"Load a new file\" dialog.");
        return;
    }

    // set up resources
    if( 1 == AskYN(0, "Would you like to load reources as segments?") )
        SetupResources();

    // set up sections
    SetupSections();

    // remove empty sections
    RemoveEmptySections();

    // analyse code
    if( 1 == AskYN(1, "Would you like to analyse the file as code?") )
        ConvertToCode( GetSectionAddr(".text"), SegEnd(GetSectionAddr(".text")) );

    // set up imports
    SetupImports();

    // set up exports
    SetupExports();

    // set up exports by name
    SetupExportsByName();

    // setup all reg loads/stores
    SetupRegSaves();

    // done
    Message("done\n\n");
}

