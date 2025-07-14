using UnityEngine;
using Gley.PedestrianSystem;


namespace FCG
{
    public class PedestrianCrossingSync : MonoBehaviour
    {
        [Tooltip("Name of the StreetCrossingComponent used in Mobile Pedestrian System")]
        public string crossingName = "Component1";

        // Call this with true to stop pedestrians, false to allow crossing
        public void SetPedestrianCrossing(bool stop)
        {
            API.SetCrossingState(crossingName, stop);
        }
    }
}
