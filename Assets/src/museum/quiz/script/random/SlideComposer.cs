using System.Collections.Generic;
using src.museum.quiz.model.item;
using UnityEngine;

namespace src.museum.quiz.script.random
{
    public class SlideComposer : MonoBehaviour
    {
        public static Dictionary<MineralName, List<SlideItem>> MineralsSlides = new Dictionary<MineralName, List<SlideItem>>();

        public static void AddToDictionary(SlideItem newSlide)
        {
            if (!MineralsSlides.ContainsKey(newSlide.mineralName))
            {
                MineralsSlides.Add(newSlide.mineralName, new List<SlideItem>());
            }
            var slides = MineralsSlides[newSlide.mineralName];
            var index = slides.FindIndex(informationSlide => informationSlide.orderNumber > newSlide.orderNumber);
            slides.Insert(index == -1 ? slides.Count : index, newSlide);
        }
        
        public static List<SlideItem> GetSlides(MineralName mineralName)
        {
            if (!MineralsSlides.ContainsKey(mineralName))
            {
                MineralsSlides.Add(mineralName, new List<SlideItem>());
            }

            return MineralsSlides[mineralName];
        }

        private void OnDestroy()
        {
            MineralsSlides = new Dictionary<MineralName, List<SlideItem>>();
        }
    }
}