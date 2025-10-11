using UnityEngine;
using UnityEngine.UI;

public class rightPanelInfo : UIButton
{

    public GameObject prefab; // prefab criado
    public GameObject informacao; // content dentro do scrollview
    public RawImage imagem; // imagem

    public override void HandleButtonClick() {
        GameObject objecto = Instantiate(prefab);
        objecto.transform.GetChild(0).GetComponent<RawImage>().texture = imagem.texture;
        objecto.transform.SetParent(informacao.transform);
    }
}
