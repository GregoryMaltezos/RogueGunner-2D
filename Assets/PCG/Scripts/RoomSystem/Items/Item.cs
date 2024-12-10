using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private BoxCollider2D itemCollider;

    [SerializeField]
    int health = 3;
    [SerializeField]
    bool nonDestructible;

    [SerializeField]
    private GameObject hitFeedback, destoyFeedback;

    public UnityEvent OnGetHit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    // Initialize item properties from an ItemData object
    public void Initialize(ItemData itemData)
    {
        // Set the item's appearance
        spriteRenderer.sprite = itemData.sprite;
        // Position the sprite in the center of its size
        spriteRenderer.transform.localPosition = new Vector2(0.5f * itemData.size.x, 0.5f * itemData.size.y);
        // Set the collider size and offset to match the sprite
        itemCollider.size = itemData.size;
        itemCollider.offset = spriteRenderer.transform.localPosition;
        // Set destructibility and health based on the itemData
        if (itemData.nonDestructible)
            nonDestructible = true;

        this.health = itemData.health;

    }
    // Handles when the item is hit by an external damage source
    public void GetHit(int damage, GameObject damageDealer)
    {
        // If the item is indestructible, do nothing
        if (nonDestructible)
            return;
        // Spawn feedback effects based on remaining health
        if (health>1)
            Instantiate(hitFeedback, spriteRenderer.transform.position, Quaternion.identity);
        else
            Instantiate(destoyFeedback, spriteRenderer.transform.position, Quaternion.identity);
        spriteRenderer.transform.DOShakePosition(0.2f, 0.3f, 75, 1, false, true).OnComplete(ReduceHealth); // Shake the item's position visually to indicate it was hit
    }
    // Reduces the item's health and handles destruction if health reaches zero
    private void ReduceHealth()
    {
        health--;
        if (health <= 0)
        {
            spriteRenderer.transform.DOComplete();
            Destroy(gameObject);
        }
            
    }
}

