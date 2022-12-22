using UnityEngine;

public class CommuityPatchesTab : MonoBehaviour
{
    private GUI_MessageBox host;
    public GameObject fadeToBlack;

    // Start is called before the first frame update
    private void Start()
    {
        host = gameObject.GetComponent<GUI_MessageBox>();
    }

    public void GOTOHOPOPATCH()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Hopo Patch";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }

    public void Back()
    {
        host.CloseAnim();
    }
}