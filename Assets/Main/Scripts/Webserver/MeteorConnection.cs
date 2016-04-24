using UnityEngine;
using Net.DDP.Client;

public class MeteorConnection : MonoBehaviour {
    private IDataSubscriber subscriber = new Subscriber();

    private DDPClient client;

    // Use this for initialization
    void Start () {

        //Debug.Log(System.Environment.Version);

        ////subscriber is an instance of IDataSubscriber, which gets a callback
        ////when a change comes in
        //DDPClient client = new DDPClient(subscriber);

        //// you can't use localhost in the simulator!
        //client.Connect("192.168.0.1:3000");
        //client.Subscribe("allMovies");

        //client.Call("helloWorld");

        //Debug.Log(subscriber);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
