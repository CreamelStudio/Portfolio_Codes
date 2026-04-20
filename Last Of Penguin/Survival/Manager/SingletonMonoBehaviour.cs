// Unity
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : Component
{
    private static bool isShuttingDown = false;
    private static object lockObject = new object();

    private static T instance = null;
    public static T Instance
    {
        get
        {
            if (isShuttingDown)
            {
                Debug.LogError($"[SingletonMonoBehaviour] Singleton object {typeof(T).Name} is destroying or already destroyed. Return default value of {typeof(T).Name}");
                return default;
            }

            lock (lockObject)
            {
                if (!instance)
                {
                    instance = FindObjectOfType<T>();
                }

                if (!instance)
                {
                    GameObject newObject = new GameObject();
                    instance = newObject.AddComponent<T>();
                    newObject.name = $"{typeof(T).Name}";
                }
            }

            return instance;
        }
    }
}