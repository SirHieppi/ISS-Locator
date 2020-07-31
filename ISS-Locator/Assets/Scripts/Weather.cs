using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using SimpleJSON;
using TMPro;

public class Weather : MonoBehaviour
{
    public string weatherDescription;
    public TextMeshProUGUI info;

    private string apiKey = "a0c0461d26beaa0d281a712abb91ab5d";

    private void Start() {
        //StartCoroutine(SetWeatherInfo("0", "0"));
    }

    private string GetUrl(string lat, string lon) {
        string baseUrl = "https://api.openweathermap.org/data/2.5/weather?";

        return (baseUrl + "lat=" + lat + "&lon=" + lon + "&appid=" +apiKey);
    }

    public IEnumerator SetWeatherInfo(string lat, string lon) {
        string url = GetUrl(lat, lon);

        Debug.Log("weather url is " + url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
            yield break;
        }

        // Show results as text
        //Debug.Log(www.downloadHandler.text);
        var json = JSON.Parse(www.downloadHandler.text);
        //requestText = www.downloadHandler.text;

        //foreach (var item in json.Keys) {
        //    Debug.Log(item);
        //}

        weatherDescription = json["weather"][0]["description"];
        //Debug.Log("Weather description is " + weatherDescription);

        info.text = "Weather: " + weatherDescription;

        yield break;
    }
}
