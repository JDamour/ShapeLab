using UnityEngine;
using WebSocketSharp;

public class StatefulMain : MonoBehaviour
{
    public StateMachine stateMachine;
    public VoxelManager voxelmanager;
    public string hostadresse;
    public enum Command
    {
        RESET_ALL,
        RESET_TOOLS,
        RESET_SCREENSHOTS,
        RESET_HMD_LOCATION,
        NEXT_USER,
        TAKE_SCREENSHOT,
        DELETE_LAST_SCREENSHOT,
        MAX_TIME_3_MINUTES,
        MAX_TIME_5_MINUTES,
        MAX_TIME_UNLIMITED,
        UNKNOWN
    }
    private string serverID;
    private WebSocket ws;
    public class ShapeLabProtocoll
    {
        public static Command parseMessage(string msg)
        {
            switch (msg)
            {
                case "reset-all":
                    return Command.RESET_ALL;
                case "reset-tools":
                    return Command.RESET_TOOLS;
                case "reset-screenshots":
                    return Command.RESET_SCREENSHOTS;
                case "next-user":
                    return Command.NEXT_USER;
                case "take-screenshot":
                    return Command.TAKE_SCREENSHOT;
                case "resetoculusposition":
                    return Command.RESET_HMD_LOCATION;
                case "removelastscreenshot":
                    return Command.DELETE_LAST_SCREENSHOT;
                case "setmaxtime 3:00":
                    return Command.MAX_TIME_3_MINUTES;
                case "setmaxtime 5:00":
                    return Command.MAX_TIME_5_MINUTES;
                case "setnocountdown":
                    return Command.MAX_TIME_UNLIMITED;
                default:
                    return Command.UNKNOWN;
            }
        }
    }

    void Awake()
    {
        //ws = new WebSocket("ws://echo.websocket.org");
        //ws = new WebSocket("ws://127.0.0.1:8080/");
        //ws = new WebSocket("ws://shapelab.kasanzew.de:8080/");
        //ws = new WebSocket("ws://141.64.64.251/websocket");
        ws = new WebSocket(hostadresse);

        ws.OnOpen += OnOpenHandler;
        ws.OnMessage += OnMessageHandler;
        ws.OnClose += OnCloseHandler;
        /*
        //----FOR TESTING-----
        stateMachine.AddHandler(State.Connected, () =>
        {
            new Wait(this, 3, () =>
            { // 3sec after connecting, send "testrun" to server
                Debug.Log("running test sequence...");
                ws.Send("testrun");
            });
        });
        //---------
        */
        stateMachine.AddHandler(State.Running, () =>
        {
            new Wait(this, 3, () =>
            {
                ws.ConnectAsync();
            });
        });

        stateMachine.AddHandler(State.Recover, () =>
        {
            Debug.Log("trying to recover connection...");
            new Wait(this, 3, () =>
            {
                ws.ConnectAsync();
            });
        });

        stateMachine.AddHandler(State.LongRecovery, () =>
        {
            new Wait(this, 60, () =>
            {
                stateMachine.Transition(State.Recover);
            });
        });

        stateMachine.AddHandler(State.Terminate, () =>
        {
            new Wait(this, 3, () =>
            {
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
        Debug.Log("WebSocket connected to " + ws.Url);
        stateMachine.Transition(State.Connected);
    }

    private void OnMessageHandler(object sender, MessageEventArgs e)
    {
        // Debug.Log("WebSocket server said: " + e.Data);
        Command cmd = ShapeLabProtocoll.parseMessage(e.Data);
        if (cmd.Equals(Command.UNKNOWN))
        {
            if (e.Data.Contains("clientID\":\""))
            {
                //erste Meldung des Servers mit Id
                serverID = e.Data.Substring(13).Replace("\"}", "");
                voxelmanager.setSessionID(serverID);
                Debug.Log("My ID is: " + serverID);
            } else
            {
                Debug.Log(cmd.ToString()+ " received: " + e.Data);
            }
        }
        else
        {
            //Debug.Log("queuing command: " + cmd.ToString());
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
        Debug.Log("An error occurred:" + e.Message);
        stateMachine.Transition(State.Recover);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ended, killing socket");
        stateMachine.Transition(State.Terminate);
    }
}
