using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Animations;
using System;

public class GUI_MessageBox : MonoBehaviour
{
    public string title = " ";
    public string message;

    [SerializeField]private TextMeshProUGUI MessageTitle;
    [SerializeField]private TextMeshProUGUI MessageBox;
    public Button button;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        if (title != null | title != " " | MessageTitle != null)
        {
            MessageTitle.text = title;
        }
        addmessage();
        if (button != null)
        {
            button.onClick.AddListener(CloseAnim);
        }
    }
    public void addmessage()
    {
        try
        {
            if (message != null || message != "")
            {
                MessageBox.text = message;
            }
        }
        catch (Exception e)
        {
            if (e.Message == "Object reference not set to an instance of an object")
            {
                Debug.LogWarning(gameObject.name + " Has no Message box attach");
            }
            else Debug.LogError(e.Message);
        }
    }
    public void CloseAnim()
    {
        anim.SetBool("exit", true);
        Destroy(gameObject, 1.2f);
    }
}
