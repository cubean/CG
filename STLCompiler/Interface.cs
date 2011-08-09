using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using CoMsBlock200Lib;

namespace STLCompiler
{
    [ComVisible(true)]
    public enum eBLOCK200_ADDRESSING_MODE
    {
        eBLOCK200_ADDRESSING_MODE_DIRECT_ADDRESS,
        eBLOCK200_ADDRESSING_MODE_ADDRESS_OF,
        eBLOCK200_ADDRESSING_MODE_INDIRECT_ADDRESS,
        eBLOCK200_ADDRESSING_MODE_INVALID_MODE
    }

    [ComVisible(true)]
    public enum eBLOCK200_BLOCKTYPES
    {
        eBLOCK200_BLOCKTYPE_ALL,
        eBLOCK200_BLOCKTYPE_OB,
        eBLOCK200_BLOCKTYPE_DB,
        eBLOCK200_BLOCKTYPE_SDB,
        eBLOCK200_BLOCKTYPE_OB_802,
        eBLOCK200_BLOCKTYPE_DB_802,
        eBLOCK200_BLOCKTYPE_SDB_802
    }

    [ComVisible(true)]
    public enum eBLOCK200_OB_VERSIONS
    {
        eBLOCK200_OB_VERSION_1ST_GEN = -1,
        eBLOCK200_OB_VERSION_2ND_GEN_0 = 0,
        eBLOCK200_OB_VERSION_2ND_GEN_1 = 1,
        eBLOCK200_OB_VERSION_I_CRASH_312 = 2,
        eBLOCK200_OB_VERSION_2ND_GEN_2 = 3,
        eBLOCK200_OB_VERSION_2ND_GEN_3 = 4,
        eBLOCK200_OB_VERSION_2ND_GEN_4 = 5,
        eBLOCK200_OB_VERSION_2ND_GEN_5 = 6,
        eBLOCK200_OB_VERSION_LATEST = 7,
        eBLOCK200_OB_VERSION_2ND_GEN_6 = 7,
    }

    [ComVisible(true)]
    public enum eBLOCK200_COMMENT_TYPES
    {
        eBLOCK200_COMMENT_UNKNOWN,
        eBLOCK200_COMMENT_LINE,
        eBLOCK200_COMMENT_INDENTED_LINE,
        eBLOCK200_COMMENT_CODE,
        eBLOCK200_COMMENT_EMPTY_LINE,
        eBLOCK200_COMMENT_ILLEGAL_LINE
    }

    [ComVisible(true)]
    public enum eCOMM200_MEM_TYPE_CODE
    {
        eCOMM200_MEM_TYPE_BOOL = 1,
        eCOMM200_MEM_TYPE_BYTE = 2,
        eCOMM200_MEM_TYPE_CHAR = 3,
        eCOMM200_MEM_TYPE_WORD = 4,
        eCOMM200_MEM_TYPE_INT = 5,
        eCOMM200_MEM_TYPE_DWORD = 6,
        eCOMM200_MEM_TYPE_DINT = 7,
        eCOMM200_MEM_TYPE_REAL = 8,
        eCOMM200_MEM_TYPE_C = 30,
        eCOMM200_MEM_TYPE_T = 31,
        eCOMM200_MEM_TYPE_HC = 32,
    }

    [ComVisible(true)]
    public enum eBLOCK200_DATAROW_TYPES
    {
        eBLOCK200_DATAROW_TYPE_EXTENDED = 3,
        eBLOCK200_DATAROW_TYPE_EXTENDED_NOSIZE = 4,
        eBLOCK200_DATAROW_TYPE_NORMAL = 1,
        eBLOCK200_DATAROW_TYPE_NORMAL_NOSIZE = 2
    }

    [ComVisible(true)]
    public enum eCOMM200_MEMORY_AREA_MASK
    {
        eCOMM200_I_MEM_ONLY = 1,
        eCOMM200_Q_MEM_ONLY = 2,
        eCOMM200_AI_MEM_ONLY = 4,
        eCOMM200_AQ_MEM_ONLY = 8,
        eCOMM200_V_MEM_ONLY = 16,
        eCOMM200_M_MEM_ONLY = 32,
        eCOMM200_T_MEM_ONLY = 64,
        eCOMM200_C_MEM_ONLY = 128,
        eCOMM200_HC_MEM_ONLY = 0x100,
        eCOMM200_SM_MEM_ONLY = 0x200,
        eCOMM200_SCR_MEM_ONLY = 0x400,
        eCOMM200_SD_MEM_ONLY = 0x800,
        eCOMM200_AC_MEM_ONLY = 0x1000,
        eCOMM200_L_MEM_ONLY = 0x2000,
        eCOMM200_INDIRECT_MOD = 0x4000,
        eCOMM200_ADDRESSOF_MOD = 0x8000,
        eCOMM200_LITERAL_ONLY = 0x10000,
        eCOMM200_SBR_MEM_ONLY = 0x20000,
        eCOMM200_INT_MEM_ONLY = 0x40000,
        eCOMM200_OB_MEM_ONLY = 0x80000,
    }

    [ComVisible(true)]
    public enum eBLOCK200_LANG_TYPES
    {
        eBLOCK200_LANG_TYPE_S7_LAD,
        eBLOCK200_LANG_TYPE_S7_STL,
        eBLOCK200_LANG_TYPE_S7_FBD,
        eBLOCK200_LANG_TYPE_IEC_LD,
        eBLOCK200_LANG_TYPE_IEC_FBD,
        eBLOCK200_LANG_TYPE_S7_NONE,
        eBLOCK200_LANG_TYPE_MAX_EDITOR
    }

    [ComVisible(true)]
    public enum eBLOCK200_DATA_TYPES : uint
    {
        eBLOCK200_TYPE_ANY_BIT = 0x1,
        eBLOCK200_TYPE_BOOL = 0x2,
        eBLOCK200_TYPE_BYTE = 0x4,
        eBLOCK200_TYPE_WORD = 0x8,
        eBLOCK200_TYPE_DWORD = 0x10,
        eBLOCK200_TYPE_ANY_INT = 0x20,
        eBLOCK200_TYPE_INT = 0x40,
        eBLOCK200_TYPE_DINT = 0x80,
        eBLOCK200_TYPE_USINT = 0x100,
        eBLOCK200_TYPE_UINT = 0x200,
        eBLOCK200_TYPE_UDINT = 0x400,
        eBLOCK200_TYPE_ANY_REAL = 0x800,
        eBLOCK200_TYPE_REAL = 0x1000,
        eBLOCK200_TYPE_ANY_TIME = 0x2000,
        eBLOCK200_TYPE_TIME = 0x4000,
        eBLOCK200_TYPE_SBR = 0x8000,
        eBLOCK200_TYPE_INTR = 0x10000,
        eBLOCK200_TYPE_OB = 0x20000,
        eBLOCK200_TYPE_CHAR = 0x40000,
        eBLOCK200_TYPE_ANY_STRING = 0x80000,
        eBLOCK200_TYPE_STRING = 0x100000,
        eBLOCK200_TYPE_NETR_STRUCT = 0x200000,
        eBLOCK200_TYPE_PID_STRUCT = 0x400000,
        eBLOCK200_TYPE_TON = 0x800000,
        eBLOCK200_TYPE_TOF = 0x1000000,
        eBLOCK200_TYPE_TP = 0x2000000,
        eBLOCK200_TYPE_CTU = 0x3000000,
        eBLOCK200_TYPE_CTD = 0x8000000,
        eBLOCK200_TYPE_CTUD = 0x10000000,
        eBLOCK200_TYPE_RS = 0x20000000,
        eBLOCK200_TYPE_SR = 0x40000000,
        eBLOCK200_TYPE_BOOL_2 = 0x80000000,

    }

    [ComVisible(true)]
    public enum eSIG200_VAR_TYPE
    {
        eSIG200_VAR_TYPE_IN = 0,
        eSIG200_VAR_TYPE_INOUT = 1,
        eSIG200_VAR_TYPE_OUT = 2,
        eSIG200_VAR_TYPE_VAR = 3,
        eSIG200_VAR_TYPE_VAR_EXTERN = 4,
    }

    [ComVisible(true)]
    public enum eSIG200_STATUS_FORMAT
    {
        eSIG200_STATUS_FORMAT_SIGNED = 0,
        eSIG200_STATUS_FORMAT_UNSIGNED = 1,
        eSIG200_STATUS_FORMAT_HEXADECIMAL = 2,
        eSIG200_STATUS_FORMAT_BINARY = 3,
        eSIG200_STATUS_FORMAT_FLOATING = 4,
        eSIG200_STATUS_FORMAT_ASCII = 5,
        eSIG200_STATUS_FORMAT_STRING = 6,
        eSIG200_STATUS_FORMAT_TIME = 7,
        eSIG200_STATUS_FORMAT_BIT = 8,
    }

    [ComVisible(true)]
    public enum eBLOCK200_LITERAL_TYPES
    {
        eBLOCK200_LITERAL_TYPE_BOOL,
        eBLOCK200_LITERAL_TYPE_UNSIGNED,
        eBLOCK200_LITERAL_TYPE_SIGNED,
        eBLOCK200_LITERAL_TYPE_TIME,
        eBLOCK200_LITERAL_TYPE_HEXADECIMAL,
        eBLOCK200_LITERAL_TYPE_BINARY,
        eBLOCK200_LITERAL_TYPE_ASCII,
        eBLOCK200_LITERAL_TYPE_REAL,
        eBLOCK200_LITERAL_TYPE_STRING
    }

    [ComVisible(true)]
    public enum eBLOCK200_OPERAND_SIZE
    {
        eBLOCK200_OPERAND_SIZE_BIT = 1,
        eBLOCK200_OPERAND_SIZE_BYTE = 2,
        eBLOCK200_OPERAND_SIZE_WORD = 4,
        eBLOCK200_OPERAND_SIZE_DWORD = 8,
        eBLOCK200_OPERAND_SIZE_STRING = 0x10,
        eBLOCK200_OPERAND_SIZE_UNDEFINED = 0x40,
        eBLOCK200_OPERAND_SIZE_UNUSED = 0x20,
    }

    [ComVisible(true)]
    public enum eBLOCK200_OPERAND_TYPES
    {
        eBLOCK200_OPERAND_TYPE_LITERAL,
        eBLOCK200_OPERAND_TYPE_ADDRESS
    }

    [ComVisible(true)]
    public enum eBLOCK200_POU_ATTRS
    {
        eBLOCK200_IMPLICIT = 1,
        eBLOCK200_POU_LIB = 4,
        eBLOCK200_POU_WIZ = 2
    }

    [ComVisible(true)]
    public enum eBLOCK200_POUTYPES
    {
        eBLOCK200_POUTYPES_INTERRUPT = 0x3ea,
        eBLOCK200_POUTYPES_MAIN = 0x3e8,
        eBLOCK200_POUTYPES_POU_ALL = 1,
        eBLOCK200_POUTYPES_POU_INITIAL = 2,
        eBLOCK200_POUTYPES_POU_INVALID = 3,
        eBLOCK200_POUTYPES_POU_VIEW_ALL = 0x3eb,
        eBLOCK200_POUTYPES_SUBROUTINE = 0x3e9
    }

    [ComVisible(true)]
    public enum eIWIZ200DATA_WIZARD_TYPES
    {
        eIWIZ200DATA_WIZARD_TYPE_ASI = 0x40,
        eIWIZ200DATA_WIZARD_TYPE_DATA = 0x83,
        eIWIZ200DATA_WIZARD_TYPE_EM241 = 0x20,
        eIWIZ200DATA_WIZARD_TYPE_EM253 = 0x10,
        eIWIZ200DATA_WIZARD_TYPE_ETHERNET = 0x80,
        eIWIZ200DATA_WIZARD_TYPE_HSC = 4,
        eIWIZ200DATA_WIZARD_TYPE_INTERNET = 0x81,
        eIWIZ200DATA_WIZARD_TYPE_NETRW = 8,
        eIWIZ200DATA_WIZARD_TYPE_NONE = 0,
        eIWIZ200DATA_WIZARD_TYPE_PID = 2,
        eIWIZ200DATA_WIZARD_TYPE_PTO = 0x84,
        eIWIZ200DATA_WIZARD_TYPE_RECIPE = 130,
        eIWIZ200DATA_WIZARD_TYPE_TD200 = 1,
        eIWIZ200DATA_WIZARD_TYPE_UNKNOWN = 0xff
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct sBLOCK200_ADDRESS_STRUCT
    {
        public eBLOCK200_ADDRESSING_MODE mode;
        public eBLOCK200_OPERAND_SIZE size;
        public int memoryArea;
        public int nOffset;
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct sBLOCK200_DB_LINE_INFO
    {
        public sBLOCK200_ADDRESS_STRUCT sAddress;
        public eBLOCK200_DATAROW_TYPES eDataRowType;
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct sBLOCK200_DB_LINE_ITEM_INFO
    {
        public sBLOCK200_LITERAL_STRUCT sValue;
        public int bEndofArray;
        public int bArrayItem;
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential, Pack = 4), ComConversionLoss]
    public struct sBLOCK200_LITERAL_STRUCT
    {
        public eBLOCK200_LITERAL_TYPES type;
        public eBLOCK200_OPERAND_SIZE size;
        public int nBytes;
        [ComConversionLoss]
        public IntPtr pValue;
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi, Pack = 4)]
    public struct sBLOCK200_SUBPARAM
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 0x41)]
        public string szName;
        public int nVarType;
        public int nDataType;
        public int nFormat;
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct sBLOCK200_TIMESTAMP
    {
        public int nYear;
        public int nMonth;
        public int nDOW;
        public int nDay;
        public int nHour;
        public int nMin;
        public int nSec;
        public int nMilliSec;
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct sIWIZ200DATA_INFO
    {
        public int nBlockVersion;
        public int nStructVersion;
        public DateTime dtModified;
        public DateTime dtCreated;
        public int nCount;
        public int nSize;
        public int nAttrs;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1f)]
        public short[] byaName;
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct __MIDL___MIDL_itf_CSysData200_0209_0002
    {
        // Fields
        [FieldOffset(0)]
        public sBLOCK200_ADDRESS_STRUCT sAddress;
        [FieldOffset(0)]
        public sBLOCK200_LITERAL_STRUCT sLiteral;
    }

    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct uBLOCK200_OPERAND
    {
        public eBLOCK200_OPERAND_TYPES eOperandType;
        public __MIDL___MIDL_itf_CSysData200_0209_0002 tagged_union;
    }

    [Guid("CA70F741-DE42-46B1-A43C-712107103DC4"), 
    ComVisible(true),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBlock200Retrieve
    {
        void GetTypedBlockCount([In] eBLOCK200_BLOCKTYPES eBlockType, out int pnBlocks);
        void GetTypedPOUCount([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] ref int pnPOUs);
        void GetPOUNetworkCount([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, out int pnNetworks);
        void GetNetworkInstCount([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, out int pnInsts);
        void GetOperandCount([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nInstIndex, out int pnOperandCount);
        void GetSubParamCount([In] int nOBNumber, [In] int nPOUNumber, out int pnParameters);
        void GetLibraryCount([In] int nOBNumber, out int pnLibraries);
        void GetSysParamCount([In] int nSDBNumber, out int pnParameters);
        void GetDBBinaryData([In] int nDBNumber, [Out] IntPtr ppbyaData, out int pnDataBytes);
        void GetDBTabCount([In] int nDBNumber, out int pnTabs);
        void GetDBTabInfo([In] int nDBNumber, [In] int nTabIndex, [MarshalAs(UnmanagedType.LPWStr)] out string pszName, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] IntPtr[] pbyaPassword, out int pnDBAttrs, out int pnTabID);
        void GetDBLineCount([In] int nDBNumber, [In] int nTabId, out int pnLines);
        void GetDBLineInfo([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, out sBLOCK200_DB_LINE_INFO psLineInfo, out int nItems);
        void GetDBLineItemInfo([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, [In] int nItemIndex, out sBLOCK200_DB_LINE_ITEM_INFO psItemInfo);
        void GetBlockInfo([In] eBLOCK200_BLOCKTYPES eBlockType, [In] int nBlockIndex, out int pnBlockNumber, out sBLOCK200_TIMESTAMP psCreateTime, out sBLOCK200_TIMESTAMP psModifyTime, out eBLOCK200_LANG_TYPES peLanguage, out int pbTargetIs22x, out int pnClientInfo);
        void GetClientPassword([In, MarshalAs(UnmanagedType.LPWStr)] string pszPouName, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] IntPtr[] pbyaDecrpytedPassword);
        void GetPOUInfo([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUIndex, [MarshalAs(UnmanagedType.LPWStr)] out string pszName, out int pnNumber, out byte pbyMajorVersion, out byte pbyMinorVersion, out eBLOCK200_POU_ATTRS pePouAttr, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] IntPtr[] pbyaPassword, out int bPOUIsEncrypted, [Out] out Guid ppGuid);
        void GetPOUBinaryData([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUTypes, [In] int nPOUNumber, [Out] IntPtr ppbyaData, out int pnDataBytes);
        void GetSubParamInfo([In] int nOBNumber, [In] int nPOUNumber, [In] int nParameter, out sBLOCK200_SUBPARAM pParamInfo);
        void GetInstructionID([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUTypes, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nInstIndex, out int pnInstructionID);
        void GetOperand([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUTypes, [In] int nPOUNumber, int nNetworkIndex, [In] int nInstIndex, [In] int nOperandIndex, out uBLOCK200_OPERAND puOperand);
        void GetLibraryInfo([In] int nOBNumber, [In] int nLibIndex, [Out] IntPtr ppGuid, out int pnMajorLibVersion, out int pnMinorLibVersion, out sBLOCK200_ADDRESS_STRUCT psAddr);
        void GetUnknownData([In] int nOBNumber, out int pnDataBytes, [Out] IntPtr ppbyData);
        void GetSystemParameter([In] int nSDBNumber, [In] int nParamIndex, out int pnParamNumber, out int pnParamBytes, [Out] IntPtr ppbyParamData);
    }

    [Guid("1A8ADC26-2DA8-498B-AA52-CB34A43B565A"),
    ComVisible(true),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBlock200Store
    {
        void AddBlock([In] eBLOCK200_BLOCKTYPES eBlockType, [In] int nNumber, [In] sBLOCK200_TIMESTAMP sCreateTime, [In] sBLOCK200_TIMESTAMP sModifyTime, [In] eBLOCK200_LANG_TYPES eLanguage, [In] int nClientInfo);
        void AddPOU([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUID, [In, MarshalAs(UnmanagedType.LPWStr)] string pszName, [In] byte byMajorVersion, [In] byte byMinorVersion, [In] eBLOCK200_POU_ATTRS ePouAttr, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] byte[] byaPassword, [In] int bPOUIsEncrypted);
        void SetPOUGuid([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUID, [In] ref Guid pGuid);
        void AddPOUBinaryData([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] ref byte pData, [In] int nDatabytes);
        void AddSystemParameter([In] int nSDBNumber, [In] int nParamNumber, [In] int nParamLength, [In] ref byte nParamData);
        void AddDBBinaryData([In] int nDBNumber, [In] ref byte pData, [In] int nDatabytes);
        void AddDBTab([In] int nDBNumber, [In] int nTabId, [In, MarshalAs(UnmanagedType.LPWStr)] string pszName, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] byte[] byaPassword, [In] int nTabAttrs);
        void AddDBLine([In] int nDBNumber, [In] int nTabId, [In] ref sBLOCK200_DB_LINE_INFO psLineInfo);
        void AddDBLineItem([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, [In] ref sBLOCK200_DB_LINE_ITEM_INFO psItemInfo);
        void AddDBComment([In] int nDBNumber, [In] int nTabId, [In] eBLOCK200_COMMENT_TYPES eCommentType, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszComment);
        void AddDBLineComment([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, [In] eBLOCK200_COMMENT_TYPES eCommentType, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszComment);
        void InsertDBLineAt([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, [In] ref sBLOCK200_DB_LINE_INFO psLineInfo);
        void RemoveDBLineAt([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex);
        void AddOperand([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nInstIndex, [In] uBLOCK200_OPERAND uOperand);
        void AddUnknownData([In] int nOBNumber, [In] ref byte pbyData, [In] int nDatbyates);
        void AddLibrary([In] int nOBNumber, [In] sBLOCK200_ADDRESS_STRUCT uAddr, [In] ref Guid pGuid, [In] int nMajorLibVersion, [In] int nMinorLibVersion, out int pbLibraryAccepted);
        void AddInstructionID([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nInstructionID);
        void AddNetworkComment([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] eBLOCK200_COMMENT_TYPES eCommentType, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszComment);
        void AddLineComment([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nLineIndex, [In] eBLOCK200_COMMENT_TYPES eCommentType, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszComment);
        void AddNetwork([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber);
        void AddSubParameter([In] int nOBNumber, [In] int nPOUNumber, [In] int nParamIndex, [In] sBLOCK200_SUBPARAM sParam);
        void AuthorizePassword([In] int nOBNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] byte[] byaEncryptedPassword, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] byte[] pbyaDecryptedPassword, out int pbDidDecrpyt);
    }

    [Guid("DD052459-BBE1-45CB-9D8C-FF38FFE52452"),
    ComVisible(true),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IWizData200
    {
        void GetOBWizardCount([In] int nBlockNumber, out int pnWizards);
        void GetOBWizardConfigData([In] int nBlockNumber, [In] int nWizardIndex, [Out] IntPtr ppbyaData, out int pnDataBytes);
        void AddOBWizard([In] int nOBNumber, [In] ref byte pData, [In] int nDatabytes);
        void IsWizardDataBlock([In] int nDBNumber, out int pIsWizardBlock, out eIWIZ200DATA_WIZARD_TYPES peWizType);
        void GetDBWizardConfigData([In] int nDBNumber, [Out] IntPtr ppbyaDefinitionData, out int pnDefinitionDataBytes, [Out] IntPtr ppbyaInstanceData, out int pnInstanceDataBytes, out sIWIZ200DATA_INFO psItemInfo);
        void AddDBWizard([In] int nOBNumber, [In] ref byte pbyaDefinitionData, [In] int nDefinitionDataBytes, [In] ref byte pbyaInstanceData, [In] int nInstanceDataBytes, [In] sIWIZ200DATA_INFO sItemInfo);
        void GenCSVFile([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [In] int nDBNumber);
    }

    [Guid("61C5954A-FA39-474F-9F85-078364AFF035"),
    ComVisible(true), 
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITagData200
    {
        void GetPrjName([MarshalAs(UnmanagedType.LPWStr)] out string ppwstrName);
        uint GetPrjPPIAddr();
        uint GetPrjIPAddr();
        int GetPrjTblCnt();
        void GetPrjTblName([In] int nTbl, [MarshalAs(UnmanagedType.LPWStr)] out string ppwstrName);
        int GetPrjTblSymCnt([In] int nTbl);
        void GetPrjTblSymName([In] int nTbl, [In] int nSym, [MarshalAs(UnmanagedType.LPWStr)] out string ppwstrName);
        void GetPrjTblSymAddress([In] int nTbl, [In] int nSym, [MarshalAs(UnmanagedType.LPWStr)] out string ppwstrAddress);
        void GetPrjTblSymComment([In] int nTbl, [In] int nSym, [MarshalAs(UnmanagedType.LPWStr)] out string ppwstrComment);
        ushort GetPrjTblSymType([In] int nTbl, [In] int nSym);
    }

    [ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("64187036-1B6F-4973-8543-76954858AFF8"),
    ComConversionLoss]
    public interface IBlock200Fix
    {
        void ParseAndReturnSection([In] ref byte pbySourceBlock, [In] int nSourceBytes, [In] eBLOCK200_SECTIONS eSection, [Out] IntPtr ppbySection, out int pnSectionBytes);
        void ParseExtendedDBArea1([In] int nBlockNumber, [In] ref byte pbyArea1, [In] int nArea1Bytes, [Out] IntPtr ppbyCPUHeaderData, out int pnCPUHeaderBytes, [Out] IntPtr ppbyWizHeaderData, out int pnWizHeaderBytes, [Out] IntPtr ppbyRawData, out int pnRawDataBytes);
        void DisassembleBlock([In, MarshalAs(UnmanagedType.IUnknown)] object pIStore, [In, MarshalAs(UnmanagedType.IUnknown)] object pISignatureServer,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 3)] byte[] pbySourceBlock, [In] int nSourceBytes,
              [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 5)] byte[] pbyEdges, [In] int nEdgeBytes, [In] int nLanguage, [In] int InChineseUserMode, out eBLOCK200_OB_VERSIONS pnBlockVersion, out int pbPOUOrdered);
        void AssembleBlock([In, MarshalAs(UnmanagedType.IUnknown)] object pIRetrieve, [In, MarshalAs(UnmanagedType.IUnknown)] object pISignatureServer, [In] int nBlockType, [In] int nBlockNumber, [In] int nBlockVersion, [In] int bForceExpandedFormat, [In] int bNoWizardInfo, [In] int bUseChineseOpcode, [Out] out IntPtr ppbyDestBlock, out int pnDestBytes, [Out] out IntPtr ppbyEdgeData, out int pnEdges);
        void AuthorizePOU([In, MarshalAs(UnmanagedType.IUnknown)] object pIRetrieve, [In, MarshalAs(UnmanagedType.IUnknown)] object pIStore, [In, MarshalAs(UnmanagedType.IUnknown)] object pISignatureServer, [In] int nBlockNumber, [In] int nPOUType, [In] int nPOUNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] byte[] abyPassword);
        void GetBlockTimestamps([In] ref byte pbySourceBlock, [In] int nSourceBytes, out CoMsBlock200Lib.sBLOCK200_TIMESTAMP pCreatedTime, out CoMsBlock200Lib.sBLOCK200_TIMESTAMP pModifiedTime);
        void UpdateDB([In, Out] IntPtr ppDB, [In, Out] ref int pnDBSize, [In] ref byte pEE, [In] int nEESize, [In] int bNoArea3);
        void GenProgramOffsets([In, MarshalAs(UnmanagedType.IUnknown)] object pIRetrieve, [In, MarshalAs(UnmanagedType.IUnknown)] object pISignatureServer, [In] int nBlockNumber, [In] eBLOCK200_OB_VERSIONS eOBVersion, [Out] IntPtr ppaInstructions, out int pnInstructions);
        void ComposeBlock([In] int nBlockType, [In] int nBlockNumber, [In] int nBlockVersion, [In] CoMsBlock200Lib.eBLOCK200_LANG_TYPES eLanguage, [In] int bForceExpandedFormat, [In] ref byte pArea1Data, [In] int nArea1Bytes, [In] ref byte pArea3Data, [In] int nArea3Bytes, [Out] IntPtr ppbyDestBlock, out int pnDestBytes);
        void CalcBlockSizes([In, MarshalAs(UnmanagedType.IUnknown)] object pIRetrieve, [In, MarshalAs(UnmanagedType.IUnknown)] object pISignatureServer, [In] int nOBVersion, [In] int nDBVersion, out int pnOBSize, out int pnOBSizeNoWiz, out int pnDBArea3Size);
    }
}
