using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace qcsystem64
{
    public class EthSimpleObject {
        public decimal power { get; set; }
        public int submit_work_num { get; set; }
        public int benefits_num { get; set; }
        public string address { get; set; }
        public string device_name { get; set; }
        public string guid { get; set; }
        public string loginmsg { get; set; }
        public string benefits_deviceid { get; set; }
        public DateTime loginTime { get; set; }
    }
    public class EthMinerInfoObject : EthSimpleObject
    {
        public EthMinerInfoObject(string g)
        {
            device_name = guid;
            guid = g;
            benefits_deviceid = EthHelper.getdeviceid();
            submit_work_num = 0;
            benefits_num = 0;
            power = 0;
            loginTime = DateTime.Now;
            clientMsg = new ConcurrentQueue<SendClientMsg>();
            server_jobs = new ConcurrentDictionary<string, DateTime>();
            benfits_jobs = new ConcurrentDictionary<string, DateTime>();
            lastworkServermsgTime = DateTime.Now;
            lastworkBenefitsmsgTime = DateTime.Now;
            runing = true;
            serverNotSendMsg = new ConcurrentQueue<byte[]>();
            benefitsNotSendMsg = new ConcurrentQueue<byte[]>();
        }
        /// <summary>
        /// 服务器是否连接
        /// </summary>
        public bool serverLinked { get; set; }
        /// <summary>
        /// 抽水服务器是否连接
        /// </summary>
        public bool benefitsLinked { get; set; }
        /// <summary>
        /// 服务器是否可以发送消息了
        /// </summary>
        public bool serverCanMsg { get; set; }
        /// <summary>
        /// 抽水段是否可以发送消息了
        /// </summary>
        public bool benefitsCanMsg { get; set; }
        /// <summary>
        /// 服务端未发出去的信息
        /// </summary>
        public ConcurrentQueue<byte[]> serverNotSendMsg { get; set; }
        /// <summary>
        /// 抽水端未发出去的信息
        /// </summary>
        public ConcurrentQueue<byte[]> benefitsNotSendMsg { get; set; }

        public async Task SendMsgToServer(byte[] msg)
        {

            if (serverCanMsg)
            {
                await ServerStream.WriteAsync(msg, EthRoute.TRANSFERING_TOKEN_SRC.Token).ConfigureAwait(false);
            }
            else
            {
                serverNotSendMsg.Enqueue(msg);
            }
        }
        public async Task SendMsgToBenefits(byte[] msg)
        {

            if (benefitsCanMsg)
            {
                await BenefitsStream.WriteAsync(msg, EthRoute.TRANSFERING_TOKEN_SRC.Token).ConfigureAwait(false);
            }
            else
            {
                benefitsNotSendMsg.Enqueue(msg);
            }
        }


        public bool runing { get; set; }
        public ConcurrentQueue<SendClientMsg> clientMsg { get; set; }
        public DateTime lastworkServermsgTime { get; set; }
        public DateTime lastworkBenefitsmsgTime { get; set; }
        public ConcurrentDictionary<string, DateTime> server_jobs { get; set; }
        public ConcurrentDictionary<string, DateTime> benfits_jobs { get; set; }
        public SslStream ServerStream{ get; set; }
        public SslStream ClientStream { get; set; }
        public SslStream BenefitsStream { get; set; }


    }
}
