using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UI;
public class Driver : MonoBehaviour {
    // Start is called before the first frame update
    LineRenderer Hline, Eline;
    FDTD[] HFields, EFields;

    FDTD[,] test;
    public ComputeShader SimCompute, CellsCompute;
    public RenderTexture render;
    public int EBrightness, HBrightness = 100;
    int Width, Height = 10;

    int NUM_FDTD = 100;
    Image[] imgs;
    int counter;
    int view = 0;
    public float resolution;
    public float timeStep, C, u, e, Mh, Me, time, TProp, dist, add;

    float E3, E2, E1, H3, H2, H1 = 0;
    public bool shouldUpdate = true;

    Vector2 selectedCell = new Vector2(0,0);
    void Start() {
        GameObject.Find("HBright").GetComponent<Slider>().value = HBrightness;
        GameObject.Find("EBright").GetComponent<Slider>().value = EBrightness;
        add = 0;
        C = 299792458;
        print(C);
        time = 0;
        //timeStep = Mathf.Pow(1*10,-10);
        
        Mh = C*timeStep/(1+u);
        Me = C*timeStep/(1-e);
        TProp = Mh*NUM_FDTD*timeStep/C;
        
//        GameObject.Find("Distance").GetComponent<Text>().text = "" + NUM_FDTD*dist;
        setupCells();
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
    public void setupCells(){
        Width = (int)resolution;
        Height = (int)resolution;
        NUM_FDTD = Width*Height;
        EFields = new FDTD[NUM_FDTD];
        HFields = new FDTD[NUM_FDTD];

        for(int i=0; i< NUM_FDTD; i++){
            //Create 600 FDTD cells
            FDTD h = new FDTD();
            FDTD ef = new FDTD();
            HFields[i] = h;
            EFields[i] = ef;

            HFields[i].Position = new Vector3(0,0,0);
            //HFields[i].Curl = new Vector3(0,0,0);
            HFields[i].Color = Random.ColorHSV();
            HFields[i].coef = u;

            EFields[i].Position = new Vector3(0,0,0);
            //EFields[i].Curl = new Vector3(0,0,0);
            EFields[i].Color = Random.ColorHSV();
            EFields[i].coef = e;
        }
        selectedCell = new Vector2(1,1);

    }
    public void setupGrid() {

        GameObject grid = GameObject.Find("GridCanvas");
        RectTransform ImgRect = GameObject.Find("SimWindow").GetComponent<RectTransform>();
        render = new RenderTexture(Width,Height,1);
        render.enableRandomWrite = true;
        render.filterMode = FilterMode.Point;
        render.Create();
        GameObject.Find("SimWindow").GetComponent<RawImage>().texture = render;
        changeColor();
    }

    void Update(){
        debug();
        if(HFields==null || EFields==null){
            setupCells();
        }
    }
    void FixedUpdate(){
        printStats();
        add/=1.5f;
        if(shouldUpdate){
            counter++;
            time = (float)counter*(float)timeStep;
            //Update H from E
            
            if(counter%2==0)updateHField();
            //Update E From H
            else updateEField();
        }
        
        ComputeBuffer Hf = new ComputeBuffer(HFields.Length,sizeof(float)*8);
        ComputeBuffer Ef = new ComputeBuffer(EFields.Length,sizeof(float)*8);
        
        Hf.SetData(HFields);
        Ef.SetData(EFields);

        SimCompute.SetBuffer(0, "H", Hf);
        SimCompute.SetBuffer(0, "E", Ef);
        SimCompute.SetTexture(0,"Result",render);
        SimCompute.SetFloat("resolution", resolution);
        SimCompute.SetFloat("EBrightness", EBrightness);
        SimCompute.SetFloat("HBrightness", HBrightness);
        SimCompute.Dispatch(0,render.width,render.height,1);

        Hf.Release();
        Ef.Release();

        Graphics.Blit(GameObject.Find("SimWindow").GetComponent<RawImage>().texture, render);
    }


    void updateHField(){
        ComputeBuffer HFieldBuffer = new ComputeBuffer(HFields.Length,sizeof(float)*8);
        ComputeBuffer EFieldBuffer = new ComputeBuffer(EFields.Length, sizeof(float)*8);

        HFieldBuffer.SetData(HFields);
        EFieldBuffer.SetData(EFields);

        CellsCompute.SetBuffer(0,"HFields",HFieldBuffer);
        CellsCompute.SetBuffer(0,"EFields", EFieldBuffer);

        CellsCompute.SetFloat("dist", dist);
        CellsCompute.SetInt("version", 0);
        CellsCompute.SetInt("resolution", Width);
        CellsCompute.SetFloat("time", timeStep);

        CellsCompute.Dispatch(0,HFields.Length,1,1);
        
        HFieldBuffer.GetData(HFields);

        HFieldBuffer.Release();
        EFieldBuffer.Release();
    }
    void updateEField(){
        ComputeBuffer HFieldBuffer = new ComputeBuffer(HFields.Length,sizeof(float)*8);
        ComputeBuffer EFieldBuffer = new ComputeBuffer(EFields.Length, sizeof(float)*8);
        HFieldBuffer.SetData(HFields);
        EFieldBuffer.SetData(EFields);
        CellsCompute.SetBuffer(0,"HFields",HFieldBuffer);
        CellsCompute.SetBuffer(0,"EFields", EFieldBuffer);
        CellsCompute.SetFloat("dist", dist);
        CellsCompute.SetInt("version", 1);
        CellsCompute.SetInt("resolution", Width);
        CellsCompute.SetFloat("time", timeStep);
        CellsCompute.Dispatch(0,EFields.Length,1,1);

        EFieldBuffer.GetData(EFields);

        HFieldBuffer.Release();
        EFieldBuffer.Release();

    }
    public void updateBrightness(){
        HBrightness = (int)GameObject.Find("HBright").GetComponent<Slider>().value;
        EBrightness = (int)GameObject.Find("EBright").GetComponent<Slider>().value;
        
    }

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
        if(Input.GetKey(KeyCode.W)){
            for(int i=0; i<HFields.Length;i++){
                if(HFields[i].Color != Color.green){ 
                    HFields[i].Color = Color.green;
                    break;
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.B)){
            for(int i =0; i<HFields.Length;i++){
                HFields[i].Color = Color.black;
            }
        }
        if(Input.GetKeyDown(KeyCode.M)){
            for(int i =0; i<HFields.Length;i++){
                HFields[0].Position = new Vector3(1,1,0);
            }
        }

        if(Input.GetKeyDown(KeyCode.H)){
            updateHField();
        }
        if(Input.GetKeyDown(KeyCode.E)){
            updateEField();
        }
        if(Input.GetKey(KeyCode.P)){
            EFields[Width/2+((int)resolution*(Height/2))].Position += new Vector3(0,0,.1f);
        }

        if(Input.GetKeyDown(KeyCode.Comma)){
            view = 0;
        }
        if(Input.GetKeyDown(KeyCode.Period)){
            view = 1;
        }

    }

    public void UI(){
        Vector2 pos;
        RectTransform r = GameObject.Find("SimWindow").GetComponent<RectTransform>();
        pos = Input.mousePosition;
        pos.x = Mathf.FloorToInt(resolution*(pos.x/r.rect.width)-1);
        pos.y = Mathf.FloorToInt(resolution*(pos.y/r.rect.height)-1);
        if(pos.x < resolution && pos.y<resolution) EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Position += new Vector3(0,0,10);
    }

    public void onHover(){
        Vector2 pos;
        RectTransform r = GameObject.Find("SimWindow").GetComponent<RectTransform>();
        pos = Input.mousePosition;
        pos.x = Mathf.FloorToInt(resolution*(pos.x/r.rect.width));
        pos.y = Mathf.FloorToInt(resolution*(pos.y/r.rect.height));
        selectedCell = pos;
            //"\nCurl: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Curl;

    }
    void printStats(){
        if(selectedCell!=null){
            Vector2 pos = selectedCell;
            GameObject.Find("InfoText").GetComponent<Text>().text = "Info:\nCoords: "+ pos +
                "\nColor: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Color+
                "\nE: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Position+
                "\nH: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Position+
                "\nε: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].coef+
                "\nμ: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].coef;
        }
    }

}
