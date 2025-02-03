using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject Entity, CardTable;
    public static int SetActive = 0;

    void ActiveAnim()
    {
        SetActive++;
        Instantiate(Entity);
        CardTable.SetActive(true);
        Animator anim = CardTable.GetComponent<Animator>();
        anim.Play("intro");
    }

    private void Update()
    {
        if (SetActive == 1)
        {
            ActiveAnim();
            SetActive++;
        }
    }
}
