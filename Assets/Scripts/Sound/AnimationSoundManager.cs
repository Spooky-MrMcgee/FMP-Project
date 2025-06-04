using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSoundManager : MonoBehaviour
{
    [SerializeField] AudioSource mFootL;
    [SerializeField] AudioSource mFootR;
    [SerializeField] AudioSource stagger;

    [SerializeField] AudioSource mcFootL;
    [SerializeField] AudioSource mcFootR;

    [SerializeField] GameObject gun;
    [SerializeField] GameObject wineMother;
    [SerializeField] GameObject wineMC;
    private void FootstepLeftMonstro()
    {
        mFootL.Play();
    }

    private void FootstepRightMonstro()
    {
        mFootR.Play();
    }

    private void EnemyStaggered()
    {
        stagger.Play();
    }

    private void FootstepLeftMC()
    {
        mcFootL.Play();
    }

    private void FootstepRightMC()
    {
        mcFootR.Play();
    }

    private void GunOFF()
    {
        gun.SetActive(false);
    }

    private void GunON()
    {
        gun.SetActive(true);
    }

    private void WineOFF()
    {
        wineMC.SetActive(false);
    }

    private void WineON()
    {
        wineMother.SetActive(true);
    }
}
