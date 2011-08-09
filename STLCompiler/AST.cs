using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Ast;
using System.Runtime.InteropServices;

namespace STLCompiler
{
    public class DisasmAstNode : AstNode
    {
        public virtual void DisasmText(StringBuilder build)
        {
        }
    }

    public class Block : DisasmAstNode
    {
        public AstNode Main = null;
        public AstNode Subroutins = null;
        public AstNode Interupts = null;
        public int BlockNumber = 1;

        public Block() { }
        public override void DisasmText(StringBuilder build)
        {
            (Main as DisasmAstNode).DisasmText(build);
            (Subroutins as DisasmAstNode).DisasmText(build);
            (Interupts as DisasmAstNode).DisasmText(build);
        }

        public void AddPOU(eBLOCK200_POUTYPES type, POU pou)
        {
            switch (type)
            {
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_MAIN:
                    Main = pou;
                    pou.SetParent(this);
                    break;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_SUBROUTINE:
                    if (Subroutins == null) {
                        Subroutins = new NodeList();
                        Subroutins.SetParent(this);
                    }
                    (Subroutins as NodeList).AddNode(pou);
                    break;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_INTERRUPT:
                    if (Interupts == null) {
                        Interupts = new NodeList();
                        Interupts.SetParent(this);
                    }
                    (Interupts as NodeList).AddNode(pou);
                    break;
            }
        }

        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            Main = AddChild(string.Empty, treeNode.ChildNodes[0]);
            Subroutins = AddChild(string.Empty, treeNode.ChildNodes[1]);
            Interupts = AddChild(string.Empty, treeNode.ChildNodes[2]);
            AsString = "Block" + BlockNumber.ToString();
        }
    }

    public class NodeList : DisasmAstNode
    {
        public NodeList() { }
        public void AddNode(AstNode node)
        {
            ChildNodes.Add(node);
            node.SetParent(this);
        }
        public override void DisasmText(StringBuilder build)
        {
            foreach (DisasmAstNode node in ChildNodes)
            {
                node.DisasmText(build);
            }
        }
        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var node in treeNode.ChildNodes)
            {
                AddChild(string.Empty, node);
            }
            if (treeNode.ChildNodes.Count > 0)
                AsString = treeNode.FirstChild.Term.Name + " " +ChildNodes.Count.ToString();
            else
                AsString = "Empty list";
        }
    }

    public class POU : DisasmAstNode
    {
        public string Name;
        public string BlockNumber;
        public int Number;
        public eBLOCK200_POUTYPES Type;
        public AstNode VarInput = null;
        public AstNode VarOutput = null;
        public AstNode VarInOut = null;
        public AstNode Var = null;
        public List<AstNode> Parameters = new List<AstNode>();
        public AstNode Body;

        public POU() {}

        public POU(string name, int number, eBLOCK200_POUTYPES type) {
            Name = name;
            Number = number;
            BlockNumber = number.ToString();
            Type = type;
            Body = new NodeList();
        }
        public override void DisasmText(StringBuilder build)
        {
            switch (Type)
            {
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_MAIN:
                    build.AppendFormat("ORGANIZATION_BLOCK {0}:OB1\n", Name);
                    break;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_SUBROUTINE:
                    build.AppendFormat("SUBROUTINE_BLOCK {0}:SBR{1}\n", Name, Number);
                    break;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_INTERRUPT:
                    build.AppendFormat("INTERRUPT_BLOCK {0}:INT{1}\n", Name, Number);
                    break;
            }

            if (VarInput != null)
            {
                build.AppendLine("VAR_INPUT");
                (VarInput as DisasmAstNode).DisasmText(build);
                build.AppendLine("END_VAR");
            }
            if (VarOutput != null)
            {
                build.AppendLine("VAR_OUTPUT");
                (VarOutput as DisasmAstNode).DisasmText(build);
                build.AppendLine("END_VAR");
            }
            if (VarInOut != null)
            {
                build.AppendLine("VAR_INOUT");
                (VarInOut as DisasmAstNode).DisasmText(build);
                build.AppendLine("END_VAR");
            }
            if (Var != null)
            {
                build.AppendLine("VAR");
                (Var as DisasmAstNode).DisasmText(build);
                build.AppendLine("END_VAR");
            }
            build.AppendLine("BEGIN");
            (Body as DisasmAstNode).DisasmText(build);
            switch (Type)
            {
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_MAIN:
                    build.Append("END_ORGANIZATION_BLOCK\n");
                    break;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_SUBROUTINE:
                    build.Append("END_SUBROUTINE_BLOCK\n");
                    break;
                case eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_INTERRUPT:
                    build.Append("END_INTERRUPT_BLOCK\n");
                    break;
            }
        }

        public void AddSubParam(eSIG200_VAR_TYPE varType, AstNode var)
        {
            switch (varType)
            {
                case eSIG200_VAR_TYPE.eSIG200_VAR_TYPE_IN:
                    if (VarInput == null)
                    {
                        VarInput = new NodeList();
                        VarInput.SetParent(this);
                    }
                    (VarInput as NodeList).AddNode(var);
                    break;
                case eSIG200_VAR_TYPE.eSIG200_VAR_TYPE_OUT:
                    if (VarOutput == null)
                    {
                        VarOutput = new NodeList();
                        VarOutput.SetParent(this);
                    }
                    (VarOutput as NodeList).AddNode(var);
                    break;
                case eSIG200_VAR_TYPE.eSIG200_VAR_TYPE_INOUT:
                    if (VarInOut == null)
                    {
                        VarInOut = new NodeList();
                        VarInOut.SetParent(this);
                    }
                    (VarInOut as NodeList).AddNode(var);
                    break;
                case eSIG200_VAR_TYPE.eSIG200_VAR_TYPE_VAR:
                    if (Var == null)
                    {
                        Var = new NodeList();
                        Var.SetParent(this);
                    }
                    (Var as NodeList).AddNode(var);
                    break;
            }
            Parameters.Add(var);
        }
        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            Name = treeNode.ChildNodes[1].ChildNodes[0].Token.Text;
            BlockNumber = treeNode.ChildNodes[1].ChildNodes[1].Token.Text;

            string nr = string.Empty;
            if (BlockNumber.StartsWith("OB"))
            {
                nr = "0";
                Type = eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_MAIN;
            }
            else if (BlockNumber.StartsWith("SBR"))
            {
                nr = BlockNumber.Substring(3);
                Type = eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_SUBROUTINE;
            }
            else if (BlockNumber.StartsWith("INT"))
            {
                nr = BlockNumber.Substring(3);
                Type = eBLOCK200_POUTYPES.eBLOCK200_POUTYPES_INTERRUPT;
            }
            if (!int.TryParse(nr, out Number))
                Number = 0;
            int varIndex = 2;
            if (treeNode.FirstChild.Token.Text == "SUBROUTINE_BLOCK")
            {
                foreach (var io in treeNode.ChildNodes[2].ChildNodes)
                {
                    switch (io.FirstChild.Token.Text)
                    {
                        case "VAR_INPUT": 
                            VarInput = AddChild(string.Empty, io); 
                            Parameters.AddRange(VarInput.ChildNodes); 
                            break;
                        case "VAR_OUTPUT": 
                            VarOutput = AddChild(string.Empty, io);
                            Parameters.AddRange(VarOutput.ChildNodes);
                            break;
                        case "VAR_IN_OUT": 
                            VarInOut = AddChild(string.Empty, io);
                            Parameters.AddRange(VarInOut.ChildNodes);
                            break;
                    }
                }
                varIndex = 3;
            }
            if (treeNode.ChildNodes[varIndex].ChildNodes.Count > 0)
            {
                Var = AddChild(string.Empty, treeNode.ChildNodes[varIndex].FirstChild);
                Parameters.AddRange(Var.ChildNodes);
            }

            Body = AddChild("Body", treeNode.ChildNodes[varIndex+1].ChildNodes[1]);
            AsString = Name + ":" + BlockNumber;
        }
    }

    public class VarDecl : DisasmAstNode
    {
        public VarDecl() { }
        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            eSIG200_VAR_TYPE Type = eSIG200_VAR_TYPE.eSIG200_VAR_TYPE_VAR;
            switch (treeNode.ChildNodes[0].Token.Text)
            {
                case "VAR_INPUT": Type = eSIG200_VAR_TYPE.eSIG200_VAR_TYPE_IN; break;
                case "VAR_OUTPUT": Type = eSIG200_VAR_TYPE.eSIG200_VAR_TYPE_OUT; break;
                case "VAR_IN_OUT": Type = eSIG200_VAR_TYPE.eSIG200_VAR_TYPE_INOUT; break;
            }
            foreach (var decl in treeNode.ChildNodes[1].ChildNodes[0].ChildNodes)
            {
                var declAst = decl.AstNode as Declaration;
                if (declAst != null)
                    declAst.Parameter.nVarType = (int)Type;
                AddChild(string.Empty, decl);
            }
            AsString = Type.ToString();
        }
    }

    public class Declaration : DisasmAstNode
    {
        public string Name;
        public string Type;
        public sBLOCK200_SUBPARAM Parameter;
        public Declaration() { }
        public Declaration(string name, string type, sBLOCK200_SUBPARAM param)
        {
            Name = name;
            Type = type;
            Parameter = param;
        }
        public override void DisasmText(StringBuilder build)
        {
            var DataTypes = new Dictionary<eBLOCK200_DATA_TYPES, string>(){
                {eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_BOOL, "BOOL"},
                {eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_BYTE, "BYTE"},
                {eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_WORD, "WORD"},
                {eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_DWORD, "DWORD"},
                {eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_CHAR, "CHAR"},
                {eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_INT, "INT"},
                {eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_DINT, "DINT"},
                {eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_REAL, "REAL"},
            };
            build.AppendFormat("{0}:{1};\n", Parameter.szName, DataTypes[(eBLOCK200_DATA_TYPES)Parameter.nDataType]);
        }
        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            Name = treeNode.ChildNodes[0].Token.Text;
            Type = treeNode.ChildNodes[1].Token.Text;;
            Parameter.szName = Name;
            Parameter.nVarType = (int)eSIG200_VAR_TYPE.eSIG200_VAR_TYPE_VAR;
            switch (Type)
            {
                case "BOOL": Parameter.nDataType = (int)eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_BOOL; break;
                case "BYTE": Parameter.nDataType = (int)eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_BYTE; break;
                case "WORD": Parameter.nDataType = (int)eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_WORD; break;
                case "DWORD": Parameter.nDataType = (int)eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_DWORD; break;
                case "CHAR": Parameter.nDataType = (int)eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_CHAR; break;
                case "INT": Parameter.nDataType = (int)eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_INT; break;
                case "DINT": Parameter.nDataType = (int)eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_DINT; break;
                case "REAL": Parameter.nDataType = (int)eBLOCK200_DATA_TYPES.eBLOCK200_TYPE_REAL; break;
            }
            Parameter.nFormat = 0;
            AsString = Name + " : " + Type;
        }
    }

    public class Network : DisasmAstNode
    {
        public int Number;

        public Network() { }
        public override void DisasmText(StringBuilder build)
        {
            build.AppendFormat("NETWORK {0}\n", Number);
            foreach (DisasmAstNode node in ChildNodes)
            {
                node.DisasmText(build);
            }
        }
        public void AddIns(AstNode ins)
        {
            ChildNodes.Add(ins);
            ins.SetParent(this);
        }

        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            Number = (int)treeNode.ChildNodes[1].FirstChild.Token.Value;
            foreach (var ins in treeNode.ChildNodes[3].ChildNodes)
            {
                AddChild(string.Empty, ins.FirstChild);
            }

            AsString = "Network " + Number.ToString();
        }
    }

    public class Instruction : DisasmAstNode
    {
        public string Mnemonic;
        public Instruction() { }
        public override void DisasmText(StringBuilder build)
        {
            build.AppendFormat(Mnemonic+ " ");
            bool first = true;
            foreach (DisasmAstNode node in ChildNodes)
            {
                if (first)
                    first = false;
                else
                    build.Append(",");
                node.DisasmText(build);
            }
            build.Append("\n");

        }
        public void AddOperand(AstNode node)
        {
            ChildNodes.Add(node);
            node.SetParent(this);
        }

        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode.ChildNodes[0]);
            
            Mnemonic = treeNode.ChildNodes[0].ChildNodes[0].Token.Text;
            AsString = Mnemonic;
            foreach (var operand in treeNode.ChildNodes[1].ChildNodes)
            {
                AddChild(string.Empty, operand);
                AsString += " " + (operand.AstNode as AstNode).AsString;
            }
        }
    }

    public class Constrant : DisasmAstNode, IDisposable
    {
        public sBLOCK200_LITERAL_STRUCT Literal = new sBLOCK200_LITERAL_STRUCT();
        public Constrant()
        {
            Literal.pValue = IntPtr.Zero;
            Literal.nBytes = 0;
        }

        public override void DisasmText(StringBuilder build)
        {
            switch (Literal.type)
            {
                case eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_BOOL:
                    {
                        int value = Marshal.ReadInt32(Literal.pValue);
                        build.Append(value == 0 ? "FALSE" : "TRUE");
                    }
                    break;
                case eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_UNSIGNED:
                case eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_TIME:
                    {
                        uint value = (uint)Marshal.ReadInt32(Literal.pValue);
                        build.Append(value.ToString("G"));
                    }
                    break;
                case eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_SIGNED:
                    {
                        int value = Marshal.ReadInt32(Literal.pValue);
                        build.Append(value.ToString("G"));
                    }
                    break;
                case eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_HEXADECIMAL:
                case eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_BINARY:
                    {
                        uint value = (uint)Marshal.ReadInt32(Literal.pValue);
                        build.Append(value.ToString("X"));
                    }
                    break;
                case eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_ASCII:
                    {
                        int value = Marshal.ReadInt32(Literal.pValue);
                        build.Append(((char)value).ToString());
                    }
                    break;
                case eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_REAL:
                    {
                        float value = (float)Marshal.PtrToStructure(Literal.pValue, typeof(float));
                        build.Append(value.ToString("E"));
                    }
                    break;
                case eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_STRING:
                    {
                        string value = Marshal.PtrToStringAnsi(Literal.pValue);
                        build.Append(value);
                    }
                    break;
            }
        }
        ~Constrant()
        {
            Dispose(false);
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed && Literal.pValue != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(Literal.pValue);
                Literal.pValue = IntPtr.Zero;
                Literal.nBytes = 0;
            }
            disposed = true;         
        }
    }

    public class Descriptor : DisasmAstNode
    {
        public eBLOCK200_OPERAND_SIZE Size = eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_UNDEFINED;

        public Descriptor() { }
        public static eBLOCK200_OPERAND_SIZE GetSizeByDescriptor(string desc)
        {
            if (string.IsNullOrEmpty(desc))
                return eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_UNDEFINED;
            switch (desc.ToUpper())
            {
                case "X":
                    return eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_BIT;
                case "B":
                    return eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_BYTE;
                case "W":
                    return eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_WORD;
                case "D":
                    return eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_DWORD;
                default:
                    return eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_UNUSED;
            }

        }
        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.ChildNodes.Count > 0)
            {
                Size = GetSizeByDescriptor(treeNode.FirstChild.Token.Text);
            }
        }
    }

    public class BoolConstrant : Constrant
    {
        public BoolConstrant() { }

        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            var constant = treeNode.ChildNodes[0];
            base.Init(context, constant);
            Literal.type = eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_BOOL;
            Literal.size = eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_BIT;
            Literal.nBytes = 1;
            int value = 1;
            switch (constant.FirstChild.Term.Name)
            {
                case "TRUE":
                case "ON":
                    value = 1;
                    break;
                case "FALSE":
                case "OFF":
                    value = 0;
                    break;
            }
            Literal.nBytes = sizeof(int);
            Literal.pValue = Marshal.AllocCoTaskMem(Literal.nBytes);
            Marshal.StructureToPtr(value, Literal.pValue, true);
        }
    }

    public class NumberConstrant : Constrant
    {
        public NumberConstrant() { }

        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            var number = treeNode.LastChild;
            base.Init(context, number);
            AsString = number.Token.Text;
            var details = (CompoundTerminalBase.CompoundTokenDetails)number.Token.Details;

            object value = number.Token.Value;
            if (number.GetType() == typeof(float)) 
            {
                Literal.type = eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_REAL;
                Literal.size = eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_UNUSED;
                Literal.nBytes = 4;
            }
            else
            {
                Literal.size = (treeNode.ChildNodes[0].AstNode as Descriptor).Size;
                if (details.IsSet((short)NumberOptions.Binary))
                    Literal.type = eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_BINARY;
                else if (details.IsSet((short)NumberOptions.Hex))
                    Literal.type = eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_HEXADECIMAL;
                else
                {
                    if (!string.IsNullOrEmpty(details.Sign))
                        Literal.type = eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_SIGNED;
                    else
                        Literal.type = eBLOCK200_LITERAL_TYPES.eBLOCK200_LITERAL_TYPE_UNSIGNED;
                }
                Literal.nBytes = 4;
                value = (int)value;
            }

            Literal.pValue = Marshal.AllocCoTaskMem(Literal.nBytes);
            Marshal.StructureToPtr(value, Literal.pValue, true);
        }
    }

    public class Address : DisasmAstNode
    {
        public sBLOCK200_ADDRESS_STRUCT S7Address;
        Dictionary<eCOMM200_MEMORY_AREA_MASK, string> Areas = new Dictionary<eCOMM200_MEMORY_AREA_MASK,string>(){
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_I_MEM_ONLY, "I"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_Q_MEM_ONLY, "Q"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_AI_MEM_ONLY, "AI"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_AQ_MEM_ONLY, "AQ"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_SD_MEM_ONLY, "SD"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_V_MEM_ONLY, "V"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_M_MEM_ONLY, "M"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_SM_MEM_ONLY, "SM"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_SCR_MEM_ONLY, "S"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_AC_MEM_ONLY, "AC"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_C_MEM_ONLY, "C"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_T_MEM_ONLY, "T"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_HC_MEM_ONLY, "HC"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_L_MEM_ONLY, "L"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_OB_MEM_ONLY, "OB"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_SBR_MEM_ONLY, "SBR"},
            {eCOMM200_MEMORY_AREA_MASK.eCOMM200_INT_MEM_ONLY, "INT"},
        };

        public Address() { }
        public override void DisasmText(StringBuilder build)
        {
            if (S7Address.mode == eBLOCK200_ADDRESSING_MODE.eBLOCK200_ADDRESSING_MODE_INDIRECT_ADDRESS)
                build.Append("*");
            else if (S7Address.mode == eBLOCK200_ADDRESSING_MODE.eBLOCK200_ADDRESSING_MODE_ADDRESS_OF)
                build.Append("&");
            build.Append(Areas[(eCOMM200_MEMORY_AREA_MASK)S7Address.memoryArea]);
            switch (S7Address.size)
            {
                case eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_BIT:
                    //build.Append("X");
                    break;
                case eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_BYTE:
                    build.Append("B");
                    break;
                case eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_WORD:
                    build.Append("W");
                    break;
                case eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_DWORD:
                    build.Append("D");
                    break;
                case eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_STRING:
                    build.Append("STR");
                    break;
            }
            if (S7Address.size == eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_BIT){
                int offset = S7Address.nOffset >> 3;
                int bit = S7Address.nOffset & 0x7;
                build.AppendFormat("{0}.{1}", offset, bit);
            } else {
                build.Append(S7Address.nOffset.ToString());
            }
        }

        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            var address = treeNode.ChildNodes[0];
            base.Init(context, address);
            AsString = address.Token.Text;
            S7Address = (sBLOCK200_ADDRESS_STRUCT)address.Token.Value;
        }
    }

    public class Variable : DisasmAstNode
    {
        public Variable() { }
        public override void Init(ParsingContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode.LastChild);
            AsString = treeNode.LastChild.Token.Text;
            if (treeNode.FirstChild.Term.Name == "#")
                AsString = "#" + AsString;
        }
    }
}
