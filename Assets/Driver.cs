using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UI;
public class Driver : MonoBehaviour {
    // Start is called before the first frame update
    LineRenderer Hline, Eline;
    FDTD[] HFields, EFields;
    public ComputeShader Compute;
    public RenderTexture render;

    public int Width, Height = 10;

    int NUM_FDTD = 100;
    Image[] imgs;
    int counter;
    public float timeStep, C, u, e, Mh, Me, time, TProp, dist, add;

    float E3, E2, E1, H3, H2, H1 = 0;
    public bool shouldUpdate = true;
    void Start() {

        NUM_FDTD = Width*Height;
        add =0;
        EFields = new FDTD[NUM_FDTD];
        HFields = new FDTD[NUM_FDTD];
        
        print(C);
        time = 0;
        timeStep = Mathf.Pow(1*10,-10);
        
        Mh = C*timeStep/(1+u);
        Me = C*timeStep/(1-e);
        TProp = Mh*NUM_FDTD*timeStep/C;
        dist = 1f;
//        GameObject.Find("Distance").GetComponent<Text>().text = "" + NUM_FDTD*dist;

        for(int i=0; i< NUM_FDTD; i++){
            //Create 600 FDTD cells
            FDTD h = new FDTD();
            FDTD e = new FDTD();
            HFields[i] = h;
            EFields[i] = e;
            //Coef
            // HFields[i].coef = Mh;
            // EFields[i].coef = Me;
            // HFields[i].coef = 1;
            // EFields[i].coef = 1;
        }
        for(int i=300; i< NUM_FDTD; i++){
            //Coef
            //HFields[i].coef = .1f;
            //EFields[i].coef = .5f;
        }
        if(timeStep<(1/C*Mathf.Sqrt(1/dist))){
            print("T is good");
        }
        else{
            print("T too high. Should be < "+ (1/C*Mathf.Sqrt(1/dist)));
        }

        print("TPROP: " + TProp);
        print("DistStep: " + dist);
        print("TimeStep: " + timeStep);
        setupGrid();
    }

    public void setupGrid() {
        GameObject grid = GameObject.Find("GridCanvas");
        RectTransform ImgRect = GameObject.Find("SimWindow").GetComponent<RectTransform>();
        render = new RenderTexture((int)ImgRect.rect.width,(int)ImgRect.rect.height,255);
        render.enableRandomWrite = true;
        render.Create();

        GameObject.Find("SimWindow").GetComponent<RawImage>().texture = render;

        Compute.SetTexture(0,"Result",render);
        Compute.Dispatch(0,render.width/8,render.height/8,1);


        Graphics.Blit(GameObject.Find("SimWindow").GetComponent<RawImage>().texture, render);

        // for(int i = 0; i<NUM_FDTD; i++){
        //     HFields[i].Position = Vector3.zero;
        //     EFields[i].Position = Vector3.zero;
        //     Hline.SetPosition(i,(new Vector3((-Screen.width/2)+Screen.width * (i+1)/NUM_FDTD,0,-0.01f)));
        //     Eline.SetPosition(i,(new Vector3((-Screen.width/2)+Screen.width * (i+1)/NUM_FDTD,0,-0.01f)));
        // }
        //Reset Boundry
        H3=0; H2=0; H1=0; E3=0; E2=0; E1 = 0;
    }
    // Update is called once per frame
    // void Update() {
    // }

    void FixedUpdate(){
        add/=1.5f;
        if(shouldUpdate){
            counter++;
            time = (float)counter*(float)timeStep;
            //Update H from E
            //updateHField();
            //Update E From H
            //updateEField();
            //EFields[0].Position.y = add;
            debug();
        }
        if (counter>100){
            counter=0;
            RectTransform ImgRect = GameObject.Find("SimWindow").GetComponent<RectTransform>();
            render = new RenderTexture((int)ImgRect.rect.width,(int)ImgRect.rect.height,255);
            render.enableRandomWrite = true;
            render.Create();

            GameObject.Find("SimWindow").GetComponent<RawImage>().texture = render;

            Compute.SetTexture(0,"Result",render);
            Compute.Dispatch(0,render.width/8,render.height/8,1);


            Graphics.Blit(GameObject.Find("SimWindow").GetComponent<RawImage>().texture, render);
        }
    }


    void updateHField(){
        for(int i=0; i<NUM_FDTD-1;i++){
            // Update Equation for Every HField
            HFields[i].Position.x = HFields[i].Position.x + (HFields[i].coef * ((EFields[i+1].Position.y - EFields[i].Position.y))/dist);

            Vector3 pos = Hline.GetPosition(i);
            pos.y = HFields[i].Position.x;
            Hline.SetPosition(i,pos);
        }
        HFields[NUM_FDTD-1].Position.x += (HFields[NUM_FDTD-1].coef * (E3 - EFields[NUM_FDTD-1].Position.y));
        H3=H2; H2=H1; H1=HFields[0].Position.x;

    }
    void updateEField(){
        EFields[0].Position.y += EFields[0].coef * (HFields[0].Position.y - E3);
        for(int i=1; i<NUM_FDTD-1; i++){   
            //Update Equation for Every EField
            EFields[i].Position.y = EFields[i].Position.y + (EFields[i].coef * (HFields[i].Position.x - HFields[i-1].Position.x)/dist);
            Vector3 pos = Eline.GetPosition(i);
            pos.y = EFields[i].Position.y;
            Eline.SetPosition(i,pos);
        }
        E3=E2;E2=E1;E1=EFields[NUM_FDTD-1].Position.y;
    }

    public void AddDevice(){
        GameObject go = GameObject.Find("Panel");
        Rect r = go.GetComponent<Image>().GetPixelAdjustedRect();
        Color a = go.GetComponent<Image>().color;
        a.a = .4f;
        go.GetComponent<Image>().color = a;
        print(r.xMin + ", "+ r.xMax);
        for(int i =0; i<NUM_FDTD-1; i++){
            if(r.xMin < Hline.GetPosition(i).x && Hline.GetPosition(i).x < r.xMax){
                HFields[i].coef = .2f;
            }
        }
    }
    
    public void setSim(bool r){
        shouldUpdate = r;
        if(!r){
            GameObject.Find("StatusText").GetComponent<Text>().text = "Status: Paused";
            GameObject.Find("StatusText").GetComponent<Text>().color = Color.yellow;
        }
        else{
            GameObject.Find("StatusText").GetComponent<Text>().text = "Status: Running";
            GameObject.Find("StatusText").GetComponent<Text>().color = Color.green;

        }
    }

    void debug(){
        if(Input.GetKey(KeyCode.Q)){
            //HFields[0].Position.x = 40;
            add += 50;
        }
        else{
            
        }
        if(Input.GetKeyDown(KeyCode.P)){
            EFields[10].Position.y = 70;
        }
    }
}
