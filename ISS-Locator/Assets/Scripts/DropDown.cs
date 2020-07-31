using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DropDown : MonoBehaviour
{
    Dropdown m_Dropdown;

    void Awake() {
        ////Fetch the Dropdown GameObject
        //m_Dropdown = transform.GetComponent<Dropdown>();
        ////Add listener for when the value of the Dropdown changes, to take action
        //m_Dropdown.onValueChanged.AddListener(delegate {
        //    DropdownValueChanged(m_Dropdown);
        //});
    }

    //Ouput the new value of the Dropdown into Text
    public void DropdownValueChanged(int change) {
        Debug.Log("New Value : " + change);
    }
}
