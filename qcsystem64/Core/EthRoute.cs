using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Collections.Concurrent;
using TcpProxy.Proxy;

namespace qcsystem64
{
    public class EthRoute
    {
        public static List<EthRoute> all_routes = new List<EthRoute>();
        public AutoResetEvent tcpClientConnected = new AutoResetEvent(false);
        
        public ConcurrentDictionary<string, EthMinerInfoObject> all_eth_list = new ConcurrentDictionary<string, EthMinerInfoObject>();
        public newportset setting { get; set; }
        public TcpListener listener { get; set; }
        public int submit_work_num { get; set; }
        public int benefits_num { get; set; }
        public X509Certificate2 serverCertificate { get; set; }
        public void Stop() {
            Isstart = false;
            tcpClientConnected.Close();
            all_eth_list.Values.ToList().ForEach(xm =>
            {
                try { xm.BenefitsStream.Close(); } catch { }
                try { xm.ClientStream.Close(); } catch { }
                try { xm.ServerStream.Close(); } catch { }
            });
            listener.Stop();
            DbHelper.DbLog("端口" + setting.localport + "被停止了");

        }
        public Thread AcceptTcpClientThread { get; set; }

        public EthRoute(newportset param)
        {
            setting = param;
            Isstart = true;
            var path = AppDomain.CurrentDomain.BaseDirectory + "cert.pfx";
            serverCertificate = new X509Certificate2(path, File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "pfx-password.txt"));
            listener = new TcpListener(System.Net.IPAddress.Any, setting.localport);
            listener.Start();

            AcceptTcpClientThread = new Thread(() =>
            {
                while (Isstart)
                {
                    try
                    {
                        listener.BeginAcceptTcpClient(new AsyncCallback(TcpClientacceptCallback), listener);
                        tcpClientConnected.WaitOne();
                    }
                    catch { }
                }
            });
            AcceptTcpClientThread.Start();

            if (!string.IsNullOrEmpty(setting.proxyip))
            {
                DbHelper.DbLog("启动代理服务器" + setting.proxyip);
                proxyClient = new Socks5ProxyClient(setting.proxyip, setting.proxyport ?? 1089);
            }

            //检查断线
            new Thread(() =>
            {
                while (Isstart)
                {
                    Thread.Sleep(5000);
                    all_eth_list.Values.ToList().ForEach(eb =>
                    {
                    //清除多余缓存
                    EthHelper.removekeyscache(eb);
                    //清除掉线
                    if ((DateTime.Now - eb.lastworkServermsgTime).TotalSeconds > 60)
                        {
                            try
                            {
                                eb.ServerStream.Close();
                            }
                            catch { }
                        }
                    //清除掉线
                    if ((DateTime.Now - eb.lastworkBenefitsmsgTime).TotalSeconds > 60)
                        {
                            try
                            {
                                eb.BenefitsStream.Close();
                            }
                            catch { }
                        }

                    });
                }
            }).Start();

            DbHelper.DbLog("端口" + setting.localport + "启动成功");
        }
        public bool Isstart { get; set; }

        Socks5ProxyClient proxyClient { get; set; }

        public static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #region 创建连接
        //收到新的链接
        public async void TcpClientacceptCallback(IAsyncResult ar)
        {
            if (!Isstart) return;
            tcpClientConnected.Set();
            #region 保证客户端能正常运行
            var guid = Guid.NewGuid().ToString();
           TcpListener clienttcp = (TcpListener)ar.AsyncState;
            TcpClient providerClient = clienttcp.EndAcceptTcpClient(ar);
           
            var ethobj = new EthMinerInfoObject(guid);
            SslStream ClientStream;
            NetworkStream providerStream = providerClient.GetStream();
            try
            {

                if (setting.ssl == 1)
                {
                    ClientStream = new SslStream(providerStream);
                    ClientStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls | SslProtocols.Tls12 | SslProtocols.Tls13, false);
                    ethobj.ClientStream = ClientStream;
                }
                else {
                    ethobj.ClientStream = providerStream;
                }
                all_eth_list.TryAdd(guid, ethobj);
            }
            catch(Exception ex) {
                DbHelper.DbLog(ex.Message);
                providerClient.Close();
                return;
            }
            var taskT2PLooping = ReadStream(ethobj.ClientStream, "客户端", ethobj, EthHandler.ClientWaitMsg(this));

            #endregion
            //创建id标记
            #region 服务器部分
            TcpClient toTargetServer = new TcpClient();
            NetworkStream ServernetworkStream;
            SslStream ssltargetServceStream;

            new Thread(async () =>
           {
               while (ethobj.address == null)
               {
                   Thread.Sleep(100);
               }
               while (ethobj.runing)
               {
                   while (!ethobj.serverLinked){
                   
                       try
                       {
                           if (proxyClient != null)
                           {
                               toTargetServer = proxyClient.CreateConnection(setting.serverip, setting.serverport);
                           }
                           else
                           {
                               toTargetServer= new TcpClient(setting.serverip, setting.serverport);
                           }
                           ServernetworkStream = toTargetServer.GetStream();
                           if (setting.sssl == 1)
                           {
                               ssltargetServceStream = new SslStream(ServernetworkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                               ssltargetServceStream.AuthenticateAsClient(setting.serverip);
                               ethobj.ServerStream = ssltargetServceStream;
                               await ssltargetServceStream.WriteAsync(Encoding.UTF8.GetBytes(ethobj.loginmsg), ethobj.ct.Token).ConfigureAwait(false);
                               await ssltargetServceStream.WriteAsync(Encoding.UTF8.GetBytes(EthHelper.getworkmsg), ethobj.ct.Token).ConfigureAwait(false);
                           }
                           else {
                               ethobj.ServerStream = ServernetworkStream;
                               await ServernetworkStream.WriteAsync(Encoding.UTF8.GetBytes(ethobj.loginmsg), ethobj.ct.Token).ConfigureAwait(false);
                               await ServernetworkStream.WriteAsync(Encoding.UTF8.GetBytes(EthHelper.getworkmsg), ethobj.ct.Token).ConfigureAwait(false);
                           }
               
                           ethobj.serverLinked = true;
                       }
                       catch(Exception ex) {
                           DbHelper.DbLog(ex.Message);
                           ethobj.serverLinked = false; ethobj.serverCanMsg = false;
                       }
                       Thread.Sleep(100);
                   }

                   if (ethobj.serverLinked)
                   {
                       //这里极有可能会出现占用问题
                       try
                       {
                           while (ethobj.benefitsNotSendMsg.Count > 0)
                           {
                               Thread.Sleep(20);
                               byte[] buffer;
                               var msg = ethobj.serverNotSendMsg.TryDequeue(out buffer);
                               await ethobj.ServerStream.WriteAsync(buffer, ethobj.ct.Token).ConfigureAwait(false);
                           }
                           ethobj.serverCanMsg = true;
                           var taskP2TLooping = ReadStream(ethobj.ServerStream, "服务端", ethobj, EthHandler.ServerWaitMsg);
                           await Task.WhenAll(taskP2TLooping);
                       }
                       catch (Exception ex) {
                           DbHelper.DbLog(ex.Message);
                       }
                   }
                   try
                   {
                       ethobj.serverLinked = false;
                       ethobj.serverCanMsg = false;
                       toTargetServer.Close();
                   }
                   catch { } 
               }


           }).Start();

            #endregion
            #region 抽水部分
            TcpClient benefitsClient = new TcpClient();
            NetworkStream benefitsnetworkStream;
            SslStream sslbenefitsClientStream;
            new Thread(async () =>
            {
                while (ethobj.runing)
                {
                    while (!ethobj.benefitsLinked)
                    {
                        try
                        {
                            if (proxyClient != null)
                                benefitsClient = proxyClient.CreateConnection(setting.bserverip, setting.bserverport);
                            else
                            {
                                benefitsClient= new TcpClient(setting.bserverip, setting.bserverport);
                            }
                            benefitsnetworkStream = benefitsClient.GetStream();
                            var s1 = "{\"id\":1,\"method\":\"eth_submitLogin\",\"worker\":\""+ ethobj.device_name + ",\"params\":[\"" + setting.benefits_address +   "\",\"x\"],\"jsonrpc\":\"2.0\"}\n";
                            if (setting.bssl == 1)
                            {
                                sslbenefitsClientStream = new SslStream(benefitsnetworkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                                sslbenefitsClientStream.AuthenticateAsClient(setting.bserverip);
                                ethobj.BenefitsStream = sslbenefitsClientStream;

                                await sslbenefitsClientStream.WriteAsync(Encoding.UTF8.GetBytes(s1), ethobj.ct.Token).ConfigureAwait(false);
                                await sslbenefitsClientStream.WriteAsync(Encoding.UTF8.GetBytes(EthHelper.getworkmsg), ethobj.ct.Token).ConfigureAwait(false);
                            }
                            else {
                                ethobj.BenefitsStream = benefitsnetworkStream;

                                await benefitsnetworkStream.WriteAsync(Encoding.UTF8.GetBytes(s1), ethobj.ct.Token).ConfigureAwait(false);
                                await benefitsnetworkStream.WriteAsync(Encoding.UTF8.GetBytes(EthHelper.getworkmsg), ethobj.ct.Token).ConfigureAwait(false);
                            }
                            ethobj.benefitsLinked = true;
                        }
                        catch(Exception ex) {
                            DbHelper.DbLog(ex.Message);
                            ethobj.benefitsLinked = false; ethobj.benefitsCanMsg = false; 
                        }
                        Thread.Sleep(100);
                    }
                    if (ethobj.benefitsLinked)
                    {
                        try
                        {
               
                            while (ethobj.benefitsNotSendMsg.Count > 0)
                            {
                                Thread.Sleep(20);
                                byte[] buffer;
                                var msg = ethobj.benefitsNotSendMsg.TryDequeue(out buffer);
                                await ethobj.BenefitsStream.WriteAsync(buffer, ethobj.ct.Token).ConfigureAwait(false);
                            }

                            ethobj.benefitsCanMsg = true;
                            var taskP2BLooping = ReadStream(ethobj.BenefitsStream, "抽水", ethobj, EthHandler.BenefitsWaitMsg);
                            await Task.WhenAll(taskP2BLooping);
                        }
                        catch(Exception ex) {
                            DbHelper.DbLog(ex.Message);
                        }
                    
                    }
                    try
                    {
                        ethobj.benefitsLinked = false;
                        ethobj.benefitsCanMsg = false; 
                        benefitsClient.Close();} catch { }
                }
            }).Start();
            #endregion

            //队列消息
            EthHelper.clientMsgSend(ethobj, this);
            var completedTask = await Task.WhenAny(taskT2PLooping);
            ethobj.runing = false;
            ethobj.ct.Cancel();
            try
            { providerClient.Close();
           
            }
            catch { }
            try
            { toTargetServer.Close(); }
            catch { }
            try
            { benefitsClient.Close(); }
            catch { }
            all_eth_list.TryRemove(guid, out EthMinerInfoObject eva);
            DbHelper.DbLog(ethobj.device_name + "被关闭");

        }
 
      
        #endregion
      
        private async Task ReadStream(Stream stream, string exmsg, EthMinerInfoObject ethobj,Action<byte[], EthClientMsg, EthMinerInfoObject> action)
        {
            DbHelper.DbLog(ethobj.device_name + "启动了" + exmsg);
            byte[] buffer = new byte[1024];
            using (stream)
            {
                int bytesRead;
                try
                {
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ethobj.ct.Token).ConfigureAwait(false)) != 0 && Isstart)
                    {
              
                        var jt = EthHelper.decmsg(buffer);
                  
                        try { action(buffer.Take(bytesRead).ToArray(), jt, ethobj); } catch (Exception ex) { DbHelper.DbLog(ex.Message); }
                    }
                }
                catch(Exception ex) {
                    DbHelper.DbLog(ethobj.device_name + exmsg + "报错:"+ex.Message);
                }

            }
            DbHelper.DbLog(ethobj.device_name + exmsg + "被关闭");
        }





    }
}
