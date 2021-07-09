using System;
using System.Collections;
using UnityEngine;

namespace Mechanics
{
    public class Coin : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(4f);
            Destroy(gameObject);
        }
    }
}