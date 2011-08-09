using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using libnodaveNET;
using STLCompiler;

namespace CG.Communication.PLC.Siemens.S7_200.Service
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.
    public class FXService : IFXService
    {
        libnodave.daveOSserialType fds;
        libnodave.daveInterface di;
        libnodave.daveConnection dc;

        #region 功能部分
        public bool OpenPort(string port, string baud, char parity)
        {
            fds.rfd = libnodave.setPort(port, baud, parity);
            fds.wfd = fds.rfd;
            return fds.rfd > 0;
        }

        public void ClosePort()
        {
            libnodave.closePort(fds.rfd);
        }

        public bool Connect(int address)
        {
            if (fds.rfd <= 0)
                return false;

            di = new libnodave.daveInterface(fds, "IF1", 0, libnodave.daveProtoPPI, libnodave.daveSpeed187k);
            libnodave.daveSetDebug(libnodave.daveDebugAll);
            dc = new libnodave.daveConnection(di, address, 0, 0);
            return 0 == dc.connectPLC();
        }

        public void DisConnect()
        {
            dc.disconnectPLC();
        }


        public bool Complie(string sourceCode, out byte[] block)
        {
            block = null;
            try
            {
                var compiler = new Compiler();
                compiler.AssemblyBlock(
                    eBLOCK200_BLOCKTYPES.eBLOCK200_BLOCKTYPE_OB,
                    1,
                    STLCompiler.eBLOCK200_OB_VERSIONS.eBLOCK200_OB_VERSION_2ND_GEN_6,
                    false,
                    true,
                    true,
                    sourceCode,
                    out block);
            }
            catch (System.Exception )
            {
                return false;
            }

            return true;
        }

        public bool Decompile(byte[] block, out string readSourceCode)
        {
            readSourceCode = string.Empty;
            try
            {
                var compiler = new Compiler();
                compiler.DisassemblyBlock(block, out readSourceCode);
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region 读取部分
        public bool ReadProgramBlock(out byte[] block)
        {
            try
            {
                int length = 65536;
                block = new byte[length];

                dc.getProgramBlock(libnodave.daveBlockType_OB, 1, block, ref length);

                block = block.Take(length).ToArray();
            }
            catch (System.Exception)
            {
                block = null;
                return false;
            }
            return true;
        }
        #endregion

        #region 写入部分
        public bool WriteProgramBlock(byte[] block)
        {
            try
            {
                dc.putProgramBlock(libnodave.daveBlockType_OB, 1, block, block.Length);
            }
            catch (System.Exception)
            {
                block = null;
                return false;
            }
            return true;

        }
        #endregion
    }
}
