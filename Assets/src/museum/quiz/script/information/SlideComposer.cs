using System.Collections.Generic;
using src.museum.quiz.model.item;
using UnityEngine;

namespace src.museum.quiz.script.random
{
    public class SlideComposer : MonoBehaviour
    {
        private static Dictionary<MineralName, List<SlideItem>> _mineralsSlides = new Dictionary<MineralName, List<SlideItem>>();

        public static void AddToDictionary(SlideItem newSlide)
        {
            if (!_mineralsSlides.ContainsKey(newSlide.mineralName))
            {
                _mineralsSlides.Add(newSlide.mineralName, new List<SlideItem>());
            }
            var slides = _mineralsSlides[newSlide.mineralName];
            var index = slides.FindIndex(informationSlide => informationSlide.orderNumber > newSlide.orderNumber);
            slides.Insert(index == -1 ? slides.Count : index, newSlide);
        }
        
        public static List<SlideItem> GetSlides(MineralName mineralName)
        {
            if (!_mineralsSlides.ContainsKey(mineralName))
            {
                _mineralsSlides.Add(mineralName, new List<SlideItem>());
            }

            return _mineralsSlides[mineralName];
        }

        private void OnDestroy()
        {
            _mineralsSlides = new Dictionary<MineralName, List<SlideItem>>();
        }
    }
}