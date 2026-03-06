using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poco;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using TcpServer;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PocoManager : MonoBehaviour
{
    public event Action<string> MessageReceived;

    public const int versionCode = 6;
    public int port = 5001;
    private bool mRunning;
    public AsyncTcpServer server = null;
    public PocoListenersBase pocoListenersBase;
    private RPCParser rpc = null;
    private SimpleProtocolFilter prot = null;
    private UnityDumper dumper = new UnityDumper();
    private System.Collections.Concurrent.ConcurrentDictionary<string, TcpClientState> inbox = new System.Collections.Concurrent.ConcurrentDictionary<string, TcpClientState>();
    private VRSupport vr_support = new VRSupport();
    private Dictionary<string, long> debugProfilingData = new Dictionary<string, long>() {
        { "dump", 0 },
        { "screenshot", 0 },
        { "handleRpcRequest", 0 },
        { "packRpcResponse", 0 },
        { "sendRpcResponse", 0 },
    };

    private bool isInitialized = false;

    class RPC : Attribute
    {
    }

    public void Initialize()
    {
        if (this.isInitialized) return;
        isInitialized = true;

        Application.runInBackground = true;
        // keep whole GameObject alive across scenes
        DontDestroyOnLoad(gameObject);

        prot = new SimpleProtocolFilter();
        rpc = new RPCParser();

        rpc.addRpcMethod("isVRSupported", vr_support.isVRSupported);
        rpc.addRpcMethod("hasMovementFinished", vr_support.IsQueueEmpty);
        rpc.addRpcMethod("RotateObject", vr_support.RotateObject);
        rpc.addRpcMethod("ObjectLookAt", vr_support.ObjectLookAt);
        rpc.addRpcMethod("Screenshot", Screenshot);
        rpc.addRpcMethod("GetScreenSize", GetScreenSize);
        rpc.addRpcMethod("Dump", Dump);
        rpc.addRpcMethod("GetDebugProfilingData", GetDebugProfilingData);
        rpc.addRpcMethod("SetText", SetText);
        rpc.addRpcMethod("SendMessage", SendMessage);
        rpc.addRpcMethod("GetSDKVersion", GetSDKVersion);

        if (pocoListenersBase != null)
        {
            PocoListenerUtils.SubscribePocoListeners(rpc, pocoListenersBase);
        }

        mRunning = true;

        bool started = false;

        for (int i = 0; i < 5; i++)
        {
            int tryPort = port + i;
            AsyncTcpServer candidate = null;
            try
            {
                candidate = new AsyncTcpServer(tryPort);
                candidate.Encoding = Encoding.UTF8;
                candidate.ClientConnected += new EventHandler<TcpClientConnectedEventArgs>(server_ClientConnected);
                candidate.ClientDisconnected += new EventHandler<TcpClientDisconnectedEventArgs>(server_ClientDisconnected);
                candidate.DatagramReceived += new EventHandler<TcpDatagramReceivedEventArgs<byte[]>>(server_Received);

                candidate.Start();
                // If start succeeded, dispose previous server if any (should not be) and assign
                if (this.server != null)
                {
                    try { this.server.Stop(); this.server.Dispose(); } catch { }
                }
                this.server = candidate;
                Debug.Log(string.Format("Tcp server started and listening at {0}", server.Port));
                started = true;
                break;
            }
            catch (SocketException e)
            {
                Debug.LogWarning(string.Format("Tcp server bind to port {0} Failed: {1}", tryPort, e.Message));
                Debug.Log("--- Failed Trace Begin ---");
                Debug.LogException(e);
                Debug.Log("--- Failed Trace End ---");
                // Try to clean candidate and continue to next port
                try
                {
                    candidate?.Stop();
                    candidate?.Dispose();
                }
                catch { }
                candidate = null;
                // continue loop to try next port
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Unexpected error when trying port {tryPort}: {ex.Message}");
                try { candidate?.Stop(); candidate?.Dispose(); } catch { }
                candidate = null;
            }
        }

        if (this.server == null)
        {
            Debug.LogError(string.Format("Unable to find an unused port from {0} to {1}", port, port + 5));
            // do not set isInitialized so that next Initialize attempt can retry
            return;
        }

        vr_support.ClearCommands();
        Debug.Log($"[PocoManager] Initialize complete. Listening on port {server.Port}");
    }

    static void server_ClientConnected(object sender, TcpClientConnectedEventArgs e)
    {
        try
        {
            Debug.Log(string.Format("TCP client {0} has connected.",
                e.TcpClient.Client.RemoteEndPoint.ToString()));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"server_ClientConnected log failed: {ex.Message}");
        }
    }

    static void server_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
    {
        try
        {
            Debug.Log(string.Format("TCP client {0} has disconnected.",
               e.TcpClient.Client.RemoteEndPoint.ToString()));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"server_ClientDisconnected log failed: {ex.Message}");
        }
    }

    private void server_Received(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
    {
        try
        {
            Debug.Log(string.Format("Client : {0} --> {1}",
                e.Client.TcpClient.Client.RemoteEndPoint.ToString(), e.Datagram.Length));
            TcpClientState internalClient = e.Client;
            string tcpClientKey = internalClient.TcpClient.Client.RemoteEndPoint.ToString();
            inbox.AddOrUpdate(tcpClientKey, internalClient, (n, o) => internalClient);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"server_Received error: {ex}");
        }
    }

    [RPC]
    private object Dump(List<object> param)
    {
        var onlyVisibleNode = true;
        if (param.Count > 0)
        {
            onlyVisibleNode = (bool)param[0];
        }
        var sw = new Stopwatch();
        sw.Start();
        var h = dumper.dumpHierarchy(onlyVisibleNode);
        debugProfilingData["dump"] = sw.ElapsedMilliseconds;

        return h;
    }

    [RPC]
    private object Screenshot(List<object> param)
    {
        var sw = new Stopwatch();
        sw.Start();
        
        var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply(false);
        byte[] fileBytes = tex.EncodeToJPG(80);
        var b64img = Convert.ToBase64String(fileBytes);
        debugProfilingData["screenshot"] = sw.ElapsedMilliseconds;
        return new object[] { b64img, "jpg" };
    }

    [RPC]
    private object GetScreenSize(List<object> param)
    {
        return new float[] { Screen.width, Screen.height };
    }

    public void stopListening()
    {
        mRunning = false;
        try
        {
            server?.Stop();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"stopListening Stop() error: {e.Message}");
        }
        try
        {
            this.server?.Dispose();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"stopListening Dispose() error: {e.Message}");
        }
        this.server = null;
    }

    [RPC]
    private object GetDebugProfilingData(List<object> param)
    {
        return debugProfilingData;
    }

    [RPC]
    private object SetText(List<object> param)
    {
        var instanceId = Convert.ToInt32(param[0]);
        var textVal = param[1] as string;
        foreach (var go in GameObject.FindObjectsOfType<GameObject>())
        {
            if (go.GetInstanceID() == instanceId)
            {
                return UnityNode.SetText(go, textVal);
            }
        }
        return false;
    }

    [RPC]
    private object SendMessage(List<object> param)
    {
        if (MessageReceived == null)
        {
            return false;
        }

        var textVal = param[0] as string;

        try
        {
            MessageReceived.Invoke(textVal);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"MessageReceived handler raised: {ex}");
        }

        return true;
    }

    [RPC]
    private object GetSDKVersion(List<object> param)
    {
        return versionCode;
    }

    void Update()
    {
        if (!isInitialized) return;

        // iterate over a stable snapshot to avoid race with server_Received
        var snapshot = inbox.ToArray();
        foreach (var kv in snapshot)
        {
            TcpClientState client = kv.Value;
            try
            {
                List<string> msgs = client.Prot.swap_msgs();
                msgs.ForEach(delegate (string msg)
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var t0 = sw.ElapsedMilliseconds;
                    string response = null;
                    try
                    {
                        response = rpc.HandleMessage(msg);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"rpc.HandleMessage threw: {ex}");
                        response = rpc.formatResponseError(null, null, ex);
                    }
                    var t1 = sw.ElapsedMilliseconds;
                    byte[] bytes = prot.pack(response);
                    var t2 = sw.ElapsedMilliseconds;
                    try
                    {
                        server?.Send(client.TcpClient, bytes);
                    }
                    catch (Exception sendEx)
                    {
                        Debug.LogWarning($"server.Send failed for {client.TcpClient?.Client?.RemoteEndPoint}: {sendEx.Message}");
                    }
                    var t3 = sw.ElapsedMilliseconds;
                    debugProfilingData["handleRpcRequest"] = t1 - t0;
                    debugProfilingData["packRpcResponse"] = t2 - t1;
                    // remove processed client from inbox
                    TcpClientState removed;
                    string tcpClientKey = client.TcpClient.Client.RemoteEndPoint.ToString();
                    inbox.TryRemove(tcpClientKey, out removed);
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error processing inbox entry: {e}");
            }
        }

        try
        {
            vr_support.PeekCommand();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"vr_support.PeekCommand error: {e}");
        }
    }

    void OnApplicationQuit()
    {
        if (!isInitialized) return;
        stopListening();
        isInitialized = false;
        Debug.Log("[PocoManager] OnApplicationQuit: stopped listening.");
    }

    void OnDestroy()
    {
        if (!isInitialized) return;
        stopListening();
        isInitialized = false;
        Debug.Log("[PocoManager] OnDestroy: stopped listening.");
    }

}


public class RPCParser
{
    public delegate object RpcMethod(List<object> param);

    protected Dictionary<string, RpcMethod> RPCHandler = new Dictionary<string, RpcMethod>();
    protected Dictionary<string, (object instance, MethodInfo method)> Listeners = new Dictionary<string, (object, MethodInfo)>();

    private JsonSerializerSettings settings = new JsonSerializerSettings()
    {
        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
    };

    public string HandleMessage(string json)
    {
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, settings);

        if (data.TryGetValue("method", out var methodObj) == false)
        {
            Debug.Log("ignore message without method");
            return null;
        }

        var method = methodObj.ToString();
        var idAction = data.TryGetValue("id", out var id) ? id : null;

        try
        {
            object result;

            switch (method)
            {
                case "Invoke":
                    result = PocoListenerUtils.HandleInvocation(Listeners, data);
                    break;

                default:
                    List<object> param = null;

                    if (data.TryGetValue("params", out var value))
                    {
                        param = ((JArray)value).ToObject<List<object>>();
                    }

                    result = RPCHandler[method](param);

                    break;
            }

            return formatResponse(idAction, result);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return formatResponseError(idAction, null, exception);
        }
    }

    // Call a method in the server
    public string formatRequest(string method, object idAction, List<object> param = null)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["jsonrpc"] = "2.0";
        data["method"] = method;
        if (param != null)
        {
            data["params"] = JsonConvert.SerializeObject(param, settings);
        }
        // if idAction is null, it is a notification
        if (idAction != null)
        {
            data["id"] = idAction;
        }
        return JsonConvert.SerializeObject(data, settings);
    }

    // Send a response from a request the server made to this client
    public string formatResponse(object idAction, object result)
    {
        Dictionary<string, object> rpc = new Dictionary<string, object>();
        rpc["jsonrpc"] = "2.0";
        rpc["id"] = idAction;
        rpc["result"] = result;
        return JsonConvert.SerializeObject(rpc, settings);
    }

    // Send a error to the server from a request it made to this client
    public string formatResponseError(object idAction, IDictionary<string, object> data, Exception e)
    {
        Dictionary<string, object> rpc = new Dictionary<string, object>();
        rpc["jsonrpc"] = "2.0";
        rpc["id"] = idAction;

        Dictionary<string, object> errorDefinition = new Dictionary<string, object>();
        errorDefinition["code"] = 1;
        errorDefinition["message"] = e.ToString();

        if (data != null)
        {
            errorDefinition["data"] = data;
        }

        rpc["error"] = errorDefinition;
        return JsonConvert.SerializeObject(rpc, settings);
    }

    public void addRpcMethod(string name, RpcMethod method)
    {
        RPCHandler[name] = method;
    }

    public void addListener(object instance, string name, MethodInfo method)
    {
        Listeners[name] = (instance, method);
    }
}