namespace qcsystem64
{
    public class ApiResult<T>
    {
        /// <summary>
        /// 构建函数，假设没有错误的发生
        /// </summary>
        public ApiResult()
        {
            msg = "ok";
            status = 1;

        }
        /// <summary>
        /// 附加信息
        /// </summary>
        public object otherdata { get; set; }
        /// <summary>
        /// status=1,表示接口调用正常
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 错误码
        /// </summary>
        public string errorcode { get; set; }
        /// <summary>
        /// status=1，data
        /// </summary>
        public T data
        {
            get; set;
        }
        /// <summary>
        /// 报错
        /// </summary>
        /// <param name="er"></param>
        public void Error(string er, string errcode = "-1")
        {
            status = 0;
            msg = er;
            errorcode = errcode;
        }

     
        public static ApiResult<T> DoApi<D>(string password,Action<ApiResult<T>, ModelContext> action)
        {
            var rt = new ApiResult<T>();
            try
            {
                using (var db = new ModelContext())
                {
                    var passwd = "admin";
                    var ppath = AppDomain.CurrentDomain.BaseDirectory + "password.txt";
                    if (File.Exists(ppath))
                    passwd = File.ReadAllText(ppath);
                    if (password != passwd)
                    {
                        rt.status = 0;
                        rt.msg = "授权失败";
                    }
                    else
                    {
                        action(rt, db);
                    }
                }
            }
            catch (Exception ex)
            {
                rt.status = 0;
                rt.msg = ex.Message;
            }
            return rt;
        }
    }
}
