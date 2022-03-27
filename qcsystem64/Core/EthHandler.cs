using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qcsystem64
{
    public class EthHandler
    {

        public static Action<byte[], EthClientMsg, EthMinerInfoObject> BenefitsWaitMsg
        {
            get
            {
                return new Action<byte[], EthClientMsg, EthMinerInfoObject>((buffer, msgjson, ethobj) =>
                {
                    string pow_hash = null;
                    if (msgjson.id == 0)
                    {
                        ethobj.lastworkBenefitsmsgTime = DateTime.Now;
                        pow_hash = ((JArray)msgjson.result)[0].ToString();
                        ethobj.benfits_jobs.TryAdd(pow_hash, DateTime.Now);
                    }
                    if (msgjson.id == 0)
                        ethobj.clientMsg.Enqueue(new SendClientMsg
                        {
                            bytes = buffer,
                            id = msgjson.id,
                            isbenfits = true
                        });

                    
                });
            }
        }
        public static Action<byte[], EthClientMsg, EthMinerInfoObject> ServerWaitMsg
        {
            get
            {
                return new Action<byte[], EthClientMsg, EthMinerInfoObject>((buffer, msgjson, ethobj) =>
                {
                 
                    string pow_hash = null;
                    if (msgjson.id == 0)
                    {
                        ethobj.lastworkServermsgTime = DateTime.Now;
                        pow_hash = ((JArray)msgjson.result)[0].ToString();
                        ethobj.server_jobs.TryAdd(pow_hash, DateTime.Now);
                        //发给客户端
                        if (msgjson.id == 0)
                            ethobj.clientMsg.Enqueue(new SendClientMsg
                            {
                                bytes = buffer,
                                id = msgjson.id,
                                isbenfits = false
                            });
                    }
                });
            }
        }
        public static Action<byte[], EthClientMsg, EthMinerInfoObject> ClientWaitMsg(EthRoute route)
        {
       
                return new Action<byte[], EthClientMsg, EthMinerInfoObject>(async (buffer, msgjson, ethobj) =>
                {
                  
                    if (msgjson.method == "eth_submitLogin")
                    {
                        var jsp = msgjson.@params[0].ToString().Split('.');
                        ethobj.address = jsp[0];
                        ethobj.device_name = msgjson.worker;
                        ethobj.loginmsg = Encoding.UTF8.GetString(buffer);
                        if (jsp.Length > 1)
                        {
                            ethobj.device_name = jsp[1];
                        }

                        ethobj.clientMsg.Enqueue(new SendClientMsg
                        {
                            bytes = Encoding.UTF8.GetBytes(EthHelper.loginsuccessmsg(msgjson.id)),
                            id = 1,
                            isbenfits = false
                        });
                    }
                    else if (msgjson.method == "eth_submitWork")
                    {
                        var trynum = 0;
                        //等待服务器接收到包，看看是谁的
                        while ((!ethobj.server_jobs.ContainsKey(msgjson.@params[1]) && !ethobj.benfits_jobs.ContainsKey(msgjson.@params[1])) && trynum++ < 300) Thread.Sleep(10);
                        if (!ethobj.benfits_jobs.ContainsKey(msgjson.@params[1])) await ethobj.SendMsgToServer(buffer);
                        else
                        {
                            ethobj.benefits_num++;
                            route.benefits_num++;
                            await ethobj.SendMsgToBenefits(buffer);
                        }
                        ethobj.submit_work_num++;
                        route.submit_work_num++;
                        //先让客户端成功再说
                        ethobj.clientMsg.Enqueue(new SendClientMsg
                        {
                            bytes = Encoding.UTF8.GetBytes(EthHelper.loginsuccessmsg(msgjson.id)),
                            id = msgjson.id,
                            isbenfits = false
                        });

                    }
                    else if (msgjson.method == "eth_submitHashrate")
                    {
                        var jtarr2 = msgjson.@params;
                        ethobj.power = (decimal)Convert.ToInt64(jtarr2[0].ToString(), 16);
                        var suanlia = Convert.ToInt64(ethobj.power * route.setting.benefits_ratio / ((decimal)100));
                        var suanlib = Convert.ToInt64(ethobj.power * (100 - route.setting.benefits_ratio) / ((decimal)100));
                        msgjson.@params[0] = "0x" + suanlib.ToString("x");
                        var by3 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msgjson) + "\n");
                        await ethobj.SendMsgToServer(by3);
                        msgjson.@params[0] = "0x" + suanlia.ToString("x");
                        msgjson.@params[1] = ethobj.benefits_deviceid;
                        by3 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msgjson) + "\n");
                        await ethobj.SendMsgToBenefits(by3);

                    }
                    else {
                        await ethobj.SendMsgToServer(buffer);
                    }
                });
            
        }
    }
}
