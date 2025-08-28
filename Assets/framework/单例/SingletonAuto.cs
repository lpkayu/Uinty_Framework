using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//继承mono的自动挂载的单例基类
public class SingletonAuto<T> : MonoBehaviour where T:MonoBehaviour 
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Type type = typeof(T);
                GameObject obj = new GameObject(type.ToString());
                instance=obj.AddComponent<T>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }
    
}
