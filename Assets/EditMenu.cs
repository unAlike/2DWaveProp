using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class EditMenu : MonoBehaviour
{
    GameObject SourceMenu;
    GameObject MaterialMenu;
    Driver driver;
    public int devIndex;
    public int type = 0;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void enablePanel(int index){
        if(index == 0){
            SourceMenu.SetActive(false);
            MaterialMenu.SetActive(false);
        }
        if(index == 1){
            SourceMenu.SetActive(true);
            MaterialMenu.SetActive(false);
        }
        if(index == 2){
            SourceMenu.SetActive(false);
            MaterialMenu.SetActive(true);
        }
    }

    public void updateEdit(){
        Device device = driver.Devices[devIndex].GetComponent<Device>();
        print(devIndex);
        print(device.pos);
        GameObject.Find("xPos").GetComponent<Slider>().value = device.pos.x;
        GameObject.Find("yPos").GetComponent<Slider>().value = device.pos.y;
        // if(device.type == 1){
        //     GameObject.Find("Width").GetComponent<Slider>().value = device.width;
        //     GameObject.Find("Height").GetComponent<Slider>().value = device.height;
        // }

    }

    public void apply(){
        GameObject d = driver.Devices[devIndex];
        // if(d.GetComponent<Device>().type != 0){
        //     d.GetComponent<Device>().width = GameObject.Find("width").GetComponent<Slider>().value;
        //     d.GetComponent<Device>().height = GameObject.Find("height").GetComponent<Slider>().value;
        // }
        d.GetComponent<Device>().pos = new Vector2(GameObject.Find("xPos").GetComponent<Slider>().value,GameObject.Find("yPos").GetComponent<Slider>().value);
        driver.Devices[devIndex] = d;
        print(d.GetComponent<Device>().u);
        driver.updateDeviceValues();
    }
    


}
