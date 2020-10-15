using UnityEngine;
using System.Collections;
 
public class SingletonScript<T> : MonoBehaviour where T : MonoBehaviour {

    protected static T instance;

    public static T Instance {
        get {
            if (instance == null) {
                instance = (T)FindObjectOfType(typeof(T));
                if (instance == null) {
                    GameObject container = new GameObject();
                    container.name = typeof(T)+"Container";
                    instance = (T)container.AddComponent(typeof(T));
                }
            }
            return instance;
        }
    }
}
