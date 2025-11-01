using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private GameObject coinEffect;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddCoin(1);
            Instantiate(coinEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
