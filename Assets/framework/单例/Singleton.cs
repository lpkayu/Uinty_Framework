using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

//没继承mono的单例基类
public abstract class Singleton<T> where T:class
{
    private static T instance;
    
    protected static readonly Object lockObj = new Object();  
    
    //类在继承该单例模式后一定要写无参构造函数，不然会报错
    public static T Instance
    {
        get
        {   //使用lock用于解决多线程并发问题
            lock (lockObj)  
            {
                if (instance == null)
                {
                    Type type = typeof(T);
                    //获取无参构造函数
                    ConstructorInfo info = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null);
                    if (info != null)
                    {
                        instance = info?.Invoke(null) as T;
                    }
                    else
                    {
                        Debug.Log("无法得到无参构造函数");
                    }
                }
            }
            return instance; 
        }
    }
}
