using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public abstract class EventInfoBase{}

public class EventInfo<T> : EventInfoBase  //可传参事件类型
{
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

public class EventInfo : EventInfoBase  //无参事件类型
{
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}


//事件中心模块
public class EventCenter : Singleton<EventCenter>
{
    private Dictionary<E_EventType, EventInfoBase> eventDic = new Dictionary<E_EventType, EventInfoBase>();

    #region 有参类型模块
     public void EventTrigger<T>(E_EventType eventName,T objInfo)
        {
            if (eventDic.ContainsKey(eventName))
            {
                (eventDic[eventName] as EventInfo<T>).actions?.Invoke(objInfo);
            }
        }

    public void AddEventListener<T>(E_EventType eventName,UnityAction<T> func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo<T>).actions += func;
        else
        {
            eventDic.Add(eventName,new EventInfo<T>(func));
        }
    }

    public void RemoveEventListener<T>(E_EventType eventName,UnityAction<T> func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo<T>).actions -= func;
    }
    
    #endregion


    #region 无参类型模块
    public void EventTrigger(E_EventType eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions?.Invoke();
        }
    }

    public void AddEventListener(E_EventType eventName,UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo).actions += func;
        else
        {
            eventDic.Add(eventName,new EventInfo(func));
        }
    }

    public void RemoveEventListener(E_EventType eventName,UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo).actions -= func;
    }
    

    #endregion
    
    
    public void Clear()
    {
        eventDic.Clear();
    }
    
    
}
