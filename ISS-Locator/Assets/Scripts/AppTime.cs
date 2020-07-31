using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppTime : MonoBehaviour
{
    public struct DateAndTime
    {
        public string year;
        public string month;
        public string day;
        public string hour;
        public string minute;
    }

    // Time Variables
    public DateAndTime dt;
    public DateAndTime dtLater;
    public string currentDateString;
    public string [] monthAbbreviations;


    private void Start() {
        monthAbbreviations = new string [] {
            "Jan", "Feb", "Mar", "Apr", "May",
            "Jun", "Jul", "Aug", "Sep", "Oct",
            "Nov", "Dec"
        };

        GetAndSetTime(true);
    }

    public void GetAndSetTime(bool usePhoneData) {
        if (usePhoneData) {
            dt.year = System.DateTime.UtcNow.Year.ToString();
            dt.month = System.DateTime.UtcNow.Month.ToString();
            dt.day = System.DateTime.UtcNow.Day.ToString();
            dt.hour = System.DateTime.UtcNow.Hour.ToString();
            dt.minute = System.DateTime.UtcNow.Minute.ToString();
        }
        else {
            dt.year = "2019";
            dt.month = "9";
            dt.day = "20";
            dt.hour = "4";
            dt.minute = "14";
        }

        currentDateString = dt.year + "-" + dt.month + "-" + dt.day;
    }

    public string GetDateDictKey() {
        if (int.Parse(dt.day) < 10) {
            return dt.year + "-" + monthAbbreviations [int.Parse(dt.month) - 1] + "-0" + dt.day;
        }

        return dt.year + "-" + monthAbbreviations [int.Parse(dt.month) - 1] + "-" + dt.day;
    }

    public string GetTimeDictKey(int h, int m) {

        if (m < 10) {
            if (h < 10) {
                return "0" + h + ":0" + m + ":00.0000";
            }
            else {
                return h + ":0" + m + ":00.0000";
            }
        }
        else {
            if (h < 10) {
                return "0" + h + ":" + m + ":00.0000";
            }
            else {
                return h + ":" + m + ":00.0000";
            }
        }
    }

    public string GetLaterTimeString(int y, int m, int d, int h, int min) {
        if (int.Parse(dt.minute) + min >= 60) {
            h++;
            min -= 60;
        }
        if (int.Parse(dt.hour) + h >= 24) {
            d++;
            h -= 24;
        }
        if (m > 0 && int.Parse(dt.day) + d < System.DateTime.DaysInMonth(y, m)) {
            m++;
            d = 1;
        }
        if (m > 12) {
            m = 1;
            d = 1;
        }


        dtLater.year = (int.Parse(dt.year) + y).ToString();
        dtLater.month = (int.Parse(dt.month) + m).ToString();
        dtLater.day = (int.Parse(dt.day) + d).ToString();
        dtLater.hour = (int.Parse(dt.hour) + h).ToString();
        dtLater.minute = (int.Parse(dt.minute) + min).ToString();

        return dtLater.year + "-" + dtLater.month + "-" + dtLater.day + "%20" + dtLater.hour + ":" + dtLater.minute;
    }

    public string GetDateTimeString() {
        return (
            dt.year + "-" + dt.month + "-" + dt.day + "%20" + dt.hour + ":" + dt.minute
        );
    }
}
