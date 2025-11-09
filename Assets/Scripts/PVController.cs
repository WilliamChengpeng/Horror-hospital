using UnityEngine;

public class PVController : MonoBehaviour
{
    public Animator room1Door;
    public Animator room2Door;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            room1Door.Play("Open");
            room2Door.Play("Open");
        }

    }
}
