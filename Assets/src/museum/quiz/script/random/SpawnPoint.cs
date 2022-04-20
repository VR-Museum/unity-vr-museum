using System;
using UnityEngine;

namespace src.museum.quiz.script.random
{
    public class SpawnPoint : MonoBehaviour
    {
        private void Start()
        {
            RandomStarter.SpawnPoints.Add(gameObject);
        }
    }
}