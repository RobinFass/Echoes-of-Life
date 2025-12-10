using UnityEngine;

namespace Object
{
    public class Door : MonoBehaviour
    {
        private Door door;

        public Door DestinationDoor
        {
            get => door;
            set
            {
                if (value != null) door = value;
            }
        }
    }
}
