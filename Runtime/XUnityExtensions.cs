using UnityEngine;

public static class XUnityExtensions
{
	public static T Find<T>(this GameObject go, string gameObjectName) where T : Component
	{
		var tr = go.transform.Find(gameObjectName);
		if (tr != null)
			return tr.GetComponent<T>();
		return default(T);
	}
	public static T FindX<T>(this Transform go, string gameObjectName) where T : Component
	{
		var tr = go.Find(gameObjectName);
		if (tr != null)
			return tr.GetComponent<T>();
		return default(T);
	}
}