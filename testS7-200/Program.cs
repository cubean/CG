using System;
using System.Collections.Generic;
using System.Linq;
using CoMsSig200Lib;
using System.Runtime.InteropServices;
using STLCompiler;
using System.Web.Script;
using System.Web.Script.Serialization;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using System.Text;
using NDesk.Options;
using libnodaveNET;

namespace ExportSig
{
    static class Program
    {
        [Serializable]
        class InsParam
        {
            public string Name;
            public int Size;
            public CoMsSig200Lib.eSIG200_VAR_TYPE VarType;
            public int DataFormats;
            public int DataTypes;
            public int MemoryAreas;
            public int AddrRangeQual;
            public eSIG200_LITERAL_TYPE DefStatusFormat;
            public int DisplaysWithRisingEdge;
            public bool IsOverloaded;
        }

        [Serializable]
        class InsStruct
        {
            public string SimaticMnemonic;
            public string InternationalMnemonic;
            public int InstId;
            public int InstructionSets;
            public Guid Guid;
            public int OpCode;
            public eSIG200_INSTR_CATEGORY IECInstructionCategory;
            public eSIG200_INSTR_CATEGORY InstructionCategory;
            public int IsOverloaded;
            public eSIG200_INST_TYPE InstructionType;
            public eSIG200_INST_TYPE RMEInstructionType;
            public int Editors;
            public eSIG200_EXEC_STAT_SCHEMES ExecutionStatusScheme;
            public eSIG200_EXP_FORMAT MacroExpansionFormat;
            public eSIG200_REC_FORMAT MacroRecognitionFormat;
            public int ReturnsStackInfo;
            public int RunModeEdgeBit;
            public int SetsENO;
            public int StackUsage;
            public int IECFBDVersion;
            public int IECLADVersion;
            public int IECSTLVersion;
            public int STLANDVersion;
            public int STLORVersion;
            public int STLVersion;
            public int LADVersion;
            public int FBDVersion;
            public bool HasPowerFlowIn;
            public bool HasPowerFlowOut;
            public bool HasRangeCheckField;
            public int ParameterCount;
            public List<InsParam> Parameters;
        }

        public class IntConverter : JavaScriptConverter
        {

            public override IEnumerable<Type> SupportedTypes
            {
                //Define the ListItemCollection as a supported type.
                get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(int) })); }
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                if (obj == null)
                    return result;

                int val = (int)obj;
                result[val.ToString("X")] = val.ToString();
                return result;
            }

            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                return null;
            }
        }

        static void DumpInst()
        {
            var server = new Sig200ServerClass();
            Sig200SignatureList insList = null;
            server.CreateInstructionSignatures(null);
            server.SetMnemonic(eSIG200_MNEMONICS.eSIG200_MNEMONIC_SIMATIC);
            server.GetInstructionSignatureList(ref insList);
            System.Web.Script.Serialization.JavaScriptSerializer oSerializer =
                new System.Web.Script.Serialization.JavaScriptSerializer();
            var sigs = new List<InsStruct>();
            for (int i = 1; i < insList.GetSize(); ++i)
            {
                InsStruct sig = new InsStruct();
                Sig200Signature ins = null;
                insList.GetAt(i, ref ins);
                var isig = (CoMsSig200Lib.ISig200SignatureServerOnlyAccess)ins;
                sig.OpCode = isig.GetOpCode();
                string name;
                isig.GetName(out name);
                sig.Editors = ins.GetEditors();
                sig.ExecutionStatusScheme = ins.GetExecutionStatusScheme();
                sig.FBDVersion = ins.GetFBDVersion();
                sig.IECFBDVersion = ins.GetIECFBDVersion();
                sig.IECInstructionCategory = ins.GetIECInstructionCategory();
                sig.IECLADVersion = ins.GetIECLADVersion();
                sig.IECSTLVersion = ins.GetIECSTLVersion();
                sig.InstId = ins.GetInstId();
                sig.InstructionCategory = ins.GetInstructionCategory();
                sig.InstructionSets = ins.GetInstructionSets();
                sig.InstructionType = ins.GetInstructionType();
                ins.GetInternationalMnemonic(out sig.InternationalMnemonic);
                sig.IsOverloaded = ins.GetIsOverloaded();
                sig.LADVersion = ins.GetLADVersion();
                sig.MacroExpansionFormat = ins.GetMacroExpansionFormat();
                sig.MacroRecognitionFormat = ins.GetMacroRecognitionFormat();
                sig.ReturnsStackInfo = ins.GetReturnsStackInfo();
                sig.RMEInstructionType = ins.GetRMEInstructionType();
                sig.RunModeEdgeBit = ins.GetRunModeEdgeBit();
                sig.SetsENO = ins.GetSetsENO();
                ins.GetSimaticMnemonic(out sig.SimaticMnemonic);
                sig.StackUsage = ins.GetStackUsage();
                sig.STLANDVersion = ins.GetSTLANDVersion();
                sig.STLORVersion = ins.GetSTLORVersion();
                sig.STLVersion = ins.GetSTLVersion();
                sig.HasPowerFlowIn = ins.HasPowerFlowIn() > 0;
                sig.HasPowerFlowOut = ins.HasPowerFlowOut() > 0;
                sig.HasRangeCheckField = ins.HasRangeCheckField() > 0;
                sig.OpCode = server.GetOpCode(sig.InstId, 0, sig.ParameterCount);
                sig.ParameterCount = ins.GetParameterCount();
                sig.Parameters = new List<InsParam>();

                for (int j=0; j<sig.ParameterCount; ++j)
                {
                    InsParam p = new InsParam();
                    Sig200Parameter param = null;
                    ins.GetParameter(j, ref param);
                    p.AddrRangeQual = param.GetAddrRangeQual();
                    p.DataFormats = param.GetDataFormats();
                    p.DataTypes = param.GetDataTypes();
                    p.DefStatusFormat = param.GetDefStatusFormat();
                    p.DisplaysWithRisingEdge = param.GetDisplaysWithRisingEdge();
                    p.MemoryAreas = param.GetMemoryAreas();
                    param.GetName(out p.Name);
                    p.Size = param.GetSize();
                    p.VarType = param.GetVarType();
                    p.IsOverloaded = param.IsOverloaded() > 0;
                    sig.Parameters.Add(p);
                }
                sigs.Add(sig);
            }

            //oSerializer.RegisterConverters(new JavaScriptConverter[] { new IntConverter() });
            string sJSON = oSerializer.Serialize(sigs);
            Console.WriteLine(sJSON);
        }

        static libnodave.daveOSserialType fds;
        static libnodave.daveInterface di;
        static libnodave.daveConnection dc;
        static int localPPI = 0;

        static void DownloadProgramBlock(string port, string baud, char parity, int address, byte[] dest)
        {
            try{
                fds.rfd = libnodave.setPort(port, baud, parity);
                fds.wfd = fds.rfd;
                if (fds.rfd > 0)
                {
                    di = new libnodave.daveInterface(fds, "IF1", localPPI, libnodave.daveProtoPPI, libnodave.daveSpeed187k);
                    di.setTimeout(1000000);
                    libnodave.daveSetDebug(libnodave.daveDebugAll);
                    dc = new libnodave.daveConnection(di, address, 0, 0);
                    if (0 == dc.connectPLC())
                    {
                        dc.putProgramBlock(libnodave.daveBlockType_OB, 1, dest, dest.Count());
                    }
                    dc.disconnectPLC();
                    libnodave.closePort(fds.rfd);
                }
                else
                {
                    Console.WriteLine("Couldn't open serial port ");
                }	
            }
            catch(COMException e)
            {
                Console.Write(e.Message);
            }
        }

        static void UploadProgramBlock(string port, string baud, char parity, int address, out byte[] dest)
        {
            dest = null;
            try
            {
                fds.rfd = libnodave.setPort(port, baud, parity);
                fds.wfd = fds.rfd;
                if (fds.rfd > 0)
                {
                    di = new libnodave.daveInterface(fds, "IF1", localPPI, libnodave.daveProtoPPI, libnodave.daveSpeed9k);
                    //libnodave.daveSetDebug(libnodave.daveDebugAll);
                    dc = new libnodave.daveConnection(di, address, 0, 0);
                    if (0 == dc.connectPLC())
                    {
                        int length = 65536;
                        dest = new byte[length];

                        dc.getProgramBlock(libnodave.daveBlockType_OB, 1, dest, ref length);

                        dest = dest.Take(length).ToArray();
                    }
                    dc.disconnectPLC();
                    libnodave.closePort(fds.rfd);
                }
                else
                {
                    Console.WriteLine("Couldn't open serial port ");
                }
            }
            catch (COMException e)
            {
                Console.Write(e.Message);
            }
        }

        static void UsageAndExit()
        {
            Console.WriteLine("");
            System.Environment.Exit(0);
        }

        static void asm(string[] args)
        {
            string f = null, o = null;
            bool d = false;
            string port = "COM6", baud="9600";
            char parity='E';
            int address=2;

            var p = new OptionSet() {
   	                        { "f|file=",  v => f = v },
   	                        { "o|output=",  v => o = v },
   	                        { "c|comport=",  v => port = v },
   	                        { "b|baud=",  v => baud = v },
   	                        { "p|parity=",  v => parity = v[0] },
   	                        { "a|address=",  v => address = int.Parse(v) },
   	                        { "d|download",  v => d = (v != null) },
                        };
            List<string> extra = p.Parse(args);
            if (f == null)
                UsageAndExit();

            using (TextReader streamReader =
                new StreamReader(f, Encoding.Default))
            {
                var source = streamReader.ReadToEnd();

                byte[] dest = null;
                var compiler = new Compiler();
                compiler.AssemblyBlock(
                    eBLOCK200_BLOCKTYPES.eBLOCK200_BLOCKTYPE_OB,
                    1,
                    STLCompiler.eBLOCK200_OB_VERSIONS.eBLOCK200_OB_VERSION_2ND_GEN_6,
                    false,
                    true,
                    true,
                    source,
                    out dest);

                if (o != null)
                {
                    using (BinaryWriter binWriter = new BinaryWriter(
                        File.Open(o, FileMode.Create)))
                    {
                        binWriter.Write(dest);
                    }
                }

                if (d)
                    DownloadProgramBlock(port, baud, parity, address, dest);
            }
        }

        static void disasm(string[] args)
        {
            string f = null, o = null;
            bool u = false;
            string port = "COM6", baud = "9600";
            char parity = 'E';
            int address = 2;
            var p = new OptionSet() {
   	                        { "f|file=",  v => f = v },
   	                        { "o|output=",  v => o = v },
   	                        { "c|comport=",  v => port = v },
   	                        { "b|baud=",  v => baud = v },
   	                        { "p|parity=",  v => parity = v[0] },
   	                        { "a|address=",  v => address = int.Parse(v) },
   	                        { "u|upload",  v => u = (v != null) },
                        };
            List<string> extra = p.Parse(args);
            if (!u && f == null)
                p.WriteOptionDescriptions(Console.Out);

            byte[] block = null;
            if (!u)
            {
                using (var binReader = new BinaryReader(
                    File.Open(f, FileMode.Open)))
                {
                    block = binReader.ReadBytes((int)binReader.BaseStream.Length);
                }
            }
            else
            {
                UploadProgramBlock(port, baud, parity, address, out block);
            }
            var compiler = new Compiler();
            string awlContent;

            compiler.DisassemblyBlock(block, out awlContent);

            using (var streamWriter =
                new StreamWriter(o, false, Encoding.Default))
            {
                streamWriter.Write(awlContent);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
                UsageAndExit();

            var subArgs = args.Skip(1).ToArray();
            switch (args[0])
            {
                case "asm": asm(subArgs); break;
                case "disasm": disasm(subArgs); break;
                case "dump": DumpInst(); break;
            }
        }
    }
}
