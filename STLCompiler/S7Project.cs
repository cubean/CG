using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Irony.Ast;
using CoMsSig200Lib;

namespace STLCompiler
{
    [Guid("2A513B1C-C0F1-4C8C-8A9C-D44DCB8F840E"),
    ComVisible(true),
    ClassInterface(ClassInterfaceType.AutoDual)]
    public class S7Project : IBlock200Retrieve, IBlock200Store, IWizData200, ITagData200
    {
        public Block BlockNode;
        private Sig200ServerClass SigServer;

        public S7Project(AstNode root, Sig200ServerClass server)
        {
            BlockNode = root as Block;
            SigServer = server;
        }

        private POU GetPOU(int nOBNumber, eBLOCK200_POUTYPES ePOUType, int nPOUNumber)
        {
            if (nOBNumber != 1)
                return null;
            switch (ePOUType)
            {
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_MAIN:
                    if (nPOUNumber != 0)
                        return null;
                    return BlockNode.Main as POU;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_SUBROUTINE:
                    if (nPOUNumber < 0 || nPOUNumber >= BlockNode.Subroutins.ChildNodes.Count)
                        return null;
                    return BlockNode.Subroutins.ChildNodes[nPOUNumber] as POU;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_INTERRUPT:
                    if (nPOUNumber < 0 || nPOUNumber >= BlockNode.Interupts.ChildNodes.Count)
                        return null;
                    return BlockNode.Interupts.ChildNodes[nPOUNumber] as POU;
            }
            return null;
        }
        sBLOCK200_LITERAL_STRUCT CloneLiteral(sBLOCK200_LITERAL_STRUCT literal)
        {
            sBLOCK200_LITERAL_STRUCT to = new sBLOCK200_LITERAL_STRUCT();
            to.type = literal.type;
            to.size = literal.size;
            to.nBytes = literal.nBytes;
            to.pValue = Marshal.AllocCoTaskMem(to.nBytes);
            byte[] buf = new byte[to.nBytes];
            Marshal.Copy(literal.pValue, buf, 0, to.nBytes);
            Marshal.Copy(buf, 0, to.pValue, to.nBytes);
            return to;
        }

        #region IBlock200Retrieve
        public void GetTypedBlockCount([In] eBLOCK200_BLOCKTYPES eBlockType, out int pnBlocks)
        {
            switch (eBlockType)
            {
                case eBLOCK200_BLOCKTYPES.eBLOCK200_BLOCKTYPE_OB: pnBlocks = 1; break;
                case eBLOCK200_BLOCKTYPES.eBLOCK200_BLOCKTYPE_OB_802: pnBlocks = 1; break;
                case eBLOCK200_BLOCKTYPES.eBLOCK200_BLOCKTYPE_DB: pnBlocks = 1; break;
                case eBLOCK200_BLOCKTYPES.eBLOCK200_BLOCKTYPE_ALL: pnBlocks = 2; break;
                default: pnBlocks = 0; break;
            }
        }
        
        public void GetTypedPOUCount([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] ref int pnPOUs)
        {
            switch (ePOUType)
            {
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_MAIN: 
                    pnPOUs = 1; 
                    break;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_SUBROUTINE: 
                    pnPOUs = BlockNode.Subroutins.ChildNodes.Count; 
                    break;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_INTERRUPT:
                    pnPOUs = BlockNode.Interupts.ChildNodes.Count;
                    break;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_POU_ALL:
                    pnPOUs = 1 + BlockNode.Subroutins.ChildNodes.Count + BlockNode.Interupts.ChildNodes.Count;
                    break;
                default:
                    pnPOUs = 0;
                    break;
            }
        }
        
        public void GetPOUNetworkCount([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, out int pnNetworks)
        {
            var pou = GetPOU(nOBNumber, ePOUType, nPOUNumber);
            if (pou == null)
                pnNetworks = 0;
            else 
                pnNetworks = pou.Body.ChildNodes.Count;
        }
        
        public void GetNetworkInstCount([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, out int pnInsts)
        {
            pnInsts = 0;
            var pou = GetPOU(nOBNumber, ePOUType, nPOUNumber);
            if (pou != null && (nNetworkIndex >= 0 && nNetworkIndex < pou.Body.ChildNodes.Count))
            {
                var network = pou.Body.ChildNodes[nNetworkIndex] as Network;
                pnInsts = network.ChildNodes.Count;
            }
        }
        
        public void GetOperandCount([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nInstIndex, out int pnOperandCount)
        {
            pnOperandCount = 0;
            var pou = GetPOU(nOBNumber, ePOUType, nPOUNumber);
            if (pou != null && (nNetworkIndex >= 0 && nNetworkIndex < pou.Body.ChildNodes.Count))
            {
                var network = pou.Body.ChildNodes[nNetworkIndex] as Network;
                if (nInstIndex >= 0 && nInstIndex < network.ChildNodes.Count)
                {
                    var inst = network.ChildNodes[nInstIndex] as Instruction;
                    pnOperandCount = inst.ChildNodes.Count;
                }
            }
        }
        
        public void GetSubParamCount([In] int nOBNumber, [In] int nPOUNumber, out int pnParameters)
        {
            pnParameters = 0;
            var pou = GetPOU(nOBNumber, eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_SUBROUTINE, nPOUNumber);
            if (pou != null) 
            {
                pnParameters = pou.Parameters.Count;
            }
        }
        
        public void GetSubParamInfo([In] int nOBNumber, [In] int nPOUNumber, [In] int nParameter, out sBLOCK200_SUBPARAM pParamInfo)
        {
            pParamInfo = new sBLOCK200_SUBPARAM();
            var pou = GetPOU(nOBNumber, eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_SUBROUTINE, nPOUNumber);
            if (pou != null)
            {
                var p = (pou.Parameters[nParameter] as Declaration).Parameter;
                pParamInfo.szName = p.szName;
                pParamInfo.nVarType = p.nVarType;
                pParamInfo.nDataType = p.nDataType;
                pParamInfo.nFormat = p.nFormat;
            }
        }
        
        public void GetLibraryCount([In] int nOBNumber, out int pnLibraries)
        {
            pnLibraries = 0;
        }
        
        public void GetSysParamCount([In] int nSDBNumber, out int pnParameters)
        {
            pnParameters = 0;
        }
        
        public void GetDBBinaryData([In] int nDBNumber, [Out] IntPtr ppbyaData, out int pnDataBytes)
        {
            pnDataBytes = 0;
            ppbyaData = IntPtr.Zero;
        }
        
        public void GetDBTabCount([In] int nDBNumber, out int pnTabs)
        {
            pnTabs = 0;
        }
        
        public void GetDBTabInfo([In] int nDBNumber, [In] int nTabIndex, [MarshalAs(UnmanagedType.LPWStr)] out string pszName, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] IntPtr[] pbyaPassword, out int pnDBAttrs, out int pnTabID)
        {
            pszName = string.Empty;
            pnDBAttrs = 0;
            pnTabID = 0;
        }
        
        public void GetDBLineCount([In] int nDBNumber, [In] int nTabId, out int pnLines)
        {
            pnLines = 0;
        }
        
        public void GetDBLineInfo([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, out sBLOCK200_DB_LINE_INFO psLineInfo, out int nItems)
        {
            psLineInfo = new sBLOCK200_DB_LINE_INFO();
            nItems = 0;
        }
        
        public void GetDBLineItemInfo([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, [In] int nItemIndex, out sBLOCK200_DB_LINE_ITEM_INFO psItemInfo)
        {
            psItemInfo = new sBLOCK200_DB_LINE_ITEM_INFO();
        }
        
        public void GetBlockInfo([In] eBLOCK200_BLOCKTYPES eBlockType, [In] int nBlockIndex, out int pnBlockNumber, out sBLOCK200_TIMESTAMP psCreateTime, out sBLOCK200_TIMESTAMP psModifyTime, out eBLOCK200_LANG_TYPES peLanguage, out int pbTargetIs22x, out int pnClientInfo)
        {
            pnBlockNumber = 1;
            psCreateTime = new sBLOCK200_TIMESTAMP();
            psCreateTime.nYear = 2011;
            psCreateTime.nMonth = 1;
            psCreateTime.nDay = 1;
            psCreateTime.nHour = 1;
            psCreateTime.nMin = 1;
            psCreateTime.nSec = 1;
            psModifyTime = new sBLOCK200_TIMESTAMP();
            psModifyTime.nYear = 2011;
            psModifyTime.nMonth = 1;
            psModifyTime.nDay = 1;
            psModifyTime.nHour = 1;
            psModifyTime.nMin = 1;
            psModifyTime.nSec = 1;
            peLanguage = eBLOCK200_LANG_TYPES.eBLOCK200_LANG_TYPE_S7_LAD;
            pbTargetIs22x = 1;
            pnClientInfo = 0x408;
        }
        
        public void GetClientPassword([In, MarshalAs(UnmanagedType.LPWStr)] string pszPouName, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] IntPtr[] pbyaDecrpytedPassword)
        {

        }
        
        public void GetPOUInfo([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUIndex, [MarshalAs(UnmanagedType.LPWStr)] out string pszName, out int pnNumber, out byte pbyMajorVersion, out byte pbyMinorVersion, out eBLOCK200_POU_ATTRS pePouAttr, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] IntPtr[] pbyaPassword, out int bPOUIsEncrypted, [Out] out Guid ppGuid)
        {
            pszName = string.Empty;
            pnNumber = 0;
            pePouAttr = eBLOCK200_POU_ATTRS.eBLOCK200_IMPLICIT;
            pbyMajorVersion = 1;
            pbyMinorVersion = 0;
            bPOUIsEncrypted = 0;
            ppGuid = Guid.Empty;

            var pou = GetPOU(nOBNumber, ePOUType, nPOUIndex);
            if (pou != null)
            {
                pszName = pou.Name;
                pnNumber = pou.Number;
                pePouAttr = eBLOCK200_POU_ATTRS.eBLOCK200_IMPLICIT;
                pbyMajorVersion = 1;
                pbyMinorVersion = 0;
                bPOUIsEncrypted = 0;
            }
        }
        
        public void GetPOUBinaryData([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUTypes, [In] int nPOUNumber, [Out] IntPtr ppbyaData, out int pnDataBytes)
        {
            ppbyaData = IntPtr.Zero;
            pnDataBytes = 0;
        }
        
        public void GetInstructionID([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUTypes, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nInstIndex, out int pnInstructionID)
        {
            pnInstructionID = 0;

            var pou = GetPOU(nOBNumber, ePOUTypes, nPOUNumber);
            if (pou != null && (nNetworkIndex >= 0 && nNetworkIndex < pou.Body.ChildNodes.Count))
            {
                var network = pou.Body.ChildNodes[nNetworkIndex] as Network;
                if (nInstIndex >= 0 && nInstIndex < network.ChildNodes.Count)
                {
                    var inst = network.ChildNodes[nInstIndex] as Instruction;
                    try
                    {
                        int insId, subId;
                        SigServer.GetIdsByName(out insId, out subId, inst.Mnemonic);
                        pnInstructionID = insId;
                    }
                    catch(COMException)
                    {
                    }
                }
            }
        }
        
        public void GetOperand([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUTypes, [In] int nPOUNumber, int nNetworkIndex, [In] int nInstIndex, [In] int nOperandIndex, out uBLOCK200_OPERAND puOperand)
        {
            puOperand = new uBLOCK200_OPERAND();
            var pou = GetPOU(nOBNumber, ePOUTypes, nPOUNumber);
            if (pou != null && (nNetworkIndex >= 0 && nNetworkIndex < pou.Body.ChildNodes.Count))
            {
                var network = pou.Body.ChildNodes[nNetworkIndex] as Network;
                if (nInstIndex >= 0 && nInstIndex < network.ChildNodes.Count)
                {
                    var inst = network.ChildNodes[nInstIndex] as Instruction;
                    if (nOperandIndex >= 0 && nOperandIndex < inst.ChildNodes.Count)
                    {
                        var operannd = inst.ChildNodes[nOperandIndex];
                        if (operannd.Term.Name == "address_term")
                        {
                            var address = operannd as Address;
                            puOperand.eOperandType = eBLOCK200_OPERAND_TYPES.eBLOCK200_OPERAND_TYPE_ADDRESS;
                            puOperand.tagged_union.sAddress = address.S7Address;
                        }
                        else
                        {
                            var constand = operannd as Constrant;
                            puOperand.eOperandType = eBLOCK200_OPERAND_TYPES.eBLOCK200_OPERAND_TYPE_LITERAL;
                            puOperand.tagged_union.sLiteral = CloneLiteral(constand.Literal);
                        }
                    }
                }
            }
        }
        
        public void GetLibraryInfo([In] int nOBNumber, [In] int nLibIndex, [Out] IntPtr ppGuid, out int pnMajorLibVersion, out int pnMinorLibVersion, out sBLOCK200_ADDRESS_STRUCT psAddr)
        {
            pnMajorLibVersion = 1;
            pnMinorLibVersion = 0;
            ppGuid = IntPtr.Zero;
            psAddr = new sBLOCK200_ADDRESS_STRUCT();
        }
        
        public void GetUnknownData([In] int nOBNumber, out int pnDataBytes, [Out] IntPtr ppbyData)
        {
            pnDataBytes = 0;
            ppbyData = IntPtr.Zero;

        }
        
        public void GetSystemParameter([In] int nSDBNumber, [In] int nParamIndex, out int pnParamNumber, out int pnParamBytes, [Out] IntPtr ppbyParamData)
        {
            pnParamBytes = 0;
            pnParamNumber = 0;
            ppbyParamData = IntPtr.Zero;
        }
        
        #endregion

        #region IBlock200Store
        public void AddBlock([In] eBLOCK200_BLOCKTYPES eBlockType, [In] int nNumber, [In] sBLOCK200_TIMESTAMP sCreateTime, [In] sBLOCK200_TIMESTAMP sModifyTime, [In] eBLOCK200_LANG_TYPES eLanguage, [In] int nClientInfo)
        {
            if (BlockNode == null)
                BlockNode = new Block();
            BlockNode.BlockNumber = nNumber;
        }

        public void AddPOU([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUID, [In, MarshalAs(UnmanagedType.LPWStr)] string pszName, [In] byte byMajorVersion, [In] byte byMinorVersion, [In] eBLOCK200_POU_ATTRS ePouAttr, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] byte[] byaPassword, [In] int bPOUIsEncrypted)
        {
            POU pou = new POU(pszName, nPOUID, ePOUType);
            BlockNode.AddPOU(ePOUType, pou);
        }

        public void SetPOUGuid([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUID, [In] ref Guid pGuid)
        { }

        public void AddPOUBinaryData([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] ref byte pData, [In] int nDatabytes)
        { }

        public void AddSystemParameter([In] int nSDBNumber, [In] int nParamNumber, [In] int nParamLength, [In] ref byte nParamData)
        { }

        public void AddDBBinaryData([In] int nDBNumber, [In] ref byte pData, [In] int nDatabytes)
        { }

        public void AddDBTab([In] int nDBNumber, [In] int nTabId, [In, MarshalAs(UnmanagedType.LPWStr)] string pszName, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] byte[] byaPassword, [In] int nTabAttrs)
        { }

        public void AddDBLine([In] int nDBNumber, [In] int nTabId, [In] ref sBLOCK200_DB_LINE_INFO psLineInfo)
        { }

        public void AddDBLineItem([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, [In] ref sBLOCK200_DB_LINE_ITEM_INFO psItemInfo)
        { }

        public void AddDBComment([In] int nDBNumber, [In] int nTabId, [In] eBLOCK200_COMMENT_TYPES eCommentType, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszComment)
        { }

        public void AddDBLineComment([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, [In] eBLOCK200_COMMENT_TYPES eCommentType, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszComment)
        { }

        public void InsertDBLineAt([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex, [In] ref sBLOCK200_DB_LINE_INFO psLineInfo)
        { }

        public void RemoveDBLineAt([In] int nDBNumber, [In] int nTabId, [In] int nLineIndex)
        { }

        public void AddUnknownData([In] int nOBNumber, [In] ref byte pbyData, [In] int nDatbyates)
        { }

        public void AddLibrary([In] int nOBNumber, [In] sBLOCK200_ADDRESS_STRUCT uAddr, [In] ref Guid pGuid, [In] int nMajorLibVersion, [In] int nMinorLibVersion, out int pbLibraryAccepted)
        { 
            pbLibraryAccepted = 0; 
        }

        public void AddInstructionID([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nInstructionID)
        {
            var pou = GetPOU(nOBNumber, ePOUType, nPOUNumber);
            if (pou != null && (nNetworkIndex >= 0 && nNetworkIndex < pou.Body.ChildNodes.Count))
            {
                var network = pou.Body.ChildNodes[nNetworkIndex] as Network;
                var ins = new Instruction();
                Sig200Signature ppISig = null;
                SigServer.GetInstructionSignatureByID(nInstructionID, ref ppISig);
                ppISig.GetSimaticMnemonic(out ins.Mnemonic);

                network.AddIns(ins);
            }
        }

        public void AddOperand([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nInstIndex, [In] uBLOCK200_OPERAND uOperand)
        {
            var pou = GetPOU(nOBNumber, ePOUType, nPOUNumber);
            if (pou != null && (nNetworkIndex >= 0 && nNetworkIndex < pou.Body.ChildNodes.Count))
            {
                var network = pou.Body.ChildNodes[nNetworkIndex] as Network;
                if (nInstIndex >= 0 && nInstIndex < network.ChildNodes.Count)
                {
                    var inst = network.ChildNodes[nInstIndex] as Instruction;
                    switch (uOperand.eOperandType)
                    {
                        case eBLOCK200_OPERAND_TYPES.eBLOCK200_OPERAND_TYPE_ADDRESS:
                            var addr = new Address();
                            addr.S7Address = uOperand.tagged_union.sAddress;
                            inst.AddOperand(addr);
                            break;
                        case eBLOCK200_OPERAND_TYPES.eBLOCK200_OPERAND_TYPE_LITERAL:
                            var constrant = new Constrant();
                            constrant.Literal = CloneLiteral(uOperand.tagged_union.sLiteral);
                            inst.AddOperand(constrant);
                            break;
                    }
                }
            }
        }

        public void AddNetworkComment([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] eBLOCK200_COMMENT_TYPES eCommentType, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszComment)
        { }

        public void AddLineComment([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber, [In] int nNetworkIndex, [In] int nLineIndex, [In] eBLOCK200_COMMENT_TYPES eCommentType, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszComment)
        { }

        public void AddNetwork([In] int nOBNumber, [In] eBLOCK200_POUTYPES ePOUType, [In] int nPOUNumber)
        {
            POU pou = GetPOU(nOBNumber, ePOUType, nPOUNumber);
            if (pou != null) {
                var body = (pou.Body as NodeList);
                body.AddNode(new Network() { Number = body.ChildNodes.Count });
            }
        }

        public void AddSubParameter([In] int nOBNumber, [In] int nPOUNumber, [In] int nParamIndex, [In] sBLOCK200_SUBPARAM sParam)
        {
            POU pou = GetPOU(nOBNumber, eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_SUBROUTINE, nPOUNumber);
            if (pou != null)
            {
                Declaration decl = new Declaration(sParam.szName, sParam.nVarType.ToString(), sParam);
                pou.AddSubParam((eSIG200_VAR_TYPE)sParam.nVarType, decl);
            }

        }

        public void AuthorizePassword([In] int nOBNumber, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] byte[] byaEncryptedPassword, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 4)] byte[] pbyaDecryptedPassword, out int pbDidDecrpyt)
        {
            pbyaDecryptedPassword[0] = 0;
            pbDidDecrpyt = 0;
        }

        #endregion

        #region IWizData200
        public void GetOBWizardCount([In] int nBlockNumber, out int pnWizards)
        {
            pnWizards = 0;
        }
        public void GetOBWizardConfigData([In] int nBlockNumber, [In] int nWizardIndex, [Out] IntPtr ppbyaData, out int pnDataBytes)
        {
            ppbyaData = IntPtr.Zero;
            pnDataBytes = 0;
        }
        public void AddOBWizard([In] int nOBNumber, [In] ref byte pData, [In] int nDatabytes)
        {

        }
        public void IsWizardDataBlock([In] int nDBNumber, out int pIsWizardBlock, out eIWIZ200DATA_WIZARD_TYPES peWizType)
        {
            pIsWizardBlock = 0;
            peWizType = eIWIZ200DATA_WIZARD_TYPES.eIWIZ200DATA_WIZARD_TYPE_NONE;
        }
        public void GetDBWizardConfigData([In] int nDBNumber, [Out] IntPtr ppbyaDefinitionData, out int pnDefinitionDataBytes, [Out] IntPtr ppbyaInstanceData, out int pnInstanceDataBytes, out sIWIZ200DATA_INFO psItemInfo)
        {
            ppbyaDefinitionData = IntPtr.Zero;
            pnDefinitionDataBytes = 0;
            ppbyaInstanceData = IntPtr.Zero;
            pnInstanceDataBytes = 0;
            psItemInfo = new sIWIZ200DATA_INFO();
        }
        public void AddDBWizard([In] int nOBNumber, [In] ref byte pbyaDefinitionData, [In] int nDefinitionDataBytes, [In] ref byte pbyaInstanceData, [In] int nInstanceDataBytes, [In] sIWIZ200DATA_INFO sItemInfo)
        {
        }
        public void GenCSVFile([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [In] int nDBNumber)
        {
        }
        #endregion

        #region ITagData200
        public void GetPrjName([MarshalAs(UnmanagedType.LPWStr)] out string ppwstrName)
        {
            ppwstrName = "prj";
        }
        public uint GetPrjPPIAddr()
        {
            return 0;
        }
        public uint GetPrjIPAddr()
        {
            return 0;
        }
        public int GetPrjTblCnt()
        {
            return 0;
        }
        public void GetPrjTblName([In] int nTbl, [MarshalAs(UnmanagedType.LPWStr)] out string ppwstrName)
        {
            ppwstrName = "";
        }
        public int GetPrjTblSymCnt([In] int nTbl)
        {
            return 0;
        }
        public void GetPrjTblSymName([In] int nTbl, [In] int nSym, [MarshalAs(UnmanagedType.LPWStr)] out string ppwstrName)
        {
            ppwstrName = string.Empty;
        }
        public void GetPrjTblSymAddress([In] int nTbl, [In] int nSym, [MarshalAs(UnmanagedType.LPWStr)] out string ppwstrAddress)
        {
            ppwstrAddress = string.Empty;
        }
        public void GetPrjTblSymComment([In] int nTbl, [In] int nSym, [MarshalAs(UnmanagedType.LPWStr)] out string ppwstrComment)
        {
            ppwstrComment = string.Empty;
        }
        public ushort GetPrjTblSymType([In] int nTbl, [In] int nSym)
        {
            return 0;
        }

        #endregion
    }
}
