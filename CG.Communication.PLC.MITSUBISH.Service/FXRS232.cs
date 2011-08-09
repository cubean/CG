using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using CG.Communication.IO;

namespace CG.Communication.PLC.MITSUBISH.Service
{
    /// <summary>
    /// FXRS232继承自通用串口基类RS232并根据FXRS232的特点重写了有关方法
    /// </summary>
    public class FXRS232 : RS232,ISynReadable
    {
        private AutoResetEvent mSyncronizedEvent;        //同步调用标志对象实例
        //private byte[] mTotalDataBuffer = new byte[TOTALBUFFERSIZE];        //一共数据的缓冲区大小
        private List<byte[]> mTotalDataList = new List<byte[]>();                       //一共数据应答列表
        //private int mTotalDataIndex = 0;            //一共数据缓冲区索引

        private const int TIMEOUT = 3000;       //等待命令返回的超时时间
        private const int EACHBUFFERSIZE = 0x90;    //每一次读取的缓冲区大小(最大一次读取0x40*2个字符,所以0x90是合适的)
        private const int TOTALBUFFERSIZE = 0x3000;     //一共数据的缓冲区大小

        /// <summary>
        ///  由于FX系列PLC规定了串口的相关参数，因此这里只需要提供串口名称
        /// </summary>
        /// <param name="portName">串口名称</param>
        public FXRS232(string portName) : base(portName,9600,Parity.Even,7,StopBits.One,EACHBUFFERSIZE)
        {
            this.mSyncronizedEvent = new AutoResetEvent(false);
        }

        /// <summary>从串口发送数据（在本类中这个方法无效，调用这个方法什么也不执行）
        /// 
        /// 
        /// </summary>
        /// <param name="dataStr">发送命令的字符串形式</param>
        public override void Send(string dataStr)
        {
            
        }
        /// <summary>从串口发送数据（在本类中这个方法无效，调用这个方法什么也不执行）
        /// 
        /// 
        /// </summary>
        /// <param name="dataBytes">需要从串口发送数据的字节数组</param>
        public override void Send(byte[] dataBytes)
        {
            
        }

        /// <summary>串口受到数据处理方法
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void mPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (this.mPort.BytesToRead > 0)              //读取串口中已经有效的数据
            {
                int revCount = this.mPort.BytesToRead;
                this.mPort.Read(this.mBuffer, this.mBufferIndex, revCount);
                this.mBufferIndex += revCount;
            }
            if (this.mBufferIndex == 1)                 //如果仅读取到一个字节，那么判断是否是ACK或者NAK
            {
                if (this.mBuffer[0] == 0x06 || this.mBuffer[0] == 0x15)
                {
                    this.mBufferIndex = 0;
                    //this.RaiseDataReceivedEvent(new byte[] { this.mBuffer[0] });
                    //this.mTotalDataBuffer[this.mTotalDataIndex] = this.mBuffer[0];      //将结构保存到一共数据缓存中
                    //this.mTotalDataIndex++;
                    this.mTotalDataList.Add(new byte[]{this.mBuffer[0]});
                    //Console.WriteLine("受到ACK或者NAK，继续执行下一条指令...");
                    this.mSyncronizedEvent.Set();
                }
            }
            else
            {
                if (this.mBufferIndex >= 3 && this.mBuffer[this.mBufferIndex - 3] == 0x03)      //如果受到数据倒数第三个字节为ETX，那么表示这个数据块已经接收完毕
                {
                    byte[] data = new byte[this.mBufferIndex-4];
                    //Array.Copy(this.mBuffer, 1, this.mTotalDataBuffer, this.mTotalDataIndex, this.mBufferIndex-3);
                    //this.mTotalDataIndex = this.mTotalDataIndex + this.mBufferIndex - 3;
                    Array.Copy(this.mBuffer, 1, data, 0, data.Length);
                    this.mTotalDataList.Add(data);
                    this.mBufferIndex = 0;
                    //Console.WriteLine("受到ETX，继续执行下一条指令...");
                    this.mSyncronizedEvent.Set();
                    //this.RaiseDataReceivedEvent(data);
                }
            }
        }

        #region 从IReadable接口继承的方法
        /// <summary>同步获取串口数据
        /// 本方法是线程安全的
        /// </summary>
        /// <param name="dataStr"></param>
        /// <returns></returns>
        public byte[] Read(string dataStr)
        {
            List<byte[]> dataBytesList = new List<byte[]>(1);
            string[] byteArrayStr = dataStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] sendData = new byte[byteArrayStr.Length];
            for (int i = 0; i < byteArrayStr.Length; i++)
            {
                sendData[i] = Convert.ToByte(byteArrayStr[i], 16);
            }
            dataBytesList.Add(sendData);
            return this.Read(dataBytesList)[0];
        }
        /// <summary>同步获取串口数据
        /// 本方法是线程安全的
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        public byte[] Read(byte[] dataBytes)
        {
            List<byte[]> dataBytesList = new List<byte[]>(1);
            dataBytesList.Add(dataBytes);
            return this.Read(dataBytesList)[0];
        }
        /// <summary>同步获取串口数据
        /// 本方法是线程安全的
        /// 如果命令集合中有一条命令发生超时，那么直接返回null
        /// </summary>
        /// <param name="dataStrList">串口命令集合</param>
        /// <returns>获得命令集合的所有命令返回数据</returns>
        public List<byte[]> Read(List<string> dataStrList)
        {
            List<byte[]> dataBytesList = new List<byte[]>();
            foreach (string dataStr in dataStrList)
            {
                string[] byteArrayStr = dataStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                byte[] sendData = new byte[byteArrayStr.Length];
                for (int i = 0; i < byteArrayStr.Length; i++)
                {
                    sendData[i] = Convert.ToByte(byteArrayStr[i], 16);
                }
                dataBytesList.Add(sendData);
            }
            return this.Read(dataBytesList);
        }
        /// <summary>本类中本方法无效
        /// 
        /// </summary>
        /// <param name="dataBytesList"></param>
        /// <returns></returns>
        public List<byte[]> Read(List<byte[]> dataBytesList)
        {
            lock (this)
            {
                foreach (byte[] sendData in dataBytesList)      //遍历每一条指令
                {
                    if(sendData.Length>2||sendData.Length==1)           //如果是正常的命令，那么正常读取
                    {
                        this.mPort.Write(sendData, 0, sendData.Length);
                        //Console.WriteLine("已发送数据，等待返回中...");
                        bool result = this.mSyncronizedEvent.WaitOne(TIMEOUT, false);
                        //Console.WriteLine("数据已返回");
                        if (!result)         //如果有一条指令返回超时，那么直接返回null
                        {
                            this.mTotalDataList.Clear();
                            this.mBufferIndex = 0;
                            //Console.WriteLine("数据已超时");
                            return null;
                        }
                    }
                    else if(sendData[0]==0xFC)            //如果是2个字节的索引占位符，那么提取索引位置的值写入
                    {
                        int index = sendData[1];
                        if (this.mTotalDataList.Count > index)
                            this.mTotalDataList.Add(this.mTotalDataList[index]);
                        else
                            return null;
                    }
                }
                List<byte[]> data = Utils.CloneList(this.mTotalDataList);
                this.mTotalDataList.Clear();
                //byte[] data = new byte[this.mTotalDataIndex];
                //Array.Copy(this.mTotalDataBuffer, data, this.mTotalDataIndex);
                //this.mTotalDataIndex = 0;
                return data;
            }
        }
        #endregion

        public bool IsDeviceOnLine
        {
            get
            {
                lock(this)
                {
                    int count = 0;
                    if (!this.IsOpen)
                        this.Open();
                    for (int i = 0; i < 3; i++)
                    {
                        byte[] reply = this.Read(Convert.ToString((int)FXSeriesControl.ENQ, 16));
                        if (reply.Length == 1 && reply[0] == (byte)FXSeriesControl.ACK)
                        {
                            count++;
                            break;
                        }
                        Thread.Sleep(500);
                    }
                    this.Close();
                    return count > 0 ? true : false;
                }
            }
        }
    }
}
