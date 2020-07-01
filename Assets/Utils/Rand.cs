using UnityEngine;

namespace Utils
{
    public class Rand
    {
        public static T Choice<T>(T[] items, float[] percentageProbabilities)
        {
            float randomValue = Random.Range(0, 100);
            int index;
            for (index = 0; index < percentageProbabilities.Length; index++)
            {
                randomValue -= percentageProbabilities[index];
                if (randomValue < 0)
                {
                    break;
                }
            }
            return items[index];
        }
    }
}