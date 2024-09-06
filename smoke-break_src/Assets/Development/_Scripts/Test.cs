using UnityEngine;

public class Test : MonoBehaviour
{
    // Methods
    private void Awake()
    {
        print("Awake is called when the script instance is being loaded.");
    }

    private void Start()
    {
        print("Start is called before the first frame update");
    }

    private void Update()
    {
        print("Update is called once per frame");
    }

    private void FixedUpdate()
    {
        print("This function is called every fixed framerate frame.");
    }
}
