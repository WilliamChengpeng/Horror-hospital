using UnityEngine;

public class InteractPrisonDoor : MonoBehaviour, IInteract
{
    public string animName;
    public Animator anim;
    public string Description()
    {
        return "";
    }

    public void OnInteract()
    {
        anim.Play(animName);
        this.GetComponent<BoxCollider>().enabled = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
