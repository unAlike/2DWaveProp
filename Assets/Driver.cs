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
    public int EBrightness, HBrightness = 50;
    int Width, Height = 10;

    int NUM_FDTD = 100;
    Image[] imgs;
    int counter;
    int view = 0;
    public float resolution;
    float e0 = 8.85f * Mathf.Pow(10,-12);
    float u0 = Mathf.Pow(1.25663706f,-6);
    public float timeStep, C, ur, er, Mh, Me, time, TProp, dist, add, DO_NOT_EDIT, u, e;

    float E3, E2, E1, H3, H2, H1, H0 = 0;
    public bool shouldUpdate = true;

    Vector2 selectedCell = new Vector2(0,0);
    void Start() {
        GameObject.Find("HBright").GetComponent<Slider>().value = HBrightness;
        GameObject.Find("EBright").GetComponent<Slider>().value = EBrightness;
        add = 0;
        C = 299792458;
        print(C);
        time = 0;

        
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
        e = er*e0;
        u = ur*u0;
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
            HFields[i].Color = Random.ColorHSV();
            HFields[i].coef = ur;
            HFields[i].cond = new Vector2(0,0);
            HFields[i].integrated = new Vector3(0,0,0);

            EFields[i].Position = new Vector3(0,0,0);
            EFields[i].Color = Random.ColorHSV();
            EFields[i].coef = er;
            EFields[i].cond = new Vector2(0,0);
            EFields[i].integrated = new Vector3(0,0,0);
        }
        for(int i=Width-20; i< Width; i++){
            for(int j=0; j<Height;j++){
                float sig = (e0*0.5f/timeStep)*Mathf.Pow(((float)(i+1)/(float)Width/2),3);
                //Hi X and Y
                HFields[i+(j*(int)resolution)].cond.x = sig;
                EFields[i+(j*(int)resolution)].cond.x = sig;
                
                HFields[(i*(int)resolution)+j].cond.y = sig;
                EFields[(i*(int)resolution)+j].cond.y = sig;

                HFields[-(i-Width)+(j*(int)resolution)].cond.x = sig;
                EFields[-(i-Width)+(j*(int)resolution)].cond.x = sig;
                
                HFields[(-(i-Width)*(int)resolution)+j].cond.y = sig;
                EFields[(-(i-Width)*(int)resolution)+j].cond.y = sig;
            }
        }
        // for(int i=10; i>-1; i--){
        //     for(int j=0; j<Height;j++){
        //         float sig = (e0*0.5f/timeStep)*Mathf.Pow(((float)(i+1)/(float)Width),3);
        //         HFields[i+(j*(int)resolution)].cond.x = sig;
        //         EFields[i+(j*(int)resolution)].cond.x = sig;

        //         HFields[(i*(int)resolution)+j].cond.y = sig;
        //         EFields[(i*(int)resolution)+j].cond.y = sig;
        //     }
        // }

        selectedCell = new Vector2(1,1);
        HFields[0].Color = Color.red;
        EFields[0].Color = Color.red;
    }
    public void setupGrid() {

        GameObject grid = GameObject.Find("GridCanvas");
        RectTransform ImgRect = GameObject.Find("SimWindow").GetComponent<RectTransform>();
        render = new RenderTexture(Width,Height,1);
        render.enableRandomWrite = true;
        render.filterMode = FilterMode.Bilinear;
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
        
        ComputeBuffer Hf = new ComputeBuffer(HFields.Length,sizeof(float)*13);
        ComputeBuffer Ef = new ComputeBuffer(EFields.Length,sizeof(float)*13);
        
        Hf.SetData(HFields);
        Ef.SetData(EFields);

        SimCompute.SetBuffer(0, "H", Hf);
        SimCompute.SetBuffer(0, "E", Ef);
        SimCompute.SetTexture(0,"Result",render);
        SimCompute.SetFloat("resolution", resolution);
        SimCompute.SetFloat("EBrightness", EBrightness);
        SimCompute.SetFloat("HBrightness", HBrightness);
        SimCompute.SetFloat("selectedX", selectedCell.x);
        SimCompute.SetFloat("selectedY", selectedCell.y);
        SimCompute.Dispatch(0,render.width,render.height,1);

        Hf.Release();
        Ef.Release();

        Graphics.Blit(GameObject.Find("SimWindow").GetComponent<RawImage>().texture, render);
    }


    void updateHField(){
        ComputeBuffer HFieldBuffer = new ComputeBuffer(HFields.Length,sizeof(float)*13);
        ComputeBuffer EFieldBuffer = new ComputeBuffer(EFields.Length, sizeof(float)*13);
        HFields[0].Color = Color.red;
        EFields[0].Color = Color.red;
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
        ComputeBuffer HFieldBuffer = new ComputeBuffer(HFields.Length,sizeof(float)*13);
        ComputeBuffer EFieldBuffer = new ComputeBuffer(EFields.Length, sizeof(float)*13);
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
        if(Input.GetKeyDown(KeyCode.LeftArrow)){
            if(selectedCell.x>0) selectedCell.x--;
        }
        if(Input.GetKeyDown(KeyCode.RightArrow)){
            if(selectedCell.x<resolution-1)selectedCell.x++;
        }
        if(Input.GetKeyDown(KeyCode.UpArrow)){
            if(selectedCell.y < resolution-1) selectedCell.y++;
        }
        if(Input.GetKeyDown(KeyCode.DownArrow)){
            if(selectedCell.y > 0) selectedCell.y--;
        }
        if(Input.GetKey(KeyCode.Q)){
            changeColor();
        }
        if(Input.GetKeyDown(KeyCode.H)){
            updateHField();
        }
        if(Input.GetKeyDown(KeyCode.E)){
            updateEField();
        }
        if(Input.GetKey(KeyCode.P)){
            EFields[Width/2+((int)resolution*(Height/2))].Position += new Vector3(0,0,1f);
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
    public void onClickOff(){
        selectedCell = new Vector2(-1,-1);
    }
    void printStats(){
        if(selectedCell!=null){
            if(selectedCell.x != -1 && selectedCell.y != -1){
                Vector2 pos = selectedCell;
                GameObject.Find("InfoText").GetComponent<Text>().text = "Info:\nCoords: "+ pos +
                    "\nColor: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Color+
                    "\nE: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Position+
                    "\nH: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Position+
                    "\nε: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].coef+
                    "\nμ: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].coef+
                    "\nσ: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].cond+
                    "\nH inte: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].integrated+
                    "\nE inte: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].integrated;
            }
            else GameObject.Find("InfoText").GetComponent<Text>().text = "Info:";
        }
    }

}
