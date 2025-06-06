using System.Threading.Tasks;

using Firebase.Auth;
using Firebase;

using UnityEngine;

using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Threading;


public class FirebaseAuthManager : MonoBehaviour

{
    private SynchronizationContext unityContext;

    // Firebase Authentication instance

    public FirebaseAuth auth;


    // UI elements (replace with your actual UI elements)

    public TMP_InputField emailInputField;

    public TMP_InputField passwordInputField;

    public Button signInButton;

    //public Button signUpButton;

    public TextMeshProUGUI statusText;
    public GameObject mainMenu;
    public GameObject statusWindow;
    private PatientData patientData;



    void Start()

    {
        unityContext = SynchronizationContext.Current;
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        //statusWindow.SetActive(false);
        
        auth = FirebaseAuth.DefaultInstance;


        signInButton.onClick.AddListener(() => SignInWithEmailAndPassword());

        //signUpButton.onClick.AddListener(() => CreateUserWithEmailAndPassword());


        //This listener will show the user if the current user is logged in

        auth.StateChanged += AuthStateChanged;

        AuthStateChanged(this, null);

    }


    void AuthStateChanged(object sender, System.EventArgs e)

    {
        if (statusText == null || statusText.transform.parent == null)
        {
            return;
        }
        
        statusText.transform.parent.gameObject.SetActive(false);
        if (auth.CurrentUser != null)

        {

            statusText.text = "Usuario ingresado";
            gameObject.SetActive(false);
            mainMenu.SetActive(true);
        }

        else

        {
            //statusText.text = "User not signed in";
            gameObject.SetActive(true);
            mainMenu.SetActive(false);
        }

    }


    public void CreateUserWithEmailAndPassword()

    {

        string email = emailInputField.text;

        string password = passwordInputField.text;


        auth.CreateUserWithEmailAndPasswordAsync(email, password)

            .ContinueWith(task =>

            {

                if (task.IsCanceled)

                {

                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");

                    return;

                }

                if (task.IsFaulted)

                {
                    Debug.Log("yelo");
                    statusText.text = "Correo y/o contraseña incorrecta";
                    statusWindow.SetActive(true);
                    
                    return;

                }


                //FirebaseUser newUser = task.Result;
                FirebaseUser newUser = task.Result.User;

                Debug.LogFormat("Firebase user created successfully: {0} ({1})",

                    newUser.DisplayName, newUser.UserId);

                //statusText.text = "Sign up successful!";

            });

    }

    public void DisplayErrorText(string msg, bool enable){
        statusText.text = msg;
        statusText.gameObject.transform.parent.gameObject.SetActive(enable);
    }



    public void SignInWithEmailAndPassword()

    {

        string email = emailInputField.text;

        string password = passwordInputField.text;


        auth.SignInWithEmailAndPasswordAsync(email, password)

            .ContinueWith(task =>

            {

                if (task.IsCanceled)

                {

                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");

                    return;

                }

                if (task.IsFaulted)

                {
                    Debug.Log("set");
                    //statusText.text = "Sign in failed!";
                    unityContext.Post(_ =>
                    {
                        statusText.text = "Correo y/o contraseña incorrecta";
                        statusWindow.SetActive(true);
                    }, null);
                    
                    return;

                }

                //FirebaseUser user = task.Result;
                FirebaseUser user = task.Result.User;
                
                unityContext.Post(_ =>
                {
                    PatientInfoLoader.Instance.SetEmailAndLoadData(user.Email);
                    HistoryLoader.Instance.StopListeningToFirestore();
                    HistoryLoader.Instance.StartDatabase(email);
                }, null);

                Debug.LogFormat("User signed in successfully: {0} ({1})",

                    user.DisplayName, user.UserId);

                //statusText.text = "Sign in successful!";
                statusText.transform.parent.gameObject.SetActive(false);

            });

    }


    public void SignOut()
    {
        HistoryLoader.Instance.CleanHistorialFile();
        PatientInfoLoader.Instance.CleanPatientFile();
        emailInputField.text = "";
        passwordInputField.text = "";
        auth.SignOut();

    }

}
