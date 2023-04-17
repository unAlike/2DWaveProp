using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditMenu : MonoBehaviour
{
    GameObject SourceMenu;
    GameObject MaterialMenu;
    int type = 0;
    // Start is called before the first frame update
    void Start()
    {
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

}
