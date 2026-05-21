using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Fusion;

public class PythonSocket : NetworkBehaviour
{
    public int port = 25002;

    private TcpListener _listener;
    private TcpClient _client;
    private NetworkStream _stream;
    private Thread _thread;

    public Action<string> OnJsonReceived;

    public override void Spawned()
    {
        // Only start the socket on the instance that has State Authority
        if (HasStateAuthority)
        {
            _thread = new Thread(ListenLoop)
            {
                IsBackground = true
            };
            _thread.Start();
            Debug.Log("[PythonSocket] Spawned and listener thread started");
        }
    }

    private void ListenLoop()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Debug.Log($"[PythonSocket] Listening on port {port}");

            _client = _listener.AcceptTcpClient();
            _stream = _client.GetStream();

            byte[] buffer = new byte[4096];

            while (true)
            {
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0)
                {
                    Thread.Sleep(10);
                    continue;
                }

                string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                OnJsonReceived?.Invoke(json);

                // send minimal response (python expects something)
                string response = "{\"buttonPressed\": false}";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                _stream.Write(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (SocketException e)
        {
            Debug.LogError($"[PythonSocket] Socket exception: {e.Message}");
        }
        catch (ThreadAbortException e) // expected on destroy
        {
            Debug.Log($"[PythonSocket] Thread aborted. ({e.Message})");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PythonSocket] {e}: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (!HasStateAuthority) 
        {
            return;
        }

        _thread?.Abort();
        _stream?.Close();
        _client?.Close();
        _listener?.Stop();

        Debug.Log("[PythonSocket] Closed listener and thread");
    }
}
