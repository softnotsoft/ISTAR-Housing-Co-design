using UnityEngine;
using System.Collections.Generic;

public class change_shader : MonoBehaviour
{
    public List <GameObject> lista_paredes;

    public void mudar_paredes (Material shader) {
        foreach (GameObject parede in lista_paredes) {
            parede.GetComponent <MeshRenderer> () .material = shader;

        }
    }
}
