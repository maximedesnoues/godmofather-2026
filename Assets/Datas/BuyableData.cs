using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "BuyableData", menuName = "ScriptableObject/BuyableData", order = 0)]
public class BuyableData : ScriptableObject
{
    [Serializable]
    public class Buyable
    {
        public string name;
        [Header("Argent qu'on récupere quand on vend")]
        public int value;
        [Header("Ne pas toucher")]
        public bool isBuyable;
    }
    public List<Buyable> data = new();
    public GameObject emptySapce;
}
