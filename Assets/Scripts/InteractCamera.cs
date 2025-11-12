using UnityEngine;

public class InteractCamera : MonoBehaviour, IInteract
{
    public string showText;
    public string Description()
    {
        return showText;
    }

    public void OnInteract()
    {
        //throw new System.NotImplementedException();
        this.gameObject.SetActive ( false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
