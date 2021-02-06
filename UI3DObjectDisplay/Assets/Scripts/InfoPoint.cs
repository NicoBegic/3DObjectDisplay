using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPoint : MonoBehaviour
{
    [TextArea(15, 20)]
    public string InfoText;
    public Material TickedMat;

    private Renderer rend;
    private Material defaultMat;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        defaultMat = rend.material;
    }

    public void SetInfoText(string text)
    {
        InfoText = text;
    }

    public void ChangeMat(bool ticked)
    {
        rend.material = (ticked) ? TickedMat : defaultMat;
    }
}
