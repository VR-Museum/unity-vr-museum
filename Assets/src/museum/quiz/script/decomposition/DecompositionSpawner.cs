using System;
using System.Collections.Generic;
using System.Linq;
using src.museum.quiz.model.decomposition;
using src.museum.quiz.model.item;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace src.museum.quiz.script.decomposition
{
    public class DecompositionSpawner : MonoBehaviour
    {
        public float spawnDelay = 0.3f;
        private DateTime _previousSpawnTime;
        
        public static List<DecompositionItem.DecompositionData> MineralsToSpawn;
    
        private void Update()
        {
            if (MineralsToSpawn != null && MineralsToSpawn.Count > 0 && DateTime.Now.Subtract(_previousSpawnTime).TotalSeconds > spawnDelay)
            {
                var mineralData = MineralsToSpawn.First();
                var position = transform.position;
                var textureOffset = CalculateOffset(mineralData.newMineral);
                var newMineral = Instantiate(
                    mineralData.newMineral, 
                    new Vector3(position.x + textureOffset.x, position.y + 0.5f + textureOffset.y, position.z + textureOffset.z), 
                    mineralData.newMineral.transform.rotation);
                var localScale = newMineral.transform.localScale;
                var proportion = (float) Math.Pow(mineralData.newMineralSize, 1f / 3f);
                localScale = new Vector3(
                    localScale.x * MinAndMax(0.3f, 1f, proportion),
                    localScale.y * MinAndMax(0.3f, 1f, proportion),
                    localScale.z * MinAndMax(0.3f, 1f, proportion)
                );
                newMineral.transform.localScale = localScale;
                newMineral.GetComponent<DecompositionItem>().enabled = false;
                MineralsToSpawn.Remove(mineralData);
                _previousSpawnTime = DateTime.Now;
            }
        }
        
        private float MinAndMax(float min, float max, float value)
        {
            return Math.Min(Math.Max(min, value), max);
        }

        private void OnDestroy()
        {
            MineralsToSpawn = new List<DecompositionItem.DecompositionData>();
        }

        private Vector3 CalculateOffset(GameObject mineral)
        {
            var realCenter = mineral.GetComponent<QuizItem>().mineralCenter.position;
            var offsetCenter = mineral.transform.position;
            return offsetCenter - realCenter;
        }
    }
}