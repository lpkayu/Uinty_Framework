using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//资源信息基类
public abstract class ResourceInfoBase
{
   //引用计数
   public int refCount;
}

//资源信息类
public class ResourceInfo<T> : ResourceInfoBase
{
   public T asset;
   //用于在异步加载结束后将资源传递给外部
   public UnityAction<T> callBack;
   public Coroutine coroutine;
   //标记是否要删除
   public bool isDel;
   
   public void AddRefCount()
   {
      ++refCount;
   }

   public void SubRefCount()
   {
      --refCount;
   }
   
}


public class ResourceManager : Singleton<ResourceManager>
{
   private ResourceManager() { }

   //用于存储加载过的资源或加载中的资源
   public Dictionary<string, ResourceInfoBase> resDic = new Dictionary<string, ResourceInfoBase>();

   
   //加载逻辑：
   //先判断字典中有没有存在该资源记录
   //无 则直接进行加载 并存入字典中
   //有 则考虑资源是否加载完毕 加载完毕就直接使用 没加载完毕就等待加载完再进行使用
   
   //直接采用同步加载
   public T Load<T>(string resName)where T:Object
   {
      string path = resName + "_" + typeof(T).Name;
      ResourceInfo<T> info;
      if (!resDic.ContainsKey(path))
      {
         //直接使用同步加载
         T res = Resources.Load<T>(resName);
         info = new ResourceInfo<T>();
         info.asset = res;
         info.AddRefCount();
         resDic.Add(path,info);
         return res;
      }
      else
      {
         info = resDic[path] as ResourceInfo<T>;
         info.AddRefCount();
         //存在异步加载正在加载中
         if (info.asset == null)
         {
            //停止异步加载
            MonoManager.Instance.StopCoroutine(info.coroutine);
            T res = Resources.Load<T>(resName);
            info.asset = res;
            //将加载后的资源直接给等待异步加载结束的委托
            info.callBack?.Invoke(res);
            //回调结束 进行引用清除
            info.callBack = null;
            info.coroutine = null;
            return res;
         }
         else
         {
            //如果异步加载已经结束直接使用
            return info.asset;
         }
      }
   }
   
   /// <summary>
   /// 调用异步加载功能函数
   /// </summary>
   /// <param name="resPath">资源路径</param>
   /// <param name="callBack">资源加载完的回调函数</param>
   /// <typeparam name="T">资源类型</typeparam>
   public void LoadAsync<T>(string resPath,UnityAction<T> callBack) where T:Object
   {
      string path = resPath + "_" + typeof(T).Name;
      ResourceInfo<T> info;
      if (!resDic.ContainsKey(path))
      {
         info=new ResourceInfo<T>();
         info.AddRefCount();
         resDic.Add(path,info);
         //记录委托
         info.callBack += callBack;
         info.coroutine=MonoManager.Instance.StartCoroutine(HandleLoadAsync<T>(resPath));
      }
      else 
      {
         //若字典中已存在，直接取出资源
         info = resDic[path] as ResourceInfo<T>;
         info.AddRefCount();
         if (info.asset == null) //若资源还没加载完
            info.callBack += callBack;
         else
            callBack?.Invoke(info.asset);
      }
      
   }
   
   
  /// <summary>
  /// 实际处理异步加载功能函数
  /// </summary>
  /// <param name="resPath">资源路径/param>
  /// <param name="callBack">资源加载完的回调函数</param>
  /// <typeparam name="T">资源类型</typeparam>
   private IEnumerator HandleLoadAsync<T>(string resPath)where T:Object
   {
      ResourceRequest resourceRequest = Resources.LoadAsync<T>(resPath);
      yield return resourceRequest;
    
      string path = resPath + "_" + typeof(T).Name;
      
      if (resDic.ContainsKey(path))
      {
         ResourceInfo<T> info = resDic[path] as ResourceInfo<T>;
         info.asset = resourceRequest.asset as T;
         if (info.refCount==0)
            UnLoadAsset<T>(path,info.isDel,null,false);
         else
         {
            info.callBack?.Invoke(info.asset);
            info.callBack = null;
            info.coroutine = null;
         }
         
      }
   }
  

  /// <summary>
  /// 卸载单个资源
  /// </summary>
  /// <param name="resPath">路径</param>
  /// <param name="isDel">记录是否删除</param>
  /// <param name="callBack">回调函数</param>
  /// <param name="isSub">是否减引用计数</param>
  /// <typeparam name="T">类型</typeparam>
   public void UnLoadAsset<T>(string resPath,bool isDel=false,UnityAction<T> callBack=null,bool isSub=true)
   {
      string path = resPath + "_" + typeof(T).Name;
      if (resDic.ContainsKey(path))
      {
         ResourceInfo<T> info = resDic[path] as ResourceInfo<T>;
         if(isSub)
            info.SubRefCount();
         info.isDel = true;
         //若资源已经加载结束  真正决定是否进行删除操作
         if (info.asset != null && info.refCount==0 && info.isDel)
         {
            resDic.Remove(path);
            Resources.UnloadAsset(info.asset as Object);
         }
         //若资源正在加载中
         else if(info.asset==null)
         {
            //当异步加载不想使用时，删除回调记录
            info.callBack -= callBack;
         }
      }
   }


   /// <summary>
   /// 异步卸载对应没有使用的Resources相关的资源
   /// </summary>
   /// <param name="callBack">回调函数</param>
   public void UnloadUnusedAssets(UnityAction callBack)
   {
      MonoManager.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callBack));
   }

   private IEnumerator ReallyUnloadUnusedAssets(UnityAction callBack)
   {
      //就是在真正移除不使用的资源之前 应该把我们自己记录的那些引用计数为0 并且没有被移除记录的资源
      //移除掉
      List<string> list = new List<string>();
      foreach (string path in resDic.Keys)
      {
         if (resDic[path].refCount == 0)
            list.Add(path);
      }
      foreach (string path in list)
      {
         resDic.Remove(path);
      }

      AsyncOperation ao = Resources.UnloadUnusedAssets();
      yield return ao;
      //卸载完毕后 通知外部
      callBack();
   }


   
   //清除字典
   public void ClearDic(UnityAction callBack)
   {
      MonoManager.Instance.StartCoroutine(ReallyClearDic(callBack));
   }

   private IEnumerator ReallyClearDic(UnityAction callBack)
   {
      resDic.Clear();
      AsyncOperation ao = Resources.UnloadUnusedAssets();
      yield return ao;
      //卸载完毕后 通知外部
      callBack();
   }
   
   
}
