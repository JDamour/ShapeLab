using UnityEngine;
using WebSocketSharp;

public class StatefulMain : MonoBehaviour {
    public StateMachine stateMachine;

    private WebSocket ws;

    void Awake()
    {
        ws = new WebSocket("ws://echo.websocket.org");
        //ws = new WebSocket("ws://127.0.0.1:3004/");
        //ws = new WebSocket("ws://141.64.64.251/websocket");

        ws.OnOpen += OnOpenHandler;
        ws.OnMessage += OnMessageHandler;
        ws.OnClose += OnCloseHandler;

        stateMachine.AddHandler(State.Running, () => {
            new Wait(this, 3, () => {
                ws.ConnectAsync();
            });
        });

        stateMachine.AddHandler(State.Recover, () => {
            Debug.Log("trying to recover...");
            new Wait(this, 3, () => {
                ws.ConnectAsync();
            });
        });

        
        stateMachine.AddHandler(State.LongRecovery, () => {
            new Wait(this, 60, () => {
                stateMachine.Transition(State.Recover);
            });
        });

        stateMachine.AddHandler(State.Terminate, () => {
            new Wait(this, 3, () => {
                ws.CloseAsync();
            });
        });
    }

    void Start() {
        

        stateMachine.Run();
    }

    private void OnOpenHandler(object sender, System.EventArgs e) {
        Debug.Log("WebSocket connected!");
        stateMachine.Transition(State.Connected);
    }

    private void OnMessageHandler(object sender, MessageEventArgs e) {
        Debug.Log("WebSocket server said: " + e.Data);
        switch (e.Data)
        {
            case "terminate":
                {
                    stateMachine.Transition(State.Terminate);
                }
                break;
            default:
                break;

        }
        //stateMachine.Transition(State.Pong);
    }

    private void OnCloseHandler(object sender, CloseEventArgs e) {
        Debug.Log("WebSocket closed with reason: " + e.Reason+"(code:"+e.Code+")");
        if (e.Code.Equals(1006))
        {
            stateMachine.Transition(State.LongRecovery);
        }
        else {
            Debug.Log("Remote Server killed Connection, retry in 1 minute");
            stateMachine.Transition(State.Recover);
        }
            
        
    }

    private void OnSendComplete(bool success) {
        Debug.Log("Message sent successfully? " + success);
    }

    private void OnErrorHandler(object sender, ErrorEventArgs e)
    {
        Debug.Log("An error occoured:" + e.Message);
        stateMachine.Transition(State.Recover);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ended, killing socket");
        stateMachine.Transition(State.Terminate);
    }
}
