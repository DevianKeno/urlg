using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

using UnityEngine;

using RL.CellularAutomata;
using RL.Telemetry;

namespace RL.Classifiers
{
    public class GNBData
    {
        public struct Entry
        {
            public int RoomSeed { get; set; }
            public int FireUseCount { get; set; }
            public int BeamUseCount { get; set; }
            public int WaveUseCount { get; set; }
            public int FireHitCount { get; set; }
            public int BeamHitCount { get; set; }
            public int WaveHitCount { get; set; }
        }
    }

    public class GNBResult
    {

    }

    public class GaussianNaiveBayes
    {
        public List<int> samples = new();

        public static GNBResult ClassifyRoom(MockRoom room, StatCollection playerStats)
        {

            return new GNBResult();
        }

        public static FeatureParameters GenerateFeatureOptionsRandom(FeatureParametersSettings settings, int seed)
        {
            UnityEngine.Random.InitState(seed);
            var features = new FeatureParameters();
            features.Seed = seed;
            (int, int, int) enemyCount = Random.Random.RandomTripleIntTotaled(settings.MaxEnemyCount);            
            features.EnemyCountFire = enemyCount.Item1;
            features.EnemyCountBeam = enemyCount.Item2;
            features.EnemyCountWave = enemyCount.Item3;

            (int, int, int) obsCount = Random.Random.RandomTripleIntTotaled(settings.MaxObstacleCount);   
            features.ObstacleCountFire = obsCount.Item1;
            features.ObstacleCountBeam = obsCount.Item2;
            features.ObstacleCountWave = obsCount.Item3;

            return features;
        }

        void CalculateDependencies()
        {
            
        }

        static void DistributionEquation()
        {
            // float delta = 0f;
            // return 1 / Math.Sqrt(2 * Math.PI * Math.Pow(delta, 2));
        }

        public void ReadCSVFromFile(string filepath)
        {
            string contents = File.ReadAllText(filepath);
        }

        // Posterior CalculatePosterior(CalculatePosteriorParameters parameters)
        // {

        // }
    }
}