using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UI;
public class Driver : MonoBehaviour {
    // Start is called before the first frame update
    LineRenderer Hline, Eline;
    FDTD[] HFields, EFields;
    public ComputeShader SimCompute, CellsCompute;
    public RenderTexture render;

    public int Width, Height = 10;

    int NUM_FDTD = 100;
    Image[] imgs;
    int counter;
    public float resolution;
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

            HFields[i].Position = Vector3.zero;
            HFields[i].Color = Random.ColorHSV();
            HFields[i].coef = 37f;
            HFields[i].time = 0;
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
        render = new RenderTexture(Width,Height,1);
        render.enableRandomWrite = true;
        render.filterMode = FilterMode.Point;
        render.Create();
        GameObject.Find("SimWindow").GetComponent<RawImage>().texture = render;
    }

    void Update(){
        debug();
    }
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
        }
        if (true){
            //randomizeColors();
            counter=0;
            ComputeBuffer col = new ComputeBuffer(HFields.Length,sizeof(float)*9);
            col.SetData(HFields);
            SimCompute.SetBuffer(0, "cells", col);
            SimCompute.SetTexture(0,"Result",render);
            SimCompute.SetFloat("resolution", resolution);
            SimCompute.SetFloat("time", counter);
            SimCompute.Dispatch(0,render.width,render.height,1);

            col.Release();

            Graphics.Blit(GameObject.Find("SimWindow").GetComponent<RawImage>().texture, render);
        }
    }


    // void updateHField(){
    //     for(int i=0; i<NUM_FDTD-1;i++){
    //         // Update Equation for Every HField
    //         HFields[i].Position.x = HFields[i].Position.x + (HFields[i].coef * ((EFields[i+1].Position.y - EFields[i].Position.y))/dist);

    //         Vector3 pos = Hline.GetPosition(i);
    //         pos.y = HFields[i].Position.x;
    //         Hline.SetPosition(i,pos);
    //     }
    //     HFields[NUM_FDTD-1].Position.x += (HFields[NUM_FDTD-1].coef * (E3 - EFields[NUM_FDTD-1].Position.y));
    //     H3=H2; H2=H1; H1=HFields[0].Position.x;

    // }
    // void updateEField(){
    //     EFields[0].Position.y += EFields[0].coef * (HFields[0].Position.y - E3);
    //     for(int i=1; i<NUM_FDTD-1; i++){   
    //         //Update Equation for Every EField
    //         EFields[i].Position.y = EFields[i].Position.y + (EFields[i].coef * (HFields[i].Position.x - HFields[i-1].Position.x)/dist);
    //         Vector3 pos = Eline.GetPosition(i);
    //         pos.y = EFields[i].Position.y;
    //         Eline.SetPosition(i,pos);
    //     }
    //     E3=E2;E2=E1;E1=EFields[NUM_FDTD-1].Position.y;
    // }


    public void AddDevice(){
        // TODO
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

    void changeColor(){
        for(int i =0; i<NUM_FDTD; i++){
            HFields[i].Color = Random.ColorHSV();
        }
    }

    void debug(){
        if(Input.GetKey(KeyCode.Q)){
            changeColor();
        }
        if(Input.GetKeyDown(KeyCode.W)){
            for(int i=0; i<HFields.Length;i++){
                if(HFields[i].Color != Color.green){ 
                    HFields[i].Color = Color.green;
                    break;
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.P)){
            print("Num FDTD: " + NUM_FDTD);
            print("Width: "+Width + "   Height: " + Height);
        }
        if(Input.GetKeyDown(KeyCode.B)){
            for(int i =0; i<HFields.Length;i++){
                HFields[i].Color = Color.black;
            }
        }
    }

    public void UI(){
        Vector2 pos;
        RectTransform r = GameObject.Find("SimWindow").GetComponent<RectTransform>();
        pos = Input.mousePosition;
        pos.x = Mathf.FloorToInt(pos.x*2/resolution);
        pos.y = Mathf.FloorToInt(pos.y*2/resolution);
        print(pos);
        print(Mathf.FloorToInt(pos.x)+Mathf.FloorToInt(pos.y)*resolution);
        print(HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Color);
        HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Color = Color.red;
    }

}
