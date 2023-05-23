using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class TabButton : MonoBehaviour, IPointerEnterHandler,IPointerClickHandler,IPointerExitHandler
{
    public TabGroup tabGroup;
    public Image background;
    public TextMeshProUGUI text;
    public UnityEvent onTabSelected;
    public UnityEvent onTabDeselected;
    public bool EnableSelectColour;
    public Image SelectSlider;

    public void OnPointerClick(PointerEventData eventData)
    {
        //add a cooldown
        tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        tabGroup.Subscribe(this);
    }

    public void Select()
    {
        if(onTabSelected != null)
        {
            onTabSelected.Invoke();
        }
    }
    public void Deselect()
    {
        if(onTabDeselected != null)
        {

            onTabDeselected.Invoke();
        }
    }
}
