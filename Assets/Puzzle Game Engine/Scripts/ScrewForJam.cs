using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
public class ScrewForJam : MonoBehaviour
{
    public Vector3 finalLocalPositionInHole;
    public Vector3 finalLocalRotationInHole;

    private Animation anim;
    private ShowcaseParent showcaseParent;
    private Color thisColor;

    private Transform reservedHole;

    [Header("Screw Animation Names")]
    public string screwInAnimName;
    public string unscrewAnimName;

    Vector3 lossyScaleAtStart;

    bool canUnscrewByClick = true;

    SoundsManagerForTemplate soundsManager;

    void Start()
    {
        soundsManager = GetComponentInParent<SoundsManagerForTemplate>();
        lossyScaleAtStart = transform.parent.lossyScale;
        anim = GetComponent<Animation>();
        showcaseParent = GetComponentInParent<ShowcaseParent>();

        GetComponent<ColorManager>().ChangeColor(GetComponentInParent<CubeWithScrews>().GetColorOfScrew(transform.parent));
        thisColor = GetComponent<ColorManager>().GetColor();
    }

    public void ColorizeBasedOnStackColor(StackColors.StackColor newColor)
    {
        GetComponent<ColorManager>().ChangeColor(newColor);
        thisColor = GetComponent<ColorManager>().GetColor();
        Debug.Log("Changed Screw Color To: " + newColor);
    }

    private void OnMouseUpAsButton()
    {
        if (!canUnscrewByClick) return;

        canUnscrewByClick = false;

        reservedHole = showcaseParent.GetComponentInChildren<ContainersManager>().ReserveNextEmptyHoleByColor(thisColor);

        if (reservedHole != null)
        {
            soundsManager.PlaySound_ScrewJam_ScrewOut();
            Unscrew();
        }
    }

    public void UnscrewFromNonColoredHole(Transform newReservedHole)
    {
        if (GetComponentInParent<ContainerHolesHolder>() != null)
            GetComponentInParent<ContainerHolesHolder>().UnReserveHole(reservedHole);

        reservedHole = newReservedHole;

        if (reservedHole != null)
            Unscrew();
    }

    void Unscrew()
    {
        anim.Play(unscrewAnimName);
    }

    void ScrewIn()
    {
        soundsManager.PlaySound_ScrewJam_ScrewIn();
        anim.Play(screwInAnimName);
    }

    public void OnUnscrewed()
    {
        CubeWithScrews parentCube = GetComponentInParent<CubeWithScrews>();

        transform.parent.SetParent(null);
        transform.parent.localScale = lossyScaleAtStart;

        transform.parent.SetParent(reservedHole);
        transform.parent.SetLocalPositionAndRotation(finalLocalPositionInHole, Quaternion.Euler(finalLocalRotationInHole));
        //transform.parent.position = reservedHole.position;

        if (parentCube != null)
            parentCube.TryToMakeCubeFall();

        ScrewIn();
    }

    public void OnScrewedIn()
    {
        GetComponentInParent<ContainerHolesHolder>().AddScrew(transform);
        showcaseParent.GetComponentInChildren<ContainersManager>().CheckIfContainerIsFull(transform);

        //If screwed into non color hole, then try to check for colored container
        if (GetComponentInParent<ContainerHolesHolder>().gameObject.name.Contains("NonColoredHolesHolder"))
        {
            Transform reservedHole2 = showcaseParent.GetComponentInChildren<ContainersManager>().ReserveNextEmptyHoleByColor(thisColor, false);

            if (reservedHole2 != null)
                UnscrewFromNonColoredHole(reservedHole2);
        }
    }
}
}