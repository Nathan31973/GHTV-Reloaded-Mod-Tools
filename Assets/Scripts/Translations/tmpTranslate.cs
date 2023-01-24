using UnityEngine;
using TMPro;

public class tmpTranslate : MonoBehaviour
{
    public bool autoupdate = false;
    private string lasttext;
    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.GetComponent<TextMeshProUGUI>() != null)
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = Translater.instance.getText(gameObject.GetComponent<TextMeshProUGUI>().text);
        }
        else
        {
            Debug.LogError("[tmpTranslate]" + gameObject.name + "is missing TextMeshPro component");
            Destroy(this);
        }
       
    }
    private void Update()
    {
        if (autoupdate)
        {
            if (gameObject.GetComponent<TextMeshProUGUI>().text != lasttext)
            {
                gameObject.GetComponent<TextMeshProUGUI>().text = Translater.instance.getText(gameObject.GetComponent<TextMeshProUGUI>().text);
                lasttext = gameObject.GetComponent<TextMeshProUGUI>().text;
            }
        }
    }

}
