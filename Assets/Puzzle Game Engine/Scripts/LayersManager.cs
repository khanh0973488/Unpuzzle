using HyperPuzzleEngine;
using TMPro;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class LayersManager : MonoBehaviour
    {
        public TextMeshPro currentLayerText;

        public bool loadAllLayersOnStart = false;

        [Space]
        public GameObject previousLayerButton;
        public GameObject nextLayerButton;

        int currentLayer = 0;

        private void Start()
        {
            currentLayer = 0;

            //IF THIS IS GAME MODE, ENABLE ALL LAYERS AND DISABLE PLANE LAYER
            if (gameObject.GetComponentInParent<ShowcaseParent>().IsInGameMode() && GetComponentInParent<LevelCreator>() == null)
            {
                for (int i = 1; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(true);

                    transform.GetChild(i).Find("PlaneLayer").gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = 1; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(false);

                UpdateCurrentLayerText();

                previousLayerButton.SetActive(false);
                nextLayerButton.SetActive(true);

                if (loadAllLayersOnStart)
                    LoadAllLayers();
            }
        }

        public void LoadNextLayer()
        {
            currentLayer++;

            //If layer is out of bounds
            if (transform.childCount <= currentLayer)
                currentLayer--;
            else
            {
                for (int i = 1; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(i == (currentLayer));
            }

            UpdateCurrentLayerText();

            nextLayerButton.SetActive(transform.childCount > (currentLayer + 1));
            previousLayerButton.SetActive(true);
        }

        public void LoadAllLayers()
        {
            Debug.Log("Trying To Load All Layers_ " + gameObject.name);

            currentLayer = transform.childCount - 1;

            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(true);

            UpdateCurrentLayerText();

            previousLayerButton.SetActive(true);
            nextLayerButton.SetActive(false);
        }

        public void LoadPreviousLayer()
        {
            currentLayer--;

            //If layer is out of bounds
            if (currentLayer < 0)
                currentLayer++;
            else
            {
                for (int i = 1; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(i == (currentLayer));
            }

            UpdateCurrentLayerText();

            nextLayerButton.SetActive(transform.childCount > currentLayer);
            previousLayerButton.SetActive(currentLayer > 0);
        }

        public Transform GetCurrentLayerTransform()
        {
            return transform.GetChild(currentLayer);
        }

        private void UpdateCurrentLayerText()
        {
            currentLayerText.text = "Layer: " + (currentLayer + 1).ToString();
        }
    }
}