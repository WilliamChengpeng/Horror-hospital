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
            room2Door.Play("Open");
            room3Door.Play("Open");
            room4Door.Play("Open");
            room5Door.Play("Open");
            room6Door.Play("Open");
            room7Door.Play("Open");
        }

    }
}
