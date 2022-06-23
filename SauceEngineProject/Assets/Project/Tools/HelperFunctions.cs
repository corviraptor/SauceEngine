using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
{
    public static Vector3 SwapYZ(this Vector3 vector){
        vector = new Vector3(vector.x, vector.z, vector.y);
        return vector;
    }
    
    public static Vector3 KillY(this Vector3 vector){
        vector = new Vector3(vector.x, 0, vector.z);
        return vector;
    }
    
    public static Vector3 KillZ(this Vector3 vector){
        vector = new Vector3(vector.x, vector.y, 0);
        return vector;
    }

    public static Vector3 transformVector(this Vector3 subject, Vector3 basis){
        subject = new Vector3(subject.x * basis.x, subject.y * basis.y, subject.z * basis.z);
        return subject;
    }
}
