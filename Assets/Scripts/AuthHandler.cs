using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Linq;

public class AuthHandler : MonoBehaviour
{
    private const string Url = "https://sid-restapi.onrender.com";

    public string Token { get; private set; }
    public string Username { get; private set; }

    public Usuario[] Usuarios { get; private set; }

    [SerializeField] private GameObject usernameField;
    [SerializeField] private GameObject passwordField;

    [SerializeField] private GameObject panelAuth;
    [SerializeField] private GameObject panelGame;
    [SerializeField] private GameObject panelUpdate;
    [SerializeField] private GameObject panelBoard;

    [SerializeField] private GameObject scoreField;
    [SerializeField] private TextMeshProUGUI[] leaderBoardText;

    private bool isUpdatePanelActive;
    private bool isBoardPanelActive;

    private void Start()
    {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(Token))
        {
            ShowPanel(panelAuth);
        }
        else
        {
            ShowPanel(panelGame);
            StartCoroutine(GetProfile());
        }
    }

    public void ToggleUpdatePanel()
    {
        isUpdatePanelActive = !isUpdatePanelActive;
        ShowPanel(isUpdatePanelActive ? panelUpdate : panelGame);
    }

    public void ToggleBoardPanel()
    {
        isBoardPanelActive = !isBoardPanelActive;
        ShowPanel(isBoardPanelActive ? panelBoard : panelGame);

        if (isBoardPanelActive)
        {
            StartCoroutine(GetUsers());
        }
    }

    public void SendScoreUpdate()
    {
        if (int.TryParse(scoreField.GetComponent<TMP_InputField>().text, out int score))
        {
            var user = new Usuario { username = Username, data = new UserDataApi { score = score } };
            StartCoroutine(UpdateScore(JsonUtility.ToJson(user)));
        }
    }

    public void Register()
    {
        var data = new AuthData
        {
            username = usernameField.GetComponent<TMP_InputField>().text,
            password = passwordField.GetComponent<TMP_InputField>().text
        };
        StartCoroutine(RegisterUser(JsonUtility.ToJson(data)));
    }

    public void Login()
    {
        var data = new AuthData
        {
            username = usernameField.GetComponent<TMP_InputField>().text,
            password = passwordField.GetComponent<TMP_InputField>().text
        };
        StartCoroutine(LoginUser(JsonUtility.ToJson(data)));
    }

    private IEnumerator RegisterUser(string json)
    {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(Url + "/api/usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            StartCoroutine(LoginUser(json));
        }
        else
        {
            Debug.LogError($"Error: {request.responseCode} - {request.error}");
        }
    }

    private IEnumerator LoginUser(string json)
    {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(Url + "/api/auth/login", json);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            var data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
            Token = data.token;
            Username = data.usuario.username;

            PlayerPrefs.SetString("token", Token);
            PlayerPrefs.SetString("username", Username);

            ShowPanel(panelGame);
        }
        else
        {
            Debug.LogError($"Error: {request.responseCode} - {request.error}");
        }
    }

    private IEnumerator UpdateScore(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(Url + "/api/usuarios", json);
        request.method = "PATCH";
        request.SetRequestHeader("x-token", Token);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.responseCode} - {request.error}");
        }
    }

    private IEnumerator GetProfile()
    {
        UnityWebRequest request = UnityWebRequest.Get(Url + "/api/usuarios/" + Username);
        request.SetRequestHeader("x-token", Token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            var data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
            Debug.Log($"User: {data.usuario.username}, Score: {data.usuario.data.score}");
        }
        else
        {
            Debug.LogError($"Error: {request.responseCode} - {request.error}");
        }
    }

    private IEnumerator GetUsers()
    {
        UnityWebRequest request = UnityWebRequest.Get(Url + "/api/usuarios");
        request.SetRequestHeader("x-token", Token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            var data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
            Usuarios = data.usuarios.OrderByDescending(u => u.data.score).Take(6).ToArray();

            for (int i = 0; i < Usuarios.Length; i++)
            {
                leaderBoardText[i].text = $"{i + 1}. {Usuarios[i].username}: {Usuarios[i].data.score}";
            }
        }
        else
        {
            Debug.LogError($"Error: {request.responseCode} - {request.error}");
        }
    }

    private void ShowPanel(GameObject panel)
    {
        panelAuth.SetActive(false);
        panelGame.SetActive(false);
        panelUpdate.SetActive(false);
        panelBoard.SetActive(false);

        panel.SetActive(true);
    }
}

[System.Serializable]
public class AuthData
{
    public string username;
    public string password;
    public Usuario usuario;
    public Usuario[] usuarios;
    public string token;
}

[System.Serializable]
public class Usuario
{
    public string _id;
    public string username;
    public UserDataApi data;
}

[System.Serializable]
public class UserDataApi
{
    public int score;
}