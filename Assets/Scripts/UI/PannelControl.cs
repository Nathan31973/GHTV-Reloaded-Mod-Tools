using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PannelControl : MonoBehaviour
{
    public float EnterTime = 0.6f;
    public float ExitTime = 0.3f;
    public List<GameObject> objects;
    public float done;

    private void Start()
    {
        if (objects.Count == 0)
        {
            objects = new List<GameObject>();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                if (gameObject.transform.GetChild(i).GetComponent<PannelControl>() == null)
                {
                    gameObject.transform.GetChild(i).rotation = Quaternion.Euler(90f, 0f, 0f);
                }
                objects.Add(gameObject.transform.GetChild(i).gameObject);
            }
        }
        done = 0;
        TurnOn();
    }

    private void OnEnable()
    {
        done = 0;
        TurnOn();
    }

    public void TurnOn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        StartCoroutine(enable());
    }

    private IEnumerator enable()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].GetComponent<PannelControl>() == null && objects[i].activeSelf)
            {
                LeanTween.cancel(objects[i]);
                LeanTween.rotateX(objects[i], 0f, EnterTime)
                    .setEaseInOutSine();
                yield return new WaitForSeconds(0.3f);
            }
        }
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].GetComponent<PannelControl>() != null && !objects[i].activeSelf)
            {
                objects[i].GetComponent<PannelControl>().TurnOn();
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void TurnOff()
    {
        StartCoroutine(dissableObject());
    }

    private IEnumerator dissableObject()
    {
        int temp3 = objects.Count;
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[temp3 - 1].GetComponent<PannelControl>() != null && objects[temp3 - 1].activeSelf)
            {
                objects[temp3 - 1].GetComponent<PannelControl>().TurnOff();
                yield return new WaitUntil(() => objects[temp3 - 1].activeSelf == false);
            }
            temp3--;
        }

        int temp = objects.Count;
        for (int i = 0; i < objects.Count; i++)
        {
            var obj = objects[temp - 1];
            if (objects[temp - 1].GetComponent<PannelControl>() == null && objects[temp - 1].activeSelf)
            {
                LeanTween.cancel(objects[temp - 1]);
                LeanTween.rotateX(objects[temp - 1], 90f, ExitTime)
                .setEaseInOutSine().setOnComplete(() =>
                {
                    done++;
                });
            }
            else
            {
                done++;
            }
            yield return new WaitForSeconds(0.09f);
            temp--;
        }
        while (done < objects.Count)
        {
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
        done = 0;
    }

    private void Update()
    {
        gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}