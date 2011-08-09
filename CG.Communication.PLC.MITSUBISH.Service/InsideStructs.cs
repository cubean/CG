using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CG.Data.DataSource;

namespace CG.Communication.PLC.MITSUBISH.Service
{
    /// <summary>软元件作为操作数被应用指令调用时的MN参数
    /// 
    /// </summary>
    public struct FXRegisterMN
    {
        /// <summary>软元件的开始编号
        /// 
        /// </summary>
        public int From { get; set; }
        /// <summary>软元件的结束编号
        /// 
        /// </summary>
        public int To { get; set; }
        /// <summary>M
        /// 
        /// </summary>
        public int M { get; set; }
        /// <summary>N
        /// 
        /// </summary>
        public int N { get; set; }
        /// <summary>是否时位元
        /// 
        /// </summary>
        public bool IsPoint { get; set; }
        /// <summary>偏移量
        /// 
        /// </summary>
        public int Offset { get; set; }
        /// <summary>步长
        /// 
        /// </summary>
        public int Step { get; set; }
    }

    /// <summary>操作数细节
    /// 
    /// </summary>
    public struct OperandDetails
    {
        /// <summary>操作数(软元件)名称
        /// 比如：D123中的D
        /// </summary>
        public string Name { get; set; }
        /// <summary>操作数(软元件)的编号
        /// 比如：D123中的123
        /// </summary>
        public string Value { get; set; }
        /// <summary>操作数的比特数
        /// K4M100之类的组合操作数，这时候类型依然为M，编号依然为100，但OperandDetails.Bits=4*4
        /// </summary>
        public int Bits { get; set; }
    }

    /// <summary>一行源代码
    /// 
    /// </summary>
    public struct SourceCodeLine
    {
        private List<string> mOperands;

        /// <summary>源代码的步号
        /// 
        /// </summary>
        public int Index { get; set; }
        /// <summary>源代码行中的命令符号
        /// 
        /// </summary>
        public string Cmd { get; set; }
        /// <summary>源代码中的操作数列表
        /// 
        /// </summary>
        public List<string> Operands
        {
            get 
            {
                if (this.mOperands == null)
                    this.mOperands = new List<string>();
                return this.mOperands;
            }
        }

        public string BasicLineStr { get; set; }
    }

    /// <summary>区域信息项列表
    /// 
    /// </summary>
    public struct FXRegionInfoList
    {
        private Dictionary<int,FXRegionInfoItem> mItems;

        public int From { get; set; }

        public int Len { get; set; }

        public bool IsLoaded { get; set; }

        public DateTime LoadTime { get; set; }

        public Dictionary<int,FXRegionInfoItem> Items
        {
            get 
            { 
                if(this.mItems==null)
                    this.mItems = new Dictionary<int,FXRegionInfoItem>();
                return this.mItems; 
            }
        }
    }

    /// <summary>区域信息项
    /// 
    /// </summary>
    public struct FXRegionInfoItem
    {
        /// <summary>信息项名称
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>信息项类型编号
        /// 
        /// </summary>
        public int TypeValue { get; set; }
        /// <summary>本信息项地址相对于信息区域起始地址的偏移量
        /// 
        /// </summary>
        public int Offset { get; set; }
        /// <summary>本信息项所占的字节长度
        /// 
        /// </summary>
        public int Len { get; set; }
        /// <summary>本信息项的值
        /// 
        /// </summary>
        public byte[] Value { get; set; }
    }

    
    /// <summary>配置文件中RegisterReadAddrList标签中的某条信息
    /// 
    /// </summary>
    public class FXRegisterReadAddrItem
    {
        private int mValue = -1;

        /// <summary>是否是输出项
        /// 
        /// </summary>
        public bool IsOutputItem { get; set; }
        /// <summary>这个条目的类型
        /// 如果IsOutputItem为false,那么本项可以强制转换成FXRegisterReadAddrType
        /// 如果IsOutputItem为true,那么本项可以强制转换成FXRegisterReadAddrOutputType
        /// </summary>
        public int Type { get; set; }
        /// <summary>寄存器符号
        /// 
        /// </summary>
        public string Register { get; set; }
        /// <summary>寄存器占用的bit数
        /// 
        /// </summary>
        public int Bitmask { get; set; }
        /// <summary>开始物理地址
        /// 
        /// </summary>
        public int From { get; set; }
        /// <summary>寄存器编号的进制
        /// 
        /// </summary>
        public FXRegisterNameFormatType NameFormatType { get; set; }
        /// <summary>这一项的具体值
        /// 这个成员只有在FXRegisterReadWriteValues中才会用到
        /// </summary>
        public int Value
        {
            get { return this.mValue; }
            set { this.mValue = value; }
        }
        /// <summary>物理地址长度(几个字节)
        /// 
        /// </summary>
        public int PhycalLen
        {
            get
            {
                int len = this.Bitmask / 8;
                return len > 0 ? len : 1;
            }
        }

        /// <summary>物理起始地址
        /// 
        /// </summary>
        public int GetPhycalAddr(int registerNO,string fxModel)
        {
            FXRegister tempRegister = FXConfigReader.Registers[fxModel][this.Register];
            registerNO = Convert.ToInt32(registerNO.ToString(), (int)this.NameFormatType) - tempRegister.From;
            return this.From + registerNO * this.Bitmask / 8;
        }

    }
}
