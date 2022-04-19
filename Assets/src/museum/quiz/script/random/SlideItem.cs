using System;
using src.museum.quiz.model.item;
using UnityEngine;

namespace src.museum.quiz.script.random
{
    public class SlideItem : MonoBehaviour
    {
        public MineralName mineralName;
        [NonSerialized]
        public GameObject Slide;
        public int orderNumber;

        private void Start()
        {
            Slide = gameObject;
            SlideComposer.AddToDictionary(this);
            foreach (var slideRenderer in GetComponentsInChildren<Renderer>())
            {
                slideRenderer.enabled = false;
            }
        }
    }
}