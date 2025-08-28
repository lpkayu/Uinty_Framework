using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    //字典用于存储所有要使用到的UI控件
    public Dictionary<string, UIBehaviour> uiDic = new Dictionary<string, UIBehaviour>();
    
    //如果控件名字存在于这个容器 意味着不会通过代码去使用它 只是起到显示作用
    private static List<string> notUsedUiList = new List<string>()
    {
        "Image", 
        "Text (TMP)",
        "RawImage",
        "Background",
        "Checkmark",
        "Label",
        "Text (Legacy)",
        "Arrow",
        "Placeholder",
        "Fill",
        "Handle",
        "Viewport",
        "Scrollbar Horizontal",
        "Scrollbar Vertical"
    };

    protected virtual void Awake()
    {
        //为了避免某一个对象上存在两种控件的情况,应该优先查找重要的组件
        GetChildrenUI<Button>();
        GetChildrenUI<Toggle>();
        GetChildrenUI<Slider>();
        GetChildrenUI<InputField>();
        GetChildrenUI<ScrollRect>();
        GetChildrenUI<Dropdown>();
        //可以通过重要组件得到身上其他挂载的内容
        GetChildrenUI<Text>();
        GetChildrenUI<TextMeshPro>();
        GetChildrenUI<Image>();
    }

    //面板显示时要执行的方法
    public abstract void ShowMe();
    //面板隐藏时要执行的方法
    public abstract void HideMe();
    
    //处理监听行为
    protected virtual void ClickBtn(string btnName)
    {
        
    }

    protected virtual void SliderValueChange(string sliderName,float value)
    {
        
    }

    protected virtual void ToggleValueChange(string toggleName, bool value)
    {
        
    }

    //获取指定类型的ui组件
    public T GetUI<T>(string uiName)where T:UIBehaviour
    {
        if (uiDic.ContainsKey(uiName))
        {
            T ui = uiDic[uiName] as T;
            if (ui == null)
                Debug.Log("不存在该类型的ui组件");
            return ui;
        }
        else
        {
            Debug.Log("不存在对应名字的ui组件");
            return null;
        }
    }
    
    
    //获取子物体ui控件方法
    public void GetChildrenUI<T>() where T : UIBehaviour
    {
        T[] childrenUI = GetComponentsInChildren<T>(true);
        for (int i = 0; i < childrenUI.Length; i++)
        {
            string uiName = childrenUI[i].name;
            if (!uiDic.ContainsKey(uiName))
            {
                if (!notUsedUiList.Contains(uiName))
                {
                    uiDic.Add(uiName,childrenUI[i]);
                    
                    //根据类型添加监听事件
                    //后续根据需要添加需监听的组件
                    if (childrenUI[i] is Button)
                    {
                        (childrenUI[i] as Button).onClick.AddListener(() =>
                        {
                            ClickBtn(uiName);
                        });
                    }
                    else if (childrenUI[i] is Slider)
                    {
                        (childrenUI[i] as Slider).onValueChanged.AddListener((value) =>
                        {
                            SliderValueChange(uiName,value);
                        });
                    }
                    else if (childrenUI[i] is Toggle)
                    {
                        (childrenUI[i] as Toggle).onValueChanged.AddListener((value) =>
                        {
                            ToggleValueChange(uiName,value);
                        });
                    }
                }
            }
        }
        
    }
    
    
    
    
}
