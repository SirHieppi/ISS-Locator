using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GoogleARCore;
using TMPro;

public class AppManager : MonoBehaviour
{
    // App Components
    public GameObject arCamera;
    public GameObject phone;
    public GameObject nasaData;
    public GameObject calculations;
    public GameObject weather;
    public GameObject appTime;
    public string currentTarget;
    // For lat, lon, alt, and time variables
    private bool usePhoneData = false;

    // Visual Components
    public GameObject target;
    public GameObject targetTrail;
    public GameObject targetTrailPosHolder; // Should be in front of camera
    public GameObject northArrow;
    public GameObject historyList;
    public List<TextMeshProUGUI> historyListTexts;
    public TextMeshProUGUI info;
    public TextMeshProUGUI azimuthText;
    public TextMeshProUGUI currentTargetText;
    public TextMeshProUGUI extraText;
    private bool trailEnabled;

    // Debug objects
    public GameObject redSphere;
    public GameObject blueSphere;

    // Calculations
    // <date, <time, value>>
    Dictionary<string, Dictionary<string, float>> azimuths;
    Dictionary<string, Dictionary<string, float>> elevations;

    // To Do
    // Known bugs
    //  converting to utc can advance a day

    void Start()
    {
        trailEnabled = false;
        azimuths = new Dictionary<string, Dictionary<string, float>>();
        elevations = new Dictionary<string, Dictionary<string, float>>();

        StartCoroutine(StartApp());
    }

    // Update is called once per frame
    void Update()
    {
        string infoText = "";
        Vector3 newEulerAngles = Camera.main.transform.eulerAngles;
        newEulerAngles.y %= 360;
        if (newEulerAngles.y < 0) {
            newEulerAngles.y += -phone.GetComponent<Phone>().phoneTrueHeading;
        } else {
            infoText = string.Format("Expected N: {0} -= {1} = {2}\n", newEulerAngles.y, phone.GetComponent<Phone>().phoneTrueHeading, (newEulerAngles.y - phone.GetComponent<Phone>().phoneTrueHeading));
            newEulerAngles.y -= phone.GetComponent<Phone>().phoneTrueHeading;
        }
        newEulerAngles.x = 0;
        northArrow.transform.eulerAngles = newEulerAngles;
        northArrow.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, (Screen.height/2) - 100, Camera.main.nearClipPlane + 4) );

        // code taken from unity compass doc
        // northArrow.transform.rotation = Quaternion.Euler(0, -Input.compass.magneticHeading, 0);
        // northArrow.transform.rotation = Quaternion.Euler(0, -Input.compass.trueHeading, 0);

        // infoText += string.Format("Cam rotation: X:{0}, Y:{1}, Z:{2}", 
        //     Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z);
        // extraText.text = infoText;

        // Rotates target trail towards target
        float singleStep = 2 * Time.deltaTime;

        // Determine which direction to rotate towards
        Vector3 targetDirection = targetTrailPosHolder.transform.position - target.transform.position;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(targetTrailPosHolder.transform.forward, targetDirection, singleStep, 0.0f);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        targetTrailPosHolder.transform.rotation = Quaternion.LookRotation(newDirection);




        // For placing target on a detected plane
        // Idea was to have user select floor as plane to anchor target to plane

        // If the player has not touched the screen, we are done with this update.
        // Touch touch;
        // if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        // {
        //     return;
        // }

        // // Should not handle input if the player is pointing on UI.
        // if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        // {
        //     return;
        // }

        // // Raycast against the location the player touched to search for planes.
        // TrackableHit hit;
        // TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
        //     TrackableHitFlags.FeaturePointWithSurfaceNormal;

        // if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        // {
        //     // Use hit pose and camera pose to check if hittest is from the
        //     // back of the plane, if it is, no need to create the anchor.
        //     if ((hit.Trackable is DetectedPlane) &&
        //         Vector3.Dot(arCamera.transform.position - hit.Pose.position,
        //             hit.Pose.rotation * Vector3.up) < 0)
        //     {
        //         Debug.Log("Hit at back of the current DetectedPlane");
        //     }
        //     else
        //     {
        //         // Choose the prefab based on the Trackable that got hit.
        //         GameObject prefab;
        //         if (hit.Trackable is FeaturePoint)
        //         {
        //             prefab = redSphere;
        //         }
        //         else if (hit.Trackable is DetectedPlane)
        //         {
        //             DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
        //             if (detectedPlane.PlaneType == DetectedPlaneType.Vertical)
        //             {
        //                 prefab = redSphere;
        //             }
        //             else
        //             {
        //                 prefab = redSphere;
        //             }
        //         }
        //         else
        //         {
        //             prefab = redSphere;
        //         }

        //         // Instantiate prefab at the hit pose.
        //         // var gameObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

        //         var gameObject = Instantiate(prefab, 
        //         new Vector3(0,0,.5f), hit.Pose.rotation);

        //         // Create an anchor to allow ARCore to track the hitpoint as understanding of
        //         // the physical world evolves.
        //         var anchor = hit.Trackable.CreateAnchor(hit.Pose);

        //         // Make game object a child of the anchor.
        //         gameObject.transform.parent = anchor.transform;

        //         blueSphere.transform.position = gameObject.transform.position;

        //         infoText += string.Format("red sphere pos: X:{0}, Y:{1}, Z:{2}\nblue sphere pos: X:{3}, Y:{4}, Z:{5}", 
        //             gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z,
        //             blueSphere.transform.position.x, blueSphere.transform.position.y, blueSphere.transform.position.z);
        //         extraText.text = infoText;
        //     }
        // }
    }

    // App functions
    private IEnumerator StartApp() {
        // Initialize time
        appTime.GetComponent<AppTime>().GetAndSetTime(usePhoneData);

        // Start phone sensors
        yield return StartCoroutine(phone.GetComponent<Phone>().StartSensors());

        if (!Input.location.isEnabledByUser && usePhoneData) {
            Debug.Log("Cannot start the app without location services");
            Debug.Log("Trying again...");
            StartCoroutine(StartApp());
        }
        else {
            // Switch to default target, ISS
            SelectTargetDropDownButton(0);

            // Get weather data
            Phone p = phone.GetComponent<Phone>();
            yield return StartCoroutine(weather.GetComponent<Weather>().SetWeatherInfo(p.phoneLatitude.ToString(), p.phoneLongitude.ToString()));

            // Set visuals
            UpdateTarget();
            ToggleTargetTrail();
        }
    }

    // UI functions

    public void ChangeDistanceMultiplier(int value) {
        int newDistMult = 1;
        
        switch (value)
        {
            case 0:
                newDistMult = 200;
                break;
            case 1:
                newDistMult = 100;
                break;
            case 2:
                newDistMult = 50;
                break;
            case 3:
                newDistMult = 25;
                break;
            case 4:
                newDistMult = 1;
                break;
        }

        calculations.GetComponent<Calculations>().ChangeDistMult(newDistMult);
    }

    public void SelectTargetDropDownButton(int value) {
        switch (value) {
            case 0:
                StartCoroutine(SelectTarget("ISS"));
                currentTarget = "ISS";
                currentTargetText.text = "Target: " + currentTarget;
                break;
            case 1:
                StartCoroutine(SelectTarget("Sun"));
                currentTarget = "Sun";
                currentTargetText.text = "Target: " + currentTarget;
                break;
            case 2:
                StartCoroutine(SelectTarget("Moon"));
                currentTarget = "Moon";
                currentTargetText.text = "Target: " + currentTarget;
                break;
            default:
                break;
        }
    }

    public void RefreshTarget() {
        StartCoroutine(SelectTarget(currentTarget));
    }

    public void ToggleHistory() {
        if (historyList.activeSelf) {
            historyList.SetActive(false);
        } else {
            historyList.SetActive(true);
        }
    }

    public void AddHistory(float targX, float targY, float targZ, float trueHeading, float turnDeg) {
        Debug.Log("adding to history");
        foreach (var item in historyListTexts)
        {
            if (item.text.Equals("New Text")) {
                item.text = string.Format("Target pos: X:{0}, Y:{1}, Z:{2}\nTrue Heading: {3}\nTurn {4} degrees\n", 
                                            targX, targY, targZ, trueHeading, turnDeg) + "\n" + currentTargetText.text;
                break;
            }
        }
    }

    // Selects target and calculates data
    private IEnumerator SelectTarget(string target) {
        if (!trailEnabled) {
            ToggleTargetTrail();
        }

        azimuths.Clear();
        elevations.Clear();
        nasaData.GetComponent<NasaData>().data.Clear();

        // Debug.Log("Azimuths count " + azimuths.Count);
        // Debug.Log("elevations count " + elevations.Count);

        appTime.GetComponent<AppTime>().GetAndSetTime(usePhoneData);

        string startTime = appTime.GetComponent<AppTime>().GetDateTimeString();
        string stopTime = appTime.GetComponent<AppTime>().GetLaterTimeString(0, 0, 0, 0, 5);
        string interval = "1m";

        Debug.Log("Start time is " + startTime);
        Debug.Log("Stop time is " + stopTime);

        // Get new data for new target
        switch (target) {
            case "Moon":
                yield return StartCoroutine(
                    nasaData.GetComponent<NasaData>().GetData("301", startTime, stopTime, interval));
                break;  
            case "Sun":
                yield return StartCoroutine(
                    nasaData.GetComponent<NasaData>().GetData("10", startTime, stopTime, interval));
                break;
            case "ISS":
                yield return StartCoroutine(
                    nasaData.GetComponent<NasaData>().GetData("-125544", startTime, stopTime, interval));
                break;
            default:
                break;
        }

        // Make calculations
        yield return StartCoroutine(Calculate());

        // Update Visuals
        UpdateTarget();

        //ToggleTargetTrail();
    }

    public void UpdateTarget() {
        Debug.Log("Locating Target...");

        AppTime t = appTime.GetComponent<AppTime>();

        if (int.Parse(t.dtLater.minute) < int.Parse(t.dt.minute)) {
            Debug.Log("Need to generate more data before updating target.");
        } else {
            appTime.GetComponent<AppTime>().GetAndSetTime(usePhoneData);

            // Debug.Log("key is " + appTime.GetComponent<AppTime>().GetTimeDictKey(int.Parse(t.dt.hour), int.Parse(t.dt.minute)));
            // Debug.Log("key is " + t.GetDateDictKey());
            // PrintAzimuthsAndElevations();

            if (azimuths.ContainsKey(t.GetDateDictKey()) && azimuths [t.GetDateDictKey()].ContainsKey(t.GetTimeDictKey(int.Parse(t.dt.hour), int.Parse(t.dt.minute)))) {
                ChangeTargetLocation(
                    azimuths [t.GetDateDictKey()] [t.GetTimeDictKey(int.Parse(t.dt.hour), int.Parse(t.dt.minute))],
                    200,
                    elevations [t.GetDateDictKey()] [t.GetTimeDictKey(int.Parse(t.dt.hour), int.Parse(t.dt.minute))]);

                azimuthText.text = "Azimuth: " + azimuths [t.GetDateDictKey()] [t.GetTimeDictKey(int.Parse(t.dt.hour), int.Parse(t.dt.minute))].ToString();
            } else {
                azimuthText.text = "Azimuth: Key error";                
            }
        }
    }

    private void ChangeTargetLocation(float azimuth, float distance, float elevation) {
        float toRotate = Mathf.Abs(azimuth - phone.GetComponent<Phone>().phoneTrueHeading);
        float heightDiff = elevation - phone.GetComponent<Phone>().phoneAltitude;

        Debug.Log("Phone angle from north = " + phone.GetComponent<Phone>().phoneTrueHeading);
        Debug.Log("Phone elevation = " + phone.GetComponent<Phone>().phoneAltitude);

        Debug.Log("Target angle from north = " + azimuth);
        Debug.Log("Target elevation = " + elevation);

        Debug.Log("Turn " + toRotate + " degrees to find target.");

        info.text = "Turn " + toRotate + " degrees to find target.";

        //Set target location and rotation to cameras location and rotation
        target.transform.position = arCamera.transform.position;
        target.transform.rotation = arCamera.transform.rotation;
        Vector3 rotation = new Vector3(0, toRotate, 0);
        
        // Rotate target torwards target IRL
        target.transform.Rotate(rotation, Space.Self);

        Vector3 newPosition = transform.position;
        newPosition.z = distance;

        target.transform.position = newPosition;

        // Move target to normalized distance between user and target IRL
        target.transform.Translate(transform.forward * distance, Space.Self);

        target.transform.position = calculations.GetComponent<Calculations>().GetTargetCoordinate(phone.GetComponent<Phone>().phoneTrueHeadingInitial);
        extraText.text = string.Format("Target pos: X:{0}, Y:{1}, Z:{2}", 
            target.transform.position.x, target.transform.position.y, target.transform.position.z);

        AddHistory(target.transform.position.x, target.transform.position.y, target.transform.position.z, phone.GetComponent<Phone>().phoneTrueHeading, toRotate);
    }

    private IEnumerator Calculate() {
        AppTime t = appTime.GetComponent<AppTime>();

        float lat;
        float lon;
        float alt;

        if (usePhoneData) {
            lat = phone.GetComponent<Phone>().phoneLatitude;
            lon = phone.GetComponent<Phone>().phoneLongitude;
            alt = phone.GetComponent<Phone>().phoneAltitude / 1000; // convert meters to kilometers
        } else {
            lat = 37.3352f;
            lon = -121.8811f;
            alt = 0;
        }

        foreach (string date in nasaData.GetComponent<NasaData>().data.Keys) {
            foreach (string time in nasaData.GetComponent<NasaData>().data [date].Keys) {
                //Debug.Log("Calculating for time " + time);

                calculations.GetComponent<Calculations>().SetData(
                    int.Parse(t.dt.year), int.Parse(t.dt.month), int.Parse(t.dt.day),
                    int.Parse(t.dt.hour), int.Parse(t.dt.minute), lat, lon, alt,
                    nasaData.GetComponent<NasaData>().data [date] [time]);

                var tup = calculations.GetComponent<Calculations>().Calculate();

                // Save values
                if (!azimuths.ContainsKey(date)) {
                    azimuths.Add(date, new Dictionary<string, float>());
                    azimuths [date].Add(time, tup.Item1);
                    elevations.Add(date, new Dictionary<string, float>());
                    elevations [date].Add(time, tup.Item2);
                }
                else {
                    azimuths [date].Add(time, tup.Item1);
                    elevations [date].Add(time, tup.Item2);
                }
            }
        }

        //PrintAzimuthsAndElevations();

        // extraText.text = ("Lat: " + lat + ", Lon:" + lon + ", alt: " + alt);

        yield return null;
    }

    private void PrintAzimuthsAndElevations() {
        Debug.Log("Azimuths: ");

        foreach (string date in azimuths.Keys) {
            Debug.Log("Key: " + date);
            foreach (string time in azimuths[date].Keys) {
                Debug.Log(date + " " + time + ": " + azimuths[date][time]);
            }
        }

        Debug.Log("Elevations: ");

        foreach (string date in elevations.Keys) {
            Debug.Log("Key: " + date);
            foreach (string time in elevations [date].Keys) {
                Debug.Log(date + " " + time + ": " + elevations [date] [time]);
            }
        }
    }

    // Visuals
    private void MoveTarget(Vector3 newPosition) {
        if (!trailEnabled) {
            ToggleTargetTrail();
        }

        target.transform.position = newPosition;

        ToggleTargetTrail();
    }

    private void ToggleTargetTrail() {
        if (!trailEnabled) {
            StartCoroutine(StartTrail());
            trailEnabled = true;
        }
        else {
            trailEnabled = false;
        }
    }

    private IEnumerator StartTrail() {
        float delay = 0.10f;
        while(true) {
            yield return new WaitForSeconds(delay);
            targetTrail.SetActive(false);
            targetTrail.GetComponent<TrailRenderer>().Clear();
            targetTrail.transform.position = targetTrailPosHolder.transform.position;
            targetTrail.GetComponent<TrailRenderer>().Clear();
            targetTrail.SetActive(true);
            yield return new WaitForSeconds(delay);

            yield return MoveTo(targetTrail, target.transform.position);

            if (!trailEnabled) {
                break;
            }
        }
    }

    private IEnumerator MoveTo(GameObject targetToMove, Vector3 targetPos) {
        float diff = 0.1f;
        float speed = 100;

        while (
            Mathf.Abs(targetToMove.transform.position.x - targetPos.x) > diff ||
            Mathf.Abs(targetToMove.transform.position.x - targetPos.x) > diff ||
            Mathf.Abs(targetToMove.transform.position.x - targetPos.x) > diff
        ) {
            yield return null;
            targetTrail.transform.position = Vector3.MoveTowards(targetTrail.transform.position, targetPos, speed * Time.deltaTime);

            if (!trailEnabled) {
                break;
            }
        }
    }
}
