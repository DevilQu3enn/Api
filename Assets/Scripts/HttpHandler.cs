using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HttpHandler : MonoBehaviour
{
    [SerializeField] private RawImage[] images;
    [SerializeField] private TextMeshProUGUI[] cardNames;
    [SerializeField] private TextMeshProUGUI[] speciesNames;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private GameObject botonComenzar;
    [SerializeField] private GameObject botonSiguiente;
    [SerializeField] private GameObject botonAnterior;

    private const string FakeApiUrl = "https://my-json-server.typicode.com/DevilQu3enn/Api/users";
    private const string RickAndMortyApiUrl = "https://rickandmortyapi.com/api";

    private int currentId = 0;
    private int imagePosition;

    public void SendRequest()
    {
        StartCoroutine(GetUserData(currentId));
    }

    private IEnumerator GetUserData(int userId)
    {
        UnityWebRequest request = UnityWebRequest.Get($"{FakeApiUrl}/{userId}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        if (request.responseCode == 200)
        {
            var user = JsonUtility.FromJson<UserData>(request.downloadHandler.text);
            usernameText.text = $"Usuario: {user.name}";
            imagePosition = 0;

            foreach (int cardId in user.deck)
            {
                StartCoroutine(GetCharacter(cardId));
            }
        }
        else
        {
            Debug.LogError($"{request.responseCode} | {request.error}");
        }
    }

    private IEnumerator GetCharacter(int characterId)
    {
        UnityWebRequest request = UnityWebRequest.Get($"{RickAndMortyApiUrl}/character/{characterId}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        if (request.responseCode == 200)
        {
            var character = JsonUtility.FromJson<CharacterData>(request.downloadHandler.text);
            cardNames[imagePosition].text = character.name;
            speciesNames[imagePosition].text = character.species;
            StartCoroutine(DownloadImage(character.image, imagePosition));
            imagePosition++;
        }
        else
        {
            Debug.LogError($"{request.responseCode} | {request.error}");
        }
    }

    private IEnumerator DownloadImage(string url, int position)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            images[position].texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    public void Comenzar()
    {
        ToggleButtons(false, true, true);
    }

    public void Siguiente()
    {
        currentId = currentId >= 3 ? 0 : currentId + 1;
        SendRequest();
    }

    public void Anterior()
    {
        currentId = currentId <= 0 ? 3 : currentId - 1;
        SendRequest();
    }

    private void ToggleButtons(bool comenzar, bool siguiente, bool anterior)
    {
        botonComenzar.SetActive(comenzar);
        botonSiguiente.SetActive(siguiente);
        botonAnterior.SetActive(anterior);
    }
}

[System.Serializable]
public class JsonData
{
    public CharacterData[] results;
    public UserData[] users;
}

[System.Serializable]
public class CharacterData
{
    public int id;
    public string name;
    public string species;
    public string image;
}

[System.Serializable]
public class UserData
{
    public int id;
    public string name;
    public int[] deck;
}