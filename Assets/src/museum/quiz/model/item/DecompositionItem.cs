using System;
using System.Collections.Generic;
using src.museum.quiz.script.decomposition;
using UnityEngine;

namespace src.museum.quiz.model.decomposition
{
    public class DecompositionItem : MonoBehaviour
    {
        [Serializable]
        public class DecompositionData
        {
            public GameObject newMineral;
            public float newMineralSize;
        }
        
        public List<DecompositionData> MineralPart;

        private void Start()
        {
            if (DecompositionAssistant.DecomposableMinerals == null)
            {
                DecompositionAssistant.DecomposableMinerals = new List<GameObject>();
            }
            DecompositionAssistant.DecomposableMinerals.Add(gameObject);
        }
    }
}