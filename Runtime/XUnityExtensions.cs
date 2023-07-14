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

	public static Vector2 xy(this Vector3 v)
	{
		return new Vector2(v.x, v.y);
    }
    public static Vector2 xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }
    public static Vector2 xx(this Vector3 v)
    {
        return new Vector2(v.x, v.x);
    }
    public static Vector2 yx(this Vector3 v)
    {
        return new Vector2(v.y, v.x);
    }
    public static Vector2 yz(this Vector3 v)
    {
        return new Vector2(v.y, v.z);
    }
    public static Vector2 yy(this Vector3 v)
    {
        return new Vector2(v.y, v.y);
    }
    public static Vector2 zx(this Vector3 v)
    {
        return new Vector2(v.z, v.x);
    }
    public static Vector2 zy(this Vector3 v)
    {
        return new Vector2(v.z, v.y);
    }
    public static Vector2 zz(this Vector3 v)
    {
        return new Vector2(v.z, v.z);
    }

    public static Vector3 xxx(this Vector3 v)
    {
        return new Vector3(v.x, v.x, v.x);
    }
    public static Vector3 xzx(this Vector3 v)
    {
        return new Vector3(v.x, v.z, v.x);
    }
    public static Vector3 xzy(this Vector3 v)
    {
        return new Vector3(v.x, v.z, v.y);
    }
    public static Vector3 xyx(this Vector3 v)
    {
        return new Vector3(v.x, v.y, v.x);
    }

    public static Vector3 yyy(this Vector3 v)
    {
        return new Vector3(v.y, v.y, v.y);
    }
    public static Vector3 yxz(this Vector3 v)
    {
        return new Vector3(v.y, v.x, v.z);
    }
    public static Vector3 yzx(this Vector3 v)
    {
        return new Vector3(v.y, v.z, v.x);
    }
    public static Vector3 yzy(this Vector3 v)
    {
        return new Vector3(v.y, v.z, v.y);
    }
    public static Vector3 yxy(this Vector3 v)
    {
        return new Vector3(v.y, v.x, v.y);
    }


    public static Vector3 zzz(this Vector3 v)
    {
        return new Vector3(v.z, v.z, v.z);
    }
    public static Vector3 zxy(this Vector3 v)
    {
        return new Vector3(v.z, v.x, v.y);
    }
    public static Vector3 zxz(this Vector3 v)
    {
        return new Vector3(v.z, v.x, v.z);
    }
    public static Vector3 zyx(this Vector3 v)
    {
        return new Vector3(v.z, v.y, v.x);
    }
    public static Vector3 zyz(this Vector3 v)
    {
        return new Vector3(v.z, v.y, v.z);
    }
}
