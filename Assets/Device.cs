using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    public Vector2 pos = new Vector2(0,0);
    bool active = false;
    GameObject editMenu;
    public int type = 0;
    public float u = 5;
    public float width = 50;
    public float height = 50;
    public float freq;
    public float wl;

    void Start()
    {
        editMenu = GameObject.Find("SimulationPanel").GetComponent<Driver>().editMenu;
        u = 5;
        width = 50;
        height = 50;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void delete(){
        GameObject.Find("SimulationPanel").GetComponent<Driver>().Devices.Remove(gameObject);
        Destroy(gameObject);
        editMenu.SetActive(false);
        GameObject content = GameObject.Find("Content");
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x,gameObject.GetComponent<RectTransform>().rect.height * content.transform.childCount);
    }

    public void edit(){
        editMenu.SetActive(true);
        editMenu.GetComponent<EditMenu>().devIndex = GameObject.Find("SimulationPanel").GetComponent<Driver>().Devices.IndexOf(gameObject);
        editMenu.GetComponent<EditMenu>().updateEdit();
    }

}
