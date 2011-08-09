using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace CG.Communication.PLC.MITSUBISH.Service
{
    public enum FXSeriesCmd
    {
        Read,
        Write,
        ON,
        OFF,
        ReadConfig,
        WriteConfig,
        ReadProgram,
        WriteProgram
    }

    /// <summary>
    /// 读取时寄存器的实际地址类型
    /// </summary>
    public enum FXRegisterReadAddrType
    {
        /// <summary>
        /// 接点
        /// </summary>
        Contact = 0,
        /// <summary>
        /// 输出
        /// </summary>
        Output = 1,
        /// <summary>
        /// 现在值
        /// </summary>
        Current =2
    }

    /// <summary>
    /// 读取输出线圈时寄存器的实际地址类型
    /// </summary>
    public enum FXRegisterReadAddrOutputType
    {
        SET = 0,
        RST = 1,
        OUT = 2,
        PLSorPLF = 3
    }

    /// <summary>
    /// 每一个寄存器所占的比特数
    /// </summary>
    public enum FXRegisterBitmaskType
    {
        bit1 = 1,
        bit4 =4,
        bit16 =16,
        bit32 =32
    }
       
}
