using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class NasaData : MonoBehaviour
{
    // <"date", <"time", <x, y, z>>>
    public Dictionary<string, Dictionary<string, Vector3>> data;

    private int dataEntries;
    private string requestText;

    private void Start() {        
        // ISS data
        // GetData("-125544", "2020-4-8%201:00", "2020-4-8%201:01","1m");

        // Moon data
        //GetData("-301", "2020-4-8%201:00", "2020-4-8%201:01","1m");

        data = new Dictionary<string, Dictionary<string, Vector3>>();
        requestText = "";
        dataEntries = 0;
    }

    public IEnumerator GetData(string ID, string START_TIME, string STOP_TIME, string STEP_SIZE) {        
        yield return MakeRequest(
            ID,
            "YES",
            "VECTORS",
            START_TIME,
            STOP_TIME,
            STEP_SIZE,
            "YES",
            "500"
        );
    }


    // Example URL:
    /* https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch=1&COMMAND='499'&MAKE_EPHEM='YES'  
  &TABLE_TYPE='OBSERVER'&START_TIME='2000-01-01'&STOP_TIME='2000-12-31'&STEP_SIZE='15%20d'  
  &QUANTITIES='1,9,20,23,24'&CSV_FORMAT='YES' */
    private IEnumerator MakeRequest(
      string COMMAND, 
      string MAKE_EPHEM,
      string TABLE_TYPE,
      string START_TIME,
      string STOP_TIME,
      string STEP_SIZE,
      string CSV_FORMAT,
      string COORDINATE_ORIGIN
    ) {
        string baseURL = "https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch=1&";

        string fullURL = (
          baseURL + 
          "COMMAND='" + COMMAND + "'" +
          "&MAKE_EPHEM='" + MAKE_EPHEM + "'" + 
          "&TABLE_TYPE='" + TABLE_TYPE + "'" +
          "&START_TIME='" + START_TIME + "'" + 
          "&STOP_TIME='" + STOP_TIME + "'" + 
          "&STEP_SIZE='" + STEP_SIZE + "'" + 
          "&CSV_FORMAT='" + CSV_FORMAT + "'" + 
          "&COORDINATE_ORIGIN='" + COORDINATE_ORIGIN + "'"
        );

        Debug.Log("URL is " + fullURL);

        yield return StartCoroutine(getInfo(fullURL));
    }

    private IEnumerator getInfo(string url) {
        yield return StartCoroutine(GetText(url));

        Debug.Log("Request is " + requestText);

        // Parse text
        if (requestText != "") {
            string between = GetBetween(requestText, "$$SOE", "$$EOE");

            string [] lines = between.Split('\n');

            for (int i = 1; i < lines.Length - 1; i++) {
                Debug.Log("Processing line " + lines [i]);
                dataEntries++;

                string [] splitLine = lines [i].Split(',');
                string [] dateSplitLine = splitLine [1].Split(' ');
                //Debug.Log("splitLine: ");
                //foreach (var item in splitLine) {
                //    Debug.Log(item);
                //}

                string date = dateSplitLine [2];
                string time = dateSplitLine [3];

                float x = float.Parse(splitLine [2]);
                float y = float.Parse(splitLine [3]);
                float z = float.Parse(splitLine [4]);

                //float x = float.Parse(splitLine [5].Substring(0, characters));
                //float y = float.Parse(splitLine [7].Substring(0, characters));
                //float z = float.Parse(splitLine [9].Substring(0, characters));

                // Debug.Log("date: " + date);
                // Debug.Log("time: " + time);
                // Debug.Log("x: " + x);
                // Debug.Log("y: " + y);
                // Debug.Log("z: " + z);

                if (!data.ContainsKey(date)) {
                   data.Add(date, new Dictionary<string, Vector3>());
                   data [date].Add(time, new Vector3(x, y, z));
                    
                } else {
                   data [date].Add(time, new Vector3(x,y,z));
                }
            }

            Debug.Log("Done extracting data from request text");
            Debug.Log(dataEntries + " entries in data");

            //foreach (var item in data ["2019-Dec-21"]) {
            //    Debug.Log(item.Key);
            //}

        } else {
            Debug.Log("Text received from http reequest is empty");
        }
    }

    private IEnumerator GetText(string url) {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
 
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
            yield break;
        }
        else {
            // Show results as text
            //Debug.Log(www.downloadHandler.text);
            requestText = www.downloadHandler.text;
        }

        yield break;
    }

    private static string GetBetween(string strSource, string strStart, string strEnd) {
        if (strSource.Contains(strStart) && strSource.Contains(strEnd)) {
            int lengthOfChunk = strSource.IndexOf(strEnd) - strSource.IndexOf(strStart) - strEnd.Length;
            return strSource.Substring(strSource.IndexOf(strStart) + strStart.Length, lengthOfChunk);
        }
        else {
            return "";
        }
    }
}
