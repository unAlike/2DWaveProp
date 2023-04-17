using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    Vector2 pos = new Vector2(-1,-1);
    bool active = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void delete(){
        Destroy(gameObject);
        GameObject content = GameObject.Find("Content");
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x,gameObject.GetComponent<RectTransform>().rect.height * content.transform.childCount);
    }

    public void edit(){
        GameObject editMenu = GameObject.Find("EditMenu");
    }

}
