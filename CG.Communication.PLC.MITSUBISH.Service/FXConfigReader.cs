using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using CG.Data.DataSource;

namespace CG.Communication.PLC.MITSUBISH.Service
{
    public class FXConfigReader
    {
        public const string PPPSTR = "+ppp";
        //private const string NSTR = "+(N)";
        public const string BLANKSTR = " ";
        public const char ZEROCHAR = '0';
        public const string DCMDPREFIX = "D";
        public const string PCMDSUFFIX = "P";
        public const string PCMDPREFIX = "P";
        public const string KCONSTANT = "K";
        public const string HCONSTANT = "H";
        public const string DIGITINDICATERx = "xx";
        public const string DIGITINDICATERy = "yy";
        public const string DIGITINDICATERz = "zz";
        public const string DIGITINDICATERw = "ww";
        public const string SIMBOLADD = "+";
        public const string SIMBOLDOT = ".";

        private static XmlDocument mDoc = null;
        private static Dictionary<string, Dictionary<string, IFXCmd>> mCmds = new Dictionary<string, Dictionary<string, IFXCmd>>();
        private static Dictionary<string, Dictionary<string, FXBasicCmdCharacter>> mReverseBasicCmds = new Dictionary<string, Dictionary<string, FXBasicCmdCharacter>>();
        private static Dictionary<string, Dictionary<int, FXAppCmd>> mReverseAppCmds = new Dictionary<string, Dictionary<int, FXAppCmd>>();
        private static Dictionary<string, Dictionary<string, FXRegister>> mRegisters = new Dictionary<string, Dictionary<string, FXRegister>>();
        private static Dictionary<string, List<OperandMap>> mOperandMaps = new Dictionary<string, List<OperandMap>>();
        private static Dictionary<string,int> mProgramRegionFrom = new Dictionary<string,int>();
        private static Dictionary<string, FXRegionInfoList> mParamRegionInfos = new Dictionary<string, FXRegionInfoList>();
        private static Dictionary<string, FXRegionInfoList> mSpecialRegionInfos = new Dictionary<string, FXRegionInfoList>();
        private static Dictionary<string, TreeNode<string, FXRegisterReadAddrItem>> mReadAddrs = new Dictionary<string, TreeNode<string, FXRegisterReadAddrItem>>();

        private static Dictionary<string, IFXCmd> GetFXCommands(XmlNode attributeNode)
        {
            Dictionary<string, IFXCmd> cmdList = new Dictionary<string, IFXCmd>();
            XmlNodeList basicCmdNodeList = attributeNode.SelectSingleNode("BasicCmdList").ChildNodes;                       //获得基本指令集
            foreach (XmlNode basicCmdNode in basicCmdNodeList)
            {
                if (basicCmdNode.NodeType == XmlNodeType.Element)
                {
                    FXBasicCmd basicCmd = new FXBasicCmd();
                    basicCmd.Name = basicCmdNode.Attributes["name"].Value;
                    basicCmd.Comment = basicCmdNode.Attributes["comment"].Value;
                    XmlNodeList characterNodeList = basicCmdNode.ChildNodes;
                    foreach (XmlNode characterNode in characterNodeList)
                    {
                        if (characterNode.NodeType == XmlNodeType.Element)
                        {
                            FXBasicCmdCharacter character = new FXBasicCmdCharacter();
                            character.BasicCmdType = (FXBasicCmdType)Convert.ToInt32(characterNode.Attributes["type"].Value);
                            character.Value = characterNode.Attributes["value"].Value;
                            character.Cmd = basicCmd.Name;
                            if (characterNode.Attributes["adapterType"] != null)
                                character.AdapterType = (FXBasicCmdAdapterType)Convert.ToInt32(characterNode.Attributes["adapterType"].Value);
                            else
                                character.AdapterType = FXBasicCmdAdapterType.NULL;
                            if (characterNode.Attributes["adapter"] != null)
                                character.Adapter = characterNode.Attributes["adapter"].Value;
                            if (characterNode.Attributes["min"] != null)
                                character.Min = Convert.ToInt32(characterNode.Attributes["min"].Value);
                            if (characterNode.Attributes["max"] != null)
                                character.Max = Convert.ToInt32(characterNode.Attributes["max"].Value);
                            basicCmd.Characters.Add(character);
                        }
                    }
                    cmdList.Add(basicCmd.Name, basicCmd);
                }
            }
            XmlNodeList appCmdNodeList = attributeNode.SelectSingleNode("AppCmdList").ChildNodes;       //获得应用指令集
            foreach (XmlNode appCmdNode in appCmdNodeList)
            {
                if (appCmdNode.NodeType == XmlNodeType.Element)
                {
                    FXAppCmd appCmd = new FXAppCmd();
                    appCmd.AppCmdType = FXAppCmdType.Normal;
                    appCmd.Comment = appCmdNode.Attributes["comment"].Value;
                    appCmd.FuncNO = Convert.ToInt32(appCmdNode.Attributes["funcNO"].Value);
                    appCmd.Name = appCmdNode.Attributes["name"].Value;
                    appCmd.Group = appCmdNode.Attributes["group"].Value;
                    cmdList.Add(appCmd.Name, appCmd);
                }
            }
            return cmdList;
        }

        private static Dictionary<string,FXBasicCmdCharacter> GetFXReverseBasicCommands(XmlNode attributeNode)
        {
            Dictionary<string, FXBasicCmdCharacter> reverseCmdList = new Dictionary<string, FXBasicCmdCharacter>();
            XmlNodeList basicCmdNodeList = attributeNode.SelectSingleNode("BasicCmdList").ChildNodes;                       //获得基本指令集
            foreach (XmlNode basicCmdNode in basicCmdNodeList)
            {
                if (basicCmdNode.NodeType == XmlNodeType.Element)
                {
                    string cmdName = basicCmdNode.Attributes["name"].Value;
                    XmlNodeList characterNodeList = basicCmdNode.ChildNodes;
                    foreach (XmlNode characterNode in characterNodeList)
                    {
                        if (characterNode.NodeType == XmlNodeType.Element)
                        {
                            FXBasicCmdCharacter character = new FXBasicCmdCharacter();
                            character.BasicCmdType = (FXBasicCmdType)Convert.ToInt32(characterNode.Attributes["type"].Value);
                            character.Value = characterNode.Attributes["value"].Value;
                            character.Cmd = cmdName;
                            if (characterNode.Attributes["adapterType"] != null)
                                character.AdapterType = (FXBasicCmdAdapterType)Convert.ToInt32(characterNode.Attributes["adapterType"].Value);
                            else
                                character.AdapterType = FXBasicCmdAdapterType.NULL;
                            if (characterNode.Attributes["adapter"] != null)
                                character.Adapter = characterNode.Attributes["adapter"].Value;
                            if (characterNode.Attributes["min"] != null)
                                character.Min = Convert.ToInt32(characterNode.Attributes["min"].Value);
                            if (characterNode.Attributes["max"] != null)
                                character.Max = Convert.ToInt32(characterNode.Attributes["max"].Value);
                            string characterKey = "";
                            switch(character.BasicCmdType)
                            {
                                case FXBasicCmdType.PureSingle:
                                    characterKey = character.Value;
                                    break;
                                case FXBasicCmdType.Single:
                                    characterKey = character.Value.Substring(0, 1);
                                    break;
                                case FXBasicCmdType.Double:
                                    characterKey = character.Value.Substring(0, 6);
                                    break;
                                case FXBasicCmdType.Triple:
                                    characterKey = character.Value.Substring(0, 1) + " " + character.Value.Substring(9, 2) + " " + character.Value.Substring(21, 2);
                                    break;
                                case FXBasicCmdType.Five:
                                    characterKey = character.Value.Substring(0, 1) + " " + character.Value.Substring(9, 2) + " " + character.Value.Substring(21, 2) + " " + character.Value.Substring(33, 2) + " " + character.Value.Substring(45, 2);
                                    break;
                            }
                            if(!reverseCmdList.ContainsKey(characterKey))           //如果已经存在，那么就不往里存了，只有在3字指令的时候才会出现已经存在的情况
                                reverseCmdList.Add(characterKey, character);
                        }
                    }
                }
            }
            return reverseCmdList;
        }

        private static Dictionary<int,FXAppCmd> GetFXReverseAppCommands(XmlNode attributeNode)
        {
            Dictionary<int, FXAppCmd> reverseAppCmds = new Dictionary<int, FXAppCmd>();
            XmlNodeList appCmdNodeList = attributeNode.SelectSingleNode("AppCmdList").ChildNodes;       //获得应用指令集
            foreach (XmlNode appCmdNode in appCmdNodeList)
            {
                if (appCmdNode.NodeType == XmlNodeType.Element)
                {
                    FXAppCmd appCmd = new FXAppCmd();
                    appCmd.AppCmdType = FXAppCmdType.Normal;
                    appCmd.Comment = appCmdNode.Attributes["comment"].Value;
                    appCmd.FuncNO = Convert.ToInt32(appCmdNode.Attributes["funcNO"].Value);
                    appCmd.Name = appCmdNode.Attributes["name"].Value;
                    appCmd.Group = appCmdNode.Attributes["group"].Value;
                    reverseAppCmds.Add(appCmd.FuncNO, appCmd);
                }
            }
            return reverseAppCmds;
        }

        private static Dictionary<string, FXRegister> GetFXRegisters(XmlNode attributeNode)
        {
            Dictionary<string, FXRegister> registerList = new Dictionary<string, FXRegister>();
            XmlNodeList registerNodeList = attributeNode.SelectSingleNode("RegisterList").ChildNodes;
            foreach (XmlNode registerNode in registerNodeList)
            {
                if (registerNode.NodeType == XmlNodeType.Element)
                {
                    FXRegister register = new FXRegister();
                    register.Name = registerNode.Attributes["name"].Value;
                    register.AddrFrom = Convert.ToInt32(registerNode.Attributes["addrFrom"].Value);
                    register.AddrTo = Convert.ToInt32(registerNode.Attributes["addrTo"].Value);
                    register.Offset = Convert.ToInt32(registerNode.Attributes["offset"].Value);
                    if (registerNode.Attributes["comment"] != null)
                        register.Comment = registerNode.Attributes["comment"].Value;
                    if (registerNode.Attributes["step"] != null)
                        register.Step = Convert.ToInt32(registerNode.Attributes["step"].Value);
                    else
                        register.Step = 1;
                    if (registerNode.Attributes["format"] != null)
                        register.NameFormatType = (FXRegisterNameFormatType)Convert.ToInt32(registerNode.Attributes["format"].Value);
                    else
                        register.NameFormatType = FXRegisterNameFormatType.Dec;
                    register.From = Convert.ToInt32(registerNode.Attributes["from"].Value, (int)register.NameFormatType);
                    register.To = Convert.ToInt32(registerNode.Attributes["to"].Value, (int)register.NameFormatType);
                    if (registerNode.Attributes["canForce"] != null)
                        register.CanForce = Convert.ToBoolean(registerNode.Attributes["canForce"].Value);
                    else
                        register.CanForce = false;
                    if (registerNode.Attributes["forceFrom"] != null)
                        register.ForceFrom = Convert.ToInt32(registerNode.Attributes["forceFrom"].Value);
                    else
                        register.ForceFrom = register.From;
                    if (registerNode.Attributes["forceTo"] != null)
                        register.ForceTo = Convert.ToInt32(registerNode.Attributes["forceTo"].Value);
                    else
                        register.ForceTo = register.To;
                    if (registerNode.HasChildNodes)
                    {
                        XmlNodeList registerMNNodeList = registerNode.ChildNodes;
                        foreach (XmlNode registerMNNode in registerMNNodeList)
                        {
                            if (registerMNNode.NodeType == XmlNodeType.Element)
                            {
                                FXRegisterMN mn = new FXRegisterMN();
                                mn.From = Convert.ToInt32(registerMNNode.Attributes["from"].Value);
                                mn.To = Convert.ToInt32(registerMNNode.Attributes["to"].Value);
                                mn.M = Convert.ToInt32(registerMNNode.Attributes["m"].Value);
                                mn.N = Convert.ToInt32(registerMNNode.Attributes["n"].Value);
                                if (registerMNNode.Attributes["isPoint"] != null)
                                    mn.IsPoint = Convert.ToBoolean(registerMNNode.Attributes["isPoint"].Value);
                                else
                                    mn.IsPoint = false;
                                if (registerMNNode.Attributes["offset"] != null)
                                    mn.Offset = Convert.ToInt32(registerMNNode.Attributes["offset"].Value);
                                else
                                    mn.Offset = 0;
                                if (registerMNNode.Attributes["step"] != null)
                                    mn.Step = Convert.ToInt32(registerMNNode.Attributes["step"].Value);
                                else
                                    mn.Step = 1;
                                register.MN.Add(mn);
                            }
                        }
                    }
                    registerList.Add(register.Name, register);
                }
            }
            return registerList;
        }

        private static List<OperandMap> GetFXRegisterMaps(XmlNode attributeNode)
        {
            List<OperandMap> mapList = new List<OperandMap>();
            XmlNodeList mapNodeList = attributeNode.SelectSingleNode("MapList").ChildNodes;
            foreach (XmlNode mapNode in mapNodeList)
            {
                if(mapNode.NodeType == XmlNodeType.Element)
                {
                    OperandMap map = new OperandMap();
                    map.Name = mapNode.Attributes["name"].Value;
                    map.From = Convert.ToInt32(mapNode.Attributes["from"].Value, 16);
                    map.To = Convert.ToInt32(mapNode.Attributes["to"].Value, 16);
                    map.ReName = mapNode.Attributes["reName"].Value;
                    mapList.Add(map);
                }
            }
            return mapList;
        }

        private static int GetFXProgramRegionFrom(XmlNode attributeNode)
        {
            XmlNode programRegionNode = attributeNode.SelectSingleNode("ProgramRegion");
            string from = programRegionNode.Attributes["from"].Value;
            return Convert.ToInt32(from);
        }

        private static FXRegionInfoList GetFXRegionInfoList(XmlNode attributeNode,FXRegionType regionType)
        {
            FXRegionInfoList list = new FXRegionInfoList();
            XmlNode regionNode;
            if(regionType == FXRegionType.Param)
                regionNode = attributeNode.SelectSingleNode("ParamRegion");
            else
                regionNode = attributeNode.SelectSingleNode("SpecialRegion");
            list.From = Convert.ToInt32(regionNode.Attributes["from"].Value);
            list.Len = Convert.ToInt32(regionNode.Attributes["len"].Value);
            list.IsLoaded = false;
            list.LoadTime = DateTime.MinValue;
            XmlNodeList paramNodeList = regionNode.ChildNodes;
            foreach (XmlNode paramNode in paramNodeList)
            {
                if(paramNode.NodeType == XmlNodeType.Element)
                {
                    FXRegionInfoItem info = new FXRegionInfoItem();
                    info.Name = paramNode.Attributes["name"].Value;
                    info.TypeValue = Convert.ToInt32(paramNode.Attributes["type"].Value);
                    info.Offset = Convert.ToInt32(paramNode.Attributes["offset"].Value);
                    info.Len = Convert.ToInt32(paramNode.Attributes["len"].Value);
                    info.Value = null;
                    list.Items.Add(info.TypeValue, info);
                }
            }
            return list;
        }

        private static TreeNode<string, FXRegisterReadAddrItem> GetFXReadAddrList(XmlNode attributeNode, Dictionary<string, FXRegister> registerDict)
        {
            TreeNode<string, FXRegisterReadAddrItem> rootNode = new TreeNode<string, FXRegisterReadAddrItem>();
            XmlNodeList registerNodeList = attributeNode.SelectSingleNode("RegisterReadAddrList").ChildNodes;
            foreach (XmlNode registerNode in registerNodeList)              //第一层循环，解出寄存器类型，这一层只有寄存器名称
            {
                if(registerNode.NodeType == XmlNodeType.Element)
                {
                    FXRegisterReadAddrItem item1Level = new FXRegisterReadAddrItem();
                    item1Level.Register = registerNode.Attributes["name"].Value;
                    TreeNode<string, FXRegisterReadAddrItem> item1LevelNode = new TreeNode<string, FXRegisterReadAddrItem>();
                    item1LevelNode.Key = item1Level.Register;
                    rootNode.Add(item1LevelNode.Key, item1LevelNode);                   //添加树的第一层子节点
                    if (registerDict.ContainsKey(item1LevelNode.Key))              //有这个寄存器类型才添加
                    {
                        FXRegister register = registerDict[item1LevelNode.Key];
                        XmlNodeList readAddrNodeList = registerNode.ChildNodes;
                        foreach (XmlNode readAddrNode in readAddrNodeList)      //第二层循环，解出FXRegisterReadAddrType列出的所有类型地址
                        {
                            if (readAddrNode.NodeType == XmlNodeType.Element)
                            {
                                FXRegisterReadAddrItem item2Level = new FXRegisterReadAddrItem();
                                item2Level.Register = item1LevelNode.Key;
                                item2Level.IsOutputItem = false;
                                item2Level.NameFormatType = register.NameFormatType;
                                item2Level.Type = Convert.ToInt32(readAddrNode.Attributes["type"].Value);
                                if (readAddrNode.Attributes["bitmask"] != null)
                                    item2Level.Bitmask = Convert.ToInt32(readAddrNode.Attributes["bitmask"].Value);
                                if (readAddrNode.Attributes["from"] != null)
                                    item2Level.From = Convert.ToInt32(readAddrNode.Attributes["from"].Value);
                                TreeNode<string, FXRegisterReadAddrItem> item2LevelNode = new TreeNode<string, FXRegisterReadAddrItem>();
                                item2LevelNode.Key = item2Level.Type.ToString();
                                item2LevelNode.Value = item2Level;
                                item1LevelNode.Add(item2LevelNode.Key, item2LevelNode);             //添加树的第二层子节点

                                XmlNodeList readAddrOutputNodeList = readAddrNode.ChildNodes;
                                if (readAddrOutputNodeList != null&&readAddrOutputNodeList.Count>0)             //如果是output类型的节点，那么执行第三层循环
                                {
                                    foreach (XmlNode readAddrOutputNode in readAddrOutputNodeList)      //第三层循环，解出FXRegisterReadAddrOutputType列出的所有输出类型地址
                                    {
                                        if (readAddrOutputNode.NodeType == XmlNodeType.Element)
                                        {
                                            FXRegisterReadAddrItem item3Level = new FXRegisterReadAddrItem();
                                            item3Level.Register = item1LevelNode.Key;
                                            item3Level.IsOutputItem = true;
                                            item3Level.NameFormatType = register.NameFormatType;
                                            item3Level.Type = Convert.ToInt32(readAddrOutputNode.Attributes["type"].Value);
                                            item3Level.From = Convert.ToInt32(readAddrOutputNode.Attributes["from"].Value);
                                            item3Level.Bitmask = Convert.ToInt32(readAddrOutputNode.Attributes["bitmask"].Value);
                                            TreeNode<string, FXRegisterReadAddrItem> item3LevelNode = new TreeNode<string, FXRegisterReadAddrItem>();
                                            item3LevelNode.Key = item3Level.Type.ToString();
                                            item3LevelNode.Value = item3Level;
                                            item2LevelNode.Add(item3LevelNode.Key, item3LevelNode);             //添加树的第三层子节点
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return rootNode;
        }

        public static void LoadFXConfigFile(string file)
        {
            UnLoadFXConfigFile();
            if (FXConfigReader.mDoc == null)
            {
                FXConfigReader.mDoc = new XmlDocument();
                FXConfigReader.mDoc.Load(file);
                XmlNodeList attributeLists = FXConfigReader.mDoc.SelectNodes("FXSeries/AttributeList");
                foreach (XmlNode attributeList in attributeLists)
                {
                    string deviceModel = attributeList.Attributes["belong"].Value;
                    if(!FXConfigReader.mCmds.ContainsKey(deviceModel)&&!FXConfigReader.mRegisters.ContainsKey(deviceModel))
                    {
                        Dictionary<string,IFXCmd> iFXCmdDic = FXConfigReader.GetFXCommands(attributeList);
                        Dictionary<string, FXBasicCmdCharacter> fxReverseBasicCmdDic = FXConfigReader.GetFXReverseBasicCommands(attributeList);
                        Dictionary<int, FXAppCmd> fxReverseAppCmdDic = FXConfigReader.GetFXReverseAppCommands(attributeList);
                        Dictionary<string,FXRegister> fXRegisterDic = FXConfigReader.GetFXRegisters(attributeList);
                        List<OperandMap> mapList = FXConfigReader.GetFXRegisterMaps(attributeList);
                        int programRegionFrom = FXConfigReader.GetFXProgramRegionFrom(attributeList);
                        FXRegionInfoList paramRegionList = FXConfigReader.GetFXRegionInfoList(attributeList, FXRegionType.Param);
                        FXRegionInfoList specialRegionList = FXConfigReader.GetFXRegionInfoList(attributeList, FXRegionType.Special);
                        TreeNode<string, FXRegisterReadAddrItem> readAddrList = FXConfigReader.GetFXReadAddrList(attributeList, fXRegisterDic);
                        FXConfigReader.mCmds.Add(deviceModel, iFXCmdDic);
                        FXConfigReader.mReverseBasicCmds.Add(deviceModel, fxReverseBasicCmdDic);
                        FXConfigReader.mReverseAppCmds.Add(deviceModel, fxReverseAppCmdDic);
                        FXConfigReader.mRegisters.Add(deviceModel, fXRegisterDic);
                        FXConfigReader.mOperandMaps.Add(deviceModel, mapList);
                        FXConfigReader.mProgramRegionFrom.Add(deviceModel, programRegionFrom);
                        FXConfigReader.mParamRegionInfos.Add(deviceModel, paramRegionList);
                        FXConfigReader.mSpecialRegionInfos.Add(deviceModel, specialRegionList);
                        FXConfigReader.mReadAddrs.Add(deviceModel, readAddrList);
                    }
                }
            }
        }

        public static void UnLoadFXConfigFile()
        {
            FXConfigReader.mDoc = null;
            FXConfigReader.mCmds.Clear();
            FXConfigReader.mReverseBasicCmds.Clear();
            FXConfigReader.mReverseAppCmds.Clear();
            FXConfigReader.mRegisters.Clear();
            FXConfigReader.mOperandMaps.Clear();
            FXConfigReader.mProgramRegionFrom.Clear();
            FXConfigReader.mParamRegionInfos.Clear();
            FXConfigReader.mSpecialRegionInfos.Clear();
            FXConfigReader.mReadAddrs.Clear();
        }

        public static bool IsFXModelExist(string fxModel)
        {
            bool isCmd = FXConfigReader.Cmds.ContainsKey(fxModel);
            bool isRegister = FXConfigReader.Registers.ContainsKey(fxModel);
            bool isOperand = FXConfigReader.OperandMaps.ContainsKey(fxModel);
            bool isProgramRegion = FXConfigReader.ProgramRegionFrom.ContainsKey(fxModel);
            bool isParamRegion = FXConfigReader.ParamRegionInfos.ContainsKey(fxModel);
            bool isSpecialRegion = FXConfigReader.SpecialRegionInfos.ContainsKey(fxModel);
            bool isReadAddr = FXConfigReader.ReadAddrs.ContainsKey(fxModel);
            return isCmd & isReadAddr & isOperand & isProgramRegion & isParamRegion & isSpecialRegion & isReadAddr;
        }

        public static Dictionary<string, Dictionary<string, IFXCmd>> Cmds
        {
            get { return FXConfigReader.mCmds; }
        }
        public static Dictionary<string, Dictionary<string, FXBasicCmdCharacter>> ReverseBasicCmds
        {
            get { return FXConfigReader.mReverseBasicCmds; }
        }
        public static Dictionary<string,Dictionary<int,FXAppCmd>> ReverseAppCmds
        {
            get { return FXConfigReader.mReverseAppCmds; }
        }
        public static Dictionary<string, Dictionary<string, FXRegister>> Registers
        {
            get { return FXConfigReader.mRegisters; }
        }
        public static Dictionary<string,List<OperandMap>> OperandMaps
        {
            get { return FXConfigReader.mOperandMaps; }
        }
        public static Dictionary<string,int> ProgramRegionFrom
        {
            get { return FXConfigReader.mProgramRegionFrom; }
        }
        public static Dictionary<string,FXRegionInfoList> ParamRegionInfos
        {
            get { return FXConfigReader.mParamRegionInfos; }
        }
        public static Dictionary<string,FXRegionInfoList> SpecialRegionInfos
        {
            get { return FXConfigReader.mSpecialRegionInfos; }
        }
        public static Dictionary<string, TreeNode<string, FXRegisterReadAddrItem>> ReadAddrs
        {
            get { return FXConfigReader.mReadAddrs; }
        }
        
       
    }
}
