using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//公共Mono模块
//公共Mono模块的主要作用
//让不继承MonoBehaviour的脚本也能
//1.利用帧更新或定时更新处理逻辑
//2.利用协同程序处理逻辑
//3.可以统一执行管理帧更新或定时更新相关逻辑(不管你是否继承MonoBehaviour)
public class MonoManager : SingletonAuto<MonoManager>  
{
    private event UnityAction updateEvent;
    private event UnityAction fixUpdateEvent;
    private event UnityAction lateUpdateEvent;

    public void AddUpdateEvent(UnityAction updateFun)
    {
        updateEvent += updateFun;
    }
    
    public void AddFixUpateEvent(UnityAction fixUpdateFun)
    {
        fixUpdateEvent += fixUpdateFun;
    }
    
    public void AddLateUpateEvent(UnityAction lateUpdateFun)
    {
        lateUpdateEvent += lateUpdateFun;
    }
    
    public void RemoveUpateEvent(UnityAction updateFun)
    {
        updateEvent -= updateFun;
    }
    
    public void RemoveFixUpateEvent(UnityAction fixUpdateFun)
    {
        fixUpdateEvent -= fixUpdateFun;
    }
    
    public void RemoveLateUpateEvent(UnityAction lateUpdateFun)
    {
        lateUpdateEvent -= lateUpdateFun;
    }
    
    
    private void Update()
    {
        updateEvent?.Invoke();
    }

    private void FixedUpdate()
    {
        fixUpdateEvent?.Invoke();
    }

    private void LateUpdate()
    {
        lateUpdateEvent?.Invoke();
    }
    
}
