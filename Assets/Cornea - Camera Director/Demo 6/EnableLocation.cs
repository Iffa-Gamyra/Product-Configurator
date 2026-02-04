using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
public class EnableLocation : MonoBehaviour
{
    public CorneaCameraDirector Cornea;
    public ConfiguratorSettings tank;
    public ConfiguratorSettings seat;
    public ConfiguratorSettings badge;
    public ConfiguratorSettings mannequin;
    public GameObject[] UIcomponents;
    public Button button_R;
    public Button button_1;
    public Button button_2;
    public Button button_3;
    public Button button_4;

    void Awake()
    {
        //get the main Cornea script
        Cornea = GetComponent<CorneaCameraDirector>();
    }

    void Update(){
        // To connect the keys with the buttons 
        if(Input.GetKeyDown(KeyCode.Keypad1)|| Input.GetKeyDown(KeyCode.Alpha1) )
        {
            button_1.onClick.Invoke();
            button_1.Select();

        }

        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            button_2.onClick.Invoke();
            button_2.Select();
        }

        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            button_3.onClick.Invoke();
            button_3.Select();
        }

        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            button_4.onClick.Invoke();
            button_4.Select();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            button_R.onClick.Invoke();
            button_R.Select();
        }

    }

    //Close all the UIs and reset the camera position to the user position

    public void ResetPosition()
    {
        
        UIcomponents = GameObject.FindGameObjectsWithTag("UI");
        foreach (GameObject UIcomponent in UIcomponents)
        {
            UIcomponent.SetActive(false);
        }
        
        Cornea.Lerp.CameraLerp(0);
    }

    public void goToPosition(int pos)
    {
        Cornea.Lerp.CameraLerp(pos);


    }
    public void goToPos1()
    {
        //this method starts the method that lerps the camera to the specified index position
        Cornea.Lerp.CameraLerp(1);

        //Open Bike01 Config settings
        tank.OpenDetailedMode();

    }

    public void goToPos2()
    {
        //this method starts the method that lerps the camera to the specified index position
        Cornea.Lerp.CameraLerp(2);

        //Open Bike02 Config settings
        seat.OpenDetailedMode();
    }

    public void goToPos3()
    {
        //this method starts the method that lerps the camera to the specified index position
        Cornea.Lerp.CameraLerp(3);
    }

    public void goToPos4()
    {
        //this method starts the method that lerps the camera to the specified index position
        Cornea.Lerp.CameraLerp(4);
    }

    public void goToPos5()
    {
        //this method starts the method that lerps the camera to the specified index position
        Cornea.Lerp.CameraLerp(5);
    }
}
