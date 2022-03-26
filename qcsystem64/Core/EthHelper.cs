using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qcsystem64
{
    public class EthHelper
    {
        //compact
        //{"id":8,"method":"eth_submitWork","params":["0x569d3a192e93b89e","0xf8c3b9809c464bb14263df659acedca0fa53570fba4c463581c859317dab5861","0x2108215d84aaeb1f1b3e9e6d1307d321b742045506ff18e56a5a1b47c15250ea"],"jsonrpc":"2.0"}
        //{"id":1,"method":"eth_submitLogin","worker":"eth1.0","params":["0x28E1DCB35D93C1a1a8685BA1Eb8EfAa26F53310D.YITAI3","x"],"jsonrpc":"2.0"}
        //{"id":9,"method":"eth_submitHashrate","params":["0xed9893d","0xa20bcc19cb0b2cbd27a643206c94932faaca9726937aec4e970bc435cee95e71"],"jsonrpc":"2.0"}
        //{"id":1,"jsonrpc":"2.0","result":true}
        //{"id":0,"jsonrpc":"2.0","result":["0xd1625485217ed95df3a79887ec3bdc83380bb240feec27d242fbabc551aaf303","0xfbaa95d823ad6c9726475e823d16c52fe4e108cf78d2b8271e900f89118fd6c5","0x00000000ffff00000000ffff00000000ffff00000000ffff00000000ffff0000","0xdcaedf"]}
        public const string getworkmsg = "{\"id\":2,\"method\":\"eth_getWork\",\"params\":[],\"jsonrpc\":\"2.0\"}\n";
        public static string loginsuccessmsg(int id) { return "{\"id\":" + id + ",\"result\":true,\"jsonrpc\":\"2.0\"}\n"; }
        //{"id":1,"jsonrpc":"2.0","result":true}
        /// <summary>
        /// 解析地址发送过来的信息
        /// dejson for server message
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static EthClientMsg decmsg(byte[] buffer) {
            var js = System.Text.Encoding.UTF8.GetString(buffer);
            js = js.Split('\n')[0];
            var jt = JsonConvert.DeserializeObject<EthClientMsg>(js);
            return jt;
        }
        /// <summary>
        /// 删除缓存
        /// delete server message cache
        /// </summary>
        /// <param name="benfitsmsgtimelist"></param>
        /// <param name="benfitsmsglist"></param>
        public static void removekeyscache(EthMinerInfoObject ethobject)
        {
            var now = DateTime.Now;
            try
            {
                ethobject.server_jobs.Keys.ToList().FindAll(q => ethobject.server_jobs[q].AddSeconds(600) < now)
               .ForEach(key =>
               {
                   ethobject.server_jobs.TryRemove(key, out DateTime dateTime);
               });
                ethobject.benfits_jobs.Keys.ToList().FindAll(q => ethobject.benfits_jobs[q].AddSeconds(600) < now)
                .ForEach(key =>
                {
                    ethobject.benfits_jobs.TryRemove(key, out DateTime dateTime);
                });
            }
            catch { }
        }
        /// <summary>
        /// 生成一个抽水用的设备id
        /// create a deviceid for benefits device
        /// </summary>
        /// <returns>a device id</returns>
        public static string getdeviceid() { 
            return "0x" + Guid.NewGuid().ToString("x").Substring(0, 16) + Guid.NewGuid().ToString("x").Substring(0, 16) + Guid.NewGuid().ToString("x").Substring(0, 16) + Guid.NewGuid().ToString("x").Substring(0, 16); ;
        }
        /// <summary>
        /// msg to client queue
        /// </summary>
        /// <param name="ethobj"></param>
        /// <param name="er"></param>
        public static void clientMsgSend(EthMinerInfoObject ethobj, EthRoute er) {
            new Thread(async () =>
            {
                while (ethobj.runing)
                {
                    Thread.Sleep(20);
                    if (ethobj.clientMsg.Count == 0) continue;
                    SendClientMsg buffer;
                    ethobj.clientMsg.TryDequeue(out buffer);
                    if (buffer != null)
                    {
                        try
                        {
                            //这是应该要抽水的数量，比如3
                            var should_benefits_num = (decimal)ethobj.submit_work_num * er.setting.benefits_ratio / 100;

                            if (buffer.id == 0)
                            {
                                if (ethobj.benefits_num < should_benefits_num)
                                {
                                    //发送抽水
                                    if (buffer.isbenfits) await ethobj.ClientStream.WriteAsync(buffer.bytes, ethobj.ct.Token).ConfigureAwait(false);
                                }
                                else
                                {
                                    //发送不抽水
                                    if (!buffer.isbenfits) await ethobj.ClientStream.WriteAsync(buffer.bytes, ethobj.ct.Token).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                await ethobj.ClientStream.WriteAsync(buffer.bytes, ethobj.ct.Token).ConfigureAwait(false);
                            }
                        }
                        catch { }
                    }
                }
            }).Start();
        }
    }
}
