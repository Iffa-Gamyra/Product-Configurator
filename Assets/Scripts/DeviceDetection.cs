using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeviceDetection : MonoBehaviour
{
   
    public GameObject desktopUI;
    public GameObject mobileUI;

    public bool isMac;
  


    // Start is called before the first frame update
    void OnEnable()
    {
        if (SystemInfo.operatingSystem.Contains("iPad"))
        {

            mobileUI.SetActive(false);
            desktopUI.SetActive(true);

        }
        else if (SystemInfo.operatingSystem.Contains("Mac"))
        {
            mobileUI.SetActive(false);
            desktopUI.SetActive(true);

        }
        else if(SystemInfo.operatingSystem.Contains("iPhone"))
        {

            mobileUI.SetActive(true);
            desktopUI.SetActive(false);

        }
        else if(SystemInfo.operatingSystem.Contains("Windows"))
        {

            mobileUI.SetActive(false);
            desktopUI.SetActive(true);

        }
        else if (SystemInfo.operatingSystem.Contains("Android"))
        {

            mobileUI.SetActive(true);
            desktopUI.SetActive(false);
        }
      
    }

    // Update is called once per frame
    void Update()
    {

     /*   if (isMac)
        {
            if (Input.touchCount > 0)
            {
                isMac = false;
                mobileUI.SetActive(true);
                desktopUI.SetActive(false);

            }
            else
            {
                mobileUI.SetActive(false);
                desktopUI.SetActive(true);
            }
        }*/
    }
}
