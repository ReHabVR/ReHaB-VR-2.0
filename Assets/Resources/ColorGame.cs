using System.Collections;
using System.Threading;
using UnityEngine;

public class ColorGame : MonoBehaviour 
{
        public GameObject firstButton;
        public GameObject secondButton;
        public GameObject thirdButton;
        public Material blueLight;
        public Material blueDark;
        public Material greenLight;
        public Material greenDark;
        public Material yellowLight;
        public Material yellowDark;
        private readonly System.Random _random = new System.Random();
        private GameObject _currentActiveButton;
       
        void Start()
        {
            _currentActiveButton = firstButton;
            firstButton.GetComponent<MeshRenderer>().material = blueLight;
        }
        public void ChooseButton(Material clickedButtonMaterial)
        {
            if (clickedButtonMaterial.color == _currentActiveButton.GetComponent<MeshRenderer>().material.color) 
            {
                MakeAllDark();
                var nextButton = _random.Next(1, 4);
                
                switch (nextButton)
                {
                    case 1:
                        _currentActiveButton = firstButton;
                        firstButton.GetComponent<MeshRenderer> ().material = blueLight;
                        break;
            
                    case 2:
                        _currentActiveButton = secondButton;
                        secondButton.GetComponent<MeshRenderer> ().material = greenLight;
                        break;
            
                    case 3:
                        _currentActiveButton = thirdButton;
                        thirdButton.GetComponent<MeshRenderer> ().material = yellowLight;
                        break;
                }
            }

        }

        private void MakeAllDark()
        {
            firstButton.GetComponent<MeshRenderer>().material = blueDark;
            secondButton.GetComponent<MeshRenderer>().material = greenDark;
            thirdButton.GetComponent<MeshRenderer>().material = yellowDark;
        }
}