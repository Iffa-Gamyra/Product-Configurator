using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfiguratorSettings : MonoBehaviour
{

    [Header("Rotate Objects")]
    public int rotationSpeed;
    private bool rotationEnabled = false;

    [Header("Naviagtion")]
    public float navigationSpeed = 5.0f;
    private bool navigationEnabled = true;

    [Header("Original Position Slerp")]
    public float SlerpSpeed = -1f;
    public GameObject ObjectToRotate;
    public Quaternion originalRot;
    public bool original = false;
    

    // Start is called before the first frame update
    void Start()
    {
        navigationSpeed = 5.0f;
        if (ObjectToRotate != null)
        {
            originalRot = ObjectToRotate.transform.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(rotationEnabled)
        {
            if (Input.GetMouseButton(0))
            {
                transform.eulerAngles += rotationSpeed * new Vector3(0, -Input.GetAxis("Mouse X"), 0);
            }
        }

        if (navigationEnabled)
        {
            //Move Forward
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                transform.Translate(Vector3.forward * Time.deltaTime * navigationSpeed);
            }
            //Move backwards
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                transform.Translate(-1 * Vector3.forward * Time.deltaTime * navigationSpeed);
            }
            // Rotate Left
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Rotate(0, -0.5f, 0);
            }
            // Rotate right
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                transform.Rotate(0, 0.5f, 0);
            }
        }

        if (original == true)
        {
            ObjectToRotate.transform.rotation = Quaternion.Slerp(ObjectToRotate.transform.rotation, originalRot, Time.deltaTime * 0.5f);

            if ((Quaternion.Dot(ObjectToRotate.transform.rotation, originalRot) > 0.9999f))
            {
                original = false;

            }

        }

    }

    public void OpenDetailedMode()
    {

        //Disable player movement 
        navigationEnabled = false;

        //Enable the bike movement
        rotationEnabled = true;
    }

    public void CloseDetailedMode()
    {
        //Enable player movement 
        navigationEnabled = true;

        //Disable the bike movement
        rotationEnabled = false;

        //Rotate the bike to the original position
        original = true;

    }
    public void ChangeDetailedMode()
    {
        //Disable the bike movement
        rotationEnabled = false;

        //Rotate the bike to the original position
        original = true;
    }

    public void OriginalRotation()
    {
        original = true;
    }
}
