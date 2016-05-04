using UnityEngine;
using WebSocketSharp;

public class StatefulMain : MonoBehaviour
{
    public StateMachine stateMachine;
    public VoxelManager voxelmanager;

    public enum Command
    {
        RESETALL,
        RESETTOOLS,
        RESETSCREENSHOTS,
        NEXTUSER,
        TAKESCREENSHOT,
        UNKOWN
    }

    private WebSocket ws;
    public class ShapeLabProtocoll
    {
        public static Command parseMessage(string msg)
        {
            switch (msg.ToLower())
            {
                case "resetall":
                    return Command.RESETALL;
                case "resettools":
                    return Command.RESETTOOLS;
                case "resetscreenshots":
                    return Command.RESETSCREENSHOTS;
                case "next":
                case "nextuser":
                    return Command.NEXTUSER;
                case "screenshot":
                case "takescreenshot":
                    return Command.TAKESCREENSHOT;
                default:
                    return Command.UNKOWN;
            }
        }
    }

    void Awake()
    {
        //ws = new WebSocket("ws://echo.websocket.org");
        ws = new WebSocket("ws://127.0.0.1:8080/");
        //ws = new WebSocket("ws://141.64.64.251/websocket");

        ws.OnOpen += OnOpenHandler;
        ws.OnMessage += OnMessageHandler;
        ws.OnClose += OnCloseHandler;

        //----FOR TESTING-----
        stateMachine.AddHandler(State.Connected, () => {
            new Wait(this, 3, () => { // 3sec after connecting, send "testrun" to server
                Debug.Log("running test sequence...");
                ws.Send("testrun");
            });
        });
        //---------

        stateMachine.AddHandler(State.Running, () => {
            new Wait(this, 3, () => {
                ws.ConnectAsync();
            });
        });

        stateMachine.AddHandler(State.Recover, () => {
            Debug.Log("trying to recover connection...");
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

    void Start()
    {


        stateMachine.Run();
    }

    private void OnOpenHandler(object sender, System.EventArgs e)
    {
        Debug.Log("WebSocket connected to " + ws.Url + "!");
        stateMachine.Transition(State.Connected);
    }

    private void OnMessageHandler(object sender, MessageEventArgs e)
    {
        //Debug.Log("WebSocket server said: " + e.Data);
        Command cmd = ShapeLabProtocoll.parseMessage(e.Data);
        if (cmd.Equals(Command.UNKOWN))
        {
            //Debug.Log("Unable to parse recived command");
            //todo send warning to server
        }
        else
        {
            //Debug.Log("queueing command: " + cmd.ToString());
            voxelmanager.queueServerCommand(cmd);
        }
    }

    private void OnCloseHandler(object sender, CloseEventArgs e)
    {
        Debug.Log("WebSocket closed with reason: " + e.Reason + "(code:" + e.Code + ")");
        if (e.Code.Equals(1006))
        {
            stateMachine.Transition(State.LongRecovery);
        }
        else
        {
            Debug.Log("Remote Server killed Connection, retry in 1 minute");
            stateMachine.Transition(State.Recover);
        }


    }

    private void OnSendComplete(bool success)
    {
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
