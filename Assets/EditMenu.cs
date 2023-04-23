using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using UnityEngine;

public class EditMenu : MonoBehaviour
{
    GameObject SourceMenu;
    GameObject MaterialMenu;
    Driver driver;
    public Device activeDevice;
    public int devIndex;
    public int type = 0;
    int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        driver = GameObject.Find("SimulationPanel").GetComponent<Driver>();
        GameObject.Find("Width").GetComponent<Slider>().maxValue = driver.resolution;
        GameObject.Find("Height").GetComponent<Slider>().maxValue = driver.resolution;
        GameObject.Find("xPos").GetComponent<Slider>().maxValue = driver.resolution;
        GameObject.Find("yPos").GetComponent<Slider>().maxValue = driver.resolution;
        SourceMenu = GameObject.Find("Source");
        MaterialMenu = GameObject.Find("Material");
        SourceMenu.SetActive(false);
        MaterialMenu.SetActive(false);
        devIndex = 0;
    }

    void OnEnable(){

    }

    // Update is called once per frame
    void Update()
    {
        driver.devicePos = driver.Devices[devIndex].GetComponent<Device>().pos;
    }
    void FixedUpdate(){
        if(counter%5==0) updateEdit();
    }

    public void enablePanel(int index){
        activeDevice.GetComponent<Device>().type = index;
        if(index == 0){
            SourceMenu.SetActive(false);
            MaterialMenu.SetActive(false);
        }
        if(index == 1){
            SourceMenu.SetActive(false);
            MaterialMenu.SetActive(true);
            Device device = driver.Devices[devIndex].GetComponent<Device>();
            GameObject.Find("Width").GetComponent<Slider>().value = device.width;
            GameObject.Find("WidthText").GetComponent<TMPro.TextMeshProUGUI>().text = "Width: "+ device.width;
            GameObject.Find("Height").GetComponent<Slider>().value = device.height;
            GameObject.Find("HeightText").GetComponent<TMPro.TextMeshProUGUI>().text = "Height: "+ device.height;
            GameObject.Find("U").GetComponent<Slider>().value = device.u;
            GameObject.Find("UText").GetComponent<TMPro.TextMeshProUGUI>().text = "U: "+ device.u;
        }
        if(index == 2){
            SourceMenu.SetActive(true);
            MaterialMenu.SetActive(false);
            Device device = driver.Devices[devIndex].GetComponent<Device>();
            GameObject.Find("Power").GetComponent<Slider>().value = device.source.Mag;
            GameObject.Find("PowerText").GetComponent<TMPro.TextMeshProUGUI>().text = "Power: "+ device.source.Mag;
            GameObject.Find("WaveLength").GetComponent<Slider>().value = device.source.B;
            GameObject.Find("WaveLengthText").GetComponent<TMPro.TextMeshProUGUI>().text = "Wavelength: "+ device.source.B;
            // GameObject.Find("Frequency").GetComponent<Slider>().value = device.source.;
            // GameObject.Find("FreqText").GetComponent<TMPro.TextMeshProUGUI>().text = "U: "+ device.u;
        }
    }

    public void updateEdit(){
        driver = GameObject.Find("SimulationPanel").GetComponent<Driver>();
        if(driver.Devices.Count>0){
            Device device = activeDevice.GetComponent<Device>();
            GameObject.Find("xPos").GetComponent<Slider>().value = device.pos.x;
            GameObject.Find("yPos").GetComponent<Slider>().value = device.pos.y;
            GameObject.Find("Toggle").GetComponent<Toggle>().isOn = device.active;
            GameObject.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>().value = device.type;
            //Material
            if(device.type == 1){
                GameObject.Find("Width").GetComponent<Slider>().value = device.width;
                GameObject.Find("WidthText").GetComponent<TMPro.TextMeshProUGUI>().text = "Width: "+ device.width;
                GameObject.Find("Height").GetComponent<Slider>().value = device.height;
                GameObject.Find("HeightText").GetComponent<TMPro.TextMeshProUGUI>().text = "Height: "+ device.height;
                GameObject.Find("U").GetComponent<Slider>().value = device.u;
                GameObject.Find("UText").GetComponent<TMPro.TextMeshProUGUI>().text = "U: "+ device.u;
            }
            //Source
            if(device.type == 2){
                GameObject.Find("PowerText").GetComponent<TMPro.TextMeshProUGUI>().text = "" + device.source.Mag;
                GameObject.Find("WaveLengthText").GetComponent<TMPro.TextMeshProUGUI>().text = "" + device.source.B;
            }
        }
    }
}
