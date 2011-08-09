using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;
using CG.Data.DataSource;
using System.IO;

// /client:"WcfTestClient.exe"
namespace CG.Communication.PLC.MITSUBISH.Service
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Single,
        InstanceContextMode = InstanceContextMode.Single)]
    public class FXService : IFXService, IDisposable
    {
        private FXRS232 mSerialPort;// = new FXRS232(FXPLCServiceConfig.Default.PortName);                //通讯的串口
        private const int MAXCMDLEN = 0x40;                 //最大的命令内容长度
        private const int REGIONOFFSET = 0x8000;            //注释区和用户文件区的地址偏移量
        private const int COMMENTREADLEN = 20;              //注释每次读取的长度
        private readonly byte[] STARTPLCCMD = { 0x02, 0x45, 0x38, 0x32, 0x35, 0x30, 0x45, 0x03, 0x35, 0x43 };          //启动PLC命令(暂时直接写死)
        private readonly byte[] STOPPLCCMD = { 0x02, 0x45, 0x37, 0x32, 0x35, 0x30, 0x45, 0x03, 0x35, 0x42 };           //停止PLC命令(暂时直接写死)
        private readonly byte[] READPLCSTATUSCMD = { 0x02, 0x30, 0x30, 0x31, 0x45, 0x30, 0x30, 0x31, 0x03, 0x36, 0x41 };       //读取PLC运行状态命令(暂时直接写死)
        private static string mFXModel = "";                        //这个实例对应的PLC型号
        //private static StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\aaaaaaa.txt", true);

        public FXService()
        {
            
        }

        #region 通讯接口部分

        #region 功能部分
        public FXComplieResult Complie(string sourceCode)          //编译源码
        {
            return this.ComplieInside(sourceCode);
        }
        public FXDeComplieResult Decompile(string readSourceCode)   //反编译读取出的源码
        {
            return DecomplieInside(readSourceCode);
        }
        public FXENQResult ENQ()              //发送一个ENQ命令
        {
            FXENQResult result = new FXENQResult();
            result.Result = 0x00;
            try
            {
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
                byte[] reply = this.mSerialPort.Read("05");
                if (reply.Length == 1)
                    result.Result = reply[0];
                else
                    result.Msg = "reply length error.";
                this.mSerialPort.Close();
            }
            catch (System.Exception ex)
            {
                result.Msg = ex.Message;
            }
            return result;
        }
        public string OpenPort()        //开启串口
        {
            string result = null;
            try
            {
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string ClosePort()       //关闭串口
        {
            string result = null;
            try
            {
                if (this.mSerialPort.IsOpen)
                    this.mSerialPort.Close();
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public string Connect(string fxModel,string comStr)         //加载配置文件，并且测试设备在线情况(这个方法必须在第一次调用的时候调用，否则配置文件无法加载)
        {
            string result = null;
            try
            {
                if (this.mSerialPort != null)
                    this.ClosePort();
                this.mSerialPort = new FXRS232(comStr);
                string path = AppDomain.CurrentDomain.BaseDirectory + "FXSeriesConfig.xml";
                FXConfigReader.LoadFXConfigFile(path);
                bool isDeviceOnline = this.mSerialPort.IsDeviceOnLine;
                if (FXConfigReader.IsFXModelExist(fxModel))
                    mFXModel = fxModel;
                else
                    result = "model not exist.";
                if (!isDeviceOnline)
                    result = "device not on line.";
            }
            catch (System.Exception ex)
            {
                result = "连接失败!请查看配置文件是否存在并保证设备在线";
            }
            return result;
        }
        public string DisConnect()      //清除配置文件，如果串口打开，那么关闭串口
        {
            string result = null;
            mFXModel = "";
            try
            {
                FXConfigReader.UnLoadFXConfigFile();
                if (this.mSerialPort.IsOpen)
                    this.mSerialPort.Close();
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        #endregion

        #region 读取部分
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <returns></returns>
        public FXRegionInfoResult ReadParamRegion()
        {
            return ReadParamRegionInside();
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <returns></returns>
        public FXReadCodeResult ReadCode()
        {
            return ReadCodeInside();
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <returns></returns>
        public FXDeComplieResult ReadDecompiledCode()
        {
            FXDeComplieResult result = new FXDeComplieResult();
            FXReadCodeResult readResult = ReadCodeInside();
            result.Msg = readResult.Msg;
            if (result.Msg == null)
            {
                result = DecomplieInside(readResult.Code);
            }
            return result;
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <returns></returns>
        public FXReadCommentResult ReadComment()
        {
            return ReadCommentInside();
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <returns></returns>
        public FXReadStatusResult ReadPLCStatus()
        {
            return ReadPLCStatusInside();
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，但不负责在执行结束时关闭，所以需要手动调用关闭串口命令
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public FXRegisterReadResult ReadRegisters(List<FXRegisterReadInfo> infos)
        {
            return this.ReadRegistersInside(infos);
        }
        #endregion

        #region 写入部分
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public string WriteComment(FXCommentInfo[] infos)
        {
            FXReadStatusResult status = ReadPLCStatus();
            if (status.Msg == null)
            {
                if (status.Status == FXStatus.Run)
                    RemoteControl(FXStatus.Stop);
                string msg = this.WriteCommentInside(infos);
                if (status.Status == FXStatus.Run)
                    RemoteControl(FXStatus.Run);
                return msg;
            }
            else
                return status.Msg;
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <param name="codeStr"></param>
        /// <returns></returns>
        public string WriteCode(string codeStr)
        {
            FXReadStatusResult status = ReadPLCStatus();
            if (status.Msg == null)
            {
                if (status.Status == FXStatus.Run)
                    RemoteControl(FXStatus.Stop);
                string msg = this.WriteCodeInside(codeStr);
                if (status.Status == FXStatus.Run)
                    RemoteControl(FXStatus.Run);
                return msg;
            }
            else
                return status.Msg;
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string WriteCodeInFile(string path)
        {
            string contentStr = File.ReadAllText(path);
            return this.WriteCodeInside(contentStr);
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public string WriteParamRegion(FXRegionInfo[] infos)
        {
            return this.WriteParamRegionInside(infos);
        }
        #endregion

        #region 调试部分
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public FXRemoteControlResult RemoteControl(FXStatus status)
        {
            byte[] replyBytes;
            FXRemoteControlResult result = new FXRemoteControlResult();
            try
            {
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
                if (status == FXStatus.Run)
                    replyBytes = this.mSerialPort.Read(STARTPLCCMD);
                else
                    replyBytes = this.mSerialPort.Read(STOPPLCCMD);
                this.mSerialPort.Close();
                if (replyBytes[0] == (byte)FXSeriesControl.ACK)
                    result.IsSuccess = true;
                else
                    result.IsSuccess = false;
            }
            catch (System.Exception ex)
            {
                result.Msg = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <returns></returns>
        public string ForceOn(FXRegisterBasicInfo info)
        {
            string result = null;
            try
            {
                byte[] cmd = info.GetForceOnOFFCmd(mFXModel, true);
                if (cmd != null)
                {
                    if (!this.mSerialPort.IsOpen)
                        this.mSerialPort.Open();
                    byte[] replyBytes = this.mSerialPort.Read(cmd);
                    this.mSerialPort.Close();
                    if (replyBytes[0] != (byte)FXSeriesControl.ACK)
                        result = "PLC Reply Fail";
                }
                else
                    result = "Can Not Force " + info.Register + info.NO + " ON";
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <returns></returns>
        public string ForceOff(FXRegisterBasicInfo info)
        {
            string result = null;
            try
            {
                byte[] cmd = info.GetForceOnOFFCmd(mFXModel, false);
                if (cmd != null)
                {
                    if (!this.mSerialPort.IsOpen)
                        this.mSerialPort.Open();
                    byte[] replyBytes = this.mSerialPort.Read(cmd);
                    this.mSerialPort.Close();
                    if (replyBytes[0] != (byte)FXSeriesControl.ACK)
                        result = "PLC Reply Fail";
                }
                else
                    result = "Can Not Force " + info.Register + info.NO + " OFF";
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 这个方法在执行开始前打开串口，并负责在执行后关闭
        /// </summary>
        /// <param name="info"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string WriteRegister(FXRegisterBasicInfo info, byte[] value)
        {
            string result = null;
            try
            {
                byte[] cmd = info.GetWriteRegisterCmd(mFXModel, value);
                byte[] cmd2 = info.GetWriteRegisterCmd(mFXModel, -111);
                if (cmd != null)
                {
                    if (!this.mSerialPort.IsOpen)
                        this.mSerialPort.Open();
                    byte[] replyBytes = this.mSerialPort.Read(cmd);
                    this.mSerialPort.Close();
                    if (replyBytes[0] != (byte)FXSeriesControl.ACK)
                        result = "PLC Reply Fail";
                }
                else
                    result = "Can Not Write " + info.Register + info.NO;
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        #endregion

        #region 获得配置部分
        public Dictionary<string, FXBasicCmd> GetBasicCmdConfig()
        {
            Dictionary<string, FXBasicCmd> result = new Dictionary<string, FXBasicCmd>();
            Dictionary<string, IFXCmd> temp = FXConfigReader.Cmds[mFXModel];
            foreach (KeyValuePair<string, IFXCmd> pair in temp)
            {
                if (pair.Value.GetType() == typeof(FXBasicCmd))
                    result.Add(pair.Key, (FXBasicCmd)pair.Value);
            }
            return result;
        }

        public Dictionary<string, FXAppCmd> GetAppCmdConfig()
        {
            Dictionary<string, FXAppCmd> result = new Dictionary<string, FXAppCmd>();
            Dictionary<string, IFXCmd> temp = FXConfigReader.Cmds[mFXModel];
            foreach (KeyValuePair<string, IFXCmd> pair in temp)
            {
                if (pair.Value.GetType() == typeof(FXAppCmd))
                    result.Add(pair.Key, (FXAppCmd)pair.Value);
            }
            return result;
        }

        public Dictionary<string, FXRegister> GetRegisterConfig()
        {
            return FXConfigReader.Registers[mFXModel];
        }

        public List<OperandMap> GetRegisterMappingConfig()
        {
            return FXConfigReader.OperandMaps[mFXModel];
        }
        #endregion

        #endregion

        #region 功能部分
        private List<byte[]> CreateRegionCMDList(FXSeriesCmd cmd, int from, int len, string[] dataStrArray)
        {
            List<byte[]> cmdList = new List<byte[]>();
            byte[] cmdBytes;
            int dataStrArrayIndex = 0;
            while (len > MAXCMDLEN)
            {
                len = len - MAXCMDLEN;
                Console.WriteLine(len);
                string[] tempMaxLen = null;
                if (dataStrArray != null)
                {
                    tempMaxLen = new string[MAXCMDLEN];
                    Array.Copy(dataStrArray, dataStrArrayIndex, tempMaxLen, 0, MAXCMDLEN);
                }
                cmdBytes = FXCommander1.CreateCommand(FXSeriesControl.STX, cmd, from, MAXCMDLEN, tempMaxLen);
                from += MAXCMDLEN;
                dataStrArrayIndex += MAXCMDLEN;
                cmdList.Add(cmdBytes);
            }
            if (len > 0)
            {
                string[] tempLeft = null;
                if (dataStrArray != null)
                {
                    tempLeft = new string[len];
                    Array.Copy(dataStrArray, dataStrArrayIndex, tempLeft, 0, len);
                }
                cmdBytes = FXCommander1.CreateCommand(FXSeriesControl.STX, cmd, from, len, tempLeft);
                cmdList.Add(cmdBytes);
            }
            return cmdList;
        }
        private FXComplieResult ComplieInside(string codeStr)
        {
            FXComplieResult result = new FXComplieResult();
            if (FXConfigReader.IsFXModelExist(mFXModel))
            {
                try
                {
                    List<SourceCodeLine> lineList = FXComplier.ConvertSourceToLineList(codeStr);         //将源代码解析成行集合
                    //编译源代码行集合
                    string compliedStr = FXComplier.ComplieCmdStr(lineList, mFXModel);
                    result.ComplieStr = compliedStr;
                }
                catch (System.Exception ex)
                {
                    result.Msg = ex.Message;
                }
            }
            else
            {
                result.Msg = "specified PLC model not exists";
            }
            return result;
        }
        private FXDeComplieResult DecomplieInside(string readSourceCode)
        {
            FXDeComplieResult result = new FXDeComplieResult();
            try
            {
                result.DecomplieStr = FXDecomplier.DecomplieCmdCode(readSourceCode, mFXModel);
            }
            catch (System.Exception ex)
            {
                result.Msg = ex.Message;
            }
            return result;
        }
        private FXRegionInfoResult ReadParamRegionInside()
        {
            FXRegionInfoResult result = new FXRegionInfoResult();
            try
            {
                FXRegionInfoList regionInfoList = FXConfigReader.ParamRegionInfos[mFXModel];
                List<byte[]> cmdList = this.CreateRegionCMDList(FXSeriesCmd.ReadProgram, regionInfoList.From, regionInfoList.Len, null);        //生成读取参数区的命令集
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
                List<byte[]> readResultList = this.mSerialPort.Read(cmdList);
                this.mSerialPort.Close();
                byte[] readResult = Utils.CombineArrays(readResultList);
                Array enumValues = Enum.GetValues(typeof(FXParamType));         //遍历FXParamType中所有的枚举值
                foreach (int value in enumValues)
                {
                    if (regionInfoList.Items.ContainsKey(value))         //如果枚举值代表的类型在配置文件中存在，那么从获得的数据中解析出对应的值
                    {
                        FXRegionInfoItem item = regionInfoList.Items[value];
                        FXRegionInfo info = FXRegionInfoFactory.CreateFXRegionInfo(mFXModel, FXRegionType.Param, value, item.Len);
                        info.Name = item.Name;
                        info.Type = value;
                        info.Value = new byte[item.Len * 2];
                        Array.Copy(readResult, item.Offset * 2, info.Value, 0, item.Len * 2);
                        result.Items.Add(value, info);
                    }
                }
            }
            catch (System.Exception ex)
            {
                result.Msg = ex.Message;
            }
            return result;
        }
        private string WriteParamRegionInside(FXRegionInfo[] infos)
        {
            List<byte[]> cmdList = new List<byte[]>();
            FXRegionInfoList infoList = FXConfigReader.ParamRegionInfos[mFXModel];
            try
            {
                foreach (FXRegionInfo info in infos)
                {
                    if (infoList.Items.ContainsKey(info.Type))
                    {
                        FXRegionInfoItem item = infoList.Items[info.Type];
                        info.SetClientValue();
                        byte[] cmdBytes = FXCommander1.CreateCommand(FXSeriesControl.STX, FXSeriesCmd.WriteProgram, infoList.From + item.Offset, item.Len, info.GetRegionValueStrArray());
                        cmdList.Add(cmdBytes);
                    }
                }
                List<byte[]> readResultList;
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
                readResultList = this.mSerialPort.Read(cmdList);
                this.mSerialPort.Close();
                return null;
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
        }
        private FXReadCodeResult ReadCodeInside()
        {
            FXReadCodeResult codeResult = new FXReadCodeResult();
            codeResult.Code = "";
            try
            {
                int programFrom = FXConfigReader.ProgramRegionFrom[mFXModel];
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
                int indexOfEnd = -1;
                int lastIndex = 0;
                List<byte[]> cmdList;
                List<byte[]> readResultList;
                while (indexOfEnd == -1)       //如果没找到000F，那么接着找
                {
                    lastIndex = codeResult.Code.Length - 2 > 0 ? codeResult.Code.Length - 2 : 0;
                    cmdList = this.CreateRegionCMDList(FXSeriesCmd.ReadProgram, programFrom, MAXCMDLEN, null);         //每次读取40H长度，直到读到End(000F)
                    readResultList = this.mSerialPort.Read(cmdList);
                    if (readResultList[0][0] == (byte)FXSeriesControl.NAK)
                    {
                        codeResult.Code = null;
                        codeResult.Msg = "device return NAK";
                        break;
                    }
                    codeResult.Code = codeResult.Code + Utils.BytesToStrInRightOrder(true, readResultList[0]);
                    indexOfEnd = codeResult.Code.IndexOf("000F", lastIndex, StringComparison.CurrentCultureIgnoreCase);
                    programFrom += MAXCMDLEN;
                }
                this.mSerialPort.Close();
                codeResult.Code = codeResult.Code.Substring(0, indexOfEnd + 4);
            }
            catch (System.Exception ex)
            {
                codeResult.Msg = ex.Message;
            }
            return codeResult;
        }
        private string WriteCodeInside(string codeStr)
        {
            FXComplieResult complieResult = this.ComplieInside(codeStr);
            if (complieResult.Msg == null)              //如果编译成功
            {
                try
                {
                    int programFrom = FXConfigReader.ProgramRegionFrom[mFXModel];
                    string[] dataStrArray = complieResult.GetComplieStrArray();
                    List<byte[]> cmdList = this.CreateRegionCMDList(FXSeriesCmd.WriteProgram, programFrom, dataStrArray.Length, dataStrArray);
                    List<byte[]> readResultList;
                    if (!this.mSerialPort.IsOpen)
                        this.mSerialPort.Open();
                    readResultList = this.mSerialPort.Read(cmdList);
                    this.mSerialPort.Close();
                    return null;
                }
                catch (System.Exception ex)
                {
                    return ex.Message;
                }
            }
            else
                return complieResult.Msg;
        }
        private FXReadCommentResult ReadCommentInside()
        {
            FXReadCommentResult result = new FXReadCommentResult();
            try
            {
                FXRegionInfoResult regionInfo = ReadParamRegionInside();
                FXRegionInfo infoAddr = regionInfo.Items[(int)FXParamType.CommentRegisterFrom];
                FXRegionInfo infoLen = regionInfo.Items[(int)FXParamType.CommentRegisterBlockLen];
                int commentAddr = Convert.ToInt32(infoAddr.ValueStr, 16) + 0x8000;              //获得注释起始地址
                int commentLen = Convert.ToInt32(infoLen.ValueStr, 16) * 1000;                         //一块是1000字节
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
                int commentOffset = 0;
                List<byte[]> cmdList;
                List<byte[]> readResultList;
                while (true)
                {
                    cmdList = this.CreateRegionCMDList(FXSeriesCmd.ReadProgram, commentAddr + commentOffset, COMMENTREADLEN, null);         //每次读取20长度，直到读到第一个就是00结束
                    readResultList = this.mSerialPort.Read(cmdList);
                    string aaa = BitConverter.ToString(readResultList[0]);
                    if (readResultList[0][0] == (byte)FXSeriesControl.NAK)          //如果设备返回NAK，那直接返回
                    {
                        result.Msg = "device return NAK";
                        break;
                    }
                    string str = Utils.BytesToStrInRightOrder(false, readResultList[0]);
                    if (str.StartsWith("00"))               //如果读到的字符串以00开头，说明没有注释了，可以直接返回
                        break;
                    FXCommentInfo comment = new FXCommentInfo();        //添加这个注释
                    comment.SetComment(str);
                    result.Comments.Add(comment);
                    commentOffset += COMMENTREADLEN;
                    if (commentOffset + COMMENTREADLEN > commentLen)          //如果已经超过注释区大小了，那么可以直接返回
                        break;
                }
                this.mSerialPort.Close();
            }
            catch (System.Exception ex)
            {
                result.Msg = ex.Message;
            }
            return result;
        }
        private string WriteCommentInside(FXCommentInfo[] infos)
        {
            string result = null;
            try
            {
                FXRegionInfoResult regionInfo = ReadParamRegionInside();
                FXRegionInfo infoAddr = regionInfo.Items[(int)FXParamType.CommentRegisterFrom];
                FXRegionInfo infoLen = regionInfo.Items[(int)FXParamType.CommentRegisterBlockLen];
                int commentAddr = Convert.ToInt32(infoAddr.ValueStr, 16) + 0x8000;              //获得注释起始地址
                int commentLen = Convert.ToInt32(infoLen.ValueStr, 16) * 1000;                         //一块是1000字节
                string[] dataStrArray = new string[commentLen / 2];
                for (int i = 0; i < dataStrArray.Length; i++)                 //初始化注释区
                    dataStrArray[i] = "00";
                if (infos != null)
                {
                    for (int i = 0; i < infos.Length; i++)                      //在注释区中添加注释
                    {
                        string[] temp = infos[i].GetCommentStrArray();
                        Array.Copy(temp, 0, dataStrArray, i * FXCommentInfo.CommentBytesLen, temp.Length);
                    }
                }
                List<byte[]> cmdList = CreateRegionCMDList(FXSeriesCmd.WriteProgram, commentAddr, dataStrArray.Length, dataStrArray);        //生成写入注释命令
                List<byte[]> readResultList;
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
                readResultList = this.mSerialPort.Read(cmdList);
                this.mSerialPort.Close();
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        private FXReadStatusResult ReadPLCStatusInside()
        {
            FXReadStatusResult result = new FXReadStatusResult();
            try
            {
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
                byte[] replyBytes = this.mSerialPort.Read(READPLCSTATUSCMD);
                this.mSerialPort.Close();
                string replyStr = Utils.BytesToStrInRightOrder(false, replyBytes);
                int replyInt = Convert.ToInt32(replyStr, 16);
                if (replyInt == (int)FXSeriesControl.NAK)
                    throw new Exception("device return NAK.");
                else if (replyInt == (int)FXStatus.Run)
                    result.Status = FXStatus.Run;
                else if (replyInt == (int)FXStatus.Stop)
                    result.Status = FXStatus.Stop;
                else
                    throw new Exception("device reply is unexpected.");
            }
            catch (System.Exception ex)
            {
                result.Msg = ex.Message;
            }
            return result;
        }
        public FXRegisterReadResult ReadRegistersInside(List<FXRegisterReadInfo> infos)
        {
            FXRegisterReadResult result = new FXRegisterReadResult();
            result.Values = new Dictionary<string, byte[]>();
            try
            {
                if (!this.mSerialPort.IsOpen)
                    this.mSerialPort.Open();
                List<byte[]> cmdList = new List<byte[]>();
                foreach (FXRegisterReadInfo info in infos)
                {
                    cmdList.Add(info.GetReadRegisterCmd(mFXModel));
                    result.Values.Add(info.Register + info.NO, null);
                }
                List<byte[]> readResultList = this.mSerialPort.Read(cmdList);
                for (int i = 0; i < infos.Count; i++)
                {
                    result.Values[infos[i].Register + infos[i].NO] = infos[i].GetValueBytes(readResultList[i]);
                }
                //this.mSerialPort.Close();
            }
            catch (System.Exception ex)
            {
                result.Msg = ex.Message;
            }
            return result;
        }
        #endregion

        public void Dispose()
        {
            this.DisConnect();
        }
    }
}
