
var vm = new Vue({
    el: '#app',
    data: {
        tabActive: 0,
        password: '',
        showlogin: true,
        cosnolelist: [],
        cosnoleloading: false,
        cosnolefinished: true,
        ethpool: [],
        name: '',
        host: location.host,
        //host: "localhost:6700",
        servername: '',
        serverport: null,
        bservername: '',
        bserverport: null,
        localport: null,
        ratio: 1,
        baddress: '',
        proxyip: null,
        proxyport:null


    

    },
    methods: {
        login: function (auto) {
            let ths=this;
            $.getJSON(location.protocol +"//" + ths.$data.host + "/eth/getconsole?password=" + ths.$data.password + "&limit=50", function (resdata) {
                if (resdata.status == 0) {
                    if (auto !== true) {
                        vant.Dialog.alert({
                            title: '温馨提示',
                            message: resdata.msg,
                            confirmButtonColor: '#39f',
                        });
                    }
                    return;
                }
                resdata.data.data.reverse()
                resdata.data.data.forEach(q=> {
                    if (!ths.$data.cosnolelist.find(c=>c.id == q.id)) {
                        ths.$data.cosnolelist.unshift(q);
                    }
                });
                localStorage.setItem("password", ths.$data.password)
                let res = resdata.data.data
                ths.$data.showlogin = false;
                ths.getsetting();
                setInterval(function () {
                    $.getJSON(location.protocol + "//" + ths.$data.host + "/eth/getconsole?password=" + ths.$data.password + "&limit=10", function (resdata2) {
                        resdata2.data.data.reverse()
                        resdata2.data.data.forEach(q=> {
                            if (!ths.$data.cosnolelist.find(c=>c.id == q.id)) {
                                ths.$data.cosnolelist.unshift(q);
                            }
                        });
                    });
                }, 5000)
            });
         
        },
        changechoushui: function (ycz, id) {
            let ths = this;
            var str = prompt("请输入新的抽水比例", ycz);
            str = parseFloat(str);
            if (isNaN(str) || str<=0 ||str>50) {
                vant.Dialog.alert({
                    title: '温馨提示',
                    message: "请输入正确的抽水比例，小于50大于0",
                    confirmButtonColor: '#39f',
                });
                return;
            }
            $.getJSON(location.protocol + "//" + ths.$data.host + "/eth/changesetting?password=" + ths.$data.password + "&id=" + id + "&ratio=" + str, function (resdata) {
                if (resdata.status == 0) {
                    
                        vant.Dialog.alert({
                            title: '温馨提示',
                            message: resdata.msg,
                            confirmButtonColor: '#39f',
                        });
                    
                    return;
                }
                vant.Dialog.alert({
                    title: '温馨提示',
                    message: "修改成功",
                    confirmButtonColor: '#39f',
                });
                ths.getsetting();

            });

        },
        getsetting: function () {
            vant.Toast.loading({
                duration: 0,
                mask: true,
                message: '刷新中'
            });
            let ths = this;
            $.getJSON(location.protocol + "//" + ths.$data.host + "/eth/getsettinglist?password=" + ths.$data.password, function (resdata) {
                vant.Toast.clear();
                resdata.data.forEach(q=> {
                    q.showmc = [];
                    q.sumpower = 0;
                    if (q.submit_work_num > 0)
                        q.shijibili = (parseFloat(q.benefits_num) * 100 / parseFloat(q.submit_work_num)).toFixed(2) + "%"
                    else {
                        q.shijibili="0%"
                    }
                    q.machines.forEach(d=> {
                        let qp = (parseFloat(d.power) / 1000000)
                        d.powerstr = qp.toFixed(2) + "M";
                        q.sumpower += qp;
                    })
                    q.sumpower = parseFloat(q.sumpower)
                    q.sumpowerstr = q.sumpower.toFixed(2) + "M";
                    if (q.sumpower > 1000) {
                        q.sumpower = q.sumpower / 1000;
                        q.sumpowerstr = q.sumpower.toFixed(2) + "G";
                    }
                })
                ths.$data.ethpool =   resdata.data
               
               
            });
        },
        submitRegisterDataFn: function (data) {
            
            vant.Toast.loading({
                duration: 0,
                mask: true,
                message: '提交中...'
            });

            let ths = this;
            if (isNaN(parseInt(ths.$data.serverport)) || ths.$data.serverport < 0 || ths.$data.serverport > 60000) {
                alert('服务端口错误'); return;
            }
            if (isNaN(parseInt(ths.$data.localport)) || ths.$data.localport < 0 || ths.$data.localport > 60000) {
                alert('本地端口错误'); return;
            }
            if (isNaN(parseInt(ths.$data.bserverport)) || ths.$data.bserverport < 0 || ths.$data.bserverport > 60000) {
                alert('抽水端口错误'); return;
            }

           
            axios.post(location.protocol + "//" + ths.$data.host + "/eth/addsetting?password=" + ths.$data.password, {
                id: 0,
                name: ths.name,
                serverip: ths.servername,
                serverport: ths.serverport,
                bserverip: ths.bservername,
                bserverport: ths.bserverport,
                localport: ths.localport,
                benefits_ratio: ths.ratio,
                benefits_address: ths.baddress,
                proxyip: ths.proxyip?ths.proxyip:null,
                proxyport: ths.proxyip?ths.proxyport:null
                })
                    .then(function (response) {
                        vant.Toast.clear();

                        if (response.data.status === 1) {
                            vant.Dialog.alert({
                                title: '提示',
                                message: '添加成功',
                                confirmButtonColor: '#39f',
                            }).then(function () {
                                this.name = '';
                                this.servername = '';
                                this.bservername = '';
                                this.serverport = null;
                                this.bserverport = null;
                               this.localport = null;
                               this.baddress = '';
                                this.ratio =1;
                                this.getsetting();
                            }.bind(ths));
                        } else{

                            vant.Dialog.alert({
                                title: '温馨提示',
                                message: response.data.msg,
                                confirmButtonColor: '#39f',
                            })
                        }

                    }.bind(ths))
                    .catch(function (error) {
                        console.log(error);
                        vant.Toast.clear();
                    })
         
        },
        delpool: function (id) {
            let ths = this;
            var styr = confirm("是否删除矿池" + id + "？")
            if (styr) {
                $.getJSON(location.protocol + "//" + ths.$data.host + "/eth/delsetting?password=" + ths.$data.password + "&id=" + id, function (resdata) {
                    if (resdata.status == 0) {
                 
                            vant.Dialog.alert({
                                title: '温馨提示',
                                message: resdata.msg,
                                confirmButtonColor: '#39f',
                            });
                        
                        return;
                    }
                    vant.Dialog.alert({
                        title: '温馨提示',
                        message: "删除成功",
                        confirmButtonColor: '#39f',
                    });
                    ths.getsetting();
                      


                });
            }
        }
    },
    mounted: function () {
    
        let ths = this;
        if (localStorage.getItem("password")) {
            ths.$data.password = localStorage.getItem("password")
            ths.login(true)
        }


       
      
    },
});
