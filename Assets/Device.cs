using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Device : MonoBehaviour
{
    public Vector2 pos = new Vector2(0,0);
    public bool active = false;
    GameObject editMenu;
    Driver driver;
    public int type = 0;
    public float u;
    public float width;
    public float height;
    public float freq;
    public float wl;

    public Source source;

    void Start()
    {
        type = 0;
        driver = GameObject.Find("SimulationPanel").GetComponent<Driver>();
        editMenu = GameObject.Find("SimulationPanel").GetComponent<Driver>().editMenu;
        pos = new Vector2((int)GameObject.Find("SimulationPanel").GetComponent<Driver>().resolution/2,(int)GameObject.Find("SimulationPanel").GetComponent<Driver>().resolution/2);
        u = 5;
        width = 50;
        height = 50;
        source = new Source(pos, 1, 1,1);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(editMenu.activeSelf){
            if(driver.Devices[editMenu.GetComponent<EditMenu>().devIndex] == gameObject){
                gameObject.GetComponent<Image>().color = Color.green;
                active = GameObject.Find("Toggle").GetComponent<Toggle>().isOn;
                print(active);
                type = GameObject.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>().value;
                pos = new Vector2(GameObject.Find("xPos").GetComponent<Slider>().value,GameObject.Find("yPos").GetComponent<Slider>().value);
                if(type == 1){
                    width = GameObject.Find("Width").GetComponent<Slider>().value;
                    height = GameObject.Find("Height").GetComponent<Slider>().value;
                    u = GameObject.Find("U").GetComponent<Slider>().value;
                }
                if(type == 2){
                    source.pos = pos;
                    source.Mag = GameObject.Find("Power").GetComponent<Slider>().value;
                    source.B = GameObject.Find("WaveLength").GetComponent<Slider>().value;
                }
            }
            else{
                gameObject.GetComponent<Image>().color = Color.grey;
            }
        }
        else{
            gameObject.GetComponent<Image>().color = Color.grey;
        }
    }


    public void delete(){
        GameObject.Find("SimulationPanel").GetComponent<Driver>().Devices.Remove(gameObject);
        Destroy(gameObject);
        editMenu.GetComponent<EditMenu>().devIndex = -1;
        editMenu.SetActive(false);
        GameObject content = GameObject.Find("Content");
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x,gameObject.GetComponent<RectTransform>().rect.height * content.transform.childCount);
    }

    public void edit(){
        editMenu.SetActive(true);
        editMenu.GetComponent<EditMenu>().activeDevice = gameObject.GetComponent<Device>();
        editMenu.GetComponent<EditMenu>().devIndex = GameObject.Find("SimulationPanel").GetComponent<Driver>().Devices.IndexOf(gameObject);
        editMenu.GetComponent<EditMenu>().updateEdit();
    }

    public void updateDeviceValues(){
        if(active){
            int resolution = (int)GameObject.Find("SimulationPanel").GetComponent<Driver>().resolution;
            if(type == 1){
                for(int w = (int)pos.x - (int)width/2; w < pos.x+ (int)(width/2); w++){
                    for(int h = (int)pos.y - (int)height/2; h < pos.y+(height/2); h++){
                        if(w>=resolution || w<0) continue;
                        if(h>=resolution || h<0) continue;
                        GameObject.Find("SimulationPanel").GetComponent<Driver>().HFields[w+h*resolution].coef = u;
                    }
                }
            }
            if(type == 2){
                //Source Logic
            }
        }
        
    }

}
