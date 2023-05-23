using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TabGroup : MonoBehaviour
{
    [Header("Tabs buttons")]
    public List<TabButton> tabButtons;
    [Header("Tab contents")]
    public List<GameObject> ObjectsToSwap;

    [Header("Colours")]
    public Color tabIdle;
    public Color tabActive;
    public Color tabHover;

    public Color textActive;
    public Color textIdle;

    public PanelGroup panelGroup;

    [Header("Settings")]
    private bool coolDown;
    public float collDownTimer = 0.5f;

    [HideInInspector]public TabButton selectedTab;
    //add a cooldown

    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }
        tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if (selectedTab == null || button != selectedTab)
        {
            button.background.color = tabHover;
        }
    }
    
    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton button)
    {
        if(selectedTab == button)
        {
            return;
        }
        if (!coolDown)
        {
            StartCoroutine(cooldown());
            if (selectedTab != null)
            {
                selectedTab.Deselect();
            }
            if (panelGroup != null)
            {
                panelGroup.SetPageIndex(button.transform.GetSiblingIndex());
            }
            selectedTab = button;

            selectedTab.Select();

            ResetTabs();
            if (button.EnableSelectColour)
            {
                button.background.color = tabActive;
            }
            button.text.color = textActive;
            int index = button.transform.GetSiblingIndex();
            StartCoroutine(waitforobjectDeactive(index));
        }

    }
    IEnumerator waitforobjectDeactive(int index)
    {
        for (int i = 0; i < ObjectsToSwap.Count; i++)
        {
            if (i != index)
            {
                if (ObjectsToSwap[i].GetComponent<PannelControl>() != null)
                {
                    if (ObjectsToSwap[i].activeSelf)
                    {
                        ObjectsToSwap[i].GetComponent<PannelControl>().TurnOff();
                        yield return new WaitUntil(() => ObjectsToSwap[i].activeSelf == false);
                    }
                }
                else
                {
                    ObjectsToSwap[i].SetActive(false);
                }
            }
        }
        ObjectsToSwap[index].SetActive(true);

    }
    public void ResetTabs()
    {
        //reset tabs to idle
        foreach(TabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab)
            {
                continue;
            }
            button.background.color = tabIdle;
            button.text.color = textIdle;
        }
    }

    public void Start()
    {
        selectedTab = tabButtons[0];
    }

    IEnumerator cooldown()
    {
        coolDown = true;
        yield return new WaitForSeconds(collDownTimer);
        coolDown = false;

    }
}
