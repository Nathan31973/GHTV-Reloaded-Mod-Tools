using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Scrollingtext : MonoBehaviour
{
    public TextMeshProUGUI TextMeshProComponent;
    public float ScrollSpeed = 10;

    private TextMeshProUGUI m_cloneTextObject;

    private RectTransform m_transform;
    private string ScrolText;
    private string Template;
    // Start is called before the first frame update

    public void UpdateText()
    {
        if (m_cloneTextObject != null)
        {
            m_cloneTextObject.text = TextMeshProComponent.text;
        }
    }
    IEnumerator Start()
   {

        yield return new WaitForSeconds(0.1f);
        m_transform = TextMeshProComponent.GetComponent<RectTransform>();
        m_cloneTextObject = Instantiate(TextMeshProComponent);
        RectTransform clonerect = m_cloneTextObject.GetComponent<RectTransform>();
        clonerect.SetParent(m_transform, false);
        clonerect.anchorMin = new Vector2(1, 0.5f);
        clonerect.localScale = new Vector3(1, 1, 1);


        float width = TextMeshProComponent.preferredWidth;
        Vector3 startPosition = m_transform.position;

        float scrollPos = 0;
        while(true)
        {
            if(TextMeshProComponent.havePropertiesChanged)
            {
                m_cloneTextObject.text = TextMeshProComponent.text;
            }
            m_transform.position = new Vector3(-scrollPos % width, startPosition.y, startPosition.z);
            scrollPos += ScrollSpeed * 20 * Time.deltaTime;
            yield return null;
        }
   }

}
