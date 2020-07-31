using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calculations : MonoBehaviour
{
    public string eastOrWest;
    public string northOrSouth;
    public string aboveOrBelowHorizon;

    public float distMult = 1;

    private int year;
    private int month;
    private int day;
    private int hour;
    private int minute;
    private float latitude;
    private float longitude;
    private float altitude;
    private float Re = 6378.137f;
    private float Rp = 6356.752f;
    private float f;
    private Vector3 iss;


    // For calculations
    private Vector3 phone;
    private float enux;
    private float enuy;
    private float enuz;

    private float azimuth;
    private float elevation;

    public void SetData(int y, int m, int d, int h, int min, float lat, float lon, float alt, Vector3 issVector) {
        year = y;
        month = m;
        day = d;
        hour = h;
        minute = min;
        latitude = lat;
        longitude = lon;
        altitude = alt;
        iss = issVector;
    }

    public (float,float) Calculate() {
        //Debug.Log("ISS Vector: " + iss.x + "," + iss.y + "," + iss.z);

        //Debug.Log("Calculating for hour " + hour + ", min " + minute);

        // Initialize f
        f = (Re - Rp) / Re;

        Rho();

        Azimuth();
        Elevation();

        return (azimuth, elevation);
    }

    // Use the initial phone's true heading!!!
    public Vector3 GetTargetCoordinate(float phoneTrueheadingInitial) {
        float mag = Mathf.Sqrt(Mathf.Pow(enux, 2) + Mathf.Pow(enuy, 2) + Mathf.Pow(enuz, 2));
        float xu = enux / mag;
        float yu = enuy / mag;
        float zu = enuz / mag;

        Debug.Log(string.Format("XU:{0}, YU:{1}, ZU:{2}", xu, yu, zu));
        // Debug.Log(string.Format("ENUX:{0}, ENUY:{1}, ENUZ:{2}", enux, enuy, enuz));

        float xTemp = distMult * xu;
        float yTemp = distMult * yu;
        float zTemp = distMult * zu;

        Debug.Log(string.Format("XTemp:{0}, YTemp:{1}, ZTemp:{2}", xTemp, yTemp, zTemp));

        float phoneRad = DegToRad(phoneTrueheadingInitial);

        float finalX = xTemp * Mathf.Cos(phoneRad) - yTemp * Mathf.Sin(phoneRad);
        float finalZ = -zTemp;
        float finalY = xTemp * Mathf.Sin(phoneRad) + yTemp * Mathf.Cos(phoneRad);


        return new Vector3(finalX, finalY, finalZ);
    }

    public void ChangeDistMult(int val) {
        distMult = val;
    }

    // Equations
    private double JEquation() {
        double j = (
            (367 * year) -
            Mathf.Floor((7 * (year + Mathf.Floor((month + 9) / 12))) / 4) +
            Mathf.Floor(275 * month / 9) + day + 1721013.5f
        );

        //Debug.Log("J: " + j);
        //Debug.Log((367 * year));
        //Debug.Log(Mathf.RoundToInt(month + 9 / 12));
        //Debug.Log(Mathf.RoundToInt(275 * month / 9));
        //Debug.Log(day);
        //Debug.Log("End of J");


        return j;
    }

    private double TEquation() {
        double t = (JEquation() - 2451545) / 36525;

        //Debug.Log("T: " + t);

        return t;
    }

    private float SigmaGNotEquation() {
        double to = TEquation();

        double g = 100.4606184f + 36000.77004f * to + 0.000387933f * to * to - 2.58310f * Mathf.Pow(10, -8) * to * to * to;

        float gFinal = (float)(g - (360 * Mathf.Floor((float)g / 360)));

        //Debug.Log("g: " + g);
        //Debug.Log("gFinal: " + gFinal);

        return gFinal;
    }

    private float SigmaGEquation() {
        float sigG = SigmaGNotEquation() + 360.98564724f * ((hour + ((float)minute / 60)) / 24);

        //Debug.Log("sigG: " + sigG);

        return sigG;
    }

    // Returns degrees
    private float SigmaEquation() {
        float sig = SigmaGEquation() + longitude;

        sig = (float)(sig - (360 * Mathf.Floor((float)sig / 360)));
        //Debug.Log("sig: " + sig);

        return sig;
    }

    private float DegToRad(float deg) {
        return deg * Mathf.PI / 180;
    }

    private float RadToDeg(float rad) {
        return rad * 180 / Mathf.PI;
    }

    private float LambdaDEquation() {
        float lambdaD = Mathf.Atan(Mathf.Tan(DegToRad(latitude)) / Mathf.Pow((1 - f), 2));

        //Debug.Log("lambdaD: " + lambdaD);
        //Debug.Log("Top: " + Mathf.Tan(DegToRad(latitude)));
        //Debug.Log("Bottom: " + Mathf.Pow((1 - f), 2));

        return lambdaD;
    }

    private void ECEquation() {
        float ecx = (Re / Mathf.Sqrt(1 - (2 * f - Mathf.Pow(f, 2)) * Mathf.Pow(Mathf.Sin(LambdaDEquation()), 2)) + altitude)
            * Mathf.Cos(LambdaDEquation()) * (Mathf.Cos(DegToRad(SigmaEquation())));

        float ecy = (Re / Mathf.Sqrt(1 - (2 * f - Mathf.Pow(f, 2)) * Mathf.Pow(Mathf.Sin(LambdaDEquation()), 2)) + altitude)
            * Mathf.Cos(LambdaDEquation()) * (Mathf.Sin(DegToRad(SigmaEquation())));

        float ecz = (Re * Mathf.Pow((1 - f), 2) / Mathf.Sqrt(1 - (2 * f - Mathf.Pow(f, 2)) * Mathf.Pow(Mathf.Sin(LambdaDEquation()), 2)) + altitude)
            * Mathf.Sin(LambdaDEquation());

        phone = new Vector3(ecx, ecy, ecz);
        //Debug.Log("cos1: " + Mathf.Cos(LambdaDEquation()));
        //Debug.Log("cos2: " + (Mathf.Cos(DegToRad(SigmaEquation()))));
    }

    private Matrix4x4 SRQEquation() {
        Matrix4x4 matrix = new Matrix4x4();

        ECEquation();

        Vector3 srq = iss - phone;

        // Debug.Log(string.Format("ISS vector: x{0},y{1},z{2}", iss.x, iss.y, iss.z));
        // Debug.Log(string.Format("Phone vector: x{0},y{1},z{2}", phone.x, phone.y, phone.z));

        matrix.m00 = srq.x;
        matrix.m10 = srq.y;
        matrix.m20 = srq.z;

        // Debug.Log(string.Format("SRQX:{0}, SRQY:{1}, SRQZ:{2}", srq.x, srq.y, srq.z));

        //Debug.Log("M00: " + matrix.m00);
        //Debug.Log("M10: " + matrix.m10);
        //Debug.Log("M20: " + matrix.m20);

        return matrix;
    }

    private Matrix4x4 R1Matrix() {
        Matrix4x4 matrix = new Matrix4x4();

        float sig = DegToRad(SigmaEquation());

        matrix.m00 = Mathf.Cos(sig);
        matrix.m01 = Mathf.Sin(sig);
        matrix.m02 = 0;

        matrix.m10 = -Mathf.Sin(sig);
        matrix.m11 = Mathf.Cos(sig);
        matrix.m12 = 0;

        matrix.m20 = 0;
        matrix.m21 = 0;
        matrix.m22 = 1;

        return matrix;
    }

    private Matrix4x4 R2Matrix() {
        Matrix4x4 matrix = new Matrix4x4();

        float lambdaD = LambdaDEquation();

        matrix.m00 = Mathf.Cos(lambdaD);
        matrix.m01 = 0;
        matrix.m02 = Mathf.Sin(lambdaD);

        matrix.m10 = 0;
        matrix.m11 = 1;
        matrix.m12 = 0;

        matrix.m20 = -Mathf.Sin(lambdaD);
        matrix.m21 = 0;
        matrix.m22 = Mathf.Cos(lambdaD);

        return matrix;
    }

    private Matrix4x4 R3Matrix() {
        Matrix4x4 matrix = new Matrix4x4();

        float lambdaD = LambdaDEquation();

        matrix.m00 = 0;
        matrix.m01 = 1;
        matrix.m02 = 0;

        matrix.m10 = 0;
        matrix.m11 = 0;
        matrix.m12 = 1;

        matrix.m20 = 1;
        matrix.m21 = 0;
        matrix.m22 = 0;

        return matrix;
    }

    private Matrix4x4 RMatrix() {
        return R3Matrix() * R2Matrix() * R1Matrix();
    }

    private void Rho() {
        Matrix4x4 m = RMatrix() * SRQEquation();
        enux = m.m00;
        enuy = m.m10;
        enuz = m.m20;

        //if (enux < 0) {
        //    Debug.Log("West");
        //}
        //else {
        //    Debug.Log("East");
        //}

        //if (enuy < 0) {
        //    Debug.Log("South");
        //}
        //else {
        //    Debug.Log("North");
        //}

        //if (enuz < 0) {
        //    Debug.Log("Below the horizon");
        //}
        //else {
        //    Debug.Log("Above the horizon");
        //}

        // Debug.Log(string.Format("In Rho function = ENUX:{0}, ENUY:{1}, ENUZ:{2}", enux, enuy, enuz));
    }
    private void Azimuth() {
        if (enuy < 0) {
            azimuth = RadToDeg(Mathf.Atan(enux / enuy)) + 180;
        }
        else {
            azimuth = RadToDeg(Mathf.Atan(enux / enuy));
        }

        if (azimuth < 0) {
            azimuth += 360;
        }

        //Debug.Log("Azimuth: " + azimuth);
    }

    private void Elevation() {
        elevation = RadToDeg(Mathf.Atan(Mathf.Sin(DegToRad(azimuth)) * (enuz / enux)));

        // Debug.Log("elevation: " + elevation);
    }
}
