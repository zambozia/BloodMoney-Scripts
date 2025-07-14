
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCG
{
    public class TrafficLights2 : MonoBehaviour
    {
        private float countTime = 0;
        private int step = 0;

        private int status;

        public TrafficLight trafficLight_N;
        public TrafficLight trafficLight_S;
        public TrafficLight trafficLight_E;
        public TrafficLight trafficLight_W;

        public PedestrianCrossingSync pedestrianSync; // 🔧 Reference to external controller

        void Start()
        {
            countTime = 0;
            step = 0;

            status = (UnityEngine.Random.Range(1, 8) < 4) ? 13 : 31;
            EnabledObjects(status);

            // Initial state: stop pedestrians
            pedestrianSync?.SetPedestrianCrossing(true);

            InvokeRepeating(nameof(TrafficLightTurn), UnityEngine.Random.Range(0, 4), 1);
        }

        private void TrafficLightTurn()
        {
            countTime += 1;

            if (step == 0)
            {
                if (countTime > 16)
                {
                    countTime = 0;
                    step = 1;

                    status = (status == 13) ? 12 : 21;
                    EnabledObjects(status);

                    pedestrianSync?.SetPedestrianCrossing(true); // 🚫 Stop pedestrians
                }
            }
            else if (step == 1)
            {
                if (countTime >= 5)
                {
                    countTime = 0;
                    step = 2;

                    status = (status == 12) ? 41 : 14;
                    EnabledObjects(44); // Pedestrian Light

                    pedestrianSync?.SetPedestrianCrossing(false); // ✅ Let pedestrians cross
                }
            }
            else if (step == 2)
            {
                if (countTime >= 7)
                {
                    countTime = 0;
                    step = 0;

                    status = (status == 14) ? 13 : 31;
                    EnabledObjects(status);

                    pedestrianSync?.SetPedestrianCrossing(true); // 🚫 Stop again
                }
            }
        }

        void EnabledObjects(int st)
        {
            if (trafficLight_N)
                trafficLight_N.SetStatus(st.ToString().Substring(0, 1));

            if (trafficLight_S)
                trafficLight_S.SetStatus(st.ToString().Substring(0, 1));

            if (trafficLight_E)
                trafficLight_E.SetStatus(st.ToString().Substring(1, 1));

            if (trafficLight_W)
                trafficLight_W.SetStatus(st.ToString().Substring(1, 1));
        }
    }
}
