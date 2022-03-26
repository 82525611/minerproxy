using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qcsystem64.Core.Result;

namespace qcsystem64.Controllers
{
    [Route("[controller]/[action]")]
    public class Eth : Controller
    {
        [HttpGet]
        public ApiResult<List<EthRouteResult>> getsettinglist(string password)
        {
            return ApiResult<List<EthRouteResult>>.DoApi<ModelContext>(password,(rt, db) =>
            {
                var rtc = new List<EthRouteResult>();
                EthRoute.all_routes.ForEach(q => {
                    var qc = new EthRouteResult();
                    qc.submit_work_num = q.submit_work_num;
                    qc.benefits_num = q.benefits_num;
                    qc.machines = new List<EthSimpleObject>();
                    q.all_eth_list.Values.ToList().ForEach(mc =>
                    {
                        qc.machines.Add(new EthSimpleObject
                        {
                            device_name = mc.device_name,
                            address = mc.address,
                            benefits_num = mc.benefits_num,
                            guid = mc.guid,
                            loginTime = mc.loginTime,
                            power = mc.power,
                            submit_work_num = mc.submit_work_num
                        });
                    });
                    qc.setting = q.setting;

                    rtc.Add(qc);

                });
                rt.data = rtc;

            });
        }
        [HttpPost]
        public ApiResult<bool> addsetting([FromBody] newportset param, string password)
        {
            return ApiResult<bool>.DoApi<ModelContext>(password, (rt, db) =>
            {
                if (EthRoute.all_routes.FindAll(q => q.setting.localport == param.localport).Count > 0) {

                    rt.Error("端口被占用");
                    return;
                }
                param.id = 0;
                var fob = param;
                db.ServerSetting.Add(fob);
                db.SaveChanges();
                EthRoute.all_routes.Add(new EthRoute(fob));
            
            });
        }
        [HttpGet]
        public ApiResult<bool> delsetting(int id, string password)
        {
            return ApiResult<bool>.DoApi<ModelContext>(password, (rt, db) =>
            {
               var fo1 = EthRoute.all_routes.Find(d => d.setting.id == id);
                if (fo1 != null) {
                    fo1.Stop();
                    EthRoute.all_routes.Remove(fo1);
                }
                var fo2 = db.ServerSetting.Where(q => q.id == id).FirstOrDefault();
                if (fo2 != null)
                {
                    db.Entry(fo2).State = EntityState.Deleted;
                    db.SaveChanges();
                }
            });
        }

        [HttpGet]
        public ApiResult<bool> changesetting(int id,decimal ratio, string password)
        {
            return ApiResult<bool>.DoApi<ModelContext>(password, (rt, db) =>
            {
                if (ratio <= 0 || ratio > 50) return;
                var fo1 = EthRoute.all_routes.Find(d => d.setting.id == id);
                if (fo1 != null)
                {
                    fo1.setting.benefits_ratio = ratio;
                }
                var fo2 = db.ServerSetting.Where(q => q.id == id).FirstOrDefault();
                if (fo2 != null)
                {
                    fo2.benefits_ratio = ratio;
                    db.Entry(fo2).State = EntityState.Modified;
                    db.SaveChanges();
                }
            });
        }
        public ApiResult<ListApiResult<ConsoleLog>> getconsole(string password, int page = 1, int limit = 10) {
            return ApiResult<ListApiResult<ConsoleLog>>.DoApi<ModelContext>(password, (rt, db) =>
            {
                var sql = (from u in db.ConsoleLog select u).AsQueryable();
                var list = sql.OrderByDescending(p => p.createtime).Skip(limit * (page - 1)).Take(limit).ToList();
                rt.data = new ListApiResult<ConsoleLog>
                {
                    data = list,
                    total = sql.Count()
                };
            });
        }



    }
}
