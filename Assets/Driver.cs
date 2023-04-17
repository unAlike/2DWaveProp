using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UI;
public class Driver : MonoBehaviour {
    // Start is called before the first frame update
    LineRenderer Hline, Eline;
    FDTD[] HFields, EFields;

    [SerializeField]
    List<Source> Sources = new List<Source>();

    [SerializeField]
    GameObject devicePrefab;

    [SerializeField]
    List<GameObject> Devices = new List<GameObject>();
    int sourceNum = -1;

    FDTD[,] test;
    public ComputeShader SimCompute, CellsCompute;
    public RenderTexture render;
    public int EBrightness, HBrightness = 50;
    bool inspected = false;
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
    GameObject InfoPanel;
    void Start() {
        InfoPanel = GameObject.Find("InfoPanel").gameObject;
        setSim(true);
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
        Sources.Clear();
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
            HFields[i].Color = Color.black;
            HFields[i].coef = ur;
            HFields[i].cond = new Vector2(0,0);
            HFields[i].integrated = new Vector3(0,0,0);

            EFields[i].Position = new Vector3(0,0,0);
            EFields[i].Color = Color.black;
            EFields[i].coef = er;
            EFields[i].cond = new Vector2(0,0);
            EFields[i].integrated = new Vector3(0,0,0);
        }
        float[,] n2 = new float[(int)Width*2,(int)Width*2];
        int nPML = 20;

        float[] pmlValues = new float[nPML*2];

        for(int i=1; i <= 2*nPML; i++){
            float id = 2*nPML - i + 1;
            float sig = (e0*0.5f/timeStep)*Mathf.Pow((float)id/2/((float)nPML),3);
            pmlValues[i-1] = sig;
        }
        foreach(float f in pmlValues){
            print(f);
        }
        for(int i=0; i<pmlValues.Length; i=i+2){
            for(int j = 0; j < Height; j++){
                HFields[(int)(i/2)+j*(int)(resolution)].cond.x = pmlValues[((int)(i/2))];
                EFields[(int)(i/2)+j*(int)(resolution)].cond.x = pmlValues[((int)(i/2)+1)];

                HFields[Width-1-(int)(i/2)+j*(int)(resolution)].cond.x = pmlValues[((int)(i/2))];
                EFields[Width-1-(int)(i/2)+j*(int)(resolution)].cond.x = pmlValues[((int)(i/2)+1)];

                HFields[(int)(j)+(i/2)*(int)(resolution)].cond.y = pmlValues[((int)(i/2))];
                EFields[(int)(j)+(i/2)*(int)(resolution)].cond.y = pmlValues[((int)(i/2)+1)];

                HFields[(Height-1-(i/2))*(int)resolution + j].cond.y = pmlValues[((int)(i/2))];
                EFields[(Height-1-(i/2))*(int)resolution + j].cond.y = pmlValues[((int)(i/2)+1)];
            }
        }

        selectedCell = new Vector2(Width/2,Height/2);

    }
    public void setupGrid() {

        GameObject grid = GameObject.Find("GridCanvas");
        RectTransform ImgRect = GameObject.Find("SimWindow").GetComponent<RectTransform>();
        render = new RenderTexture(Width,Height,1);
        render.enableRandomWrite = true;
        render.filterMode = FilterMode.Bilinear;
        render.Create();
        GameObject.Find("SimWindow").GetComponent<RawImage>().texture = render;
    }

    void Update(){
        debug();
        if(HFields==null || EFields==null){
            InfoPanel.SetActive(true);
            setupCells();
        }
        if(inspected){
            Vector2 pos = Vector2.zero;
            RectTransform r = GameObject.Find("SimWindow").GetComponent<RectTransform>();
            pos = Input.mousePosition;
            pos.x = Mathf.FloorToInt(resolution*(pos.x/r.rect.width));
            pos.y = Mathf.FloorToInt(resolution*(pos.y/r.rect.height));
            selectedCell = pos;
            InfoPanel.SetActive(true);

            Vector3 newPos = Vector3.zero;
            if(Camera.current != null){
                newPos = Camera.current.ScreenToWorldPoint(Input.mousePosition);
            }
            newPos.z=0;
            InfoPanel.transform.position = newPos + new Vector3(0.5f,0.5f,0);
        }
        else{
            InfoPanel.SetActive(false);
        }
    }
    void FixedUpdate(){
        printStats();
        add/=1.5f;
        if(shouldUpdate){
            counter++;
            time = (float)counter*(float)timeStep;
            updateSources();
            //Update H from E
            if(counter%2==0) updateHField();
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

    public void updateSources(){
        for(int i = 0; i < Sources.Count; i++){
            Sources[i].update(time);
            EFields[(int)Sources[i].pos.x + ((int)Sources[i].pos.y*Height)].Color.r = Sources[i].getSourceVal();
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

    public void reset(){
        setupGrid();
        InfoPanel.SetActive(true);
        setupCells();
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
        if(Input.GetKeyDown(KeyCode.H)){
            updateHField();
        }
        if(Input.GetKeyDown(KeyCode.E)){
            updateEField();
        }
        if(Input.GetKeyDown(KeyCode.R)){
            setupCells();
        }
        if(Input.GetKeyDown(KeyCode.A)){
            Source s = new Source(selectedCell, 10, .9f, 1*Mathf.Pow(10,7));
            Sources.Add(s);
        }
        if(Input.GetKeyDown(KeyCode.C)){
            Sources.Add(new Source(selectedCell, 10, .9f, 3*Mathf.Pow(10,7)));
        }
        if(Input.GetKeyDown(KeyCode.P)){
            for(int i=0; i < Sources.Count; i++){
                print("Started Source " + i+1);
                Sources[i].start(time);
            }
        }
        if(Input.GetKeyDown(KeyCode.B)){
            for(int i =(int)resolution/4; i<3*resolution/4;i++){
                for(int j = (int)resolution/4; j<3 * resolution/4; j++){
                    HFields[i+(int)(j*resolution)].coef = 7;
                }
            }
        }
    }
    public int getIndex(int x, int y){
        return (x+(y*Height));
    }
    public void UI(){
        Vector2 pos;
        RectTransform r = GameObject.Find("SimWindow").GetComponent<RectTransform>();
        pos = Input.mousePosition;
        pos.x = Mathf.FloorToInt(resolution*(pos.x/r.rect.width)-1);
        pos.y = Mathf.FloorToInt(resolution*(pos.y/r.rect.height)-1);
        if(pos.x < resolution && pos.y<resolution) EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Position += new Vector3(0,0,10);
    }

    public void addDevice(){
        GameObject content = GameObject.Find("Content").gameObject;
        GameObject device = Instantiate(devicePrefab);
        device.transform.SetParent(content.transform, false);
        Rect newR = content.GetComponent<RectTransform>().rect;
        newR.height = content.transform.childCount * device.GetComponent<RectTransform>().rect.height;
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x,device.GetComponent<RectTransform>().rect.height * content.transform.childCount);
        
        //device.transform.localScale = Vector3.one;
    }

    public void onStartHover(){
        inspected = true;
    }
    public void onStopHover(){
        selectedCell = new Vector2(-1,-1);
        inspected = false;
    }
    public void onClickOff(){
        selectedCell = new Vector2(-1,-1);
    }

    void printStats(){
        if(selectedCell!=null && InfoPanel.activeSelf){
            if(selectedCell.x != -1 && selectedCell.y != -1){
                Vector2 pos = selectedCell;
                string srcText = "";
                foreach(Source s in Sources){
                    srcText += "____________\nPos: (" + s.pos.x + ", " + s.pos.y + ")\nVal: " + s.getSourceVal() + "\n";       
                }
                GameObject.Find("InfoText").GetComponent<Text>().text = "Info:\nCoords: "+ pos +
                    "\nColor H: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Color+
                    "\nColor E: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Color+
                    "\nE: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Position+
                    "\nH: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].Position+
                    "\nε: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].coef+
                    "\nμ: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].coef+
                    "\nσx: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].cond.x+
                    "\nσy: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].cond.y+ "\n"+
                    "\nσx: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].cond.x+
                    "\nσy: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].cond.y+
                    "\nH inte: " + HFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].integrated+
                    "\nE inte: " + EFields[(int)(pos.x)+(int)(Mathf.FloorToInt(pos.y)*resolution)].integrated+ "\n\n"+
                    srcText;
                    
            }
            else GameObject.Find("InfoText").GetComponent<Text>().text = "Info:";
        }
    }
}
