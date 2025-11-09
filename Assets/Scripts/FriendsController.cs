using UnityEngine;

public class FriendsController : MonoBehaviour
{
    public enum FriendsState
    {
        // È°Ëµ 
        // ÒýÂ·
    }

    public Animator ac;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ac.Play("pulll");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ac.Play("SneakWalk");
        }
    }
}
