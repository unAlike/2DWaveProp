using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FDTD {
    public Vector3 Position;
    public float time;
    public float coef;
    public FDTD(){
        Position = Vector3.zero;
        time = 0;
        coef = 1.0000001f;
    }
    public FDTD(float t){
        Position = Vector3.zero;
        time = t;
    }

    //Returns the Cross Product of Position
    public Vector3 Cross(Vector3 v){
        return Vector3.Cross(Position, v);
    }

}
