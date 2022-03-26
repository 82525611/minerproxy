using Microsoft.AspNetCore.HttpOverrides;
using qcsystem64;
/// <summary>
/// �����ݿ��ж�ȡ��ص����ü��� 
/// load dbsetting to do
/// </summary>
#region EthServer
using (var db = new ModelContext())
{
    var settings = db.ServerSetting.ToList();
    settings.ForEach(x =>
    {
        EthRoute.all_routes.Add(new EthRoute(x));
    });
}

#endregion

/// <summary>
/// ����ʱ��Ӳ��� --urls http://*:5555 ���Զ���˿�
/// Add parameters at startup --urls http://*:5555 ,It can set its own port 
/// </summary>
#region mvcApi
var builder = WebApplication.CreateBuilder(args);
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
        builder => builder.SetIsOriginAllowed(_ => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    CoreHelper.setJsonSerializerOptions(options.JsonSerializerOptions);
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers().RequireCors(MyAllowSpecificOrigins); ;
});

app.Run();
#endregion

