using System;using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum E_UiLayer
{
    bottom,
    middle,
    top,
    system
}



public class UIManager : Singleton<UIManager>
{
    private abstract class PanelInfoBase{}
    //面板信息类
    private class PanelInfo<T>:PanelInfoBase where T:BasePanel
    {
        public T panel;
        public UnityAction<T> callBack;
        public bool isHide;
        
        public PanelInfo(UnityAction<T> callBack)
        {
            this.callBack += callBack;
        }
    }
    
    
    private Canvas uiCanvas;
    private EventSystem uiEventSystem;
    private Camera uiCamera;

    //层级
    private Transform bottomLayer;
    private Transform middleLayer;
    private Transform topLayer;
    private Transform systemLayer;

    private Dictionary<string, PanelInfoBase> panelDic = new Dictionary<string, PanelInfoBase>();
    
    private UIManager()
    {
        uiCamera = GameObject.Instantiate(Resources.Load<Camera>("UI/UICamera")).GetComponent<Camera>();
        GameObject.DontDestroyOnLoad(uiCamera);
        
        uiCanvas = GameObject.Instantiate(Resources.Load<Canvas>("UI/Canvas")).GetComponent<Canvas>();
        uiCanvas.worldCamera = uiCamera;
        GameObject.DontDestroyOnLoad(uiCanvas);

        uiEventSystem = GameObject.Instantiate(Resources.Load<EventSystem>("UI/EventSystem")).GetComponent<EventSystem>();
        GameObject.DontDestroyOnLoad(uiEventSystem);
        
        bottomLayer = uiCanvas.transform.Find("Bottom");
        middleLayer = uiCanvas.transform.Find("Middle");
        topLayer = uiCanvas.transform.Find("Top");
        systemLayer = uiCanvas.transform.Find("System");
    }

    //设置ui层级
    public Transform SetUILayer(E_UiLayer uiLayer)
    {
        switch (uiLayer)
        {
            case E_UiLayer.bottom:
                return bottomLayer;
            case E_UiLayer.middle:
                return middleLayer;
            case E_UiLayer.top:
                return topLayer;
            case E_UiLayer.system:
                return systemLayer;
            default:
                return null;
        }
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <param name="layer">面板的层级</param>
    /// <param name="callBack">用于把加载完的面板传出外部</param>
    /// <typeparam name="T">面板类型</typeparam>
    public void ShowPanel<T>(E_UiLayer layer,bool isAsync,UnityAction<T> callBack=null)where T:BasePanel
    {
        string panelName = typeof(T).Name;
        
        //面板信息已存在的情况
        if (panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> info = panelDic[panelName] as PanelInfo<T>;
            //若资源正在加载中
            if (info.panel == null)
            {
                info.isHide = false;
                if (callBack != null)
                    info.callBack += callBack;
            }
            else
            {
                if(!info.panel.gameObject.activeSelf)
                    info.panel.gameObject.SetActive(true);
                info.panel.ShowMe();
                callBack?.Invoke(info.panel);
            }
            return;
        }
        
        //不存在面板时，先往字典中添加数据（占位置），可用于判断资源是否处于在加载状态
        panelDic.Add(panelName,new PanelInfo<T>(callBack));
        ABManager.Instance.LoadResourceAsync<GameObject>("ui",panelName,isAsync, 
            (panelObj) =>
            {
                //取出字典中提前占位置的面板信息数据
                PanelInfo<T> info=panelDic[panelName] as PanelInfo<T>;
                if (info.isHide)
                {
                    panelDic.Remove(panelName);
                    return;
                }
                //创建面板
                Transform uilayer = SetUILayer(layer);
                GameObject obj = GameObject.Instantiate(panelObj, uilayer, false);
                T panel = obj.GetComponent<T>();
                panel.ShowMe();
               
                //加载完将面板对象传出让外部能使用
                info.callBack?.Invoke(panel);
                //存储panel
                info.panel = panel;
            });
    }

    
    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <param name="isDestroy">是否要删除面板</param>
    /// <typeparam name="T">面板类型</typeparam>
    public void HidePanel<T>(bool isDestroy=false)where T:BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> info=panelDic[panelName] as PanelInfo<T>;
            //若资源正在加载中
            if (info.panel == null)
            {
                info.isHide = true;
                info.callBack = null;
            }
            else
            {
                info.panel.HideMe();
                if(!isDestroy)
                    info.panel.gameObject.SetActive(false);
                else
                {
                    GameObject.Destroy(info.panel.gameObject);
                    panelDic.Remove(panelName); 
                }
            }
        }
    }

    
    //获取面板
    public void GetPanel<T>(UnityAction<T> callBack)where T:BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> info=panelDic[panelName] as PanelInfo<T>;
            //若资源正在加载中
            if (info.panel == null)
            {
                info.callBack += callBack;
            }
            else if (!info.isHide)  //表示加载完成且没有进行隐藏
            {
                callBack?.Invoke(info.panel);
            }
        }
    }

    //为ui组件添加自定义的事件
    public static void AddUIEventListener(UIBehaviour uiObj,EventTriggerType type,UnityAction<BaseEventData> callBack)
    {
        EventTrigger eventTrigger = uiObj.GetComponent<EventTrigger>();
        //保证唯一性
        if (eventTrigger == null)
            eventTrigger = uiObj.gameObject.AddComponent<EventTrigger>();

        //添加事件
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callBack);

        eventTrigger.triggers.Add(entry);
    }
    
}
