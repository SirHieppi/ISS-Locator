using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Phone : MonoBehaviour
{
    public float xRotation;
    public float yRotation;
    public float zRotation;

    public float phoneAltitude;
    public float phoneLatitude;
    public float phoneLongitude;
    public float phoneTrueHeadingInitial = -999;
    public float phoneTrueHeading;

    public TextMeshProUGUI trueheadingText;
    public TextMeshProUGUI initalTrueheadingText;

    private Gyroscope gyro;

    public IEnumerator StartSensors() {
        yield return new WaitForSeconds(2);

        // Gyroscope
        if (SystemInfo.supportsGyroscope) {
            gyro = Input.gyro;
            gyro.enabled = true;
            Debug.Log(gyro.attitude);
        } else {
            Debug.Log("Gyro not supported");
        }

        // Compass 
        Input.compass.enabled = true;

        // Location
        yield return StartCoroutine(StartLocationServices());

        yield return new WaitForSeconds(1);

        StartCoroutine(UpdatePhoneData());

        // if (phoneTrueHeadingInitial == -999) {
        //     phoneTrueHeadingInitial = Input.compass.trueHeading;
        // }
        phoneTrueHeadingInitial = Input.compass.trueHeading;
        initalTrueheadingText.text = "Initial True Heading: " + phoneTrueHeadingInitial;

        Debug.Log("Phone sensors started");

        yield return new WaitForSeconds(3);

        //StartCoroutine(PrintSensorInfo());
    }

    public void UpdateInitialTrueHeading() {
        phoneTrueHeadingInitial = phoneTrueHeading;
        initalTrueheadingText.text = "Initial True Heading: " + phoneTrueHeadingInitial;
    }

    private void Update() {

        if (Input.GetKeyDown(KeyCode.P)) {
            StartCoroutine(PrintSensorInfo());
        }
        //Debug.Log(gyro.attitude);
    }

    private IEnumerator StartLocationServices() {
        if (!Input.location.isEnabledByUser) {
            Debug.Log("Location services already enabled");
            yield break;
        }

        // Start service before querying location
        Input.location.Start(1, 0.1f);

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1) {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed) {
            print("Unable to determine device location");
            yield break;
        }
        else {
            // Access granted and location value could be retrieved
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }
    }

    private IEnumerator SetTrueHeadingAverage() {
        float total = 0;
        float step = 0.2f;
        
        for (float i = 0; i < 1; i+=step)
        {
            total += Input.compass.trueHeading;

            yield return new WaitForSeconds(step);
        }

        phoneTrueHeading = total / (1 / step);
    }

    private IEnumerator UpdatePhoneData() {
        while (true) {
            phoneAltitude = Input.location.lastData.altitude;
            phoneLatitude = Input.location.lastData.latitude;
            phoneLongitude = Input.location.lastData.longitude;
            // phoneTrueHeading = Input.compass.trueHeading;
            yield return StartCoroutine(SetTrueHeadingAverage());

            trueheadingText.text = "phone true heading: " + phoneTrueHeading.ToString();

            // yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator PrintSensorInfo() {
        if (Input.location.isEnabledByUser) {
            Debug.Log("Location services have started");
        }
        else {
            Debug.Log("Location services failed to start.");
        }

        while (true) {
            //Debug.Log(gyro.attitude);
            yield return new WaitForSeconds(1);

            Debug.Log("*******************************************");
            Debug.Log("Phone altitude: " + Input.location.lastData.altitude);
            Debug.Log("Phone latitude: " + Input.location.lastData.latitude);
            Debug.Log("Phone longitude: " + Input.location.lastData.longitude);
            Debug.Log("Compass heading: " + Input.compass.trueHeading);

            yield return new WaitForSeconds(1);
        }
    }
}
