using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//池对象类
public class PoolData
{
    private Stack<GameObject> notUsedStack = new Stack<GameObject>();//用于记录不在使用中的对象
    
    private List<GameObject> usedList = new List<GameObject>(); //用于记录正在使用中的对象

    public int notUsedcount => notUsedStack.Count;
    
    public int usedLsitCount => usedList.Count;

    
    public PoolData(GameObject usedObj) //在创建对象时压入list 
    {
        PushUsedList(usedObj);
    }

    public GameObject Pop()
    {
        GameObject obj;

        if (notUsedcount > 0)
        {
            obj = notUsedStack.Pop();
            usedList.Add(obj);
        }
        else //若对象都已在使用中（超上限），就使用使用时长最长的对象
        {
            obj = usedList[0];
            usedList.RemoveAt(0);
            usedList.Add(obj);
        }
        obj.SetActive(true);
        return obj;
    }

    public void Push(GameObject obj)
    {
        obj.SetActive(false);
        notUsedStack.Push(obj);
        usedList.Remove(obj);
    }
    
    
    public void PushUsedList(GameObject obj)
    {
        usedList.Add(obj);
    }

}


//对象池模块类
public class PoolMgr : Singleton<PoolMgr>
{
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();

    private PoolMgr()
    {
        
    }
    
    public GameObject GetObj(string name,int number=10)
    {
        GameObject obj;
        
        //加入数量上限
        if ((poolDic.ContainsKey(name) == false)||(poolDic[name].notUsedcount==0&&poolDic[name].usedLsitCount < number))
        {
            obj =GameObject.Instantiate(Resources.Load<GameObject>(name));
            obj.name = name;

            if (poolDic.ContainsKey(name) == false)
            {
                poolDic.Add(name,new PoolData(obj));
            }
            else
            {
                poolDic[name].PushUsedList(obj);
            }
        }
        else
        {
            obj = poolDic[name].Pop();
        }
        
        
        return obj;
    }

    public void PushObj(GameObject gameObject)
    {
        gameObject.SetActive(false);
        poolDic[gameObject.name].Push(gameObject);    
    }

    public void ClearPool()
    {
        poolDic.Clear();
    }
    
}
