using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Events;
//using System.Threading.Tasks;

public class FirebaseInit : MonoBehaviour
{
    public UnityEvent OnFirebaseInitialized = new UnityEvent();

    // private void Awake()
    // {
    //     DontDestroyOnLoad(gameObject);
    // }

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => 
        {
            if (task.Exception != null){
                Debug.LogError("error de firebase");
                return;
            }
            OnFirebaseInitialized.Invoke();
        }
        );
        // DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // public void ReadData()
    // {
    //     FirebaseDatabase.DefaultInstance.GetReference("nombre").GetValueAsync().ContinueWithOnMainThread(task => {
    //         if (task.IsFaulted)
    //             {
    //                 // Handle the error...
    //             }
    //             else if (task.IsCompleted)
    //             {
    //                 DataSnapshot snapshot = task.Result;
    //                 // Do something with snapshot...
    //                 //docText.text = (string) snapshot.GetValue(true);
    //             }
    //         }
    //     );
    // }

    // public void WriteData() {
    //     FirebaseDatabase.DefaultInstance.GetReference("nombre").SetValueAsync("No pepe");
    // }
}