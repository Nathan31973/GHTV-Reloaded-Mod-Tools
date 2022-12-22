using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreditText : MonoBehaviour
{
    public TextMeshProUGUI TextComponent;
    public float ScrollSpeed = 10;

    private TextMeshProUGUI m_cloneTextObject;
    private RectTransform m_textRectTransform;
    private string sourceText;
    private string temptext;
    public float height = 0;
    public int heightX = 5;
    public float scrollPosition = 0;
    public float offset = 0;
    // Start is called before the first frame update
    void Start()
    {
        m_textRectTransform = TextComponent.GetComponent<RectTransform>();

        StartCoroutine(StartScroll());
    }

    IEnumerator StartScroll()
    {
        height = heightX * TextComponent.preferredHeight;

        while(true)
        {
            if(TextComponent.havePropertiesChanged)
            {
                height = heightX * TextComponent.preferredHeight;

            }
            if(scrollPosition > height)
            {
                scrollPosition = 0;
            }
            m_textRectTransform.position = new Vector3(offset, scrollPosition, 0);
            scrollPosition += ScrollSpeed * Time.deltaTime;
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
