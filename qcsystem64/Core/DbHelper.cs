using log4net;
using log4net.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace qcsystem64
{
    public class DbHelper
    {
   
        public static void AddServerLogs(EthClientMsg jt, EthMinerInfoObject ethobj, string txt, string pow_hash,int isbenefits)
        {
            using (var db = new ModelContext())
            {
                while (string.IsNullOrEmpty(ethobj.address)) Thread.Sleep(20);
                if (jt.id != 0 && isbenefits==0)
                {
                    while (!db.ClientLogs.Any(q => q.guid == ethobj.guid && q.rid == jt.id)) Thread.Sleep(20);
                    var fob = db.ClientLogs.Where(q => q.guid == ethobj.guid && q.rid == jt.id).FirstOrDefault();
                    fob.res_text = txt;
                    db.Entry(fob).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                db.ServerLogs.Add(new ServerLogs()
                {
                    rid = jt.id,
                    text = txt,
                    createtime = DateTime.Now,
                    guid = ethobj.guid,
                    pow_hash = pow_hash,
                    address = ethobj.address,
                    benefits = 0
                });
                db.SaveChanges();
            }
        }

        public static void AddClientLogs(EthClientMsg jt, EthMinerInfoObject ethobj, int isbenefits,string txt)
        {
            using (var db = new ModelContext())
            {
                db.ClientLogs.Add(new ClientLogs()
                {
                    rid = jt.id,
                    text = txt,
                    createtime = DateTime.Now,
                    method = jt.method,
                    address = ethobj.address,
                    device_name = ethobj.device_name,
                    guid = ethobj.guid,
                    benefits = isbenefits
                });

                db.SaveChanges();
            }
        }

        public static void DbLog(string str)
        {

            try
            {
                new Thread(() =>
                {
                    using (var db = new ModelContext())
                    {
                        Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + str);
                        db.ConsoleLog.Add(new ConsoleLog
                        {
                            createtime = DateTime.Now,
                            text = str
                        });
                        db.SaveChanges();
                    }
                }).Start();
            }
            catch { }

        }
    }
}
