using UnityEngine;

public class PVController : MonoBehaviour
{
    public Animator room1Door;
    public Animator room2Door;
    public Animator room3Door;
    public Animator room4Door;
    public Animator room5Door;
    public Animator room6Door;
    public Animator room7Door;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            room1Door.Play("Open");
           // room2Door.Play("Open2");
            //room3Door.Play("Open3");
            //room4Door.Play("Open4");
            //room5Door.Play("Open5");
            room6Door.Play("Open6");
            room7Door.Play("Open7");
        }

    }
}
