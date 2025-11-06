using UnityEngine;

public class ShowExtraButtons : MonoBehaviour
{
    [SerializeField] GameObject extraButton1;
    [SerializeField] GameObject extraButton2;

    bool showing = false;

    public void ToggleButtons()
    {
        showing = !showing;

        extraButton1.SetActive(showing);
        extraButton2.SetActive(showing);
    }
}
