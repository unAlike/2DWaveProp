using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Source
{
    public float add;
    public float tao;
    public float B;
    public float Mag;
    public float startTime= 1000000000;
    public Vector2 pos = new Vector2(0,0);
    
    public Source(Vector2 p, int mag, float a, float b){
        pos = p;
        Mag = mag;
        B = b;
        tao = .5f/b;
    }
    public void enable(){

    }
    public void disable(){

    }

    public void update(float t){
        add = (Mag * (Mathf.Sin(t*B*10000000f)));
    }

    public void start(float currentTime){
        startTime = currentTime + (7*tao);
    }
    public float getSourceVal(){
        return add;
    }

}
