using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    [SerializeField] GameObject staticImage;
    public IEnumerator Quit()
    {
        yield return new WaitForSeconds(1.5f);
        staticImage.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}
