
using Grapevine;
using Grapevine.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using TNetD.Nodes;

/*
public sealed class JsonQueryService : RESTResource
{
    public JsonQueryService(NodeConfig nodeConfig)
    {

    }

    [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/r$")]
    public void HandleAllGetRequests(HttpListenerContext context)
    {
        JObject obj = this.GetJsonPayload(context.Request);

        string str = obj.ToString();

        //JsonConvert
        
        //JObject obj = GetJsonPayload(context);

        this.SendTextResponse(context, "JSON:" + str);
    }
}*/