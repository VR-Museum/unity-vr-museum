using System;
using System.Collections.Generic;
using src.museum.quiz.script;
using UnityEngine;

namespace src.museum.quiz.model.item
{
    
    [Serializable]
    public class QuizItem : MonoBehaviour
    {
        public MineralName mineral;
        public Transform mineralCenter;
        [NonSerialized]
        public GameObject MineralObject;
        public float hardness = 5;
        public Color color = Color.red;

        private void Start()
        {
            MineralObject = gameObject;
            QuizComposer._component.AddMineral(gameObject);
        }
    }

    public enum MineralName
    {
        Aragonite,
        Calcite,
        Marcacite,
        Halite,
        Pseudomalachite,
        GrenatAlmandin,
        Vanadinite,
        Aurichalcite,
        Antimonite,
        GypseRose,
        Slate,
        Amethyst,
        Fluorite,
        Tourmaline,
        Sfaleryte,
        Kianite,
        Azurite,
        Agate,
        Apatite,
        Epidote,
        Goethite,
        Jasper,
        Obsidian,
        Chrysoprase,
        Citrine,
        Diopside,
        Magnetite,
        Talc,
        Actinolite,
        Sapphire,
        Topaz
    }

    public readonly struct IndicatorMineralsPairItem
    {
        public readonly GameObject Indicator;
        public readonly HashSet<MineralName> UniqueExpectedMinerals;

        public IndicatorMineralsPairItem(GameObject gameObject, HashSet<MineralName> mineralNames)
        {
            Indicator = gameObject;
            UniqueExpectedMinerals = mineralNames;
        }
    }
}