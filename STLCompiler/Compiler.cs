using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Irony.Parsing;
using CoMsSig200Lib;
using CoMsBlock200Lib;

namespace STLCompiler
{
    public class Compiler
    {
        public bool AssemblyBlock(eBLOCK200_BLOCKTYPES blockType, int blockNr, eBLOCK200_OB_VERSIONS blockVer, 
            bool forceExpandedFormat, bool bNoWizardInfo, bool bUseChineseOpcode, 
            string source, out byte[] dest)
        {
            dest = null;

            var server = new CoMsSig200Lib.Sig200ServerClass();
            Sig200SignatureList InsList = null;
            server.CreateInstructionSignatures(null);
            server.SetMnemonic(eSIG200_MNEMONICS.eSIG200_MNEMONIC_SIMATIC);
            server.GetInstructionSignatureList(ref InsList);

            var grammar = new STLCompiler.STLGrammar();
            LanguageData language = new LanguageData(grammar);
            Parser parser = new Parser(language);
            ParseTree parseTree = parser.Parse(source);
            ParseTreeNode root = parseTree.Root;            
            
            var project = new STLCompiler.S7Project(root.AstNode as Block, server);
            var builder = new CoMsBlock200Lib.Block200Class() as IBlock200Fix;
            int pnOBSize, pnOBSizeNoWiz, pnDBArea3Size;

            builder.CalcBlockSizes(project, server,
                (int)eBLOCK200_OB_VERSIONS.eBLOCK200_OB_VERSION_2ND_GEN_5,
                2, /* Must 2, from reverse */
                out pnOBSize,
                out pnOBSizeNoWiz,
                out pnDBArea3Size);

            IntPtr ppbyDestBlock = Marshal.AllocCoTaskMem(pnOBSize);
            IntPtr ppbyEdgeData = IntPtr.Zero;
            int nDestBytes = pnOBSize, nEdges = 0;

            try
            {
                builder.AssembleBlock(project,
                server,
                (int)eBLOCK200_BLOCKTYPES.eBLOCK200_BLOCKTYPE_OB,
                1,
                (int)eBLOCK200_OB_VERSIONS.eBLOCK200_OB_VERSION_2ND_GEN_5,
                0,
                1,
                1,
                out ppbyDestBlock,
                out nDestBytes,
                out ppbyEdgeData,
                out nEdges);

                dest = new byte[nDestBytes];
                Marshal.Copy(ppbyDestBlock, dest, 0, nDestBytes);
             }
            catch(COMException e)
            {
                Console.Write(e.Message);
                return false;
            }

            return true;
        }

        public bool DisassemblyBlock(
            byte[] SourceBlock,
            out string awl)
        {
            var server = new CoMsSig200Lib.Sig200ServerClass();
            Sig200SignatureList InsList = null;
            server.CreateInstructionSignatures(null);
            server.SetMnemonic(eSIG200_MNEMONICS.eSIG200_MNEMONIC_SIMATIC);
            server.GetInstructionSignatureList(ref InsList);

            var project = new STLCompiler.S7Project(null, server);
            var builder = new CoMsBlock200Lib.Block200Class() as IBlock200Fix;

            awl = string.Empty;
            var BlockVersion = eBLOCK200_OB_VERSIONS.eBLOCK200_OB_VERSION_2ND_GEN_6;
            int POUOrdered = 0;

            try
            {
                byte[] Edges = null;
                int EdgeBytes = 0;
                int nLanguage = (int)eBLOCK200_LANG_TYPES.eBLOCK200_LANG_TYPE_S7_STL;
                int InChineseUserMode = 1;

                builder.DisassembleBlock(project, server, SourceBlock, SourceBlock.Length,
                                Edges, EdgeBytes, nLanguage, InChineseUserMode,
                                out BlockVersion, out POUOrdered);

                StringBuilder sb = new StringBuilder();
                if (project.BlockNode != null)
                    project.BlockNode.DisasmText(sb);
                awl = sb.ToString();
            }
            catch (COMException e)
            {
                Console.Write(e.Message);
                return false;
            }

            return true;
        }
    }
}
