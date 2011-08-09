using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using CG.Data.DataSource;

namespace CG.Communication.PLC.MITSUBISH.Service
{
    /// <summary>FX的一条注释
    /// 
    /// </summary>
    [DataContract]
    public class FXCommentInfo
    {
        [DataMember]
        public char RegsterName { get; set; }
        [DataMember]
        public int RegisterNO { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public static int CommentBytesLen
        {
            get { return 20; }
        }

        /// <summary>这个方法将Comment数据字符串解析并放到对应的字段里
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void SetComment(string data)
        {
            if (data.Length != 40)
                throw new Exception("the length of one comment string must be 40");
            this.RegsterName = (char)Convert.ToInt32(data.Substring(0, 2), 16);
            this.RegisterNO = Convert.ToInt32(data.Substring(2, 2), 16) + (Convert.ToInt32(data.Substring(4, 2), 16) << 8);
            int indexOfEnd = data.IndexOf("00", 6) == -1 ? 0 : data.IndexOf("00", 6);
            this.Comment = Utils.GetGB2312FromBytesInTwoCharFormat(data.Substring(6, indexOfEnd - 6));
            this.Comment = this.Comment.Trim();
        }
        /// <summary>这个方法将这个结构体实例中的信息转换成FX的注释字节数组
        /// 注释内容最长为8个中文字符，16个英文字符(以GB2312编码)
        /// 注释内容不够16个字节的用0x20填充，超过16个字节的截断到16个字节
        /// </summary>
        /// <returns></returns>
        public byte[] GetComment()
        {
            byte[] result = new byte[20];
            result[0] = Convert.ToByte(RegsterName);              //软件元符号
            result[1] = (byte)RegisterNO;                   //软件元编号低字节
            result[2] = (byte)(RegisterNO >> 8);        //软件元编号高字节
            for (int i = 3; i < 19;i++ )                        //填充空格
                result[i] = 0x20;
            if (this.Comment != null && this.Comment != string.Empty)
            {
                byte[] comment = Encoding.GetEncoding("GB2312").GetBytes(this.Comment);
                int commentLen = comment.Length > 16 ? 16 : comment.Length;
                Array.Copy(comment, 0, result, 3, commentLen);
            }
            result[19] = 0x00;
            return result;
        }
        /// <summary>这个方法将这个结构体实例中的信息转换成FX的注释字符串数组
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] GetCommentStrArray()
        {
            byte[] comment = this.GetComment();
            string[] strArray = new string[20];
            for (int i = 0; i < 20;i++ )
            {
                strArray[i] = Utils.FillStringWithChar(Convert.ToString(comment[i], 16), '0', 2, true);
            }
            return strArray;
        }
        /// <summary>这个方法返回一个空的注释字节数组(20个0x00)
        /// 
        /// </summary>
        /// <returns></returns>
        public static byte[] GetEmptyComment()
        {
            byte[] result = new byte[20];
            return result;
        }
        /// <summary>这个方法返回一个空的注释字符串数组(20个0x00)
        /// 
        /// </summary>
        /// <returns></returns>
        public static string[] GetEmptyCommentStrArray()
        {
            byte[] comment = GetEmptyComment();
            string[] strArray = new string[20];
            for (int i = 0; i < 20; i++)
            {
                strArray[i] = comment[i].ToString("00");
            }
            return strArray;
        }
    }

    /// <summary>FX的注释区域读取结果
    /// 
    /// </summary>
    [DataContract]
    public class FXReadCommentResult
    {
        private List<FXCommentInfo> mComments;

        [DataMember]
        public string Msg { get; set; }
        [DataMember]
        public List<FXCommentInfo> Comments
        {
            get 
            {
                if (this.mComments == null)
                    this.mComments = new List<FXCommentInfo>();
                return mComments; 
            }
        }
    }

    /// <summary>编译结果及结果消息
    /// 
    /// </summary>
    [DataContract]
    public class FXComplieResult
    {
        [DataMember]
        public string Msg { get; set; }
        [DataMember]
        public string ComplieStr { get; set; }
        
        public string[] GetComplieStrArray()
        {
            string[] tempArray = this.ComplieStr.Split(new string[]{" "},StringSplitOptions.RemoveEmptyEntries);
            string[] resultArray = new string[tempArray.Length*2];
            for (int i = 0; i < tempArray.Length;i++ )
            {
                resultArray[i * 2] = tempArray[i].Substring(0, 2);               
                resultArray[i * 2 + 1] = tempArray[i].Substring(2, 2);
            }
            return resultArray;
        }
    }

    /// <summary>反编译结果及消息
    /// 
    /// </summary>
    [DataContract]
    public class FXDeComplieResult
    {
        [DataMember]
        public string Msg { get; set; }
        [DataMember]
        public string DecomplieStr { get; set; }
    }

    /// <summary>读取程序结果及消息
    /// 
    /// </summary>
    [DataContract]
    public class FXReadCodeResult
    {
        [DataMember]
        public string Msg { get; set; }
        [DataMember]
        public string Code { get; set; }
    }

    /// <summary>读取PLC运行状态结果及消息
    /// 
    /// </summary>
    [DataContract]
    public class FXReadStatusResult
    {
        [DataMember]
        public string Msg { get; set; }
        [DataMember]
        public FXStatus Status { get; set; }
    }

    /// <summary>测试通讯结果及消息
    /// 
    /// </summary>
    [DataContract]
    public class FXENQResult
    {
        [DataMember]
        public byte Result { get; set; }
        [DataMember]
        public string Msg { get; set; }
    }

    /// <summary>远程控制结果及消息
    /// 
    /// </summary>
    [DataContract]
    public class FXRemoteControlResult
    {
        [DataMember]
        public bool IsSuccess { get; set; }
        [DataMember]
        public string Msg { get; set; }
    }

    /// <summary>获得区域信息结果及消息
    /// 包括参数区和特殊寄存器
    /// </summary>
    [DataContract]
    public class FXRegionInfoResult
    {
        private Dictionary<int, FXRegionInfo> mItems;       //参数区时可以将key强制转换成FXParamType，特殊寄存器可以转换成FXSpecialType

        public FXRegionInfoResult()
        {
            this.mItems = new Dictionary<int, FXRegionInfo>();
        }

        [DataMember]
        public string Msg { get; set; }
        [DataMember]
        public Dictionary<int, FXRegionInfo> Items
        {
            get { return mItems; }
        }
    }

    /// <summary>PLC区域信息项，包括参数区和特殊寄存器
    /// 这个类作为通用的区域信息项基类，提供最一般的ValueStr属性
    /// 获得ValueStr值属性时，该属性简单的将Value中保存的Byte数组逐个变成Char，然后以4个chars为一组前后交换，如果不满4一组那么保持原样
    /// 该属性为只读，如果为该属性赋值那什么也不执行
    /// </summary>
    [DataContract]
    [KnownType("GetTypes")]
    public class FXRegionInfo
    {
        protected string mName;
        protected byte[] mValue;
        protected int mType;          //参数区时可以将此项强制转换成FXParamType，特殊寄存器可以转换成FXSpecialType
        private string mClientStr;        //客户端设置的值，服务器通过自己的逻辑处理这个值

        static Type[] GetTypes()
        {
            return new Type[] { typeof(FXStringRegionInfo),typeof(FXRegisterRegionInfo) };
        }
        
        [DataMember]
        public string Name
	    {
		    get { return mName; }
		    set { mName = value; }
	    }
        [DataMember]
	    public byte[] Value
	    {
		    get { return mValue; }
		    set { mValue = value; }
	    }
        [DataMember]
        public int Type
        {
            get { return mType; }
            set { mType = value; }
        }
        [DataMember]
        public string ClientStr
        {
            get { return this.mClientStr; }
            set { this.mClientStr = value; }
        }
        [DataMember]
        public virtual string ValueStr
        {
            get
            {
                return Utils.BytesToStrInRightOrder(true, this.mValue);
            }
            set { }
        }

        public virtual string[] GetRegionValueStrArray()
        {
            string[] result = new string[this.mValue.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ((char)this.mValue[i * 2]) + ((char)this.mValue[i * 2 + 1]).ToString();
            }
            return result;
        }

        public virtual void SetClientValue()
        {
            string resultStr = Utils.FillStringWithChar(this.ClientStr, '0', 4, true);
            this.mValue = Utils.StrToBytesInRightOrder(true, resultStr);
        }
    }
    /// <summary>PLC区域信息项的细化，可以用来表示值为一个字符串的区域信息项
    /// 该类重写了基类FXRegionInfo的ValueStr属性，将Value中的byte数组当成GB2312编码的字符串
    /// 注意，这个时候Value中的数据不用交换顺序
    /// </summary>
    [DataContract]
    public class FXStringRegionInfo : FXRegionInfo
    {
        public FXStringRegionInfo(int len)
        {
            this.Len = len;
        }

        [DataMember]
        public int Len { get; set; }

        [DataMember]
        public override string ValueStr
        {
            get 
            {
                string result = Utils.BytesToStrInRightOrder(false, this.mValue);
                result = Utils.GetGB2312FromBytesInTwoCharFormat(result);
                return result;
            }
            set { }
        }

        public override void SetClientValue()
        {
            byte[] temp = Encoding.GetEncoding("GB2312").GetBytes(this.ClientStr);
            int len = temp.Length > this.Len ? this.Len : temp.Length;
            this.mValue = new byte[this.Len * 2];
            string tempStr = BitConverter.ToString(temp).Replace("-", "") + "2020202020202020202020202020202020202020202020";
            for (int i = 0; i < this.mValue.Length; i++)
            {
                this.mValue[i] = (byte)tempStr[i];
            }
        }
    }
    /// <summary>PLC区域信息项的细化，可以用来标识值为一个寄存器的区域信息项
    /// 该类重写了基类FXRegionInfo的ValueStr属性，将Value中的byte数组当成PLC寄存器的绝对地址值
    /// 需要使用配置文件RegisterList节点信息将绝对地址对应成寄存器名称
    /// </summary>
    [DataContract]
    public class FXRegisterRegionInfo : FXRegionInfo
    {
        public FXRegisterRegionInfo(string fxModel,string register)
        {
            this.FxModel = fxModel;
            this.Register = register;
        }

        [DataMember]
        public string Register { get; set; }
        [DataMember]
        public string FxModel { get; set; }
        [DataMember]
        public override string ValueStr
        {
            get
            {
                string result = Utils.BytesToStrInRightOrder(true, this.mValue);
                if (FXConfigReader.Registers.ContainsKey(this.FxModel))
                {
                    Dictionary<string, FXRegister> registers = FXConfigReader.Registers[this.FxModel];
                    if(registers.ContainsKey(this.Register))
                    {
                        FXRegister register = registers[this.Register];
                        int resultValue = Convert.ToInt32(result, 16);
                        int registerNo = 0;
                        if (register.Step == 1)
                            registerNo = resultValue - register.Offset - register.AddrFrom + register.From;
                        else
                            registerNo = resultValue / register.Step;
                        result = this.Register + Convert.ToString(registerNo, (int)register.NameFormatType);
                    }
                }
                return result;
            }
            set { }
        }

        public override void SetClientValue()
        {
            if (FXConfigReader.Registers.ContainsKey(this.FxModel))
            {
                Dictionary<string, FXRegister> registers = FXConfigReader.Registers[this.FxModel];
                if (registers.ContainsKey(this.Register))
                {
                    FXRegister register = registers[this.Register];
                    int resultValue = 0;
                    int registerNo = Convert.ToInt32(this.ClientStr, (int)register.NameFormatType);
                    if (register.Step == 1)
                        resultValue = registerNo + register.Offset + register.AddrFrom - register.From;
                    else
                        resultValue = registerNo * register.Step;
                    string resultStr = Utils.FillStringWithChar(resultValue.ToString("X"), '0', 4, true);
                    this.mValue = Utils.StrToBytesInRightOrder(true, resultStr);
                }
            }
        }
    }

    /// <summary>读取位元值的返回结果集
    /// 
    /// </summary>
    [DataContract]
    public class FXRegisterReadResult
    {
        /// <summary>key=Register+NO
        /// 
        /// </summary>
        [DataMember]
        public Dictionary<string, byte[]> Values { get; set; }
        [DataMember]
        public string Msg { get; set; }
    }

    /// <summary>寄存器的最基本信息，包括名称和编号
    /// 
    /// </summary>
    [DataContract]
    [KnownType(typeof(FXRegisterReadInfo))]
    public class FXRegisterBasicInfo
    {
        [DataMember]
        public string Register { get; set; }
        /// <summary>这个编号是最原始的编号，几进制都有可能，具体看Register
        /// 
        /// </summary>
        [DataMember]
        public int NO { get; set; }

        /// <summary>服务器端调用这个方法获得强制该位元ON/OFF命令
        /// 
        /// </summary>
        /// <param name="fxModel"></param>
        /// <param name="isOn"></param>
        /// <returns></returns>
        public byte[] GetForceOnOFFCmd(string fxModel,bool isOn)
        {
            Dictionary<string,FXRegister> registerDict = FXConfigReader.Registers[fxModel];
            if (registerDict.ContainsKey(this.Register))           //软件元类型必须存在
            {
                FXRegister register = registerDict[this.Register];
                if (register.CanForce && this.NO >= register.ForceFrom && this.NO <= register.ForceTo)         //如果这个软件元可以强制ON/OFF，并且在规定的范围内
                {
                    int addr = Convert.ToInt32(this.NO.ToString(), (int)register.NameFormatType) - register.ForceFrom + register.Offset;
                    byte[] cmd;
                    if(isOn)
                        cmd = FXCommander1.CreateCommand(FXSeriesControl.STX, FXSeriesCmd.ON, addr, 1, null);
                    else
                        cmd = FXCommander1.CreateCommand(FXSeriesControl.STX, FXSeriesCmd.OFF, addr, 1, null);
                    return cmd;
                }
                else
                    return null;
            }
            else
                return null;
        }
        /// <summary>服务器端调用这个方法获得写字元软元件命令
        /// 
        /// </summary>
        /// <param name="fxModel"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetWriteRegisterCmd(string fxModel, byte[] valueArray)
        {
            TreeNode<string, FXRegisterReadAddrItem> rootNode = FXConfigReader.ReadAddrs[fxModel];
            TreeNode<string, FXRegisterReadAddrItem> outputNode = TreeNode<string, FXRegisterReadAddrItem>.FindTreeNode(rootNode, this.Register,((int)FXRegisterReadAddrType.Current).ToString());
            if (outputNode != null&&outputNode.Value.Bitmask>=8)            //当前值配置必须存在并且掩码位数必须大于等于8(要不然怎么能说是字元呢？)
            {
                int addr = outputNode.Value.GetPhycalAddr(this.NO, fxModel);            //获得这个元件的物理地址
                //int len = outputNode.Value.PhycalLen;
                int len = outputNode.Value.PhycalLen > valueArray.Length ? outputNode.Value.PhycalLen : valueArray.Length;
                //string hexValueStr = BasicTools.FillStringWithChar(value.ToString("X"), '0', len * 2, true);
                string[] dataArray = new string[len];
                //for (int i = 0; i < hexValueStr.Length / 2; i = i + 2)
                //{
                //    dataArray[i] = hexValueStr.Substring(hexValueStr.Length - 2 * (i + 1), 2);
                //    dataArray[i + 1] = hexValueStr.Substring(hexValueStr.Length - 2 * (i + 2), 2);
                //}
                return FXCommander1.CreateCommandEx(FXSeriesControl.STX, FXSeriesCmd.Write, addr, len, valueArray);
            }
            else
                return null;
        }

        public byte[] GetWriteRegisterCmd(string fxModel, int value)
        {
            TreeNode<string, FXRegisterReadAddrItem> rootNode = FXConfigReader.ReadAddrs[fxModel];
            TreeNode<string, FXRegisterReadAddrItem> outputNode = TreeNode<string, FXRegisterReadAddrItem>.FindTreeNode(rootNode, this.Register, ((int)FXRegisterReadAddrType.Current).ToString());
            if (outputNode != null && outputNode.Value.Bitmask >= 8)            //当前值配置必须存在并且掩码位数必须大于等于8(要不然怎么能说是字元呢？)
            {
                int addr = outputNode.Value.GetPhycalAddr(this.NO, fxModel);            //获得这个元件的物理地址
                int len = outputNode.Value.PhycalLen;
                //int len = outputNode.Value.PhycalLen > valueArray.Length ? outputNode.Value.PhycalLen : valueArray.Length;
                string hexValueStr = Utils.FillStringWithChar(value.ToString("X"), '0', len * 2, true);
                string[] dataArray = new string[len];
                for (int i = 0; i < hexValueStr.Length / 2; i = i + 2)
                {
                    dataArray[i] = hexValueStr.Substring(hexValueStr.Length - 2 * (i + 1), 2);
                    dataArray[i + 1] = hexValueStr.Substring(hexValueStr.Length - 2 * (i + 2), 2);
                }
                return FXCommander1.CreateCommand(FXSeriesControl.STX, FXSeriesCmd.Write, addr, len, dataArray);
            }
            else
                return null;
        }
    }

    [DataContract]
    public class FXRegisterReadInfo : FXRegisterBasicInfo
    {
        [DataMember]
        public int ReadLen { get; set; }
        //[DataMember]
        //public string ValueStr { get; private set; }

        public byte[] GetReadRegisterCmd(string fxModel)
        {
            TreeNode<string, FXRegisterReadAddrItem> rootNode = TreeNode<string, FXRegisterReadAddrItem>.FindTreeNode(FXConfigReader.ReadAddrs[fxModel], this.Register);
            FXRegisterReadAddrItem item;
            if(this.Register.Equals("X")||this.Register.Equals("Y")||this.Register.StartsWith("M")||this.Register.Equals("S"))
                item = TreeNode<string, FXRegisterReadAddrItem>.FindTreeNode(rootNode, ((int)FXRegisterReadAddrType.Contact).ToString()).Value;
            else
                item = TreeNode<string, FXRegisterReadAddrItem>.FindTreeNode(rootNode, ((int)FXRegisterReadAddrType.Current).ToString()).Value;
            int phycalAddr = item.GetPhycalAddr(this.NO, fxModel);
            int readPhysicalLen = this.ReadLen / 8;
            readPhysicalLen = readPhysicalLen > item.PhycalLen ? readPhysicalLen : item.PhycalLen;
            byte[] tempCmd = FXCommander1.CreateCommand(FXSeriesControl.STX, FXSeriesCmd.Read, phycalAddr, readPhysicalLen, null);
            return tempCmd;
        }

        public byte[] GetValueBytes(byte[] value)
        {
            return Utils.BytesToBytes(value);
        }
    }

    [DataContract]
    [KnownType("GetTypes")]
    public class IFXCmd
    {
        static Type[] GetTypes()
        {
            return new Type[] { typeof(FXBasicCmd), typeof(FXAppCmd) };
        }

        [DataMember]
        public virtual FXCmdType CmdType { get { return FXCmdType.Basic; } set { } }
        [DataMember]
        public virtual string Name { get{return null;} set{} }
        [DataMember]
        public virtual string Comment { get { return null; } set { } }
    }

    /// <summary>基本命令
    /// 
    /// </summary>
    [DataContract]
    public class FXBasicCmd : IFXCmd
    {
        /// <summary>基本命令个性集
        /// 一个基本命令可以有一个或者多个个性
        /// </summary>
        private List<FXBasicCmdCharacter> mCharacters;
        
        [DataMember]
        public override FXCmdType CmdType
        {
            get
            {
                return FXCmdType.Basic;
            }
            set { }
        }
        /// <summary>基本命令名称
        /// 
        /// </summary>
        [DataMember]
        public override string Name { get; set; }
        /// <summary>基本命令描述
        /// 
        /// </summary>
        [DataMember]
        public override string Comment { get; set; }
        [DataMember]
        public List<FXBasicCmdCharacter> Characters
        {
            get
            {
                if (this.mCharacters == null)
                    this.mCharacters = new List<FXBasicCmdCharacter>();
                return this.mCharacters;
            }
            set { }
        }
    }

    /// <summary>应用命令
    /// 
    /// </summary>
    [DataContract]
    public class FXAppCmd : IFXCmd
    {
        [DataMember]
        public override FXCmdType CmdType
        {
            get
            {
                return FXCmdType.Application;
            }
            set { }
        }
        /// <summary>应用命令名称
        /// 
        /// </summary>
        [DataMember]
        public override string Name { get; set; }
        /// <summary>应用命令描述
        /// 
        /// </summary>
        [DataMember]
        public override string Comment { get; set; }
        /// <summary>应用命令组名称
        /// 
        /// </summary>
        [DataMember]
        public string Group { get; set; }
        /// <summary>应用命令的功能编号
        /// 
        /// </summary>
        [DataMember]
        public int FuncNO { get; set; }
        /// <summary>应用命令类型
        /// 
        /// </summary>
        [DataMember]
        public FXAppCmdType AppCmdType { get; set; }

        /// <summary>服务器端调用这个方法来获取指定机器码的应用指令类型
        /// 
        /// </summary>
        /// <param name="code">机器码字符串(4位16进制表示)</param>
        /// <param name="funcNO">应用指令编号</param>
        /// <returns></returns>
        public static FXAppCmdType GetAppCmdTypeAndFuncNO(string code,ref int funcNO)
        {
            FXAppCmdType returnType = FXAppCmdType.Normal;
            int valueInt = Convert.ToInt32(code, 16);
            if (valueInt > 0x1000 && valueInt % 2 == 0)
            {
                returnType = FXAppCmdType.P;
                funcNO = (valueInt - 0x1000) / 2 - 8;
            }
            else if (valueInt > 0x1000 && valueInt % 2 == 1)
            {
                returnType = FXAppCmdType.DP;
                funcNO = (valueInt - 0x1001) / 2 - 8;
            }
            else if (valueInt < 0x1000 && valueInt % 2 == 1)
            {
                returnType = FXAppCmdType.D;
                funcNO = (valueInt - 1) / 2 - 8;
            }
            return returnType;
        }
    }
    /// <summary>基本命令的个性信息
    /// 一个基本命令可以有一个或者多个个性，个性的类型由FXBasicCmdType定义
    /// </summary>
    [DataContract]
    public class FXBasicCmdCharacter
    {
        /// <summary>基本命令的适配器类型
        /// 
        /// </summary>
        [DataMember]
        public FXBasicCmdAdapterType AdapterType { get; set; }
        /// <summary>基本命令类型
        /// 
        /// </summary>
        [DataMember]
        public FXBasicCmdType BasicCmdType { get; set; }
        /// <summary>基本命令个性值
        /// 就是将这个命令编译时用的通配符
        /// </summary>
        [DataMember]
        public string Value { get; set; }
        /// <summary>基本命令适配器字符串
        /// 
        /// </summary>
        [DataMember]
        public string Adapter { get; set; }
        /// <summary>通配符替换的最小值
        /// 只有在FXBasicCmdAdapterType.N时才有效
        /// </summary>
        [DataMember]
        public int Min { get; set; }
        /// <summary>通配符替换的最大值
        /// 只有在FXBasicCmdAdapterType.N时才有效
        /// </summary>
        [DataMember]
        public int Max { get; set; }
        /// <summary>基本命令符号
        /// 
        /// </summary>
        [DataMember]
        public string Cmd { get; set; }
    }
    /// <summary>软元件
    /// 
    /// </summary>
    [DataContract]
    public class FXRegister
    {
        private List<FXRegisterMN> mMN;

        /// <summary>软元件名称
        /// 
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>软元件描述
        /// 
        /// </summary>
        [DataMember]
        public string Comment { get; set; }
        /// <summary>软元件编号开始
        /// 不管原来是几进制的，这里一律是10进制
        /// </summary>
        [DataMember]
        public int From { get; set; }
        /// <summary>软元件编号结束
        /// 不管原来是几进制的，这里一律是10进制
        /// </summary>
        [DataMember]
        public int To { get; set; }
        /// <summary>编译时的开始地址
        /// 
        /// </summary>
        [DataMember]
        public int AddrFrom { get; set; }

        [DataMember]
        public int ABSMinAddr
        {
            get { return this.AddrFrom + this.Offset; }
            set{}
        }
        [DataMember]
        public int ABSMaxAddr
        {
            get
            {
                int formatType = (int)this.NameFormatType;
                return (Convert.ToInt32(this.To.ToString(), formatType) - Convert.ToInt32(this.From.ToString(), formatType)) * this.Step + this.AddrFrom + this.Offset;
            }
            set { }
        }
        /// <summary>编译时的结束地址
        /// 
        /// </summary>
        [DataMember]
        public int AddrTo { get; set; }
        /// <summary>地址偏移量
        /// 
        /// </summary>
        [DataMember]
        public int Offset { get; set; }
        /// <summary>软元件编号的进制类型
        /// 
        /// </summary>
        [DataMember]
        public FXRegisterNameFormatType NameFormatType { get; set; }
        /// <summary>软元件占用的字节数，默认为1
        /// 
        /// </summary>
        [DataMember]
        public int Step { get; set; }
        /// <summary>软元件编译时候需要用到的MN值列表
        /// 
        /// </summary>
        public List<FXRegisterMN> MN
        {
            get
            {
                if (this.mMN == null)
                    this.mMN = new List<FXRegisterMN>();
                return this.mMN;
            }
        }
        /// <summary>是否允许强制ON，OFF
        /// 默认为Off，false
        /// </summary>
        [DataMember]
        public bool CanForce { get; set; }
        /// <summary>强制ON/OFF的软件编号开始
        /// 
        /// </summary>
        [DataMember]
        public int ForceFrom { get; set; }
        /// <summary>强制ON/OFF的软件编号结束
        /// 
        /// </summary>
        [DataMember]
        public int ForceTo { get; set; }

        /// <summary>服务器端调用这个方法可以获得指定地址的Register编号
        /// 如果指定的地址不存在，那么返回null
        /// 返回的值按照NameFormatType的值格式化
        /// </summary>
        /// <param name="decimalAddr"></param>
        /// <returns></returns>
        public string GetRegisterNOStr(int decimalAddr)
        {
            string result = null;
            if (decimalAddr >= this.ABSMinAddr && decimalAddr <= this.ABSMaxAddr)       //只有在最大和最小值之间的地址才合法
            {
                int temp = decimalAddr - this.Offset - this.AddrFrom;
                if (temp % this.Step == 0)          //必须整除
                {
                    int formatBase = (int)this.NameFormatType;
                    temp = temp / this.Step + Convert.ToInt32(this.From.ToString(), formatBase);
                    result = Convert.ToString(temp, formatBase);
                }
            }
            return result;
        }
    }

    /// <summary>软元件符号的映射关系
    /// 表示将从from到to范围内的软元件符号映射成新的符号ReName
    /// </summary>
    [DataContract]
    public struct OperandMap
    {
        /// <summary>软元件源名称
        /// 
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>需要映射的软元件编号开始值
        /// 
        /// </summary>
        [DataMember]
        public int From { get; set; }
        /// <summary>需要映射的软元件编号结束值
        /// 
        /// </summary>
        [DataMember]
        public int To { get; set; }
        /// <summary>映射后的软元件名称
        /// 
        /// </summary>
        [DataMember]
        public string ReName { get; set; }
    }

    #region Enum Contracts
    [DataContract]
    public enum FXReadWriteType
    {
        [EnumMember]
        Contact = 0x01,
        [EnumMember]
        Set = 0x02,
        [EnumMember]
        Rst = 0x04,
        [EnumMember]
        Out = 0x08,
        [EnumMember]
        PlsOrPlf = 0x16,
        [EnumMember]
        Current = 0x32
    }

    [DataContract]
    public enum FXSeriesControl
    {
        /// <summary>
        /// Enquery
        /// </summary>
        [EnumMember]
        ENQ = 0x05,
        /// <summary>
        /// Acknowledge
        /// </summary>
        [EnumMember]
        ACK = 0x06,
        /// <summary>
        /// Negative ack
        /// </summary>
        [EnumMember]
        NAK = 0x15,
        /// <summary>
        /// Start of Text
        /// </summary>
        [EnumMember]
        STX = 0x02,
        /// <summary>
        /// End of Text
        /// </summary>
        [EnumMember]
        ETX = 0x03
    }

    /// <summary>
    /// 参数区的参数类型
    /// </summary>
    [DataContract]
    public enum FXParamType
    {
        /// <summary>
        /// 程序总步数
        /// </summary>
        [EnumMember]
        ProgramStepLen = 0,
        /// <summary>
        /// PLC密码
        /// </summary>
        [EnumMember]
        Password = 4,
        /// <summary>
        /// PLC标题
        /// </summary>
        [EnumMember]
        Title = 8,
        /// <summary>
        /// M锁存开始
        /// </summary>
        [EnumMember]
        MLockFrom = 24,
        /// <summary>
        /// M锁存结束
        /// </summary>
        [EnumMember]
        MLockTo = 25,
        /// <summary>
        /// S锁存开始
        /// </summary>
        [EnumMember]
        SLockFrom = 26,
        /// <summary>
        /// S锁存结束
        /// </summary>
        [EnumMember]
        SLockTo = 27,
        /// <summary>
        /// C锁存开始(16位)
        /// </summary>
        [EnumMember]
        C16LockFrom = 28,
        /// <summary>
        /// C锁存结束(16位)
        /// </summary>
        [EnumMember]
        C16LockTo = 29,
        /// <summary>
        /// C锁存开始(32位)
        /// </summary>
        [EnumMember]
        C32LockFrom = 30,
        /// <summary>
        /// C锁存结束(32位)
        /// </summary>
        [EnumMember]
        C32LockTo = 31,
        /// <summary>
        /// D锁存开始
        /// </summary>
        [EnumMember]
        DLockFrom = 32,
        /// <summary>
        /// D锁存结束
        /// </summary>
        [EnumMember]
        DLockTo = 33,
        /// <summary>
        /// 文件寄存器开始地址(这个地址加上0x8000才是物理地址)
        /// </summary>
        [EnumMember]
        FileRegisterFrom = 36,
        /// <summary>
        /// 文件寄存器容量块数(1块=500点=1000字节)
        /// </summary>
        [EnumMember]
        FileRegisterBlockLen = 37,
        /// <summary>
        /// 注释开始地址(这个地址加上0x8000才是物理地址)
        /// </summary>
        [EnumMember]
        CommentRegisterFrom = 38,
        /// <summary>
        /// 注释容量块数(1块=50点=1000字节)
        /// </summary>
        [EnumMember]
        CommentRegisterBlockLen = 39
    }

    /// <summary>
    /// 特殊寄存器类型
    /// </summary>
    [DataContract]
    public enum FXSpecialType
    {
        [EnumMember]
        MoniterTimer = 0,
        [EnumMember]
        PLCModelvsVersion = 1,
        [EnumMember]
        Ram = 2,
        [EnumMember]
        RamType = 3,
        [EnumMember]
        MsWhenError = 4,
        [EnumMember]
        BatteryVoltage = 5,
        [EnumMember]
        DetectBatteryLow = 6,
        [EnumMember]
        InstantaneousStopDetectCount = 7,
        [EnumMember]
        PowerCutDetectCount = 8,
        [EnumMember]
        DC24ErrorNO = 9,
        [EnumMember]
        CurrentScanValue = 10,
        [EnumMember]
        MinScanInterval = 11,
        [EnumMember]
        MaxScanInterval = 12,
        [EnumMember]
        Second = 13,
        [EnumMember]
        Minute = 14,
        [EnumMember]
        Hour = 15,
        [EnumMember]
        Day = 16,
        [EnumMember]
        Month = 17,
        [EnumMember]
        Year = 18,
        [EnumMember]
        Week = 19,
        [EnumMember]
        InputFilterSet1 = 20,
        [EnumMember]
        InputFilterSet2 = 21,
        [EnumMember]
        Z0Value = 28,
        [EnumMember]
        V0Value = 29,
        [EnumMember]
        No1AnalogValue = 30,
        [EnumMember]
        No2AnalogValue = 31,
        [EnumMember]
        ConstScanTime = 39,
        [EnumMember]
        STLStatus1 = 40,
        [EnumMember]
        STLStatus2 = 41,
        [EnumMember]
        STLStatus3 = 42,
        [EnumMember]
        STLStatus4 = 43,
        [EnumMember]
        STLStatus5 = 44,
        [EnumMember]
        STLStatus6 = 45,
        [EnumMember]
        STLStatus7 = 46,
        [EnumMember]
        STLStatus8 = 47,
        [EnumMember]
        MinActivateStatus = 49,
        [EnumMember]
        IOConfigError = 60,
        [EnumMember]
        PLCHardwareError1 = 61,
        [EnumMember]
        PLCHardwareError2 = 62,
        [EnumMember]
        SerialCommunicateError = 63,
        [EnumMember]
        ParamError = 64,
        [EnumMember]
        GrammerError = 65,
        [EnumMember]
        CircuitError = 66,
        [EnumMember]
        OperatorError = 67,
        [EnumMember]
        OperatorErrorStep = 68,
        [EnumMember]
        ErrorStepM8065ToM8067 = 69,
        [EnumMember]
        ParallelLinkErrorTime = 70,
        [EnumMember]
        LeftSampleCount = 74,
        [EnumMember]
        SetSampleCount = 75,
        [EnumMember]
        SamplePeriod = 76,
        [EnumMember]
        TrigSpecify = 77,
        [EnumMember]
        SetTrigCondition = 78,
        [EnumMember]
        SamplePointer = 79,
        [EnumMember]
        BitDeviceNo0 = 80,
        [EnumMember]
        BitDeviceNo1 = 81,
        [EnumMember]
        BitDeviceNo2 = 82,
        [EnumMember]
        BitDeviceNo3 = 83,
        [EnumMember]
        BitDeviceNo4 = 84,
        [EnumMember]
        BitDeviceNo5 = 85,
        [EnumMember]
        BitDeviceNo6 = 86,
        [EnumMember]
        BitDeviceNo7 = 87,
        [EnumMember]
        BitDeviceNo8 = 88,
        [EnumMember]
        BitDeviceNo9 = 89,
        [EnumMember]
        BitDeviceNo10 = 90,
        [EnumMember]
        BitDeviceNo11 = 91,
        [EnumMember]
        BitDeviceNo12 = 92,
        [EnumMember]
        BitDeviceNo13 = 93,
        [EnumMember]
        BitDeviceNo14 = 94,
        [EnumMember]
        BitDeviceNo15 = 95,
        [EnumMember]
        ByteDeviceNo0 = 96,
        [EnumMember]
        ByteDeviceNo1 = 97,
        [EnumMember]
        ByteDeviceNo2 = 98,
        [EnumMember]
        HighSpeedLoopCounter = 99,
        [EnumMember]
        SequencevsVersion = 101,
        [EnumMember]
        FuncExStoreType = 104,
        [EnumMember]
        FuncExStoreVersion = 105,
        [EnumMember]
        CommentCount = 107,
        [EnumMember]
        SpecialModelCount = 108,
        [EnumMember]
        OutputRefreshError = 109,
        [EnumMember]
        Ch1DigitValue = 112,
        [EnumMember]
        Ch2DigitValue = 113,
        [EnumMember]
        Ch3DigitValue = 114
        //[EnumMember]
        //SetCommunicateType = 120
    }

    /// <summary>PLC信息区域类型
    /// 
    /// </summary>
    [DataContract]
    public enum FXRegionType
    {
        [EnumMember]
        Param,
        [EnumMember]
        Special
    }

    /// <summary>PLC运行状态类型
    /// 
    /// </summary>
    [DataContract]
    public enum FXStatus
    {
        [EnumMember]
        Run = 0x09,
        [EnumMember]
        Stop = 0x0A
    }

    /// <summary>
    /// FX系列PLC命令类型
    /// </summary>
    [DataContract]
    public enum FXCmdType
    {
        /// <summary>
        /// 基本型命令
        /// </summary>
        [EnumMember]
        Basic,
        /// <summary>
        ///  应用型命令
        /// </summary>
        [EnumMember]
        Application
    }

    /// <summary>应用命令类型
    /// 
    /// </summary>
    [DataContract]
    public enum FXAppCmdType
    {
        /// <summary>普通应用命令
        /// 
        /// </summary>
        [EnumMember]
        Normal,
        /// <summary>D指令
        /// 
        /// </summary>
        [EnumMember]
        D,
        /// <summary>P指令
        /// 
        /// </summary>
        [EnumMember]
        P,
        /// <summary>DP指令
        /// 
        /// </summary>
        [EnumMember]
        DP
    }

    /// <summary>
    /// 寄存器名字中数字的格式
    /// </summary>
    [DataContract]
    public enum FXRegisterNameFormatType
    {
        [EnumMember]
        Oct = 8,
        [EnumMember]
        Dec = 10,
        [EnumMember]
        Hex = 16
    }

    /// <summary>
    ///  FX系列PLC基本型命令种类
    /// </summary>
    [DataContract]
    public enum FXBasicCmdType
    {
        /// <summary>
        /// 纯单字
        /// </summary>
        [EnumMember]
        PureSingle = 0,
        /// <summary>
        /// 单字
        /// </summary>
        [EnumMember]
        Single,
        /// <summary>
        /// 双字
        /// </summary>
        [EnumMember]
        Double,
        /// <summary>
        /// 三字
        /// </summary>
        [EnumMember]
        Triple,
        /// <summary>
        /// 五字
        /// </summary>
        [EnumMember]
        Five
    }
    /// <summary>
    /// FX系列PLC基本指令的操作数适配类型
    /// </summary>
    [DataContract]
    public enum FXBasicCmdAdapterType
    {
        /// <summary>
        ///  排除指定的元素
        /// </summary>
        [EnumMember]
        Exclude = 0,
        /// <summary>
        ///  只能应用于指定的元素
        /// </summary>
        [EnumMember]
        Only,
        /// <summary>
        ///  指定的元素必须同时满足
        /// </summary>
        [EnumMember]
        With,
        /// <summary>
        /// 需要N
        /// </summary>
        [EnumMember]
        N,
        /// <summary>
        /// 不需要Adapter
        /// </summary>
        [EnumMember]
        NULL
    }
    #endregion

    #region FXRegionInfo及其子类的工厂方法
    public class FXRegionInfoFactory
    {
        /// <summary>使用这个方法获得特定FXRegionInfo的实例
        /// 
        /// </summary>
        /// <param name="fxModel">PLC的型号</param>
        /// <param name="type">区域类型：参数区域或者特殊区域</param>
        /// <param name="typeInside">参数区域或者特殊区域内部类型由FXParamType或者FXSpecialType指定</param>
        /// <returns></returns>
        public static FXRegionInfo CreateFXRegionInfo(string fxModel,FXRegionType type,int typeInside,int len)
        {
            if(type == FXRegionType.Param)
            {
                FXParamType paramType = (FXParamType)typeInside;
                if (paramType == FXParamType.Title)           //如果是PLC标题，那么返回string类型的区域信息数据
                    return new FXStringRegionInfo(len);
                else if (paramType == FXParamType.MLockFrom || paramType == FXParamType.MLockTo)
                    return new FXRegisterRegionInfo(fxModel, "M");
                else if (paramType == FXParamType.SLockFrom || paramType == FXParamType.SLockTo)
                    return new FXRegisterRegionInfo(fxModel, "S");
                else if (paramType == FXParamType.C16LockFrom || paramType == FXParamType.C16LockTo)
                    return new FXRegisterRegionInfo(fxModel, "C");
                else if (paramType == FXParamType.C32LockFrom || paramType == FXParamType.C32LockTo)
                    return new FXRegisterRegionInfo(fxModel, "Cp");
                else if (paramType == FXParamType.DLockFrom || paramType == FXParamType.DLockTo)
                    return new FXRegisterRegionInfo(fxModel, "D");
                else
                    return new FXRegionInfo();
            }
            return null;
        }
    }
    #endregion
}
